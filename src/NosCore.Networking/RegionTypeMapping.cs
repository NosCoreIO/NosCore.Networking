//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using NosCore.Shared.Enumerations;

namespace NosCore.Networking
{
    public class RegionTypeMapping
    {
        public RegionTypeMapping(int sessionId, RegionType regionType)
        {
            SessionId = sessionId;
            RegionType = regionType;
        }

        public int SessionId { get; set; }
        public RegionType RegionType { get; set; }
    }
}