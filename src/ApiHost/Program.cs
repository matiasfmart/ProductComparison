using Asp.Versioning;
using BuildingBlocks.Configuration;
using BuildingBlocks.Errors;
using Features.Products.GetByIds;
using Features.Products.Infrastructure;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

//instancio servicios base
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//versionado de API
builder.Services
    .AddApiVersioning(o =>
    {
        o.DefaultApiVersion = new ApiVersion(1, 0);
        o.AssumeDefaultVersionWhenUnspecified = true;
        o.ReportApiVersions = true;
    })
    .AddApiExplorer(o =>
    {
        o.GroupNameFormat = "'v'V";
        o.SubstituteApiVersionInUrl = true;
    });

//estructura de respuesta estandar
builder.Services.AddProblemDetails();

builder.Services.Configure<DataOptions>(builder.Configuration.GetSection("data"));
builder.Services.AddSingleton<IProductRepository, JsonProductRepository>();
builder.Services.AddScoped<GetByIdsHandler>();
builder.Services.AddScoped<GetByIdsValidator>();

//agrego health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

//middleware de exceptions no controladas
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        //log de la excepción con stack trace
        var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (feature?.Error is not null)
            app.Logger.LogError(feature.Error, "Unhandled exception");

        var problem = ProblemDetailsFactoryEx.FromUnhandled(context);
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(problem);
    });
});


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//endpoints de health
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

var api = app.NewVersionedApi();
var v1 = api.MapGroup("/api/v{version:apiVersion}");

var products = v1.MapGroup("/products").HasApiVersion(1, 0).WithTags("Products");
products.MapGetByIds();

app.Run();
public partial class Program { }