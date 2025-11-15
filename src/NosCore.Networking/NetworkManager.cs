//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NosCore.Networking.Resource;
using NosCore.Shared.Configuration;
using NosCore.Shared.I18N;

namespace NosCore.Networking
{
    /// <summary>
    /// Manages the network server lifecycle and handles incoming client connections.
    /// </summary>
    public class NetworkManager
    {
        private readonly IOptions<ServerConfiguration> _configuration;
        private readonly ILogger<NetworkManager> _logger;
        private readonly Func<ISocketChannel, IPipelineFactory> _pipelineFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkManager"/> class.
        /// </summary>
        /// <param name="configuration">The server configuration options.</param>
        /// <param name="pipelineFactory">Factory function for creating pipeline instances per channel.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="logLanguage">The localized log language provider.</param>
        public NetworkManager(IOptions<ServerConfiguration> configuration,
            Func<ISocketChannel, IPipelineFactory> pipelineFactory, ILogger<NetworkManager> logger, ILogLanguageLocalizer<LogLanguageKey> logLanguage)
        {
            _configuration = configuration;
            _pipelineFactory = pipelineFactory;
            _logger = logger;
            _logLanguage = logLanguage;
        }

        private static readonly AutoResetEvent ClosingEvent = new AutoResetEvent(false);
        private readonly ILogLanguageLocalizer<LogLanguageKey> _logLanguage;

        /// <summary>
        /// Starts and runs the network server, listening for incoming client connections.
        /// </summary>
        /// <returns>A task representing the asynchronous server operation.</returns>
        public async Task RunServerAsync()
        {
            var bossGroup = new MultithreadEventLoopGroup(1);
            var workerGroup = new MultithreadEventLoopGroup();

            try
            {
                var bootstrap = new ServerBootstrap();
                bootstrap
                    .Group(bossGroup, workerGroup)
                    .Channel<TcpServerSocketChannel>()
                    .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
                        _pipelineFactory(channel).CreatePipeline()));

                _logger.LogInformation(_logLanguage[LogLanguageKey.LISTENING_PORT], _configuration.Value.Port);
                var bootstrapChannel = await bootstrap.BindAsync(_configuration.Value.Port).ConfigureAwait(false);
                Console.CancelKeyPress += ((s, a) =>
                {
                    ClosingEvent.Set();
                });
                ClosingEvent.WaitOne();

                await bootstrapChannel.CloseAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            finally
            {
                await Task.WhenAll(bossGroup.ShutdownGracefullyAsync(), workerGroup.ShutdownGracefullyAsync()).ConfigureAwait(false);
            }
        }
    }
}