# UMLMM
Unified Model/Media Metadata — ingestion (CivitAI, Danbooru, e621, ComfyUI, Ollama), centralized PostgreSQL, orchestration, and Blazor Server admin UI.

## Gateway API

The Gateway API provides core endpoints for managing models, versions, tags, images, and runs.

### Features

- **Models API**: List, search, and retrieve AI models with version summaries
- **Model Versions API**: Get version details and list versions by model
- **Tags API**: Manage tags and tag assignments to models
- **Images API**: Search and filter images by model version, hash, or rating
- **Runs API**: Track workflow execution history
- **Pagination**: All list endpoints support pagination with metadata
- **Filtering & Sorting**: Advanced filtering and sorting capabilities
- **Swagger UI**: Interactive API documentation

### Running the API

```bash
cd src/GatewayApi
dotnet run
```

The API will be available at `http://localhost:5125` (or the configured port).
Swagger UI will be available at the root URL for interactive testing.

### Testing

```bash
cd tests/GatewayApi.Tests
dotnet test
```

### Endpoints

#### Models
- `GET /api/models` - List and search models with pagination
- `GET /api/models/{id}` - Get model by ID with version summaries

#### Model Versions
- `GET /api/model-versions/{id}` - Get model version by ID
- `GET /api/model-versions/by-model/{modelId}` - List versions for a specific model

#### Tags
- `GET /api/tags` - List and search tags
- `POST /api/tags/assign` - Assign a tag to a model
- `DELETE /api/tags/remove` - Remove a tag from a model

#### Images
- `GET /api/images` - List and search images by model version, hash, or rating
- `GET /api/images/{id}` - Get image by ID

#### Runs
- `GET /api/runs` - List latest runs with optional status filter
- `GET /api/runs/{id}` - Get run by ID

### Architecture

The solution is organized into three projects:

- **GatewayApi**: ASP.NET Core minimal API implementation
- **Contracts**: DTOs and shared contracts
- **Infrastructure**: Entity models and EF Core DbContext

For more details, see the Swagger documentation at the root URL when running the API.
