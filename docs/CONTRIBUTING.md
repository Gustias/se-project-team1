# Contributing

This project is developed by a four-person Software Engineering team.

To keep the repository stable and easy to review, all changes should be made through branches and Pull Requests.

## Workflow

Start from the latest `main` branch:

```bash
git switch main
git pull
```

Create a new branch:

```bash
git switch -c feature/example-feature
```

Make your changes, then commit them:

```bash
git add .
git commit -m "feat: add example feature"
```

Push the branch:

```bash
git push -u origin feature/example-feature
```

Then open a Pull Request into `main`.

Direct pushes to `main` should be avoided.

## Branch Naming

Use the following format:

```text
<type>/<short-description>
```

Supported branch types:

| Type | Usage |
|---|---|
| `feature/` | New functionality |
| `fix/` | Bug fixes |
| `hotfix/` | Urgent fixes |
| `refactor/` | Code restructuring without changing behavior |
| `chore/` | Project setup or maintenance |
| `docs/` | Documentation changes |
| `test/` | Tests |
| `release/` | Release preparation |
| `ci/` | CI configuration |
| `build/` | Build system or dependencies |
| `perf/` | Performance improvements |
| `style/` | Formatting or style-only changes |

Examples:

```text
feature/open-library-search
feature/want-to-read
fix/reading-progress-validation
docs/update-api-documentation
refactor/book-service
```

## Commit Messages

Use short, descriptive commit messages.

Recommended format:

```text
<type>: <description>
```

Examples:

```text
feat: add book search endpoint
fix: validate reading progress IDs
docs: update API documentation
refactor: simplify book service
chore: set up initial project structure
```

Keep commits focused on one logical change when possible.

## Pull Requests

A Pull Request should explain what changed and why.

Example:

```md
## Summary

Adds Open Library book search integration.

## Changes

- replaces mock book search data
- adds HttpClient integration
- maps Open Library responses to BookSearchResultDto
- keeps the frontend API contract unchanged
```

Before requesting review:

- make sure the project builds;
- test the affected functionality;
- review your own changed files;
- remove temporary/debug code;
- do not commit secrets or database passwords;
- update documentation when API contracts change.

Backend build check:

```bash
cd backend/BookClub.Api
dotnet build
```

## Code Review

Reviewers should focus on functionality, maintainability, API consistency, validation, and integration with existing code.

Useful review questions include:

- Does this change work with the latest `main` branch?
- Are request and response contracts consistent?
- Are DTOs used instead of exposing database entities directly?
- Are invalid inputs handled correctly?
- Are appropriate HTTP status codes returned?
- Are secrets kept outside the repository?
- Will another team member be able to run and use this code?
- Does documentation need to be updated?

Small style issues should not block a Pull Request unless they affect readability or consistency.

## Resolving Review Feedback

When review feedback is addressed:

1. Make the requested changes.
2. Commit them to the same feature branch.
3. Push the commits.
4. Resolve the related review conversations once the issue is actually fixed.

If another team member needs to contribute directly to an existing Pull Request branch, they may create commits on top of that branch and push them to the same remote branch, provided the team agrees.

Avoid force-pushing shared branches unless absolutely necessary.

## Keeping a Branch Updated

Before merging a long-running branch, update it with the latest `main`:

```bash
git fetch origin
git merge origin/main
```

Resolve conflicts carefully and verify that functionality from both branches remains intact.

Do not use `--force` to solve a normal merge conflict.

## Repository Structure

```text
frontend/
    React frontend

backend/BookClub.Api/
    Controllers/     HTTP endpoints
    Services/        Application logic
    DTOs/            API contracts
    Models/          Database models
    Data/            EF Core and database configuration
    Integrations/    External API integrations

docs/
    Project and API documentation
```

Keep new code in the appropriate layer rather than placing unrelated logic in controllers or other arbitrary folders.

## Secrets and Configuration

Never commit:

- database passwords;
- API keys;
- private connection strings;
- `.env` files containing secrets.

The backend database connection string should be configured using .NET User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:ConnectionString" "<connection-string>"
```

## Documentation

Update documentation when introducing or changing:

- API endpoints;
- request/response formats;
- setup requirements;
- architecture decisions;
- developer workflow.

Main documentation files:

```text
README.md
CONTRIBUTING.md
docs/api.md
docs/architecture.md
```
