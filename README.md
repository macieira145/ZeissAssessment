# ZeissAssessment

A .NET 10 Web API for managing a `Product` catalog, including CRUD operations, search/filtering, and stock-level management (increment/decrement with domain-enforced invariants).

## 1. Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) (used to run the SQL Server dependency)

### Running the database

The API depends on a SQL Server instance, provided via Docker Compose. From the repository root:

```bash
docker compose up -d
```

This starts a `mcr.microsoft.com/mssql/server:2025-latest` container (`zeiss-assessment`) listening on `localhost:1433`, with credentials matching the default connection string in `appsettings.Development.json`. No manual database creation is required — see [Migrations and seeding](#migrations-and-seeding) below.

### Running the API

```bash
dotnet run --project src/ZeissAssessment.Api
```

By default this runs in the `Development` environment (via `launchSettings.json`), which:

- Applies EF Core migrations and seeds sample data automatically on startup (see below).
- Serves Swagger UI at the application root (`/`).

If running outside of `Development`, ensure a valid `ConnectionStrings:DefaultConnection` value is supplied via `appsettings.json`, environment variables, or user secrets — `appsettings.json` ships with this value empty intentionally, since only local/dev configuration should carry a real connection string in source control.

### Migrations and seeding

On startup, `app.ApplyMigrationsAndSeedAsync()` (`src/ZeissAssessment.Infrastructure/Extensions`) applies any pending EF Core migrations and, if the `Products` table is empty, seeds it with 80 generated products (see [Data seeding](#data-seeding)). There is no separate manual migration step to run — starting the API against a fresh database is sufficient.

### Running the tests

```bash
dotnet test
```

This runs both the unit test suite (`tests/ZeissAssessment.UnitTests`, NUnit + Moq + Shouldly) and the integration test suite (`tests/ZeissAssessment.IntegrationTests`, using `Testcontainers.MsSql` to spin up an ephemeral, isolated SQL Server container per test run — no manual setup needed, only Docker running).

### API collections

Postman collection and environment files are provided under `postman/` for manual exploration of the API surface.

## 2. Architecture and conventions

### Project structure

The solution follows **Clean Architecture**, with dependencies pointing inward:

```
src/
  ZeissAssessment.Domain          – Entities, value objects, domain exceptions. No dependencies on other layers.
  ZeissAssessment.Application     – Use-case orchestration: services, contracts (DTOs), mappers, filters, interfaces. Depends on Domain only.
  ZeissAssessment.Infrastructure  – EF Core DbContext, repositories, migrations, seeders, configuration. Depends on Application + Domain.
  ZeissAssessment.Api             – ASP.NET Core host: controllers, middleware, filters, composition root (Program.cs). Depends on all layers.
tests/
  ZeissAssessment.UnitTests        – Domain and Application logic in isolation (Moq for dependencies).
  ZeissAssessment.IntegrationTests – Full HTTP pipeline against a real, ephemeral SQL Server (Testcontainers).
  ZeissAssessment.TestCommon       – Shared test builders.
```

Each layer only references the layers inside it (`Api` → `Infrastructure`/`Application`/`Domain`; `Infrastructure` → `Application`/`Domain`; `Application` → `Domain`), and each exposes its own `DependencyInjection.cs` (`AddInfrastructure`, `AddApplication`) so `Program.cs` stays a thin composition root.

### Domain-Driven Design and Clean Architecture

The `Domain` layer models `Product` as an entity whose invariants are enforced by construction, not by callers remembering to check them. Stock quantity, in particular, is modeled as a `Stock` value object (`Domain/ValueObjects/Stock.cs`) rather than a bare `int` on `Product`: it cannot be constructed with a negative quantity, and its `Increment`/`Decrement` operations are the *only* way to change it, throwing domain-specific exceptions (`InvalidStockQuantityException`, `InsufficientStockException`) when an operation would violate an invariant. This guarantees stock can never go negative regardless of which code path mutates it — the Application layer's `ProductService` and any future caller inherit that guarantee for free, instead of re-implementing the check.

Clean Architecture's dependency rule (outer layers depend on inner layers, never the reverse) keeps this domain model persistence- and framework-ignorant: `Domain` has no reference to EF Core, ASP.NET Core, or any other infrastructure concern, so business rules can be unit tested without a database and can survive infrastructure changes (e.g., swapping SQL Server for another store) without modification. This separation is intentionally proportional to the size of this project, a single entity with clear invariants, while still demonstrating the boundary discipline that scales to a larger domain model.

### Why services instead of CQRS

This project uses a Clean Architecture layout (Domain / Application / Infrastructure / Api) with a service-based structure (`IProductService`) rather than CQRS with separate command/query handlers. Given the scope of this assessment — a single entity with a handful of CRUD and stock-management operations, a full command/query split would have added structural overhead without a corresponding benefit: there's no divergence yet between read and write models that would justify separating them. Domain rules (e.g., stock can never go negative) are enforced in a `Stock` value object so that logic stays in one place regardless of which service method touches it. If this API's scope grew, more complex read projections, different scaling needs for reads vs. writes, or a larger team working across the codebase, CQRS would become the more appropriate choice, and the service layer here is structured so that migration path stays straightforward.

### Typed connection string binding instead of the Options pattern

Instead of reading the connection string via a raw string key (`configuration.GetConnectionString("DefaultConnection")`), the app binds the `ConnectionStrings` configuration section to a small typed class (`Infrastructure/Persistence/ConnectionStrings.cs`) and reads `DefaultConnection` as a compiled property. This avoids a class of bugs where a typo in the config key string compiles fine but silently returns `null` at runtime, with a typed property, a typo instead breaks the build. The full ASP.NET Core Options pattern (`IOptions<T>` + `services.Configure<T>`) was considered but not used, since the connection string is only ever consumed in one place (`AddInfrastructure`'s `AddDbContext` registration); the Options pattern's benefits, DI-injectable settings elsewhere in the app, live-reload support via `IOptionsMonitor` aren't needed here and would add unjustified  for this scope.

### Product ID generation

For unique product ID generation, three approaches were evaluated: a purely random 6-digit number backed by a database unique constraint and retry-on-conflict, a timestamp-derived hybrid combining truncated ticks with a small random suffix, and a database sequence formatted into a 6-digit value.

The timestamp hybrid was particularly appealing under high concurrency: because ticks change every 100 nanoseconds, two requests only risk collision if they land within the same tick window, and even then the 2-digit random suffix keeps the in-window collision probability low. This makes it a strong candidate specifically when concurrent writes are the dominant risk, since it degrades gracefully rather than accumulating collision risk with total record count the way pure random does. However, it still requires the same unique constraint and retry loop as pure random generation, since neither approach guarantees uniqueness outright — it only reduces how often that retry path is exercised, not whether it's needed.

A database sequence (`dbo.ProductIdSequence`, see the `AddedIdSequenceOnProductInsert` and `ProductIdSequenceRange` migrations and `ProductConfiguration.cs`) was chosen instead because it guarantees uniqueness by construction, requires no retry logic or exception handling, and is naturally safe across concurrent instances since the database itself serializes sequence value assignment. This keeps the implementation simpler and more robust than any probabilistic alternative. The sequence is bounded to `[100000, 999999]` (`.HasMin`/`.HasMax`, starting at `100000`) so every generated ID is a 6-digit number, with a `CK_Product_Id_Range` check constraint on `Products.Id` as a backstop against that invariant being bypassed.

### Data seeding

For seeding test data, [Bogus](https://github.com/bchavez/Bogus) was chosen over manually hand-writing seed records. Manually authored seed data tends to be repetitive, low-cardinality (the same three product names copy-pasted with minor edits), and easy to let drift out of sync as the `Product` entity evolves — every new field means manually updating every hardcoded record. Bogus generates realistic, varied data (commerce-appropriate names and prices, randomized stock levels) while still routing every instance through the domain's `Product` construction and `Stock` value object via `CustomInstantiator` (`Infrastructure/Persistence/Seeders/Fakers/ProductFaker.cs`), so all domain invariants are enforced exactly as they would be for data created through the API itself. This keeps the seeder both more representative of real-world data distribution (useful for exercising the `search`, `stock-level`, and stock adjustment endpoints meaningfully) and more maintainable, without introducing the added complexity of reflection-based generators like AutoFixture/AutoBogus, which aren't warranted for a single, simple entity.

### Other conventions

- **Mapping**: [Mapperly](https://mapperly.riok.app/) is used for compile-time-generated mapping between domain entities and DTOs (`Application/Mappers`), avoiding the runtime reflection cost and hidden misconfiguration risk of a reflection-based mapper.
- **Error handling**: Application-layer exceptions (`NotFoundException`, `ConflictException`, `ValidationException`, `PersistenceException`) derive from a common `AppException` carrying an `ErrorCode`, and are translated to RFC 7807 `ProblemDetails` responses by a global exception handler (`Api/Middleware/GlobalExceptionHandler.cs`), keeping error-response formatting out of individual controller actions.
- **Validation**: request models are validated via a `ValidateModelFilter` action filter, with `SuppressModelStateInvalidFilter` set so the filter — not the default ASP.NET Core behavior — controls the response shape.
- **Logging**: structured logging via Serilog, configured from `appsettings.json` (compact JSON in production-like environments, human-readable console output in Development), enriched with machine name and environment.
- **Persistence**: repository + unit-of-work pattern (`IProductRepository`, `IUnitOfWork`) abstracts EF Core from the Application layer, and `Stock` is persisted as an EF Core owned type rather than a separate table, since it has no independent identity or lifecycle from its owning `Product`.
- **Testing**: unit tests isolate Domain/Application logic with mocked dependencies; integration tests exercise the full HTTP pipeline against a real, disposable SQL Server instance via Testcontainers, so behavior is verified against the actual database engine (including the ID sequence and stock constraints) rather than an in-memory substitute.
