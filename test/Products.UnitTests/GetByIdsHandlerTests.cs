using Features.Products.Domain;
using Features.Products.GetByIds;
using Features.Products.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Products.UnitTests;

public class GetByIdsHandlerTests
{
    private sealed class FakeRepo : IProductRepository
    {
        private readonly Dictionary<string, Product> _byId;
        private readonly string _etag;

        public FakeRepo(IEnumerable<Product> items, string etag = "\"test\"")
        {
            _byId = items.ToDictionary(p => p.Id, StringComparer.OrdinalIgnoreCase);
            _etag = etag;
        }

        public Task<(IReadOnlyList<Product>, string, IReadOnlyList<string>)> GetByIdsAsync(IEnumerable<string> ids, CancellationToken ct)
        {
            var found = new List<Product>();
            var missing = new List<string>();
            foreach (var id in ids)
            {
                if (_byId.TryGetValue(id, out var p)) found.Add(p);
                else missing.Add(id);
            }
            return Task.FromResult(((IReadOnlyList<Product>)found, _etag, (IReadOnlyList<string>)missing));
        }
    }

    [Fact]
    public async Task HandleAsync_Returns400_WhenValidationFails()
    {
        var handler = new GetByIdsHandler(new FakeRepo(Array.Empty<Product>()), new GetByIdsValidator(), NullLogger<GetByIdsHandler>.Instance);

        var (payload, status, etag, title, detail) =
            await handler.HandleAsync(new GetByIdsRequest(new[] { "   " }), default);

        Assert.Equal(400, status);
        Assert.Null(payload);
        Assert.Null(etag);
        Assert.Equal("Validation Failed", title);
        Assert.False(string.IsNullOrWhiteSpace(detail));
    }

    [Fact]
    public async Task HandleAsync_Returns404_WhenNoProductsFound()
    {
        var handler = new GetByIdsHandler(new FakeRepo(Array.Empty<Product>()), new GetByIdsValidator(), NullLogger<GetByIdsHandler>.Instance);

        var (payload, status, etag, title, detail) =
            await handler.HandleAsync(new GetByIdsRequest(new[] { "does-not-exist" }), default);

        Assert.Equal(404, status);
        Assert.Null(payload);
        Assert.Null(etag);
    }

    [Fact]
    public async Task HandleAsync_Returns200_WithItems_AndEtag()
    {
        var products = new[]
        {
            new Product
            {
                Id = "1",
                Name = "A",
                ImageUrl = "img",
                Description = "desc",
                Price = 1m,
                Currency = "USD",
                Rating = 4m,
                Specifications = new Dictionary<string, string>()
            }
        };
        var handler = new GetByIdsHandler(new FakeRepo(products, "\"etag-xyz\""), new GetByIdsValidator(), NullLogger<GetByIdsHandler>.Instance);

        var (payload, status, etag, title, detail) =
            await handler.HandleAsync(new GetByIdsRequest(new[] { "1" }), default);

        Assert.Equal(200, status);
        Assert.NotNull(payload);
        Assert.Single(payload!.Products);
        Assert.Equal("\"etag-xyz\"", etag);
        Assert.Null(title);
        Assert.Null(detail);
    }
}
