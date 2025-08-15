# Run

Guía corta para levantar, probar y verificar la API **ProductComparison**.

> **Stack**: .NET 8 + Minimal APIs + Versionado (Asp.Versioning)  
> **Persistencia**: archivo local `data/products.json` (sin DB)  
> **Endpoint principal**: `GET /api/v1/products?ids=...`

---

## 1) Requisitos

- .NET SDK **8.0+** (`dotnet --version`)
- (Opcional) `jq` para pretty-print en ejemplos de `curl`

---

## 2) Levantar la API

```bash
# Restaurar y compilar
dotnet restore
dotnet build -c Release

# Correr tests
dotnet test -c Release

# Levantar la API
dotnet run --project src/ApiHost
```

Por defecto Kestrel publica en un puerto libre (p. ej. `http://localhost:5000`).  
Podés fijar el puerto exportando `ASPNETCORE_URLS`:

```bash
# Linux/macOS
export ASPNETCORE_URLS=http://localhost:5000
dotnet run --project src/ApiHost

# Windows (PowerShell)
$env:ASPNETCORE_URLS="http://localhost:5000"
dotnet run --project src/ApiHost
```

**Swagger:** `http://localhost:5000/swagger`  
**Health:**  `http://localhost:5000/health/live`  y  `http://localhost:5000/health/ready`

---

## 3) Datos locales

La API lee por defecto `data/products.json` (ruta relativa al root de la solución).  
Podés cambiar la ruta vía configuración (clave `data:filePath`) en *appsettings* o variables de entorno.

```jsonc
// appsettings.json (ejemplo)
{
  "data": {
    "filePath": "data/products.json", // ruta relativa o absoluta
    "format": "json"                   // reservado para alternar a CSV en el futuro
  }
}
```

---

## 4) Endpoints principales

### GET `/api/v1/products?ids={id}&ids={id}...`
Devuelve detalles de múltiples productos por id.  
**Campos**: `name`, `imageUrl`, `description`, `price`, `currency`, `rating`, `specifications`.

**Ejemplos**

```bash
# 2 productos por id
curl -s "http://localhost:5000/api/v1/products?ids=kbd-redragon-k552&ids=hx-cloud2" | jq

# 1 producto (smoke test)
curl -s "http://localhost:5000/api/v1/products?ids=kbd-redragon-k552" | jq
```

**Manejo de errores (RFC 7807)**

```bash
# 400 Bad Request (ids faltantes)
curl -i "http://localhost:5000/api/v1/products"

# 404 Not Found (ids inexistentes)
curl -i "http://localhost:5000/api/v1/products?ids=not-found-id"
```

**Caching condicional (ETag / If-None-Match)**

```bash
# 1) Primer GET obtiene ETag en el header
curl -i "http://localhost:5000/api/v1/products?ids=kbd-redragon-k552"

# 2) Reintento con If-None-Match debe retornar 304 Not Modified sin body
etag=$(curl -si "http://localhost:5000/api/v1/products?ids=kbd-redragon-k552" | awk -F': ' '/^ETag/ {print $2}' | tr -d '\r')
curl -i -H "If-None-Match: $etag" "http://localhost:5000/api/v1/products?ids=kbd-redragon-k552"
```

> La respuesta exitosa incluye `ETag`. Si el cliente envía **If-None-Match** con ese valor y el archivo `products.json` no cambió, el servidor responde **304**.

---

## 5) Pruebas automatizadas

```bash
dotnet test -c Release
```

Incluye:
- **Unit**: repositorio JSON, validador y handler.
- **Integration**: endpoint `/api/v1/products` con lectura real del archivo de datos y verificación de **304**.

---

## 6) Troubleshooting

- **404 (ids inexistentes)**: verifica que los ids existan en `data/products.json`.
- **304 esperado y no ocurre**: modificaste el JSON; el ETag cambió. Probá con el nuevo ETag o restablecé el archivo.
- **No abre Swagger**: confirma el puerto (revisá el output de `dotnet run`) o fija `ASPNETCORE_URLS`.

---

## 7) Referencias rápidas

- Swagger UI: `GET /swagger`
- Health: `GET /health/live`, `GET /health/ready`
- Endpoint principal: `GET /api/v1/products?ids=...`