import { useEffect, useState } from "react";
import { Link, useLocation } from "react-router-dom";
import { client } from "../api/index.js";
import { ReadingStatus } from "../constants/readingStatus.js";

function getStatusLabel(status) {
  switch (status) {
    case ReadingStatus.Reading:
      return "Currently reading";
    case ReadingStatus.Read:
      return "Read";
    case ReadingStatus.WantToRead:
      return "Want to read";
    default:
      return "Unknown";
  }
}

function LibraryRow({ item }) {
  const [imgFailed, setImgFailed] = useState(false);

  const coverUrl = item.isbn13
    ? `https://covers.openlibrary.org/b/isbn/${item.isbn13}-M.jpg?default=false`
    : null;

  const workPath =
    `/books/${encodeURIComponent(client.toWorkId(item.externalId))}`;

  return (
    <li className="edition">
      {coverUrl && !imgFailed ? (
        <img
          src={coverUrl}
          alt=""
          loading="lazy"
          onError={() => setImgFailed(true)}
        />
      ) : (
        <div className="no-cover">No cover</div>
      )}

      <div className="edition-info">
        <h3>
          <Link className="book-link" to={workPath}>
            {item.title}
          </Link>
        </h3>

        <p className="meta">
          {item.author || "Unknown author"}
        </p>

        <p className="meta">
          Status: {getStatusLabel(item.status)}
        </p>

        <Link
          className="view-editions-btn"
          to={workPath}
        >
          View details
        </Link>
      </div>
    </li>
  );
}

function PersonalLibrary() {
  const location = useLocation();

  const [user, setUser] = useState(null);
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    const controller = new AbortController();

    async function loadLibrary() {
      setLoading(true);
      setError("");

      try {
        const currentUser = await client.getCurrentUser();
        setUser(currentUser);

        if (!currentUser) {
          return;
        }

        const library = await client.getPersonalLibrary(
          controller.signal,
        );

        setItems(library);
      } catch (err) {
        if (err.name !== "AbortError") {
          setError(
            err.message || "Could not load your library.",
          );
        }
      } finally {
        if (!controller.signal.aborted) {
          setLoading(false);
        }
      }
    }

    loadLibrary();

    return () => controller.abort();
  }, []);

  if (loading) {
    return (
      <div className="book-section">
        <p>Loading library...</p>
      </div>
    );
  }

  if (!user) {
    return (
      <div className="book-section">
        <p>
          <Link
            to="/login"
            state={{ from: location.pathname }}
          >
            Log in
          </Link>{" "}
          to view your personal library.
        </p>
      </div>
    );
  }

  return (
    <div className="book-section">
      <h2>My Library</h2>

      {error && (
        <p className="error">{error}</p>
      )}

      {!error && items.length === 0 && (
        <p>
          Your library is empty.{" "}
          <Link to="/">Find a book</Link> to get started.
        </p>
      )}

      {!error && items.length > 0 && (
        <>
          <p className="meta">
            {items.length}{" "}
            {items.length === 1 ? "book" : "books"}
          </p>

          <ul className="editions">
            {items.map((item) => (
              <LibraryRow
                key={item.id}
                item={item}
              />
            ))}
          </ul>
        </>
      )}
    </div>
  );
}

export default PersonalLibrary;