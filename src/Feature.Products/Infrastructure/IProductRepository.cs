using Features.Products.Domain;

namespace Features.Products.Infrastructure;

/// <summary>
/// Defines methods for retrieving products from a data source.
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Retrieves products by their IDs.
    /// </summary>
    /// <param name="ids">The collection of product IDs to retrieve.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>
    /// A tuple containing the list of found products, an ETag for caching, and a list of missing IDs.
    /// </returns>
    Task<(IReadOnlyList<Product> Items, string ETag, IReadOnlyList<string> Missing)>
        GetByIdsAsync(IEnumerable<string> ids, CancellationToken ct);
}
