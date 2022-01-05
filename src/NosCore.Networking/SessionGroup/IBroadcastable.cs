//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Collections.Concurrent;
using NosCore.Packets.Interfaces;

namespace NosCore.Networking.SessionGroup
{
    public interface IBroadcastable
    {
        ISessionGroup Sessions { get; set; }
        ConcurrentQueue<IPacket> LastPackets { get; }
        short MaxPacketsBuffer { get; }
    }
}