//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NosCore.Networking.Extensions;
using NosCore.Shared.Enumerations;

namespace NosCore.Networking.Tests
{
    [TestClass]
    public class RegionTypeExtensionTests
    {
        [TestMethod]
        [DataRow(RegionType.EN, 1252)]
        [DataRow(RegionType.ES, 1252)]
        [DataRow(RegionType.FR, 1252)]
        [DataRow(RegionType.DE, 1250)]
        [DataRow(RegionType.IT, 1250)]
        [DataRow(RegionType.PL, 1250)]
        [DataRow(RegionType.CS, 1250)]
        [DataRow(RegionType.TR, 1254)]
        [DataRow(RegionType.RU, 1251)]
        public void EachRegionUsesItsClientsCodePage(RegionType region, int codePage)
        {
            Assert.AreEqual(codePage, region.GetEncoding()?.CodePage);
        }
    }
}
