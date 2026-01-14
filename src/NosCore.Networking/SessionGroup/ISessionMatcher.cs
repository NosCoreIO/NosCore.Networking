//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

namespace NosCore.Networking.SessionGroup
{
    /// <summary>
    /// Defines a matcher for filtering sessions by their identifier.
    /// </summary>
    public interface ISessionMatcher
    {
        /// <summary>
        /// Determines whether the specified session matches the filter criteria.
        /// </summary>
        /// <param name="sessionId">The session identifier to test.</param>
        /// <returns>True if the session matches; otherwise, false.</returns>
        bool Matches(string sessionId);
    }
}
