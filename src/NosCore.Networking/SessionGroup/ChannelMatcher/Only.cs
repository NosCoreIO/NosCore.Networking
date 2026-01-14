//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

namespace NosCore.Networking.SessionGroup.ChannelMatcher
{
    /// <summary>
    /// A session matcher that matches only a specific session by its identifier.
    /// </summary>
    public class Only : ISessionMatcher
    {
        private readonly string _id;

        /// <summary>
        /// Initializes a new instance of the <see cref="Only"/> class.
        /// </summary>
        /// <param name="id">The session identifier to match.</param>
        public Only(string id)
        {
            _id = id;
        }

        /// <summary>
        /// Determines whether the specified session matches the target session.
        /// </summary>
        /// <param name="sessionId">The session to test.</param>
        /// <returns>True if the session matches; otherwise, false.</returns>
        public bool Matches(string sessionId)
        {
            return sessionId == _id;
        }
    }
}
