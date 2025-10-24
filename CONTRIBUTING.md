# Contributing to UMLMM

Thank you for your interest in contributing to UMLMM! This document provides guidelines and information for contributors.

## Code of Conduct

Be respectful, collaborative, and professional in all interactions.

## Development Setup

### Prerequisites

- .NET 8.0 SDK or later
- Git
- Your preferred IDE (Visual Studio, VS Code, or Rider)

### Getting Started

1. **Fork and clone the repository:**
   ```bash
   git clone https://github.com/LuisVDataIntelligence/UMLMM.git
   cd UMLMM
   ```

2. **Create a feature branch:**
   ```bash
   git checkout -b feature/your-feature-name
   ```

3. **Install dependencies:**
   ```bash
   dotnet restore
   ```

4. **Build the solution:**
   ```bash
   dotnet build
   ```

5. **Run tests:**
   ```bash
   dotnet test
   ```

## Coding Standards

### Code Style

- Follow the `.editorconfig` settings
- Use `dotnet format` before committing
- Keep code clean and well-documented
- Write meaningful commit messages

### Formatting

Format your code before committing:
```bash
dotnet format
```

### Naming Conventions

- **PascalCase**: Classes, methods, properties, public fields
- **camelCase**: Local variables, parameters, private fields
- **UPPER_CASE**: Constants
- **IPascalCase**: Interfaces (prefix with 'I')

### Code Quality

- Enable nullable reference types
- Use implicit usings where appropriate
- Follow SOLID principles
- Write self-documenting code
- Add comments only when necessary to explain "why", not "what"

## Pull Request Process

### Before Creating a PR

1. **Ensure your code builds:**
   ```bash
   dotnet build
   ```

2. **Run all tests:**
   ```bash
   dotnet test
   ```

3. **Format your code:**
   ```bash
   dotnet format
   ```

4. **Check for security issues:**
   - Review CodeQL findings
   - Ensure no secrets in code

### Creating a PR

1. **Push your branch:**
   ```bash
   git push origin feature/your-feature-name
   ```

2. **Open a Pull Request** on GitHub

3. **Fill out the PR template** with:
   - Description of changes
   - Related issue number
   - Testing performed
   - Screenshots (if UI changes)

4. **Ensure all CI checks pass:**
   - Build and Test
   - Code Quality
   - CodeQL Security Scan
   - Secret Scanning

### PR Review Process

- All PRs require approval before merging
- Address reviewer feedback promptly
- Keep PRs focused and reasonably sized
- Squash commits if requested
- Ensure commit messages are clear

## Testing Guidelines

### Unit Tests

- Write tests for all new functionality
- Use meaningful test names that describe the scenario
- Follow AAA pattern (Arrange, Act, Assert)
- Keep tests isolated and independent
- Mock external dependencies

### Integration Tests

- Use Testcontainers for database tests
- Clean up resources after tests
- Test realistic scenarios
- Ensure tests are repeatable

### Test Organization

- Place tests in `*.Tests` projects
- Mirror the structure of the main project
- Name test files: `ClassNameTests.cs`
- Name test methods: `MethodName_Scenario_ExpectedBehavior`

## Commit Message Guidelines

### Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

- **feat**: New feature
- **fix**: Bug fix
- **docs**: Documentation changes
- **style**: Code style changes (formatting, etc.)
- **refactor**: Code refactoring
- **test**: Test additions or changes
- **chore**: Build process or auxiliary tool changes

### Examples

```
feat(ingestor): add CivitAI model fetching

Implement the model fetching logic for CivitAI API with pagination
support and error handling.

Closes #123
```

```
fix(database): resolve connection pooling issue

Fix connection leak in DbContext by properly disposing connections.
```

## Project Structure

```
UMLMM/
├── .github/
│   ├── workflows/          # CI/CD workflows
│   ├── dependabot.yml      # Dependency updates
│   └── pull_request_template.md
├── docs/                   # Documentation
├── src/                    # Source code
│   ├── Domain/            # Domain entities
│   ├── Infrastructure/    # EF Core, DbContext
│   ├── Ingestors/         # Data ingestion services
│   ├── GatewayApi/        # REST API
│   └── BlazorFrontend/    # UI
├── tests/                  # Test projects
├── .editorconfig          # Code style rules
├── .gitignore             # Git ignore rules
├── Directory.Build.props  # Common build properties
└── README.md
```

## Issue Reporting

### Before Creating an Issue

- Search existing issues to avoid duplicates
- Gather relevant information (logs, versions, etc.)
- Create a minimal reproduction if possible

### Issue Template

**Bug Report:**
- Description
- Steps to reproduce
- Expected behavior
- Actual behavior
- Environment (OS, .NET version, etc.)

**Feature Request:**
- Problem description
- Proposed solution
- Alternatives considered
- Additional context

## Questions and Support

- Create a discussion for general questions
- Use issues for bugs and feature requests
- Check documentation first
- Be specific and provide context

## License

By contributing, you agree that your contributions will be licensed under the same license as the project.

## Additional Resources

- [GitHub Flow](https://guides.github.com/introduction/flow/)
- [Writing Good Commit Messages](https://chris.beams.io/posts/git-commit/)
- [.NET Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [EditorConfig](https://editorconfig.org/)

Thank you for contributing to UMLMM! 🎉
