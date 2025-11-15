//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using NosCore.Packets.Interfaces;

namespace NosCore.Networking
{
    /// <summary>
    /// Defines a network client that handles packet communication and channel management.
    /// </summary>
    public interface INetworkClient : IChannelHandler
    {
        /// <summary>
        /// Gets or sets the unique session identifier for this client.
        /// </summary>
        int SessionId { get; set; }

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

        /// <summary>
        /// Registers a socket channel with this network client.
        /// </summary>
        /// <param name="channel">The socket channel to register.</param>
        void RegisterChannel(ISocketChannel channel);
    }
}