//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace NosCore.Networking;

/// <summary>
/// Global registry for network clients, allowing lookup by session key.
/// </summary>
public static class NetworkClientRegistry
{
    private static readonly ConcurrentDictionary<string, INetworkClient> _clients = new();

    /// <summary>
    /// Registers a client in the registry.
    /// </summary>
    /// <param name="client">The client to register.</param>
    public static void Register(INetworkClient client)
    {
        _clients.TryAdd(client.SessionKey, client);
    }

    /// <summary>
    /// Unregisters a client from the registry.
    /// </summary>
    /// <param name="client">The client to unregister.</param>
    public static void Unregister(INetworkClient client)
    {
        _clients.TryRemove(client.SessionKey, out _);
    }

    /// <summary>
    /// Unregisters a client from the registry by session key.
    /// </summary>
    /// <param name="sessionKey">The session key of the client to unregister.</param>
    public static void Unregister(string sessionKey)
    {
        _clients.TryRemove(sessionKey, out _);
    }

    /// <summary>
    /// Tries to get a client from the registry by session key.
    /// </summary>
    /// <param name="sessionKey">The session key to look up.</param>
    /// <param name="client">The client if found; otherwise, null.</param>
    /// <returns>True if the client was found; otherwise, false.</returns>
    public static bool TryGetClient(string sessionKey, [NotNullWhen(true)] out INetworkClient? client)
    {
        return _clients.TryGetValue(sessionKey, out client);
    }

    /// <summary>
    /// Clears all clients from the registry. Used for testing and shutdown scenarios.
    /// </summary>
    public static void Clear()
    {
        _clients.Clear();
    }
}
