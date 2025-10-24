# UMLMM

Unified Model/Media Metadata — ingestion (CivitAI, Danbooru, e621, ComfyUI, Ollama), centralized PostgreSQL, orchestration, and Blazor Server admin UI.

## Overview

UMLMM is a centralized metadata management system that ingests, normalizes, and stores metadata from multiple sources including:

- **Danbooru**: Anime image board with extensive tagging
- **e621**: Furry art repository (planned)
- **CivitAI**: AI model and checkpoint repository (planned)
- **ComfyUI**: Workflow metadata (planned)
- **Ollama**: LLM model metadata (planned)

## Features

### Phase 4 - Danbooru Ingestor (✅ Completed)

- **Resilient API Client**: Built with Polly for retry, circuit breaker, and timeout policies
- **Idempotent Upserts**: No duplicates on re-runs, tracked by (source_id, external_id)
- **Comprehensive Mapping**: Extracts posts, tags (with categories), ratings, and metadata
- **Statistical Tracking**: Each run records created/updated/no-op/error counts with timing
- **Structured Logging**: Serilog with enrichment for runId and source
- **Full Test Coverage**: Unit tests for mapping, integration tests for database operations

## Project Structure

```
UMLMM/
├── src/
│   ├── UMLMM.Domain/              # Domain entities (Image, Tag, FetchRun)
│   ├── UMLMM.Infrastructure/       # EF Core DbContext and data access
│   └── UMLMM.DanbooruIngestor/    # Danbooru worker service
├── tests/
│   ├── UMLMM.DanbooruIngestor.Tests/          # Unit tests
│   └── UMLMM.Infrastructure.IntegrationTests/  # Integration tests with Testcontainers
└── docs/
    ├── phase-04-danbooru-ingestor.md          # Detailed implementation docs
    └── AGENT-INSTRUCTIONS.md                   # Development guidelines
```

## Quick Start

### Prerequisites

- .NET 9 SDK
- PostgreSQL 16+ (or Docker)
- Docker (for integration tests)

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/LuisVDataIntelligence/UMLMM.git
   cd UMLMM
   ```

2. **Start PostgreSQL** (if not already running)
   ```bash
   docker run -d \
     --name umlmm-postgres \
     -e POSTGRES_DB=umlmm \
     -e POSTGRES_USER=postgres \
     -e POSTGRES_PASSWORD=postgres \
     -p 5432:5432 \
     postgres:16-alpine
   ```

3. **Configure the application**
   
   Edit `src/UMLMM.DanbooruIngestor/appsettings.json` or set environment variables:
   ```bash
   export ConnectionStrings__DefaultConnection="Host=localhost;Database=umlmm;Username=postgres;Password=postgres"
   export Danbooru__Tags="rating:safe"
   export Danbooru__MaxPages=2
   ```

4. **Run the Danbooru ingestor**
   ```bash
   dotnet run --project src/UMLMM.DanbooruIngestor
   ```

### Running Tests

```bash
# All tests
dotnet test

# Unit tests only
dotnet test tests/UMLMM.DanbooruIngestor.Tests

# Integration tests only (requires Docker)
dotnet test tests/UMLMM.Infrastructure.IntegrationTests
```

## Configuration

### Connection String

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=umlmm;Username=postgres;Password=postgres"
  }
}
```

### Danbooru Settings

```json
{
  "Danbooru": {
    "BaseUrl": "https://danbooru.donmai.us",
    "PageSize": 100,
    "MaxPages": 10,
    "Tags": "rating:safe",
    "TimeoutSeconds": 30,
    "RetryCount": 3,
    "CircuitBreakerThreshold": 5,
    "CircuitBreakerDurationSeconds": 60
  }
}
```

## Database Schema

### Images
- Unique index on (source_id, external_id)
- Tracks preview/original URLs, rating, SHA256
- JSONB metadata field for source-specific data

### Tags
- Unique index on name
- Category field (general, character, copyright, artist, meta)

### ImageTags
- Many-to-many relationship
- Composite primary key (image_id, tag_id)

### FetchRuns
- Tracks ingestion statistics
- JSONB parameters field for query tracking

## Technology Stack

- **.NET 9**: Latest framework features
- **Entity Framework Core 9**: PostgreSQL ORM
- **PostgreSQL 16**: Primary database
- **Polly 8**: Resilience patterns (retry, circuit breaker)
- **Serilog**: Structured logging
- **xUnit + FluentAssertions**: Testing
- **Testcontainers**: Integration testing

## Documentation

- [Phase 4 - Danbooru Ingestor](docs/phase-04-danbooru-ingestor.md)
- [Agent Instructions](docs/AGENT-INSTRUCTIONS.md)

## Roadmap

- [x] Phase 4: Danbooru Ingestor
- [ ] Phase 5: e621 Ingestor
- [ ] Phase 6: CivitAI Ingestor
- [ ] Phase 7: Admin UI (Blazor Server)
- [ ] Phase 8: Search and Query API
- [ ] Phase 9: Orchestration and Scheduling

## Contributing

See [AGENT-INSTRUCTIONS.md](docs/AGENT-INSTRUCTIONS.md) for development guidelines.

## License

[Add your license here]
