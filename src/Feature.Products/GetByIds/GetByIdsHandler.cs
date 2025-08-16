using Features.Products.Infrastructure;
using Microsoft.Extensions.Logging;
using static Features.Products.GetByIds.GetByIdsResponse;

namespace Features.Products.GetByIds;

/// <summary>
/// Handles the logic for retrieving products by their IDs.
/// </summary>
public sealed class GetByIdsHandler
{
    private readonly IProductRepository _repo;
    private readonly GetByIdsValidator _validator;
    private readonly ILogger<GetByIdsHandler> _log;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetByIdsHandler"/> class.
    /// </summary>
    /// <param name="repo">The product repository.</param>
    /// <param name="validator">The request validator.</param>
    /// <param name="log">The logger instance.</param>
    public GetByIdsHandler(IProductRepository repo, GetByIdsValidator validator, ILogger<GetByIdsHandler> log)
    {
        _repo = repo;
        _validator = validator;
        _log = log;
    }

    /// <summary>
    /// Handles the request to retrieve products by their IDs.
    /// </summary>
    /// <param name="req">The request containing the product IDs.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>
    /// A tuple containing the response, HTTP status code, ETag, error title, and error detail.
    /// </returns>
    public async Task<(GetByIdsResponse? resp, int status, string? etag, string? errorTitle, string? errorDetail)>
        HandleAsync(GetByIdsRequest req, CancellationToken ct)
    {
        var (ok, error) = _validator.Validate(req);
        if (!ok)
        {
            _log.LogWarning("Validation failed: {error}", error);
            return (null, 400, null, "Validation Failed", error);
        }

        var (items, etag, missing) = await _repo.GetByIdsAsync(req.Ids, ct);

        if (missing.Count > 0)
            _log.LogInformation("Missing ids: {missingCount}", missing.Count);

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
