# UMLMM

Unified Model/Media Metadata — ingestion (CivitAI, Danbooru, e621, ComfyUI, Ollama), centralized PostgreSQL, orchestration, and Blazor Server admin UI.

## Phase 5 — e621 Ingestor

A production-ready .NET worker service that ingests posts from e621.net API into PostgreSQL with:

- **Idempotent upserts** - Safe to re-run without creating duplicates
- **Resilient HTTP client** - Polly retry policies with exponential backoff and circuit breaker
- **Rate limiting** - Respects e621 API guidelines (1 req/sec minimum)
- **Structured logging** - Serilog with enriched context
- **Comprehensive tests** - 16 passing tests (10 unit + 6 integration with Testcontainers)

### Quick Start

1. **Prerequisites**
   - .NET 9 SDK
   - PostgreSQL 12+ (or Docker)
   - Docker (for running tests)

2. **Configuration**

   Update `src/UMLMM.E621Ingestor/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=umlmm;Username=postgres;Password=postgres"
     },
     "E621": {
       "UserAgent": "UMLMM/1.0 (by your_e621_username on e621)",
       "PageSize": 100,
       "TagFilter": null,
       "RateLimitDelayMs": 1000
     }
   }
   ```

3. **Run PostgreSQL**
   ```bash
   docker run -d --name postgres -p 5432:5432 -e POSTGRES_PASSWORD=postgres postgres:16-alpine
   ```

4. **Run the Ingestor**
   ```bash
   cd src/UMLMM.E621Ingestor
   dotnet run
   ```

5. **Run Tests**
   ```bash
   dotnet test
   ```

### Architecture

- **UMLMM.Core** - Domain entities (Post, Tag, Image, Source, FetchRun)
- **UMLMM.Data** - EF Core data access with PostgreSQL
- **UMLMM.E621Ingestor** - Worker service with API client, mapper, and orchestration

### Database Schema

Key entities:
- **Sources** - API sources (e621, danbooru, etc.)
- **Posts** - Indexed by (source_id, external_id)
- **Tags** - Shared across posts with category
- **Images** - URLs, dimensions, file info, SHA256
- **FetchRuns** - Audit trail with stats and timing

### Documentation

See [docs/phase-05-e621-ingestor.md](docs/phase-05-e621-ingestor.md) for detailed documentation.

### Features

✅ Idempotent re-runs (no duplicate rows)  
✅ Respects e621 API rules and handles rate limits  
✅ Retry with exponential backoff + jitter  
✅ Circuit breaker pattern  
✅ Structured logging with Serilog  
✅ Comprehensive test coverage  
✅ EF Core with PostgreSQL  
✅ Proper indexes for performance  

### Testing

```bash
# Unit tests (fast)
dotnet test --filter "FullyQualifiedName~Unit"

# Integration tests (requires Docker)
dotnet test --filter "FullyQualifiedName~Integration"

# All tests
dotnet test
```

### License

See LICENSE file for details.
