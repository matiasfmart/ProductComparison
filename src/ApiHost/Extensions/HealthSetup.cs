using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ApiHost.Extensions;

/// <summary>
/// Provides extension methods to configure health checks for the API.
/// </summary>
public static class HealthSetup
{
    /// <summary>
    /// Adds health checks to the service collection, including liveness and readiness checks.
    /// </summary>
    /// <param name="services">The service collection to add health checks to.</param>
    /// <param name="config">The application configuration.</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
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

    /// <summary>
    /// Maps health check endpoints for liveness and readiness.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <returns>The updated <see cref="IEndpointRouteBuilder"/>.</returns>
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
/// HealthCheck that validates the existence and deserialization of a JSON file.
/// </summary>
public sealed class JsonFileHealthCheck : IHealthCheck
{
    private readonly string _filePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonFileHealthCheck"/> class.
    /// </summary>
    /// <param name="filePath">The path to the JSON file to check.</param>
    public JsonFileHealthCheck(string filePath) => _filePath = filePath;

    /// <summary>
    /// Checks the health of the JSON file by verifying its existence and deserializability.
    /// </summary>
    /// <param name="context">The health check context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="HealthCheckResult"/> indicating the health of the JSON file.</returns>
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
