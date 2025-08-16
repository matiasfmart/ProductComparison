namespace BuildingBlocks.Configuration;

/// <summary>
/// Represents configuration options for data storage.
/// </summary>
public sealed class DataOptions
{
    /// <summary>
    /// Gets or sets the file path where the data is stored.
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Gets or sets the format of the data file (e.g., "json").
    /// </summary>
    public string Format { get; set; } = "json";
}
