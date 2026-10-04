import { createBook } from "./client.js";

const STORAGE_KEY = "bookclub:library";

const ReadingStatus = Object.freeze({
  Reading: 0,
  Read: 1,
  WantToRead: 2,
});

function readAll() {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    return raw ? JSON.parse(raw) : {};
  } catch {
    return {};
  }
}

function writeAll(entries) {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(entries));
  } catch {
    // storage unavailable (private browsing, quota, etc.) - fail silently
  }
}

export function getLibrary() {
  return Object.values(readAll()).sort((a, b) => b.addedAt - a.addedAt);
}

export function isInLibrary(externalId) {
  return Boolean(readAll()[externalId]);
}

export async function addToWantToRead(book) {
  const entries = readAll();
  if (entries[book.externalId]) return entries[book.externalId];

  // Save book to the backend first
  await createBook({
    externalId: book.externalId,
    title: book.title,
    author: book.author,
  });

  const entry = {
    externalId: book.externalId,
    title: book.title,
    author: book.author,
    coverUrl: book.coverUrl,
    status: ReadingStatus.WantToRead,
    rating: null,
    addedAt: Date.now(),
  };
  entries[book.externalId] = entry;
  writeAll(entries);
  return entry;
}

export function markFinished(externalId) {
  const entries = readAll();
  if (!entries[externalId]) return null;
  entries[externalId] = { ...entries[externalId], status: ReadingStatus.Read };
  writeAll(entries);
  return entries[externalId];
}

export function markWantToRead(externalId) {
  const entries = readAll();
  if (!entries[externalId]) return null;
  entries[externalId] = { ...entries[externalId], status: ReadingStatus.WantToRead };
  writeAll(entries);
  return entries[externalId];
}

export function rateBook(externalId, rating) {
  const entries = readAll();
  if (!entries[externalId]) return null;
  entries[externalId] = { ...entries[externalId], rating };
  writeAll(entries);
  return entries[externalId];
}

export function removeFromLibrary(externalId) {
  const entries = readAll();
  delete entries[externalId];
  writeAll(entries);
}
