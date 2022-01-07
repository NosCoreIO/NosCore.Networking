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
using NosCore.Networking.Extensions;
using NosCore.Networking.SessionRef;
using NosCore.Packets.Interfaces;

namespace NosCore.Networking.Encoding
{
    public class WorldEncoder : MessageToMessageEncoder<IEnumerable<IPacket>>, IEncoder
    {
        private readonly ISerializer _serializer;
        private readonly ISessionRefHolder _sessionRefHolder;

        public WorldEncoder(ISerializer serializer, ISessionRefHolder sessionRefHolder)
        {
            _serializer = serializer;
            _sessionRefHolder = sessionRefHolder;
        }

        protected override void Encode(IChannelHandlerContext context, IEnumerable<IPacket> message,
            List<object> output)
        {
            output.Add(Unpooled.WrappedBuffer(Encode(context.Channel.Id.AsLongText(), message)));
        }

        public byte[] Encode(string clientSessionId, IEnumerable<IPacket> packets)
        {
            return packets.SelectMany(packet =>
            {
                var region = _sessionRefHolder[clientSessionId].RegionType.GetEncoding();
                var strBytes = region!.GetBytes(_serializer.Serialize(packet)).AsSpan();
                var bytesLength = strBytes.Length;

                var encryptedData = new byte[bytesLength + (int)Math.Ceiling((decimal)bytesLength / 0x7E) + 1];

                var j = 0;
                for (var i = 0; i < bytesLength; i++)
                {
                    if (i % 0x7E == 0)
                    {
                        encryptedData[i + j] = (byte)(bytesLength - i > 0x7E ? 0x7E : bytesLength - i);
                        j++;
                    }

                    encryptedData[i + j] = (byte)~strBytes[i];
                }

                encryptedData[^1] = 0xFF;
                return encryptedData;
            }).ToArray();
        }

    }
}