# Phase 6 — ComfyUI Ingestor

## Overview

The ComfyUI Ingestor is a .NET worker service that discovers and ingests ComfyUI workflow files from configured directories into a PostgreSQL database. It provides idempotent upserts, structured logging, and tracks ingestion runs.

## Architecture

### Components

1. **UMLMM.Core**: Core domain models and EF Core DbContext
   - Models: `Workflow`, `Artifact`, `FetchRun`
   - Database: PostgreSQL with Npgsql
   - Schema: snake_case column names, JSONB for graph storage

2. **UMLMM.ComfyUIIngestor**: Worker service for workflow ingestion
   - Hosted service with configurable intervals
   - Workflow discovery with include/exclude patterns
   - JSON parsing with System.Text.Json
   - Idempotent upserts keyed by (source_id, external_id)
   - Serilog structured logging

3. **UMLMM.ComfyUIIngestor.Tests**: Unit and integration tests
   - Unit tests: Workflow parsing and mapping
   - Integration tests: Testcontainers PostgreSQL for upsert/idempotency
   - Test fixtures: Sample ComfyUI workflow JSONs

## Configuration

Configuration is done via `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=umlmm;Username=postgres;Password=postgres"
  },
  "ComfyUIIngestor": {
    "BaseDirectories": [
      "/path/to/comfyui/workflows"
    ],
    "IncludePatterns": [
      "*.json"
    ],
    "ExcludePatterns": [
      "temp",
      ".backup"
    ],
    "IntervalSeconds": 3600,
    "SourceId": "comfyui"
  }
}
```

### Configuration Options

- **BaseDirectories**: List of directories to search for workflow files
- **IncludePatterns**: File patterns to include (e.g., `*.json`)
- **ExcludePatterns**: Patterns to exclude from results
- **IntervalSeconds**: How often to run ingestion (default: 3600 = 1 hour)
- **SourceId**: Source identifier for workflows (default: "comfyui")

## Database Schema

### workflows

| Column | Type | Description |
|--------|------|-------------|
| id | int | Primary key |
| source_id | varchar(100) | Source identifier |
| external_id | varchar(500) | File name or external identifier |
| name | varchar(500) | Workflow name |
| description | text | Workflow description |
| graph_jsonb | jsonb | Full workflow graph as JSONB |
| nodes_count | int | Number of nodes in the workflow |
| created_at | timestamp | Creation timestamp |
| updated_at | timestamp | Last update timestamp |

**Unique constraint**: (source_id, external_id)

### artifacts

| Column | Type | Description |
|--------|------|-------------|
| id | int | Primary key |
| source_id | varchar(100) | Source identifier |
| external_id | varchar(500) | External identifier |
| name | varchar(500) | Artifact name |
| type | varchar(100) | Artifact type |
| path | varchar(1000) | File path |
| created_at | timestamp | Creation timestamp |
| updated_at | timestamp | Last update timestamp |

**Unique constraint**: (source_id, external_id)

### fetch_runs

| Column | Type | Description |
|--------|------|-------------|
| id | int | Primary key |
| source_id | varchar(100) | Source identifier |
| run_id | varchar(100) | Unique run identifier (GUID) |
| started_at | timestamp | Run start time |
| completed_at | timestamp | Run completion time |
| created_count | int | Number of records created |
| updated_count | int | Number of records updated |
| no_op_count | int | Number of unchanged records |
| error_count | int | Number of errors |
| error_details | text | Error details if any |

**Unique constraint**: (source_id, run_id)

## Workflow File Formats

The ingestor supports two ComfyUI workflow formats:

### Format 1: Object with numeric keys

```json
{
  "1": {
    "class_type": "CLIPTextEncode",
    "inputs": { ... }
  },
  "2": {
    "class_type": "KSampler",
    "inputs": { ... }
  },
  "extra": {
    "ds": {
      "workflow_name": "My Workflow"
    }
  }
}
```

### Format 2: Array of nodes

```json
{
  "nodes": [
    {
      "id": 1,
      "type": "LoadImage",
      "properties": { ... }
    },
    {
      "id": 2,
      "type": "Upscale",
      "properties": { ... }
    }
  ]
}
```

## Running the Ingestor

### Prerequisites

- .NET 9.0 SDK
- PostgreSQL 12+
- Docker (for running tests)

### Building

```bash
dotnet build
```

### Running Tests

```bash
# All tests
dotnet test

# Unit tests only
dotnet test --filter "FullyQualifiedName~Unit"

# Integration tests only
dotnet test --filter "FullyQualifiedName~Integration"
```

### Running the Worker

```bash
cd src/UMLMM.ComfyUIIngestor
dotnet run
```

The worker will:
1. Connect to PostgreSQL and ensure the database schema exists
2. Discover workflow files from configured directories
3. Parse and ingest workflows with idempotent upserts
4. Record fetch run statistics
5. Wait for the configured interval and repeat

## Logging

The ingestor uses Serilog for structured logging. Logs include:

- Run start/completion with run ID and source
- Files discovered
- Workflow creation/update/no-op operations
- Errors during processing
- Per-run statistics (created/updated/no-op/errors)

Example log output:

```
[INF] Starting workflow ingestion run abc123-... for source comfyui
[INF] Discovered 5 workflow files
[INF] Created workflow workflow1.json
[INF] Updated workflow workflow2.json
[DBG] No changes for workflow workflow3.json
[INF] Ingestion run abc123-... completed: Created=1, Updated=1, NoOp=3, Errors=0
```

## Idempotency

The ingestor implements idempotent upserts:

1. **Create**: If a workflow with (source_id, external_id) doesn't exist, it's created
2. **Update**: If it exists and content has changed, it's updated
3. **No-op**: If it exists and content is unchanged, no database modification occurs

This ensures re-running the ingestor multiple times produces consistent results without duplicates.

## Testing

### Unit Tests

- `WorkflowParserTests`: Tests workflow parsing logic
  - Parses object-based workflows with numeric keys
  - Parses array-based workflows
  - Extracts metadata (name, description)
  - Counts nodes correctly
  - Sets timestamps

### Integration Tests

- `WorkflowIngestServiceTests`: Tests full ingestion pipeline
  - Creates new workflows
  - Implements idempotent behavior
  - Records fetch runs
  - Updates modified workflows

Uses Testcontainers to spin up PostgreSQL containers for isolated testing.

## Future Enhancements

- Artifact association: Link workflows to referenced models/artifacts
- Enhanced metadata extraction from workflow graphs
- Workflow validation and schema checking
- Performance optimizations for large workflow sets
- REST API for querying workflows
- Blazor UI for browsing workflows
