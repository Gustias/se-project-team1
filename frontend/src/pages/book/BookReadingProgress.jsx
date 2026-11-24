import { useEffect, useState } from "react";
import { Link, useLocation, useOutletContext } from "react-router-dom";
import { client } from "../../api/index.js";
import { ReadingStatus } from "../../constants/readingStatus.js";

function BookReadingProgress() {
  const { book, externalId } = useOutletContext();
  const location = useLocation();

  const [loading, setLoading] = useState(true);
  const [user, setUser] = useState(null);
  const [entry, setEntry] = useState(null);
  const [pagesRead, setPagesRead] = useState("0");
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  useEffect(() => {
    const controller = new AbortController();

    setLoading(true);
    setError("");
    setMessage("");
    setEntry(null);

    (async () => {
      try {
        const currentUser = await client.getCurrentUser();
        setUser(currentUser);
        if (!currentUser) return;

        const existing = await client.getReadingProgressByWork(
          externalId,
          controller.signal,
        );
        setEntry(existing);
        setPagesRead(String(existing?.pagesRead ?? 0));
      } catch (err) {
        if (err.name !== "AbortError") setError(err.message);
      } finally {
        if (!controller.signal.aborted) setLoading(false);
      }
    })();

    return () => controller.abort();
  }, [externalId]);

  const pagesNumber = Number(pagesRead);
  const valid =
    pagesRead !== "" && Number.isInteger(pagesNumber) && pagesNumber >= 0;

  async function handleSave(event) {
    event.preventDefault();
    if (!valid) return;

    setSaving(true);
    setMessage("");
    setError("");

    // Reading pages means you're reading; don't downgrade a finished book.
    const status =
      !entry || entry.status === ReadingStatus.WantToRead
        ? ReadingStatus.Reading
        : entry.status;

    try {
      let saved;

      if (entry) {
        saved = await client.updateReadingProgress(entry.id, {
          progress: entry.progress ?? 0,
          lastChapterRead: entry.lastChapterRead,
          status,
          pagesRead: pagesNumber,
        });
      } else {
        // Not on the user's list yet: save the book, then start tracking it.
        const savedBook = await client.createBook({
          externalId: client.toWorkId(externalId),
          title: book?.title ?? "Untitled",
          author: book?.author || null,
        });

        saved = await client.createReadingProgress({
          bookId: savedBook.id,
          progress: 0,
          lastChapterRead: null,
          status,
          pagesRead: pagesNumber,
        });
      }

      setEntry(saved);
      setMessage("Progress saved.");
    } catch (err) {
      setError(err.message || "Could not save progress.");
    } finally {
      setSaving(false);
    }
  }

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
          <Link to="/login" state={{ from: location.pathname }}>Log in</Link> to track
          your reading progress.
        </p>
      </div>
    );
  }

  return (
    <div className="book-section">
      <h3 className="editions-title">Reading progress</h3>
      <p className="meta">
        {entry
          ? `You've read ${entry.pagesRead ?? 0} pages so far.`
          : "You haven't started tracking this book yet."}
      </p>

      <form className="progress-form" onSubmit={handleSave}>
        <label>
          Pages read
          <input
            type="number"
            min="0"
            step="1"
            value={pagesRead}
            onChange={(e) => setPagesRead(e.target.value)}
          />
        </label>
        <button type="submit" className="edition-pick" disabled={!valid || saving}>
          {saving ? "Saving..." : entry ? "Update progress" : "Start tracking"}
        </button>
      </form>

      {!valid && pagesRead !== "" && (
        <p className="error">Enter a whole number, 0 or more.</p>
      )}
      {message && <p className="progress-success">{message}</p>}
      {error && <p className="error">{error}</p>}
    </div>
  );
}

export default BookReadingProgress;
