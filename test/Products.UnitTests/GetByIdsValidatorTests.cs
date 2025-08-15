using Features.Products.GetByIds;
using Xunit;

namespace Products.UnitTests;

public class GetByIdsValidatorTests
{
    [Fact]
    public void Validate_ReturnsFalse_WhenIdsIsNull()
    {
        var v = new GetByIdsValidator();
        var req = new GetByIdsRequest(null!);

        var (ok, error) = v.Validate(req);

        Assert.False(ok);
        Assert.Equal("Query 'ids' is required", error);
    }

    [Fact]
    public void Validate_ReturnsFalse_WhenIdsIsEmpty()
    {
        var v = new GetByIdsValidator();
        var req = new GetByIdsRequest(Array.Empty<string>());

        var (ok, error) = v.Validate(req);

        Assert.False(ok);
        Assert.Equal("Query 'ids' is required", error);
    }

    [Fact]
    public void Validate_ReturnsFalse_WhenAnyIdIsWhitespace()
    {
        var v = new GetByIdsValidator();
        var req = new GetByIdsRequest(new[] { " ", "\t", "valid" });

        var (ok, error) = v.Validate(req);

        Assert.False(ok);
        Assert.Equal("All ids must be non-empty", error);
    }

    [Fact]
    public void Validate_ReturnsTrue_WhenAllIdsValid()
    {
        var v = new GetByIdsValidator();
        var req = new GetByIdsRequest(new[] { "a", "b" });

        var (ok, error) = v.Validate(req);

        Assert.True(ok);
        Assert.Null(error);
    }
}
