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

public class SessionGroup : ISessionGroup
{
    private readonly DefaultChannelGroup _channelGroup;

    public SessionGroup()
    {
        ExecutionEnvironment.TryGetCurrentExecutor(out var executor);
        _channelGroup = new DefaultChannelGroup(executor);
    }

    public Task Broadcast(IPacket[] packetDefinitions)
    {
        return _channelGroup.WriteAndFlushAsync(packetDefinitions);
    }

    public Task Broadcast(IPacket[] packetDefinitions, IChannelMatcher channelMatcher)
    {
        return _channelGroup.WriteAndFlushAsync(packetDefinitions, channelMatcher);
    }

    public bool Add(IChannel channel)
    {
        return _channelGroup.Add(channel);
    }

    public bool Remove(IChannel channel)
    {
        return _channelGroup.Remove(channel);
    }

    public int Count => _channelGroup.Count;
}