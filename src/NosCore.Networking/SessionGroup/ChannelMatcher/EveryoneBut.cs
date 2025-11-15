//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Groups;

namespace NosCore.Networking.SessionGroup.ChannelMatcher
{
    /// <summary>
    /// A channel matcher that matches all channels except a specific one.
    /// </summary>
    public class EveryoneBut : IChannelMatcher
    {
        private readonly IChannelId _id;

        /// <summary>
        /// Initializes a new instance of the <see cref="EveryoneBut"/> class.
        /// </summary>
        /// <param name="id">The channel identifier to exclude from matching.</param>
        public EveryoneBut(IChannelId id)
        {
            _id = id;
        }

        /// <summary>
        /// Determines whether the specified channel matches (is not the excluded channel).
        /// </summary>
        /// <param name="channel">The channel to test.</param>
        /// <returns>True if the channel is not the excluded channel; otherwise, false.</returns>
        public bool Matches(IChannel channel)
        {
            return channel.Id != _id;
        }
    }
}