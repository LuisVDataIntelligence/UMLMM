# Phase 8 - Orchestration and Scheduling

## Overview

This document describes the implementation of the orchestration and scheduling system for UMLMM, which manages automated data ingestion from multiple sources using Quartz.NET.

## Architecture

### Components

1. **UMLMM.Core** - Shared library containing:
   - `DataSource` enum - Defines all supported sources (CivitAI, Danbooru, e621, ComfyUI, Ollama)
   - `FetchRun` model - Tracks execution statistics for each job run
   - `FetchRunStatus` enum - Status lifecycle (Queued, Running, Completed, Failed, Cancelled)
   - `IDataContext` interface - Abstraction for data persistence operations
   - `InMemoryDataContext` - In-memory implementation for development/testing

2. **UMLMM.Orchestrator** - Worker service containing:
   - `BaseIngestionJob` - Abstract base class with no-overlap logic and error handling
   - Source-specific jobs:
     - `CivitAIIngestionJob`
     - `DanbooruIngestionJob`
     - `E621IngestionJob`
     - `ComfyUIIngestionJob`
     - `OllamaIngestionJob`
   - Configuration classes for job schedules
   - Quartz.NET scheduler integration

3. **UMLMM.Orchestrator.Tests** - Test suite containing:
   - Unit tests for job registration and configuration
   - Integration tests for no-overlap validation

## Key Features

### No-Overlap Per Source

The `BaseIngestionJob` class uses the `[DisallowConcurrentExecution]` attribute and checks for running jobs before execution:

```csharp
[DisallowConcurrentExecution]
public abstract class BaseIngestionJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        // Check if there's already a running job for this source
        if (await _dataContext.HasRunningFetchAsync(Source, cancellationToken))
        {
            _logger.LogWarning("Job for {Source} is already running, skipping this execution", Source);
            return;
        }
        // ... rest of execution
    }
}
```

This ensures that:
- Only one job per source can run at a time
- Multiple sources can run concurrently
- Jobs are skipped (not queued) if already running

### Run Statistics Tracking

Each job execution creates a `FetchRun` record that tracks:
- **Status**: Queued → Running → Completed/Failed/Cancelled
- **Timings**: StartTime, EndTime, DurationMs
- **Counts**: RecordsFetched, RecordsProcessed, RecordsFailed
- **Errors**: ErrorMessage, ErrorDetails (full exception stack trace)

### Configurable Schedules

Job schedules are defined in `appsettings.json` using Quartz cron expressions:

```json
{
  "JobSchedules": {
    "CivitAI": {
      "CronSchedule": "0 0 */6 * * ?",
      "Description": "Every 6 hours"
    },
    "Danbooru": {
      "CronSchedule": "0 0 */4 * * ?",
      "Description": "Every 4 hours"
    }
  }
}
```

Schedules can be overridden via environment variables:
- `JobSchedules__CivitAI__CronSchedule`
- `JobSchedules__Danbooru__CronSchedule`
- etc.

### Graceful Shutdown

The orchestrator uses Quartz's built-in graceful shutdown:

```csharp
builder.Services.AddQuartzHostedService(options =>
{
    // Wait for jobs to complete on shutdown
    options.WaitForJobsToComplete = true;
});
```

Jobs can also check for cancellation via `context.CancellationToken` and handle it gracefully.

## Configuration

### Default Schedules

| Source   | Schedule              | Description           |
|----------|-----------------------|-----------------------|
| CivitAI  | `0 0 */6 * * ?`       | Every 6 hours         |
| Danbooru | `0 0 */4 * * ?`       | Every 4 hours         |
| e621     | `0 0 */4 * * ?`       | Every 4 hours         |
| ComfyUI  | `0 0 */12 * * ?`      | Every 12 hours        |
| Ollama   | `0 0 0 * * ?`         | Daily at midnight     |

### Cron Expression Format

Quartz uses 6 or 7-part cron expressions:
```
┌───────────── second (0-59)
│ ┌───────────── minute (0-59)
│ │ ┌───────────── hour (0-23)
│ │ │ ┌───────────── day of month (1-31)
│ │ │ │ ┌───────────── month (1-12 or JAN-DEC)
│ │ │ │ │ ┌───────────── day of week (0-6 or SUN-SAT)
│ │ │ │ │ │
│ │ │ │ │ │
* * * * * *
```

Examples:
- `0 0 */4 * * ?` - Every 4 hours
- `0 15 10 * * ?` - Every day at 10:15 AM
- `0 0 0 1 * ?` - First day of every month at midnight

## Running the Orchestrator

### Development

```bash
cd src/UMLMM.Orchestrator
dotnet run
```

### Production

```bash
cd src/UMLMM.Orchestrator
dotnet publish -c Release -o publish
cd publish
./UMLMM.Orchestrator
```

### Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY publish .
ENTRYPOINT ["dotnet", "UMLMM.Orchestrator.dll"]
```

## Testing

### Run All Tests

```bash
dotnet test
```

### Run Specific Test Category

```bash
# Unit tests
dotnet test --filter "FullyQualifiedName~JobRegistrationTests"

# Integration tests
dotnet test --filter "FullyQualifiedName~NoOverlapIntegrationTests"
```

## Extending the System

### Adding a New Data Source

1. Add new source to `DataSource` enum in `UMLMM.Core/Models/DataSource.cs`
2. Create new job class inheriting from `BaseIngestionJob`
3. Implement `ExecuteIngestionAsync` method
4. Add job registration in `Program.cs`
5. Add schedule configuration to `appsettings.json`
6. Add schedule property to `JobSchedulesConfig`

Example:

```csharp
public class NewSourceIngestionJob : BaseIngestionJob
{
    protected override DataSource Source => DataSource.NewSource;

    public NewSourceIngestionJob(IDataContext dataContext, ILogger<NewSourceIngestionJob> logger) 
        : base(dataContext, logger)
    {
    }

    protected override async Task ExecuteIngestionAsync(FetchRun fetchRun, IJobExecutionContext context)
    {
        // Implement ingestion logic
        await FetchDataFromSource(context.CancellationToken);
        
        // Update statistics
        fetchRun.RecordsFetched = 100;
        fetchRun.RecordsProcessed = 95;
        fetchRun.RecordsFailed = 5;
    }
}
```

### Implementing Real Data Persistence

Replace `InMemoryDataContext` with a real implementation:

1. Implement `IDataContext` using Entity Framework Core or Dapper
2. Connect to PostgreSQL database
3. Update DI registration in `Program.cs`:

```csharp
builder.Services.AddSingleton<IDataContext, PostgresDataContext>();
```

## Monitoring and Observability

### Logs

The orchestrator emits structured logs at key points:
- Job start/completion with run ID and source
- Statistics on completion (duration, counts)
- Warnings for skipped executions (overlap prevention)
- Errors with full exception details

### Metrics (Future)

Consider adding:
- Job execution duration metrics
- Success/failure rates per source
- Records processed per minute
- Queue depth per source

### Health Checks (Future)

Add health checks for:
- Scheduler health (Quartz running)
- Database connectivity
- Last successful run per source
- Stuck jobs (running too long)

## Troubleshooting

### Job Not Running

1. Check logs for scheduling errors
2. Verify cron expression is valid
3. Check if previous run is still active (overlap prevention)
4. Verify job is registered in `Program.cs`

### Jobs Taking Too Long

1. Check fetch run records for duration statistics
2. Review error logs for retries or slow operations
3. Consider adjusting schedule frequency
4. Add timeout logic in job implementation

### Overlapping Executions

The system prevents overlaps by design. If you see warnings about skipped executions:
1. This is normal behavior - the previous job is still running
2. Consider increasing schedule interval
3. Review job implementation for optimization opportunities
4. Check for stuck jobs (status = Running but no activity)

## References

- [Quartz.NET Documentation](https://www.quartz-scheduler.net/)
- [Cron Expression Generator](https://www.freeformatter.com/cron-expression-generator-quartz.html)
- [.NET Generic Host](https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host)
