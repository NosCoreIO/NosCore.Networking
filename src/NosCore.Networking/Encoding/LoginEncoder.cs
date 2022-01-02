//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using NosCore.Networking.Extensions;
using NosCore.Packets.Interfaces;

namespace NosCore.Networking.Encoding
{
    public class LoginEncoder : MessageToMessageEncoder<IEnumerable<IPacket>>
    {
        private readonly ILogger _logger;
        private readonly ISerializer _serializer;
        private readonly ISessionRefHolder _sessionRefHolder;

        public LoginEncoder(ILogger logger, ISerializer serializer, ISessionRefHolder sessionRefHolder)
        {
            _logger = logger;
            _serializer = serializer;
            _sessionRefHolder = sessionRefHolder;
        }

        protected override void Encode(IChannelHandlerContext context, IEnumerable<IPacket> message,
            List<object> output)
        {
            try
            {
                output.Add(Unpooled.WrappedBuffer(message.SelectMany(packet =>
                {
                    var packetString = _serializer.Serialize(packet);
                    var tmp = _sessionRefHolder[context.Channel.Id.AsLongText()].RegionType.GetEncoding()!.GetBytes($"{packetString} ");
                    for (var i = 0; i < packetString.Length; i++)
                    {
                        tmp[i] = Convert.ToByte(tmp[i] + 15);
                    }

                    tmp[^1] = 25;
                    return tmp.Length == 0 ? new byte[] { 0xFF } : tmp;
                }).ToArray()));
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                _logger.LogError(ex, "Encode Error: {0}", ex.Message);
            }
        }
    }
}