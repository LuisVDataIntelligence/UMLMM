# UMLMM Project - Component Breakdown for Integration

This document provides a comprehensive breakdown of all C# classes and Razor components in the UMLMM project, organized by feature and functionality for easy integration into other projects.

## Table of Contents

1. [Core Domain Entities](#core-domain-entities)
2. [Data Transfer Objects (DTOs)](#data-transfer-objects-dtos)
3. [Repository Pattern](#repository-pattern)
4. [API Client Patterns](#api-client-patterns)
5. [Data Ingestion Services](#data-ingestion-services)
6. [Background Job Scheduling](#background-job-scheduling)
7. [Blazor UI Components](#blazor-ui-components)
8. [Entity Framework Configurations](#entity-framework-configurations)
9. [API Endpoints](#api-endpoints)
10. [Integration Guide](#integration-guide)

---

## Core Domain Entities

### Base Entity
**Purpose**: Foundation for all domain entities with common properties

**File**: `src/UMLMM.Domain/Common/BaseEntity.cs`

**Dependencies**: None

**Usage**:
```csharp
public abstract class BaseEntity
{
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
```

**Integration**: Inherit from this class for any new entities that need timestamp tracking.

---

### Model Entity
**Purpose**: Represents an AI model (Checkpoint, LoRA, Embedding, etc.)

**File**: `src/UMLMM.Domain/Entities/Model.cs`

**Dependencies**:
- `BaseEntity`
- `ModelType` enum
- `Source` entity
- `ModelVersion` entity (navigation)
- `ModelTag` entity (navigation)

**Key Properties**:
- `ExternalId`: Original ID from source system
- `Name`, `Description`: Model metadata
- `SourceId`: References the data source
- `Type`: ModelType enum
- `Versions`: Collection of ModelVersion
- `ModelTags`: Many-to-many with Tags

**Integration**: Use for any project that needs to track AI models with versioning.

---

### ModelVersion Entity
**Purpose**: Represents a specific version of a model

**File**: `src/UMLMM.Domain/Entities/ModelVersion.cs`

**Dependencies**:
- `BaseEntity`
- `Model` entity
- `Artifact` entity (navigation)

**Key Properties**:
- `VersionName`: Version identifier
- `BaseModel`: Base model name (e.g., "SD 1.5")
- `DownloadUrl`: Direct download link
- `TrainedWords`: Trigger words/activation phrases
- `Artifacts`: Collection of downloadable files

**Integration**: Use with Model entity for version tracking.

---

### Artifact Entity
**Purpose**: Represents downloadable files for model versions

**File**: `src/UMLMM.Domain/Entities/Artifact.cs`

**Dependencies**:
- `BaseEntity`
- `ModelVersion` entity
- `ArtifactKind` enum

**Key Properties**:
- `Kind`: ArtifactKind (Model, VAE, Config, Pruned, TrainingData)
- `FileName`: Original filename
- `DownloadUrl`: Direct download link
- `SizeBytes`: File size
- `Format`: File format (safetensors, ckpt, etc.)

**Integration**: Use for tracking downloadable assets associated with versions.

---

### Image Entity
**Purpose**: Represents an image from various sources

**File**: `src/UMLMM.Domain/Entities/Image.cs`

**Dependencies**:
- `BaseEntity`
- `Source` entity
- `NsfwRating` enum
- `ImageTag` entity (navigation)
- `Prompt` entity (navigation)

**Key Properties**:
- `ExternalId`: Original ID from source
- `Url`: Image URL
- `ThumbnailUrl`: Thumbnail URL
- `Width`, `Height`: Dimensions
- `Rating`: NsfwRating (Safe, Questionable, Explicit)
- `Score`: Community rating/score
- `ImageTags`: Many-to-many with Tags
- `Prompts`: Associated AI prompts

**Integration**: Use for image gallery/search features.

---

### Tag Entity
**Purpose**: Categorization and metadata tags

**File**: `src/UMLMM.Domain/Entities/Tag.cs`

**Dependencies**:
- `BaseEntity`
- `ModelTag`, `ImageTag` entities (navigation)

**Key Properties**:
- `Name`: Tag name (normalized, lowercase)
- `DisplayName`: Human-readable display name
- `Category`: Tag category (character, artist, style, etc.)
- `ModelTags`: Many-to-many with Models
- `ImageTags`: Many-to-many with Images

**Integration**: Universal tagging system for any content type.

---

### Source Entity
**Purpose**: Tracks external data sources

**File**: `src/UMLMM.Domain/Entities/Source.cs`

**Dependencies**: `BaseEntity`

**Key Properties**:
- `Name`: Source name (CivitAI, Danbooru, E621, etc.)
- `BaseUrl`: API base URL
- `ApiVersion`: API version
- `IsActive`: Enable/disable flag

**Integration**: Use for multi-source data aggregation.

---

### FetchRun Entity
**Purpose**: Tracks data ingestion runs with statistics

**File**: `src/UMLMM.Domain/Entities/FetchRun.cs`

**Dependencies**:
- `BaseEntity`
- `Source` entity
- `FetchStatus` enum

**Key Properties**:
- `SourceId`: Source being ingested
- `Status`: FetchStatus (Running, Succeeded, Failed)
- `StartedAtUtc`, `CompletedAtUtc`: Timing
- `ItemsFetched`, `ItemsCreated`, `ItemsUpdated`: Statistics
- `ErrorMessage`: Error details

**Integration**: Track any background data fetching operations.

---

### Prompt Entity
**Purpose**: AI generation prompts

**File**: `src/UMLMM.Domain/Entities/Prompt.cs`

**Dependencies**:
- `BaseEntity`
- `Image` entity

**Key Properties**:
- `PositivePrompt`: Positive prompt text
- `NegativePrompt`: Negative prompt text
- `Steps`, `Seed`, `CfgScale`: Generation parameters
- `Sampler`, `Model`: Generation settings
- `ImageId`: Associated image

**Integration**: Use for storing AI image generation metadata.

---

### Workflow Entity
**Purpose**: ComfyUI workflow graphs

**File**: `src/UMLMM.Domain/Entities/Workflow.cs`

**Dependencies**:
- `BaseEntity`
- `Source` entity

**Key Properties**:
- `Name`: Workflow name
- `WorkflowJson`: Full workflow as JSON
- `Description`: Workflow description
- `FilePath`: Original file path
- `SourceId`: Source reference

**Integration**: Use for storing graph-based workflows.

---

### Enums

#### ModelType
**File**: `src/UMLMM.Domain/Enums/ModelType.cs`

**Values**: Checkpoint, Lora, LoCon, Embedding, Hypernetwork, AestheticGradient, ControlNet, Poses, Wildcards, Upscaler, MotionModule, VAE, Other

#### NsfwRating
**File**: `src/UMLMM.Domain/Enums/NsfwRating.cs`

**Values**: Safe, Questionable, Explicit

#### ArtifactKind
**File**: `src/UMLMM.Domain/Enums/ArtifactKind.cs`

**Values**: Model, VAE, Config, Pruned, TrainingData

#### FetchStatus
**File**: `src/UMLMM.Domain/Enums/FetchStatus.cs`

**Values**: Running, Succeeded, Failed

---

## Data Transfer Objects (DTOs)

### PagedResult<T>
**Purpose**: Generic pagination wrapper

**File**: `src/Contracts/DTOs/PagedResult.cs`

**Dependencies**: None

**Properties**:
- `Items`: List<T>
- `TotalCount`: Total items
- `PageNumber`: Current page
- `PageSize`: Items per page
- `TotalPages`: Calculated total pages

**Integration**: Use for any paginated API response.

```csharp
var result = new PagedResult<ModelDto>
{
    Items = models,
    TotalCount = totalCount,
    PageNumber = 1,
    PageSize = 20
};
```

---

### SearchRequestDto
**Purpose**: Universal search request with filtering

**File**: `src/Contracts/DTOs/SearchRequestDto.cs`

**Dependencies**: None

**Properties**:
- `Query`: Search query string
- `PageNumber`, `PageSize`: Pagination
- `Tags`: List<string> for tag filtering
- `SourceIds`: List<int> for source filtering
- `MinRating`, `MaxRating`: Rating range
- `SortBy`, `SortDescending`: Sorting

**Integration**: Standard search request pattern.

---

### ModelDto
**Purpose**: Model data transfer

**File**: `src/Contracts/DTOs/ModelDto.cs`

**Dependencies**: `ModelVersion`, `TagDto`, `ImageDto`

**Properties**:
- All Model entity properties
- `SourceName`: Denormalized source name
- `Versions`: List<ModelVersionDto>
- `Tags`: List<TagDto>
- `Images`: List<ImageDto>

**Integration**: Use for API responses containing models.

---

### ImageDto
**Purpose**: Image data transfer

**File**: `src/Contracts/DTOs/ImageDto.cs`

**Dependencies**: `TagDto`

**Properties**:
- All Image entity properties
- `SourceName`: Denormalized source name
- `Tags`: List<TagDto>

**Integration**: Use for image gallery APIs.

---

### RunDto
**Purpose**: Ingestion run status transfer

**File**: `src/Contracts/DTOs/RunDto.cs`

**Dependencies**: None

**Properties**:
- All FetchRun properties
- `SourceName`: Denormalized source name
- `DurationSeconds`: Calculated duration

**Integration**: Monitor background job status.

---

## Repository Pattern

### IModelRepository Interface
**Purpose**: Abstract repository for model operations

**File**: `src/UMLMM.Infrastructure/Repositories/IModelRepository.cs`

**Dependencies**: `Model` entity

**Methods**:
- `Task<Model?> GetByIdAsync(int id)`
- `Task<IEnumerable<Model>> GetAllAsync()`
- `Task<Model> UpsertAsync(Model model, string externalId, int sourceId)`
- `Task SaveChangesAsync()`

**Integration**: Implement this interface for any data access pattern.

---

### ModelRepository (EF Core)
**Purpose**: Entity Framework implementation

**File**: `src/UMLMM.Infrastructure/Repositories/ModelRepository.cs`

**Dependencies**:
- `IModelRepository`
- `UmlmmDbContext`
- Entity Framework Core

**Key Features**:
- Upsert pattern (update if exists, insert if new)
- External ID + Source ID composite key
- Include related entities
- Transaction management

**Integration**: Copy this pattern for EF Core repositories.

---

### JsonModelRepository
**Purpose**: JSON file-based implementation

**File**: `src/UMLMM.Infrastructure/Repositories/JsonModelRepository.cs`

**Dependencies**:
- `IModelRepository`
- System.Text.Json

**Key Features**:
- File-based persistence
- In-memory caching
- Atomic file writes
- Same interface as EF implementation

**Integration**: Use for lightweight storage without database.

---

## API Client Patterns

### IApiClient Interface (Gateway)
**Purpose**: HTTP client for Gateway API

**File**: `src/BlazorFrontend/Services/IApiClient.cs`

**Dependencies**: DTOs

**Methods**:
- `Task<PagedResultDto<ModelDto>> SearchModelsAsync(SearchRequestDto request)`
- `Task<ModelDto?> GetModelAsync(int id)`
- `Task<List<RunDto>> GetRunsAsync()`

**Integration**: Pattern for typed HTTP clients.

---

### ApiClient Implementation
**Purpose**: HttpClient-based implementation

**File**: `src/BlazorFrontend/Services/ApiClient.cs`

**Dependencies**:
- `IApiClient`
- `HttpClient`
- `ILogger`

**Key Features**:
- JSON serialization with camelCase
- Error handling with logging
- Null result handling
- Async/await pattern

**Integration**: Template for REST API clients.

---

### CivitAI API Client
**Purpose**: Client for CivitAI API

**File**: `src/UMLMM.Ingestors.CivitAI/CivitAI/Client/CivitAIApiClient.cs`

**Dependencies**: `CivitAIDtos`

**Key Features**:
- Paginated fetching
- Rate limiting headers
- NSFW filtering
- Model type filtering

**Integration**: Pattern for external API integration.

---

### Danbooru API Client
**Purpose**: Client for Danbooru API

**File**: `src/UMLMM.DanbooruIngestor/Danbooru/DanbooruApiClient.cs`

**Dependencies**: `DanbooruPostDto`

**Key Features**:
- Tag-based search
- Page-based pagination
- Rating filtering
- Custom User-Agent requirement

**Integration**: Pattern for image board APIs.

---

### E621 API Client
**Purpose**: Client for E621 API

**File**: `src/UMLMM.E621Ingestor/Client/E621ApiClient.cs`

**Dependencies**: `E621DTOs`

**Key Features**:
- Required User-Agent header
- Tag search with operators
- Post limit per request
- Page navigation

**Integration**: Similar to Danbooru pattern.

---

### Ollama API Client
**Purpose**: Client for Ollama API

**File**: `src/OllamaIngestor/Services/OllamaClient.cs`

**Dependencies**: `OllamaModels`

**Key Features**:
- Local API (localhost:11434)
- Model listing
- Model information
- Simple JSON responses

**Integration**: Pattern for local service APIs.

---

## Data Ingestion Services

### CivitAI Ingestion Service
**Purpose**: Ingest models from CivitAI

**File**: `src/UMLMM.Ingestors.CivitAI/Services/CivitAIIngestionService.cs`

**Dependencies**:
- `CivitAIApiClient`
- `IModelRepository`
- `UmlmmDbContext`
- `CivitAIMapper`

**Key Features**:
- Paginated ingestion
- Upsert pattern for idempotency
- FetchRun tracking
- Statistics collection
- Error handling

**Integration Steps**:
1. Copy the service class
2. Implement the API client
3. Create mapper for DTO transformation
4. Configure repository
5. Wire up in DI container

**Code Pattern**:
```csharp
public async Task IngestAsync(CancellationToken ct)
{
    var run = new FetchRun { SourceId = sourceId, Status = Running };
    await _context.FetchRuns.AddAsync(run, ct);

    try
    {
        var page = 1;
        while (!ct.IsCancellationRequested)
        {
            var response = await _client.GetModelsAsync(page, ct);
            if (!response.Items.Any()) break;

            foreach (var dto in response.Items)
            {
                var model = _mapper.MapToModel(dto);
                await _repository.UpsertAsync(model, dto.Id, sourceId);
                run.ItemsFetched++;
            }

            page++;
        }

        run.Status = Succeeded;
    }
    catch (Exception ex)
    {
        run.Status = Failed;
        run.ErrorMessage = ex.Message;
    }
    finally
    {
        run.CompletedAtUtc = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(ct);
    }
}
```

---

### Danbooru Ingestion Service
**Purpose**: Ingest posts/images from Danbooru

**File**: `src/UMLMM.DanbooruIngestor/Services/DanbooruIngestionService.cs`

**Dependencies**:
- `IDanbooruApiClient`
- `IPostRepository`
- `DanbooruMapper`

**Key Features**:
- Tag-based fetching
- Image metadata extraction
- Tag normalization
- Rating mapping

**Integration**: Similar pattern to CivitAI, adapted for image posts.

---

### E621 Ingestion Service
**Purpose**: Ingest posts from E621

**File**: `src/UMLMM.E621Ingestor/Services/E621IngestorService.cs`

**Dependencies**:
- `E621ApiClient`
- `AppDbContext`
- `E621Mapper`

**Key Features**:
- Tag search
- Batch processing
- Duplicate detection
- File type handling

**Integration**: Copy pattern from Danbooru or CivitAI.

---

### ComfyUI Workflow Ingestion
**Purpose**: Discover and parse ComfyUI workflow files

**File**: `src/UMLMM.ComfyUIIngestor/Services/WorkflowIngestService.cs`

**Dependencies**:
- `WorkflowDiscovery`
- `WorkflowParser`
- `UmlmmDbContext`

**Key Features**:
- File system scanning
- JSON parsing
- Node graph extraction
- Metadata extraction

**Integration Steps**:
1. Copy WorkflowDiscovery for file scanning
2. Copy WorkflowParser for JSON parsing
3. Store in Workflow entity

---

### Ollama Model Ingestion
**Purpose**: Ingest locally installed Ollama models

**File**: `src/OllamaIngestor/Services/OllamaIngestionService.cs`

**Dependencies**:
- `OllamaClient`
- `UmlmmDbContext`

**Key Features**:
- Local model listing
- Model details fetching
- Simple mapping

**Integration**: Pattern for local service ingestion.

---

## Background Job Scheduling

### BaseIngestionJob
**Purpose**: Abstract base for all scheduled ingestion jobs

**File**: `src/UMLMM.Orchestrator/Jobs/BaseIngestionJob.cs`

**Dependencies**: Quartz.NET

**Key Features**:
- No-overlap execution (DisallowConcurrentExecution)
- Logging integration
- Error handling
- Abstract ExecuteAsync method

**Usage**:
```csharp
[DisallowConcurrentExecution]
public abstract class BaseIngestionJob : IJob
{
    protected readonly ILogger Logger;

    public async Task Execute(IJobExecutionContext context)
    {
        Logger.LogInformation("Starting job...");
        try
        {
            await ExecuteAsync(context.CancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Job failed");
            throw;
        }
    }

    protected abstract Task ExecuteAsync(CancellationToken ct);
}
```

**Integration**: Inherit for any scheduled job.

---

### CivitAI Ingestion Job
**Purpose**: Scheduled job for CivitAI ingestion

**File**: `src/UMLMM.Orchestrator/Jobs/CivitAIIngestionJob.cs`

**Dependencies**:
- `BaseIngestionJob`
- `CivitAIIngestionService`

**Integration**: Wire up with Quartz.NET scheduler.

---

### Job Configuration
**Purpose**: Cron schedule configuration

**File**: `src/UMLMM.Orchestrator/Configuration/JobScheduleConfig.cs`

**Properties**:
- `JobName`: Unique job identifier
- `CronExpression`: Quartz cron expression
- `Enabled`: Enable/disable flag

**Example**:
```json
{
  "JobSchedules": [
    {
      "JobName": "CivitAI",
      "CronExpression": "0 0 2 * * ?",
      "Enabled": true
    }
  ]
}
```

**Integration**: Use for appsettings.json configuration.

---

## Blazor UI Components

### LoadingSpinner Component
**Purpose**: Reusable loading indicator

**File**: `src/BlazorFrontend/Components/Shared/LoadingSpinner.razor`

**Dependencies**: None

**Parameters**:
- `Message`: Optional loading message
- `Size`: Spinner size (small, medium, large)

**Usage**:
```razor
<LoadingSpinner Message="Loading models..." />
```

**Integration**: Copy .razor file, add to _Imports.razor.

---

### ErrorDisplay Component
**Purpose**: Error display with retry capability

**File**: `src/BlazorFrontend/Components/Shared/ErrorDisplay.razor`

**Dependencies**: None

**Parameters**:
- `ErrorMessage`: Error text to display
- `OnRetry`: EventCallback for retry button

**Usage**:
```razor
<ErrorDisplay ErrorMessage="@errorMessage" OnRetry="LoadDataAsync" />
```

**Integration**: Copy .razor file.

---

### SearchPage Component
**Purpose**: Full-featured search page with filters

**File**: `src/BlazorFrontend/Pages/SearchPage.razor`

**Dependencies**:
- `IApiClient`
- `LoadingSpinner`
- `ErrorDisplay`
- `SearchRequestDto`
- `PagedResultDto<ModelDto>`

**Key Features**:
- Text search
- Tag filtering
- Source filtering
- Pagination
- Sorting
- Grid display

**Integration**:
1. Copy .razor file
2. Update API client references
3. Customize UI styling

---

### ModelDetailPage Component
**Purpose**: Model detail view with versions

**File**: `src/BlazorFrontend/Pages/ModelDetailPage.razor`

**Dependencies**:
- `IApiClient`
- `ModelDto`

**Key Features**:
- Model metadata display
- Version tabs
- Image gallery
- Tag list
- Download links

**Integration**: Copy and adapt to your model entity.

---

### RunsDashboardPage Component
**Purpose**: Dashboard showing ingestion run statistics

**File**: `src/BlazorFrontend/Pages/RunsDashboardPage.razor`

**Dependencies**:
- `IApiClient`
- `RunDto`

**Key Features**:
- Statistics cards
- Run history table
- Status badges
- Duration calculation
- Auto-refresh

**Integration**: Use for monitoring background jobs.

---

### MainLayout Component
**Purpose**: Application layout wrapper

**File**: `src/BlazorFrontend/Components/Layout/MainLayout.razor`

**Dependencies**: `NavMenu`

**Integration**: Standard Blazor layout pattern.

---

### NavMenu Component
**Purpose**: Navigation menu

**File**: `src/BlazorFrontend/Components/Layout/NavMenu.razor`

**Dependencies**: None

**Integration**: Customize navigation links.

---

## Entity Framework Configurations

### Model Configuration
**Purpose**: Fluent API configuration for Model entity

**File**: `src/UMLMM.Infrastructure/Persistence/Configurations/ModelConfiguration.cs`

**Key Configurations**:
- Primary key
- Required fields
- String max lengths
- Indexes on ExternalId + SourceId
- One-to-many relationships
- Many-to-many with tags

**Pattern**:
```csharp
public class ModelConfiguration : IEntityTypeConfiguration<Model>
{
    public void Configure(EntityTypeBuilder<Model> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(m => new { m.ExternalId, m.SourceId })
            .IsUnique();

        builder.HasOne(m => m.Source)
            .WithMany()
            .HasForeignKey(m => m.SourceId);

        builder.HasMany(m => m.Versions)
            .WithOne(v => v.Model)
            .HasForeignKey(v => v.ModelId);
    }
}
```

**Integration**: Copy pattern for any entity configuration.

---

### Image Configuration
**Purpose**: Configuration for Image entity with JSONB

**File**: `src/UMLMM.Infrastructure/Persistence/Configurations/ImageConfiguration.cs`

**Key Configurations**:
- JSONB column for metadata (PostgreSQL)
- Indexes on ExternalId, Score, Rating
- Enum conversion
- String lengths

**Integration**: Use for PostgreSQL JSONB columns.

---

### Tag Configuration
**Purpose**: Configuration for Tag entity

**File**: `src/UMLMM.Infrastructure/Persistence/Configurations/TagConfiguration.cs`

**Key Configurations**:
- Unique index on Name
- Required fields
- Max lengths

**Integration**: Standard configuration pattern.

---

### ModelTag Configuration
**Purpose**: Many-to-many join table configuration

**File**: `src/UMLMM.Infrastructure/Persistence/Configurations/ModelTagConfiguration.cs`

**Key Configurations**:
- Composite primary key (ModelId, TagId)
- Relationships to both sides

**Pattern**:
```csharp
builder.HasKey(mt => new { mt.ModelId, mt.TagId });

builder.HasOne(mt => mt.Model)
    .WithMany(m => m.ModelTags)
    .HasForeignKey(mt => mt.ModelId);

builder.HasOne(mt => mt.Tag)
    .WithMany(t => t.ModelTags)
    .HasForeignKey(mt => mt.TagId);
```

**Integration**: Use for any many-to-many relationship.

---

## API Endpoints

### Model Endpoints
**Purpose**: REST API for model operations

**File**: `src/GatewayApi/Endpoints/ModelEndpoints.cs`

**Endpoints**:
- `GET /api/models` - List models (paginated)
- `GET /api/models/search` - Search models
- `GET /api/models/{id}` - Get model by ID

**Pattern**:
```csharp
public static class ModelEndpoints
{
    public static void MapModelEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/models")
            .WithTags("Models")
            .WithOpenApi();

        group.MapGet("/", async (UmlmmDbContext db, int page = 1, int size = 20) =>
        {
            var models = await db.Models
                .Include(m => m.Versions)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            return Results.Ok(models);
        });

        group.MapGet("/{id}", async (int id, UmlmmDbContext db) =>
        {
            var model = await db.Models
                .Include(m => m.Versions)
                .Include(m => m.ModelTags)
                .ThenInclude(mt => mt.Tag)
                .FirstOrDefaultAsync(m => m.Id == id);

            return model is not null ? Results.Ok(model) : Results.NotFound();
        });
    }
}
```

**Integration**: Use minimal API pattern for REST endpoints.

---

### Image Endpoints
**Purpose**: REST API for image operations

**File**: `src/GatewayApi/Endpoints/ImageEndpoints.cs`

**Endpoints**:
- `GET /api/images/search` - Search images
- `GET /api/images/{id}` - Get image by ID

**Integration**: Similar to Model Endpoints pattern.

---

### Run Endpoints
**Purpose**: REST API for ingestion run monitoring

**File**: `src/GatewayApi/Endpoints/RunEndpoints.cs`

**Endpoints**:
- `GET /api/runs` - List recent runs
- `GET /api/runs/{id}` - Get run details

**Integration**: Use for monitoring background jobs.

---

## Integration Guide

### 1. Integrating the Repository Pattern

**Step 1**: Copy the interface and entity
```bash
# Copy files:
- UMLMM.Domain/Common/BaseEntity.cs
- UMLMM.Domain/Entities/Model.cs (or your entity)
- UMLMM.Infrastructure/Repositories/IModelRepository.cs
- UMLMM.Infrastructure/Repositories/ModelRepository.cs
```

**Step 2**: Register in DI container
```csharp
services.AddScoped<IModelRepository, ModelRepository>();
```

**Step 3**: Use in your service
```csharp
public class MyService
{
    private readonly IModelRepository _repository;

    public MyService(IModelRepository repository)
    {
        _repository = repository;
    }

    public async Task ProcessAsync()
    {
        var models = await _repository.GetAllAsync();
        // ...
    }
}
```

---

### 2. Integrating a Data Ingestion Service

**Step 1**: Copy required files
```bash
# Copy:
- Domain entities
- Repository interface and implementation
- API client
- Mapper class
- Ingestion service
```

**Step 2**: Configure appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=mydb;Username=user;Password=pass"
  },
  "ExternalApis": {
    "CivitAI": {
      "BaseUrl": "https://civitai.com/api/v1",
      "PageSize": 100
    }
  }
}
```

**Step 3**: Register services
```csharp
services.AddDbContext<UmlmmDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

services.AddHttpClient<CivitAIApiClient>();
services.AddScoped<IModelRepository, ModelRepository>();
services.AddScoped<CivitAIIngestionService>();
```

**Step 4**: Run ingestion
```csharp
var service = scope.ServiceProvider.GetRequiredService<CivitAIIngestionService>();
await service.IngestAsync(cancellationToken);
```

---

### 3. Integrating Blazor Components

**Step 1**: Copy component files
```bash
# Copy:
- Components/Shared/LoadingSpinner.razor
- Components/Shared/ErrorDisplay.razor
```

**Step 2**: Add to _Imports.razor
```razor
@using YourProject.Components.Shared
```

**Step 3**: Use in pages
```razor
@page "/my-page"

@if (isLoading)
{
    <LoadingSpinner Message="Loading data..." />
}
else if (errorMessage != null)
{
    <ErrorDisplay ErrorMessage="@errorMessage" OnRetry="LoadDataAsync" />
}
else
{
    <!-- Your content -->
}

@code {
    private bool isLoading = true;
    private string? errorMessage;

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        isLoading = true;
        errorMessage = null;

        try
        {
            // Load data
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
        finally
        {
            isLoading = false;
        }
    }
}
```

---

### 4. Integrating Background Jobs with Quartz.NET

**Step 1**: Install NuGet package
```bash
dotnet add package Quartz.Extensions.Hosting
```

**Step 2**: Copy job classes
```bash
# Copy:
- Jobs/BaseIngestionJob.cs
- Jobs/CivitAIIngestionJob.cs (or your job)
- Configuration/JobScheduleConfig.cs
```

**Step 3**: Configure in Program.cs
```csharp
builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();

    var jobKey = new JobKey("CivitAIIngestionJob");
    q.AddJob<CivitAIIngestionJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("CivitAIIngestionJob-trigger")
        .WithCronSchedule("0 0 2 * * ?") // 2 AM daily
    );
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
```

**Step 4**: Add appsettings.json configuration
```json
{
  "JobSchedules": [
    {
      "JobName": "CivitAI",
      "CronExpression": "0 0 2 * * ?",
      "Enabled": true
    }
  ]
}
```

---

### 5. Integrating the API Client Pattern

**Step 1**: Copy DTOs
```bash
# Copy:
- Contracts/DTOs/ModelDto.cs
- Contracts/DTOs/PagedResult.cs
- Contracts/DTOs/SearchRequestDto.cs
```

**Step 2**: Copy API client interface and implementation
```bash
# Copy:
- Services/IApiClient.cs
- Services/ApiClient.cs
```

**Step 3**: Register HttpClient
```csharp
builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
{
    client.BaseAddress = new Uri("https://your-api.com");
    client.DefaultRequestHeaders.Add("User-Agent", "YourApp/1.0");
});
```

**Step 4**: Use in services
```csharp
public class MyService
{
    private readonly IApiClient _apiClient;

    public MyService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<PagedResult<ModelDto>> SearchAsync(string query)
    {
        var request = new SearchRequestDto
        {
            Query = query,
            PageNumber = 1,
            PageSize = 20
        };

        return await _apiClient.SearchModelsAsync(request);
    }
}
```

---

### 6. Integrating Entity Framework Configurations

**Step 1**: Copy configuration classes
```bash
# Copy:
- Persistence/Configurations/ModelConfiguration.cs
- Persistence/Configurations/TagConfiguration.cs
```

**Step 2**: Apply in DbContext
```csharp
public class MyDbContext : DbContext
{
    public DbSet<Model> Models { get; set; }
    public DbSet<Tag> Tags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ModelConfiguration());
        modelBuilder.ApplyConfiguration(new TagConfiguration());

        // Or apply all at once:
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MyDbContext).Assembly);
    }
}
```

**Step 3**: Create migration
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

### 7. Integrating Minimal API Endpoints

**Step 1**: Copy endpoint classes
```bash
# Copy:
- Endpoints/ModelEndpoints.cs
- Endpoints/ImageEndpoints.cs
```

**Step 2**: Map in Program.cs
```csharp
var app = builder.Build();

app.MapModelEndpoints();
app.MapImageEndpoints();

app.Run();
```

**Step 3**: Add OpenAPI/Swagger (optional)
```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

app.UseSwagger();
app.UseSwaggerUI();
```

---

## Dependency Matrix

| Component | Dependencies | NuGet Packages |
|-----------|--------------|----------------|
| Domain Entities | None | None |
| DTOs | None | None |
| Repository Pattern | Domain, EF Core | Microsoft.EntityFrameworkCore |
| API Clients | DTOs, HttpClient | System.Net.Http.Json |
| Ingestion Services | Domain, Repository, API Client | EF Core, HttpClient |
| Background Jobs | Services | Quartz.Extensions.Hosting |
| Blazor Components | DTOs, API Client | None (built-in Blazor) |
| EF Configurations | Domain | Microsoft.EntityFrameworkCore |
| API Endpoints | Domain, DTOs, EF Core | Microsoft.AspNetCore.OpenApi |

---

## Common NuGet Packages

### Required for most integrations:
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
<PackageReference Include="System.Net.Http.Json" Version="8.0.0" />
```

### For background jobs:
```xml
<PackageReference Include="Quartz.Extensions.Hosting" Version="3.8.0" />
```

### For Blazor:
```xml
<PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="8.0.0" />
```

### For API:
```xml
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
```

---

## Best Practices

### 1. Separation of Concerns
- Keep domain entities clean (no infrastructure concerns)
- Use DTOs for API boundaries
- Repository pattern for data access abstraction

### 2. Dependency Injection
- Register services with appropriate lifetimes
- Use interfaces for testability
- Avoid service locator pattern

### 3. Error Handling
- Use try-catch in services
- Log errors with ILogger
- Return appropriate HTTP status codes

### 4. Async/Await
- All I/O operations should be async
- Pass CancellationToken through call stack
- Use ConfigureAwait(false) in libraries

### 5. Configuration
- Use appsettings.json for environment-specific settings
- Use Options pattern for strongly-typed configuration
- Don't hardcode connection strings or API keys

---

## Quick Start Checklist

To integrate a feature from UMLMM into your project:

- [ ] Identify the component you need
- [ ] Review dependencies in the matrix above
- [ ] Copy required domain entities
- [ ] Copy required DTOs
- [ ] Copy service/repository classes
- [ ] Install NuGet packages
- [ ] Register services in DI container
- [ ] Add configuration to appsettings.json
- [ ] Create EF migrations (if using EF)
- [ ] Test integration
- [ ] Update namespace references

---

## Support

For questions or issues with integration:

1. Review the original source code in this repository
2. Check the dependency matrix for missing references
3. Ensure all NuGet packages are installed
4. Verify configuration in appsettings.json
5. Check DI registration in Program.cs

---

## License

Components are extracted from the UMLMM project and inherit the same license.

---

**Document Version**: 1.0
**Last Updated**: 2025-11-15
**UMLMM Commit**: 731532a
