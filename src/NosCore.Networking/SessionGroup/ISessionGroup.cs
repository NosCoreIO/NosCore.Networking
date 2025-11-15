//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Groups;
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
    /// <param name="channelMatcher">The matcher to filter which channels receive the packets.</param>
    /// <returns>A task representing the asynchronous broadcast operation.</returns>
    Task Broadcast(IPacket[] packetDefinitions, IChannelMatcher channelMatcher);

    /// <summary>
    /// Adds a channel to the session group.
    /// </summary>
    /// <param name="channel">The channel to add.</param>
    /// <returns>True if the channel was added; otherwise, false.</returns>
    bool Add(IChannel channel);

    /// <summary>
    /// Removes a channel from the session group.
    /// </summary>
    /// <param name="channel">The channel to remove.</param>
    /// <returns>True if the channel was removed; otherwise, false.</returns>
    bool Remove(IChannel channel);

    /// <summary>
    /// Gets the number of channels in the session group.
    /// </summary>
    int Count { get; }
}