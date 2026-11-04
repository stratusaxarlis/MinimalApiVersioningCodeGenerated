# MinimalApiVersioningCodeGenerated

A .NET 10 Minimal API that demonstrates **API versioning where the endpoints are generated from code**. You describe an endpoint as a plain C# `record` with an `[Endpoint]` attribute. At startup the app discovers every request record, expands it across API versions, and maps it to a versioned route. Requests are dispatched through MediatR, validated with FluentValidation, and documented with OpenAPI and Scalar.

The sample feature is a small **Products** API backed by an in-memory repository, with `v1` and `v2` of the API side by side.

---

## What's included

### Tech stack

| Concern | Package |
|---|---|
| API versioning (URL segment) | `Asp.Versioning.Http`, `Asp.Versioning.Mvc.ApiExplorer`, `Asp.Versioning.OpenApi` |
| CQRS / mediator | `MediatR` |
| Validation | `FluentValidation` (+ DI extensions) |
| API docs | `Microsoft.AspNetCore.OpenApi` + `Scalar.AspNetCore` |
| Logging | `Serilog.AspNetCore` (console + rolling JSON file in `logs/`) |
| Rate limiting | Custom config-driven limiter built on ASP.NET Core `RateLimiter` |

### Project layout

```
MinimalApiVersioningCodeGenerated.slnx
└── MinimalApiVersioningCodeGenerated/
    ├── Program.cs                      Composition root and pipeline order
    ├── appsettings*.json               Serilog and rate limiting config
    ├── Properties/launchSettings.json  http (5272) and https (7156) profiles
    ├── Constants/                      API name, default version, header name, log theme
    ├── Extensions/
    │   ├── MinimalApiDiscovery.cs      Scans assemblies for [Endpoint] request records
    │   ├── MinimalApiExtensions.cs     Maps discovered endpoints into versioned groups
    │   ├── ServiceCollectionExtensions.cs   MediatR, validators, API versioning, OpenAPI
    │   ├── WebApplicationExtensions.cs      Middleware pipeline, OpenAPI and Scalar mapping
    │   ├── RateLimiterExtensions.cs    Config-driven fixed-window rate limiting
    │   ├── ResultExtensions.cs         Result/Error -> HTTP ProblemDetails mapping
    │   └── LoggingExtensions.cs        Serilog bootstrap and full logger
    ├── Common/
    │   ├── Attributes/CQRSAttribute.cs [Endpoint(route, Verb, Version, UntilVersion, Group)]
    │   ├── Interfaces/                 ICommand, ICommand<T>, IQuery<T>, IRepository<T>, Result contracts
    │   ├── Mappers/CQRSMapper.cs       Turns a request record into a MapGet/MapPost/... handler
    │   ├── Models/                     Result/Error/PagedResult and option models
    │   ├── Pipelines/ValidationPipeline.cs   MediatR behavior that runs FluentValidation
    │   └── Enumerators/                HttpVerb, ErrorType
    └── Feature/TestFeature/            The sample "Products" feature slice
        ├── EndPoints/                  Request records and their [Endpoint] attributes
        ├── Commands/ Queries/          MediatR handlers
        ├── Validators/                 FluentValidation rules (shared + per version)
        ├── Persistence/                Thread-safe in-memory repository
        ├── DTO/ Errors/ Interfaces/    Product entity, error factory, shared contracts
        └── ProductsFeatureExtensions.cs  DI registration and seed data
```

### How it works

1. **Declare** a request record and tag it with `[Endpoint]`:
   ```csharp
   [Endpoint("{id}", Verb = HttpVerb.Get, UntilVersion = 2, Group = "Products")]
   public sealed record GetProduct(Guid Id) : IQuery<ProductResponse>;
   ```
   - `Version` is the first version the endpoint exists in (default `1`).
   - `UntilVersion` is the last version it exists in (inclusive). It is expanded to one route per version.
   - `Group` becomes the route segment, lowercased (`Products` -> `/products`).
2. **Discover**: `CqrsEndpointDiscovery` finds every record implementing `ICommand`, `ICommand<T>` or `IQuery<T>`.
3. **Map**: `CqrsEndpointExtensions` creates `/api/v{version}/{group}/{route}` for each version. GET and DELETE bind from route and query string; POST, PUT and PATCH bind from the JSON body.
4. **Handle**: the request is sent through MediatR. `ValidationBehavior` runs the FluentValidation validators first and short-circuits with a `400` on failure.
5. **Respond**: handlers return `Result` / `Result<T>`, and `ResultHttpExtensions` converts them to `200`, `204` or RFC 7807 problem responses.

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- `curl` (preinstalled on macOS)
- Optional: JetBrains Rider or Visual Studio

## Run

From the solution root:

```bash
dotnet restore
dotnet run --project MinimalApiVersioningCodeGenerated --launch-profile http
```

The API listens on **http://localhost:5272**.

Or use the HTTPS profile, which also opens Scalar in the browser (**https://localhost:7156**):

```bash
dotnet dev-certs https --trust     # one-time, trusts the dev certificate
dotnet run --project MinimalApiVersioningCodeGenerated --launch-profile https
```

In Rider, open `MinimalApiVersioningCodeGenerated.slnx` and run the `https` or `http` profile.

> The curl examples below use the `http` profile. `UseHttpsRedirection` is enabled, so against the `https` profile `http://localhost:5272` will redirect to HTTPS. Either use `https://localhost:7156` with `curl -k`, or stick with the `http` profile.

## API documentation (Scalar)

OpenAPI and Scalar are mapped in **Development** only.

| What | URL |
|---|---|
| Scalar UI | http://localhost:5272/scalar |
| OpenAPI `v1` document | http://localhost:5272/openapi/v1.json |
| OpenAPI `v2` document | http://localhost:5272/openapi/v2.json |

Use the document picker in Scalar to switch between `v1` and `v2`. Each request has a **Test Request** button and a **Client Libraries -> Shell / cURL** snippet, which produces the same commands as below.

---

## Endpoints

| Method | Path | Versions | Description | Success |
|---|---|---|---|---|
| GET | `/api/v{version}/products` | v1, v2 | List products (`page`, `size` query params) | `200` paged result |
| GET | `/api/v1/products/{id}` | v1 | Get product, flat price | `200` |
| GET | `/api/v2/products/{id}` | v2 | Get product, nested `price` object | `200` |
| POST | `/api/v1/products` | v1 | Create product | `200` with the new id |
| POST | `/api/v2/products` | v2 | Create product | `200` with the new id |
| DELETE | `/api/v2/products/{id}` | v2 | Delete product | `204` |

The version is part of the URL path. A path that doesn't exist for that version (for example `DELETE` on `v1`) is not routed.

### Seed data

Three products are loaded at startup. The store is **in memory**, so created products are lost when the app restarts.

| Id | Name | Price |
|---|---|---|
| `11111111-1111-1111-1111-111111111111` | Keyboard | 49.99 USD |
| `22222222-2222-2222-2222-222222222222` | Mouse | 24.99 EUR |
| `33333333-3333-3333-3333-333333333333` | Monitor | 199.00 USD |

---

## cURL examples

These mirror what Scalar generates. Replace the host if you use the HTTPS profile (add `-k` for the dev certificate).

### List products

```bash
curl 'http://localhost:5272/api/v1/products' \
  --header 'Accept: application/json'
```

With paging (`page` >= 1, `size` between 1 and 100):

```bash
curl 'http://localhost:5272/api/v1/products?page=1&size=2' \
  --header 'Accept: application/json'
```

The same endpoint exists in v2:

```bash
curl 'http://localhost:5272/api/v2/products?page=1&size=2' \
  --header 'Accept: application/json'
```

Response (both versions):

```json
{
  "items": [
    { "id": "11111111-1111-1111-1111-111111111111", "name": "Keyboard", "price": 49.99, "currency": "USD" }
  ],
  "page": 1,
  "pageSize": 2,
  "totalCount": 3,
  "totalPages": 2,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

### Get a product by id

**v1**: flat price:

```bash
curl 'http://localhost:5272/api/v1/products/11111111-1111-1111-1111-111111111111' \
  --header 'Accept: application/json'
```

```json
{ "id": "11111111-1111-1111-1111-111111111111", "name": "Keyboard", "price": 49.99, "currency": "USD" }
```

**v2**: price is a nested object (breaking change from v1):

```bash
curl 'http://localhost:5272/api/v2/products/11111111-1111-1111-1111-111111111111' \
  --header 'Accept: application/json'
```

```json
{ "id": "11111111-1111-1111-1111-111111111111", "name": "Keyboard", "price": { "amount": 49.99, "currency": "USD" } }
```

### Create a product

**v1**:

```bash
curl --request POST 'http://localhost:5272/api/v1/products' \
  --header 'Content-Type: application/json' \
  --header 'Accept: application/json' \
  --data '{
    "name": "Headphones",
    "price": 59.90,
    "currency": "USD"
  }'
```

**v2**:

```bash
curl --request POST 'http://localhost:5272/api/v2/products' \
  --header 'Content-Type: application/json' \
  --header 'Accept: application/json' \
  --data '{
    "name": "Headphones",
    "price": 59.90,
    "currency": "USD"
  }'
```

Both return `200 OK` with the new product id as a JSON string:

```json
"4f0c1a52-8b3e-4d55-9a57-3d6e0a1b7c21"
```

Validation rules (shared by v1 and v2): `name` required, max 200 characters; `price` greater than 0; `currency` a 3-letter uppercase ISO 4217 code such as `USD`.

### Delete a product (v2 only)

```bash
curl --request DELETE 'http://localhost:5272/api/v2/products/33333333-3333-3333-3333-333333333333' \
  --header 'Accept: application/json'
```

Returns `204 No Content`. Deleting an id that doesn't exist returns `404`.

### Create, read, delete in one go

```bash
BASE='http://localhost:5272'

# Create (v2) and capture the id (strips the JSON quotes)
ID=$(curl -s --request POST "$BASE/api/v2/products" \
  --header 'Content-Type: application/json' \
  --data '{"name":"Webcam","price":79.00,"currency":"EUR"}' | tr -d '"')
echo "Created: $ID"

# Read it back in v1 and v2 to compare response shapes
curl -s "$BASE/api/v1/products/$ID"; echo
curl -s "$BASE/api/v2/products/$ID"; echo

# Delete it (v2) and confirm it's gone
curl -i --request DELETE "$BASE/api/v2/products/$ID"
curl -i "$BASE/api/v2/products/$ID"
```

### Error responses

Errors use RFC 7807 problem details with an extra `code` field.

**Validation failure** (`400`):

```bash
curl -i --request POST 'http://localhost:5272/api/v2/products' \
  --header 'Content-Type: application/json' \
  --data '{ "name": "", "price": 0, "currency": "usd" }'
```

```json
{
  "title": "Validation failed",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "errors": {
    "name": ["'Name' must not be empty."],
    "price": ["'Price' must be greater than '0'."],
    "currency": ["'Currency' must be a 3-letter ISO 4217 code (e.g. 'USD')."]
  },
  "code": "Validation.Failed"
}
```

**Not found** (`404`):

```bash
curl -i 'http://localhost:5272/api/v1/products/00000000-0000-0000-0000-000000000000'
```

```json
{
  "title": "Resource not found",
  "status": 404,
  "detail": "Product with id '00000000-0000-0000-0000-000000000000' was not found.",
  "code": "Product.NotFound"
}
```

**Rate limited** (`429`) returns a `Retry-After` header and a problem body.

> Tip: add `-i` to see response headers. `ReportApiVersions` is enabled, so responses include `api-supported-versions` headers.

---

## Configuration

**Rate limiting** (`ClientRateLimiting` section in `appsettings*.json`):

- Clients are identified by the `x-client-ip` header, falling back to the remote IP.
- `appsettings.json` (production defaults) applies global rules (40 requests/second, plus 15m, 12h and 7d windows) and stricter per-endpoint rules for `POST` and `DELETE` on products.
- `appsettings.Development.json` relaxes this to 1000 requests/second and turns per-endpoint limiting off.
- `/health` and `/openapi/*` are whitelisted.

**Logging**: Serilog writes to the console and to `logs/api-.log` (daily rolling, 14 files kept, compact JSON).

**Other**: the default API version is `1` (`ApiConstants.DefaultApiVersion`). `AllowedHosts` in `appsettings.json` is restricted to `api.yourdomain.com;localhost`, so update it before deploying.

---

## Adding a new endpoint

1. Add a request record in `Feature/<YourFeature>/EndPoints/` implementing `IQuery<T>`, `ICommand<T>` or `ICommand`, and decorate it with `[Endpoint(...)]`.
2. Add a `IRequestHandler<,>` for it. MediatR picks it up by assembly scan.
3. Optionally add an `AbstractValidator<YourRequest>`. It is picked up automatically.
4. Run the app. The route appears in Scalar for every version in the `Version` to `UntilVersion` range, with no manual mapping needed.
