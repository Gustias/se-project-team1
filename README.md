# Book Club

A university Software Engineering team project for discovering books, managing a personal reading library, and building toward social book-club features.

## Current Scope

The first development stage focuses on the core personal-library flow:

- Search for books
- Add books to a **Want to Read** list
- Remove books from the **Want to Read** list
- Mark books as finished
- Rate finished books from **1 to 5**

The backend already supports real book search through Open Library and reading-progress CRUD operations. The remaining first-stage work is focused on connecting the frontend and implementing the personal-library features above.

## Current Backend Features

- ASP.NET Core REST API
- Entity Framework Core
- PostgreSQL database support
- Neon-compatible database connection
- Real Open Library book search
- Reading progress create, read, update, and delete endpoints
- DTO-based API responses
- React development CORS configuration
- .NET User Secrets for database credentials
- User registration, login, and logout using ASP.NET Core Identity
- Cookie-based authentication
- CSRF protection

## Current Frontend Features

- Book search using Open Library
- Book details and cover images
- Adding books to a personal reading list
- Changing reading status
- Rating books from 1 to 5
- User registration and login pages
- Logout functionality
- Displaying the logged-in user's name
- Keeping users logged in after refreshing the page

## Planned Features

Later development may include:

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

### External API

- Open Library

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

The frontend communicates with the ASP.NET Core backend instead of calling Open Library directly.

External API responses are mapped into application DTOs before being returned to the frontend. This keeps the frontend independent from the raw Open Library response format.

For more detail, see [Architecture](docs/architecture.md).

## Project Structure

```text
se-project/
├── backend/
│   └── BookClub.Api/
│       ├── Controllers/
│       ├── Services/
│       ├── DTOs/
│       ├── Models/
│       ├── Data/
│       └── Integrations/
│
├── frontend/
│   ├── src/
│   │   ├── api/
│   │   ├── components/
│   │   ├── constants/
│   │   ├── App.jsx
│   │   ├── index.css
│   │   └── main.jsx
│   ├── index.html
│   └── package.json
│
├── docs/
│   ├── api.md
│   ├── architecture.md
│   └── user-stories.md
│
├── CONTRIBUTING.md
└── README.md
```

## API

### Book Search

```http
GET /api/books/search?q=dune
```

The backend requests matching books from Open Library and returns the application's own response format.

Example:

```json
[
    {
        "externalId": "/works/OL...",
        "title": "Dune",
        "author": "Frank Herbert",
        "coverUrl": "https://covers.openlibrary.org/b/id/..."
    }
]
```

The search currently requests up to 30 results.

### Work Details

```http
GET /api/works/OL45883W
```

Returns title, description, cover and first publish date for one work. `404` if the work does not exist, `502` if Open Library cannot be reached.

### Book Editions

```http
GET /api/editions/search?workId=OL45883W&limit=20&offset=0
```

Returns one page of editions for a work. `workId` is the work's Open Library id (without `/works/`). `limit` is 1-50 (default 20) and `offset` is 0 or greater (default 0).

Example:

```json
{
    "total": 57,
    "items": [
        {
            "externalId": "OL1M",
            "workExternalId": "OL45883W",
            "title": "Dune",
            "subtitle": null,
            "publishers": ["Ace"],
            "publishDate": "1990",
            "pageCount": 412,
            "format": "Paperback",
            "language": "eng",
            "isbn10": "0441172717",
            "isbn13": "9780441172719",
            "coverUrl": "https://covers.openlibrary.org/b/id/...-M.jpg"
        }
    ]
}
```

`total` is the number of editions Open Library has for the work, so clients can page with `offset`. Responses: `400` for invalid parameters, `404` if the work does not exist, `502` if Open Library cannot be reached.

### Reading Progress

```text
POST   /api/reading-progress
GET    /api/reading-progress/{id}
PUT    /api/reading-progress/{id}
DELETE /api/reading-progress/{id}
```

Full API documentation is available in [docs/api.md](docs/api.md).

### Authentication

```text
POST /api/auth/register
POST /api/auth/login
POST /api/auth/logout
GET  /api/auth/me
GET  /api/auth/csrf
```

Users can register, log in, and log out.
Authentication uses cookies to keep users logged in.

## Running the Backend

### Requirements

Install:

- .NET 10 SDK
- Git

A PostgreSQL-compatible database connection is required for database functionality.

### Configure the Database Connection

Navigate to the backend project:

```bash
cd backend/BookClub.Api
```

Set the connection string using .NET User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:ConnectionString" "<your-connection-string>"
```

The connection string must not be committed to the repository.

### Restore, Build, and Run

```bash
dotnet restore
dotnet build
dotnet run --launch-profile https
```

Authentication requires HTTPS during local development.

The backend runs at:
https://localhost:7211

## Running the Frontend

Navigate to the frontend folder:

```bash
cd frontend
npm install
```

Create a `.env` file with:

```env
VITE_API_BASE_URL=https://localhost:7211
```

The frontend uses HTTPS for authentication. Local development requires a trusted HTTPS certificate.

The Vite configuration expects certificate files in:

- `frontend/.cert/localhost.pem`
- `frontend/.cert/localhost.key`

These files are not committed to Git and must be generated locally.

Start the frontend:

```bash
npm run dev
```

The frontend runs at `https://localhost:5173` when HTTPS is configured.

The backend CORS policy allows requests from this origin.

## Database

Database code is located in:

```text
backend/BookClub.Api/Data/
```

The project uses Entity Framework Core with PostgreSQL.

Application-specific data belongs in the database. The project does not copy the complete Open Library catalog into PostgreSQL.

### Authentication Database Changes

ASP.NET Core Identity is used to store user accounts.

The Identity migration renames the existing `Users` table to
`AspNetUsers` and creates additional Identity tables.

The migration was tested on a separate Neon database branch
(`feature/user-authentication`).

Do not apply this migration to the shared Neon database
until the team is ready to merge and apply these changes.

## Git Workflow

Development is done through branches and Pull Requests.

Direct pushes to `main` should be avoided.

Typical workflow:

```bash
git switch main
git pull
git switch -c feature/example-feature

# make changes

git add .
git commit -m "feat: add example feature"
git push -u origin feature/example-feature
```

Then create a Pull Request into `main`, review the changes, resolve feedback, and merge once the branch is ready.

Examples:

```text
feature/open-library-search
feature/want-to-read
fix/reading-progress-validation
docs/update-api-documentation
refactor/book-service
```

See [CONTRIBUTING.md](CONTRIBUTING.md) for the full team workflow.

## Documentation

| Document                             | Description                                            |
| ------------------------------------ | ------------------------------------------------------ |
| [User Stories](docs/user-stories.md) | User stories                                           |
| [API Documentation](docs/api.md)     | Endpoints, request bodies, responses, and status codes |
| [Architecture](docs/architecture.md) | High-level application structure and responsibilities  |
| [Contributing](CONTRIBUTING.md)      | Branches, commits, Pull Requests, and review workflow  |

Documentation should be updated in the same Pull Request whenever API contracts, setup requirements, or architecture change.

## Team

Developed by a four-person Software Engineering team.

Responsibilities are split across:

- Frontend development
- Backend development
- Database development
- External API integration

Code is integrated through Pull Requests and peer review.

## Development Status

Current priority:

```text
Search for a book
        ↓
Add to Want to Read
        ↓
Mark as Finished
        ↓
Rate the book
```

The backend and frontend foundations are in place. The next major milestone is fully completing personal-library features.
