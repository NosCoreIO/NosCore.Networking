//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using NosCore.Shared.Enumerations;

namespace NosCore.Networking
{
    /// <summary>
    /// Represents a mapping between a session identifier and its associated region type.
    /// </summary>
    public class RegionTypeMapping
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegionTypeMapping"/> class.
        /// </summary>
        /// <param name="sessionId">The unique session identifier.</param>
        /// <param name="regionType">The region type associated with the session.</param>
        public RegionTypeMapping(int sessionId, RegionType regionType)
        {
            SessionId = sessionId;
            RegionType = regionType;
        }

        /// <summary>
        /// Gets or sets the unique session identifier.
        /// </summary>
        public int SessionId { get; set; }

        /// <summary>
        /// Gets or sets the region type associated with the session.
        /// </summary>
        public RegionType RegionType { get; set; }
    }
}