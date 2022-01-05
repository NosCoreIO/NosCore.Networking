//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using NodaTime;
using NosCore.Networking.Resource;
using NosCore.Shared.I18N;

namespace NosCore.Networking.Encoding.Filter
{
    public class SpamRequestFilter : RequestFilter
    {
        private readonly Dictionary<EndPoint, Instant> _connectionsByIp = new();
        private readonly TimeSpan _timeBetweenConnection = TimeSpan.FromMilliseconds(1000);
        private readonly IClock _clock;
        private readonly ILogger<SpamRequestFilter> _logger;
        private readonly ILogLanguageLocalizer<LogLanguageKey> _logLanguage;
        public override bool IsSharable => true;

        public SpamRequestFilter(IClock clock, ILogger<SpamRequestFilter> logger, ILogLanguageLocalizer<LogLanguageKey> logLanguage)
        {
            _clock = clock;
            _logger = logger;
            _logLanguage = logLanguage;
        }

        public override byte[]? Filter(IChannelHandlerContext context, Span<byte> message)
        {
            if (_connectionsByIp.TryGetValue(context.Channel.RemoteAddress, out var date))
            {
                if (date.Plus(Duration.FromTimeSpan(_timeBetweenConnection)) > _clock.GetCurrentInstant())
                {
                    _logger.LogWarning(_logLanguage[LogLanguageKey.BLOCKED_BY_SPAM_FILTER], context.Channel.RemoteAddress);
                    return null;
                }
            }

            _connectionsByIp[context.Channel.RemoteAddress] = _clock.GetCurrentInstant();
            return message.ToArray();
        }
    }
}
