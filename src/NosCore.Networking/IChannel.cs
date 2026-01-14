//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Threading.Tasks;

namespace NosCore.Networking
{
    /// <summary>
    /// Defines a communication channel for network operations.
    /// </summary>
    public interface IChannel
    {
        /// <summary>
        /// Gets the unique identifier for this channel.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Disconnects the channel asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous disconnect operation.</returns>
        Task DisconnectAsync();

        /// <summary>
        /// Sends data to the channel asynchronously.
        /// </summary>
        /// <param name="data">The data to send.</param>
        /// <returns>A task representing the asynchronous send operation.</returns>
        ValueTask SendAsync(ReadOnlyMemory<byte> data);
    }
}
