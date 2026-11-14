//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NosCore.Networking.Encoding;
using NosCore.Networking.SessionRef;
using SuperSocket.Server.Abstractions.Session;

namespace NosCore.Networking.Tests
{
    [TestClass]
    public class PipelineFactoryTests
    {
        private static (PipelineFactory Factory, IAppSession Session) Connect(System.Func<NosPackageInfo, INetworkClient, Task> handler)
        {
            var factory = new PipelineFactory(new Mock<IDecoder>().Object, new Mock<ISessionRefHolder>().Object, [],
                new Mock<IPipelineConfiguration>().Object, () => new Mock<INetworkClient>().Object, handler);
            var session = new Mock<IAppSession>();
            session.SetupGet(s => s.SessionID).Returns("session");
            factory.OnSessionConnected(session.Object);
            return (factory, session.Object);
        }

        [TestMethod]
        public async Task HandlingAPacketWaitsForTheHandler()
        {
            var handled = new TaskCompletionSource();
            var (factory, session) = Connect((_, _) => handled.Task);

            var handling = factory.HandlePackageAsync(session, new NosPackageInfo());

            Assert.IsFalse(handling.IsCompleted);
            handled.SetResult();
            await handling;
        }

        [TestMethod]
        public async Task APacketFromAnUnknownSessionIsDropped()
        {
            var called = false;
            var (factory, _) = Connect((_, _) =>
            {
                called = true;
                return Task.CompletedTask;
            });
            var stranger = new Mock<IAppSession>();
            stranger.SetupGet(s => s.SessionID).Returns("stranger");

            await factory.HandlePackageAsync(stranger.Object, new NosPackageInfo());

            Assert.IsFalse(called);
        }
    }
}
