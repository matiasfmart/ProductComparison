namespace Features.Products.GetByIds;

public sealed class GetByIdsResponse
{
    public List<ProductDto> Products { get; init; } = new();

    public sealed class ProductDto
    {
        public string Id { get; init; } = default!;
        public string Name { get; init; } = default!;
        public string ImageUrl { get; init; } = default!;
        public string Description { get; init; } = default!;
        public decimal Price { get; init; }
        public string Currency { get; init; } = "USD";
        public decimal Rating { get; init; }
        public Dictionary<string, string> Specifications { get; init; } = new();
    }
}