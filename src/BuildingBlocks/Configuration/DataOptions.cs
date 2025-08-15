namespace BuildingBlocks.Configuration;

public sealed class DataOptions
{
    public string? FilePath { get; set; }
    public string Format { get; set; } = "json";
}
