using System.Reflection;
using Asp.Versioning.ApiExplorer;
using Microsoft.OpenApi.Models;

namespace ApiHost.Extensions;

/// <summary>
/// Provides extension methods to configure Swagger with API versioning.
/// </summary>
public static class SwaggerSetup
{
    /// <summary>
    /// Adds Swagger generation and API versioning support to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add Swagger and versioning to.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddSwaggerWithVersioning(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        // Explorar y publicar un Swagger por versión de API
        services.AddSwaggerGen(options =>
        {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            }

            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Product Comparison API",
                Version = "v1",
                Description = "API para comparación de ítems (productos)."
            });
        });

        return services;
    }

    /// <summary>
    /// Configures the application to use Swagger and Swagger UI with API versioning support.
    /// </summary>
    /// <param name="app">The web application instance.</param>
    /// <param name="provider">The API version description provider.</param>
    /// <returns>The configured <see cref="WebApplication"/>.</returns>
    public static WebApplication UseSwaggerWithUI(this WebApplication app, IApiVersionDescriptionProvider provider)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            c.DocumentTitle = "Product Comparison API Docs";
            c.RoutePrefix = "swagger";
        });

        return app;
    }
}
