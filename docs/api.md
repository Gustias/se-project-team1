# API Documentation

## Book Search

Searches for books by title.

### Endpoint

GET /api/books/search

### Query parameters

| Parameter | Type | Required | Description |
|---|---|---|---|
| `q` | string | Yes | Search query / book title |

### Example request

```http
GET /api/books/search?q=dune
```

### Example response

```http
[
  {
    "externalId": "test-1",
    "title": "Dune",
    "author": "Frank Herbert",
    "coverUrl": null
  }
]
```