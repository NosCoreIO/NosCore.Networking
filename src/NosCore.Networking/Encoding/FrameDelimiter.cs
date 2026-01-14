//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

namespace NosCore.Networking.Encoding;

/// <summary>
/// Provides delimiter calculation for session-specific frame detection.
/// </summary>
public static class FrameDelimiter
{
    /// <summary>
    /// Gets the delimiter byte for a specific session.
    /// </summary>
    /// <param name="session">The session identifier.</param>
    /// <param name="isFirstPacket">Whether this is the first packet in the session.</param>
    /// <returns>The delimiter byte for the session.</returns>
    public static byte GetDelimiter(int session, bool isFirstPacket = false)
    {
        int stype = !isFirstPacket ? (session >> 6) & 3 : -1;

        var key = session & 0xFF;

        return (byte)(stype switch
        {
            0 => (0xff + key + 0x40) & 0xFF,
            1 => (0xff - key - 0x40) & 0xFF,
            2 => ((0xff ^ 0xC3) + key + 0x40) & 0xFF,
            3 => ((0xff ^ 0xC3) - key - 0x40) & 0xFF,
            _ => (0xff + 0xF) & 0xFF
        });
    }
}
