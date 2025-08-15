using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BuildingBlocks.Configuration;
using Features.Products.Domain;
using Microsoft.Extensions.Options;

namespace Features.Products.Infrastructure;

public sealed class JsonProductRepository : IProductRepository, IDisposable
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly string _path;
    private Dictionary<string, Product> _byId = new(StringComparer.OrdinalIgnoreCase);
    private string _etag = "\"init\"";
    private FileSystemWatcher? _watcher;

    public JsonProductRepository(IOptions<DataOptions> opts)
    {
        _path = opts.Value.FilePath ?? "data/products.json";
        Load();
        StartWatcher();
    }

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
        _watcher.Changed += (_, __) => Load();
        _watcher.Created += (_, __) => Load();
        _watcher.Renamed += (_, __) => Load();
    }


    private void Load()
    {
        _lock.EnterWriteLock();
        try
        {
            if (!File.Exists(_path))
            {
                _byId = new(StringComparer.OrdinalIgnoreCase);
                _etag = "\"empty\"";
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
        }
        catch
        {
            //si hay error leyendo dejamos el cache vacío con ETag 'error'
            _byId = new(StringComparer.OrdinalIgnoreCase);
            _etag = "\"error\"";
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

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

    private static string ComputeHash(string content)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(content);
        return Convert.ToHexString(sha.ComputeHash(bytes));
    }

    public void Dispose() => _watcher?.Dispose();
}
