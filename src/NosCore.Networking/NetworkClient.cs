//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NosCore.Networking.Encoding;
using NosCore.Networking.Resource;
using NosCore.Packets.Interfaces;
using NosCore.Shared.I18N;
using Serilog;

namespace NosCore.Networking
{
    /// <summary>
    /// Represents a network client that manages packet communication and handles session events.
    /// </summary>
    public class NetworkClient : INetworkClient
    {
        private const short MaxPacketsBuffer = 50;
        private readonly ILogger _logger;
        private readonly ILogLanguageLocalizer<LogLanguageKey> _logLanguage;
        private readonly IEncoder _encoder;
        private IChannel? _channel;
        private string _sessionKey = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkClient"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="logLanguage">The localized log language provider.</param>
        /// <param name="encoder">The packet encoder.</param>
        public NetworkClient(ILogger logger, ILogLanguageLocalizer<LogLanguageKey> logLanguage, IEncoder encoder)
        {
            _logger = logger;
            _logLanguage = logLanguage;
            _encoder = encoder;
            LastPackets = new ConcurrentQueue<IPacket?>();
        }

        /// <summary>
        /// Gets the unique session key for this client (used for encoding/decoding).
        /// </summary>
        public string SessionKey => _sessionKey;

        /// <summary>
        /// Gets or sets a value indicating whether the client has selected a character.
        /// </summary>
        public bool HasSelectedCharacter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the client is authenticated.
        /// </summary>
        public bool IsAuthenticated { get; set; }

        /// <summary>
        /// Gets or sets the unique session identifier for this client.
        /// </summary>
        public int SessionId { get; set; }

        /// <summary>
        /// Gets the queue of recently sent packets.
        /// </summary>
        public ConcurrentQueue<IPacket?> LastPackets { get; }

        /// <summary>
        /// Gets the communication channel associated with this client.
        /// </summary>
        public IChannel? Channel => _channel;

        /// <summary>
        /// Registers a channel with this network client.
        /// </summary>
        /// <param name="channel">The channel to register.</param>
        public void RegisterChannel(IChannel channel)
        {
            _channel = channel;
            _sessionKey = channel.Id;
            NetworkClientRegistry.Register(this);
        }

        /// <summary>
        /// Disconnects the client asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous disconnect operation.</returns>
        public async Task DisconnectAsync()
        {
            _logger.Information(_logLanguage[LogLanguageKey.FORCED_DISCONNECTION], SessionId);
            NetworkClientRegistry.Unregister(this);
            if (_channel != null)
            {
                await _channel.DisconnectAsync();
            }
        }

        /// <summary>
        /// Sends a single packet to the client asynchronously.
        /// </summary>
        /// <param name="packet">The packet to send.</param>
        /// <returns>A task representing the asynchronous send operation.</returns>
        public Task SendPacketAsync(IPacket? packet)
        {
            return SendPacketsAsync(new[] { packet });
        }

        /// <summary>
        /// Sends multiple packets to the client asynchronously.
        /// </summary>
        /// <param name="packets">The collection of packets to send.</param>
        /// <returns>A task representing the asynchronous send operation.</returns>
        public async Task SendPacketsAsync(IEnumerable<IPacket?> packets)
        {
            var packetList = packets.Where(c => c != null).Cast<IPacket>().ToList();
            if (packetList.Count == 0)
            {
                return;
            }

            foreach (var packet in packetList)
            {
                if (packet?.IsValid == false)
                {
                    _logger.Error(_logLanguage[LogLanguageKey.SENDING_INVALID_PACKET], packet.Header, packet.ValidationResult);
                }
                LastPackets.Enqueue(packet);
            }

            while (LastPackets.Count > MaxPacketsBuffer)
            {
                LastPackets.TryDequeue(out _);
            }

            if (_channel == null)
            {
                return;
            }

            try
            {
                var encoded = _encoder.Encode(SessionKey, packetList);
                await _channel.SendAsync(new ReadOnlyMemory<byte>(encoded));
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, _logLanguage[LogLanguageKey.ENCODE_ERROR], SessionId);
            }
        }
    }
}
