# UMLMM
Unified Model/Media Metadata — ingestion (CivitAI, Danbooru, e621, ComfyUI, Ollama), centralized PostgreSQL, orchestration, and Blazor Server admin UI.

## Overview
UMLMM is a comprehensive metadata management system designed to ingest, normalize, and manage model and media metadata from multiple sources. The system provides a unified interface for searching, viewing, and managing models and monitoring ingestion runs.

## Architecture

### Components
- **Contracts**: Shared data transfer objects (DTOs) used across the application
- **BlazorFrontend**: Web-based admin UI built with Blazor Server
- **ApiGateway**: (Future) RESTful API for data access
- **Ingestion Services**: (Future) Data ingestion from multiple sources
- **Database**: (Future) PostgreSQL with normalized schema

## Phase 9: Blazor Frontend

This repository currently implements Phase 9 of the UMLMM project, which includes:

### Features
1. **Search Page** (`/search`)
   - Server-side pagination
   - Filters: source, rating, tags
   - Text search across model names and descriptions
   - Visual card-based layout with images

2. **Model Detail/Edit Page** (`/model/{id}`)
   - View model metadata, versions, images, and tags
   - Edit model information
   - Manage versions and images

3. **Runs Dashboard** (`/runs`)
   - Monitor ingestion runs
   - View run statistics (completed, running, failed)
   - Real-time status updates

### Technology Stack
- **.NET 8.0**: Modern cross-platform framework
- **Blazor Server**: Interactive web UI with C#
- **bUnit**: Component testing framework
- **xUnit**: Unit testing framework
- **Bootstrap 5**: Responsive UI styling

## Getting Started

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later

### Building the Project

```bash
# Clone the repository
git clone https://github.com/LuisVDataIntelligence/UMLMM.git
cd UMLMM

# Restore dependencies and build
dotnet restore
dotnet build
```

### Running the Application

```bash
# Run the Blazor Frontend
cd src/BlazorFrontend
dotnet run

# The application will be available at:
# https://localhost:5001 (HTTPS)
# http://localhost:5000 (HTTP)
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run tests with coverage (requires coverlet)
dotnet test /p:CollectCoverage=true
```

## Project Structure

```
UMLMM/
├── docs/                           # Documentation
│   ├── phase-09-blazor-frontend.md
│   └── AGENT-INSTRUCTIONS.md
├── src/                            # Source code
│   ├── BlazorFrontend/            # Blazor Server application
│   │   ├── Components/
│   │   │   ├── Layout/           # Layout components
│   │   │   └── Shared/           # Shared components
│   │   ├── Pages/                # Page components
│   │   ├── Services/             # API client and services
│   │   └── wwwroot/              # Static files
│   └── Contracts/                 # Shared DTOs
│       └── DTOs/
├── tests/                          # Test projects
│   └── BlazorFrontend.Tests/     # Component tests
└── UMLMM.sln                      # Solution file
```

## Configuration

The Blazor Frontend can be configured via `appsettings.json`:

```json
{
  "ApiGateway": {
    "BaseUrl": "http://localhost:5000"
  }
}
```

Currently, the application uses mock data for demonstration purposes. When the API Gateway is implemented, it will automatically connect to the real backend.

## Development

### Adding New Pages
1. Create a new `.razor` file in `src/BlazorFrontend/Pages/`
2. Define the route using `@page "/your-route"`
3. Inject required services using `@inject`
4. Add navigation links in `NavMenu.razor`

### Creating Tests
1. Create a new test file in `tests/BlazorFrontend.Tests/`
2. Inherit from `TestContext` (bUnit)
3. Mock dependencies using NSubstitute
4. Use the Arrange-Act-Assert pattern

### Coding Standards
- Follow Microsoft C# coding conventions
- Use nullable reference types
- Implement proper error handling
- Add XML documentation for public APIs
- Write tests for new components

## Documentation

For detailed information about the project:
- [Phase 9 Documentation](docs/phase-09-blazor-frontend.md)
- [Agent Instructions](docs/AGENT-INSTRUCTIONS.md)

## Current Status

### ✅ Completed
- Project structure and configuration
- Shared DTOs (Models, Runs, Search)
- API client with mock data support
- Search page with pagination and filters
- Model detail/edit page
- Runs dashboard with statistics
- Layout and navigation
- Shared components (Loading, Error display)
- Comprehensive bUnit test suite
- Documentation

### 🚧 Future Work
- API Gateway implementation
- Real backend integration
- Database schema and migrations
- Ingestion services
- Authentication and authorization
- Advanced filtering and sorting
- Export functionality
- Real-time updates via SignalR

## Testing

The project includes comprehensive test coverage:
- **Shared Components**: Loading spinner, error display
- **Search Page**: Rendering, filtering, pagination
- **Model Detail Page**: View, edit, versions
- **Runs Dashboard**: Statistics, status display, refresh

Run tests to ensure everything works:
```bash
dotnet test
```

## Contributing

1. Create a feature branch
2. Make your changes
3. Add/update tests
4. Ensure all tests pass
5. Update documentation
6. Submit a pull request

## License

[Add License Information]

## Contact

[Add Contact Information]

