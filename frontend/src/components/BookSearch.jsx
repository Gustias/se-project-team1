import { useState } from "react";
import { client } from "../api/index.js";

function BookSearch() {
  const [query, setQuery] = useState("");
  const [books, setBooks] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  async function handleSearch(event) {
    event.preventDefault();

    const trimmedQuery = query.trim();

    if (!trimmedQuery) return;

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

      {!loading && !error && query && books.length === 0 && (
        <p>No books found.</p>
      )}

      <section className="books">
        {books.map((book) => (
          <article className="book-card" key={book.externalId}>
            {book.coverUrl ? (
              <img src={book.coverUrl} alt={book.title} />
            ) : (
              <div className="no-cover">No cover</div>
            )}

            <div>
              <h2>{book.title}</h2>
              <p>{book.author || "Unknown author"}</p>
            </div>
          </article>
        ))}
      </section>
    </>
  );
}

export default BookSearch;