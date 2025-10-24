# UMLMM Agent Instructions

## Overview
This document provides guidance for AI agents working on the UMLMM (Unified Model/Media Metadata) project. The project consists of multiple phases building a comprehensive metadata management system.

## Project Architecture

### System Components
1. **Ingestion Services**: Pull data from various sources (CivitAI, Danbooru, e621, ComfyUI, Ollama)
2. **Database**: Centralized PostgreSQL with normalized schema
3. **API Gateway**: RESTful API for data access
4. **Orchestration**: Manage and coordinate ingestion runs
5. **Blazor Frontend**: Admin UI for search and reporting

### Technology Stack
- **Backend**: .NET 8.0, C#
- **Frontend**: Blazor Server
- **Database**: PostgreSQL
- **Testing**: xUnit, bUnit, Playwright
- **Containerization**: Docker

## Development Phases

### Phase 1-8 (Prerequisites)
- Database schema design
- Ingestion pipeline setup
- API Gateway implementation
- Orchestration system
- Data contracts and DTOs

### Phase 9 (Current): Blazor Frontend
Focus on building the admin UI with three main areas:
1. Search page with pagination and filters
2. Model detail/edit views
3. Runs dashboard

## Coding Standards

### C# Guidelines
- Use latest C# language features (.NET 8.0)
- Follow Microsoft naming conventions
- Use nullable reference types
- Prefer dependency injection
- Implement proper error handling
- Use async/await consistently

### Blazor Best Practices
- Component-based architecture
- Separate presentation from logic
- Use code-behind for complex logic
- Implement proper loading states
- Handle errors gracefully
- Follow one-way data flow pattern

### Testing Requirements
- Unit tests for all business logic
- Component tests using bUnit
- Integration tests where appropriate
- Minimum 80% code coverage target
- Test edge cases and error conditions

## Project Structure

```
UMLMM/
├── docs/                    # Documentation
├── src/                     # Source code
│   ├── BlazorFrontend/     # Blazor Server UI
│   ├── Contracts/          # Shared DTOs
│   ├── ApiGateway/         # API layer (future)
│   ├── Orchestration/      # Run coordination (future)
│   └── Ingestion/          # Data ingestion (future)
├── tests/                   # Test projects
└── docker/                  # Docker configurations (future)
```

## Implementation Guidelines

### When Adding New Features
1. Review existing code and patterns
2. Check for reusable components/services
3. Follow established architectural patterns
4. Add appropriate tests
5. Update documentation
6. Consider error handling and edge cases

### When Creating Components
1. Keep components focused and single-purpose
2. Use parameters for data input
3. Use EventCallback for data output
4. Implement loading and error states
5. Add XML documentation
6. Create corresponding tests

### When Working with APIs
1. Use HttpClient through DI
2. Handle network errors
3. Implement retry logic where appropriate
4. Use DTOs for data transfer
5. Validate responses
6. Log errors appropriately

### When Writing Tests
1. Follow AAA pattern (Arrange, Act, Assert)
2. Use descriptive test names
3. Test one thing per test
4. Mock external dependencies
5. Use test fixtures appropriately
6. Clean up resources

## Common Patterns

### Service Registration
```csharp
builder.Services.AddScoped<IApiClient, ApiClient>();
builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiGateway:BaseUrl"]);
});
```

### Component Structure
```razor
@page "/page-route"
@using Contracts.DTOs
@inject IApiClient ApiClient

<PageTitle>Page Title</PageTitle>

@if (loading)
{
    <LoadingSpinner />
}
else if (error != null)
{
    <ErrorDisplay Message="@error" />
}
else
{
    <!-- Content -->
}

@code {
    private bool loading = true;
    private string? error;
    private DataType? data;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            data = await ApiClient.GetDataAsync();
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }
        finally
        {
            loading = false;
        }
    }
}
```

### bUnit Test Structure
```csharp
[Fact]
public void Component_ShouldRenderCorrectly()
{
    // Arrange
    using var ctx = new TestContext();
    var mockApiClient = Substitute.For<IApiClient>();
    ctx.Services.AddSingleton(mockApiClient);

    // Act
    var cut = ctx.RenderComponent<ComponentName>();

    // Assert
    cut.Find("expected-element").Should().NotBeNull();
}
```

## Error Handling Strategy

### UI Layer
- Display user-friendly error messages
- Log detailed errors for debugging
- Provide recovery options where possible
- Never expose sensitive information

### API Communication
- Catch HttpRequestException
- Handle timeouts
- Parse API error responses
- Implement retry logic for transient failures

### Validation
- Validate user input on client side
- Validate data from APIs
- Provide clear validation messages
- Prevent invalid state

## Performance Considerations

### Blazor Server
- Minimize SignalR traffic
- Use virtualization for large lists
- Implement proper pagination
- Cache when appropriate
- Optimize re-renders

### API Calls
- Use async/await
- Implement cancellation tokens
- Batch requests when possible
- Cache responses appropriately
- Handle concurrent requests

## Security Considerations

### Data Handling
- Validate all input
- Sanitize display data
- Use parameterized queries (when applicable)
- Follow principle of least privilege

### API Communication
- Use HTTPS
- Implement authentication (future)
- Include authorization checks (future)
- Validate API responses

## Debugging Tips

### Blazor Debugging
- Use browser developer tools
- Check browser console for errors
- Use Blazor debugging in Visual Studio/VS Code
- Inspect SignalR traffic
- Use logging liberally

### Common Issues
- Component not updating: Check StateHasChanged()
- API calls failing: Verify base URL and endpoints
- Tests failing: Check for proper mocking
- Navigation issues: Verify route parameters

## Resources

### Documentation
- [Blazor Documentation](https://docs.microsoft.com/aspnet/core/blazor)
- [bUnit Documentation](https://bunit.dev)
- [.NET Documentation](https://docs.microsoft.com/dotnet)

### Project-Specific
- See docs/phase-09-blazor-frontend.md for frontend details
- See src/Contracts for available DTOs
- See tests/ for test examples

## Getting Help

When stuck:
1. Review this document
2. Check existing code for patterns
3. Consult relevant documentation
4. Review test cases for examples
5. Check error messages carefully

## Phase 9 Specific Notes

### Current Focus
Building the Blazor frontend with three main pages:
1. **Search Page**: List models with filters and pagination
2. **Model Detail**: View and edit individual models
3. **Runs Dashboard**: Monitor ingestion runs

### Key Requirements
- Server-side pagination for performance
- Multiple filter options
- Loading and error states
- Responsive design
- bUnit test coverage

### Dependencies
- Assumes API Gateway exists (will mock for now)
- Uses Contracts DTOs (to be created)
- Requires HttpClient configuration

### Testing Approach
- bUnit for component tests
- Mock API responses
- Test loading states
- Test error handling
- Test user interactions

### Success Criteria
- Pages render correctly
- API integration works
- Filters and pagination function
- Tests pass
- Basic styling applied
