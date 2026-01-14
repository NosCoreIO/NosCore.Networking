//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Threading.Tasks;
using NosCore.Packets.Interfaces;

namespace NosCore.Networking.SessionGroup;

/// <summary>
/// Defines a group of network sessions that can be managed collectively.
/// </summary>
public interface ISessionGroup
{
    /// <summary>
    /// Broadcasts packets to all sessions in the group.
    /// </summary>
    /// <param name="packetDefinitions">The array of packets to broadcast.</param>
    /// <returns>A task representing the asynchronous broadcast operation.</returns>
    Task Broadcast(IPacket[] packetDefinitions);

    /// <summary>
    /// Broadcasts packets to matching sessions in the group.
    /// </summary>
    /// <param name="packetDefinitions">The array of packets to broadcast.</param>
    /// <param name="sessionMatcher">The matcher to filter which sessions receive the packets.</param>
    /// <returns>A task representing the asynchronous broadcast operation.</returns>
    Task Broadcast(IPacket[] packetDefinitions, ISessionMatcher sessionMatcher);

    /// <summary>
    /// Adds a client to the session group.
    /// </summary>
    /// <param name="client">The client to add.</param>
    /// <returns>True if the client was added; otherwise, false.</returns>
    bool Add(INetworkClient client);

    /// <summary>
    /// Adds a client to the session group by channel.
    /// </summary>
    /// <param name="channel">The channel of the client to add.</param>
    /// <returns>True if the client was added; otherwise, false.</returns>
    bool Add(IChannel channel);

    /// <summary>
    /// Adds a client to the session group by session key.
    /// </summary>
    /// <param name="sessionKey">The session key of the client to add.</param>
    /// <returns>True if the client was added; otherwise, false.</returns>
    bool Add(string sessionKey);

    /// <summary>
    /// Removes a client from the session group.
    /// </summary>
    /// <param name="client">The client to remove.</param>
    /// <returns>True if the client was removed; otherwise, false.</returns>
    bool Remove(INetworkClient client);

    /// <summary>
    /// Removes a client from the session group by channel.
    /// </summary>
    /// <param name="channel">The channel of the client to remove.</param>
    /// <returns>True if the client was removed; otherwise, false.</returns>
    bool Remove(IChannel channel);

    /// <summary>
    /// Removes a client from the session group by session key.
    /// </summary>
    /// <param name="sessionKey">The session key of the client to remove.</param>
    /// <returns>True if the client was removed; otherwise, false.</returns>
    bool Remove(string sessionKey);

    /// <summary>
    /// Gets the number of clients in the session group.
    /// </summary>
    int Count { get; }
}
