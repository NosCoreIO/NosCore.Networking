//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NosCore.Networking.Filters;
using NosCore.Networking.Resource;
using NosCore.Shared.Configuration;
using NosCore.Shared.I18N;
using Serilog;
using SuperSocket.ProtoBase;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Host;

namespace NosCore.Networking
{
    /// <summary>
    /// Manages the network server lifecycle and handles incoming client connections.
    /// </summary>
    public class NetworkManager
    {
        private readonly IOptions<ServerConfiguration> _configuration;
        private readonly ILogger<NetworkManager> _logger;
        private readonly ILogLanguageLocalizer<LogLanguageKey> _logLanguage;
        private readonly PipelineFactory _pipelineFactory;

        private static readonly AutoResetEvent ClosingEvent = new AutoResetEvent(false);

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkManager"/> class.
        /// </summary>
        /// <param name="configuration">The server configuration options.</param>
        /// <param name="pipelineFactory">Factory for creating pipeline filters.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="logLanguage">The localized log language provider.</param>
        public NetworkManager(IOptions<ServerConfiguration> configuration,
            PipelineFactory pipelineFactory, ILogger<NetworkManager> logger, ILogLanguageLocalizer<LogLanguageKey> logLanguage)
        {
            _configuration = configuration;
            _pipelineFactory = pipelineFactory;
            _logger = logger;
            _logLanguage = logLanguage;
        }

        /// <summary>
        /// Starts and runs the network server, listening for incoming client connections.
        /// </summary>
        /// <returns>A task representing the asynchronous server operation.</returns>
        public async Task RunServerAsync()
        {
            try
            {
                var host = SuperSocketHostBuilder.Create<NosPackageInfo, PipelineFilter>()
                    .UsePackageHandler(async (session, package) =>
                    {
                        _pipelineFactory.HandlePackage(session, package);
                        await Task.CompletedTask.ConfigureAwait(false);
                    })
                    .UseSessionHandler(
                        async session =>
                        {
                            _pipelineFactory.OnSessionConnected(session);
                            await Task.CompletedTask.ConfigureAwait(false);
                        },
                        async (session, e) =>
                        {
                            _pipelineFactory.OnSessionClosed(session);
                            await Task.CompletedTask.ConfigureAwait(false);
                        })
                    .ConfigureSuperSocket(options =>
                    {
                        options.Name = "NosCore";
                        options.Listeners = new List<ListenOptions>
                        {
                            new ListenOptions
                            {
                                Ip = "Any",
                                Port = _configuration.Value.Port
                            }
                        };
                    })
                    .ConfigureLogging((context, logging) =>
                    {
                        logging.ClearProviders();
                        logging.AddSerilog();
                    })
                    .ConfigureServices((context, services) =>
                    {
                        services.AddSingleton<IPipelineFilterFactory<NosPackageInfo>>(_pipelineFactory);
                    })
                    .Build();

                _logger.LogInformation(_logLanguage[LogLanguageKey.LISTENING_PORT], _configuration.Value.Port);
                await host.StartAsync().ConfigureAwait(false);
                Console.CancelKeyPress += ((s, a) =>
                {
                    ClosingEvent.Set();
                });
                ClosingEvent.WaitOne();

                await host.StopAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
}