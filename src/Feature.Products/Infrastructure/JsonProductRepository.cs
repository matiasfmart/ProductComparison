using BuildingBlocks.Configuration;
using Features.Products.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Features.Products.Infrastructure;

/// <summary>
/// Repository implementation for retrieving products from a JSON file with ETag and file watcher support.
/// </summary>
public sealed class JsonProductRepository : IProductRepository, IDisposable
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly string _path;
    private Dictionary<string, Product> _byId = new(StringComparer.OrdinalIgnoreCase);
    private string _etag = "\"init\"";
    private FileSystemWatcher? _watcher;
    private readonly ILogger<JsonProductRepository> _log;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonProductRepository"/> class.
    /// </summary>
    /// <param name="opts">The data options for file path configuration.</param>
    /// <param name="log">The logger instance.</param>
    public JsonProductRepository(IOptions<DataOptions> opts, ILogger<JsonProductRepository> log) 
    {
        _log = log;                                                                               
        _path = opts.Value.FilePath ?? "data/products.json";
        _log.LogInformation("Repository initialized with path {path}", _path);                   
        Load();
        StartWatcher();
    }

    /// <summary>
    /// Starts the file system watcher to monitor changes in the JSON file.
    /// </summary>
    private void StartWatcher()
    {
        var full = Path.GetFullPath(_path);
        var dir = Path.GetDirectoryName(full);
        var file = Path.GetFileName(full);

        if (dir is null || string.IsNullOrWhiteSpace(file)) return;
        if (!Directory.Exists(dir)) return;

        _watcher = new FileSystemWatcher(dir, file)
        {
            EnableRaisingEvents = true,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
        };
        _log.LogInformation("Watcher started for {file}", file);
        _watcher.Changed += (_, __) => Load();
        _watcher.Created += (_, __) => Load();
        _watcher.Renamed += (_, __) => Load();
    }

    /// <summary>
    /// Loads products from the JSON file into memory and updates the ETag.
    /// </summary>
    private void Load()
    {
        _lock.EnterWriteLock();
        try
        {
            if (!File.Exists(_path))
            {
                _byId = new(StringComparer.OrdinalIgnoreCase);
                _etag = "\"empty\"";
                _log.LogWarning("File {path} not found. Cache empty", _path);
                return;
            }

            using var fs = File.Open(_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var items = JsonSerializer.Deserialize<List<Product>>(fs, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new();

            _byId = items.ToDictionary(p => p.Id, StringComparer.OrdinalIgnoreCase);
            var serialized = JsonSerializer.Serialize(items);
            _etag = $"\"{ComputeHash(serialized)}\"";

            _log.LogInformation("Loaded {count} products. New ETag {etag}", _byId.Count, _etag);
        }
        catch (Exception ex)
        {
            //si hay error leyendo dejamos el cache vacío con ETag 'error'
            _byId = new(StringComparer.OrdinalIgnoreCase);
            _etag = "\"error\"";
            _log.LogError(ex, "Error loading products from {path}", _path);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Retrieves products by their IDs from the in-memory cache.
    /// </summary>
    /// <param name="ids">The collection of product IDs to retrieve.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A tuple containing the list of found products, the ETag, and a list of missing IDs.</returns>
    public Task<(IReadOnlyList<Product>, string, IReadOnlyList<string>)> GetByIdsAsync(IEnumerable<string> ids, CancellationToken ct)
    {
        _lock.EnterReadLock();
        try
        {
            var list = new List<Product>();
            var missing = new List<string>();

            foreach (var id in ids)
            {
                if (_byId.TryGetValue(id, out var p)) list.Add(p);
                else missing.Add(id);
            }
            return Task.FromResult(((IReadOnlyList<Product>)list, _etag, (IReadOnlyList<string>)missing));
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Computes a SHA256 hash for the given content string.
    /// </summary>
    /// <param name="content">The content to hash.</param>
    /// <returns>The computed hash as a hexadecimal string.</returns>
    private static string ComputeHash(string content)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(content);
        return Convert.ToHexString(sha.ComputeHash(bytes));
    }

    /// <summary>
    /// Disposes the file system watcher.
    /// </summary>
    public void Dispose() => _watcher?.Dispose();
}
