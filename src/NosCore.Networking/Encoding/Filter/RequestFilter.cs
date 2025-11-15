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
    /// <summary>
    /// Abstract base class for request filters that process incoming byte data.
    /// </summary>
    public abstract class RequestFilter : MessageToMessageDecoder<IByteBuffer>
    {
        /// <summary>
        /// Filters incoming request data.
        /// </summary>
        /// <param name="context">The channel handler context.</param>
        /// <param name="message">The incoming message bytes.</param>
        /// <returns>The filtered byte array, or null if the request should be blocked.</returns>
        public abstract byte[]? Filter(IChannelHandlerContext context, Span<byte> message);

        /// <summary>
        /// Decodes the incoming byte buffer through the filter.
        /// </summary>
        /// <param name="context">The channel handler context.</param>
        /// <param name="message">The byte buffer to decode.</param>
        /// <param name="output">The output list to add filtered results to.</param>
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
