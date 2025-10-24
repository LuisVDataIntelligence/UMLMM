# Phase 10 — CI/CD and Quality

This document describes the CI/CD pipeline and quality gates implemented for the UMLMM project.

## Overview

The project uses GitHub Actions for continuous integration and deployment with the following workflows:

### Build and Test (`build.yml`)

Runs on every push to `main` and on all pull requests.

**Steps:**
1. Checkout code with full history
2. Set up .NET 8.0
3. Restore NuGet dependencies
4. Build in Release configuration
5. Run all tests with code coverage
6. Upload coverage reports to Codecov
7. Archive test results

**Configuration:**
- Runs on Ubuntu latest
- Conditional execution (skips if no .NET projects found)
- Code coverage collected with XPlat Code Coverage
- Results retained for 30 days

### Code Quality (`quality.yml`)

Ensures code formatting and static analysis standards are met.

**Jobs:**

1. **Format Check**
   - Verifies code follows formatting standards
   - Uses `dotnet format --verify-no-changes`
   - Fails if unformatted code is found

2. **Static Analysis**
   - Builds with .NET analyzers enabled
   - Warnings are reported but don't fail the build initially
   - Configured via `Directory.Build.props`

### Security Scanning

#### CodeQL (`codeql.yml`)

- Runs on push to main, PRs, and weekly schedule (Mondays)
- Performs deep code analysis for security vulnerabilities
- Analyzes C# code patterns
- Results visible in Security tab

#### Secret Scanning (`secrets.yml`)

- Uses Gitleaks to detect accidentally committed secrets
- Runs on push to main and all PRs
- Scans entire git history for sensitive data

### Dependency Management (`dependabot.yml`)

Dependabot automatically checks for outdated dependencies:

- **NuGet packages**: Weekly on Mondays
- **GitHub Actions**: Weekly on Mondays
- Creates PRs for updates
- Auto-labels PRs for easy filtering

## Quality Standards

### Code Formatting

The project uses `.editorconfig` for consistent code style:

- 4 spaces for C# code
- 2 spaces for XML/JSON/YAML
- UTF-8 encoding
- Trim trailing whitespace
- Insert final newline

### Analyzers

Enabled analyzers (via `Directory.Build.props`):
- Microsoft.CodeAnalysis.NetAnalyzers
- Latest analysis level
- Code style enforcement in build
- Nullable reference types enabled
- Implicit usings enabled

### Coverage Reporting

- Code coverage collected during test runs
- Reports uploaded to Codecov
- Coverage data available in PR comments (if Codecov is configured)
- Threshold enforcement can be added via Codecov settings

## Local Development

### Before Committing

1. **Format your code:**
   ```bash
   dotnet format
   ```

2. **Build the solution:**
   ```bash
   dotnet build
   ```

3. **Run tests:**
   ```bash
   dotnet test
   ```

4. **Run tests with coverage:**
   ```bash
   dotnet test --collect:"XPlat Code Coverage"
   ```

### IDE Setup

The `.editorconfig` file is automatically recognized by:
- Visual Studio 2019+
- Visual Studio Code (with C# extension)
- JetBrains Rider
- Most modern editors

## PR Workflow

1. Create a feature branch
2. Make changes and commit
3. Push branch to GitHub
4. Open a Pull Request
5. CI checks run automatically:
   - Build and Test
   - Code Quality (format + analysis)
   - CodeQL Security Scan
   - Secret Scanning
6. All checks must pass before merge
7. Review and merge when green

## Status Badges

The README includes badges for:
- Build and Test status
- Code Quality status
- CodeQL status
- Code coverage (Codecov)

## Troubleshooting

### Format Check Fails

Run locally to see differences:
```bash
dotnet format --verify-no-changes --verbosity diagnostic
```

Fix formatting:
```bash
dotnet format
```

### Build Fails in CI but Works Locally

- Ensure dependencies are committed (packages.lock.json if using it)
- Check for case-sensitivity issues (Linux CI vs Windows dev)
- Verify .NET version matches (8.0.x)

### CodeQL Analysis Issues

- CodeQL may report false positives
- Review findings in Security > Code scanning alerts
- Dismiss false positives with justification
- Fix genuine issues before merging

### Secret Scanning Alerts

If Gitleaks finds a secret:
1. Rotate the exposed secret immediately
2. Remove it from git history or use `.gitleaksignore` if it's a false positive
3. Update the code to use environment variables or secure vaults

## Future Enhancements

Potential improvements for later phases:

- [ ] Add integration test workflow with Testcontainers
- [ ] Set up deployment workflows (staging/production)
- [ ] Add performance benchmarking
- [ ] Configure branch protection rules
- [ ] Add SARIF upload for third-party security tools
- [ ] Implement automatic release notes generation
- [ ] Add Docker image building and publishing
- [ ] Set up code coverage thresholds and gates

## References

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [CodeQL Documentation](https://codeql.github.com/docs/)
- [Dependabot Documentation](https://docs.github.com/en/code-security/dependabot)
- [.NET Code Analysis](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview)
- [EditorConfig](https://editorconfig.org/)
