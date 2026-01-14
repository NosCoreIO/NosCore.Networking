//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.Logging;
using NodaTime;
using NosCore.Networking.Resource;
using NosCore.Shared.I18N;

namespace NosCore.Networking.Encoding.Filter
{
    /// <summary>
    /// Filters spam requests by rate-limiting connections from the same IP address.
    /// </summary>
    public class SpamRequestFilter : IRequestFilter
    {
        private readonly Dictionary<EndPoint, Instant> _connectionsByIp = new();
        private readonly TimeSpan _timeBetweenConnection = TimeSpan.FromMilliseconds(1000);
        private readonly IClock _clock;
        private readonly ILogger<SpamRequestFilter> _logger;
        private readonly ILogLanguageLocalizer<LogLanguageKey> _logLanguage;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpamRequestFilter"/> class.
        /// </summary>
        /// <param name="clock">The clock instance for time tracking.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="logLanguage">The localized log language provider.</param>
        public SpamRequestFilter(IClock clock, ILogger<SpamRequestFilter> logger, ILogLanguageLocalizer<LogLanguageKey> logLanguage)
        {
            _clock = clock;
            _logger = logger;
            _logLanguage = logLanguage;
        }

        /// <summary>
        /// Filters incoming requests based on connection rate from the same IP address.
        /// </summary>
        /// <param name="remoteEndPoint">The remote endpoint of the connection.</param>
        /// <param name="message">The incoming message bytes.</param>
        /// <returns>The message bytes if allowed, or null if blocked by the spam filter.</returns>
        public byte[]? Filter(EndPoint remoteEndPoint, Span<byte> message)
        {
            if (_connectionsByIp.TryGetValue(remoteEndPoint, out var date))
            {
                if (date.Plus(Duration.FromTimeSpan(_timeBetweenConnection)) > _clock.GetCurrentInstant())
                {
                    _logger.LogWarning(_logLanguage[LogLanguageKey.BLOCKED_BY_SPAM_FILTER], remoteEndPoint);
                    return null;
                }
            }

            _connectionsByIp[remoteEndPoint] = _clock.GetCurrentInstant();
            return message.ToArray();
        }
    }
}
