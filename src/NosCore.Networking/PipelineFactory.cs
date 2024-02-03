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

        public void CreatePipeline()
        {
            _sessionRefHolder[_channel.Id.AsLongText()] =
                new RegionTypeMapping(0, _configuration.Value.Language);
            var pipeline = _channel.Pipeline;
            foreach (var filter in _requestFilters)
            {
                pipeline.AddLast(filter);
            }

            if (_pipelineConfiguration.Delimiter != null)
            {
                pipeline.AddLast(new DelimiterBasedFrameDecoder(8192, new[] {
                    Unpooled.WrappedBuffer(new[] { (byte)_pipelineConfiguration.Delimiter })
                }));
            }

            pipeline.AddLast(_decoder);
            _clientSession.RegisterChannel(_channel);
            pipeline.AddLast(_clientSession);
            pipeline.AddLast(_encoder);
        }
    }
}