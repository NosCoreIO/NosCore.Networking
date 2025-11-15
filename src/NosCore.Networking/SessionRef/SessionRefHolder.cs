//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Collections.Concurrent;

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
        _sessionCounter += 2;
        return _sessionCounter;
    }
}