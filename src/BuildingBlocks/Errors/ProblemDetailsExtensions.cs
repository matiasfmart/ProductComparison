using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.Errors;

public static class ProblemDetailsFactoryEx
{
    //generico como middleware para exceptions no controladas
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

    //helper para 400/404
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
