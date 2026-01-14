//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

namespace NosCore.Networking.SessionGroup.ChannelMatcher
{
    /// <summary>
    /// A session matcher that matches all sessions except a specific one.
    /// </summary>
    public class EveryoneBut : ISessionMatcher
    {
        private readonly string _id;

        /// <summary>
        /// Initializes a new instance of the <see cref="EveryoneBut"/> class.
        /// </summary>
        /// <param name="id">The session identifier to exclude from matching.</param>
        public EveryoneBut(string id)
        {
            _id = id;
        }

        /// <summary>
        /// Determines whether the specified session matches (is not the excluded session).
        /// </summary>
        /// <param name="sessionId">The session to test.</param>
        /// <returns>True if the session is not the excluded session; otherwise, false.</returns>
        public bool Matches(string sessionId)
        {
            return sessionId != _id;
        }
    }
}
