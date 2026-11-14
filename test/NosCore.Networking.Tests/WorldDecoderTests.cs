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
using NosCore.Packets;
using NosCore.Shared.Enumerations;
using NosCore.Shared.I18N;

namespace NosCore.Networking.Tests
{
    [TestClass]
    public class WorldDecoderTests
    {
        private const int ConnectedSession = 2;
        private const int ArrivingSession = 70;

        private static readonly System.Text.Encoding Cp1250 = System.Text.CodePagesEncodingProvider.Instance.GetEncoding(1250)!;
        private static readonly System.Text.Encoding Cp1252 = System.Text.CodePagesEncodingProvider.Instance.GetEncoding(1252)!;

        private SessionsWithTraffic _sessions = null!;
        private WorldDecoder _decoder = null!;
        private UnresolvedPacket? _connectedPacket;

        [TestInitialize]
        public void Setup()
        {
            _sessions = new SessionsWithTraffic();
            _sessions["connected"] = new RegionTypeMapping(ConnectedSession, RegionType.EN);
            _sessions["arriving"] = new RegionTypeMapping(0, RegionType.PL);
            _decoder = new WorldDecoder(new Deserializer(Array.Empty<Type>()), NullLogger<WorldDecoder>.Instance,
                _sessions, new Mock<ILogLanguageLocalizer<LogLanguageKey>>().Object);

            // Every connection decodes through this one decoder, so another connection's packet can
            // land in the middle of the arriving one's decode.
            _sessions.OnLookup("arriving", () => _connectedPacket = (UnresolvedPacket)_decoder
                .Decode("connected", ClientCipher.Encrypt("6001 walk 10 12", ConnectedSession, Cp1252)).Single());
        }

        [TestMethod]
        public void AnArrivingConnectionKeepsItsSessionWhileAnotherIsDecoded()
        {
            _decoder.Decode("arriving", ClientCipher.SessionPacket("5000 70"));

            Assert.AreEqual(ArrivingSession, _sessions["arriving"].SessionId);
            Assert.AreEqual("walk", _connectedPacket?.Header);
        }

        [TestMethod]
        public void AnArrivingConnectionsTextKeepsItsRegionWhileAnotherIsDecoded()
        {
            // The chat line shares the session packet's frame, so it decodes after the other connection.
            var frame = ClientCipher.SessionPacket("5000 70")
                .Concat(ClientCipher.Encrypt("5001 say Zażółć gęślą jaźń", ArrivingSession, Cp1250)).ToArray();

            var packets = _decoder.Decode("arriving", frame);

            Assert.AreEqual("Zażółć gęślą jaźń", (packets.ElementAtOrDefault(1) as UnresolvedPacket)?.Body);
        }

        // Runs another connection's decode whenever the decoder looks this connection's session up,
        // which Decode also does mid-decode, to store the session the client sent.
        private sealed class SessionsWithTraffic : SessionRefHolder, IDictionary<string, RegionTypeMapping>
        {
            private string? _key;
            private Action? _otherConnection;

            public void OnLookup(string key, Action otherConnection)
            {
                _key = key;
                _otherConnection = otherConnection;
            }

            RegionTypeMapping IDictionary<string, RegionTypeMapping>.this[string key]
            {
                get
                {
                    if (key == _key)
                    {
                        _otherConnection?.Invoke();
                    }

                    return base[key];
                }
                set => base[key] = value;
            }
        }

        // The client's half of the world cipher, which the decoder undoes.
        private static class ClientCipher
        {
            // "<keepalive> <session>", sent before the server knows the session.
            public static byte[] SessionPacket(string text)
            {
                var nibbles = text.Select(c => c == ' ' ? 1 : c - '0' + 4).ToArray();
                var wire = new List<byte> { (byte)(((0x80 | text.Length) + 0x0F) & 0xFF) };
                for (var i = 0; i < nibbles.Length; i += 2)
                {
                    var low = i + 1 < nibbles.Length ? nibbles[i + 1] : 0;
                    wire.Add((byte)((((nibbles[i] << 4) | low) + 0x0F) & 0xFF));
                }

                wire.Add(0x0E);
                return wire.ToArray();
            }

            // One raw chunk, so packets up to 0x7A bytes.
            public static byte[] Encrypt(string packet, int session, System.Text.Encoding codePage)
            {
                var plain = codePage.GetBytes(packet);
                var packed = new List<byte> { (byte)plain.Length };
                packed.AddRange(plain.Select(b => (byte)(b ^ 0xFF)));
                packed.Add(0xFF);

                var key = (session + 0x40) & 0xFF;
                return packed.Select(b => (byte)(((session >> 6) & 3) switch
                {
                    0 => b + key,
                    1 => b - key,
                    2 => (b ^ 0xC3) + key,
                    _ => (b ^ 0xC3) - key,
                } & 0xFF)).ToArray();
            }
        }
    }
}
