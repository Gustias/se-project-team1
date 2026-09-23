# Book Club

A web application for discovering books, tracking reading activity, organizing a personal reading library, and later participating in book clubs and reading-related events.

This project is being developed as a university Software Engineering team project.

## Project Goals

The first development stage focuses on the core personal library functionality:

- Search for books
- Add books to a **Want to Read** list
- Remove books from the **Want to Read** list
- Mark books as finished
- Rate finished books from **1 to 5**

Additional social and reading-tracking features are planned for later development.

## Current Development

The project currently includes:

- ASP.NET Core backend structure
- REST API controllers, services, DTOs, and models
- Entity Framework Core integration
- PostgreSQL database support
- Reading progress API functionality
- React development CORS configuration
- API documentation
- Open Library book search integration in development

## Planned Features

Future features may include:

- Reading progress tracking
- Book clubs
- Public and private clubs
- Progress-based discussions
- Spoiler-safe comments
- Reading statistics
- Book-related events
- Event map integration

## Tech Stack

### Frontend

- React
- Vite
- JavaScript

### Backend

- C#
- ASP.NET Core
- .NET 10
- Entity Framework Core

### Database

- PostgreSQL
- Neon

### External APIs

- Open Library API

## Architecture

```text
                 ┌─────────────────────┐
                 │    React Frontend   │
                 └──────────┬──────────┘
                            │
                            │ HTTP / JSON
                            ▼
                 ┌─────────────────────┐
                 │   ASP.NET Core API  │
                 └───────┬───────┬─────┘
                         │       │
                         │       │
                         ▼       ▼
              ┌──────────────┐  ┌─────────────────┐
              │ PostgreSQL   │  │ Open Library API│
              │ Database     │  │                 │
              └──────────────┘  └─────────────────┘
```

The frontend communicates with the ASP.NET Core backend rather than directly with external APIs.

External API responses are mapped into the application's own DTOs before being returned to the frontend. This keeps the frontend independent from the raw Open Library response format.

## Project Structure

```text
se-project/
├── frontend/
│   └── React / Vite application
│
├── backend/
│   └── BookClub.Api/
│       ├── Controllers/
│       │   └── API endpoints
│       │
│       ├── Services/
│       │   └── Application and business logic
│       │
│       ├── DTOs/
│       │   └── API data contracts
│       │
│       ├── Models/
│       │   └── Database models
│       │
│       ├── Data/
│       │   ├── AppDbContext
│       │   └── EF Core migrations
│       │
│       └── Integrations/
│           └── External API integrations
│
├── docs/
│   └── API documentation and project notes
│
└── README.md
```

## API

### Book Search

Search for books by title:

```http
GET /api/books/search?q=dune
```

Example response format:

```json
[
  {
    "externalId": "/works/OL893415W",
    "title": "Dune",
    "author": "Frank Herbert",
    "coverUrl": "https://covers.openlibrary.org/..."
  }
]
```

The frontend should depend on this response structure rather than the raw Open Library response.

More API documentation can be found in:

```text
docs/api.md
```

## Running the Project

### Requirements

Make sure the following tools are installed:

- .NET 10 SDK
- Node.js
- npm
- Git

A PostgreSQL-compatible database connection is also required for database functionality.

### Backend

Navigate to the backend project:

```bash
cd backend/BookClub.Api
```

Configure the database connection string using .NET User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:ConnectionString" "<your-connection-string>"
```

Restore dependencies:

```bash
dotnet restore
```

Run the backend:

```bash
dotnet run
```

The backend development URL may differ depending on the local environment.

### Frontend

Navigate to the frontend directory:

```bash
cd frontend
```

Install dependencies:

```bash
npm install
```

Start the development server:

```bash
npm run dev
```

The React development server normally runs on:

```text
http://localhost:5173
```

The backend CORS policy allows requests from this development origin.

## Database

The backend uses Entity Framework Core with PostgreSQL.

Database-related code is located in:

```text
backend/BookClub.Api/Data/
```

The database connection string is **not stored in the repository**.

Each developer should configure it locally using .NET User Secrets.

Example:

```bash
dotnet user-secrets set "ConnectionStrings:ConnectionString" "<connection-string>"
```

## Git Workflow

Development is done using branches and pull requests.

Direct pushes to `main` should be avoided.

### Branch Naming

Examples:

```text
feature/open-library-search
feature/want-to-read
fix/reading-progress-validation
docs/update-readme
refactor/book-service
```

Common branch prefixes:

```text
feature/
fix/
hotfix/
refactor/
chore/
docs/
test/
release/
ci/
build/
perf/
style/
```

### Typical Workflow

```bash
git switch main
git pull

git switch -c feature/example-feature

# Make changes

git add .
git commit -m "feat: add example feature"
git push -u origin feature/example-feature
```

Then create a Pull Request into `main`.

```text
main
  │
  └── feature branch
          │
          ├── commits
          │
          └── Pull Request
                  │
                  ├── code review
                  └── merge into main
```

## Pull Request Guidelines

Before merging a Pull Request:

- Make sure the project builds successfully
- Review the changed files
- Test the affected functionality
- Resolve review comments
- Avoid committing secrets or database passwords
- Keep API contracts consistent
- Prefer DTOs instead of exposing database entities directly

Backend build check:

```bash
cd backend/BookClub.Api
dotnet build
```

## Documentation

Project documentation is stored in:

```text
docs/
```

Current documentation includes API contracts and development notes.

## Team

Developed by a four-person university Software Engineering team.

Team responsibilities are split across:

- Frontend development
- Backend development
- Database development
- External API integration

Code is integrated through GitHub Pull Requests and peer review.

## Development Status

The application is under active development.

The current priority is completing the first end-to-end flow:

```text
Book Search
    ↓
Add to Want to Read
    ↓
Mark as Finished
    ↓
Rate Book
```

More advanced book club, social, statistics, and event functionality will be developed after the core application flow is stable.
