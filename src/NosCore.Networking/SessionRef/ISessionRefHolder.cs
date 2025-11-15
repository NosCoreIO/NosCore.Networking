//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Collections.Generic;

namespace NosCore.Networking.SessionRef
{
    /// <summary>
    /// Defines a holder for managing session references and generating session identifiers.
    /// </summary>
    public interface ISessionRefHolder : IDictionary<string, RegionTypeMapping>
    {
        /// <summary>
        /// Generates a new unique session identifier.
        /// </summary>
        /// <returns>A new session identifier.</returns>
        public int GenerateSessionId();
    }
}
