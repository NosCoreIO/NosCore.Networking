﻿//  __  _  __    __   ___ __  ___ ___
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
    public interface IEncoder : IChannelHandler
    {
        byte[] Encode(string clientSessionId, IEnumerable<IPacket> packets);
    }
}