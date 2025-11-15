//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Collections.Generic;
using DotNetty.Transport.Channels;
using NosCore.Packets.Interfaces;

namespace NosCore.Networking.Encoding
{
    /// <summary>
    /// Defines a packet decoder that converts byte data into packets.
    /// </summary>
    public interface IDecoder : IChannelHandler
    {
        /// <summary>
        /// Decodes a byte span into a collection of packets.
        /// </summary>
        /// <param name="clientSessionId">The client session identifier.</param>
        /// <param name="message">The byte span containing the encoded packet data.</param>
        /// <returns>A collection of decoded packets.</returns>
        IEnumerable<IPacket> Decode(string clientSessionId, Span<byte> message);
    }
}