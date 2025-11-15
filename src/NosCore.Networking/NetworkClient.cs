//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using NosCore.Networking.Resource;
using NosCore.Packets.Interfaces;
using NosCore.Shared.I18N;
using Serilog;

namespace NosCore.Networking
{
    /// <summary>
    /// Represents a network client that manages packet communication and handles channel events.
    /// </summary>
    public class NetworkClient : ChannelHandlerAdapter, INetworkClient
    {
        private const short MaxPacketsBuffer = 50;
        private readonly ILogger _logger;
        private readonly ILogLanguageLocalizer<LogLanguageKey> _logLanguage;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkClient"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="logLanguage">The localized log language provider.</param>
        public NetworkClient(ILogger logger, ILogLanguageLocalizer<LogLanguageKey> logLanguage)
        {
            _logger = logger;
            LastPackets = new ConcurrentQueue<IPacket?>();
            _logLanguage = logLanguage;
        }

        /// <summary>
        /// Gets the communication channel associated with this client.
        /// </summary>
        public IChannel? Channel { get; private set; }

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
        /// Disconnects the client asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous disconnect operation.</returns>
        public async Task DisconnectAsync()
        {
            _logger.Information(_logLanguage[LogLanguageKey.FORCED_DISCONNECTION],
                SessionId);
            if (Channel != null)
            {
                await Channel.DisconnectAsync();
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
            var packetlist = packets.ToList();
            var packetDefinitions = (packets as IPacket?[] ?? packetlist.ToArray()).Where(c => c != null);
            if (!packetDefinitions.Any())
            {
                return;
            }


            await Task.WhenAll(packetlist.Select(packet => Task.Run(() =>
            {
                if (packet?.IsValid == false)
                {
                    _logger.Error(_logLanguage[LogLanguageKey.SENDING_INVALID_PACKET], packet.Header, packet.ValidationResult);
                }
                LastPackets.Enqueue(packet);
            }))).ConfigureAwait(false);
            Parallel.For(0, LastPackets.Count - MaxPacketsBuffer, (_, __) => LastPackets.TryDequeue(out var ___));
            if (Channel == null)
            {
                return;
            }
            await Channel.WriteAndFlushAsync(packetDefinitions).ConfigureAwait(false);
        }

        /// <summary>
        /// Registers a socket channel with this network client.
        /// </summary>
        /// <param name="channel">The socket channel to register.</param>
        public void RegisterChannel(ISocketChannel? channel)
        {
            Channel = channel;
        }

        /// <summary>
        /// Handles exceptions that occur during channel operations.
        /// </summary>
        /// <param name="context">The channel handler context.</param>
        /// <param name="exception">The exception that occurred.</param>
#pragma warning disable VSTHRD100 // Avoid async void methods
        public override async void ExceptionCaught(IChannelHandlerContext context, Exception exception)
#pragma warning restore VSTHRD100 // Avoid async void methods
        {
            if ((exception == null) || (context == null))
            {
                throw new ArgumentNullException(nameof(exception));
            }

            if (exception is SocketException sockException)
            {
                switch (sockException.SocketErrorCode)
                {
                    case SocketError.ConnectionReset:
                        _logger.Information(
                            "Client disconnected. ClientId = {SessionId}",
                            SessionId);
                        break;
                    default:
                        _logger.Fatal(exception.StackTrace ?? "");
                        break;
                }
            }
            else
            {
                _logger.Fatal(exception.StackTrace ?? "");
            }

            // ReSharper disable once AsyncConverter.AsyncAwaitMayBeElidedHighlighting
            await context.CloseAsync().ConfigureAwait(false);
        }
    }
}