//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Net;

namespace NosCore.Networking.Encoding.Filter
{
    /// <summary>
    /// Defines a request filter that processes incoming byte data.
    /// </summary>
    public interface IRequestFilter
    {
        /// <summary>
        /// Filters incoming request data.
        /// </summary>
        /// <param name="remoteEndPoint">The remote endpoint of the connection.</param>
        /// <param name="message">The incoming message bytes.</param>
        /// <returns>The filtered byte array, or null if the request should be blocked.</returns>
        byte[]? Filter(EndPoint remoteEndPoint, Span<byte> message);
    }
}
