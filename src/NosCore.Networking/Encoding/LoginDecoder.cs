//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using NosCore.Networking.Resource;
using NosCore.Networking.SessionRef;
using NosCore.Packets.Interfaces;
using NosCore.Shared.I18N;

namespace NosCore.Networking.Encoding
{
    /// <summary>
    /// Decodes packets from login server communication using region-specific decoding.
    /// </summary>
    public class LoginDecoder : IDecoder
    {
        private readonly IDeserializer _deserializer;
        private readonly ILogger<LoginDecoder> _logger;
        private readonly ISessionRefHolder _sessionRefHolder;
        private readonly ILogLanguageLocalizer<LogLanguageKey> _logLanguage;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginDecoder"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="deserializer">The packet deserializer.</param>
        /// <param name="sessionRefHolder">The session reference holder.</param>
        /// <param name="logLanguage">The localized log language provider.</param>
        public LoginDecoder(ILogger<LoginDecoder> logger, IDeserializer deserializer, ISessionRefHolder sessionRefHolder, ILogLanguageLocalizer<LogLanguageKey> logLanguage)
        {
            _logger = logger;
            _deserializer = deserializer;
            _sessionRefHolder = sessionRefHolder;
            _logLanguage = logLanguage;
        }

        /// <summary>
        /// Decodes a byte span into a collection of packets using login server decoding.
        /// </summary>
        /// <param name="clientSessionId">The client session identifier.</param>
        /// <param name="message">The byte span containing the encoded packet data.</param>
        /// <returns>A collection of decoded packets.</returns>
        public IEnumerable<IPacket> Decode(string clientSessionId, Span<byte> message)
        {
            try
            {
                var decryptedPacket = new StringBuilder();
                var mapper = _sessionRefHolder[clientSessionId];
                if (mapper.SessionId == 0)
                {
                    _sessionRefHolder[clientSessionId].SessionId =
                        _sessionRefHolder.GenerateSessionId();
                    _logger.LogInformation(_logLanguage[LogLanguageKey.CLIENT_CONNECTED],
                        mapper.SessionId);
                }

                foreach (var character in message)
                {
                    decryptedPacket.Append(character > 14 ? Convert.ToChar((character - 15) ^ 195)
                        : Convert.ToChar((256 - (15 - character)) ^ 195));
                }

                var decrypt = decryptedPacket.ToString();
                var des = _deserializer.Deserialize(decrypt);
                if (des.IsValid)
                {
                    return new[] { des };
                }
                else if (!des.IsValid)
                {
                    _logger.LogError(_logLanguage[LogLanguageKey.CORRUPTED_PACKET], des.Header, decrypt);
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                _logger.LogError(_logLanguage[LogLanguageKey.ERROR_DECODING], ex);
            }

            return Array.Empty<IPacket>();
        }
    }
}
