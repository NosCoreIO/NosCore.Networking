//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Text;
using NosCore.Shared.Enumerations;

namespace NosCore.Networking.Extensions
{
    public static class RegionTypeExtension
    {
        public static System.Text.Encoding? GetEncoding(this RegionType region)
        {
            return region switch
            {
                RegionType.ES => CodePagesEncodingProvider.Instance.GetEncoding(1252),
                RegionType.EN => CodePagesEncodingProvider.Instance.GetEncoding(1252),
                RegionType.FR => CodePagesEncodingProvider.Instance.GetEncoding(1252),
                RegionType.DE => CodePagesEncodingProvider.Instance.GetEncoding(1250),
                RegionType.IT => CodePagesEncodingProvider.Instance.GetEncoding(1250),
                RegionType.PL => CodePagesEncodingProvider.Instance.GetEncoding(1250),
                RegionType.CS => CodePagesEncodingProvider.Instance.GetEncoding(1250),
                RegionType.TR => CodePagesEncodingProvider.Instance.GetEncoding(1254),
                RegionType.RU => CodePagesEncodingProvider.Instance.GetEncoding(1251),
                _ => System.Text.Encoding.Default
            };
        }
    }
}