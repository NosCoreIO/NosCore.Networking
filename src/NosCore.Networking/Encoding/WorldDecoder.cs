//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using NosCore.Networking.Extensions;
using NosCore.Networking.Resource;
using NosCore.Networking.SessionRef;
using NosCore.Packets;
using NosCore.Packets.Interfaces;
using NosCore.Shared.Enumerations;
using NosCore.Shared.I18N;

namespace NosCore.Networking.Encoding
{
    public class WorldDecoder : MessageToMessageDecoder<IByteBuffer>
    {
        private readonly IDeserializer _deserializer;
        private readonly ILogger<WorldDecoder> _logger;
        private RegionType _region;
        private int _sessionId;
        private readonly ISessionRefHolder _sessionRefHolder;
        private readonly ILogLanguageLocalizer<LogLanguageKey> _logLanguage;

        public WorldDecoder(IDeserializer deserializer, ILogger<WorldDecoder> logger, ISessionRefHolder sessionRefHolder, ILogLanguageLocalizer<LogLanguageKey> logLanguage)
        {
            _deserializer = deserializer;
            _logger = logger;
            _sessionRefHolder = sessionRefHolder;
            _logLanguage = logLanguage;
        }

        private string DecryptPrivate(string str)
        {
            var receiveData = new List<byte>();
            char[] table = { ' ', '-', '.', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'n' };
            int count;
            for (count = 0; count < str.Length; count++)
            {
                if (str[count] <= 0x7A)
                {
                    int len = str[count];

                    for (var i = 0; i < len; i++)
                    {
                        count++;

                        try
                        {
                            receiveData.Add(unchecked((byte)(str[count] ^ 0xFF)));
                        }
#pragma warning disable CA1031 // Do not catch general exception types
                        catch
#pragma warning restore CA1031 // Do not catch general exception types
                        {
                            receiveData.Add(255);
                        }
                    }
                }
                else
                {
                    int len = str[count];
                    len &= 0x7F;

                    for (var i = 0; i < len; i++)
                    {
                        count++;
                        var highbyte = str.Length > count ? str[count] : 0;

                        highbyte &= 0xF0;
                        highbyte >>= 0x4;

                        var lowbyte = str.Length > count ? str[count] : 0;

                        lowbyte &= 0x0F;

                        if ((highbyte != 0x0) && (highbyte != 0xF))
                        {
                            receiveData.Add(unchecked((byte)table[highbyte - 1]));
                            i++;
                        }

                        if ((lowbyte != 0x0) && (lowbyte != 0xF))
                        {
                            receiveData.Add(unchecked((byte)table[lowbyte - 1]));
                        }
                    }
                }
            }

            return _region.GetEncoding()!.GetString(receiveData.ToArray());
        }

        private static string DecryptCustomParameter(byte[] str, out byte[] endOfPacket)
        {
            endOfPacket = Array.Empty<byte>();
            try
            {
                var encryptedStringBuilder = new StringBuilder();
                for (var i = 1; i < str.Length; i++)
                {
                    if (Convert.ToChar(str[i]) == 0xE)
                    {
                        endOfPacket = str.Skip(i + 1).ToArray();
                        return encryptedStringBuilder.ToString();
                    }

                    var firstbyte = Convert.ToInt32(str[i] - 0xF);
                    var secondbyte = firstbyte;
                    secondbyte &= 0xF0;
                    firstbyte = Convert.ToInt32(firstbyte - secondbyte);
                    secondbyte >>= 0x4;

                    switch (secondbyte)
                    {
                        case 0:
                        case 1:
                            encryptedStringBuilder.Append(' ');
                            break;

                        case 2:
                            encryptedStringBuilder.Append('-');
                            break;

                        case 3:
                            encryptedStringBuilder.Append('.');
                            break;

                        default:
                            secondbyte += 0x2C;
                            encryptedStringBuilder.Append(Convert.ToChar(secondbyte));
                            break;
                    }

                    switch (firstbyte)
                    {
                        case 0:

                        case 1:
                            encryptedStringBuilder.Append(' ');
                            break;

                        case 2:
                            encryptedStringBuilder.Append('-');
                            break;

                        case 3:
                            encryptedStringBuilder.Append('.');
                            break;

                        default:
                            firstbyte += 0x2C;
                            encryptedStringBuilder.Append(Convert.ToChar(firstbyte));
                            break;
                    }
                }

                return encryptedStringBuilder.ToString();
            }
            catch (OverflowException)
            {
                return string.Empty;
            }
        }

        protected override void Decode(IChannelHandlerContext context, IByteBuffer message, List<object> output)
        {
            var continueToDecode = true;
            var temp = new List<IPacket>();
            var encryptedString = "";
            var mapper = _sessionRefHolder[context.Channel.Id.AsLongText()];
            _region = mapper.RegionType;
            _sessionId = mapper.SessionId;
            var str = ((Span<byte>)message.Array).Slice(message.ArrayOffset, message.ReadableBytes).ToArray();
            if (_sessionId == 0)
            {
                if (_deserializer.Deserialize(DecryptCustomParameter(str, out var endofPacket)) is not UnresolvedPacket pack)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                if (!int.TryParse(pack.Header, out _sessionId))
                {
                    _logger.LogError(_logLanguage[LogLanguageKey.ERROR_SESSIONID], mapper.SessionId);
                    return;
                }

                _sessionRefHolder[context.Channel.Id.AsLongText()].SessionId = _sessionId;
                _logger.LogInformation(_logLanguage[LogLanguageKey.CLIENT_CONNECTED], mapper.SessionId);
                temp.Add(pack);
                if (endofPacket.Length == 0)
                {
                    continueToDecode = false;
                }

                str = endofPacket;
            }

            if (continueToDecode)
            {
                var sessionKey = _sessionId & 0xFF;
                var sessionNumber = unchecked((byte)(_sessionId >> 6));
                sessionNumber &= 0xFF;
                sessionNumber &= unchecked((byte)0x80000003);

                switch (sessionNumber)
                {
                    case 0:
                        encryptedString =
                            (from character in str
                             let firstbyte = unchecked((byte)(sessionKey + 0x40))
                             select unchecked((byte)(character - firstbyte))).Aggregate(encryptedString,
                                (current, highbyte) => current + (char)highbyte);
                        break;

                    case 1:
                        encryptedString =
                            (from character in str
                             let firstbyte = unchecked((byte)(sessionKey + 0x40))
                             select unchecked((byte)(character + firstbyte))).Aggregate(encryptedString,
                                (current, highbyte) => current + (char)highbyte);
                        break;

                    case 2:
                        encryptedString =
                            (from character in str
                             let firstbyte = unchecked((byte)(sessionKey + 0x40))
                             select unchecked((byte)((character - firstbyte) ^ 0xC3))).Aggregate(encryptedString,
                                (current, highbyte) => current + (char)highbyte);
                        break;

                    case 3:
                        encryptedString =
                            (from character in str
                             let firstbyte = unchecked((byte)(sessionKey + 0x40))
                             select unchecked((byte)((character + firstbyte) ^ 0xC3))).Aggregate(encryptedString,
                                (current, highbyte) => current + (char)highbyte);
                        break;

                    default:
                        encryptedString += (char)0xF;
                        break;
                }

                temp.AddRange(encryptedString.Split((char)0xFF, StringSplitOptions.RemoveEmptyEntries).Select(p =>
               {
                   try
                   {
                       var decrypt = DecryptPrivate(p);
                       var packet = _deserializer.Deserialize(decrypt);
                       if (!packet.IsValid)
                       {
                           _logger.LogError(_logLanguage[LogLanguageKey.CORRUPTED_PACKET],
                               packet.Header, decrypt);
                       }

                       return packet;
                   }
#pragma warning disable CA1031 // Do not catch general exception types
                   catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                   {
                       _logger.LogError(_logLanguage[LogLanguageKey.ERROR_DECODING],
                           ex.Data["Packet"]);
                       ushort? keepalive = null;
                       if (ushort.TryParse(ex.Data["Packet"]?.ToString()?.Split(" ")[0], out var kpalive))
                       {
                           keepalive = kpalive;
                       }
                       return new UnresolvedPacket
                       { KeepAliveId = keepalive, Header = "0" };
                   }
               }));
            }

            if (temp.Count > 0)
            {
                output.Add(temp);
            }
        }
    }
}