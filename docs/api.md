# API Documentation

This document describes the current backend API available in the `main` branch.

> The local backend port may differ depending on the development environment.  
> Examples below use `http://localhost:5027`.

## Base URL

```text
http://localhost:5027
```

During local development, CORS is enabled for the React development origin:

```text
http://localhost:5173
```

---

# Book Search

Searches for books by title.

## Endpoint

```http
GET /api/books/search
```

## Query Parameters

| Parameter | Type | Required | Description |
|---|---|---|---|
| `q` | string | Yes | Search query / book title |

## Example Request

```http
GET http://localhost:5027/api/books/search?q=dune
```

## Example Response

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

## Response Fields

| Field | Type | Description |
|---|---|---|
| `externalId` | string | External identifier of the book |
| `title` | string | Book title |
| `author` | string | Book author or authors |
| `coverUrl` | string or null | URL of the book cover |

## Errors

If `q` is empty or contains only whitespace:

```http
GET /api/books/search?q=
```

Response:

```text
400 Bad Request
```

Example response body:

```text
Search query cannot be empty.
```

## Frontend Example

```javascript
const response = await fetch(
  `http://localhost:5027/api/books/search?q=${encodeURIComponent(query)}`
);

const books = await response.json();
```

## Notes

- The endpoint currently returns mock data in the `main` branch.
- Open Library integration is being developed separately and will replace the mock implementation.
- The frontend should rely on this API response format instead of the raw Open Library response format.

---

# Reading Progress

Reading progress endpoints are available under:

```text
/api/reading-progress
```

A reading progress entry belongs to one user and one book.

The `progress` field must be between `0` and `100`.

---

## Create Reading Progress

Creates a new reading progress entry.

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

| Field | Type | Required | Description |
|---|---|---|---|
| `userId` | integer | Yes | ID of the user. Must be greater than `0` |
| `bookId` | integer | Yes | ID of the book. Must be greater than `0` |
| `progress` | integer | Yes | Reading progress from `0` to `100` |
| `chapter` | integer or null | No | Current chapter |

### Success Response

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
  "dateRead": "2026-09-23"
}
```

### Duplicate Entry

Only one reading progress entry should exist for the same user and book.

If an entry already exists:

```text
409 Conflict
```

Example response:

```json
{
  "message": "Reading progress for this user and book already exists."
}
```

### Validation Errors

Invalid IDs or progress values result in:

```text
400 Bad Request
```

---

## Get Reading Progress

Returns one reading progress entry by its ID.

### Endpoint

```http
GET /api/reading-progress/{id}
```

### Example Request

```http
GET http://localhost:5027/api/reading-progress/1
```

### Success Response

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
  "dateRead": "2026-09-23"
}
```

### Not Found

If the entry does not exist:

```text
404 Not Found
```

---

## Update Reading Progress

Updates an existing reading progress entry.

### Endpoint

```http
PUT /api/reading-progress/{id}
```

### Example Request

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

| Field | Type | Required | Description |
|---|---|---|---|
| `progress` | integer | Yes | Updated reading progress from `0` to `100` |
| `chapter` | integer or null | No | Updated chapter |

### Success Response

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
  "dateRead": "2026-09-23"
}
```

### Not Found

If the entry does not exist:

```text
404 Not Found
```

### Validation Error

If `progress` is outside the allowed range:

```text
400 Bad Request
```

---

## Delete Reading Progress

Deletes an existing reading progress entry.

### Endpoint

```http
DELETE /api/reading-progress/{id}
```

### Example Request

```http
DELETE http://localhost:5027/api/reading-progress/1
```

### Success Response

```text
204 No Content
```

### Not Found

If the entry does not exist:

```text
404 Not Found
```

---

# Current Endpoint Summary

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/books/search?q={query}` | Search for books |
| `POST` | `/api/reading-progress` | Create reading progress |
| `GET` | `/api/reading-progress/{id}` | Get reading progress |
| `PUT` | `/api/reading-progress/{id}` | Update reading progress |
| `DELETE` | `/api/reading-progress/{id}` | Delete reading progress |

---

# Development Notes

- API responses should use DTOs instead of exposing EF Core entities directly.
- Database credentials must not be committed to the repository.
- The frontend should communicate with the ASP.NET Core API instead of directly calling external services.
- When new endpoints are added or response contracts change, this file should be updated in the same Pull Request.
