//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NosCore.Networking.SessionGroup;
using NosCore.Networking.SessionGroup.ChannelMatcher;
using NosCore.Packets.Interfaces;

namespace NosCore.Networking.Tests
{
    [TestClass]
    public class BroadcastableExtensionTests
    {
        private Mock<ISessionGroup> _sessions = null!;
        private Broadcastable _map = null!;

        [TestInitialize]
        public void Setup()
        {
            _sessions = new Mock<ISessionGroup>();
            _map = new Broadcastable { Sessions = _sessions.Object };
        }

        [TestMethod]
        public async Task APacketIsBroadcastToEveryone()
        {
            var packet = new Mock<IPacket>().Object;

            await _map.SendPacketAsync(packet);

            _sessions.Verify(s => s.Broadcast(It.Is<IPacket[]>(p => p.Single() == packet)), Times.Once);
        }

        [TestMethod]
        public async Task AMatcherNarrowsTheBroadcast()
        {
            var packet = new Mock<IPacket>().Object;
            var matcher = new Only("first");

            await _map.SendPacketAsync(packet, matcher);

            _sessions.Verify(s => s.Broadcast(It.Is<IPacket[]>(p => p.Single() == packet), matcher), Times.Once);
            _sessions.Verify(s => s.Broadcast(It.IsAny<IPacket[]>()), Times.Never);
        }

        [TestMethod]
        public async Task NothingIsBroadcastWhenThereAreNoPackets()
        {
            await _map.SendPacketsAsync([]);

            _sessions.Verify(s => s.Broadcast(It.IsAny<IPacket[]>()), Times.Never);
            Assert.IsTrue(_map.LastPackets.IsEmpty);
        }

        [TestMethod]
        public async Task OnlyTheLatestPacketsAreKept()
        {
            var packets = Enumerable.Range(0, 5).Select(_ => new Mock<IPacket>().Object).ToArray();

            await _map.SendPacketsAsync(packets);

            CollectionAssert.AreEqual(packets.Skip(2).ToArray(), _map.LastPackets.ToArray());
        }

        [TestMethod]
        public async Task PacketsAreKeptEvenWithoutSessions()
        {
            var map = new Broadcastable();

            await map.SendPacketAsync(new Mock<IPacket>().Object);

            Assert.AreEqual(1, map.LastPackets.Count);
        }

        private sealed class Broadcastable : IBroadcastable
        {
            public ISessionGroup Sessions { get; set; } = null!;

            public ConcurrentQueue<IPacket> LastPackets { get; } = new();

            public short MaxPacketsBuffer => 3;
        }
    }
}
