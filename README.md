# ProductComparison — Backend API (Item Comparison)

API backend minimalista para alimentar una **función de comparación de ítems**.
Centrada en **buenas prácticas**: diseño por vertical slice, manejo uniforme de errores (**RFC 7807**), **ETag** para cache condicional, validación *fail-fast*, logging estructurado con correlación y suite de **tests** (unit, integration, E2E).

---

## Índice

* [Objetivo](#objetivo)
* [Arquitectura y decisiones](#arquitectura-y-decisiones)
* [Modelo de datos](#modelo-de-datos)
* [Endpoints](#endpoints)
* [Errores y contrato](#errores-y-contrato)
* [Caching y ETag](#caching-y-etag)
* [Logging y observabilidad](#logging-y-observabilidad)
* [Configuración](#configuración)
* [Ejecutar localmente](#ejecutar-localmente)
* [Tests](#tests)
* [Performance y escalabilidad](#performance-y-escalabilidad)
* [Evolución a producción](#evolución-a-producción)
* [Uso de GenAI y herramientas](#uso-de-genai-y-herramientas)

---

## Objetivo

**Item Comparison**: exponer un endpoint que devuelva **detalles de múltiples productos** (nombre, imagen, descripción, precio, rating y especificaciones) para alimentar una UI de comparación.

Requisitos del enunciado:

* Backend REST que **no** use bases reales (persistencia en **JSON/CSV** local).
* **Buenas prácticas**: manejo de errores, documentación, tests, claridad del diseño.
* Se permite y fomenta el uso de herramientas de **GenAI**.

---

## Arquitectura y decisiones

**Stack**: .NET 8 + Minimal APIs + Asp.Versioning.
**Estilo**: **Vertical Slice** (por feature), con ideas de arquitectura **hexagonal** (puertos/adaptadores).

```text
src/
 ├─ BuildingBlocks/                 # utilitarios transversales
 │   ├─ Configuration/              # DataOptions (ruta del JSON)
 │   └─ Errors/                     # ProblemDetailsFactoryEx (RFC 7807)
 └─ Features/
     └─ Products/
         ├─ Domain/                 # entidades (Product, Money)
         ├─ Infrastructure/         # adaptadores (JsonProductRepository)
         └─ GetByIds/               # vertical slice: endpoint + handler + validator + DTOs
```

**Decisiones clave**

* **Separación de responsabilidades**

  * **Endpoint**: protocolo HTTP (status/headers), doc swagger.
  * **Handler**: caso de uso (qué es OK/Invalid/NotFound).
  * **Validator**: *fail-fast* de entrada.
  * **Repository (puerto/adapter)**: acceso a datos (JSON en disco), índice en memoria, **ETag**.
* **Versionado de API** (Asp.Versioning): rutas `api/v{version}` y planeamiento de evolución.
* **Error handling** homogéneo con **ProblemDetails** (RFC 7807).
* **Cache condicional** con **ETag** (ahorra ancho de banda/respuesta).
* **Logging** estructurado, con **correlation id** por request.

---

## Modelo de datos

**Product**

```jsonc
{
  "id": "kbd-redragon-k552",
  "name": "Redragon K552 Kumara",
  "imageUrl": "https://...",
  "description": "Tenkeyless mechanical keyboard...",
  "price": 49.99,
  "rating": 4.5,
  "specifications": {
    "switch": "Outemu Red",
    "layout": "ANSI",
    "connection": "Wired"
  }
}
```

* El repositorio **indexa en memoria por `id`** para lookups O(1).

---

## Endpoints

### GET `/api/v1/products?ids={id}&ids={id}...`

Retorna detalles de múltiples productos por `id`.

**Query**

* `ids` (repetible): lista de IDs a consultar.
  Ej.: `?ids=kbd-redragon-k552&ids=hx-cloud2`

**Respuesta 200**

```jsonc
{
  "items": [
    {
      "id": "kbd-redragon-k552",
      "name": "Redragon K552 Kumara",
      "imageUrl": "...",
      "description": "...",
      "price": 49.99,
      "rating": 4.5,
      "specifications": { "switch": "Outemu Red", "layout": "ANSI" }
    }
  ]
}
```

Headers:

* `ETag: "abcd1234..."`

**Códigos**

* `200 OK` — con body y `ETag`.
* `304 Not Modified` — si el cliente envía `If-None-Match` con el ETag actual.
* `400 Bad Request` — validación fallida (falta `ids` o vacío).
* `404 Not Found` — ninguno de los `ids` existe.

**Health**

* `GET /health/live`  — liveness.
* `GET /health/ready` — readiness.

---

## Errores y contrato

* Formato estándar **RFC 7807 (ProblemDetails)** para 4xx/5xx:

```jsonc
{
  "status": 404,
  "title": "Not Found",
  "detail": "No products match the provided ids.",
  "traceId": "0HM...xyz"
}
```

* Middleware global captura **excepciones no controladas** ⇒ `500` con `ProblemDetails`.
* En errores esperados (validación/no encontrado) el **endpoint** construye `ProblemDetails` consistente.

---

## Caching y ETag

* El repositorio calcula un **ETag fuerte** (`SHA-256` del contenido serializado) y lo expone al endpoint.
* Flujo:

  1. Cliente hace `GET` → recibe `ETag: "hash"`.
  2. Cliente reintenta con `If-None-Match: "hash"`.
  3. Si no hubo cambios, la API responde `304` sin body.

**Beneficios**: evita payloads repetidos y reduce latencia/ancho de banda, especialmente detrás de un CDN.

---

## Logging y observabilidad

* **Correlación por request**: middleware agrega `traceId` (o `X-Request-Id` si el cliente lo envía) a todos los logs.
* **Request logging**: método, ruta, status, **latencia**.
* Logs de **negocio** (p. ej. cantidad de `ids` faltantes) en el handler.
* Logs de **infraestructura** en el repo (carga/recarga del JSON, cambios de ETag, errores de I/O).

---

## Configuración

App lee la sección `data`:

```jsonc
{
  "data": {
    "filePath": "data/products.json"
  }
}
```

* Por defecto, `data/products.json` (relativo a la raíz del repo).
* En tests E2E se inyecta ruta **absoluta** vía `WebApplicationFactory`.

---

## Ejecutar localmente

Requisitos:

* .NET 8 SDK
* Visual Studio 2022 **o** `dotnet` CLI

### VS 2022

1. Abrir la solución.
2. Proyecto de inicio: `src/ProductComparison.Api` (o el nombre en tu repo).
3. F5. Swagger UI queda disponible en **Development**.

### CLI

```bash
dotnet restore
dotnet build
dotnet run --project src/ProductComparison.Api
```

Por defecto:

* API: `http://localhost:5000` (o el puerto que asigne el Kestrel).

### Ejemplos `curl`

```bash
# 200 OK
curl -i "http://localhost:5000/api/v1/products?ids=kbd-redragon-k552&ids=hx-cloud2"

# 304 Not Modified (reusar el ETag obtenido)
curl -i -H 'If-None-Match: "abcd1234..."' \
  "http://localhost:5000/api/v1/products?ids=kbd-redragon-k552"
```

---

## Tests

Pirámide de pruebas:

* **Unit**

  * `GetByIdsValidatorTests`: validación *fail-fast* y normalización de input.
  * `GetByIdsHandlerTests`: 400/404/200 + mapeos a DTO.
* **Integration**

  * `JsonProductRepositoryTests`: carga desde JSON real, índice por `id`, ETag.
* **E2E**

  * `ProductsApiIntegrationTests` con `WebApplicationFactory<Program>`: contrato HTTP real (`200/304/400/404`) y uso de `ETag`.

Ejecutar:

```bash
dotnet test -v n
```

---

## Performance y escalabilidad

* **Hot path sin I/O**: el repo mantiene un **índice en memoria** por `id`. Las lecturas son O(1).
* **Recarga segura**: cuando cambia el archivo, se recalcula el **ETag** y se reemplaza el snapshot de manera atómica (protección con **Reader/Writer lock**).
* **Cache HTTP**: `ETag` permite **304** y descarga cero bytes cuando no hay cambios.

---

## Evolución a producción

* **Storage**: cambiar `JsonProductRepository` por `DbProductRepository` (SQL/NoSQL) manteniendo `IProductRepository` (puerto).
* **Cache**: si hay múltiples instancias, mover cache a **distribuido** (Redis) o delegar al **CDN/API Gateway** con `ETag`.
* **Observabilidad**: exportar logs en **JSON**/OpenTelemetry, métricas (latencia por endpoint, tasa de aciertos 304, recargas).
* **Seguridad**: authn/authz (JWT), rate limit, input hardening (tam. de `ids`, límites de concurrencia).

---

## Uso de GenAI y herramientas

**Objetivo:** acelerar tareas repetitivas (scaffolding, documentación, testing) sin ceder control sobre diseño, contratos ni calidad.

**Herramientas utilizadas**

* **ChatGPT / GPT**: ideación, borradores de documentación, ejemplos de tests y snippets (p. ej., middleware de logging y `ProblemDetails`), borradores de **tests unitarios/e2e**, **plantillas** de endpoints/handlers/validators, **snippets** de logging y manejo de errores, **borradores** de README/.

* **Copilot-Agent Integrado VS2022**: autocompletado contextual para código repetitivo (p. ej., asserts, DTOs, wiring de DI).

**Controles de calidad aplicados**

* Antes de pedir código a la IA, se definio **contrato** (status/headers/payloads), **requisitos de validación** y **criterios de aceptacion**.
* Cada aporte de IA se validó con **tests** (unit, integration, E2E).
* Refactors y analisis para mantener **separación de responsabilidades** (endpoint ↔ handler ↔ repo) y evitar fugas de HTTP en negocio.

**Ejemplos de tareas donde la IA aportó valor**

* Generar el esqueleto de **tests E2E** con `WebApplicationFactory<Program>` y **override** de `data:FilePath` (evitar 404 por rutas relativas).
* Redactar y compactar el **middleware** de correlación (`traceId`) y logging de latencia.
* Crear **plantillas** de README y `run.md`, luego ajustadas manualmente para reflejar exactamente el repo.

**Transparencia y reproducibilidad**

* Los **prompts** representativos (limpios de datos internos) están en `docs/prompts.md` junto con el **resultado esperado** y la **validación** aplicada.

---

### Notas finales

* El diseño es **agnóstico al stack** (conceptos transferibles a Go/Python/Node): separación de delivery/uso de casos/infra, contrato de errores uniforme, cache condicional y una pirámide de tests que protege el comportamiento.
* El repositorio de datos en JSON cumple la restricción del challenge pero con **buenas prácticas** de performance y resiliencia.

> Para correr rápidamente: ver `docs/run.md`.