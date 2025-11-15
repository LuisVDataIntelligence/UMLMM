# Blazor Razor Components Reference Guide

This document provides detailed information about all Razor components in the UMLMM project, including complete source code, usage examples, and integration instructions.

> **Note**: This is a **Blazor Server** application, not WPF/MAUI/UWP. Instead of XAML, Blazor uses **Razor component syntax** (.razor files) which combines HTML markup with C# code.

## Table of Contents

1. [Shared Components](#shared-components)
   - [LoadingSpinner](#loadingspinner)
   - [ErrorDisplay](#errordisplay)
2. [Page Components](#page-components)
   - [SearchPage](#searchpage)
   - [ModelDetailPage](#modeldetailpage)
   - [RunsDashboardPage](#runsdashboardpage)
3. [Layout Components](#layout-components)
4. [Integration Guide](#integration-guide)

---

## Shared Components

### LoadingSpinner

**Purpose**: Reusable loading indicator with optional message

**File**: `src/BlazorFrontend/Components/Shared/LoadingSpinner.razor`

**Full Source Code**:

```razor
<div class="text-center my-5">
    <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Loading...</span>
    </div>
    @if (!string.IsNullOrWhiteSpace(Message))
    {
        <p class="mt-3">@Message</p>
    }
</div>

@code {
    [Parameter]
    public string? Message { get; set; }
}
```

**Parameters**:
- `Message` (string?, optional): Custom loading message to display

**Usage Examples**:

```razor
<!-- Simple usage -->
<LoadingSpinner />

<!-- With custom message -->
<LoadingSpinner Message="Loading models..." />

<!-- With variable message -->
<LoadingSpinner Message="@customMessage" />

<!-- Conditional rendering -->
@if (isLoading)
{
    <LoadingSpinner Message="Fetching data..." />
}
```

**Styling**:
- Uses Bootstrap 5 classes
- `spinner-border` for the spinner animation
- `text-primary` for blue color
- `text-center my-5` for centering and vertical margins

**Customization**:

```razor
<!-- Change spinner color -->
<div class="spinner-border text-success" role="status">

<!-- Change spinner size -->
<div class="spinner-border spinner-border-sm text-primary" role="status">

<!-- Add custom CSS class parameter -->
@code {
    [Parameter]
    public string? CssClass { get; set; } = "text-primary";
}
```

**Integration Steps**:
1. Copy the component file to your project
2. Add to `_Imports.razor`: `@using YourProject.Components.Shared`
3. Use in any Razor page or component

---

### ErrorDisplay

**Purpose**: Error message display with optional retry functionality

**File**: `src/BlazorFrontend/Components/Shared/ErrorDisplay.razor`

**Full Source Code**:

```razor
<div class="alert alert-danger" role="alert">
    <h4 class="alert-heading">
        <span class="bi bi-exclamation-triangle-fill"></span> Error
    </h4>
    @if (!string.IsNullOrWhiteSpace(Message))
    {
        <p>@Message</p>
    }
    @if (ShowRetry && OnRetry.HasDelegate)
    {
        <hr>
        <button class="btn btn-sm btn-outline-danger" @onclick="OnRetry">
            <span class="bi bi-arrow-clockwise"></span> Retry
        </button>
    }
</div>

@code {
    [Parameter]
    public string? Message { get; set; }

    [Parameter]
    public bool ShowRetry { get; set; } = true;

    [Parameter]
    public EventCallback OnRetry { get; set; }
}
```

**Parameters**:
- `Message` (string?, optional): Error message to display
- `ShowRetry` (bool, default: true): Whether to show retry button
- `OnRetry` (EventCallback, optional): Callback function for retry button

**Usage Examples**:

```razor
<!-- Basic error display -->
<ErrorDisplay Message="An error occurred" />

<!-- With retry callback -->
<ErrorDisplay Message="@errorMessage" OnRetry="LoadDataAsync" />

<!-- Without retry button -->
<ErrorDisplay Message="@errorMessage" ShowRetry="false" />

<!-- Complete pattern -->
@code {
    private string? errorMessage;

    private async Task LoadDataAsync()
    {
        try
        {
            // Load data
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
    }
}

@if (errorMessage != null)
{
    <ErrorDisplay Message="@errorMessage" OnRetry="LoadDataAsync" />
}
```

**Styling**:
- Uses Bootstrap 5 alert components
- `alert-danger` for red error styling
- `bi` classes for Bootstrap Icons (requires Bootstrap Icons)

**Bootstrap Icons Dependency**:

```html
<!-- Add to index.html or _Host.cshtml -->
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.10.0/font/bootstrap-icons.css">
```

**Customization**:

```razor
<!-- Warning instead of error -->
<div class="alert alert-warning" role="alert">

<!-- Success message variant -->
<div class="alert alert-success" role="alert">
    <h4 class="alert-heading">
        <span class="bi bi-check-circle-fill"></span> Success
    </h4>
```

---

## Page Components

### SearchPage

**Purpose**: Full-featured search page with filters, pagination, and grid display

**File**: `src/BlazorFrontend/Pages/SearchPage.razor`

**Key Features**:
- Text search input
- Source filtering dropdown
- Minimum rating filter
- Clear filters button
- Paginated results grid
- Card-based model display
- Tag badges
- Navigation to detail pages
- Loading and error states

**Dependencies**:
- `Contracts.DTOs` (ModelDto, SearchRequestDto, PagedResultDto)
- `IApiClient` service
- `LoadingSpinner` component
- `ErrorDisplay` component
- Bootstrap 5
- Bootstrap Icons

**Parameters**:
- `@page "/search"` - Route definition

**Injected Services**:
- `IApiClient ApiClient` - API client for searching models
- `NavigationManager Navigation` - For navigation

**Code Structure**:

```razor
@page "/search"
@inject IApiClient ApiClient

<!-- Search Filters Card -->
<div class="card">
    <div class="card-body">
        <!-- Text input -->
        <input type="text" @bind="searchRequest.Query" />

        <!-- Source dropdown -->
        <select @bind="searchRequest.Source">...</select>

        <!-- Rating dropdown -->
        <select @bind="minRatingString">...</select>

        <!-- Search and Clear buttons -->
        <button @onclick="SearchAsync">Search</button>
        <button @onclick="ClearFilters">Clear</button>
    </div>
</div>

<!-- Loading State -->
@if (loading)
{
    <LoadingSpinner Message="Searching models..." />
}
<!-- Error State -->
else if (error != null)
{
    <ErrorDisplay Message="@error" OnRetry="SearchAsync" />
}
<!-- Results -->
else if (searchResult != null)
{
    <!-- Result count -->
    <p>Found @searchResult.TotalCount model(s)</p>

    <!-- Grid of model cards -->
    <div class="row">
        @foreach (var model in searchResult.Items)
        {
            <div class="col-md-4">
                <div class="card">
                    <img src="@model.Images.First().Url" />
                    <div class="card-body">
                        <h5>@model.Name</h5>
                        <p>@model.Description</p>
                        <!-- Tags -->
                        @foreach (var tag in model.Tags.Take(3))
                        {
                            <span class="badge">@tag</span>
                        }
                        <a href="/model/@model.Id">View Details</a>
                    </div>
                </div>
            </div>
        }
    </div>

    <!-- Pagination -->
    <nav>
        <ul class="pagination">
            <li class="page-item">
                <button @onclick="() => GoToPage(searchResult.PageNumber - 1)">
                    Previous
                </button>
            </li>
            <!-- Page numbers -->
            <li class="page-item">
                <button @onclick="() => GoToPage(searchResult.PageNumber + 1)">
                    Next
                </button>
            </li>
        </ul>
    </nav>
}

@code {
    private bool loading = true;
    private string? error;
    private PagedResultDto<ModelDto>? searchResult;
    private SearchRequestDto searchRequest = new();

    protected override async Task OnInitializedAsync()
    {
        await SearchAsync();
    }

    private async Task SearchAsync()
    {
        loading = true;
        error = null;

        try
        {
            searchResult = await ApiClient.SearchModelsAsync(searchRequest);
        }
        catch (Exception ex)
        {
            error = $"Failed to search: {ex.Message}";
        }
        finally
        {
            loading = false;
        }
    }

    private async Task GoToPage(int pageNumber)
    {
        searchRequest.PageNumber = pageNumber;
        await SearchAsync();
    }
}
```

**Usage Pattern**:

This component is a complete page. To integrate:

1. **Copy dependencies**:
   - DTOs: `SearchRequestDto`, `PagedResultDto<ModelDto>`
   - Service: `IApiClient` with `SearchModelsAsync` method

2. **Create API client method**:
```csharp
public interface IApiClient
{
    Task<PagedResultDto<ModelDto>> SearchModelsAsync(SearchRequestDto request);
}
```

3. **Register route**:
   - Component automatically registers at `/search`

4. **Customize filters**:
   - Modify source dropdown options
   - Add/remove filter fields
   - Adjust card layout

**Key Code Patterns**:

```csharp
// State management
private bool loading = true;  // Loading state
private string? error;         // Error message
private PagedResultDto<ModelDto>? searchResult;  // Results

// Loading pattern
loading = true;
try { /* API call */ }
catch (Exception ex) { error = ex.Message; }
finally { loading = false; }

// Pagination
private async Task GoToPage(int pageNumber)
{
    if (pageNumber < 1 || pageNumber > searchResult.TotalPages)
        return;
    searchRequest.PageNumber = pageNumber;
    await SearchAsync();
}

// Filter clearing
private void ClearFilters()
{
    searchRequest = new SearchRequestDto
    {
        PageNumber = 1,
        PageSize = 20
    };
}
```

---

### ModelDetailPage

**Purpose**: Detailed view of a single model with versions, images, and metadata

**File**: `src/BlazorFrontend/Pages/ModelDetailPage.razor`

**Key Features**:
- Route parameter for model ID
- Model metadata display
- Version list with download links
- Image gallery sidebar
- Inline editing capability
- Tag management
- File size formatting
- Back navigation
- Edit/save/cancel workflow

**Dependencies**:
- `Contracts.DTOs` (ModelDto)
- `IApiClient` service
- `LoadingSpinner` component
- `ErrorDisplay` component

**Parameters**:
- `@page "/model/{id:int}"` - Route with ID parameter
- `[Parameter] public int Id { get; set; }` - Route parameter binding

**Code Structure**:

```razor
@page "/model/{id:int}"
@inject IApiClient ApiClient

<!-- Loading State -->
@if (loading)
{
    <LoadingSpinner Message="Loading model details..." />
}
<!-- Error State -->
else if (error != null)
{
    <ErrorDisplay Message="@error" OnRetry="LoadModelAsync" />
}
<!-- Model Details -->
else if (model != null)
{
    <!-- Navigation buttons -->
    <a href="/search">Back to Search</a>
    <button @onclick="StartEdit">Edit</button>

    <!-- Edit Form (when isEditing is true) -->
    @if (isEditing)
    {
        <div class="card">
            <div class="card-body">
                <input type="text" @bind="editModel.Name" />
                <textarea @bind="editModel.Description"></textarea>
                <input type="text" @bind="tagsString" />
                <button @onclick="SaveModelAsync">Save</button>
                <button @onclick="CancelEdit">Cancel</button>
            </div>
        </div>
    }

    <!-- Model Information -->
    <div class="row">
        <div class="col-md-8">
            <div class="card">
                <div class="card-header">
                    <h3>@model.Name</h3>
                </div>
                <div class="card-body">
                    <p><strong>Source:</strong> @model.Source</p>
                    <p><strong>Rating:</strong> @model.Rating</p>
                    <p><strong>Description:</strong> @model.Description</p>

                    <!-- Tags -->
                    @foreach (var tag in model.Tags)
                    {
                        <span class="badge">@tag</span>
                    }
                </div>
            </div>

            <!-- Versions -->
            <div class="card">
                <div class="card-header">
                    <h5>Versions (@model.Versions.Count)</h5>
                </div>
                <div class="card-body">
                    @foreach (var version in model.Versions)
                    {
                        <div class="list-group-item">
                            <h6>@version.Name</h6>
                            <p>@version.Description</p>
                            <small>Size: @FormatFileSize(version.FileSize)</small>
                            <a href="@version.DownloadUrl">Download</a>
                        </div>
                    }
                </div>
            </div>
        </div>

        <!-- Images Sidebar -->
        <div class="col-md-4">
            <div class="card">
                <div class="card-header">
                    <h5>Images (@model.Images.Count)</h5>
                </div>
                <div class="card-body">
                    @foreach (var image in model.Images)
                    {
                        <img src="@image.Url" class="img-fluid" />
                        <small>@image.Width x @image.Height</small>
                    }
                </div>
            </div>
        </div>
    </div>
}

@code {
    [Parameter]
    public int Id { get; set; }

    private bool loading = true;
    private string? error;
    private ModelDto? model;
    private bool isEditing = false;
    private ModelDto editModel = new();
    private string tagsString = "";
    private bool saving = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadModelAsync();
    }

    private async Task LoadModelAsync()
    {
        loading = true;
        error = null;

        try
        {
            model = await ApiClient.GetModelAsync(Id);
            if (model == null)
                error = "Model not found";
        }
        catch (Exception ex)
        {
            error = $"Failed to load: {ex.Message}";
        }
        finally
        {
            loading = false;
        }
    }

    private void StartEdit()
    {
        isEditing = true;
        editModel = new ModelDto { /* copy properties */ };
        tagsString = string.Join(", ", model.Tags);
    }

    private async Task SaveModelAsync()
    {
        saving = true;
        try
        {
            editModel.Tags = tagsString
                .Split(',')
                .Select(t => t.Trim())
                .ToList();
            model = await ApiClient.UpdateModelAsync(Id, editModel);
            isEditing = false;
        }
        catch (Exception ex)
        {
            // Handle error
        }
        finally
        {
            saving = false;
        }
    }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
```

**Key Patterns**:

**1. Route Parameters**:
```razor
@page "/model/{id:int}"

@code {
    [Parameter]
    public int Id { get; set; }
}
```

**2. Inline Editing**:
```csharp
private bool isEditing = false;
private ModelDto editModel = new();

private void StartEdit()
{
    isEditing = true;
    // Create a copy for editing
    editModel = new ModelDto { /* copy from model */ };
}

private void CancelEdit()
{
    isEditing = false;
}

private async Task SaveModelAsync()
{
    // Save changes
    model = await ApiClient.UpdateModelAsync(Id, editModel);
    isEditing = false;
}
```

**3. File Size Formatting**:
```csharp
private static string FormatFileSize(long bytes)
{
    string[] sizes = { "B", "KB", "MB", "GB", "TB" };
    double len = bytes;
    int order = 0;
    while (len >= 1024 && order < sizes.Length - 1)
    {
        order++;
        len /= 1024;
    }
    return $"{len:0.##} {sizes[order]}";
}
```

**4. Tag Parsing**:
```csharp
// Display: Join with commas
tagsString = string.Join(", ", model.Tags);

// Save: Split and trim
editModel.Tags = tagsString
    .Split(',')
    .Select(t => t.Trim())
    .Where(t => !string.IsNullOrWhiteSpace(t))
    .ToList();
```

---

### RunsDashboardPage

**Purpose**: Dashboard for monitoring background ingestion runs with statistics

**File**: `src/BlazorFrontend/Pages/RunsDashboardPage.razor`

**Key Features**:
- Statistics cards (total, running, completed, failed)
- Runs table with detailed information
- Status badges with icons
- Duration formatting
- Error message display
- Auto-refresh capability
- Pagination
- Real-time duration calculation for running jobs

**Dependencies**:
- `Contracts.DTOs` (RunDto, PagedResultDto)
- `IApiClient` service
- `LoadingSpinner` component
- `ErrorDisplay` component

**Code Structure**:

```razor
@page "/runs"
@inject IApiClient ApiClient

<h1>Runs Dashboard</h1>
<button @onclick="RefreshAsync">Refresh</button>

@if (loading && runsResult == null)
{
    <LoadingSpinner Message="Loading runs..." />
}
else if (error != null)
{
    <ErrorDisplay Message="@error" OnRetry="LoadRunsAsync" />
}
else if (runsResult != null)
{
    <!-- Statistics Cards -->
    <div class="row">
        <div class="col-md-3">
            <div class="card bg-primary text-white">
                <div class="card-body">
                    <h5>Total Runs</h5>
                    <h2>@runsResult.TotalCount</h2>
                </div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card bg-info text-white">
                <div class="card-body">
                    <h5>Running</h5>
                    <h2>@runsResult.Items.Count(r => r.Status == RunStatus.Running)</h2>
                </div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card bg-success text-white">
                <div class="card-body">
                    <h5>Completed</h5>
                    <h2>@runsResult.Items.Count(r => r.Status == RunStatus.Completed)</h2>
                </div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card bg-danger text-white">
                <div class="card-body">
                    <h5>Failed</h5>
                    <h2>@runsResult.Items.Count(r => r.Status == RunStatus.Failed)</h2>
                </div>
            </div>
        </div>
    </div>

    <!-- Runs Table -->
    <div class="card">
        <div class="card-header">
            <h5>Recent Runs</h5>
        </div>
        <table class="table">
            <thead>
                <tr>
                    <th>ID</th>
                    <th>Source</th>
                    <th>Status</th>
                    <th>Started</th>
                    <th>Duration</th>
                    <th>Records</th>
                    <th>Errors</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var run in runsResult.Items)
                {
                    <tr>
                        <td>@run.Id</td>
                        <td>
                            <span class="badge bg-secondary">@run.Source</span>
                        </td>
                        <td>
                            @switch (run.Status)
                            {
                                case RunStatus.Running:
                                    <span class="badge bg-info">
                                        <span class="spinner-border spinner-border-sm"></span>
                                        Running
                                    </span>
                                    break;
                                case RunStatus.Completed:
                                    <span class="badge bg-success">
                                        <span class="bi bi-check-circle"></span>
                                        Completed
                                    </span>
                                    break;
                                case RunStatus.Failed:
                                    <span class="badge bg-danger">
                                        <span class="bi bi-exclamation-circle"></span>
                                        Failed
                                    </span>
                                    break;
                            }
                        </td>
                        <td>@run.StartedAt.ToString("MMM dd, HH:mm")</td>
                        <td>
                            @if (run.DurationSeconds.HasValue)
                            {
                                <span>@FormatDuration(run.DurationSeconds.Value)</span>
                            }
                            else if (run.Status == RunStatus.Running)
                            {
                                <span>@FormatDuration((DateTime.UtcNow - run.StartedAt).TotalSeconds)</span>
                            }
                        </td>
                        <td>@run.RecordsProcessed</td>
                        <td>
                            @if (run.ErrorCount > 0)
                            {
                                <span class="text-danger">@run.ErrorCount</span>
                            }
                        </td>
                    </tr>
                    <!-- Error message row -->
                    @if (run.Status == RunStatus.Failed && !string.IsNullOrWhiteSpace(run.ErrorMessage))
                    {
                        <tr>
                            <td colspan="7" class="bg-light">
                                <small class="text-danger">
                                    <strong>Error:</strong> @run.ErrorMessage
                                </small>
                            </td>
                        </tr>
                    }
                }
            </tbody>
        </table>
    </div>
}

@code {
    private bool loading = true;
    private string? error;
    private PagedResultDto<RunDto>? runsResult;
    private int currentPage = 1;

    protected override async Task OnInitializedAsync()
    {
        await LoadRunsAsync();
    }

    private async Task LoadRunsAsync()
    {
        loading = true;
        error = null;

        try
        {
            runsResult = await ApiClient.GetRunsAsync(currentPage, 20);
        }
        catch (Exception ex)
        {
            error = $"Failed to load runs: {ex.Message}";
        }
        finally
        {
            loading = false;
        }
    }

    private async Task RefreshAsync()
    {
        await LoadRunsAsync();
    }

    private static string FormatDuration(double seconds)
    {
        if (seconds < 60)
            return $"{seconds:F0}s";

        var minutes = (int)(seconds / 60);
        var remainingSeconds = (int)(seconds % 60);

        if (minutes < 60)
            return $"{minutes}m {remainingSeconds}s";

        var hours = minutes / 60;
        var remainingMinutes = minutes % 60;
        return $"{hours}h {remainingMinutes}m";
    }
}
```

**Key Patterns**:

**1. Statistics Cards**:
```razor
<div class="row">
    <div class="col-md-3">
        <div class="card bg-primary text-white">
            <div class="card-body">
                <h5>Total Runs</h5>
                <h2>@runsResult.TotalCount</h2>
            </div>
        </div>
    </div>
    <!-- More cards -->
</div>
```

**2. LINQ Aggregation**:
```csharp
@runsResult.Items.Count(r => r.Status == RunStatus.Running)
@runsResult.Items.Count(r => r.Status == RunStatus.Completed)
@runsResult.Items.Count(r => r.Status == RunStatus.Failed)
```

**3. Status Badges with Icons**:
```razor
@switch (run.Status)
{
    case RunStatus.Running:
        <span class="badge bg-info">
            <span class="spinner-border spinner-border-sm"></span> Running
        </span>
        break;
    case RunStatus.Completed:
        <span class="badge bg-success">
            <span class="bi bi-check-circle"></span> Completed
        </span>
        break;
}
```

**4. Duration Formatting**:
```csharp
private static string FormatDuration(double seconds)
{
    if (seconds < 60) return $"{seconds:F0}s";

    var minutes = (int)(seconds / 60);
    var remainingSeconds = (int)(seconds % 60);

    if (minutes < 60) return $"{minutes}m {remainingSeconds}s";

    var hours = minutes / 60;
    var remainingMinutes = minutes % 60;
    return $"{hours}h {remainingMinutes}m";
}
```

**5. Real-time Duration**:
```razor
@if (run.Status == RunStatus.Running)
{
    <span>@FormatDuration((DateTime.UtcNow - run.StartedAt).TotalSeconds)</span>
}
```

**6. Conditional Table Rows**:
```razor
@if (run.Status == RunStatus.Failed && !string.IsNullOrWhiteSpace(run.ErrorMessage))
{
    <tr>
        <td colspan="7" class="bg-light">
            <small class="text-danger">
                <strong>Error:</strong> @run.ErrorMessage
            </small>
        </td>
    </tr>
}
```

---

## Layout Components

### MainLayout

**File**: `src/BlazorFrontend/Components/Layout/MainLayout.razor`

Basic layout wrapper with navigation menu.

**Structure**:
```razor
@inherits LayoutComponentBase

<div class="page">
    <div class="sidebar">
        <NavMenu />
    </div>

    <main>
        <div class="top-row px-4">
            <!-- Header content -->
        </div>

        <article class="content px-4">
            @Body
        </article>
    </main>
</div>
```

### NavMenu

**File**: `src/BlazorFrontend/Components/Layout/NavMenu.razor`

Navigation menu with routing.

**Structure**:
```razor
<div class="nav-menu">
    <nav class="flex-column">
        <div class="nav-item px-3">
            <NavLink class="nav-link" href="/" Match="NavLinkMatch.All">
                <span class="bi bi-house"></span> Home
            </NavLink>
        </div>
        <div class="nav-item px-3">
            <NavLink class="nav-link" href="/search">
                <span class="bi bi-search"></span> Search
            </NavLink>
        </div>
        <div class="nav-item px-3">
            <NavLink class="nav-link" href="/runs">
                <span class="bi bi-bar-chart"></span> Runs
            </NavLink>
        </div>
    </nav>
</div>
```

---

## Integration Guide

### Prerequisites

**NuGet Packages**:
```xml
<PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="8.0.0" />
```

**CSS Dependencies**:
```html
<!-- Bootstrap 5 -->
<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">

<!-- Bootstrap Icons -->
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.10.0/font/bootstrap-icons.css">
```

### Step 1: Copy Component Files

```bash
YourProject/
├── Components/
│   ├── Shared/
│   │   ├── LoadingSpinner.razor
│   │   └── ErrorDisplay.razor
│   └── Layout/
│       ├── MainLayout.razor
│       └── NavMenu.razor
└── Pages/
    ├── SearchPage.razor
    ├── ModelDetailPage.razor
    └── RunsDashboardPage.razor
```

### Step 2: Update _Imports.razor

```razor
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Components.Routing
@using YourProject.Components.Shared
@using YourProject.Components.Layout
@using YourProject.Services
@using YourProject.DTOs
```

### Step 3: Create Required Services

**IApiClient Interface**:
```csharp
public interface IApiClient
{
    Task<PagedResultDto<ModelDto>> SearchModelsAsync(SearchRequestDto request);
    Task<ModelDto?> GetModelAsync(int id);
    Task<ModelDto> UpdateModelAsync(int id, ModelDto model);
    Task<PagedResultDto<RunDto>> GetRunsAsync(int page, int pageSize);
}
```

**Register in Program.cs**:
```csharp
builder.Services.AddScoped<IApiClient, ApiClient>();
```

### Step 4: Common Usage Patterns

**Loading/Error/Content Pattern**:
```razor
@if (loading)
{
    <LoadingSpinner Message="Loading..." />
}
else if (error != null)
{
    <ErrorDisplay Message="@error" OnRetry="LoadDataAsync" />
}
else if (data != null)
{
    <!-- Display content -->
}

@code {
    private bool loading = true;
    private string? error;
    private MyData? data;

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        loading = true;
        error = null;
        StateHasChanged();  // Force UI update

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
            StateHasChanged();
        }
    }
}
```

**Pagination Pattern**:
```razor
@if (result.TotalPages > 1)
{
    <nav>
        <ul class="pagination">
            <li class="page-item @(!result.HasPreviousPage ? "disabled" : "")">
                <button class="page-link" @onclick="() => GoToPage(result.PageNumber - 1)">
                    Previous
                </button>
            </li>

            @for (int i = 1; i <= result.TotalPages; i++)
            {
                var pageNum = i;
                <li class="page-item @(pageNum == result.PageNumber ? "active" : "")">
                    <button class="page-link" @onclick="() => GoToPage(pageNum)">
                        @pageNum
                    </button>
                </li>
            }

            <li class="page-item @(!result.HasNextPage ? "disabled" : "")">
                <button class="page-link" @onclick="() => GoToPage(result.PageNumber + 1)">
                    Next
                </button>
            </li>
        </ul>
    </nav>
}

@code {
    private async Task GoToPage(int pageNumber)
    {
        if (pageNumber < 1 || pageNumber > result.TotalPages)
            return;

        request.PageNumber = pageNumber;
        await LoadDataAsync();
    }
}
```

**Form Binding Pattern**:
```razor
<input type="text" class="form-control" @bind="model.Name" />
<textarea class="form-control" @bind="model.Description"></textarea>
<select class="form-select" @bind="model.Category">
    <option value="">Select...</option>
    <option value="Option1">Option 1</option>
</select>
<input type="checkbox" class="form-check-input" @bind="model.IsActive" />

@code {
    private MyModel model = new();
}
```

**Event Callback Pattern**:
```razor
<!-- Parent Component -->
<ChildComponent OnValueChanged="HandleValueChanged" />

@code {
    private void HandleValueChanged(string newValue)
    {
        // Handle the value change
    }
}

<!-- Child Component -->
<input @bind="value" @bind:event="oninput" @onchange="NotifyParent" />

@code {
    private string value = "";

    [Parameter]
    public EventCallback<string> OnValueChanged { get; set; }

    private async Task NotifyParent()
    {
        await OnValueChanged.InvokeAsync(value);
    }
}
```

---

## Comparison: XAML vs Razor

For developers coming from WPF/MAUI/UWP:

| XAML | Razor | Notes |
|------|-------|-------|
| `<TextBlock Text="{Binding Name}" />` | `<p>@model.Name</p>` | Direct property access |
| `<ListView ItemsSource="{Binding Items}">` | `@foreach (var item in items) { }` | Loop instead of binding |
| `<Button Command="{Binding SaveCommand}">` | `<button @onclick="SaveAsync">` | Direct method binding |
| `<Converter>` | `@FormatValue(value)` | C# methods instead of converters |
| `Visibility="Collapsed"` | `@if (condition) { }` | Conditional rendering |
| `DataTemplate` | Component parameters | Reusable components |
| `UserControl` | Razor Component | Same concept |
| `{x:Bind}` | `@bind` | Two-way binding |
| `DependencyProperty` | `[Parameter]` | Component parameters |
| `INotifyPropertyChanged` | `StateHasChanged()` | Manual UI refresh |

---

## Best Practices

### 1. State Management
```csharp
// Always call StateHasChanged() after async operations
private async Task LoadDataAsync()
{
    loading = true;
    StateHasChanged();  // Update UI immediately

    try
    {
        data = await ApiClient.GetDataAsync();
    }
    finally
    {
        loading = false;
        StateHasChanged();  // Update UI with results
    }
}
```

### 2. Null Safety
```razor
<!-- Safe navigation -->
@if (model?.Name != null)
{
    <p>@model.Name</p>
}

<!-- Null-coalescing -->
<p>@(model?.Name ?? "Unknown")</p>

<!-- Conditional rendering -->
@if (items?.Any() == true)
{
    @foreach (var item in items)
    {
        <div>@item.Name</div>
    }
}
```

### 3. Component Parameters Validation
```csharp
[Parameter]
public string? Message { get; set; }

protected override void OnParametersSet()
{
    if (string.IsNullOrWhiteSpace(Message))
    {
        Message = "Default message";
    }
}
```

### 4. Dispose Pattern
```csharp
@implements IDisposable

@code {
    private System.Timers.Timer? timer;

    protected override void OnInitialized()
    {
        timer = new System.Timers.Timer(1000);
        timer.Elapsed += OnTimerElapsed;
        timer.Start();
    }

    public void Dispose()
    {
        timer?.Dispose();
    }
}
```

---

## Troubleshooting

### Common Issues

**1. "Parameter is null"**
- Ensure parent component passes the parameter
- Check parameter name casing (case-sensitive)

**2. "UI not updating"**
- Call `StateHasChanged()` after data changes
- Ensure async operations are awaited

**3. "Binding not working"**
- Use `@bind` instead of `value="@variable"`
- For input events, use `@bind:event="oninput"`

**4. "Bootstrap icons not showing"**
- Ensure Bootstrap Icons CSS is included
- Use `bi` class prefix: `<span class="bi bi-search"></span>`

**5. "Route not found"**
- Check `@page` directive
- Ensure component is in correct namespace
- Verify `_Imports.razor` includes component namespace

---

## Resources

- [Blazor Documentation](https://docs.microsoft.com/aspnet/core/blazor)
- [Bootstrap 5 Documentation](https://getbootstrap.com/docs/5.3)
- [Bootstrap Icons](https://icons.getbootstrap.com)
- [Razor Syntax Reference](https://docs.microsoft.com/aspnet/core/mvc/views/razor)

---

**Document Version**: 1.0
**Last Updated**: 2025-11-15
**Framework**: Blazor Server (.NET 8)
**UI Framework**: Bootstrap 5.3
**Icon Library**: Bootstrap Icons 1.10
