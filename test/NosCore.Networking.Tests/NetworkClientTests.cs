//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NosCore.Networking.Encoding;
using NosCore.Networking.Resource;
using NosCore.Packets.Interfaces;
using NosCore.Shared.I18N;

namespace NosCore.Networking.Tests
{
    [TestClass]
    public class NetworkClientTests
    {
        private static readonly byte[] Encoded = [1, 2, 3];

        private Mock<IEncoder> _encoder = null!;
        private Mock<IChannel> _channel = null!;
        private NetworkClient _client = null!;
        private string _key = null!;

        [TestInitialize]
        public void Setup()
        {
            _key = Guid.NewGuid().ToString();
            _encoder = new Mock<IEncoder>();
            _encoder.Setup(e => e.Encode(It.IsAny<string>(), It.IsAny<IEnumerable<IPacket>>())).Returns(Encoded);
            _channel = new Mock<IChannel>();
            _channel.SetupGet(c => c.Id).Returns(_key);
            _client = new NetworkClient(NullLogger.Instance, new Mock<ILogLanguageLocalizer<LogLanguageKey>>().Object,
                _encoder.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            NetworkClientRegistry.Unregister(_key);
        }

        [TestMethod]
        public void RegisteringAChannelMakesTheClientReachable()
        {
            _client.RegisterChannel(_channel.Object);

            Assert.AreEqual(_key, _client.SessionKey);
            Assert.AreSame(_channel.Object, _client.Channel);
            Assert.IsTrue(NetworkClientRegistry.TryGetClient(_key, out var found));
            Assert.AreSame(_client, found);
        }

        [TestMethod]
        public async Task DisconnectingClosesTheChannelAndForgetsTheClient()
        {
            _client.RegisterChannel(_channel.Object);

            await _client.DisconnectAsync();

            _channel.Verify(c => c.DisconnectAsync(), Times.Once);
            Assert.IsFalse(NetworkClientRegistry.TryGetClient(_key, out _));
        }

        [TestMethod]
        public async Task APacketIsEncodedAndSentOnTheChannel()
        {
            _client.RegisterChannel(_channel.Object);
            var packet = ValidPacket();

            await _client.SendPacketAsync(packet);

            _encoder.Verify(e => e.Encode(_key, It.Is<IEnumerable<IPacket>>(p => p.Single() == packet)), Times.Once);
            _channel.Verify(c => c.SendAsync(It.Is<ReadOnlyMemory<byte>>(m => m.ToArray().SequenceEqual(Encoded))), Times.Once);
        }

        [TestMethod]
        public async Task NullPacketsAreNotSent()
        {
            _client.RegisterChannel(_channel.Object);

            await _client.SendPacketsAsync(new IPacket?[] { null });

            _channel.Verify(c => c.SendAsync(It.IsAny<ReadOnlyMemory<byte>>()), Times.Never);
            Assert.IsTrue(_client.LastPackets.IsEmpty);
        }

        [TestMethod]
        public async Task PacketsSentBeforeAChannelAreOnlyRemembered()
        {
            var packet = ValidPacket();

            await _client.SendPacketAsync(packet);

            Assert.AreSame(packet, _client.LastPackets.Single());
            _encoder.Verify(e => e.Encode(It.IsAny<string>(), It.IsAny<IEnumerable<IPacket>>()), Times.Never);
        }

        [TestMethod]
        public async Task OnlyTheLastFiftyPacketsAreRemembered()
        {
            var packets = Enumerable.Range(0, 60).Select(_ => ValidPacket()).ToList();

            await _client.SendPacketsAsync(packets);

            CollectionAssert.AreEqual(packets.Skip(10).ToList(), _client.LastPackets.ToList());
        }

        [TestMethod]
        public async Task AnInvalidPacketIsStillSent()
        {
            _client.RegisterChannel(_channel.Object);
            var invalid = new Mock<IPacket>();
            invalid.SetupGet(p => p.IsValid).Returns(false);
            invalid.SetupGet(p => p.ValidationResult).Returns([
                new ValidationResult("too long", ["Message"]),
                new ValidationResult(null),
            ]);

            await _client.SendPacketAsync(invalid.Object);

            _channel.Verify(c => c.SendAsync(It.IsAny<ReadOnlyMemory<byte>>()), Times.Once);
        }

        [TestMethod]
        public async Task AnEncodingFailureDoesNotReachTheCaller()
        {
            _client.RegisterChannel(_channel.Object);
            _encoder.Setup(e => e.Encode(It.IsAny<string>(), It.IsAny<IEnumerable<IPacket>>()))
                .Throws<InvalidOperationException>();

            await _client.SendPacketAsync(ValidPacket());

            _channel.Verify(c => c.SendAsync(It.IsAny<ReadOnlyMemory<byte>>()), Times.Never);
        }

        private static IPacket ValidPacket()
        {
            var packet = new Mock<IPacket>();
            packet.SetupGet(p => p.IsValid).Returns(true);
            return packet.Object;
        }
    }
}
