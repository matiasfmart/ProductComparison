using BuildingBlocks.Errors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Features.Products.GetByIds;

/// <summary>
/// Provides endpoint mapping for retrieving multiple products by their IDs.
/// </summary>
public static class GetByIdsEndpoint
{
    /// <summary>
    /// Maps the GET endpoint for retrieving product details by IDs.
    /// </summary>
    /// <param name="group">The route group builder to add the endpoint to.</param>
    /// <returns>The updated <see cref="RouteGroupBuilder"/>.</returns>
    public static RouteGroupBuilder MapGetByIds(this RouteGroupBuilder group)
    {
        group.MapGet("",
            async ([FromQuery(Name = "ids")] string[] ids,
            GetByIdsHandler handler,
            HttpContext ctx,
            ILoggerFactory lf) =>
        {
            var log = lf.CreateLogger("Endpoint.Products.GetByIds");
            log.LogInformation("Request with {idsCount} ids", ids?.Length ?? 0);
            //soporte condicional con ETag (If-None-Match)
            var (resp, status, etag, title, detail) = await handler.HandleAsync(new GetByIdsRequest(ids), ctx.RequestAborted);

            if (status != StatusCodes.Status200OK)
            {
                log.LogWarning("Non-OK {status}: {title} - {detail}", status, title, detail);
                var problem = ProblemDetailsFactoryEx.Create(status, title ?? "Error", detail ?? "Request failed", ctx.TraceIdentifier);
                return Results.Problem(problem);
            }

            //si el cliente manda If-None-Match igual al ETag, devolvemos 304
            if (!string.IsNullOrEmpty(etag) && ctx.Request.Headers.IfNoneMatch == etag)
            {
                log.LogInformation("ETag matched; return 304");
                return Results.StatusCode(StatusCodes.Status304NotModified);
            }

            if (!string.IsNullOrEmpty(etag))
                ctx.Response.Headers.ETag = etag;

            log.LogInformation("Returning {count} products", resp!.Products.Count);
            return Results.Ok(resp);
        })
        .Produces<GetByIdsResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithName("GetProductsByIds")
        .WithSummary("Returns details for multiple products by ids")
        .WithDescription("Use this endpoint to power the item comparison feature.");

        return group;
    }
}
