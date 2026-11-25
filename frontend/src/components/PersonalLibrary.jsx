import { useEffect, useState } from "react";
import { Link, useLocation } from "react-router-dom";
import { client } from "../api/index.js";
import { ReadingStatus } from "../constants/readingStatus.js";

function LibraryRow({ item, updating, onStatusChange }) {
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

        <label className="library-status">
          Reading status:
          <select
            value={item.status}
            disabled={updating}
            onChange={(event) =>
              onStatusChange(item, Number(event.target.value))
            }
          >
            <option value={ReadingStatus.WantToRead}>
              Want to read
            </option>
            <option value={ReadingStatus.Reading}>
              Currently reading
            </option>
            <option value={ReadingStatus.Read}>
              Read
            </option>
          </select>
        </label>

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
  const [updatingId, setUpdatingId] = useState(null);

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

  async function handleStatusChange(item, newStatus) {
    const previousStatus = item.status;

    setUpdatingId(item.id);
    setError("");

    setItems((currentItems) =>
      currentItems.map((currentItem) =>
        currentItem.id === item.id
          ? { ...currentItem, status: newStatus }
          : currentItem,
      ),
    );

    try {
      await client.updateReadingProgress(item.id, {
        progress: item.progress,
        lastChapterRead: item.lastChapterRead,
        status: newStatus,
        totalPages: item.totalPages,
        pagesRead: item.pagesRead,
      });
    } catch (err) {
      setItems((currentItems) =>
        currentItems.map((currentItem) =>
          currentItem.id === item.id
            ? { ...currentItem, status: previousStatus }
            : currentItem,
        ),
      );

      setError(
        err.message || "Could not update reading status.",
      );
    } finally {
      setUpdatingId(null);
    }
  }

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
                updating={updatingId === item.id}
                onStatusChange={handleStatusChange}
              />
            ))}
          </ul>
        </>
      )}
    </div>
  );
}

export default PersonalLibrary;