//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Collections.Generic;
using NosCore.Packets.Interfaces;

namespace NosCore.Networking
{
    public class NosPackageInfo
    {
        public IEnumerable<IPacket> Packets { get; set; } = [];
    }
}
