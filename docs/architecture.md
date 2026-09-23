# Architecture

This document describes the current high-level architecture of the Book Club application.

## Overview

The application follows a client-server architecture.

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

The frontend should not call Open Library directly. External API communication is handled by the backend so the application controls its own API contract.

## Frontend

Technology:

```text
React
```

Responsibilities:

- render the user interface;
- collect user input;
- call backend API endpoints;
- display loading and error states;
- display normalized backend responses.

Example request:

```http
GET /api/books/search?q=dune
```

The frontend depends on the response format defined by our backend, not the raw Open Library JSON structure.

## Backend

Technology:

```text
C#
ASP.NET Core
.NET 10
```

The backend exposes REST endpoints and coordinates application logic.

Current backend structure:

```text
backend/BookClub.Api/
├── Controllers/
├── Services/
├── DTOs/
├── Models/
├── Data/
└── Integrations/
```

### Controllers

Controllers handle HTTP requests and responses.

Their responsibilities should remain small:

```text
Request
   ↓
Validation / routing
   ↓
Service call
   ↓
HTTP response
```

Controllers should not contain large amounts of business or database logic.

Examples:

```text
BooksController
ReadingProgressController
```

### Services

Services contain application logic and coordinate database or external API operations.

Example:

```text
BooksController
      ↓
BookService
      ↓
Open Library
```

Another example:

```text
ReadingProgressController
          ↓
ReadingProgressService
          ↓
AppDbContext
```

### DTOs

DTOs define the data contract exposed by the API.

Example book search response:

```json
{
  "externalId": "/works/OL893415W",
  "title": "Dune",
  "author": "Frank Herbert",
  "coverUrl": "https://covers.openlibrary.org/..."
}
```

DTOs allow the backend to control its API independently from database models and external APIs.

The frontend should consume DTOs rather than EF Core entities or raw Open Library responses.

### Models

Models represent application/database entities.

Examples include:

```text
Book
User
ReadingProgress
ReadingLog
```

Database models should not automatically become public API response models.

### Data Layer

Entity Framework Core is used to communicate with PostgreSQL.

Main component:

```text
AppDbContext
```

The data layer contains:

- `DbContext`;
- entity configuration;
- database constraints;
- EF Core migrations.

Database credentials are not stored in Git.

## Database

Technology:

```text
PostgreSQL
Neon
Entity Framework Core
```

Application-specific information belongs in the database.

Examples:

- users;
- books referenced by the application;
- reading progress;
- reading state;
- ratings;
- future club membership and activity.

The project should not attempt to copy the complete Open Library catalog into PostgreSQL.

External book information may be stored only when required by application functionality.

## Open Library Integration

Open Library provides external book catalog data.

Expected flow:

```text
React
  ↓
GET /api/books/search?q=dune
  ↓
BooksController
  ↓
BookService
  ↓
HttpClient
  ↓
Open Library
  ↓
Open Library JSON
  ↓
BookSearchResultDto
  ↓
React
```

This separation provides several benefits:

- the frontend is independent from Open Library response changes;
- external API details stay in the backend;
- validation and error handling can be centralized;
- another book provider could be introduced later without rewriting the frontend.

## Dependency Injection

ASP.NET Core dependency injection is used to provide services to controllers.

Example conceptually:

```text
BooksController
    needs
BookService

ReadingProgressController
    needs
ReadingProgressService

ReadingProgressService
    needs
AppDbContext
```

Services must be registered in `Program.cs`.

For HTTP integrations, typed `HttpClient` registration should be preferred.

Example:

```csharp
builder.Services.AddHttpClient<BookService>(client =>
{
    client.BaseAddress = new Uri("https://openlibrary.org/");
});
```

## CORS

During local development:

```text
React:   http://localhost:5173
Backend: local ASP.NET Core port
```

Because these are different origins, the backend enables CORS for the React development origin.

The development policy should allow only the frontend origins required by the project instead of using unrestricted origins unnecessarily.

## API Design

Endpoints should follow consistent REST-style conventions.

Examples:

```text
GET    /api/books/search?q=dune

POST   /api/reading-progress
GET    /api/reading-progress/{id}
PUT    /api/reading-progress/{id}
DELETE /api/reading-progress/{id}
```

Common response codes:

```text
200 OK
201 Created
204 No Content
400 Bad Request
404 Not Found
409 Conflict
```

Detailed endpoint contracts are documented in:

```text
docs/api.md
```

## Current Core Flow

The first lab focuses on a small end-to-end product flow:

```text
Search for a book
        ↓
Add to Want to Read
        ↓
Mark as Finished
        ↓
Rate the book
```

The architecture should prioritize completing this flow before adding more advanced functionality.

## Future Architecture

Later features may introduce additional services and models for:

- book clubs;
- club membership;
- discussions;
- spoiler-safe comments;
- reading statistics;
- events;
- map integration.

These features should reuse the same general layering:

```text
Controller
    ↓
Service
    ↓
Database / External Integration
```

The architecture should remain simple until additional complexity is actually required.
