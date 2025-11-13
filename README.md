# UMLMM

Unified Model/Media Metadata — A system for ingesting, managing, and querying metadata from multiple AI model and media sources (CivitAI, Danbooru, e621, ComfyUI, Ollama) into a centralized PostgreSQL database.

## Overview

UMLMM provides:

- **Multi-source ingestion**: Fetch models, versions, files, images, and tags from external APIs
- **Centralized storage**: PostgreSQL database with normalized schema
- **Idempotent operations**: Upserts using natural keys (source_id, external_id) and content hashes
- **Resilient HTTP**: Polly policies for retry, timeout, and circuit breaking
- **Structured logging**: Serilog with console and file sinks
- **Resume capability**: Cursor-based pagination with run tracking

## Architecture

The solution is organized into three main layers:

### Domain Layer (`UMLMM.Domain`)
Contains core business entities:
- `Source` - External data sources (CivitAI, etc.)
- `Model` - AI models with metadata
- `ModelVersion` - Versions of models
- `Artifact` - Files (safetensors, checkpoints, etc.)
- `Image` - Preview images
- `Tag` - Normalized tags (lowercase kebab-case)
- `FetchRun` - Ingestion run tracking with statistics

### Infrastructure Layer (`UMLMM.Infrastructure`)
Provides data access and persistence:
- EF Core `DbContext` with Postgres provider
- Database migrations
- Snake_case column naming convention
- Indexes on natural keys and content hashes
- JSONB columns for raw API responses

### Ingestor Layer (`UMLMM.Ingestors.CivitAI`)
Orchestrates data fetching and mapping:
- CivitAI API client with resilience policies
- DTO to entity mapping
- Idempotent upsert logic
- Tag normalization
- Structured logging and metrics

## Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- PostgreSQL 16+ (or Docker for local development)
- Docker (for running integration tests)

### Database Setup

1. **Start PostgreSQL** (using Docker):
   ```bash
   docker run --name umlmm-postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres:16-alpine
   ```

2. **Update connection string** in `src/UMLMM.Ingestors.CivitAI/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=umlmm;Username=postgres;Password=postgres"
     }
   }
   ```

3. **Run migrations** (automatically applied on startup):
   ```bash
   cd src/UMLMM.Ingestors.CivitAI
   dotnet run
   ```

### Running the CivitAI Ingestor

```bash
cd src/UMLMM.Ingestors.CivitAI
dotnet run
```

#### Configuration

Configure via `appsettings.json` or environment variables:

```json
{
  "CivitAI": {
    "StartPage": 1,
    "PageSize": 100,
    "MaxPages": null,
    "ApiKey": null
  }
}
```

Environment variables (prefix with `UMLMM_`):
```bash
export UMLMM_CivitAI__StartPage=1
export UMLMM_CivitAI__MaxPages=5
export UMLMM_CivitAI__ApiKey=your-api-key-here
export UMLMM_ConnectionStrings__DefaultConnection="Host=localhost;Database=umlmm;..."
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/UMLMM.Ingestors.CivitAI.Tests

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

The integration tests use Testcontainers to spin up ephemeral PostgreSQL containers automatically.

## Database Schema

### Key Tables

- **sources** - Data source definitions
- **models** - AI models with unique constraint on (source_id, external_id)
- **model_versions** - Versions with unique constraint on (model_id, external_id)
- **artifacts** - Files with sha256 index for deduplication
- **images** - Preview images with sha256 index
- **tags** - Normalized tag names (unique)
- **model_tags** - Many-to-many join table
- **fetch_runs** - Ingestion run metadata and statistics

### Idempotency

The system ensures idempotent ingestion through:

1. **Natural key constraints**: `(source_id, external_id)` prevents duplicate models/versions/artifacts
2. **Content hash matching**: SHA256 hashes used to identify duplicate files
3. **Tag normalization**: Lowercase kebab-case ensures consistent tag matching
4. **Run tracking**: Cursor persistence enables resume on failure

## Resilience Policies

The HTTP client includes Polly policies:

- **Retry**: 3 attempts with exponential backoff + jitter for transient failures
- **Circuit Breaker**: Opens after 5 consecutive failures, breaks for 30s
- **Timeout**: 30s per request with optimistic cancellation

## Logging

Serilog outputs to:
- **Console**: Colored, structured logs
- **File**: `logs/civitai-ingestor-YYYYMMDD.log` (rolling daily)

Log context includes:
- Source context (class name)
- Timestamps
- Log levels
- Structured properties

## Development

### Building

```bash
dotnet build
```

### Code Formatting

```bash
dotnet format
```

### Adding Migrations

```bash
cd src/UMLMM.Infrastructure
dotnet ef migrations add <MigrationName> --output-dir Data/Migrations
```

## CI/CD

GitHub Actions workflow (`.github/workflows/dotnet.yml`) runs on push/PR:
- Build
- Run tests (including Testcontainers integration tests)
- Check code formatting

## Future Enhancements

- Additional ingestors (Danbooru, e621, ComfyUI, Ollama)
- Blazor Server admin UI for browsing and managing data
- GraphQL API for querying
- Background job scheduling with Hangfire
- Advanced search with full-text indexing
- Model comparison and recommendation features

## License

[Specify your license here]

## Contributing

[Contribution guidelines]

