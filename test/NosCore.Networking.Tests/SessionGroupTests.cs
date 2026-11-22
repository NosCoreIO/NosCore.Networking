//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NosCore.Networking.Resource;
using NosCore.Networking.SessionGroup;
using NosCore.Networking.SessionGroup.ChannelMatcher;
using NosCore.Packets.Interfaces;
using NosCore.Shared.I18N;

namespace NosCore.Networking.Tests
{
    [TestClass]
    public class SessionGroupTests
    {
        private readonly IPacket[] _packets = [new Mock<IPacket>().Object];
        private ISessionGroup _group = null!;

        [TestInitialize]
        public void Setup()
        {
            _group = new SessionGroupFactory(NullLogger<SessionGroup.SessionGroup>.Instance,
                new Mock<ILogLanguageLocalizer<LogLanguageKey>>().Object).Create();
        }

        [TestMethod]
        public async Task ABroadcastReachesEveryMember()
        {
            var first = Client("first");
            var second = Client("second");
            _group.Add(first.Object);
            _group.Add(second.Object);

            await _group.Broadcast(_packets);

            first.Verify(c => c.SendPacketsAsync(_packets), Times.Once);
            second.Verify(c => c.SendPacketsAsync(_packets), Times.Once);
        }

        [TestMethod]
        public async Task OnlyReachesTheNamedSession()
        {
            var first = Client("first");
            var second = Client("second");
            _group.Add(first.Object);
            _group.Add(second.Object);

            await _group.Broadcast(_packets, new Only("second"));

            first.Verify(c => c.SendPacketsAsync(It.IsAny<IEnumerable<IPacket>>()), Times.Never);
            second.Verify(c => c.SendPacketsAsync(_packets), Times.Once);
        }

        [TestMethod]
        public async Task EveryoneButSkipsTheNamedSession()
        {
            var first = Client("first");
            var second = Client("second");
            _group.Add(first.Object);
            _group.Add(second.Object);

            await _group.Broadcast(_packets, new EveryoneBut("second"));

            first.Verify(c => c.SendPacketsAsync(_packets), Times.Once);
            second.Verify(c => c.SendPacketsAsync(It.IsAny<IEnumerable<IPacket>>()), Times.Never);
        }

        [TestMethod]
        public async Task AFailingMemberDoesNotStopTheBroadcast()
        {
            var broken = Client("broken");
            broken.Setup(c => c.SendPacketsAsync(It.IsAny<IEnumerable<IPacket>>())).ThrowsAsync(new InvalidOperationException());
            var healthy = Client("healthy");
            _group.Add(broken.Object);
            _group.Add(healthy.Object);

            await _group.Broadcast(_packets);
            await _group.Broadcast(_packets, new EveryoneBut("nobody"));

            healthy.Verify(c => c.SendPacketsAsync(_packets), Times.Exactly(2));
        }

        [TestMethod]
        public void ASessionJoinsOnlyOnce()
        {
            var client = Client("first");

            Assert.IsTrue(_group.Add(client.Object));
            Assert.IsFalse(_group.Add(client.Object));
            Assert.AreEqual(1, _group.Count);
        }

        [TestMethod]
        public void ARemovedSessionLeavesTheGroup()
        {
            var client = Client("first");
            _group.Add(client.Object);

            Assert.IsTrue(_group.Remove(client.Object));
            Assert.IsFalse(_group.Remove("first"));
            Assert.AreEqual(0, _group.Count);
        }

        [TestMethod]
        public void AConnectedClientJoinsThroughItsChannel()
        {
            var key = Guid.NewGuid().ToString();
            var client = Client(key);
            NetworkClientRegistry.Register(client.Object);
            var channel = new Mock<IChannel>();
            channel.SetupGet(c => c.Id).Returns(key);
            try
            {
                Assert.IsTrue(_group.Add(channel.Object));
                Assert.AreEqual(1, _group.Count);
                Assert.IsTrue(_group.Remove(channel.Object));
            }
            finally
            {
                NetworkClientRegistry.Unregister(key);
            }
        }

        [TestMethod]
        public void AnUnknownSessionCannotJoin()
        {
            Assert.IsFalse(_group.Add(Guid.NewGuid().ToString()));
            Assert.AreEqual(0, _group.Count);
        }

        private static Mock<INetworkClient> Client(string key)
        {
            var client = new Mock<INetworkClient>();
            client.SetupGet(c => c.SessionKey).Returns(key);
            client.Setup(c => c.SendPacketsAsync(It.IsAny<IEnumerable<IPacket>>())).Returns(Task.CompletedTask);
            return client;
        }
    }
}
