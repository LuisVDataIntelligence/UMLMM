# Agent Instructions

## Project Structure

This is a .NET 9 solution with the following structure:

```
UMLMM/
├── src/
│   ├── UMLMM.Domain/              # Domain entities and models
│   ├── UMLMM.Infrastructure/       # Database context and data access
│   └── UMLMM.DanbooruIngestor/    # Worker service for Danbooru ingestion
├── tests/
│   ├── UMLMM.DanbooruIngestor.Tests/          # Unit tests
│   └── UMLMM.Infrastructure.IntegrationTests/  # Integration tests
└── docs/
    ├── phase-04-danbooru-ingestor.md
    └── AGENT-INSTRUCTIONS.md
```

## Technology Stack

- **.NET 9**: Latest LTS version
- **Entity Framework Core 9**: ORM for PostgreSQL
- **PostgreSQL 16**: Database
- **Polly 8**: Resilience and transient fault handling
- **Serilog**: Structured logging
- **xUnit**: Testing framework
- **FluentAssertions**: Test assertions
- **Testcontainers**: Integration testing with Docker
- **NSubstitute**: Mocking framework

## Development Guidelines

### Building

```bash
dotnet build
```

### Running Tests

```bash
# All tests
dotnet test

# Unit tests only
dotnet test tests/UMLMM.DanbooruIngestor.Tests

# Integration tests only
dotnet test tests/UMLMM.Infrastructure.IntegrationTests
```

### Code Style

- Use C# 12 features (file-scoped namespaces, primary constructors where appropriate)
- Follow standard .NET naming conventions
- Use `var` for local variables when type is obvious
- Prefer explicit types for public APIs
- Use nullable reference types (`string?` for nullable strings)

### Database Migrations

To create a migration:

```bash
cd src/UMLMM.Infrastructure
dotnet ef migrations add MigrationName --startup-project ../UMLMM.DanbooruIngestor
dotnet ef database update --startup-project ../UMLMM.DanbooruIngestor
```

Note: Currently using `EnsureCreatedAsync()` for simplicity. For production, use proper migrations.

### Testing Guidelines

#### Unit Tests
- Test business logic and mappings
- Mock external dependencies
- Use descriptive test names: `MethodName_Scenario_ExpectedBehavior`
- Use AAA pattern: Arrange, Act, Assert

#### Integration Tests
- Use Testcontainers for real PostgreSQL instances
- Test database operations and constraints
- Verify idempotency
- Clean up resources properly

### Adding New Ingestors

To add a new data source ingestor (e.g., e621, CivitAI):

1. **Create Domain Entities** (if needed)
   - Add to `UMLMM.Domain/Entities/`
   - Update `UmlmmDbContext` with DbSets and configuration

2. **Create Worker Project**
   ```bash
   cd src
   dotnet new worker -n UMLMM.NewSourceIngestor
   dotnet sln add UMLMM.NewSourceIngestor
   ```

3. **Add API Client**
   - Create DTOs for API responses
   - Implement client with HttpClientFactory
   - Add Polly policies for resilience

4. **Add Mapper**
   - Map API DTOs to domain entities
   - Handle all edge cases

5. **Add Ingestion Service**
   - Implement idempotent upserts
   - Track statistics in FetchRun
   - Handle errors gracefully

6. **Add Tests**
   - Unit tests for mapping
   - Integration tests for database operations

### Logging

Use structured logging with Serilog:

```csharp
_logger.LogInformation(
    "Processed page {Page}: Created={Created}, Updated={Updated}",
    page, createdCount, updatedCount);
```

### Error Handling

- Use try-catch at service boundaries
- Log errors with context
- Track errors in FetchRun.ErrorCount
- Don't let single item failures stop batch processing

### Performance Considerations

- Use `AsNoTracking()` for read-only queries
- Batch database operations when possible
- Consider pagination for large result sets
- Use indexes for frequently queried columns

## Common Tasks

### Adding a New Configuration Setting

1. Add property to settings class (e.g., `DanbooruSettings`)
2. Update `appsettings.json`
3. Document in `phase-04-danbooru-ingestor.md`

### Adding a New Entity

1. Create entity class in `UMLMM.Domain/Entities/`
2. Add DbSet to `UmlmmDbContext`
3. Configure in `OnModelCreating` (indexes, constraints, etc.)
4. Create migration (if using migrations)
5. Add integration tests

### Debugging Issues

1. Check logs for structured data
2. Verify database constraints
3. Check for version conflicts in dependencies
4. Run integration tests to verify database operations

## CI/CD Considerations

- Tests run in CI environment
- Integration tests use Testcontainers (requires Docker)
- Build warnings are treated as informational
- All tests must pass before merge

## Security

- Never commit secrets or API keys
- Use environment variables or secret management for sensitive data
- Validate and sanitize all external input
- Use parameterized queries (EF Core does this automatically)
- Keep dependencies updated

## Documentation

- Update README.md for major changes
- Keep phase documentation current
- Document breaking changes
- Add inline comments for complex logic only
