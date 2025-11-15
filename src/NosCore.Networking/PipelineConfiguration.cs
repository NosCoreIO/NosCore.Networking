//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

namespace NosCore.Networking;

/// <summary>
/// Provides configuration settings for the network pipeline.
/// </summary>
public class PipelineConfiguration : IPipelineConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether to use frame delimiters in the pipeline.
    /// </summary>
    public bool UseDelimiter { get; set; }
}