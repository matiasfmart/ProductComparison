namespace Features.Products.GetByIds;

/// <summary>
/// Validates the <see cref="GetByIdsRequest"/> for retrieving products by IDs.
/// </summary>
public sealed class GetByIdsValidator
{
    /// <summary>
    /// Validates the specified <see cref="GetByIdsRequest"/>.
    /// </summary>
    /// <param name="req">The request to validate.</param>
    /// <returns>
    /// A tuple indicating whether the request is valid and an error message if not.
    /// </returns>
    public (bool ok, string? error) Validate(GetByIdsRequest req)
    {
        if (req.Ids is null || req.Ids.Length == 0) return (false, "Query 'ids' is required");
        if (req.Ids.Any(string.IsNullOrWhiteSpace)) return (false, "All ids must be non-empty");
        return (true, null);
    }
}