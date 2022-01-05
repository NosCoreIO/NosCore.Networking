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
    public class NetworkClient : ChannelHandlerAdapter, INetworkClient
    {
        private const short MaxPacketsBuffer = 50;
        private readonly ILogger _logger;
        private readonly ILogLanguageLocalizer<LogLanguageKey> _logLanguage;

        public NetworkClient(ILogger logger, ILogLanguageLocalizer<LogLanguageKey> logLanguage)
        {
            _logger = logger;
            LastPackets = new ConcurrentQueue<IPacket?>();
            _logLanguage = logLanguage;
        }

        public IChannel? Channel { get; private set; }

        public bool HasSelectedCharacter { get; set; }

        public bool IsAuthenticated { get; set; }

        public int SessionId { get; set; }
        public ConcurrentQueue<IPacket?> LastPackets { get; }

        public async Task DisconnectAsync()
        {
            _logger.Information(_logLanguage[LogLanguageKey.FORCED_DISCONNECTION],
                SessionId);
            if (Channel != null)
            {
                await Channel.DisconnectAsync();
            }
        }

        public Task SendPacketAsync(IPacket? packet)
        {
            return SendPacketsAsync(new[] { packet });
        }

        public async Task SendPacketsAsync(IEnumerable<IPacket?> packets)
        {
            var packetlist = packets.ToList();
            var packetDefinitions = (packets as IPacket?[] ?? packetlist.ToArray()).Where(c => c != null);
            if (!packetDefinitions.Any())
            {
                return;
            }

            await Task.WhenAll(packetlist.Select(packet => Task.Run(() => LastPackets.Enqueue(packet)))).ConfigureAwait(false);
            Parallel.For(0, LastPackets.Count - MaxPacketsBuffer, (_, __) => LastPackets.TryDequeue(out var ___));
            if (Channel == null)
            {
                return;
            }
            await Channel.WriteAndFlushAsync(packetDefinitions).ConfigureAwait(false);
        }

        public void RegisterChannel(ISocketChannel? channel)
        {
            Channel = channel;
        }

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
                        _logger.Fatal(exception.StackTrace);
                        break;
                }
            }
            else
            {
                _logger.Fatal(exception.StackTrace);
            }

            // ReSharper disable once AsyncConverter.AsyncAwaitMayBeElidedHighlighting
            await context.CloseAsync().ConfigureAwait(false);
        }
    }
}