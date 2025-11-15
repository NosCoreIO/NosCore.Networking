//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Collections.Generic;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Options;
using NosCore.Networking.Encoding;
using NosCore.Networking.Encoding.Filter;
using NosCore.Networking.SessionRef;
using NosCore.Packets.Interfaces;
using NosCore.Shared.Configuration;

namespace NosCore.Networking
{
    /// <summary>
    /// Factory class responsible for creating and configuring network channel pipelines.
    /// </summary>
    public class PipelineFactory : IPipelineFactory
    {
        private readonly ISocketChannel _channel;
        private readonly INetworkClient _clientSession;
        private readonly IOptions<ServerConfiguration> _configuration;
        private readonly IDecoder _decoder;
        private readonly IEncoder _encoder;
        private readonly ISessionRefHolder _sessionRefHolder;
        private readonly IEnumerable<RequestFilter> _requestFilters;
        private readonly IPipelineConfiguration _pipelineConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineFactory"/> class.
        /// </summary>
        /// <param name="channel">The socket channel to configure.</param>
        /// <param name="decoder">The decoder for incoming packets.</param>
        /// <param name="encoder">The encoder for outgoing packets.</param>
        /// <param name="clientSession">The network client session handler.</param>
        /// <param name="configuration">The server configuration options.</param>
        /// <param name="sessionRefHolder">The session reference holder.</param>
        /// <param name="requestFilters">The collection of request filters to apply.</param>
        /// <param name="pipelineConfiguration">The pipeline configuration settings.</param>
        public PipelineFactory(ISocketChannel channel, IDecoder decoder,
           IEncoder encoder, INetworkClient clientSession,
            IOptions<ServerConfiguration> configuration, ISessionRefHolder sessionRefHolder, IEnumerable<RequestFilter> requestFilters, IPipelineConfiguration pipelineConfiguration)
        {
            _channel = channel;
            _decoder = decoder;
            _encoder = encoder;
            _clientSession = clientSession;
            _configuration = configuration;
            _sessionRefHolder = sessionRefHolder;
            _requestFilters = requestFilters;
            _pipelineConfiguration = pipelineConfiguration;
        }

        /// <summary>
        /// Creates and configures the network channel pipeline with filters, decoder, encoder, and client handlers.
        /// </summary>
        public void CreatePipeline()
        {
            _sessionRefHolder[_channel.Id.AsLongText()] =
                new RegionTypeMapping(0, _configuration.Value.Language);
            var pipeline = _channel.Pipeline;
            foreach (var filter in _requestFilters)
            {
                pipeline.AddLast(filter);
            }

            if (_pipelineConfiguration.UseDelimiter)
            {
                pipeline.AddLast(new FrameDelimiter(_sessionRefHolder));
            }

            pipeline.AddLast(_decoder);
            _clientSession.RegisterChannel(_channel);
            pipeline.AddLast(_clientSession);
            pipeline.AddLast(_encoder);
        }
    }
}