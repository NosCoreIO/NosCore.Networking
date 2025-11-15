//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Collections.Concurrent;
using NosCore.Packets.Interfaces;

namespace NosCore.Networking.SessionGroup
{
    /// <summary>
    /// Defines an entity that can broadcast packets to multiple sessions.
    /// </summary>
    public interface IBroadcastable
    {
        /// <summary>
        /// Gets or sets the session group for broadcasting packets.
        /// </summary>
        ISessionGroup Sessions { get; set; }

        /// <summary>
        /// Gets the queue of recently broadcast packets.
        /// </summary>
        ConcurrentQueue<IPacket> LastPackets { get; }

        /// <summary>
        /// Gets the maximum number of packets to retain in the buffer.
        /// </summary>
        short MaxPacketsBuffer { get; }
    }
}