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
using NosCore.Networking.Resource;
using NosCore.Networking.SessionRef;
using NosCore.Packets.Interfaces;
using NosCore.Shared.I18N;

namespace NosCore.Networking.Encoding
{
    public class LoginEncoder : MessageToMessageEncoder<IEnumerable<IPacket>>, IEncoder
    {
        private readonly ILogger<LoginEncoder> _logger;
        private readonly ISerializer _serializer;
        private readonly ISessionRefHolder _sessionRefHolder;
        private readonly ILogLanguageLocalizer<LogLanguageKey> _logLanguage;

        public LoginEncoder(ILogger<LoginEncoder> logger, ISerializer serializer, ISessionRefHolder sessionRefHolder, ILogLanguageLocalizer<LogLanguageKey> logLanguage)
        {
            _logger = logger;
            _serializer = serializer;
            _sessionRefHolder = sessionRefHolder;
            _logLanguage = logLanguage;
        }

        protected override void Encode(IChannelHandlerContext context, IEnumerable<IPacket> message,
            List<object> output)
        {
            output.Add(Unpooled.WrappedBuffer(Encode(context.Channel.Id.AsLongText(), message)));
        }

        public byte[] Encode(string clientSessionId, IEnumerable<IPacket> packets)
        {
            try
            {
                return packets.SelectMany(packet =>
                {
                    var packetString = _serializer.Serialize(packet);
                    var tmp = _sessionRefHolder[clientSessionId].RegionType.GetEncoding()!.GetBytes($"{packetString} ");
                    for (var i = 0; i < packetString.Length; i++)
                    {
                        tmp[i] = Convert.ToByte(tmp[i] + 15);
                    }

                    tmp[^1] = 25;
                    return tmp.Length == 0 ? new byte[] { 0xFF } : tmp;
                }).ToArray();
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                _logger.LogError(ex, _logLanguage[LogLanguageKey.ENCODE_ERROR], ex.Message);
            }

            return Array.Empty<byte>();
        }
    }
}