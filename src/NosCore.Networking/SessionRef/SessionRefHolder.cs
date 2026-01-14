//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Collections.Concurrent;
using System.Threading;

namespace NosCore.Networking.SessionRef;

/// <summary>
/// Manages session references and generates unique session identifiers in a thread-safe manner.
/// </summary>
public class SessionRefHolder : ConcurrentDictionary<string, RegionTypeMapping>, ISessionRefHolder
{
    private int _sessionCounter;

    /// <summary>
    /// Generates a new unique session identifier.
    /// </summary>
    /// <returns>A new session identifier incremented by 2.</returns>
    public int GenerateSessionId()
    {
        return Interlocked.Add(ref _sessionCounter, 2);
    }

    /// <summary>
    /// Attempts to remove and return the value with the specified key.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <param name="value">The removed value, if found.</param>
    /// <returns>true if the element was removed successfully; otherwise, false.</returns>
    public new bool TryRemove(string key, out RegionTypeMapping? value)
    {
        var result = base.TryRemove(key, out var baseValue);
        value = baseValue;
        return result;
    }
}