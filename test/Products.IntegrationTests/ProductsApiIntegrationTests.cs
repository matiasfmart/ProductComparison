using System.Net;

namespace Products.IntegrationTests;

public class ProductsApiIntegrationTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public ProductsApiIntegrationTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public async Task GetByIds_ReturnsProducts_WhenIdsExist()
    {
        var client = _factory.CreateClient();

        var ids = new[] { "kbd-redragon-k552", "hx-cloud2" };
        var url = $"/api/v1/products?ids={string.Join("&ids=", ids)}";

        var resp = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.NotNull(resp.Headers.ETag);

        var json = await resp.Content.ReadAsStringAsync();
        Assert.Contains("Redragon", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("HyperX", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetByIds_Returns404_WhenIdsNotFound()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/api/v1/products?ids=__notfound__");

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);

        var body = await resp.Content.ReadAsStringAsync();
        Assert.Contains("problem", resp.Content.Headers.ContentType!.MediaType, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("status", body);
    }

    [Fact]
    public async Task GetByIds_Returns400_WhenIdsMissing()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/api/v1/products");

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);

        var body = await resp.Content.ReadAsStringAsync();
        Assert.Contains("problem", resp.Content.Headers.ContentType!.MediaType, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("status", body);
    }

    [Fact]
    public async Task GetByIds_Returns304_WhenIfNoneMatchMatches()
    {
        var client = _factory.CreateClient();

        var first = await client.GetAsync("/api/v1/products?ids=kbd-redragon-k552");
        first.EnsureSuccessStatusCode();
        var etag = first.Headers.ETag?.Tag;
        Assert.False(string.IsNullOrWhiteSpace(etag));

        var req = new HttpRequestMessage(HttpMethod.Get, "/api/v1/products?ids=kbd-redragon-k552");
        req.Headers.TryAddWithoutValidation("If-None-Match", etag);

        var second = await client.SendAsync(req);
        Assert.Equal(HttpStatusCode.NotModified, second.StatusCode);
        Assert.Equal(0, (await second.Content.ReadAsByteArrayAsync()).Length);
    }
}