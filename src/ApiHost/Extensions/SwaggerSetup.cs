using System.Reflection;
using Asp.Versioning.ApiExplorer;
using Microsoft.OpenApi.Models;

namespace ApiHost.Extensions;

public static class SwaggerSetup
{
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
