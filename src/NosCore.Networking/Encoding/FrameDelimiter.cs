//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Collections.Generic;
using System.Linq;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using NosCore.Networking.SessionRef;

namespace NosCore.Networking.Encoding;

/// <summary>
/// Delimits incoming byte streams into frames based on session-specific delimiters.
/// </summary>
public class FrameDelimiter : ByteToMessageDecoder
{
    private readonly ISessionRefHolder _sessionRefHolder;

    /// <summary>
    /// Initializes a new instance of the <see cref="FrameDelimiter"/> class.
    /// </summary>
    /// <param name="sessionRefHolder">The session reference holder.</param>
    public FrameDelimiter(ISessionRefHolder sessionRefHolder)
    {
        _sessionRefHolder = sessionRefHolder;
    }

    /// <summary>
    /// Decodes the incoming byte buffer into frames based on delimiters.
    /// </summary>
    /// <param name="context">The channel handler context.</param>
    /// <param name="input">The input byte buffer.</param>
    /// <param name="output">The output list to add decoded frames to.</param>
    protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
    {
        var sessionId = context.Channel.Id.AsLongText();
        var mapper = _sessionRefHolder[sessionId];

        var currentDelimiter = GetDelimiter(mapper.SessionId, mapper.SessionId == 0);

        var startReaderIndex = input.ReaderIndex;
        var endReaderIndex = startReaderIndex + input.ReadableBytes;

        for (var i = startReaderIndex; i < endReaderIndex; i++)
        {
            if (input.GetByte(i) == currentDelimiter)
            {
                var frameLength = i - startReaderIndex + 1;
                var frame = input.Copy(startReaderIndex, frameLength);
                output.Add(frame);
                input.SetReaderIndex(i + 1);
                break;
            }
        }
    }

    /// <summary>
    /// Gets the delimiter byte for a specific session.
    /// </summary>
    /// <param name="session">The session identifier.</param>
    /// <param name="isFirstPacket">Whether this is the first packet in the session.</param>
    /// <returns>The delimiter byte for the session.</returns>
    public static byte GetDelimiter(int session, bool isFirstPacket = false)
    {
        int stype = !isFirstPacket ? (session >> 6) & 3 : -1;

        var key = session & 0xFF;

        return (byte)(stype switch
        {
            0 => (0xff + key + 0x40) & 0xFF,
            1 => (0xff - key - 0x40) & 0xFF,
            2 => ((0xff ^ 0xC3) + key + 0x40) & 0xFF,
            3 => ((0xff ^ 0xC3) - key - 0x40) & 0xFF,
            _ => (0xff + 0xF) & 0xFF
        });
    }
}