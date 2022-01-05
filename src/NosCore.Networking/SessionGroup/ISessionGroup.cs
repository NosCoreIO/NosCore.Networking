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

public interface ISessionGroup
{
    Task Broadcast(IPacket[] packetDefinitions);
    Task Broadcast(IPacket[] packetDefinitions, IChannelMatcher channelMatcher);

    bool Add(IChannel channel);

    bool Remove(IChannel channel);

    int Count { get; }
}