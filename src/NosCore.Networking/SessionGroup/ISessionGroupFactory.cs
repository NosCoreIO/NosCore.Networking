//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

namespace NosCore.Networking.SessionGroup;

/// <summary>
/// Factory for creating session groups with proper dependency injection.
/// </summary>
public interface ISessionGroupFactory
{
    /// <summary>
    /// Creates a new session group instance.
    /// </summary>
    /// <returns>A new session group.</returns>
    ISessionGroup Create();
}
