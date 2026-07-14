# Product API

A RESTful backend API for managing **Products** (and their related **Items**) built with
.NET 8, ASP.NET Core Web API, and EF Core, following Clean Architecture principles.

## Tech Stack

| Concern            | Choice                                          |
|---------------------|--------------------------------------------------|
| Framework           | .NET 8 / C#                                      |
| API Framework       | ASP.NET Core Web API                             |
| Database            | SQL Server + EF Core 8                           |
| Authentication      | JWT (access token + refresh token)               |
| Validation          | FluentValidation                                 |
| Object Mapping      | AutoMapper                                       |
| Documentation       | Swagger / OpenAPI (Swashbuckle)                  |
| Logging             | Serilog (structured, console sink)               |
| Testing             | xUnit, Moq, FluentAssertions, WebApplicationFactory, EF Core InMemory |
| Containerization    | Docker + Docker Compose                          |

## Solution Layout

```
ProductApi.sln
src/
  API/              ASP.NET Core Web API host (controllers, middleware, DI wiring)
  Application/      Use-case orchestration: DTOs, service interfaces/implementations, validators, mapping
  Domain/           Entities and domain exceptions — no external dependencies
  Infrastructure/   EF Core DbContext, repositories, Unit of Work, JWT token service
tests/
  API.Tests/            Integration tests via WebApplicationFactory + EF Core InMemory
  Application.Tests/    Unit tests for ProductService (mocked repositories)
  Infrastructure.Tests/ Unit tests for the EF Core repositories
docker-compose.yml   API + SQL Server for local/dev use
```

This mirrors a standard Clean Architecture dependency flow:
`API → Application → Domain`, with `Infrastructure` implementing `Application`'s interfaces
and depending on `Application`/`Domain` only (never the reverse).

## Database Schema

```sql
CREATE TABLE [dbo].[Product]
(
    [Id]           INT NOT NULL PRIMARY KEY IDENTITY (1,1),
    [ProductName]  NVARCHAR(255) NOT NULL,
    [CreatedBy]    NVARCHAR(100) NOT NULL,
    [CreatedOn]    DATETIME NOT NULL,
    [ModifiedBy]   NVARCHAR(100) NULL,
    [ModifiedOn]   DATETIME NULL
)

CREATE TABLE [dbo].[Item]
(
    [Id]         INT NOT NULL PRIMARY KEY IDENTITY (1,1),
    [ProductId]  INT NOT NULL FOREIGN KEY REFERENCES Product(Id),
    [Quantity]   INT NOT NULL
)
```

EF Core migrations aren't checked in (no SDK/network access was available to generate them
in this environment). Create the initial migration once you have the .NET 8 SDK and a
reachable NuGet feed:

```bash
dotnet tool install --global dotnet-ef   # if not already installed
cd src/API
dotnet ef migrations add InitialCreate --project ../Infrastructure --startup-project .
dotnet ef database update --project ../Infrastructure --startup-project .
```

Alternatively, set `"ApplyMigrationsOnStartup": true` (already the default for
`docker-compose` / `Development`) so the app calls `dbContext.Database.Migrate()` on boot.

## API Endpoints

| Method | Route                              | Auth | Description                       |
|--------|-------------------------------------|------|------------------------------------|
| GET    | `/api/v1/products`                  | No   | Paginated product list (`?pageNumber=&pageSize=&search=`) |
| GET    | `/api/v1/products/{id}`             | No   | Get a single product (with items) |
| POST   | `/api/v1/products`                  | Yes  | Create a product                  |
| PUT    | `/api/v1/products/{id}`             | Yes  | Update a product                  |
| DELETE | `/api/v1/products/{id}`             | Yes  | Delete a product                  |
| GET    | `/api/v1/products/{id}/items`       | No   | List items for a product          |
| POST   | `/api/v1/products/{id}/items`       | Yes  | Add an item to a product          |
| POST   | `/api/v1/auth/login`                | No   | Get access + refresh token        |
| POST   | `/api/v1/auth/refresh`              | No   | Exchange refresh token for a new pair |

> `AuthController.Login` currently accepts any non-empty username/password as a placeholder
> so the JWT flow can be demonstrated end-to-end. Swap in a real user store (e.g. ASP.NET
> Core Identity or your own `Users` table + password hashing) before shipping this.

All error responses share a consistent shape, produced by `ExceptionHandlingMiddleware`:

```json
{
  "title": "Entity \"Product\" with key (42) was not found.",
  "status": 404,
  "traceId": "0HN...",
  "errors": null
}
```

## Running Locally

### Option A — Docker Compose (recommended)

```bash
docker compose up --build
```

This starts SQL Server and the API together; the API applies EF Core migrations on startup
and is reachable at `http://localhost:8080/swagger`.

### Option B — .NET SDK directly

```bash
# Start a local SQL Server instance (or point ConnectionStrings:DefaultConnection at one)
cd src/API
dotnet run
```

Swagger UI is available at `https://localhost:{port}/swagger` in the Development environment.

## Running Tests

```bash
dotnet test
```

- **Application.Tests** — mocks `IUnitOfWork`/repositories to unit test `ProductService` business rules (not-found handling, mapping, pagination metadata).
- **Infrastructure.Tests** — exercises `ProductRepository` against the EF Core InMemory provider (paging, search filter, eager-loading items).
- **API.Tests** — boots the full app via `WebApplicationFactory<Program>` (swapping SQL Server for InMemory) to verify routing, status codes, and the auth requirement on write endpoints.

## Design Notes

- **Repository + Unit of Work** over `DbContext` directly, so `Application` never references EF Core.
- **Pagination** is enforced on every collection endpoint (`PaginationQuery` caps `PageSize` at 100) and reads use `AsNoTracking()`.
- **FluentValidation** validators run automatically via `AddFluentValidationAutoValidation()`; failures are normalized to a single error shape by `ValidationFilter`/`ExceptionHandlingMiddleware`.
- **API versioning** is wired up via `Asp.Versioning` (`api/v1/...`) so future breaking changes can ship as `v2` alongside `v1`.
- **JWT**: short-lived access tokens (15 min default) + opaque refresh tokens. The sample keeps refresh-token issuance stateless for simplicity — a production system should persist and rotate/revoke refresh tokens server-side.
- **CORS**, **response compression**, **HTTPS redirection**, and **structured request logging** (Serilog) are configured in `Program.cs`.

## Known Gaps / Next Steps

- No persisted user store — `AuthController` is a stub for demoing the JWT flow.
- EF Core migrations aren't included (see *Database Schema* above) since this environment has no .NET SDK/NuGet access; they're a one-command step once you have both.
- Refresh tokens aren't persisted/revocable; add a `RefreshTokens` table for that in a real deployment.
- Rate limiting and API key throttling aren't included but would slot into the `Program.cs` middleware pipeline (`app.UseRateLimiter()`) if needed.
