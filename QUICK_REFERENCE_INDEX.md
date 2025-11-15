# UMLMM Project - Quick Reference Index

This index provides a fast lookup for all classes and components in the UMLMM project, organized alphabetically and by category.

## Documentation Files

| Document | Purpose |
|----------|---------|
| [COMPONENT_BREAKDOWN.md](./COMPONENT_BREAKDOWN.md) | Comprehensive breakdown of all C# classes with integration guides |
| [RAZOR_COMPONENTS_REFERENCE.md](./RAZOR_COMPONENTS_REFERENCE.md) | Complete Razor component reference with source code |
| [QUICK_REFERENCE_INDEX.md](./QUICK_REFERENCE_INDEX.md) | This file - quick lookup index |

---

## Alphabetical Class Index

| Class Name | File Path | Category | Dependencies |
|------------|-----------|----------|--------------|
| ApiClient | `/src/BlazorFrontend/Services/ApiClient.cs` | HTTP Client | HttpClient, ILogger |
| Artifact | `/src/UMLMM.Domain/Entities/Artifact.cs` | Domain Entity | BaseEntity, ModelVersion |
| ArtifactConfiguration | `/src/UMLMM.Infrastructure/Persistence/Configurations/ArtifactConfiguration.cs` | EF Configuration | EF Core |
| ArtifactKind | `/src/UMLMM.Domain/Enums/ArtifactKind.cs` | Enum | None |
| BaseEntity | `/src/UMLMM.Domain/Common/BaseEntity.cs` | Domain Base | None |
| BaseIngestionJob | `/src/UMLMM.Orchestrator/Jobs/BaseIngestionJob.cs` | Background Job | Quartz.NET |
| CivitAIApiClient | `/src/UMLMM.Ingestors.CivitAI/CivitAI/Client/CivitAIApiClient.cs` | HTTP Client | HttpClient |
| CivitAIDtos | `/src/UMLMM.Ingestors.CivitAI/CivitAI/DTOs/CivitAIDtos.cs` | DTO | None |
| CivitAIIngestionJob | `/src/UMLMM.Orchestrator/Jobs/CivitAIIngestionJob.cs` | Background Job | BaseIngestionJob, Quartz.NET |
| CivitAIIngestionService | `/src/UMLMM.Ingestors.CivitAI/Services/CivitAIIngestionService.cs` | Ingestion | CivitAIApiClient, Repository |
| CivitAIMapper | `/src/UMLMM.Ingestors.CivitAI/Mapping/CivitAIMapper.cs` | Mapper | Domain, DTOs |
| ComfyUIIngestorOptions | `/src/UMLMM.ComfyUIIngestor/Configuration/ComfyUIIngestorOptions.cs` | Configuration | None |
| DanbooruApiClient | `/src/UMLMM.DanbooruIngestor/Danbooru/DanbooruApiClient.cs` | HTTP Client | HttpClient |
| DanbooruIngestionService | `/src/UMLMM.DanbooruIngestor/Services/DanbooruIngestionService.cs` | Ingestion | DanbooruApiClient, Repository |
| DanbooruMapper | `/src/UMLMM.DanbooruIngestor/Mapping/DanbooruMapper.cs` | Mapper | Domain, DTOs |
| DanbooruPostDto | `/src/UMLMM.DanbooruIngestor/Danbooru/DanbooruPostDto.cs` | DTO | None |
| DanbooruSettings | `/src/UMLMM.DanbooruIngestor/Configuration/DanbooruSettings.cs` | Configuration | None |
| E621ApiClient | `/src/UMLMM.E621Ingestor/Client/E621ApiClient.cs` | HTTP Client | HttpClient |
| E621DTOs | `/src/UMLMM.E621Ingestor/Client/DTOs/E621DTOs.cs` | DTO | None |
| E621IngestorService | `/src/UMLMM.E621Ingestor/Services/E621IngestorService.cs` | Ingestion | E621ApiClient, Repository |
| E621Mapper | `/src/UMLMM.E621Ingestor/Mapping/E621Mapper.cs` | Mapper | Domain, DTOs |
| ErrorDisplay.razor | `/src/BlazorFrontend/Components/Shared/ErrorDisplay.razor` | UI Component | EventCallback |
| FetchRun | `/src/UMLMM.Domain/Entities/FetchRun.cs` | Domain Entity | BaseEntity, Source |
| FetchRunConfiguration | `/src/UMLMM.Infrastructure/Persistence/Configurations/FetchRunConfiguration.cs` | EF Configuration | EF Core |
| FetchStatus | `/src/UMLMM.Domain/Enums/FetchStatus.cs` | Enum | None |
| IApiClient | `/src/BlazorFrontend/Services/IApiClient.cs` | Interface | DTOs |
| IDanbooruApiClient | `/src/UMLMM.DanbooruIngestor/Danbooru/IDanbooruApiClient.cs` | Interface | DTOs |
| Image | `/src/UMLMM.Domain/Entities/Image.cs` | Domain Entity | BaseEntity, Source |
| ImageConfiguration | `/src/UMLMM.Infrastructure/Persistence/Configurations/ImageConfiguration.cs` | EF Configuration | EF Core |
| ImageDto | `/src/Contracts/DTOs/ImageDto.cs` | DTO | None |
| ImageEndpoints | `/src/GatewayApi/Endpoints/ImageEndpoints.cs` | API Endpoint | Minimal API |
| ImageTag | `/src/UMLMM.Domain/Entities/ImageTag.cs` | Join Entity | Image, Tag |
| IModelRepository | `/src/UMLMM.Infrastructure/Repositories/IModelRepository.cs` | Interface | Domain |
| JobScheduleConfig | `/src/UMLMM.Orchestrator/Configuration/JobScheduleConfig.cs` | Configuration | None |
| JobSchedulesConfig | `/src/UMLMM.Orchestrator/Configuration/JobSchedulesConfig.cs` | Configuration | None |
| JsonModelRepository | `/src/UMLMM.Infrastructure/Repositories/JsonModelRepository.cs` | Repository | IModelRepository, JSON |
| LoadingSpinner.razor | `/src/BlazorFrontend/Components/Shared/LoadingSpinner.razor` | UI Component | None |
| MainLayout.razor | `/src/BlazorFrontend/Components/Layout/MainLayout.razor` | UI Layout | NavMenu |
| Model | `/src/UMLMM.Domain/Entities/Model.cs` | Domain Entity | BaseEntity, Source |
| ModelConfiguration | `/src/UMLMM.Infrastructure/Persistence/Configurations/ModelConfiguration.cs` | EF Configuration | EF Core |
| ModelDetailPage.razor | `/src/BlazorFrontend/Pages/ModelDetailPage.razor` | UI Page | IApiClient, DTOs |
| ModelDto | `/src/Contracts/DTOs/ModelDto.cs` | DTO | None |
| ModelEndpoints | `/src/GatewayApi/Endpoints/ModelEndpoints.cs` | API Endpoint | Minimal API |
| ModelRepository | `/src/UMLMM.Infrastructure/Repositories/ModelRepository.cs` | Repository | IModelRepository, EF Core |
| ModelTag | `/src/UMLMM.Domain/Entities/ModelTag.cs` | Join Entity | Model, Tag |
| ModelTagConfiguration | `/src/UMLMM.Infrastructure/Persistence/Configurations/ModelTagConfiguration.cs` | EF Configuration | EF Core |
| ModelType | `/src/UMLMM.Domain/Enums/ModelType.cs` | Enum | None |
| ModelVersion | `/src/UMLMM.Domain/Entities/ModelVersion.cs` | Domain Entity | BaseEntity, Model |
| ModelVersionConfiguration | `/src/UMLMM.Infrastructure/Persistence/Configurations/ModelVersionConfiguration.cs` | EF Configuration | EF Core |
| ModelVersionEndpoints | `/src/GatewayApi/Endpoints/ModelVersionEndpoints.cs` | API Endpoint | Minimal API |
| NavMenu.razor | `/src/BlazorFrontend/Components/Layout/NavMenu.razor` | UI Component | NavLink |
| NsfwRating | `/src/UMLMM.Domain/Enums/NsfwRating.cs` | Enum | None |
| OllamaClient | `/src/OllamaIngestor/Services/OllamaClient.cs` | HTTP Client | HttpClient |
| OllamaIngestionService | `/src/OllamaIngestor/Services/OllamaIngestionService.cs` | Ingestion | OllamaClient, Repository |
| OllamaModels | `/src/OllamaIngestor/Models/OllamaModels.cs` | DTO | None |
| PagedResult | `/src/Contracts/DTOs/PagedResult.cs` | DTO | Generic |
| PagedResultDto | `/src/Contracts/DTOs/PagedResultDto.cs` | DTO | Generic |
| Prompt | `/src/UMLMM.Domain/Entities/Prompt.cs` | Domain Entity | BaseEntity, Image |
| PromptConfiguration | `/src/UMLMM.Infrastructure/Persistence/Configurations/PromptConfiguration.cs` | EF Configuration | EF Core |
| RunDto | `/src/Contracts/DTOs/RunDto.cs` | DTO | None |
| RunEndpoints | `/src/GatewayApi/Endpoints/RunEndpoints.cs` | API Endpoint | Minimal API |
| RunsDashboardPage.razor | `/src/BlazorFrontend/Pages/RunsDashboardPage.razor` | UI Page | IApiClient, DTOs |
| SearchPage.razor | `/src/BlazorFrontend/Pages/SearchPage.razor` | UI Page | IApiClient, DTOs |
| SearchRequestDto | `/src/Contracts/DTOs/SearchRequestDto.cs` | DTO | None |
| Source | `/src/UMLMM.Domain/Entities/Source.cs` | Domain Entity | BaseEntity |
| SourceConfiguration | `/src/UMLMM.Infrastructure/Persistence/Configurations/SourceConfiguration.cs` | EF Configuration | EF Core |
| Tag | `/src/UMLMM.Domain/Entities/Tag.cs` | Domain Entity | BaseEntity |
| TagConfiguration | `/src/UMLMM.Infrastructure/Persistence/Configurations/TagConfiguration.cs` | EF Configuration | EF Core |
| TagDto | `/src/Contracts/DTOs/TagDto.cs` | DTO | None |
| TagEndpoints | `/src/GatewayApi/Endpoints/TagEndpoints.cs` | API Endpoint | Minimal API |
| UmlmmDbContext | `/src/UMLMM.Infrastructure/Data/UmlmmDbContext.cs` | DbContext | EF Core, Domain |
| UmlmmDbContextFactory | `/src/UMLMM.Infrastructure/Data/UmlmmDbContextFactory.cs` | Factory | EF Core |
| Workflow | `/src/UMLMM.Domain/Entities/Workflow.cs` | Domain Entity | BaseEntity, Source |
| WorkflowConfiguration | `/src/UMLMM.Infrastructure/Persistence/Configurations/WorkflowConfiguration.cs` | EF Configuration | EF Core |
| WorkflowDiscovery | `/src/UMLMM.ComfyUIIngestor/Services/WorkflowDiscovery.cs` | Service | File System |
| WorkflowIngestService | `/src/UMLMM.ComfyUIIngestor/Services/WorkflowIngestService.cs` | Ingestion | WorkflowDiscovery, WorkflowParser |
| WorkflowParser | `/src/UMLMM.ComfyUIIngestor/Services/WorkflowParser.cs` | Service | JSON |

---

## Category Index

### Domain Entities
All located in `/src/UMLMM.Domain/Entities/`

| Entity | Purpose | Key Properties |
|--------|---------|----------------|
| Artifact | Model downloadable files | FileName, DownloadUrl, SizeBytes, Format |
| FetchRun | Ingestion run tracking | Status, ItemsFetched, ItemsCreated, ErrorMessage |
| Image | Image metadata | Url, Width, Height, Rating, Score |
| ImageTag | Image-Tag join | ImageId, TagId |
| Model | AI model | Name, Description, Type, ExternalId |
| ModelTag | Model-Tag join | ModelId, TagId |
| ModelVersion | Model version | VersionName, BaseModel, DownloadUrl |
| Prompt | AI generation prompt | PositivePrompt, NegativePrompt, Steps, Seed |
| Source | External data source | Name, BaseUrl, ApiVersion |
| Tag | Categorization tag | Name, DisplayName, Category |
| Workflow | ComfyUI workflow | Name, WorkflowJson, FilePath |

### DTOs (Data Transfer Objects)
All located in `/src/Contracts/DTOs/`

| DTO | Purpose |
|-----|---------|
| ImageDto | Image transfer |
| ModelDto | Model transfer with versions, tags, images |
| PagedResult<T> | Generic pagination wrapper |
| PagedResultDto<T> | Paged result transfer |
| RunDto | Ingestion run status |
| SearchRequestDto | Search query with filters |
| TagDto | Tag transfer |

### Enums
All located in `/src/UMLMM.Domain/Enums/`

| Enum | Values |
|------|--------|
| ArtifactKind | Model, VAE, Config, Pruned, TrainingData |
| FetchStatus | Running, Succeeded, Failed |
| ModelType | Checkpoint, Lora, LoCon, Embedding, Hypernetwork, ControlNet, VAE, etc. |
| NsfwRating | Safe, Questionable, Explicit |

### Razor Components

#### Shared Components
Located in `/src/BlazorFrontend/Components/Shared/`

| Component | Purpose | Parameters |
|-----------|---------|------------|
| ErrorDisplay.razor | Error message with retry | Message, ShowRetry, OnRetry |
| LoadingSpinner.razor | Loading indicator | Message |

#### Page Components
Located in `/src/BlazorFrontend/Pages/`

| Component | Route | Purpose |
|-----------|-------|---------|
| Index.razor | `/` | Home page |
| SearchPage.razor | `/search` | Model search with filters |
| ModelDetailPage.razor | `/model/{id}` | Model detail view |
| RunsDashboardPage.razor | `/runs` | Ingestion runs dashboard |

#### Layout Components
Located in `/src/BlazorFrontend/Components/Layout/`

| Component | Purpose |
|-----------|---------|
| MainLayout.razor | Main layout wrapper |
| NavMenu.razor | Navigation menu |

### Repository Pattern

| Interface | Implementation | Storage |
|-----------|----------------|---------|
| IModelRepository | ModelRepository | EF Core (PostgreSQL) |
| IModelRepository | JsonModelRepository | JSON files |
| IPostRepository | PostRepository | EF Core |

### API Clients

| Client | Target API | Location |
|--------|-----------|----------|
| ApiClient | Gateway API | `/src/BlazorFrontend/Services/ApiClient.cs` |
| CivitAIApiClient | CivitAI API | `/src/UMLMM.Ingestors.CivitAI/CivitAI/Client/CivitAIApiClient.cs` |
| DanbooruApiClient | Danbooru API | `/src/UMLMM.DanbooruIngestor/Danbooru/DanbooruApiClient.cs` |
| E621ApiClient | E621 API | `/src/UMLMM.E621Ingestor/Client/E621ApiClient.cs` |
| OllamaClient | Ollama API | `/src/OllamaIngestor/Services/OllamaClient.cs` |

### Ingestion Services

| Service | Source | Location |
|---------|--------|----------|
| CivitAIIngestionService | CivitAI | `/src/UMLMM.Ingestors.CivitAI/Services/CivitAIIngestionService.cs` |
| DanbooruIngestionService | Danbooru | `/src/UMLMM.DanbooruIngestor/Services/DanbooruIngestionService.cs` |
| E621IngestorService | E621 | `/src/UMLMM.E621Ingestor/Services/E621IngestorService.cs` |
| OllamaIngestionService | Ollama | `/src/OllamaIngestor/Services/OllamaIngestionService.cs` |
| WorkflowIngestService | ComfyUI | `/src/UMLMM.ComfyUIIngestor/Services/WorkflowIngestService.cs` |

### Background Jobs
All located in `/src/UMLMM.Orchestrator/Jobs/`

| Job | Purpose | Base Class |
|-----|---------|-----------|
| BaseIngestionJob | Abstract base for all jobs | IJob |
| CivitAIIngestionJob | Schedule CivitAI ingestion | BaseIngestionJob |
| ComfyUIIngestionJob | Schedule ComfyUI ingestion | BaseIngestionJob |
| DanbooruIngestionJob | Schedule Danbooru ingestion | BaseIngestionJob |
| E621IngestionJob | Schedule E621 ingestion | BaseIngestionJob |
| OllamaIngestionJob | Schedule Ollama ingestion | BaseIngestionJob |

### API Endpoints
All located in `/src/GatewayApi/Endpoints/`

| Endpoints Class | Prefix | Purpose |
|----------------|--------|---------|
| ImageEndpoints | `/api/images` | Image search and retrieval |
| ModelEndpoints | `/api/models` | Model search, list, get |
| ModelVersionEndpoints | `/api/versions` | Model version operations |
| RunEndpoints | `/api/runs` | Ingestion run monitoring |
| TagEndpoints | `/api/tags` | Tag search |

### EF Core Configurations
All located in `/src/UMLMM.Infrastructure/Persistence/Configurations/`

| Configuration | Entity | Key Features |
|--------------|--------|--------------|
| ArtifactConfiguration | Artifact | Foreign keys, indexes |
| FetchRunConfiguration | FetchRun | Status enum, timestamps |
| ImageConfiguration | Image | JSONB metadata, indexes |
| ModelConfiguration | Model | Composite unique index |
| ModelTagConfiguration | ModelTag | Composite primary key |
| ModelVersionConfiguration | ModelVersion | Relationships |
| PromptConfiguration | Prompt | Foreign keys |
| SourceConfiguration | Source | Unique name |
| TagConfiguration | Tag | Unique name index |
| WorkflowConfiguration | Workflow | JSONB workflow |

### Mapper Classes

| Mapper | Source → Target | Location |
|--------|----------------|----------|
| CivitAIMapper | CivitAIDtos → Domain | `/src/UMLMM.Ingestors.CivitAI/Mapping/CivitAIMapper.cs` |
| DanbooruMapper | DanbooruPostDto → Domain | `/src/UMLMM.DanbooruIngestor/Mapping/DanbooruMapper.cs` |
| E621Mapper | E621DTOs → Domain | `/src/UMLMM.E621Ingestor/Mapping/E621Mapper.cs` |

---

## Feature-Based Lookup

### "I need to add pagination to my API"

**Copy these files**:
- `/src/Contracts/DTOs/PagedResult.cs`
- `/src/Contracts/DTOs/PagedResultDto.cs`
- `/src/BlazorFrontend/Pages/SearchPage.razor` (see pagination section)

**Pattern**:
```csharp
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
```

---

### "I need to create a background job scheduler"

**Copy these files**:
- `/src/UMLMM.Orchestrator/Jobs/BaseIngestionJob.cs`
- `/src/UMLMM.Orchestrator/Configuration/JobScheduleConfig.cs`
- `/src/UMLMM.Orchestrator/Program.cs` (for Quartz.NET setup)

**NuGet Package**: `Quartz.Extensions.Hosting`

**Pattern**: See "Background Job Scheduling" in COMPONENT_BREAKDOWN.md

---

### "I need to build a REST API with minimal API pattern"

**Copy these files**:
- `/src/GatewayApi/Endpoints/ModelEndpoints.cs` (as template)
- `/src/GatewayApi/Program.cs` (for setup)

**Pattern**:
```csharp
public static class MyEndpoints
{
    public static void MapMyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/my-resource")
            .WithTags("MyResource")
            .WithOpenApi();

        group.MapGet("/", GetAll);
        group.MapGet("/{id}", GetById);
        group.MapPost("/", Create);
    }
}
```

---

### "I need a loading spinner for Blazor"

**Copy**: `/src/BlazorFrontend/Components/Shared/LoadingSpinner.razor`

**Usage**:
```razor
@if (loading)
{
    <LoadingSpinner Message="Loading data..." />
}
```

---

### "I need to implement Repository pattern with EF Core"

**Copy these files**:
- `/src/UMLMM.Infrastructure/Repositories/IModelRepository.cs`
- `/src/UMLMM.Infrastructure/Repositories/ModelRepository.cs`
- `/src/UMLMM.Domain/Entities/Model.cs` (adapt to your entity)

**Pattern**: See "Repository Pattern" in COMPONENT_BREAKDOWN.md

---

### "I need to create an HTTP API client"

**Copy these files**:
- `/src/BlazorFrontend/Services/IApiClient.cs`
- `/src/BlazorFrontend/Services/ApiClient.cs`

**Pattern**:
```csharp
public class MyApiClient
{
    private readonly HttpClient _httpClient;

    public MyApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<T> GetAsync<T>(string endpoint)
    {
        var response = await _httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }
}
```

---

### "I need to ingest data from an external API"

**Copy as template**:
- `/src/UMLMM.Ingestors.CivitAI/Services/CivitAIIngestionService.cs`
- `/src/UMLMM.Ingestors.CivitAI/CivitAI/Client/CivitAIApiClient.cs`

**Key patterns**:
- FetchRun tracking
- Paginated fetching
- Upsert for idempotency
- Error handling
- Statistics collection

**See**: "Data Ingestion Services" in COMPONENT_BREAKDOWN.md

---

### "I need a search page with filters"

**Copy**: `/src/BlazorFrontend/Pages/SearchPage.razor`

**Includes**:
- Text search
- Dropdown filters
- Pagination
- Grid display
- Loading/error states

---

### "I need to configure Entity Framework relationships"

**Copy template**: Any file in `/src/UMLMM.Infrastructure/Persistence/Configurations/`

**Patterns**:
- One-to-many: See `ModelConfiguration.cs`
- Many-to-many: See `ModelTagConfiguration.cs`
- Indexes: See `TagConfiguration.cs`
- JSONB columns: See `ImageConfiguration.cs`

---

## Quick Start Scenarios

### Scenario 1: Build a Simple CRUD API

**Files to copy**:
1. Domain entity: `/src/UMLMM.Domain/Entities/Model.cs`
2. DTO: `/src/Contracts/DTOs/ModelDto.cs`
3. Repository: `/src/UMLMM.Infrastructure/Repositories/ModelRepository.cs`
4. Endpoints: `/src/GatewayApi/Endpoints/ModelEndpoints.cs`
5. EF Config: `/src/UMLMM.Infrastructure/Persistence/Configurations/ModelConfiguration.cs`

**Time estimate**: 1-2 hours

---

### Scenario 2: Build a Blazor Search Page

**Files to copy**:
1. Search page: `/src/BlazorFrontend/Pages/SearchPage.razor`
2. Loading spinner: `/src/BlazorFrontend/Components/Shared/LoadingSpinner.razor`
3. Error display: `/src/BlazorFrontend/Components/Shared/ErrorDisplay.razor`
4. API client: `/src/BlazorFrontend/Services/ApiClient.cs`
5. DTOs: `/src/Contracts/DTOs/`

**Time estimate**: 2-3 hours

---

### Scenario 3: Set Up Background Job Scheduler

**Files to copy**:
1. Base job: `/src/UMLMM.Orchestrator/Jobs/BaseIngestionJob.cs`
2. Job config: `/src/UMLMM.Orchestrator/Configuration/JobScheduleConfig.cs`
3. Program setup: `/src/UMLMM.Orchestrator/Program.cs`

**NuGet**: `Quartz.Extensions.Hosting`

**Time estimate**: 1-2 hours

---

### Scenario 4: Implement Data Ingestion from API

**Files to copy**:
1. Ingestion service: `/src/UMLMM.Ingestors.CivitAI/Services/CivitAIIngestionService.cs`
2. API client: `/src/UMLMM.Ingestors.CivitAI/CivitAI/Client/CivitAIApiClient.cs`
3. Mapper: `/src/UMLMM.Ingestors.CivitAI/Mapping/CivitAIMapper.cs`
4. FetchRun entity: `/src/UMLMM.Domain/Entities/FetchRun.cs`
5. Repository: `/src/UMLMM.Infrastructure/Repositories/ModelRepository.cs`

**Time estimate**: 3-4 hours

---

## Technology Stack Reference

### Backend
- **.NET 8/9**: Target framework
- **EF Core 8.0**: ORM
- **PostgreSQL**: Primary database (Npgsql provider)
- **SQLite**: Alternative database
- **Quartz.NET 3.8**: Job scheduling
- **ASP.NET Core Minimal API**: REST endpoints
- **Swashbuckle**: OpenAPI/Swagger

### Frontend
- **Blazor Server (.NET 8)**: UI framework
- **Bootstrap 5.3**: CSS framework
- **Bootstrap Icons 1.10**: Icon library
- **Razor Components**: UI components (not XAML)

### Patterns
- **Clean Architecture**: Domain/Infrastructure separation
- **Repository Pattern**: Data access abstraction
- **DTO Pattern**: Data transfer
- **Minimal API**: Endpoint routing
- **Unit of Work**: Transaction management

---

## Common NuGet Packages

### Core
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
<PackageReference Include="System.Net.Http.Json" Version="8.0.0" />
```

### Background Jobs
```xml
<PackageReference Include="Quartz.Extensions.Hosting" Version="3.8.0" />
```

### Blazor
```xml
<PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="8.0.0" />
```

### API
```xml
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
```

---

## File Tree Summary

```
UMLMM/
├── src/
│   ├── BlazorFrontend/               # Blazor Server UI
│   │   ├── Components/
│   │   │   ├── Layout/              # MainLayout, NavMenu
│   │   │   └── Shared/              # LoadingSpinner, ErrorDisplay
│   │   ├── Pages/                   # Index, Search, ModelDetail, RunsDashboard
│   │   └── Services/                # IApiClient, ApiClient
│   │
│   ├── Contracts/                    # Shared DTOs
│   │   └── DTOs/                    # ModelDto, SearchRequestDto, PagedResult, etc.
│   │
│   ├── GatewayApi/                   # REST API Gateway
│   │   └── Endpoints/               # Model, Image, Tag, Run endpoints
│   │
│   ├── UMLMM.Domain/                 # Core Domain
│   │   ├── Common/                  # BaseEntity
│   │   ├── Entities/                # Model, Image, Tag, FetchRun, etc.
│   │   └── Enums/                   # ModelType, NsfwRating, FetchStatus
│   │
│   ├── UMLMM.Infrastructure/         # Data Access
│   │   ├── Data/                    # UmlmmDbContext
│   │   ├── Persistence/
│   │   │   └── Configurations/      # EF configurations
│   │   └── Repositories/            # ModelRepository, JsonModelRepository
│   │
│   ├── UMLMM.Orchestrator/           # Job Scheduler
│   │   ├── Configuration/           # JobScheduleConfig
│   │   └── Jobs/                    # BaseIngestionJob, CivitAIIngestionJob, etc.
│   │
│   ├── UMLMM.Ingestors.CivitAI/      # CivitAI Ingestor
│   │   ├── CivitAI/
│   │   │   ├── Client/              # CivitAIApiClient
│   │   │   └── DTOs/                # CivitAIDtos
│   │   ├── Mapping/                 # CivitAIMapper
│   │   └── Services/                # CivitAIIngestionService
│   │
│   ├── UMLMM.DanbooruIngestor/       # Danbooru Ingestor
│   │   ├── Danbooru/                # API client, DTOs
│   │   ├── Mapping/                 # DanbooruMapper
│   │   └── Services/                # DanbooruIngestionService
│   │
│   ├── UMLMM.E621Ingestor/           # E621 Ingestor
│   ├── UMLMM.ComfyUIIngestor/        # ComfyUI Ingestor
│   └── OllamaIngestor/               # Ollama Ingestor
│
└── Documentation/                     # THIS DIRECTORY
    ├── COMPONENT_BREAKDOWN.md        # Comprehensive class breakdown
    ├── RAZOR_COMPONENTS_REFERENCE.md # Razor components reference
    └── QUICK_REFERENCE_INDEX.md      # This file
```

---

## Color-Coded Dependency Levels

### 🟢 Level 0 - No Dependencies
- BaseEntity
- All Enums
- All DTOs
- LoadingSpinner.razor

### 🟡 Level 1 - Domain Dependencies Only
- All Domain Entities
- ErrorDisplay.razor

### 🟠 Level 2 - Infrastructure Dependencies
- EF Configurations
- Repositories
- DbContext

### 🔴 Level 3 - Service Dependencies
- API Clients
- Ingestion Services
- Background Jobs
- API Endpoints
- Blazor Pages

**Integration Tip**: Start with Level 0, work your way up.

---

## Frequently Asked Questions

**Q: Is this XAML?**
A: No, this is Razor syntax for Blazor. Blazor is web-based, not desktop (WPF/MAUI).

**Q: Can I use these components in WPF?**
A: Not directly. WPF uses XAML. You'd need to port the UI logic.

**Q: Can I use these components in Blazor WebAssembly?**
A: Yes, most components are compatible. API clients may need adjustment.

**Q: Which database is used?**
A: PostgreSQL (primary) and SQLite (alternative). EF Core abstracts the difference.

**Q: How do I test these components?**
A: Each component is self-contained. Copy to your project and test individually.

**Q: Are these production-ready?**
A: They are functional but may need hardening for production (error handling, validation, etc.).

---

## Integration Checklist

When integrating a component:

- [ ] Identify the component in this index
- [ ] Review dependencies in the table
- [ ] Copy required files
- [ ] Install NuGet packages
- [ ] Update namespaces
- [ ] Register services in DI container
- [ ] Add configuration to appsettings.json
- [ ] Test in isolation
- [ ] Integrate with your code

---

## Version Information

**Document Version**: 1.0
**Last Updated**: 2025-11-15
**UMLMM Commit**: 731532a
**Framework**: .NET 8/9
**Total Classes**: 133
**Total Razor Components**: 10

---

For detailed information about any component, refer to:
- **COMPONENT_BREAKDOWN.md** for C# classes
- **RAZOR_COMPONENTS_REFERENCE.md** for Razor components
