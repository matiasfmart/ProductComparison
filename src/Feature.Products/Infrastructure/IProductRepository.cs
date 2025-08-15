using Features.Products.Domain;

namespace Features.Products.Infrastructure;

public interface IProductRepository
{
    Task<(IReadOnlyList<Product> Items, string ETag, IReadOnlyList<string> Missing)>
        GetByIdsAsync(IEnumerable<string> ids, CancellationToken ct);
}
