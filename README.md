# UMLMM
Unified Model/Media Metadata — ingestion (CivitAI, Danbooru, e621, ComfyUI, Ollama), centralized PostgreSQL, orchestration, and Blazor Server admin UI.

## Project Structure

```
UMLMM/
├── src/
│   ├── UMLMM.Core/           # Domain entities
│   ├── UMLMM.Infrastructure/ # Data access layer (EF Core)
│   └── OllamaIngestor/       # Ollama ingestion worker service
├── tests/
│   └── UMLMM.Tests/          # Unit and integration tests
└── docs/
    └── phase-07-ollama-ingestor.md
```

## Current Status

✅ **Phase 7 - Ollama Ingestor** - Implemented
- Worker service for ingesting Ollama models
- EF Core with PostgreSQL
- Idempotent upserts
- Structured logging with Serilog
- Unit and integration tests with Testcontainers

## Quick Start

### Prerequisites

- .NET 9 SDK
- PostgreSQL (or use Docker Compose)
- Docker (for Testcontainers in tests)
- Ollama (optional, for actual ingestion)

### Run with Docker Compose

```bash
docker-compose up -d
```

This starts:
- PostgreSQL database
- OllamaIngestor worker service

### Run Locally

1. Start PostgreSQL:
   ```bash
   docker run -d \
     --name postgres \
     -e POSTGRES_DB=umlmm \
     -e POSTGRES_USER=postgres \
     -e POSTGRES_PASSWORD=postgres \
     -p 5432:5432 \
     postgres:17-alpine
   ```

2. Run the ingestor:
   ```bash
   cd src/OllamaIngestor
   dotnet run
   ```

### Run Tests

```bash
dotnet test
```

Tests use Testcontainers to spin up PostgreSQL automatically.

## Configuration

See `src/OllamaIngestor/appsettings.json` for configuration options:

- `ConnectionStrings:DefaultConnection` - PostgreSQL connection string
- `Ollama:CommandPath` - Path to Ollama CLI (default: `ollama`)
- `Ollama:IntervalMinutes` - Ingestion interval (default: 60)
- `Ollama:RunOnStartup` - Run on startup (default: true)

## Documentation

- [Phase 7 - Ollama Ingestor](docs/phase-07-ollama-ingestor.md) - Detailed documentation

## Features

- **Idempotent Ingestion**: Re-running yields consistent results
- **Model Discovery**: Lists and parses locally installed Ollama models
- **Manifest Parsing**: Extracts model parameters, parent models, etc.
- **JSONB Metadata**: Flexible storage for model-specific data
- **Fetch Run Tracking**: Records counts, timings, and status
- **Structured Logging**: Serilog with enrichment

## License

MIT
