# Phase 4 — Danbooru Ingestor

## Overview

This phase implements a resilient worker service that fetches posts and tags from the Danbooru API, maps them to normalized entities, and persists them to PostgreSQL with idempotent upserts.

## Architecture

### Components

1. **Domain Layer** (`UMLMM.Domain`)
   - `Image`: Represents an image with source tracking and metadata
   - `Tag`: Represents tags with categories
   - `ImageTag`: Many-to-many relationship between images and tags
   - `FetchRun`: Tracks ingestion runs with statistics

2. **Infrastructure Layer** (`UMLMM.Infrastructure`)
   - `UmlmmDbContext`: Entity Framework Core database context
   - PostgreSQL configuration with proper indexes and constraints

3. **Worker Service** (`UMLMM.DanbooruIngestor`)
   - `DanbooruApiClient`: HTTP client for Danbooru API
   - `DanbooruMapper`: Maps DTOs to domain entities
   - `DanbooruIngestionService`: Orchestrates the ingestion process
   - `Worker`: Hosted service that runs the ingestion

### Key Features

#### Resilience

- **Retry Policy**: Exponential backoff with jitter for transient failures
- **Circuit Breaker**: Prevents cascading failures
- **Timeout**: Configurable timeouts for API calls
- **Rate Limiting**: Handles Danbooru API rate limits gracefully

#### Idempotency

- Images are keyed by `(source_id, external_id)` with a unique index
- Tags are keyed by `name` with a unique index
- Re-running with the same inputs produces no duplicates
- Updates are detected and tracked separately from creates

#### Data Model

```sql
Images:
  - id (uuid, PK)
  - source_id (varchar, indexed with external_id)
  - external_id (varchar, indexed with source_id)
  - sha256 (varchar, indexed)
  - preview_url (text)
  - original_url (text)
  - rating (varchar)
  - metadata (jsonb)
  - created_at, updated_at

Tags:
  - id (uuid, PK)
  - name (varchar, unique index)
  - category (varchar)
  - created_at, updated_at

ImageTags:
  - image_id (uuid, FK, composite PK)
  - tag_id (uuid, FK, composite PK)
  - created_at

FetchRuns:
  - id (uuid, PK)
  - run_id (varchar, indexed)
  - source_id (varchar, indexed with started_at)
  - started_at, completed_at
  - created_count, updated_count, no_op_count, error_count
  - parameters (jsonb)
  - error_details (text)
```

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=umlmm;Username=postgres;Password=postgres"
  },
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

### Environment Variables

You can also configure via environment variables:
- `ConnectionStrings__DefaultConnection`
- `Danbooru__BaseUrl`
- `Danbooru__ApiKey`
- `Danbooru__Username`
- `Danbooru__Tags`

## Usage

### Running the Worker

```bash
dotnet run --project src/UMLMM.DanbooruIngestor
```

### Running with Docker

```bash
# Start PostgreSQL
docker run -d \
  --name umlmm-postgres \
  -e POSTGRES_DB=umlmm \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -p 5432:5432 \
  postgres:16-alpine

# Run the worker
dotnet run --project src/UMLMM.DanbooruIngestor
```

## Testing

### Unit Tests

```bash
dotnet test tests/UMLMM.DanbooruIngestor.Tests
```

Tests cover:
- DTO to entity mapping
- Tag extraction from various categories
- Rating conversion
- Edge cases (null values, empty strings, whitespace)

### Integration Tests

```bash
dotnet test tests/UMLMM.Infrastructure.IntegrationTests
```

Tests cover:
- Database CRUD operations
- Unique constraint enforcement
- Idempotent upserts
- Relationship management
- Uses Testcontainers for isolated PostgreSQL instances

## Monitoring

### Structured Logging

The application uses Serilog with structured logging:

```
[INF] Starting ingestion run danbooru-20251024-011500 for Danbooru
[INF] Fetched 100 posts from page 1
[INF] Processed page 1/10: Created=95, Updated=5, NoOp=0, Errors=0
[INF] Completed ingestion run danbooru-20251024-011500: Duration=45.2s, Created=950, Updated=50, NoOp=0, Errors=0
```

### FetchRun Statistics

Each ingestion run is tracked in the database with:
- Start and completion timestamps
- Counts of created, updated, no-op, and error operations
- Query parameters used
- Error details if failures occurred

## Future Enhancements

1. **Incremental Updates**: Track last sync timestamp to fetch only new posts
2. **Parallel Processing**: Process multiple pages concurrently
3. **Image Download**: Download and compute actual SHA256 hashes
4. **Metrics**: Add Prometheus metrics for monitoring
5. **API Authentication**: Support Danbooru API keys for higher rate limits
6. **Webhook Support**: React to Danbooru webhooks for real-time updates

## References

- [Danbooru API Documentation](https://danbooru.donmai.us/wiki_pages/help:api)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [Polly Resilience](https://www.pollydocs.org/)
- [Serilog](https://serilog.net/)
- [Testcontainers](https://dotnet.testcontainers.org/)
