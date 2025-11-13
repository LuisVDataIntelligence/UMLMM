# Phase 5 — e621 Ingestor

## Overview

The e621 ingestor is a worker service that fetches posts from the e621 API, maps them to normalized domain entities, and persists them to PostgreSQL with idempotent upserts.

## Architecture

### Components

1. **UMLMM.Core** - Domain entities and enums
   - `Post`, `Tag`, `Image`, `Source`, `FetchRun`
   - `Rating` enum (Safe, Questionable, Explicit)

2. **UMLMM.Data** - Data access layer
   - EF Core `UmlmmDbContext`
   - Entity configurations with indexes
   - `PostRepository` for upserts and queries

3. **UMLMM.E621Ingestor** - Worker service
   - `E621ApiClient` - HTTP client with resilience policies
   - `E621Mapper` - Maps API DTOs to domain entities
   - `E621IngestorService` - Orchestrates ingestion
   - `E621IngestorWorker` - Background service host

### Database Schema

#### Sources
- `id` (PK)
- `name` (unique)
- `created_at`

#### Posts
- `id` (PK)
- `source_id` (FK)
- `external_id`
- `description`
- `rating` (enum)
- `external_created_at`
- `created_at`
- `updated_at`
- Unique index on `(source_id, external_id)`

#### Tags
- `id` (PK)
- `name` (unique)
- `category`
- `created_at`

#### PostTags (many-to-many)
- `post_id` (PK, FK)
- `tag_id` (PK, FK)

#### Images
- `id` (PK)
- `post_id` (FK)
- `sha256` (indexed)
- `url`
- `sample_url`
- `width`, `height`
- `file_size`
- `file_extension`
- `created_at`

#### FetchRuns
- `id` (PK)
- `source_id` (FK)
- `started_at` (indexed)
- `completed_at`
- `posts_fetched`, `posts_created`, `posts_updated`
- `success`
- `error_message`

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=umlmm;Username=postgres;Password=postgres"
  },
  "E621": {
    "BaseUrl": "https://e621.net",
    "UserAgent": "UMLMM/1.0 (by your_e621_username on e621)",
    "PageSize": 100,
    "TagFilter": null,
    "RateLimitDelayMs": 1000,
    "MaxRetries": 3,
    "TimeoutSeconds": 30
  }
}
```

### Environment Variables

You can also configure via environment variables:
- `ConnectionStrings__DefaultConnection`
- `E621__UserAgent`
- `E621__TagFilter`
- `E621__RateLimitDelayMs`

## Resilience

The HTTP client is configured with Polly policies:

1. **Retry Policy** - Exponential backoff with jitter
   - Retries transient HTTP errors
   - Configurable max retries (default: 3)
   - Exponential delay: 2^retry seconds + random jitter

2. **Circuit Breaker**
   - Opens after 5 consecutive failures
   - Stays open for 30 seconds
   - Automatically resets on success

3. **Timeout**
   - Configurable per-request timeout (default: 30s)

## Rate Limiting

The ingestor respects e621's API guidelines:
- Minimum 1 second between requests (configurable)
- Proper User-Agent header required
- Graceful handling of 429 responses

## Idempotency

The upsert logic ensures:
- Posts are keyed by `(source_id, external_id)`
- Re-running the ingestor updates existing posts
- No duplicate rows created
- Tags are reused across posts
- Images are updated in place

## Testing

### Unit Tests

```bash
dotnet test --filter "FullyQualifiedName~Unit"
```

Tests cover:
- DTO to entity mapping
- Rating enum conversion
- Tag extraction from API responses

### Integration Tests

```bash
dotnet test --filter "FullyQualifiedName~Integration"
```

Uses Testcontainers PostgreSQL for:
- Source and FetchRun creation
- Post upsert logic
- Idempotency verification
- Index coverage

## Running the Ingestor

### Prerequisites

1. PostgreSQL 12+
2. .NET 9 SDK

### Local Development

1. Start PostgreSQL:
```bash
docker run -d --name postgres -p 5432:5432 -e POSTGRES_PASSWORD=postgres postgres:16-alpine
```

2. Update User-Agent in appsettings.json with your e621 username

3. Run the ingestor:
```bash
cd src/UMLMM.E621Ingestor
dotnet run
```

### Docker

```bash
docker build -t umlmm-e621-ingestor .
docker run -e ConnectionStrings__DefaultConnection="..." umlmm-e621-ingestor
```

## Monitoring

Logs are structured with Serilog and include:
- `runId` - FetchRun identifier
- `source` - Always "e621"
- HTTP request/response details
- Retry and circuit breaker events
- Post processing progress

## API Compliance

The ingestor follows e621 API guidelines:
- User-Agent header includes project name and contact
- Respects rate limits (1 req/sec minimum)
- Handles 429 Too Many Requests gracefully
- Uses HTTPS for all requests

## Security

- No secrets in source code
- Connection strings via configuration/environment
- Input validation on API responses
- SQL injection protection via EF Core parameterized queries

## Performance

- Batch processing by page (100 posts per request)
- Efficient upserts using EF Core tracking
- Index on `(source_id, external_id)` for fast lookups
- Async/await throughout

## Future Enhancements

- Incremental updates (only fetch new posts)
- Parallel page processing
- Image download and SHA256 computation
- Retry failed posts from previous runs
- Scheduled/periodic execution
- Metrics and health checks
