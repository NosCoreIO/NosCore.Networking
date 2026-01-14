//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NosCore.Networking.Resource;
using NosCore.Packets.Interfaces;
using NosCore.Shared.I18N;

namespace NosCore.Networking.SessionGroup;

/// <summary>
/// Manages a group of network sessions and provides broadcasting capabilities.
/// </summary>
public class SessionGroup : ISessionGroup
{
    private readonly ConcurrentDictionary<string, INetworkClient> _sessions = new();
    private readonly ILogger<SessionGroup> _logger;
    private readonly ILogLanguageLocalizer<LogLanguageKey> _logLanguage;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionGroup"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="logLanguage">The log language localizer.</param>
    public SessionGroup(ILogger<SessionGroup> logger, ILogLanguageLocalizer<LogLanguageKey> logLanguage)
    {
        _logger = logger;
        _logLanguage = logLanguage;
    }

    /// <summary>
    /// Broadcasts packets to all sessions in the group.
    /// </summary>
    /// <param name="packetDefinitions">The array of packets to broadcast.</param>
    /// <returns>A task representing the asynchronous broadcast operation.</returns>
    public async Task Broadcast(IPacket[] packetDefinitions)
    {
        var tasks = _sessions.Select(async kvp =>
        {
            try
            {
                await kvp.Value.SendPacketsAsync(packetDefinitions);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, _logLanguage[LogLanguageKey.BROADCAST_ERROR], kvp.Key);
            }
        });
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Broadcasts packets to matching sessions in the group.
    /// </summary>
    /// <param name="packetDefinitions">The array of packets to broadcast.</param>
    /// <param name="sessionMatcher">The matcher to filter which sessions receive the packets.</param>
    /// <returns>A task representing the asynchronous broadcast operation.</returns>
    public async Task Broadcast(IPacket[] packetDefinitions, ISessionMatcher sessionMatcher)
    {
        var matchedSessions = _sessions.Where(kvp => sessionMatcher.Matches(kvp.Key));
        var tasks = matchedSessions.Select(async kvp =>
        {
            try
            {
                await kvp.Value.SendPacketsAsync(packetDefinitions);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, _logLanguage[LogLanguageKey.BROADCAST_ERROR], kvp.Key);
            }
        });
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Adds a client to the session group.
    /// </summary>
    /// <param name="client">The client to add.</param>
    /// <returns>True if the client was added; otherwise, false.</returns>
    public bool Add(INetworkClient client)
    {
        return _sessions.TryAdd(client.SessionKey, client);
    }

    /// <summary>
    /// Adds a client to the session group by channel.
    /// </summary>
    /// <param name="channel">The channel of the client to add.</param>
    /// <returns>True if the client was added; otherwise, false.</returns>
    public bool Add(IChannel channel)
    {
        return Add(channel.Id);
    }

    /// <summary>
    /// Adds a client to the session group by session key.
    /// </summary>
    /// <param name="sessionKey">The session key of the client to add.</param>
    /// <returns>True if the client was added; otherwise, false.</returns>
    public bool Add(string sessionKey)
    {
        if (NetworkClientRegistry.TryGetClient(sessionKey, out var client))
        {
            return _sessions.TryAdd(sessionKey, client!);
        }
        return false;
    }

    /// <summary>
    /// Removes a client from the session group.
    /// </summary>
    /// <param name="client">The client to remove.</param>
    /// <returns>True if the client was removed; otherwise, false.</returns>
    public bool Remove(INetworkClient client)
    {
        return _sessions.TryRemove(client.SessionKey, out _);
    }

    /// <summary>
    /// Removes a client from the session group by channel.
    /// </summary>
    /// <param name="channel">The channel of the client to remove.</param>
    /// <returns>True if the client was removed; otherwise, false.</returns>
    public bool Remove(IChannel channel)
    {
        return Remove(channel.Id);
    }

    /// <summary>
    /// Removes a client from the session group by session key.
    /// </summary>
    /// <param name="sessionKey">The session key of the client to remove.</param>
    /// <returns>True if the client was removed; otherwise, false.</returns>
    public bool Remove(string sessionKey)
    {
        return _sessions.TryRemove(sessionKey, out _);
    }

    /// <summary>
    /// Gets the number of clients in the session group.
    /// </summary>
    public int Count => _sessions.Count;
}
