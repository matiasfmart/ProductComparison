using BuildingBlocks.Errors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Features.Products.GetByIds;

public static class GetByIdsEndpoint
{
    public static RouteGroupBuilder MapGetByIds(this RouteGroupBuilder group)
    {
        group.MapGet("", async (
            [FromQuery(Name = "ids")] string[] ids,
            GetByIdsHandler handler,
            HttpContext ctx) =>
        {
            //soporte condicional con ETag (If-None-Match)
            var (resp, status, etag, title, detail) = await handler.HandleAsync(new GetByIdsRequest(ids), ctx.RequestAborted);

            if (status != StatusCodes.Status200OK)
            {
                var problem = ProblemDetailsFactoryEx.Create(status, title ?? "Error", detail ?? "Request failed", ctx.TraceIdentifier);
                return Results.Problem(problem);
            }

            //si el cliente manda If-None-Match igual al ETag, devolvemos 304
            if (!string.IsNullOrEmpty(etag) && ctx.Request.Headers.IfNoneMatch == etag)
                return Results.StatusCode(StatusCodes.Status304NotModified);

            if (!string.IsNullOrEmpty(etag))
                ctx.Response.Headers.ETag = etag;

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
