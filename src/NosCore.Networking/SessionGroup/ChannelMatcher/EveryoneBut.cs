﻿//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Groups;

namespace NosCore.Networking.SessionGroup.ChannelMatcher
{
    public class EveryoneBut : IChannelMatcher
    {
        private readonly IChannelId _id;

        public EveryoneBut(IChannelId id)
        {
            _id = id;
        }

        public bool Matches(IChannel channel)
        {
            return channel.Id != _id;
        }
    }
}