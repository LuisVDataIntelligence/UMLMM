# UMLMM

[![Build and Test](https://github.com/LuisVDataIntelligence/UMLMM/actions/workflows/build.yml/badge.svg)](https://github.com/LuisVDataIntelligence/UMLMM/actions/workflows/build.yml)
[![Code Quality](https://github.com/LuisVDataIntelligence/UMLMM/actions/workflows/quality.yml/badge.svg)](https://github.com/LuisVDataIntelligence/UMLMM/actions/workflows/quality.yml)
[![CodeQL](https://github.com/LuisVDataIntelligence/UMLMM/actions/workflows/codeql.yml/badge.svg)](https://github.com/LuisVDataIntelligence/UMLMM/actions/workflows/codeql.yml)
[![codecov](https://codecov.io/gh/LuisVDataIntelligence/UMLMM/branch/main/graph/badge.svg)](https://codecov.io/gh/LuisVDataIntelligence/UMLMM)

Unified Model/Media Metadata — ingestion (CivitAI, Danbooru, e621, ComfyUI, Ollama), centralized PostgreSQL, orchestration, and Blazor Server admin UI.

## CI/CD & Quality

This repository uses GitHub Actions for continuous integration and deployment:

- **Build and Test**: Builds all projects and runs tests with code coverage
- **Code Quality**: Enforces code formatting with `dotnet format` and runs static analysis
- **CodeQL**: Performs security scanning to identify vulnerabilities
- **Dependabot**: Automatically updates dependencies weekly

### Local Development

Format code before committing:
```bash
dotnet format
```

Run tests with coverage:
```bash
dotnet test --collect:"XPlat Code Coverage"
```
