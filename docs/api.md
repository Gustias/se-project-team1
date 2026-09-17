# API Documentation

## Book Search

Searches for books by title.

### Endpoint

```http
GET /api/books/search
```

### Query parameters

| Parameter | Type | Required | Description |
|---|---|---|---|
| `q` | string | Yes | Search query / book title |

### Example request

```http
GET http://localhost:5027/api/books/search?q=dune
```

> The backend port may differ depending on the local development environment.

### Example response

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

### Response fields

| Field | Type | Description |
|---|---|---|
| `externalId` | string | External identifier of the book |
| `title` | string | Book title |
| `author` | string | Book author |
| `coverUrl` | string or null | URL of the book cover |

### Error response

If the `q` parameter is missing or empty:

```http
GET /api/books/search?q=
```

The API returns:

```text
400 Bad Request
```

### Frontend usage

During local development, the React frontend can call the backend directly:

```javascript
const response = await fetch(
  `http://localhost:5027/api/books/search?q=${encodeURIComponent(query)}`
);

const books = await response.json();
```

CORS is enabled for:

```text
http://localhost:5173
```

### Notes

- The endpoint currently returns mock book data.
- The mock implementation will later be replaced with Open Library data.
- The frontend should rely on this API response structure instead of the raw Open Library response format.
