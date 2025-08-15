using System.Text.Json;
using BuildingBlocks.Configuration;
using Features.Products.Domain;
using Features.Products.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Products.UnitTests;

public class JsonProductRepositoryTests
{
    private static string CreateTempJson(params Product[] items)
    {
        var tmp = Path.Combine(Path.GetTempPath(), $"products_{Guid.NewGuid():N}.json");
        using var fs = File.Create(tmp);
        JsonSerializer.Serialize(fs, items);
        return tmp;
    }

    private static IOptions<DataOptions> Opts(string filePath) =>
        Options.Create(new DataOptions { FilePath = filePath });

    [Fact]
    public async Task GetByIdsAsync_ReturnsProducts_AndMissing_AndQuotedEtag()
    {
        var path = CreateTempJson(
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
            },
            new Product
            {
                Id = "2",
                Name = "B",
                ImageUrl = "img",
                Description = "desc",
                Price = 2m,
                Currency = "USD",
                Rating = 5m,
                Specifications = new Dictionary<string, string>()
            }
        );

        var repo = new JsonProductRepository(Opts(path), NullLogger<JsonProductRepository>.Instance);

        // Act
        var (found, etag, missing) = await repo.GetByIdsAsync(new[] { "1", "3" }, default);

        // Assert
        Assert.Single(found);
        Assert.Equal("1", found[0].Id);
        Assert.Single(missing);
        Assert.Equal("3", missing[0]);
        Assert.StartsWith("\"", etag);
        Assert.EndsWith("\"", etag);
    }
}
