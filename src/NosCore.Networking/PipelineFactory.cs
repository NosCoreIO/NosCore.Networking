//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NosCore.Networking.Encoding;
using NosCore.Networking.Encoding.Filter;
using NosCore.Networking.Filters;
using NosCore.Networking.SessionRef;
using NosCore.Shared.Enumerations;
using SuperSocket.ProtoBase;
using SuperSocket.Server.Abstractions.Session;

namespace NosCore.Networking
{
    /// <summary>
    /// Factory class responsible for creating and configuring network channel pipelines.
    /// </summary>
    public class PipelineFactory : IPipelineFilterFactory<NosPackageInfo>
    {
        private readonly IDecoder _decoder;
        private readonly ISessionRefHolder _sessionRefHolder;
        private readonly IEnumerable<IRequestFilter> _requestFilters;
        private readonly IPipelineConfiguration _pipelineConfiguration;
        private readonly Func<INetworkClient> _clientFactory;
        private readonly Action<NosPackageInfo, INetworkClient> _packetHandler;
        private readonly Action<INetworkClient>? _disconnectHandler;

        private readonly ConcurrentDictionary<object, PipelineFilter> _filtersByConnection = new();
        private readonly ConcurrentDictionary<string, INetworkClient> _clientsBySession = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineFactory"/> class.
        /// </summary>
        /// <param name="decoder">The decoder for incoming packets.</param>
        /// <param name="sessionRefHolder">The session reference holder.</param>
        /// <param name="requestFilters">The collection of request filters to apply.</param>
        /// <param name="pipelineConfiguration">The pipeline configuration settings.</param>
        /// <param name="clientFactory">Factory function to create network client instances.</param>
        /// <param name="packetHandler">Handler for processing received packets.</param>
        /// <param name="disconnectHandler">Handler for processing client disconnections.</param>
        public PipelineFactory(IDecoder decoder, ISessionRefHolder sessionRefHolder,
            IEnumerable<IRequestFilter> requestFilters, IPipelineConfiguration pipelineConfiguration,
            Func<INetworkClient> clientFactory, Action<NosPackageInfo, INetworkClient> packetHandler,
            Action<INetworkClient>? disconnectHandler = null)
        {
            _decoder = decoder;
            _sessionRefHolder = sessionRefHolder;
            _requestFilters = requestFilters;
            _pipelineConfiguration = pipelineConfiguration;
            _clientFactory = clientFactory;
            _packetHandler = packetHandler;
            _disconnectHandler = disconnectHandler;
        }

        /// <summary>
        /// Creates a pipeline filter for a new connection.
        /// </summary>
        /// <param name="connection">The connection object.</param>
        /// <returns>A configured pipeline filter.</returns>
        public IPipelineFilter<NosPackageInfo> Create(object connection)
        {
            var filter = new PipelineFilter(_decoder, _sessionRefHolder, _requestFilters, _pipelineConfiguration.UseDelimiter);
            var typedContext = filter.Context as IAppSession;
            _sessionRefHolder[typedContext!.SessionID] = new RegionTypeMapping(0, _pipelineConfiguration.Language);
            _filtersByConnection[connection] = filter;
            return filter;
        }

        /// <summary>
        /// Creates a pipeline filter.
        /// </summary>
        /// <returns>A configured pipeline filter.</returns>
        IPipelineFilter<NosPackageInfo> IPipelineFilterFactory<NosPackageInfo>.Create()
        {
            return new PipelineFilter(_decoder, _sessionRefHolder, _requestFilters, _pipelineConfiguration.UseDelimiter);
        }

        /// <summary>
        /// Handles a session connection event.
        /// </summary>
        /// <param name="session">The connected session.</param>
        public void OnSessionConnected(IAppSession session)
        {
            var client = _clientFactory();
            client.RegisterChannel(new NetworkChannel(session));
            _clientsBySession[session.SessionID] = client;
        }

        /// <summary>
        /// Handles a session disconnection event.
        /// </summary>
        /// <param name="session">The disconnected session.</param>
        public void OnSessionClosed(IAppSession session)
        {
            _filtersByConnection.TryRemove(session.Connection, out _);
            if (_clientsBySession.TryRemove(session.SessionID, out var client))
            {
                _disconnectHandler?.Invoke(client);
            }
        }

        /// <summary>
        /// Handles an incoming packet.
        /// </summary>
        /// <param name="session">The session that received the packet.</param>
        /// <param name="package">The received package.</param>
        public void HandlePackage(IAppSession session, NosPackageInfo package)
        {
            if (_clientsBySession.TryGetValue(session.SessionID, out var client))
            {
                _packetHandler(package, client);
            }
        }
    }
}