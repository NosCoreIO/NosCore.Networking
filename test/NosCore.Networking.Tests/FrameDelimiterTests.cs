//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NosCore.Networking.Encoding;

namespace NosCore.Networking.Tests
{
    [TestClass]
    public class FrameDelimiterTests
    {
        [TestMethod]
        public void TheFirstPacketEndsWithTheSessionlessDelimiter()
        {
            Assert.AreEqual(0x0E, FrameDelimiter.GetDelimiter(70, true));
        }

        [TestMethod]
        [DataRow(2, 0x41)]
        [DataRow(70, 0x79)]
        [DataRow(130, 0xFE)]
        [DataRow(200, 0x34)]
        public void EachSessionKindHasItsOwnDelimiter(int session, int delimiter)
        {
            Assert.AreEqual((byte)delimiter, FrameDelimiter.GetDelimiter(session));
        }
    }
}
