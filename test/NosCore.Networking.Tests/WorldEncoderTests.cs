//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NosCore.Networking.Encoding;
using NosCore.Networking.SessionRef;
using NosCore.Packets.Interfaces;
using NosCore.Shared.Enumerations;

namespace NosCore.Networking.Tests
{
    [TestClass]
    public class WorldEncoderTests
    {
        private const string Session = "session";

        private Mock<ISerializer> _serializer = null!;
        private SessionRefHolder _sessions = null!;
        private WorldEncoder _encoder = null!;

        [TestInitialize]
        public void Setup()
        {
            _serializer = new Mock<ISerializer>();
            _sessions = new SessionRefHolder();
            _sessions[Session] = new RegionTypeMapping(2, RegionType.EN);
            _encoder = new WorldEncoder(_serializer.Object, _sessions);
        }

        [TestMethod]
        public void AShortPacketIsOneChunkOfInvertedBytes()
        {
            _serializer.Setup(s => s.Serialize(It.IsAny<IPacket>())).Returns("say 1");

            var wire = _encoder.Encode(Session, [new Mock<IPacket>().Object]);

            var expected = new List<byte> { 5 };
            expected.AddRange("say 1".Select(c => (byte)~c));
            expected.Add(0xFF);
            CollectionAssert.AreEqual(expected, wire);
        }

        [TestMethod]
        public void ALongPacketIsSplitIntoChunksOfAHundredAndTwentySixBytes()
        {
            var text = new string('a', 0x7E + 10);
            _serializer.Setup(s => s.Serialize(It.IsAny<IPacket>())).Returns(text);

            var wire = _encoder.Encode(Session, [new Mock<IPacket>().Object]);

            Assert.AreEqual(text.Length + 2 + 1, wire.Length);
            Assert.AreEqual(0x7E, wire[0]);
            Assert.AreEqual(10, wire[0x7E + 1]);
            Assert.AreEqual(0xFF, wire[^1]);
            Assert.AreEqual(text, Decode(wire));
        }

        [TestMethod]
        public void EachPacketEndsWithItsOwnTerminator()
        {
            _serializer.SetupSequence(s => s.Serialize(It.IsAny<IPacket>())).Returns("a").Returns("b");

            var wire = _encoder.Encode(Session, [new Mock<IPacket>().Object, new Mock<IPacket>().Object]);

            CollectionAssert.AreEqual(new byte[] { 1, unchecked((byte)~'a'), 0xFF, 1, unchecked((byte)~'b'), 0xFF }, wire);
        }

        [TestMethod]
        public void TextIsEncodedWithTheConnectionsCodePage()
        {
            _sessions[Session].RegionType = RegionType.PL;
            _serializer.Setup(s => s.Serialize(It.IsAny<IPacket>())).Returns("ż");

            var wire = _encoder.Encode(Session, [new Mock<IPacket>().Object]);

            Assert.AreEqual(0xBF, (byte)~wire[1]);
        }

        private static string Decode(byte[] wire)
        {
            var bytes = new List<byte>();
            var i = 0;
            while (wire[i] != 0xFF)
            {
                var length = wire[i++];
                for (var j = 0; j < length; j++)
                {
                    bytes.Add((byte)~wire[i++]);
                }
            }

            return new string(bytes.Select(b => (char)b).ToArray());
        }
    }
}
