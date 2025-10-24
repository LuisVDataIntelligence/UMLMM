# UMLMM

Unified Model/Media Metadata — ingestion (CivitAI, Danbooru, e621, ComfyUI, Ollama), centralized PostgreSQL, orchestration, and Blazor Server admin UI.

## Overview

UMLMM is a system for ingesting, managing, and querying metadata from various AI model and media sources. Phase 6 implements the ComfyUI workflow ingestor.

## Features

### Phase 6: ComfyUI Ingestor

- ✅ Discovers and parses ComfyUI workflow files (JSON graphs)
- ✅ Stores workflows in PostgreSQL with JSONB graph storage
- ✅ Idempotent upserts keyed by (source_id, external_id)
- ✅ Tracks ingestion runs with statistics (created/updated/no-op/errors)
- ✅ Configurable directory scanning with include/exclude patterns
- ✅ Structured logging with Serilog
- ✅ Unit and integration tests with Testcontainers

## Architecture

```
UMLMM/
├── src/
│   ├── UMLMM.Core/              # Domain models and EF Core DbContext
│   └── UMLMM.ComfyUIIngestor/   # Worker service for workflow ingestion
├── tests/
│   └── UMLMM.ComfyUIIngestor.Tests/  # Unit and integration tests
└── docs/
    └── phase-06-comfyui-ingestor.md  # Detailed documentation
```

## Quick Start

### Prerequisites

- .NET 9.0 SDK
- PostgreSQL 12+
- Docker (for running tests)

### Build

```bash
dotnet build
```

### Run Tests

```bash
# All tests
dotnet test

# Unit tests only
dotnet test --filter "FullyQualifiedName~Unit"

# Integration tests only
dotnet test --filter "FullyQualifiedName~Integration"
```

### Configure

Edit `src/UMLMM.ComfyUIIngestor/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=umlmm;Username=postgres;Password=postgres"
  },
  "ComfyUIIngestor": {
    "BaseDirectories": [
      "/path/to/comfyui/workflows"
    ],
    "IncludePatterns": ["*.json"],
    "ExcludePatterns": ["temp", ".backup"],
    "IntervalSeconds": 3600,
    "SourceId": "comfyui"
  }
}
```

### Run

```bash
cd src/UMLMM.ComfyUIIngestor
dotnet run
```

## Documentation

See [docs/phase-06-comfyui-ingestor.md](docs/phase-06-comfyui-ingestor.md) for detailed documentation.

## License

MIT
