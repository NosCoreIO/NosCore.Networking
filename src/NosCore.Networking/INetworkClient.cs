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
    /// <summary>
    /// Defines a network client that handles packet communication and session management.
    /// </summary>
    public interface INetworkClient
    {
        /// <summary>
        /// Gets the unique session key for this client.
        /// </summary>
        string SessionKey { get; }

        /// <summary>
        /// Gets or sets the unique session identifier for this client.
        /// </summary>
        int SessionId { get; set; }

        /// <summary>
        /// Gets the communication channel associated with this client.
        /// </summary>
        IChannel? Channel { get; }

        /// <summary>
        /// Registers a channel with this network client.
        /// </summary>
        /// <param name="channel">The channel to register.</param>
        void RegisterChannel(IChannel channel);

        /// <summary>
        /// Disconnects the client asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous disconnect operation.</returns>
        Task DisconnectAsync();

        /// <summary>
        /// Sends a single packet to the client asynchronously.
        /// </summary>
        /// <param name="packet">The packet to send.</param>
        /// <returns>A task representing the asynchronous send operation.</returns>
        Task SendPacketAsync(IPacket packet);

        /// <summary>
        /// Sends multiple packets to the client asynchronously.
        /// </summary>
        /// <param name="packets">The collection of packets to send.</param>
        /// <returns>A task representing the asynchronous send operation.</returns>
        Task SendPacketsAsync(IEnumerable<IPacket> packets);
    }
}
