//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Collections.Concurrent;

namespace NosCore.Networking.SessionRef;

public class SessionRefHolder : ConcurrentDictionary<string, RegionTypeMapping>, ISessionRefHolder
{
    private int _sessionCounter;
        
    public int GenerateSessionId()
    {
        _sessionCounter += 2;
        return _sessionCounter;
    }
}