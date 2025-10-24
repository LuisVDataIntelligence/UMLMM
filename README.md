# UMLMM
Unified Model/Media Metadata — ingestion (CivitAI, Danbooru, e621, ComfyUI, Ollama), centralized PostgreSQL, orchestration, and Blazor Server admin UI.

## Phase 2 — Database and Shared Library (EF Core + PostgreSQL)

This phase implements the central PostgreSQL schema and shared .NET class library with EF Core entities, DbContext, and migrations.

### Project Structure

```
src/
  UMLMM.Domain/              # Domain entities, enums, and value objects
  UMLMM.Infrastructure/      # EF Core DbContext, entity configurations, and migrations
tests/
  UMLMM.Domain.Tests/        # Unit tests for domain entities
  UMLMM.Infrastructure.Tests/ # Integration tests with Testcontainers
tools/
  UMLMM.DbTool/              # Console tool for database migrations
```

### Database Schema

The schema includes the following normalized tables:

- **sources** - Source systems (CivitAI, Danbooru, etc.)
- **models** - AI models with metadata and raw JSONB data
- **model_versions** - Model versions with JSONB metadata
- **tags** - Tags with normalization support
- **model_tags** - Many-to-many relationship between models and tags
- **artifacts** - Model artifacts (files, configs, etc.)
- **images** - Images associated with models/versions
- **workflows** - ComfyUI workflows with JSONB graph data
- **prompts** - Prompts associated with models
- **fetch_runs** - Metadata about ingestion runs

All tables use:
- Snake_case naming for PostgreSQL compatibility
- JSONB columns for flexible metadata storage
- Unique constraints for idempotent upserts
- Proper indexes (btree on FKs, GIN on JSONB, hash on sha256)
- UTC timestamps (timestamptz)

### Getting Started

#### Prerequisites

- .NET 9.0 SDK
- PostgreSQL 14+ (or use Docker)

#### Build

```bash
dotnet build
```

#### Run Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/UMLMM.Domain.Tests
dotnet test tests/UMLMM.Infrastructure.Tests
```

#### Database Tool

The `UMLMM.DbTool` console application provides commands for managing database migrations:

```bash
cd tools/UMLMM.DbTool
dotnet run -- migrate     # Apply pending migrations
dotnet run -- check       # Check migration status
dotnet run -- ensure      # Ensure database exists
dotnet run -- drop        # Drop database (requires confirmation)
```

Connection string can be configured via:
- `appsettings.json`
- `DATABASE_URL` environment variable
- Default: `Host=localhost;Database=umlmm;Username=postgres;Password=postgres`

#### Using with Docker (PostgreSQL)

```bash
# Start PostgreSQL
docker run -d \
  --name umlmm-postgres \
  -e POSTGRES_DB=umlmm \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -p 5432:5432 \
  postgres:16-alpine

# Apply migrations
cd tools/UMLMM.DbTool
dotnet run -- migrate
```

### Entity Framework Migrations

To create a new migration:

```bash
cd src/UMLMM.Infrastructure
dotnet ef migrations add YourMigrationName
```

To apply migrations programmatically:

```csharp
using Microsoft.EntityFrameworkCore;
using UMLMM.Infrastructure.Persistence;

var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseNpgsql("your-connection-string")
    .Options;

await using var context = new AppDbContext(options);
await context.Database.MigrateAsync();
```

### Idempotent Upserts

The schema enforces unique constraints to support idempotent operations:

- Models: unique on `(source_id, external_id)`
- Workflows: unique on `(source_id, external_id)`
- Tags: unique on `(normalized_name, source_id)`
- Artifacts/Images: sha256 hashing for deduplication

### Testing

- **Unit Tests**: Validate domain entity properties and behavior
- **Integration Tests**: Use Testcontainers to test migrations, constraints, and idempotency
- **Performance Tests**: Verify batch operations (100-1000 records) perform acceptably

### Quality Standards

- ✅ All migrations apply successfully
- ✅ Unique constraints prevent duplicates
- ✅ JSONB columns store and retrieve data correctly
- ✅ Foreign key relationships enforce referential integrity
- ✅ Batch operations complete in reasonable time
- ✅ All tests pass in CI

### Next Steps

Phase 3 will implement:
- Ingestion services for each source (CivitAI, Danbooru, etc.)
- Background job orchestration
- Blazor Server admin UI
