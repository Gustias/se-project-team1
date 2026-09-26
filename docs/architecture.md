# Architecture

This document describes the current high-level architecture of the Book Club application.

## Overview

The application uses a client-server architecture.

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

The frontend communicates with the ASP.NET Core backend.

The backend is responsible for:

- exposing the application's REST API;
- validating requests;
- coordinating application logic;
- accessing PostgreSQL through Entity Framework Core;
- calling Open Library;
- mapping external data into application DTOs.

## Frontend

Planned/current frontend technology:

```text
React
Vite
JavaScript
```

Responsibilities:

- render the user interface;
- collect user input;
- call backend endpoints;
- display loading and error states;
- display normalized backend responses.

The frontend should not depend directly on Open Library response structures.

## Backend

Technology:

```text
C#
ASP.NET Core
.NET 10
```

Current structure:

```text
backend/BookClub.Api/
├── Controllers/
├── Services/
├── DTOs/
├── Models/
├── Data/
└── Integrations/
```

## Controllers

Controllers define HTTP endpoints and translate service results into HTTP responses.

Example:

```text
HTTP request
    ↓
Controller
    ↓
Service
    ↓
HTTP response
```

Current examples:

```text
BooksController
ReadingProgressController
```

Controllers should remain small and should not contain database or external API implementation details.

## Services

Services contain application logic and coordinate dependencies.

### Book Search

```text
BooksController
      ↓
BookService
      ↓
HttpClient
      ↓
Open Library
```

`BookService` performs the Open Library request and maps results into `BookSearchResultDto`.

### Reading Progress

```text
ReadingProgressController
          ↓
ReadingProgressService
          ↓
AppDbContext
          ↓
PostgreSQL
```

## DTOs

DTOs define the public API contract.

Example:

```json
{
  "externalId": "/works/OL...",
  "title": "Dune",
  "author": "Frank Herbert",
  "coverUrl": "https://covers.openlibrary.org/b/id/..."
}
```

The backend should expose DTOs instead of raw EF Core entities or raw Open Library objects.

This keeps the API contract independent from:

- database schema changes;
- Open Library schema changes;
- internal implementation details.

## Models

Models represent application/database entities.

Current examples include:

```text
Book
User
ReadingProgress
ReadingLog
```

Database models should not automatically be used as public API response models.

## Data Layer

Entity Framework Core is used for database access.

Main component:

```text
AppDbContext
```

The data layer contains:

- DbContext configuration;
- database entity sets;
- indexes and constraints;
- EF Core migrations.

The database is PostgreSQL-compatible and can be hosted using Neon.

## Database Responsibilities

The application database stores project-specific state.

Examples:

- users;
- books referenced by the application;
- reading progress;
- future personal-library state;
- future ratings;
- future club membership.

The complete Open Library catalog should not be copied into PostgreSQL.

## Open Library Integration

Open Library provides external book catalog data.

Current flow:

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
Open Library Search API
  ↓
Open Library JSON
  ↓
BookSearchResultDto
  ↓
React
```

Current search behavior:

- the query is URL-encoded;
- up to 20 results are requested;
- entries without titles are filtered out;
- authors are joined into one string;
- cover IDs are mapped to Open Library cover URLs.

The frontend never needs to understand the raw Open Library response.

## Dependency Injection

ASP.NET Core dependency injection provides dependencies to controllers and services.

Conceptually:

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

`BookService` is registered as a typed `HttpClient`.

Example:

```csharp
builder.Services.AddHttpClient<BookService>(client =>
{
    client.BaseAddress = new Uri("https://openlibrary.org/");
});
```

Database and application services are registered in `Program.cs`.

## Configuration and Secrets

Database credentials must not be committed to Git.

Developers configure the database connection locally with .NET User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:ConnectionString" "<connection-string>"
```

## CORS

During local development:

```text
React:   http://localhost:5173
Backend: local ASP.NET Core port
```

Because these are different origins, the backend enables CORS for the React development origin.

## API Design

Current endpoints:

```text
GET    /api/books/search?q=dune

POST   /api/reading-progress
GET    /api/reading-progress/{id}
PUT    /api/reading-progress/{id}
DELETE /api/reading-progress/{id}
```

Common HTTP status codes currently used:

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

## Current Product Flow

The first development stage is focused on:

```text
Search for a book
        ↓
Add to Want to Read
        ↓
Mark as Finished
        ↓
Rate the book
```

Reading progress and Open Library integration already provide part of the backend foundation.

The next architecture work should prioritize completing this end-to-end flow before introducing more advanced social functionality.

## Future Architecture

Later features may introduce:

- book club services;
- membership models;
- discussions;
- spoiler-safe comments;
- statistics;
- events;
- map integration.

New features should continue using the same general layering:

```text
Controller
    ↓
Service
    ↓
Database / External Integration
```

The architecture should remain as simple as possible until additional complexity is required.
