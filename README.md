# UMLMM - Unified Model/Media Metadata

A comprehensive metadata ingestion and management system that consolidates data from multiple sources including CivitAI, Danbooru, e621, ComfyUI, and Ollama.

## Features

- **Multi-Source Ingestion**: Automated data fetching from 5+ sources
- **Quartz.NET Scheduling**: Robust job scheduling with configurable cron expressions
- **No-Overlap Execution**: Prevents concurrent runs per source while allowing parallel execution across different sources
- **Comprehensive Tracking**: Records detailed statistics for every job run (timings, counts, errors)
- **Graceful Shutdown**: Waits for jobs to complete before stopping
- **Configurable Schedules**: Easily adjust job frequencies via configuration files or environment variables

## Architecture

### Projects

- **UMLMM.Core** - Shared library with models, interfaces, and services
- **UMLMM.Orchestrator** - Worker service with Quartz.NET job scheduling
- **UMLMM.Orchestrator.Tests** - Comprehensive test suite with unit and integration tests

### Data Sources

| Source   | Description                    | Default Schedule  |
|----------|--------------------------------|-------------------|
| CivitAI  | AI model repository            | Every 6 hours     |
| Danbooru | Image board                    | Every 4 hours     |
| e621     | Image board                    | Every 4 hours     |
| ComfyUI  | Workflow engine metadata       | Every 12 hours    |
| Ollama   | Local LLM model metadata       | Daily at midnight |

## Quick Start

### Prerequisites

- .NET 9.0 SDK
- Git

### Build and Run

```bash
# Clone the repository
git clone https://github.com/LuisVDataIntelligence/UMLMM.git
cd UMLMM

# Build the solution
dotnet build

# Run tests
dotnet test

# Run the orchestrator
dotnet run --project src/UMLMM.Orchestrator
```

### Configuration

Job schedules can be configured in `src/UMLMM.Orchestrator/appsettings.json`:

```json
{
  "JobSchedules": {
    "CivitAI": {
      "CronSchedule": "0 0 */6 * * ?",
      "Description": "Every 6 hours"
    }
  }
}
```

Or via environment variables:

```bash
export JobSchedules__CivitAI__CronSchedule="0 0 */2 * * ?"
```

## Documentation

- [Phase 8 - Orchestration and Scheduling](docs/phase-08-orchestration-and-scheduling.md) - Detailed implementation guide
- [Agent Instructions](docs/AGENT-INSTRUCTIONS.md) - Developer guidelines and best practices

## Testing

The project includes comprehensive tests:

```bash
# Run all tests
dotnet test

# Run specific test categories
dotnet test --filter "FullyQualifiedName~JobRegistrationTests"
dotnet test --filter "FullyQualifiedName~NoOverlapIntegrationTests"
```

All tests validate:
- Job registration and configuration
- No-overlap enforcement per source
- Concurrent execution across different sources
- Statistics tracking
- Error handling and cancellation

## Development Status

### Phase 8 - Orchestration and Scheduling ✅ Complete

- [x] Quartz.NET integration
- [x] Job implementations for all 5 sources
- [x] No-overlap logic per source
- [x] Configuration support (appsettings + env vars)
- [x] Comprehensive logging
- [x] Graceful shutdown
- [x] Unit and integration tests
- [x] Documentation

### Future Phases

- **Phase 9**: PostgreSQL integration with Entity Framework Core
- **Phase 10**: Blazor Server admin UI for job monitoring and management
- **Phase 11**: Advanced features (dependencies, retries, webhooks, metrics)

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Run tests to ensure everything works
5. Submit a pull request

## License

[Add your license here]

## Contact

For questions or issues, please open an issue on GitHub.
