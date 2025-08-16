using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ApiHost.Extensions;

public static class HealthSetup
{
    public static IServiceCollection AddApiHealthChecks(this IServiceCollection services, IConfiguration config)
    {
        var dataPath = config.GetSection("Data")["FilePath"] // 👈 misma clave que usa DataOptions
                       ?? Path.Combine(AppContext.BaseDirectory, "data", "products.json");

        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" }) // 👈 solo liveness
            .Add(new HealthCheckRegistration(
                "products-json",
                sp => new JsonFileHealthCheck(dataPath),
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "ready" })); // 👈 solo readiness

        return services;
    }

    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // /health/live -> solo checks taggeadas como "live"
        endpoints.MapHealthChecks("/health/live",
            new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });

        // /health/ready -> solo checks taggeadas como "ready"
        endpoints.MapHealthChecks("/health/ready",
            new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("ready"),
                ResponseWriter = async (ctx, report) =>
                {
                    ctx.Response.ContentType = "application/json; charset=utf-8";
                    var payload = new
                    {
                        status = report.Status.ToString(),
                        checks = report.Entries.Select(e => new
                        {
                            name = e.Key,
                            status = e.Value.Status.ToString(),
                            error = e.Value.Exception?.Message
                        }),
                        timestamp = DateTimeOffset.UtcNow
                    };
                    await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload));
                }
            });

        return endpoints;
    }
}

/// <summary>
/// HealthCheck que valida que el archivo JSON exista y se pueda deserializar.
/// </summary>
public sealed class JsonFileHealthCheck : IHealthCheck
{
    private readonly string _filePath;
    public JsonFileHealthCheck(string filePath) => _filePath = filePath;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(_filePath))
                return HealthCheckResult.Unhealthy($"File not found: {_filePath}");

            await using var stream = File.OpenRead(_filePath);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            if (doc.RootElement.ValueKind is not (JsonValueKind.Object or JsonValueKind.Array))
                return HealthCheckResult.Unhealthy("Invalid JSON root.");

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Error reading JSON.", ex);
        }
    }
}
