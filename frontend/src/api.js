import { handle, BASE_URL } from "./api/client.js"

export async function searchBooks(query, signal) {
  const url = `${BASE_URL}/api/books/search?q=${encodeURIComponent(query)}`;
  const response = await fetch(url, { signal });
  return handle(response);
}