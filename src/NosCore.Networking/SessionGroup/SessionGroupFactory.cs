//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using Microsoft.Extensions.Logging;
using NosCore.Networking.Resource;
using NosCore.Shared.I18N;

namespace NosCore.Networking.SessionGroup;

/// <summary>
/// Factory for creating session groups with proper dependency injection.
/// </summary>
public class SessionGroupFactory : ISessionGroupFactory
{
    private readonly ILogger<SessionGroup> _logger;
    private readonly ILogLanguageLocalizer<LogLanguageKey> _logLanguage;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionGroupFactory"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="logLanguage">The log language localizer.</param>
    public SessionGroupFactory(ILogger<SessionGroup> logger, ILogLanguageLocalizer<LogLanguageKey> logLanguage)
    {
        _logger = logger;
        _logLanguage = logLanguage;
    }

    /// <summary>
    /// Creates a new session group instance.
    /// </summary>
    /// <returns>A new session group with injected dependencies.</returns>
    public ISessionGroup Create()
    {
        return new SessionGroup(_logger, _logLanguage);
    }
}
