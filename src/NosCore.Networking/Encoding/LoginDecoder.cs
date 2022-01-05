//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using NosCore.Networking.Resource;
using NosCore.Networking.SessionRef;
using NosCore.Packets.Interfaces;
using NosCore.Shared.I18N;

namespace NosCore.Networking.Encoding
{
    public class LoginDecoder : MessageToMessageDecoder<IByteBuffer>
    {
        private readonly IDeserializer _deserializer;
        private readonly ILogger<LoginDecoder> _logger;
        private readonly ISessionRefHolder _sessionRefHolder;
        private readonly ILogLanguageLocalizer<LogLanguageKey> _logLanguage;

        public LoginDecoder(ILogger<LoginDecoder> logger, IDeserializer deserializer, ISessionRefHolder sessionRefHolder, ILogLanguageLocalizer<LogLanguageKey> logLanguage)
        {
            _logger = logger;
            _deserializer = deserializer;
            _sessionRefHolder = sessionRefHolder;
            _logLanguage = logLanguage;
        }

        protected override void Decode(IChannelHandlerContext context, IByteBuffer message, List<object> output)
        {
            try
            {
                var decryptedPacket = new StringBuilder();
                var mapper = _sessionRefHolder[context.Channel.Id.AsLongText()];
                if (mapper.SessionId == 0)
                {
                    _sessionRefHolder[context.Channel.Id.AsLongText()].SessionId =
                        _sessionRefHolder.GenerateSessionId();
                    _logger.LogInformation(_logLanguage[LogLanguageKey.CLIENT_CONNECTED],
                        mapper.SessionId);
                }

                foreach (var character in ((Span<byte>)message.Array).Slice(message.ArrayOffset, message.ReadableBytes)
                )
                {
                    decryptedPacket.Append(character > 14 ? Convert.ToChar((character - 15) ^ 195)
                        : Convert.ToChar((256 - (15 - character)) ^ 195));
                }

                var decrypt = decryptedPacket.ToString();
                var des = _deserializer.Deserialize(decrypt);
                if (des.IsValid)
                {
                    output.Add(new[] { des });
                }
                else if (!des.IsValid)
                {
                    _logger.LogError(_logLanguage[LogLanguageKey.CORRUPTED_PACKET], des.Header, decrypt);
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                _logger.LogError(_logLanguage[LogLanguageKey.ERROR_DECODING], ex);
            }
        }
    }
}