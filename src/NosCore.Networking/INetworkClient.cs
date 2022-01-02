//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using NosCore.Packets.Interfaces;

namespace NosCore.Networking
{
    public interface INetworkClient
    {
        int SessionId { get; set; }

        Task DisconnectAsync();

        Task SendPacketAsync(IPacket packet);

        Task SendPacketsAsync(IEnumerable<IPacket> packets);
    }
}