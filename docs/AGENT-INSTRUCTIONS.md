# Agent Instructions for UMLMM

## Project Overview

UMLMM (Unified Model/Media Metadata) is a system for ingesting, storing, and managing metadata from multiple sources:
- CivitAI - AI model repository
- Danbooru - Image board
- e621 - Image board
- ComfyUI - Workflow engine metadata
- Ollama - Local LLM model metadata

The system consists of:
1. **Core Library** - Shared models and interfaces
2. **Orchestrator** - Quartz.NET-based job scheduler
3. **Future Components** - PostgreSQL database, Blazor admin UI

## Architecture Principles

### Separation of Concerns
- **Core** contains only models, interfaces, and utilities
- **Orchestrator** handles scheduling and execution
- **Data Access** is abstracted via `IDataContext`

### Dependency Injection
All components use constructor injection for testability:
```csharp
public class MyJob : BaseIngestionJob
{
    public MyJob(IDataContext dataContext, ILogger<MyJob> logger)
        : base(dataContext, logger)
    {
    }
}
```

### Configuration
- Use `appsettings.json` for default configuration
- Support environment variable overrides
- Bind strongly-typed configuration classes

## Job Implementation Guidelines

### Creating New Jobs

1. Inherit from `BaseIngestionJob`
2. Set the `Source` property
3. Implement `ExecuteIngestionAsync`
4. Update statistics in the `FetchRun` object

Example:
```csharp
public class NewSourceJob : BaseIngestionJob
{
    protected override DataSource Source => DataSource.NewSource;

    public NewSourceJob(IDataContext dataContext, ILogger<NewSourceJob> logger) 
        : base(dataContext, logger)
    {
    }

    protected override async Task ExecuteIngestionAsync(FetchRun fetchRun, IJobExecutionContext context)
    {
        // 1. Fetch data from source
        var data = await FetchFromApi(context.CancellationToken);
        fetchRun.RecordsFetched = data.Count;

        // 2. Process and store data
        var processed = 0;
        var failed = 0;
        foreach (var item in data)
        {
            try
            {
                await ProcessItem(item, context.CancellationToken);
                processed++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process item {Id}", item.Id);
                failed++;
            }
        }

        // 3. Update statistics
        fetchRun.RecordsProcessed = processed;
        fetchRun.RecordsFailed = failed;
    }
}
```

### Error Handling

The `BaseIngestionJob` handles:
- Exceptions (status → Failed, logs error details)
- Cancellation (status → Cancelled)
- Overlap prevention (skips if already running)

Your job should:
- Catch and log individual item failures
- Update `RecordsFailed` count
- Allow cancellation via `context.CancellationToken`

### Logging

Use structured logging with context:
```csharp
_logger.LogInformation("Processing {Count} items from {Source}", count, Source);
_logger.LogError(ex, "Failed to process item {ItemId} from {Source}", itemId, Source);
```

## Testing Guidelines

### Unit Tests

Test individual components in isolation:
```csharp
[Fact]
public void Configuration_IsLoadedCorrectly()
{
    var config = LoadConfig();
    Assert.Equal("0 0 */6 * * ?", config.CivitAI.CronSchedule);
}
```

### Integration Tests

Test cross-component behavior:
```csharp
[Fact]
public async Task TwoJobs_ForSameSource_ShouldNotOverlap()
{
    var dataContext = new InMemoryDataContext();
    var job = CreateJob(dataContext);
    
    // Start first job
    var task1 = job.Execute(context1);
    await Task.Delay(50); // Let it start
    
    // Try to start second job
    await job.Execute(context2);
    
    // Verify second was skipped
    VerifySkipped();
}
```

## Database Schema (Future)

When implementing PostgreSQL support:

```sql
CREATE TABLE fetch_runs (
    id SERIAL PRIMARY KEY,
    source VARCHAR(50) NOT NULL,
    status VARCHAR(20) NOT NULL,
    start_time TIMESTAMP NOT NULL,
    end_time TIMESTAMP,
    records_fetched INTEGER NOT NULL DEFAULT 0,
    records_processed INTEGER NOT NULL DEFAULT 0,
    records_failed INTEGER NOT NULL DEFAULT 0,
    error_message TEXT,
    error_details TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_fetch_runs_source_start ON fetch_runs(source, start_time DESC);
CREATE INDEX idx_fetch_runs_status ON fetch_runs(status);
```

## Deployment

### Environment Variables

Override configuration via environment variables:
```bash
JobSchedules__CivitAI__CronSchedule="0 0 */2 * * ?"
JobSchedules__Danbooru__CronSchedule="0 0 */2 * * ?"
Logging__LogLevel__Default="Debug"
```

### Docker Deployment

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY publish .

# Set timezone (optional)
ENV TZ=UTC

# Run as non-root user
USER app
ENTRYPOINT ["dotnet", "UMLMM.Orchestrator.dll"]
```

### Kubernetes

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: umlmm-orchestrator
spec:
  replicas: 1  # Only one instance to prevent duplicate jobs
  selector:
    matchLabels:
      app: umlmm-orchestrator
  template:
    metadata:
      labels:
        app: umlmm-orchestrator
    spec:
      containers:
      - name: orchestrator
        image: umlmm/orchestrator:latest
        env:
        - name: JobSchedules__CivitAI__CronSchedule
          value: "0 0 */6 * * ?"
        resources:
          requests:
            memory: "256Mi"
            cpu: "100m"
          limits:
            memory: "512Mi"
            cpu: "500m"
```

## Code Style

### Naming Conventions
- Classes: PascalCase
- Methods: PascalCase
- Parameters: camelCase
- Private fields: _camelCase with underscore prefix

### File Organization
- One class per file
- File name matches class name
- Group related classes in subdirectories

### Comments
- XML comments for public APIs
- Inline comments for complex logic only
- Use meaningful variable names instead of comments where possible

## Common Tasks

### Adding a Schedule Configuration

1. Update `JobSchedulesConfig.cs`:
```csharp
public JobScheduleConfig NewSource { get; set; } = new();
```

2. Update `appsettings.json`:
```json
"NewSource": {
  "CronSchedule": "0 0 */6 * * ?",
  "Description": "Every 6 hours"
}
```

3. Register in `Program.cs`:
```csharp
var newSourceJobKey = new JobKey("NewSourceIngestionJob");
q.AddJob<NewSourceIngestionJob>(opts => opts.WithIdentity(newSourceJobKey));
q.AddTrigger(opts => opts
    .ForJob(newSourceJobKey)
    .WithIdentity("NewSourceIngestionJob-trigger")
    .WithCronSchedule(jobSchedules.NewSource.CronSchedule));
```

### Changing Log Levels

Update `appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Quartz": "Warning",
      "UMLMM.Orchestrator.Jobs": "Debug"
    }
  }
}
```

### Debugging a Job

1. Add breakpoint in job's `ExecuteIngestionAsync` method
2. Run orchestrator in debug mode
3. Manually trigger job via Quartz API (future feature)
4. Or wait for scheduled execution

## Future Enhancements

### Phase 9 - PostgreSQL Integration
- Implement `PostgresDataContext`
- Add Entity Framework Core
- Implement migrations
- Add connection pooling

### Phase 10 - Blazor Admin UI
- View job history
- Manually trigger jobs
- View statistics and charts
- Configure schedules
- View logs

### Phase 11 - Advanced Features
- Job dependencies (run X after Y completes)
- Retry logic with exponential backoff
- Rate limiting per source
- Webhook notifications
- Prometheus metrics

## Troubleshooting

### "Job already running" warnings
Normal behavior - overlap prevention is working. If persistent, check for stuck jobs in database.

### Jobs not executing
1. Check cron expression syntax
2. Verify job is registered in Program.cs
3. Check logs for startup errors
4. Verify Quartz scheduler is starting

### High memory usage
1. Check for memory leaks in job implementations
2. Ensure proper disposal of resources
3. Review `InMemoryDataContext` - it never clears old runs
4. Consider implementing cleanup/archival

### Tests failing intermittently
1. Likely timing issues in integration tests
2. Increase delays to allow async operations to complete
3. Use proper synchronization primitives
4. Avoid Thread.Sleep, use Task.Delay

## Resources

- [Quartz.NET Tutorial](https://www.quartz-scheduler.net/documentation/quartz-3.x/tutorial/index.html)
- [.NET Worker Services](https://learn.microsoft.com/en-us/dotnet/core/extensions/workers)
- [xUnit Testing](https://xunit.net/)
- [Structured Logging](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging)
