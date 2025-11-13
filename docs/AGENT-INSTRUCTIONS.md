# Agent Instructions

This document provides guidance for AI agents working on the UMLMM project.

## Project Overview

UMLMM (Unified Model/Media Metadata) is a system for ingesting media metadata from multiple sources (e621, CivitAI, Danbooru, ComfyUI, Ollama) into a centralized PostgreSQL database with a Blazor admin UI.

## Current State (Phase 5 Complete)

✅ **e621 Ingestor** - Production-ready worker service
- Clean architecture with Core, Data, and Worker layers
- EF Core with PostgreSQL
- Resilient HTTP client with Polly (retry + circuit breaker)
- Comprehensive test coverage (16 tests passing)
- Structured logging with Serilog

## Architecture Principles

1. **Clean Architecture**
   - Core: Domain entities and enums
   - Data: EF Core, repositories, configurations
   - Applications: Worker services, API clients

2. **Idempotency**
   - All ingestion operations must be idempotent
   - Use unique indexes on (source_id, external_id)
   - Upsert patterns for all data

3. **Resilience**
   - Polly retry policies with exponential backoff
   - Circuit breakers for external APIs
   - Proper error handling and logging

4. **Testing**
   - Unit tests for mapping and business logic
   - Integration tests with Testcontainers
   - Test fixtures with recorded API responses

## Code Style

- Use modern C# features (.NET 9)
- Async/await throughout
- Structured logging with context
- Configuration via appsettings.json and environment variables
- Nullable reference types enabled

## Database Schema

All entities should follow this naming convention:
- Table names: lowercase with underscores (e.g., `post_tags`)
- Column names: lowercase with underscores (e.g., `external_id`)
- Primary keys: `id`
- Foreign keys: `<entity>_id` (e.g., `source_id`)

Always include:
- Unique indexes on natural keys
- Timestamps (`created_at`, `updated_at` where applicable)
- Proper foreign key relationships with cascade rules

## Adding New Ingestors

When adding a new ingestor (e.g., Danbooru, CivitAI):

1. **Domain Entities**
   - Reuse existing entities where possible
   - Add new entities to UMLMM.Core if needed

2. **API Client**
   - Create DTOs matching the API response
   - Implement resilient HTTP client
   - Add proper User-Agent
   - Respect rate limits

3. **Mapper**
   - Map API DTOs to domain entities
   - Handle null values gracefully
   - Extract and normalize tags

4. **Service**
   - Orchestrate API calls and persistence
   - Track FetchRuns for audit
   - Log progress with structured logging

5. **Tests**
   - Unit tests for mapper
   - Integration tests for repository
   - Recorded API responses for fixtures

## Security Guidelines

- Never commit secrets or API keys
- Use environment variables for sensitive config
- Validate all external input
- Use parameterized queries (EF Core does this)
- Keep dependencies updated

## Performance Considerations

- Use indexes on frequently queried columns
- Batch operations where possible
- Async/await for I/O operations
- Connection pooling (built into EF Core)
- Rate limiting for external APIs

## Monitoring and Observability

- Structured logging with Serilog
- Enrich logs with context (runId, source, etc.)
- Track metrics in FetchRuns
- Log errors with full context

## Next Steps (Future Phases)

- Phase 6: Danbooru ingestor
- Phase 7: CivitAI ingestor
- Phase 8: Orchestration service
- Phase 9: Blazor admin UI
- Phase 10: ComfyUI integration
- Phase 11: Ollama integration

## Common Patterns

### Upsert Pattern

```csharp
var existing = await GetByExternalIdAsync(sourceId, externalId);
if (existing != null)
{
    // Update existing
    existing.Property = newValue;
    _context.Update(existing);
}
else
{
    // Create new
    var entity = new Entity { ... };
    _context.Add(entity);
}
await _context.SaveChangesAsync();
```

### Resilient HTTP Client

```csharp
builder.Services.AddHttpClient<IClient, Client>(client =>
{
    client.BaseAddress = new Uri(options.BaseUrl);
    client.DefaultRequestHeaders.Add("User-Agent", options.UserAgent);
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());
```

### FetchRun Tracking

```csharp
var fetchRun = await CreateFetchRunAsync(sourceId);
try
{
    // ... ingestion logic ...
    fetchRun.Success = true;
    fetchRun.PostsFetched = count;
}
catch (Exception ex)
{
    fetchRun.Success = false;
    fetchRun.ErrorMessage = ex.Message;
}
finally
{
    fetchRun.CompletedAt = DateTime.UtcNow;
    await UpdateFetchRunAsync(fetchRun);
}
```

## Testing Guidelines

- Use xUnit for test framework
- FluentAssertions for assertions
- Testcontainers for integration tests
- Moq for mocking dependencies
- Arrange-Act-Assert pattern

## Questions?

Refer to:
- [phase-05-e621-ingestor.md](phase-05-e621-ingestor.md) for e621 specifics
- EF Core documentation for data access patterns
- Polly documentation for resilience patterns
