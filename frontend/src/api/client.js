export const BASE_URL = import.meta.env.VITE_API_BASE_URL;

export async function handle(response) {
  if (!response.ok) {
    let message = `Request failed (${response.status})`;
    try {
      const body = await response.json();
      message = body.message || body.title || message;
    } catch {
      // response had no JSON body
    }
    throw new Error(message);
  }
  if (response.status === 204) return null;
  return response.json();
}

export async function searchBooks(query, signal) {
  const url = `${BASE_URL}/api/books/search?q=${encodeURIComponent(query)}`;
  const response = await fetch(url, { signal });

  console.log("API URL:", url);
  console.log("Status:", response.status);
  console.log("Content-Type:", response.headers.get("content-type"));

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

export async function updateReadingProgress(id, { progress, chapter }) {
  const response = await fetch(`${BASE_URL}/api/reading-progress/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ progress, chapter: chapter ?? null }),
  });
  return handle(response);
}

export async function deleteReadingProgress(id) {
  const response = await fetch(`${BASE_URL}/api/reading-progress/${id}`, {
    method: "DELETE",
  });
  return handle(response);
}
