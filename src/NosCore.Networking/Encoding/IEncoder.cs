//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Collections.Generic;
using NosCore.Packets.Interfaces;

namespace NosCore.Networking.Encoding
{
    /// <summary>
    /// Defines a packet encoder that converts packets to byte arrays for transmission.
    /// </summary>
    public interface IEncoder
    {
        /// <summary>
        /// Encodes a collection of packets into a byte array.
        /// </summary>
        /// <param name="clientSessionId">The client session identifier.</param>
        /// <param name="packets">The packets to encode.</param>
        /// <returns>A byte array containing the encoded packet data.</returns>
        byte[] Encode(string clientSessionId, IEnumerable<IPacket> packets);
    }
}