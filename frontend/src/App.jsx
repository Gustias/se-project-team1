import { useState } from "react";
import { client } from "./api/index.js";

function App() {
  const [query, setQuery] = useState("");
  const [books, setBooks] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  async function handleSearch(event) {
    event.preventDefault();

    if (!query.trim()) return;

    setLoading(true);
    setError("");

    try {
      const results = await client.searchBooks(query);
      setBooks(results);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="container">
      <h1>Book Club</h1>

      <form className="search" onSubmit={handleSearch}>
        <input
          type="text"
          placeholder="Search by title or author..."
          value={query}
          onChange={(event) => setQuery(event.target.value)}
        />

        <button type="submit">Search</button>
      </form>

      {loading && <p>Searching...</p>}
      {error && <p className="error">{error}</p>}

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
    </main>
  );
}

export default App;