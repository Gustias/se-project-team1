import { useEffect, useState } from "react";
import { Link, useLocation } from "react-router-dom";
import { client } from "../api/index.js";
import { ReadingStatus } from "../constants/readingStatus.js";

function ReadingRow({ item }) {
  const [imgFailed, setImgFailed] = useState(false);

  const coverUrl = item.isbn13
    ? `https://covers.openlibrary.org/b/isbn/${item.isbn13}-M.jpg?default=false`
    : null;

  const workPath = `/books/${encodeURIComponent(client.toWorkId(item.externalId))}`;
  const percent = item.totalPages
    ? Math.min(100, Math.round((item.pagesRead / item.totalPages) * 100))
    : null;

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
        <p className="meta">{item.author || "Unknown author"}</p>

        <p className="meta">
          {item.totalPages
            ? `${item.pagesRead} of ${item.totalPages} pages (${percent}%)`
            : `${item.pagesRead} pages read`}
        </p>
        {item.totalPages && (
          <progress
            value={item.pagesRead}
            max={item.totalPages}
            aria-label={`Progress for ${item.title}`}
            style={{ width: "100%" }}
          />
        )}
        {item.lastChapterRead && (
          <p className="meta">Last chapter read: {item.lastChapterRead}</p>
        )}

        <Link className="view-editions-btn" to={`${workPath}/progress`}>
          Update progress
        </Link>
      </div>
    </li>
  );
}

function CurrentlyReading() {
  const location = useLocation();

  const [loading, setLoading] = useState(true);
  const [user, setUser] = useState(null);
  const [items, setItems] = useState([]);
  const [error, setError] = useState("");

  useEffect(() => {
    const controller = new AbortController();

    setLoading(true);
    setError("");

    (async () => {
      try {
        const currentUser = await client.getCurrentUser();
        setUser(currentUser);
        if (!currentUser) return;

        const list = await client.getReadingList(
          ReadingStatus.Reading,
          controller.signal,
        );
        setItems(list);
      } catch (err) {
        if (err.name !== "AbortError") setError(err.message);
      } finally {
        if (!controller.signal.aborted) setLoading(false);
      }
    })();

    return () => controller.abort();
  }, []);

  if (loading) {
    return (
      <div className="book-section">
        <p>Loading...</p>
      </div>
    );
  }

  if (!user) {
    return (
      <div className="book-section">
        <p>
          <Link to="/login" state={{ from: location.pathname }}>Log in</Link> to see
          the books you're reading.
        </p>
      </div>
    );
  }

  return (
    <div className="book-section">
      <h3 className="editions-title">
        {items.length > 0 ? `Currently reading (${items.length})` : "Currently reading"}
      </h3>

      {error && <p className="error">{error}</p>}

      {!error && items.length === 0 && (
        <p>
          You're not reading anything right now. <Link to="/">Find a book</Link> and
          start tracking it.
        </p>
      )}

      <ul className="editions">
        {items.map((item) => (
          <ReadingRow key={item.id} item={item} />
        ))}
      </ul>
    </div>
  );
}

export default CurrentlyReading;
