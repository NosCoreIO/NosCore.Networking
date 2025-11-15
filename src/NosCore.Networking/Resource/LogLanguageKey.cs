//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

namespace NosCore.Networking.Resource
{
    /// <summary>
    /// Defines localization keys for networking-related log messages.
    /// </summary>
    public enum LogLanguageKey
    {
        /// <summary>
        /// Log message when a client connects.
        /// </summary>
        CLIENT_CONNECTED,

        /// <summary>
        /// Log message indicating the port the server is listening on.
        /// </summary>
        LISTENING_PORT,

        /// <summary>
        /// Log message for encoding errors.
        /// </summary>
        ENCODE_ERROR,

        /// <summary>
        /// Log message for decoding errors.
        /// </summary>
        ERROR_DECODING,

        /// <summary>
        /// Log message for session ID errors.
        /// </summary>
        ERROR_SESSIONID,

        /// <summary>
        /// Log message when a client is forcefully disconnected.
        /// </summary>
        FORCED_DISCONNECTION,

        /// <summary>
        /// Log message when a connection is blocked by the spam filter.
        /// </summary>
        BLOCKED_BY_SPAM_FILTER,

        /// <summary>
        /// Log message for corrupted packet detection.
        /// </summary>
        CORRUPTED_PACKET,

        /// <summary>
        /// Log message when attempting to send an invalid packet.
        /// </summary>
        SENDING_INVALID_PACKET
    }
}