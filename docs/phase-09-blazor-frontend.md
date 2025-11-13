# Phase 9 — Blazor Frontend

## Overview
Build a Blazor Server admin UI for search and reporting over the normalized schema. This provides a user-friendly interface for managing models, viewing runs, and searching through the unified metadata.

## Architecture

### Components
- **BlazorFrontend**: Main Blazor Server application
- **Contracts**: Shared DTOs for data transfer
- **API Gateway Integration**: HttpClient-based communication

### Pages
1. **Search Page**: Server-side pagination with filters and text search
2. **Model Detail/Edit**: View and edit model metadata, versions, images, and tags
3. **Runs Dashboard**: Monitor recent ingestion runs with status and statistics

## Implementation Details

### Project Structure
```
src/
  BlazorFrontend/
    Pages/
      SearchPage.razor
      ModelDetailPage.razor
      RunsDashboardPage.razor
    Components/
      Layout/
        MainLayout.razor
        NavMenu.razor
      Shared/
        LoadingSpinner.razor
        ErrorDisplay.razor
    Services/
      ApiClient.cs
    Program.cs
    App.razor
  Contracts/
    DTOs/
      ModelDto.cs
      RunDto.cs
      SearchResultDto.cs
      PagedResultDto.cs
tests/
  BlazorFrontend.Tests/
    SearchPageTests.cs
    ModelDetailPageTests.cs
    RunsDashboardPageTests.cs
```

### Features

#### Search Page
- Server-side pagination for efficient data handling
- Filters:
  - Source (CivitAI, Danbooru, e621, ComfyUI, Ollama)
  - Rating
  - Tags
- Text search across model names and descriptions
- Real-time loading states
- Error handling and display

#### Model Detail/Edit
- Display model information:
  - Basic metadata
  - Multiple versions
  - Associated images
  - Tags
- Edit flows:
  - Update model metadata
  - Add/remove tags
  - Manage versions
- Validation and error handling

#### Runs Dashboard
- Recent ingestion runs display
- Status indicators (Running, Completed, Failed)
- Statistics:
  - Start/end time
  - Records processed
  - Errors encountered
- Automatic refresh capability

## Technology Stack
- **Blazor Server**: .NET 8.0
- **HttpClient**: API communication
- **bUnit**: Component testing
- **Bootstrap**: Basic styling

## Testing Strategy

### Unit Tests (bUnit)
- Component rendering tests
- User interaction tests
- State management tests
- API response handling tests

### Integration Tests (Optional)
- End-to-end smoke tests using Playwright
- Full user workflow validation

## API Integration

### Endpoints Used
- `GET /api/models/search` - Search models with pagination and filters
- `GET /api/models/{id}` - Get model details
- `PUT /api/models/{id}` - Update model
- `GET /api/runs` - Get recent runs
- `GET /api/runs/{id}` - Get run details

### Error Handling
- Network errors
- API errors (4xx, 5xx)
- Validation errors
- Timeout handling

## Configuration
- API Gateway URL (configurable via appsettings.json)
- Pagination defaults
- Cache settings
- Timeout values

## Deployment Considerations
- Hosted as a Docker container
- Environment-based configuration
- Health checks
- Logging integration

## Acceptance Criteria
- ✓ All pages render correctly
- ✓ API integration works as expected
- ✓ Server-side pagination functions properly
- ✓ Filters work correctly
- ✓ Edit operations save successfully
- ✓ Loading states display appropriately
- ✓ Error states display appropriately
- ✓ All bUnit tests pass
- ✓ Basic styling is applied
- ✓ Navigation works correctly

## Future Enhancements
- Advanced filtering and sorting
- Bulk operations
- Export functionality
- Real-time updates via SignalR
- User authentication and authorization
- Audit logging
