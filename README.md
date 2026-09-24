# Book Club

A web application for discovering books, managing a personal reading library, tracking reading activity, and eventually participating in book clubs and reading-related events.

This project is being developed as a university Software Engineering team project.

## Core Scope

The first development stage focuses on the main personal library flow:

- Search for books
- Add books to a **Want to Read** list
- Remove books from the **Want to Read** list
- Mark books as finished
- Rate finished books from **1 to 5**

More advanced social and reading-tracking functionality will be developed after the core flow is stable.

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
- Pull Request based team workflow and code review

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

The React frontend communicates with the ASP.NET Core backend.

External API responses are mapped into application DTOs before being returned to the frontend. This keeps the frontend independent from the raw Open Library response format.

For a more detailed explanation, see [Architecture](docs/architecture.md).

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
│       ├── Services/
│       │   └── Application and business logic
│       ├── DTOs/
│       │   └── API data contracts
│       ├── Models/
│       │   └── Database models
│       ├── Data/
│       │   ├── AppDbContext
│       │   └── EF Core migrations
│       └── Integrations/
│           └── External API integrations
│
├── docs/
│   ├── api.md
│   └── architecture.md
│
├── CONTRIBUTING.md
└── README.md
```

## API

### Book Search

```http
GET /api/books/search?q=dune
```

Example response format:

```json
[
  {
    "externalId": "test-1",
    "title": "Dune",
    "author": "Frank Herbert",
    "coverUrl": null
  }
]
```

The frontend should depend on this backend response format rather than the raw Open Library response.

### Reading Progress

```text
POST   /api/reading-progress
GET    /api/reading-progress/{id}
PUT    /api/reading-progress/{id}
DELETE /api/reading-progress/{id}
```

Full endpoint documentation is available in [API Documentation](docs/api.md).

## Running the Project

### Requirements

Install:

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

Build the project:

```bash
dotnet build
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

Each developer should configure it locally using .NET User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:ConnectionString" "<connection-string>"
```

## Git Workflow

Development is done through branches and Pull Requests.

Direct pushes to `main` should be avoided.

Typical workflow:

```bash
git switch main
git pull

git switch -c feature/example-feature

# Make changes

git add .
git commit -m "feat: add example feature"
git push -u origin feature/example-feature
```

Then create a Pull Request into `main`, review the changes, resolve feedback, and merge once the branch is ready.

Examples of branch names:

```text
feature/open-library-search
feature/want-to-read
fix/reading-progress-validation
docs/update-readme
refactor/book-service
```

Detailed contribution rules are available in [CONTRIBUTING.md](CONTRIBUTING.md).

## Documentation

| Document | Description |
|---|---|
| [API Documentation](docs/api.md) | Backend endpoints, request bodies, responses, and status codes |
| [Architecture](docs/architecture.md) | High-level application structure and responsibilities |
| [Contributing](CONTRIBUTING.md) | Branch naming, commits, Pull Requests, and code review workflow |

Documentation should be updated whenever API contracts, setup requirements, architecture decisions, or team workflow change.

## Team Workflow

The project is developed by a four-person Software Engineering team.

Responsibilities are split across:

- Frontend development
- Backend development
- Database development
- External API integration

Code is integrated through GitHub Pull Requests and peer review.

Before merging a Pull Request:

- make sure the project builds;
- test the affected functionality;
- review the changed files;
- resolve review comments;
- avoid committing secrets;
- update documentation when required.

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
