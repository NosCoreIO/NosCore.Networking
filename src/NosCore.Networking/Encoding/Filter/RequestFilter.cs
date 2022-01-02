//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Collections.Generic;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;

namespace NosCore.Networking.Encoding.Filter
{
    public abstract class RequestFilter : MessageToMessageDecoder<IByteBuffer>
    {
        public abstract byte[]? Filter(IChannelHandlerContext context, Span<byte> message);

        protected override void Decode(IChannelHandlerContext context, IByteBuffer message, List<object> output)
        {
            var result = Filter(context, ((Span<byte>)message.Array).Slice(message.ArrayOffset, message.ReadableBytes));
            if (result != null)
            {
                output.Add(Unpooled.WrappedBuffer(result));
            }
        }
    }
}
