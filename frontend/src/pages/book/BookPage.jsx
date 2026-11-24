import { useEffect, useState } from "react";
import { Link, NavLink, Outlet, useLocation, useParams } from "react-router-dom";
import { client } from "../../api/index.js";

// Add new sections here (and a matching <Route> in App.jsx).
const TABS = [
  { to: ".", label: "Overview", end: true },
  { to: "editions", label: "Editions" },
  { to: "progress", label: "Reading progress" },
];

export function descriptionText(description) {
  if (!description) return "";
  return typeof description === "string" ? description : description.value ?? "";
}

function BookPage() {
  const { externalId } = useParams();
  const location = useLocation();

  const [book, setBook] = useState(null);
  const [error, setError] = useState("");
  const [imgFailed, setImgFailed] = useState(false);

  // Runs once per book, not per tab. A card click passes the book along so the
  // header renders instantly; a direct link / refresh fetches it instead.
  useEffect(() => {
    const passed = location.state?.book;
    setError("");
    setImgFailed(false);

    if (passed && client.toWorkId(passed.externalId) === externalId) {
      setBook(passed);
      return;
    }

    setBook(null);
    const controller = new AbortController();

    client
      .getWork(externalId, controller.signal)
      .then((work) => {
        setBook({
          externalId,
          title: work.title,
          author: null,
          description: work.description,
          coverUrl: work.coverUrl,
        });
      })
      .catch((err) => {
        if (err.name !== "AbortError") setError(err.message);
      });

    return () => controller.abort();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [externalId]);

  return (
    <section className="book-page">
      <Link className="back-link" to="/">← Back to search</Link>

      {error && <p className="error">{error}</p>}

      {book && (
        <div className="book-header">
          {book.coverUrl && !imgFailed ? (
            <img
              src={book.coverUrl}
              alt={book.title}
              onError={() => setImgFailed(true)}
            />
          ) : (
            <div className="no-cover">No cover</div>
          )}

          <div>
            <h2>{book.title}</h2>
            <p>{book.author || "Unknown author"}</p>
            {book.year && <p className="meta">First published: {book.year}</p>}
          </div>
        </div>
      )}

      <nav className="book-tabs">
        {TABS.map((tab) => (
          <NavLink key={tab.label} to={tab.to} end={tab.end}>
            {tab.label}
          </NavLink>
        ))}
      </nav>

      {/* The active tab renders here and can read { book, externalId } */}
      <Outlet context={{ book, externalId }} />
    </section>
  );
}

export default BookPage;
