//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NosCore.Networking.Encoding;
using NosCore.Networking.Resource;
using NosCore.Networking.SessionRef;
using NosCore.Shared.Configuration;
using NosCore.Shared.I18N;

namespace NosCore.Networking.Tests
{
    [TestClass]
    public class NetworkManagerTests
    {
        [TestMethod]
        public async Task CancellingTheTokenStopsTheServer()
        {
            var probe = new TcpListener(IPAddress.Loopback, 0);
            probe.Start();
            var port = (ushort)((IPEndPoint)probe.LocalEndpoint).Port;
            probe.Stop();

            var logLanguage = new Mock<ILogLanguageLocalizer<LogLanguageKey>>();
            logLanguage.Setup(l => l[It.IsAny<LogLanguageKey>()])
                .Returns((LogLanguageKey key) => new LocalizedString(key.ToString(), key.ToString()));
            var pipelineFactory = new PipelineFactory(new Mock<IDecoder>().Object, new Mock<ISessionRefHolder>().Object, [],
                new Mock<IPipelineConfiguration>().Object, () => new Mock<INetworkClient>().Object, (_, _) => Task.CompletedTask);
            var manager = new NetworkManager(Options.Create(new ServerConfiguration { Port = port }), pipelineFactory,
                NullLogger<NetworkManager>.Instance, logLanguage.Object);
            using var cts = new CancellationTokenSource();

            var running = manager.RunServerAsync(cts.Token);
            using (var ready = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
            {
                await ConnectAsync(port, ready.Token);
            }

            cts.Cancel();

            await running.WaitAsync(TimeSpan.FromSeconds(10));
            using var afterStop = new TcpClient();
            await Assert.ThrowsAsync<SocketException>(() => afterStop.ConnectAsync(IPAddress.Loopback, port));
        }

        private static async Task ConnectAsync(ushort port, CancellationToken cancellationToken)
        {
            while (true)
            {
                using var client = new TcpClient();
                try
                {
                    await client.ConnectAsync(IPAddress.Loopback, port, cancellationToken);
                    return;
                }
                catch (SocketException)
                {
                    await Task.Delay(50, cancellationToken);
                }
            }
        }
    }
}
