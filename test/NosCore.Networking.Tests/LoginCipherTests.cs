//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NosCore.Networking.Encoding;
using NosCore.Networking.Resource;
using NosCore.Networking.SessionRef;
using NosCore.Packets.Interfaces;
using NosCore.Shared.Enumerations;
using NosCore.Shared.I18N;

namespace NosCore.Networking.Tests
{
    [TestClass]
    public class LoginCipherTests
    {
        private const string Session = "session";

        private SessionRefHolder _sessions = null!;
        private Mock<ISerializer> _serializer = null!;
        private Mock<IDeserializer> _deserializer = null!;
        private LoginEncoder _encoder = null!;
        private LoginDecoder _decoder = null!;

        [TestInitialize]
        public void Setup()
        {
            _sessions = new SessionRefHolder();
            _sessions[Session] = new RegionTypeMapping(0, RegionType.EN);
            _serializer = new Mock<ISerializer>();
            _deserializer = new Mock<IDeserializer>();
            var logLanguage = new Mock<ILogLanguageLocalizer<LogLanguageKey>>().Object;
            _encoder = new LoginEncoder(NullLogger<LoginEncoder>.Instance, _serializer.Object, _sessions, logLanguage);
            _decoder = new LoginDecoder(NullLogger<LoginDecoder>.Instance, _deserializer.Object, _sessions, logLanguage);
        }

        [TestMethod]
        public void EachCharacterIsShiftedAndThePacketEndsWithTheTerminator()
        {
            _serializer.Setup(s => s.Serialize(It.IsAny<IPacket>())).Returns("failc 5");

            var wire = _encoder.Encode(Session, [new Mock<IPacket>().Object]);

            CollectionAssert.AreEqual("failc 5".Select(c => (byte)(c + 15)).Append((byte)25).ToArray(), wire);
        }

        [TestMethod]
        public void SeveralPacketsAreEncodedOneAfterTheOther()
        {
            _serializer.SetupSequence(s => s.Serialize(It.IsAny<IPacket>())).Returns("a").Returns("bc");

            var wire = _encoder.Encode(Session, [new Mock<IPacket>().Object, new Mock<IPacket>().Object]);

            CollectionAssert.AreEqual(new byte[] { 'a' + 15, 25, 'b' + 15, 'c' + 15, 25 }, wire);
        }

        [TestMethod]
        public void AnEncodingFailureSendsNothing()
        {
            _serializer.Setup(s => s.Serialize(It.IsAny<IPacket>())).Throws<InvalidOperationException>();

            var wire = _encoder.Encode(Session, [new Mock<IPacket>().Object]);

            Assert.AreEqual(0, wire.Length);
        }

        [TestMethod]
        public void TheClientsLoginTextReachesTheDeserializer()
        {
            var packet = ValidPacket();
            _deserializer.Setup(d => d.Deserialize("NoS0575 1 admin")).Returns(packet);

            var packets = _decoder.Decode(Session, ClientEncrypt("NoS0575 1 admin"));

            Assert.AreSame(packet, packets.Single());
        }

        [TestMethod]
        public void AFirstPacketGivesTheConnectionASession()
        {
            _deserializer.Setup(d => d.Deserialize(It.IsAny<string>())).Returns(ValidPacket());

            _decoder.Decode(Session, ClientEncrypt("NoS0575"));

            Assert.AreNotEqual(0, _sessions[Session].SessionId);
        }

        [TestMethod]
        public void ALaterPacketKeepsTheConnectionsSession()
        {
            _sessions[Session].SessionId = 42;
            _deserializer.Setup(d => d.Deserialize(It.IsAny<string>())).Returns(ValidPacket());

            _decoder.Decode(Session, ClientEncrypt("NoS0575"));

            Assert.AreEqual(42, _sessions[Session].SessionId);
        }

        [TestMethod]
        public void AnInvalidPacketIsDropped()
        {
            var invalid = new Mock<IPacket>();
            invalid.SetupGet(p => p.IsValid).Returns(false);
            _deserializer.Setup(d => d.Deserialize(It.IsAny<string>())).Returns(invalid.Object);

            var packets = _decoder.Decode(Session, ClientEncrypt("NoS0575"));

            Assert.IsFalse(packets.Any());
        }

        [TestMethod]
        public void AnUnknownConnectionDecodesToNothing()
        {
            var packets = _decoder.Decode("stranger", ClientEncrypt("NoS0575"));

            Assert.IsFalse(packets.Any());
        }

        private static IPacket ValidPacket()
        {
            var packet = new Mock<IPacket>();
            packet.SetupGet(p => p.IsValid).Returns(true);
            return packet.Object;
        }

        private static byte[] ClientEncrypt(string text)
        {
            return text.Select(c => (byte)(((c ^ 195) + 15) & 0xFF)).ToArray();
        }
    }
}
