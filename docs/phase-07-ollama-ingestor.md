# Phase 7 — Ollama Ingestor (Models/Manifests + Upserts)

## Overview

The Ollama Ingestor is a .NET Worker service that periodically reads locally installed Ollama models and their manifests, mapping them to normalized entities with idempotent upserts to a PostgreSQL database.

## Architecture

### Components

1. **UMLMM.Core** - Domain entities
   - `Source` - Ingestion source (e.g., Ollama)
   - `Model` - AI model representation
   - `ModelVersion` - Version/tag of a model
   - `ModelArtifact` - Files/layers associated with a model version
   - `FetchRun` - Tracks ingestion runs with counts and timings

2. **UMLMM.Infrastructure** - Data access layer
   - `UmlmmDbContext` - EF Core DbContext with PostgreSQL
   - `ModelRepository` - Repository with idempotent upsert operations
   - Migrations for database schema

3. **OllamaIngestor** - Worker service
   - `OllamaCliClient` - Interacts with Ollama CLI
   - `OllamaIngestionService` - Parses and ingests model data
   - `Worker` - Background service for periodic execution

## Features

### Model Discovery
- Lists all locally installed Ollama models using `ollama list`
- Retrieves detailed information using `ollama show`
- Parses model manifests (Modelfile) to extract:
  - Model name and tag
  - Parent/base model (FROM line)
  - Model parameters (temperature, context_length, etc.)
  - Templates and other metadata

### Data Persistence
- **Idempotent Upserts**: Re-running the ingestor yields consistent results
- **Unique Constraints**: Models/versions keyed by `(source_id, external_id)`
- **JSONB Metadata**: Flexible storage for model-specific data
- **Fetch Run Tracking**: Records counts, timings, and status

### Logging
- Structured logging with Serilog
- Enriched with:
  - RunId for correlation
  - Source information
  - Machine name and environment
- Console output with timestamps

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=umlmm;Username=postgres;Password=postgres"
  },
  "Ollama": {
    "CommandPath": "ollama",
    "IntervalMinutes": 60,
    "RunOnStartup": true
  }
}
```

### Environment Variables

- `ConnectionStrings__DefaultConnection` - PostgreSQL connection string
- `Ollama__CommandPath` - Path to Ollama CLI (default: `ollama`)
- `Ollama__IntervalMinutes` - Ingestion interval in minutes (default: 60)
- `Ollama__RunOnStartup` - Run ingestion on startup (default: true)

## Database Schema

### Tables

- `sources` - Ingestion sources
- `models` - AI models
- `model_versions` - Model versions/tags
- `model_artifacts` - Model files/layers
- `fetch_runs` - Ingestion run tracking

### Indexes

- Unique index on `sources.name`
- Unique index on `models.(source_id, external_id)`
- Unique index on `model_versions.(model_id, tag)`
- Unique index on `model_versions.external_id`
- Unique index on `fetch_runs.run_id`
- Index on `model_artifacts.digest`
- Index on `fetch_runs.started_at`

## Running the Ingestor

### Prerequisites

1. .NET 9 SDK
2. PostgreSQL 12+ (or use Docker)
3. Ollama installed with models

### Setup Database

The ingestor automatically runs migrations on startup. Alternatively:

```bash
cd src/UMLMM.Infrastructure
dotnet ef database update --startup-project ../OllamaIngestor/OllamaIngestor.csproj
```

### Run with Docker Compose (Recommended)

```bash
docker-compose up -d
```

### Run Manually

```bash
cd src/OllamaIngestor
dotnet run
```

## Testing

### Unit Tests

Located in `tests/UMLMM.Tests/Unit/`:
- Model name parsing
- Modelfile parent extraction
- Ingestion service logic

```bash
dotnet test --filter "FullyQualifiedName~Unit"
```

### Integration Tests

Located in `tests/UMLMM.Tests/Integration/`:
- Repository upsert idempotency
- JSONB metadata storage
- Fetch run tracking
- Uses Testcontainers for PostgreSQL

```bash
dotnet test --filter "FullyQualifiedName~Integration"
```

### Run All Tests

```bash
dotnet test
```

## Example Workflow

1. **Startup**
   - Worker starts and connects to PostgreSQL
   - Runs migrations if needed
   - Creates/updates "Ollama" source

2. **First Run**
   - Generates unique RunId (e.g., `a1b2c3d4-...`)
   - Creates FetchRun record with status "running"
   - Lists all Ollama models via CLI
   - For each model:
     - Parses name and tag (e.g., `llama2:7b` → name=`llama2`, tag=`7b`)
     - Retrieves detailed manifest
     - Extracts parent model from Modelfile
     - Upserts Model entity
     - Upserts ModelVersion entity with parameters
     - Upserts ModelArtifact for modelfile
   - Updates FetchRun with counts and status "completed"

3. **Subsequent Runs**
   - Same process, but upserts update existing records
   - No duplicates created
   - Idempotent operation

4. **Periodic Execution**
   - Runs every N minutes (configurable)
   - Each run gets a new RunId for tracking

## Idempotency

The ingestor is designed to be idempotent:

- **Sources**: Keyed by name, description updated on re-run
- **Models**: Keyed by (source_id, external_id), metadata updated
- **ModelVersions**: Keyed by (model_id, tag), parameters updated
- **ModelArtifacts**: Keyed by (version_id, type, digest), size/metadata updated
- **FetchRuns**: New record per run, no updates to completed runs

Re-running the ingestor multiple times will not create duplicate records.

## Monitoring

### Logs

Structured logs include:
- Ingestion start/completion
- Models/versions/artifacts processed
- Errors with full exception details
- RunId for correlation

Example:
```
[02:00:00 INF] Starting Ollama ingestion run a1b2c3d4-... {"MachineName": "host", "EnvironmentName": "Production"}
[02:00:01 INF] Found 5 Ollama models
[02:00:02 INF] Successfully ingested model llama2:7b (ModelId: 1, VersionId: 1)
[02:00:05 INF] Completed Ollama ingestion run a1b2c3d4-... Models: 5, Versions: 5, Artifacts: 5
```

### Database

Query `fetch_runs` table for historical data:

```sql
SELECT 
  run_id, 
  started_at, 
  completed_at, 
  status, 
  models_processed, 
  versions_processed, 
  artifacts_processed,
  error_message
FROM fetch_runs
ORDER BY started_at DESC
LIMIT 10;
```

## Troubleshooting

### Ollama not found

Ensure Ollama is installed and in PATH, or set `Ollama:CommandPath` in configuration.

### Database connection failed

Verify PostgreSQL is running and connection string is correct.

### Models not appearing

Check:
1. Ollama models are installed (`ollama list`)
2. Worker logs for errors
3. Database connection and migrations

### Duplicate records

This shouldn't happen due to unique constraints. If it does:
1. Check migration files
2. Verify unique indexes are in place
3. Review repository upsert logic

## Future Enhancements

- HTTP API support (in addition to CLI)
- Model download tracking
- Differential updates (only changed models)
- Webhook notifications
- Admin UI for monitoring
