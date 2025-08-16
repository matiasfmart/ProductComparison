namespace Features.Products.Domain;

/// <summary>
/// Represents a product with details such as name, price, rating, and specifications.
/// </summary>
public sealed class Product
{
    /// <summary>
    /// Gets the unique identifier of the product.
    /// </summary>
    public string Id { get; init; } = default!;

    /// <summary>
    /// Gets the name of the product.
    /// </summary>
    public string Name { get; init; } = default!;

    /// <summary>
    /// Gets the URL of the product image.
    /// </summary>
    public string ImageUrl { get; init; } = default!;

    /// <summary>
    /// Gets the description of the product.
    /// </summary>
    public string Description { get; init; } = default!;

    /// <summary>
    /// Gets the price of the product.
    /// </summary>
    public decimal Price { get; init; }

    /// <summary>
    /// Gets the currency of the product price (e.g., "USD").
    /// </summary>
    public string Currency { get; init; } = "USD";

    /// <summary>
    /// Gets the rating of the product (0 to 5).
    /// </summary>
    public decimal Rating { get; init; } // 0..5

    /// <summary>
    /// Gets the specifications of the product as key-value pairs.
    /// </summary>
    public IReadOnlyDictionary<string, string> Specifications { get; init; }
        = new Dictionary<string, string>();
}
