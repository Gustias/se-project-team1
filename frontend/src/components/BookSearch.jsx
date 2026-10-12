import { useState } from "react";
import { client } from "../api/index.js";
import BookCard from "./BookCard.jsx";

function BookSearch() {
  const [query, setQuery] = useState("");
  const [searchedQuery, setSearchedQuery] = useState("");
  const [books, setBooks] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  async function handleSearch(event) {
    event.preventDefault();

    const trimmedQuery = query.trim();

    if (!trimmedQuery) return;
    setSearchedQuery(trimmedQuery);
    setLoading(true);
    setError("");

    try {
      const results = await client.searchBooks(trimmedQuery);
      setBooks(results);
    } catch (err) {
      setError(err.message);
      setBooks([]);
    } finally {
      setLoading(false);
    }
  }

  return (
    <>
      <form className="search" onSubmit={handleSearch}>
        <input
          type="text"
          placeholder="Search by title or author..."
          value={query}
          maxLength={100}
          onChange={(event) => setQuery(event.target.value)}
        />

        <button type="submit" disabled={loading}>
          {loading ? "Searching..." : "Search"}
        </button>
      </form>

      {error && <p className="error">{error}</p>}

      {!loading && !error && searchedQuery && books.length === 0 && (
        <p>No books found.</p>
      )}

      <section className="books">
        {books.map((book) => (
          <BookCard key={book.externalId} book={book} />
        ))}
      </section>
    </>
  );
}

export default BookSearch;
