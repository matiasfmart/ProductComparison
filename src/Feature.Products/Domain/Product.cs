namespace Features.Products.Domain;

public sealed class Product
{
    public string Id { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string ImageUrl { get; init; } = default!;
    public string Description { get; init; } = default!;
    public decimal Price { get; init; }
    public string Currency { get; init; } = "USD";
    public decimal Rating { get; init; } // 0..5
    public IReadOnlyDictionary<string, string> Specifications { get; init; }
        = new Dictionary<string, string>();
}
