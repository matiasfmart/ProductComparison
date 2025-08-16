using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.Errors;

/// <summary>
/// Provides factory methods for creating <see cref="ProblemDetails"/> instances for error handling.
/// </summary>
public static class ProblemDetailsFactoryEx
{
    /// <summary>
    /// Creates a <see cref="ProblemDetails"/> instance for unhandled exceptions (HTTP 500).
    /// </summary>
    /// <param name="ctx">The current HTTP context.</param>
    /// <returns>A <see cref="ProblemDetails"/> object representing an unexpected error.</returns>
    public static Microsoft.AspNetCore.Mvc.ProblemDetails FromUnhandled(HttpContext ctx)
    {
        var pd = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Unexpected error",
            Detail = "An unexpected error occurred."
        };
        pd.Extensions["traceId"] = ctx.TraceIdentifier;
        return pd;
    }

    /// <summary>
    /// Creates a <see cref="ProblemDetails"/> instance for the specified status, title, and detail.
    /// </summary>
    /// <param name="status">The HTTP status code.</param>
    /// <param name="title">The title of the error.</param>
    /// <param name="detail">The detail message of the error.</param>
    /// <param name="traceId">The optional trace identifier.</param>
    /// <returns>A <see cref="ProblemDetails"/> object representing the error.</returns>
    public static Microsoft.AspNetCore.Mvc.ProblemDetails Create(
        int status, string title, string detail, string? traceId = null)
    {
        var pd = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail
        };
        if (!string.IsNullOrWhiteSpace(traceId))
            pd.Extensions["traceId"] = traceId;

        return pd;
    }
}
