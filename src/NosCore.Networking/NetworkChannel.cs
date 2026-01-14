//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Threading.Tasks;
using SuperSocket.Connection;
using SuperSocket.Server.Abstractions.Session;

namespace NosCore.Networking
{
    /// <summary>
    /// Represents a communication channel that wraps a SuperSocket session.
    /// </summary>
    public class NetworkChannel : IChannel
    {
        private readonly IAppSession _session;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkChannel"/> class.
        /// </summary>
        /// <param name="session">The SuperSocket session to wrap.</param>
        public NetworkChannel(IAppSession session)
        {
            _session = session;
        }

        /// <summary>
        /// Gets the unique identifier for this channel.
        /// </summary>
        public string Id => _session.SessionID;

        /// <summary>
        /// Disconnects the channel asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous disconnect operation.</returns>
        public async Task DisconnectAsync()
        {
            await _session.CloseAsync(CloseReason.LocalClosing);
        }

        /// <summary>
        /// Sends data to the channel asynchronously.
        /// </summary>
        /// <param name="data">The data to send.</param>
        /// <returns>A task representing the asynchronous send operation.</returns>
        public ValueTask SendAsync(ReadOnlyMemory<byte> data)
        {
            return _session.SendAsync(data);
        }
    }
}
