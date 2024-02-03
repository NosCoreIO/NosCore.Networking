//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Collections.Generic;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using NosCore.Networking.SessionRef;

namespace NosCore.Networking.Encoding;

public class FrameDelimiter : ByteToMessageDecoder
{
    private readonly ISessionRefHolder _sessionRefHolder;
    private readonly IPipelineConfiguration _pipelineConfiguration;

    public FrameDelimiter(ISessionRefHolder sessionRefHolder, IPipelineConfiguration pipelineConfiguration)
    {
        _sessionRefHolder = sessionRefHolder;
        _pipelineConfiguration = pipelineConfiguration;
    }

    protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
    {
        var sessionId = context.Channel.Id.AsLongText();
        var mapper = _sessionRefHolder[sessionId];

        var currentDelimiter = mapper.SessionId == 0 ? (byte)0xE : unchecked((byte)((_pipelineConfiguration.Delimiter ?? 0) + mapper.SessionId));

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
}