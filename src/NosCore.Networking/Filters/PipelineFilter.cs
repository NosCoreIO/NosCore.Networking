//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using NosCore.Networking.Encoding;
using NosCore.Networking.Encoding.Filter;
using NosCore.Networking.SessionRef;
using SuperSocket.ProtoBase;
using SuperSocket.Server.Abstractions.Session;
using System;
using System.Buffers;
using System.Collections.Generic;

namespace NosCore.Networking.Filters
{
    public class PipelineFilter : IPipelineFilter<NosPackageInfo>
    {
        private readonly IDecoder _decoder;
        private readonly ISessionRefHolder _sessionRefHolder;
        private readonly IEnumerable<IRequestFilter> _requestFilters;
        private readonly bool _useDelimiter;

        public IPackageDecoder<NosPackageInfo>? Decoder { get; set; }
        public IPipelineFilter<NosPackageInfo>? NextFilter { get; private set; }
        public object? Context { get; set; }

        public PipelineFilter(
            IDecoder decoder,
            ISessionRefHolder sessionRefHolder,
            IEnumerable<IRequestFilter> requestFilters,
            bool useDelimiter)
        {
            _decoder = decoder;
            _sessionRefHolder = sessionRefHolder;
            _requestFilters = requestFilters;
            _useDelimiter = useDelimiter;
        }

        public NosPackageInfo Filter(ref SequenceReader<byte> reader)
        {
            var typedCtx = Context as IAppSession;
            if (reader.Remaining == 0)
            {
                return default!;
            }

            if (!_sessionRefHolder.TryGetValue(typedCtx!.SessionID, out var mapper))
            {
                mapper = new RegionTypeMapping(0, NosCore.Shared.Enumerations.RegionType.EN);
                _sessionRefHolder.Add(typedCtx.SessionID, mapper);
            }
            byte[] frameData;

            if (_useDelimiter)
            {
                var delimiter = FrameDelimiter.GetDelimiter(mapper.SessionId, mapper.SessionId == 0);

                if (!reader.TryReadTo(out ReadOnlySequence<byte> frame, delimiter, advancePastDelimiter: true))
                {
                    return default!;
                }

                frameData = frame.ToArray();
            }
            else
            {
                frameData = reader.UnreadSequence.ToArray();
                reader.Advance(reader.Remaining);
            }

            var filteredData = ApplyFilters(frameData);
            if (filteredData == null)
            {
                return default!;
            }

            var packets = _decoder.Decode(typedCtx.SessionID, filteredData.AsSpan());

            return new NosPackageInfo { Packets = packets };
        }

        private byte[]? ApplyFilters(byte[] data)
        {
            byte[]? currentData = data;
            var typedCtx = Context as IAppSession;
            foreach (var filter in _requestFilters)
            {
                if (currentData == null || typedCtx?.RemoteEndPoint == null)
                {
                    return null;
                }
                currentData = filter.Filter(typedCtx.RemoteEndPoint, currentData.AsSpan());
            }
            return currentData;
        }

        public void Reset()
        {
            NextFilter = null;
        }
    }
}
