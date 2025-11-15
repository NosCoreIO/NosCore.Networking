//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Threading.Tasks;
using DotNetty.Common.Concurrency;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Groups;
using NosCore.Packets.Interfaces;

namespace NosCore.Networking.SessionGroup;

/// <summary>
/// Manages a group of network sessions and provides broadcasting capabilities.
/// </summary>
public class SessionGroup : ISessionGroup
{
    private readonly DefaultChannelGroup _channelGroup;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionGroup"/> class.
    /// </summary>
    public SessionGroup()
    {
        ExecutionEnvironment.TryGetCurrentExecutor(out var executor);
        _channelGroup = new DefaultChannelGroup(executor);
    }

    /// <summary>
    /// Broadcasts packets to all sessions in the group.
    /// </summary>
    /// <param name="packetDefinitions">The array of packets to broadcast.</param>
    /// <returns>A task representing the asynchronous broadcast operation.</returns>
    public Task Broadcast(IPacket[] packetDefinitions)
    {
        return _channelGroup.WriteAndFlushAsync(packetDefinitions);
    }

    /// <summary>
    /// Broadcasts packets to matching sessions in the group.
    /// </summary>
    /// <param name="packetDefinitions">The array of packets to broadcast.</param>
    /// <param name="channelMatcher">The matcher to filter which channels receive the packets.</param>
    /// <returns>A task representing the asynchronous broadcast operation.</returns>
    public Task Broadcast(IPacket[] packetDefinitions, IChannelMatcher channelMatcher)
    {
        return _channelGroup.WriteAndFlushAsync(packetDefinitions, channelMatcher);
    }

    /// <summary>
    /// Adds a channel to the session group.
    /// </summary>
    /// <param name="channel">The channel to add.</param>
    /// <returns>True if the channel was added; otherwise, false.</returns>
    public bool Add(IChannel channel)
    {
        return _channelGroup.Add(channel);
    }

    /// <summary>
    /// Removes a channel from the session group.
    /// </summary>
    /// <param name="channel">The channel to remove.</param>
    /// <returns>True if the channel was removed; otherwise, false.</returns>
    public bool Remove(IChannel channel)
    {
        return _channelGroup.Remove(channel);
    }

    /// <summary>
    /// Gets the number of channels in the session group.
    /// </summary>
    public int Count => _channelGroup.Count;
}