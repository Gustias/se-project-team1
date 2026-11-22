export const BASE_URL = import.meta.env.VITE_API_BASE_URL;

export async function getCsrfToken() {
  const response = await fetch(`${BASE_URL}/api/auth/csrf`, {
    credentials: "include",
  });

  const data = await handle(response);
  return data.token;
}

export async function register(userName, email, password) {
  const csrfToken = await getCsrfToken();

  const response = await fetch(`${BASE_URL}/api/auth/register`, {
    method: "POST",
    credentials: "include",
    headers: {
      "Content-Type": "application/json",
      "X-CSRF-TOKEN": csrfToken,
    },
    body: JSON.stringify({
      userName,
      email,
      password,
    }),
  });

  return handle(response);
}

export async function login(email, password) {
  const csrfToken = await getCsrfToken();

  const response = await fetch(`${BASE_URL}/api/auth/login`, {
    method: "POST",
    credentials: "include",
    headers: {
      "Content-Type": "application/json",
      "X-CSRF-TOKEN": csrfToken,
    },
    body: JSON.stringify({
      email,
      password,
    }),
  });

  return handle(response);
}

export async function logout() {
  const csrfToken = await getCsrfToken();

  const response = await fetch(`${BASE_URL}/api/auth/logout`, {
    method: "POST",
    credentials: "include",
    headers: {
      "X-CSRF-TOKEN": csrfToken,
    },
  });

  return handle(response);
}

export async function getCurrentUser() {
  const response = await fetch(`${BASE_URL}/api/auth/me`, {
    credentials: "include",
  });

  if (response.status === 401) {
    return null;
  }

  return handle(response);
}

export async function handle(response) {
  if (!response.ok) {
    let message = `Request failed (${response.status})`;
    const text = await response.text();

    try {
      const body = JSON.parse(text);

      if (Array.isArray(body.errors) && body.errors.length > 0) {
        message = body.errors.join(" ");
      } else {
        message = body.detail || body.message || body.title || message;
      }
    } catch {
      // not JSON - the API sometimes returns a plain-text message
      if (text && text.length <= 200) message = text;
    }

    throw new Error(message);
  }

  if (response.status === 204) return null;
  return response.json();
}

export async function searchBooks(query, signal) {
  const url = `${BASE_URL}/api/books/search?q=${encodeURIComponent(query)}`;
  const response = await fetch(url, { signal });

  return handle(response);
}

export async function createBook({
  externalId,
  isbn10,
  isbn13,
  title,
  author,
}) {
  const response = await fetch(`${BASE_URL}/api/books`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ externalId, isbn10, isbn13, title, author }),
  });
  return handle(response);
}

export async function getBook(id) {
  const response = await fetch(`${BASE_URL}/api/books/${id}`);
  return handle(response);
}

export async function deleteBook(id) {
  const response = await fetch(`${BASE_URL}/api/books/${id}`, {
    method: "DELETE",
  });
  return handle(response);
}

export async function createReadingProgress({
  userId,
  bookId,
  progress,
  lastChapterRead,
  status,
}) {
  const response = await fetch(`${BASE_URL}/api/reading-progress`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      userId,
      bookId,
      progress,
      lastChapterRead: lastChapterRead ?? null,
      status,
    }),
  });
  return handle(response);
}

export async function getReadingProgress(id) {
  const response = await fetch(`${BASE_URL}/api/reading-progress/${id}`);
  return handle(response);
}

export async function updateReadingProgress(
  id,
  { progress, lastChapterRead, status },
) {
  const response = await fetch(`${BASE_URL}/api/reading-progress/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      progress,
      lastChapterRead: lastChapterRead ?? null,
      status,
    }),
  });

  return handle(response);
}

export async function deleteReadingProgress(id) {
  const response = await fetch(`${BASE_URL}/api/reading-progress/${id}`, {
    method: "DELETE",
  });
  return handle(response);
}

export async function createBookRating({ userId, bookId, rating }) {
  const response = await fetch(`${BASE_URL}/api/book-ratings`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ userId, bookId, rating }),
  });

  return handle(response);
}

export async function getReadingSummary(userId, from, to){
  const response = await fetch(`${BASE_URL}/api/reading-log/summary/${userId}?from=${from}&to=${to}`);
  return handle(response);
}

// ---- Work details + editions (both served by our API) ----
// externalId may be "OL45883W" or "/works/OL45883W" - always return "OL45883W"
export function toWorkId(externalId) {
  return String(externalId).replace(/^\/?works\//, "");
}

export async function getWork(externalId, signal) {
  const response = await fetch(
    `${BASE_URL}/api/works/${encodeURIComponent(toWorkId(externalId))}`,
    { signal },
  );
  return handle(response);
}

export async function getWorkEditions(
  externalId,
  { limit = 20, offset = 0 } = {},
  signal,
) {
  const params = new URLSearchParams({
    workId: toWorkId(externalId),
    limit,
    offset,
  });

  const response = await fetch(`${BASE_URL}/api/editions/search?${params}`, {
    signal,
  });
  const data = await handle(response);

  return { total: data.total, editions: data.items };
}
