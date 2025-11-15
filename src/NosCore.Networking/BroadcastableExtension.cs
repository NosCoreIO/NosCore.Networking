//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetty.Transport.Channels.Groups;
using NosCore.Networking.SessionGroup;
using NosCore.Packets.Interfaces;

namespace NosCore.Networking
{
    /// <summary>
    /// Provides extension methods for broadcasting packets to <see cref="IBroadcastable"/> instances.
    /// </summary>
    public static class IBroadcastableExtension
    {
        /// <summary>
        /// Sends a single packet to all sessions in the broadcastable group.
        /// </summary>
        /// <param name="channelGroup">The broadcastable group to send the packet to.</param>
        /// <param name="packet">The packet to send.</param>
        /// <returns>A task representing the asynchronous send operation.</returns>
        public static Task SendPacketAsync(this IBroadcastable channelGroup, IPacket packet)
        {
            return channelGroup.SendPacketsAsync(new[] { packet });
        }

        /// <summary>
        /// Sends a single packet to matching sessions in the broadcastable group.
        /// </summary>
        /// <param name="channelGroup">The broadcastable group to send the packet to.</param>
        /// <param name="packet">The packet to send.</param>
        /// <param name="matcher">The channel matcher to filter recipients.</param>
        /// <returns>A task representing the asynchronous send operation.</returns>
        public static Task SendPacketAsync(this IBroadcastable channelGroup, IPacket packet, IChannelMatcher matcher)
        {
            return channelGroup.SendPacketsAsync(new[] { packet }, matcher);
        }

        /// <summary>
        /// Sends multiple packets to matching sessions in the broadcastable group.
        /// </summary>
        /// <param name="channelGroup">The broadcastable group to send the packets to.</param>
        /// <param name="packets">The collection of packets to send.</param>
        /// <param name="matcher">The optional channel matcher to filter recipients.</param>
        /// <returns>A task representing the asynchronous send operation.</returns>
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
                    await channelGroup.Sessions.Broadcast(packetDefinitions).ConfigureAwait(false);
                }
                else
                {
                    await channelGroup.Sessions.Broadcast(packetDefinitions, matcher).ConfigureAwait(false);
                }
            }
        }


        /// <summary>
        /// Sends multiple packets to all sessions in the broadcastable group.
        /// </summary>
        /// <param name="channelGroup">The broadcastable group to send the packets to.</param>
        /// <param name="packets">The collection of packets to send.</param>
        /// <returns>A task representing the asynchronous send operation.</returns>
        public static Task SendPacketsAsync(this IBroadcastable channelGroup, IEnumerable<IPacket> packets)
        {
            return channelGroup.SendPacketsAsync(packets, null);
        }
    }
}