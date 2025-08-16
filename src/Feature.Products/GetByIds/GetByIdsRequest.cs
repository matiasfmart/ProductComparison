namespace Features.Products.GetByIds;

/// <summary>
/// Represents a request to retrieve products by their IDs.
/// </summary>
/// <param name="Ids">The array of product IDs to retrieve.</param>
public sealed record GetByIdsRequest(string[] Ids);