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
using NosCore.Packets.Interfaces;

namespace NosCore.Networking.Encoding
{
    public class LoginDecoder : MessageToMessageDecoder<IByteBuffer>
    {
        private readonly IDeserializer _deserializer;
        private readonly ILogger _logger;
        private readonly ISessionRefHolder _sessionRefHolder;

        public LoginDecoder(ILogger logger, IDeserializer deserializer, ISessionRefHolder sessionRefHolder)
        {
            _logger = logger;
            _deserializer = deserializer;
            _sessionRefHolder = sessionRefHolder;
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
                    _logger.LogInformation("New client connected. ClientId = {SessionId}",
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
                    _logger.LogError("Packet with Header {des} is corrupt or PacketDefinition is invalid. Content: {PacketContent}", des.Header, decrypt);
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                _logger.LogError("Decoding error {0}", ex);
            }
        }
    }
}