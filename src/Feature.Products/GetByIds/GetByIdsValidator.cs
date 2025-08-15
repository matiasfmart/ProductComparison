namespace Features.Products.GetByIds;

public sealed class GetByIdsValidator
{
    public (bool ok, string? error) Validate(GetByIdsRequest req)
    {
        if (req.Ids is null || req.Ids.Length == 0) return (false, "Query 'ids' is required");
        if (req.Ids.Any(string.IsNullOrWhiteSpace)) return (false, "All ids must be non-empty");
        return (true, null);
    }
}