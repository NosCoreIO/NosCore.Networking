//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using DotNetty.Transport.Channels;
using NodaTime;

namespace NosCore.Networking.Encoding.Filter
{
    public class SpamRequestFilter : RequestFilter
    {
        private readonly Dictionary<EndPoint, Instant> _connectionsByIp = new();
        private readonly TimeSpan _timeBetweenConnection = TimeSpan.FromMilliseconds(1000);
        private readonly IClock _clock;
        public SpamRequestFilter(IClock clock)
        {
            _clock = clock;
        }

        public override byte[]? Filter(IChannelHandlerContext context, Span<byte> message)
        {
            if (_connectionsByIp.TryGetValue(context.Channel.RemoteAddress, out var date))
            {
                if (date.Plus(Duration.FromTimeSpan(_timeBetweenConnection)) > _clock.GetCurrentInstant())
                {
                    return null;
                }
            }

            _connectionsByIp[context.Channel.RemoteAddress] = _clock.GetCurrentInstant();
            return message.ToArray();
        }
    }
}
