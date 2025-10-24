# Quick Start Guide - UMLMM Blazor Frontend

## Prerequisites
- .NET 8.0 SDK installed

## Quick Start

### 1. Build the Project
```bash
cd /home/runner/work/UMLMM/UMLMM
dotnet build
```

### 2. Run Tests
```bash
dotnet test
```

Expected: 22 tests pass ✅

### 3. Run the Application
```bash
cd src/BlazorFrontend
dotnet run
```

### 4. Access the Application
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001

## Available Pages

### Search Page (Default)
- URL: `/search`
- Features: Search models, apply filters, view pagination

### Model Detail/Edit
- URL: `/model/{id}`
- Example: `/model/1`
- Features: View model details, edit metadata, see versions

### Runs Dashboard
- URL: `/runs`
- Features: Monitor ingestion runs, view statistics

## Key Features

### Search Page
1. Enter text in search box
2. Select source from dropdown
3. Choose minimum rating
4. Click "Search" button
5. Navigate pages with pagination controls
6. Click "View Details" on any model card

### Model Detail
1. View all model information
2. Click "Edit" button
3. Modify name, description, or tags
4. Click "Save" to persist changes
5. Click "Back to Search" to return

### Runs Dashboard
1. View statistics cards at top
2. Scroll through runs table
3. Click "Refresh" to update data
4. See status indicators and durations

## Mock Data
Currently uses mock data for demonstration:
- 10 sample models from various sources
- 5 sample runs with different statuses
- All features fully functional

## Testing

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Class
```bash
dotnet test --filter ClassName=SearchPageTests
```

### Test with Verbose Output
```bash
dotnet test --verbosity normal
```

## Project Structure
```
src/
  BlazorFrontend/     - Main application
  Contracts/          - DTOs
tests/
  BlazorFrontend.Tests/ - Component tests
docs/                   - Documentation
```

## Configuration

Edit `src/BlazorFrontend/appsettings.json`:
```json
{
  "ApiGateway": {
    "BaseUrl": "http://your-api-gateway:5000"
  }
}
```

## Troubleshooting

### Build Errors
- Ensure .NET 8.0 SDK is installed
- Run `dotnet restore` first

### Test Failures
- Rebuild with `dotnet build`
- Check dependencies are restored

### Runtime Issues
- Verify port 5000/5001 is available
- Check appsettings.json configuration

## Next Steps

1. **Backend Integration**: Update ApiGateway BaseUrl when API is ready
2. **Authentication**: Add authentication middleware
3. **Real Data**: Replace mock data with actual API calls
4. **Deployment**: Containerize with Docker

## Support

For more details, see:
- `/docs/phase-09-blazor-frontend.md` - Full documentation
- `/docs/AGENT-INSTRUCTIONS.md` - Development guide
- `/COMPLETION_SUMMARY.md` - Implementation summary
- `/README.md` - Project overview

---

**Status**: ✅ Production Ready for Demo
**Version**: Phase 9 Complete
**Last Updated**: 2025-10-24
