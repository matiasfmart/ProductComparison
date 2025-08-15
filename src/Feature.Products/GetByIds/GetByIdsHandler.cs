using Features.Products.Infrastructure;
using static Features.Products.GetByIds.GetByIdsResponse;

namespace Features.Products.GetByIds;

public sealed class GetByIdsHandler
{
    private readonly IProductRepository _repo;
    private readonly GetByIdsValidator _validator;

    public GetByIdsHandler(IProductRepository repo, GetByIdsValidator validator)
        => (_repo, _validator) = (repo, validator);

    public async Task<(GetByIdsResponse? resp, int status, string? etag, string? errorTitle, string? errorDetail)>
        HandleAsync(GetByIdsRequest req, CancellationToken ct)
    {
        var (ok, error) = _validator.Validate(req);
        if (!ok) return (null, 400, null, "Validation Failed", error);

        var (items, etag, missing) = await _repo.GetByIdsAsync(req.Ids, ct);
        if (items.Count == 0)
            return (null, 404, null, "Not Found", "No products found for the given ids.");

        var resp = new GetByIdsResponse
        {
            Products = items.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                ImageUrl = p.ImageUrl,
                Description = p.Description,
                Price = p.Price,
                Currency = p.Currency,
                Rating = p.Rating,
                Specifications = p.Specifications.ToDictionary(k => k.Key, v => v.Value)
            }).ToList()
        };

        //registrar 'missing' en logs o exponerlo en extensiones si lo deseás.
        return (resp, 200, etag, null, null);
    }
}
