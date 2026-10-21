# API Documentation

This document describes the current Book Club backend API.

> Local backend ports may differ by development environment.  
> Examples below use `http://localhost:5027`.

## Base URL

```text
http://localhost:5027
```

During local development, CORS is enabled for:

```text
http://localhost:5173
```

---

# Book Search

Searches for books through Open Library.

## Endpoint

```http
GET /api/books/search
```

## Query Parameters

| Parameter | Type   | Required | Description               |
| --------- | ------ | -------- | ------------------------- |
| `q`       | string | Yes      | Search query / book title |

## Example Request

```http
GET http://localhost:5027/api/books/search?q=dune
```

## Example Response

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

## Response Fields

| Field        | Type           | Description                                     |
| ------------ | -------------- | ----------------------------------------------- |
| `externalId` | string         | Open Library identifier returned by the backend |
| `title`      | string         | Book title                                      |
| `author`     | string         | One or more authors joined into a single string |
| `coverUrl`   | string or null | Open Library cover URL when available           |

## Behavior

The backend:

1. URL-encodes the search query.
2. Requests up to 100 results from Open Library.
3. Filters out entries without a title and keeps books whose title or author matches the search query.
4. Returns up to 30 matching books.
5. Maps the results to `BookSearchResultDto`.
6. Returns the normalized result to the frontend.

The frontend should depend on this response format rather than the raw Open Library response.

## Empty Query

If `q` is empty or contains only whitespace:

```http
GET /api/books/search?q=
```

Response:

```text
400 Bad Request
```

Response body:

```text
Search query cannot be empty.
```

## Frontend Example

```javascript
const response = await fetch(
    `http://localhost:5027/api/books/search?q=${encodeURIComponent(query)}`,
);

const books = await response.json();
```

---

# Books

Creates a book in the database or returns an existing book if it
has already been saved.

## Create Book

### Endpoint

```http
POST /api/books
```

### Request Body

```json
{
    "externalId": "/works/OL...W",
    "isbn10": null,
    "isbn13": "9780441172719",
    "title": "Dune",
    "author": "Frank Herbert"
}
```

### Request Fields

| Field        | Type           | Required | Description                                |
| ------------ | -------------- | -------- | ------------------------------------------ |
| `externalId` | string         | Yes      | External book identifier from Open Library |
| `isbn10`     | string or null | No       | Book ISBN-10                               |
| `isbn13`     | string or null | No       | Book ISBN-13                               |
| `title`      | string         | Yes      | Book title                                 |
| `author`     | string or null | No       | Book author                                |

### Success: New Book

If a book with the given `externalId` does not exist, it is created.

Response:

```text
201 Created
```

Example response:

```json
{
    "id": 1,
    "externalId": "/works/OL...W",
    "isbn10": null,
    "isbn13": "9780441172719",
    "title": "Dune",
    "author": "Frank Herbert"
}
```

### Success: Existing Book

If a book with the same `externalId` already exists, the API returns
the existing book instead of creating a duplicate.

Response:

```text
200 OK
```

The response body contains the existing book's data, including its
database ID. The existing book is not modified.

### Validation Error

Invalid request data may return:

```text
400 Bad Request
```

---

# Reading Progress

Reading progress endpoints are available under:

```text
/api/reading-progress
```

A reading progress entry belongs to one user and one book.

`progress` must be between `0` and `100`.

---

## Create Reading Progress

### Endpoint

```http
POST /api/reading-progress
```

### Request Body

```json
{
    "userId": 1,
    "bookId": 1,
    "progress": 10,
    "chapter": 1
}
```

### Request Fields

| Field      | Type            | Required | Description                        |
| ---------- | --------------- | -------- | ---------------------------------- |
| `userId`   | integer         | Yes      | User ID. Must be greater than `0`  |
| `bookId`   | integer         | Yes      | Book ID. Must be greater than `0`  |
| `progress` | integer         | Yes      | Reading progress from `0` to `100` |
| `chapter`  | integer or null | No       | Current chapter                    |

### Success

```text
201 Created
```

Example response:

```json
{
    "id": 1,
    "userId": 1,
    "bookId": 1,
    "progress": 10,
    "chapter": 1,
    "dateRead": "2026-09-24"
}
```

### Duplicate Entry

Only one reading progress entry should exist for the same user and book.

If one already exists:

```text
409 Conflict
```

Example:

```json
{
    "message": "Reading progress for this user and book already exists."
}
```

### Validation Error

Invalid IDs or progress values return:

```text
400 Bad Request
```

---

## Get Reading Progress

### Endpoint

```http
GET /api/reading-progress/{id}
```

### Example

```http
GET http://localhost:5027/api/reading-progress/1
```

### Success

```text
200 OK
```

Example response:

```json
{
    "id": 1,
    "userId": 1,
    "bookId": 1,
    "progress": 10,
    "chapter": 1,
    "dateRead": "2026-09-24"
}
```

### Not Found

```text
404 Not Found
```

---

## Update Reading Progress

### Endpoint

```http
PUT /api/reading-progress/{id}
```

### Example

```http
PUT http://localhost:5027/api/reading-progress/1
Content-Type: application/json
```

```json
{
    "progress": 50,
    "chapter": 5
}
```

### Request Fields

| Field      | Type            | Required | Description                        |
| ---------- | --------------- | -------- | ---------------------------------- |
| `progress` | integer         | Yes      | Updated progress from `0` to `100` |
| `chapter`  | integer or null | No       | Updated chapter                    |

### Success

```text
200 OK
```

Example response:

```json
{
    "id": 1,
    "userId": 1,
    "bookId": 1,
    "progress": 50,
    "chapter": 5,
    "dateRead": "2026-09-24"
}
```

### Not Found

```text
404 Not Found
```

### Validation Error

```text
400 Bad Request
```

---

## Delete Reading Progress

### Endpoint

```http
DELETE /api/reading-progress/{id}
```

### Example

```http
DELETE http://localhost:5027/api/reading-progress/1
```

### Success

```text
204 No Content
```

### Not Found

```text
404 Not Found
```

---

# Endpoint Summary

| Method   | Endpoint                      | Description                              |
| -------- | ----------------------------- | ---------------------------------------- |
| `GET`    | `/api/books/search?q={query}` | Search Open Library for books            |
| `POST`   | `/api/books`                  | Create a book or return an existing book |
| `POST`   | `/api/reading-progress`       | Create reading progress                  |
| `GET`    | `/api/reading-progress/{id}`  | Get reading progress                     |
| `PUT`    | `/api/reading-progress/{id}`  | Update reading progress                  |
| `DELETE` | `/api/reading-progress/{id}`  | Delete reading progress                  |

---

# Development Rules

- Frontend code should communicate with the ASP.NET Core API rather than calling Open Library directly.
- API responses should use DTOs instead of exposing EF Core entities directly.
- Database credentials must not be committed.
- New or changed endpoints should update this document in the same Pull Request.
