using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Products.IntegrationTests;

public class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            // BaseDirectory apunta a /test/Products.IntegrationTests/bin/Debug/net8.0/
            var baseDir = AppContext.BaseDirectory;

            var solutionRoot = Path.GetFullPath(Path.Combine(baseDir, "../../../../.."));

            // Ruta absoluta al JSON del repo
            var dataPath = Path.Combine(solutionRoot, "data", "products.json");

            // Inyectamos config para el entorno de test
            var overrides = new Dictionary<string, string?>
            {
                ["data:FilePath"] = dataPath
            };

            configBuilder.AddInMemoryCollection(overrides);
        });
    }
}
