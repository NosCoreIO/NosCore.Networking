//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetty.Transport.Channels.Groups;
using NosCore.Packets.Interfaces;

namespace NosCore.Networking
{
    public static class IBroadcastableExtension
    {
        public static Task SendPacketAsync(this IBroadcastable channelGroup, IPacket packet)
        {
            return channelGroup.SendPacketsAsync(new[] { packet });
        }

        public static Task SendPacketAsync(this IBroadcastable channelGroup, IPacket packet, IChannelMatcher matcher)
        {
            return channelGroup.SendPacketsAsync(new[] { packet }, matcher);
        }


        public static async Task SendPacketsAsync(this IBroadcastable channelGroup, IEnumerable<IPacket> packets,
            IChannelMatcher? matcher)
        {
            var packetDefinitions = (packets as IPacket[] ?? packets).Where(c => c != null).ToArray();
            if (packetDefinitions.Any())
            {
                Parallel.ForEach(packets, packet => channelGroup.LastPackets.Enqueue(packet));
                Parallel.For(0, channelGroup.LastPackets.Count - channelGroup.MaxPacketsBuffer, (_, __) => channelGroup.LastPackets.TryDequeue(out var ___));
                if (channelGroup.Sessions == null!)
                {
                    return;
                }

                if (matcher == null)
                {
                    await channelGroup.Sessions.WriteAndFlushAsync(packetDefinitions).ConfigureAwait(false);
                }
                else
                {
                    await channelGroup.Sessions.WriteAndFlushAsync(packetDefinitions, matcher).ConfigureAwait(false);
                }
            }
        }


        public static Task SendPacketsAsync(this IBroadcastable channelGroup, IEnumerable<IPacket> packets)
        {
            return channelGroup.SendPacketsAsync(packets, null);
        }
    }
}