# Contributing

This project is developed by a four-person Software Engineering team.

All significant changes should be made through branches and Pull Requests.

## Basic Workflow

Start from the latest `main`:

```bash
git switch main
git pull
```

Create a branch:

```bash
git switch -c feature/example-feature
```

Make and commit changes:

```bash
git add .
git commit -m "feat: add example feature"
```

Push:

```bash
git push -u origin feature/example-feature
```

Then open a Pull Request into `main`.

Direct pushes to `main` should be avoided.

## Branch Naming

Format:

```text
<type>/<short-description>
```

Supported prefixes:

| Prefix | Purpose |
|---|---|
| `feature/` | New functionality |
| `fix/` | Bug fixes |
| `hotfix/` | Urgent fixes |
| `refactor/` | Internal restructuring |
| `chore/` | Maintenance or project cleanup |
| `docs/` | Documentation |
| `test/` | Tests |
| `release/` | Release preparation |
| `ci/` | CI changes |
| `build/` | Build or dependency changes |
| `perf/` | Performance improvements |
| `style/` | Formatting-only changes |

Examples:

```text
feature/open-library-search
feature/want-to-read
fix/reading-progress-validation
chore/project-cleanup
docs/update-api-documentation
```

## Commit Messages

Recommended format:

```text
<type>: <description>
```

Examples:

```text
feat: add book search endpoint
fix: resolve backend integration conflict
docs: update API documentation
refactor: simplify book service
chore: remove duplicate documentation
```

Keep commits focused on one logical change when possible.

## Pull Requests

A Pull Request should explain what changed and why.

Example:

```md
## Summary

Adds Open Library book search integration.

## Changes

- replaces mock search data
- integrates Open Library through HttpClient
- maps external results to BookSearchResultDto
- preserves the frontend API contract
```

Before requesting review:

- build the affected project;
- test the affected functionality;
- review your own diff;
- remove temporary/debug code;
- do not commit secrets;
- update documentation when API behavior changes.

Backend build check:

```bash
cd backend/BookClub.Api
dotnet build
```

## Code Review

Reviewers should focus on:

- correctness;
- integration with the latest `main`;
- maintainability;
- API consistency;
- input validation;
- HTTP status codes;
- database behavior;
- secret handling;
- whether another team member can run the code.

Useful questions:

- Does this branch include the latest important changes from `main`?
- Does the project build?
- Are DTOs used instead of exposing database entities?
- Are invalid inputs handled?
- Are external API details kept inside the backend?
- Does documentation match the actual implementation?

Avoid blocking a Pull Request for purely cosmetic issues unless readability or consistency is affected.

## Shared Branches

If another team member needs to help with an existing Pull Request branch, they may create commits on top of that branch and push them to the same remote branch when the team agrees.

Before doing this:

```bash
git fetch origin
```

Avoid force-pushing shared branches.

If a push is rejected as non-fast-forward, fetch and integrate the remote changes instead of using `--force`.

## Keeping a Branch Updated

For a long-running branch:

```bash
git fetch origin
git merge origin/main
```

Resolve conflicts carefully.

After resolving a conflict:

```bash
git add <resolved-files>
git commit
```

Never commit unresolved conflict markers such as:

```text
<<<<<<<
=======
>>>>>>>
```

Always run a build after resolving backend conflicts.

## Secrets

Never commit:

- database passwords;
- API keys;
- private connection strings;
- secret `.env` files.

Database connection strings should be configured with .NET User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:ConnectionString" "<connection-string>"
```

## Documentation

Current documentation:

```text
README.md
CONTRIBUTING.md
docs/api.md
docs/architecture.md
```

Do not keep duplicate copies of the same documentation in multiple locations.

Update documentation in the same Pull Request when changing:

- endpoints;
- request/response contracts;
- setup requirements;
- architecture decisions;
- development workflow.
