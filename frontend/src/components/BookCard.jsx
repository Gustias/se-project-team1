import { useState } from "react";
import { client } from "../api/index.js";

function BookCard({ book }) {
  const [expanded, setExpanded] = useState(false);
  const [imgFailed, setImgFailed] = useState(false);

  const [status, setStatus] = useState("WantToRead");
  const [savedStatus, setSavedStatus] = useState(null);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  const [savedBookId, setSavedBookId] = useState(null);
  const [readingProgressId, setReadingProgressId] = useState(null);

  const [rating, setRating] = useState(0);
  const [ratingSaving, setRatingSaving] = useState(false);
  const [ratingMessage, setRatingMessage] = useState("");
  const [ratingError, setRatingError] = useState("");

  const statusValues = {
    Reading: 0,
    Read: 1,
    WantToRead: 2,
  };

  const publishers = book.publishers ?? [];
  const hasDetails = Boolean(book.description) || publishers.length > 0;

  async function handleAddToList() {
    setSaving(true);
    setMessage("");
    setError("");

    try {
      const savedBook = await client.createBook({
        externalId: book.externalId,
        isbn10: book.isbn10 ?? null,
        isbn13: book.isbn13 ?? null,
        title: book.title,
        author: book.author || null,
      });

      setSavedBookId(savedBook.id);

      const readingProgress = await client.createReadingProgress({
        userId: 1,
        bookId: savedBook.id,
        progress: 0,
        lastChapterRead: null,
        status: statusValues[status],
      });

      setReadingProgressId(readingProgress.id);
      setSavedStatus(status);
      setMessage("Book added to your list!");
    } catch (err) {
      setError(err.message || "Couldn't add this book.");
    } finally {
      setSaving(false);
    }
  }

  async function handleStatusChange(event) {
    const newStatus = event.target.value;

    if (!readingProgressId) {
      setStatus(newStatus);
      return;
    }

    const previousStatus = status;

    setStatus(newStatus);
    setSaving(true);
    setMessage("");
    setError("");

    try {
      await client.updateReadingProgress(readingProgressId, {
        progress: 0,
        lastChapterRead: null,
        status: statusValues[newStatus],
      });

      setSavedStatus(newStatus);
      setMessage("Reading status updated!");
    } catch (err) {
      setStatus(previousStatus);
      setError(err.message || "Could not update reading status.");
    } finally {
      setSaving(false);
    }
  }

  async function handleSaveRating() {
    if (!savedBookId || rating === 0) return;

    setRatingSaving(true);
    setRatingMessage("");
    setRatingError("");

    try {
      await client.createBookRating({
        userId: 1,
        bookId: savedBookId,
        rating,
      });

      setRatingMessage("Rating saved!");
    } catch (err) {
      setRatingError(err.message || "Could not save rating.");
    } finally {
      setRatingSaving(false);
    }
  }

  return (
    <article className="book-card">
      {book.coverUrl && !imgFailed ? (
        <img
          src={book.coverUrl}
          alt={book.title}
          loading="lazy"
          onError={() => setImgFailed(true)}
        />
      ) : (
        <div className="no-cover">No cover</div>
      )}

      <div className="info">
        <h2>{book.title}</h2>

        <p>{book.author || "Unknown author"}</p>

        {book.year && (
          <p className="meta">First published: {book.year}</p>
        )}

        {book.isbn13 && (
          <p className="meta">ISBN-13: {book.isbn13}</p>
        )}

        {hasDetails && (
          <>
            <button
              type="button"
              onClick={() => setExpanded((prev) => !prev)}
              aria-expanded={expanded}
            >
              {expanded ? "Hide details" : "Show details"}
            </button>

            {expanded && (
              <div className="details">
                {book.isbn10 && (
                  <p className="meta">ISBN-10: {book.isbn10}</p>
                )}

                {book.description && (
                  <p className="description">{book.description}</p>
                )}

                {publishers.length > 0 && (
                  <>
                    <h3>Publishers</h3>

                    <ul>
                      {publishers.map((name) => (
                        <li key={name}>{name}</li>
                      ))}
                    </ul>
                  </>
                )}
              </div>
            )}
          </>
        )}

        <div className="reading-list">
          <label>
            Reading status:
            <select
              value={status}
              onChange={handleStatusChange}
              disabled={saving}
            >
              <option value="WantToRead">Want to read</option>
              <option value="Reading">Currently reading</option>
              <option value="Read">Read</option>
            </select>
          </label>

          <button
            type="button"
            onClick={handleAddToList}
            disabled={saving || readingProgressId !== null}
          >
            {readingProgressId
              ? "Added to my list"
              : saving
                ? "Adding..."
                : "Add to my list"}
          </button>

          {message && <p>{message}</p>}
          {error && <p className="error">{error}</p>}
        </div>

        {savedStatus === "Read" && savedBookId && (
          <div className="book-rating">
            <p>Rate this book:</p>

            <div className="rating-stars">
              {[1, 2, 3, 4, 5].map((value) => (
                <button
                  key={value}
                  type="button"
                  aria-label={`Rate ${value} out of 5`}
                  aria-pressed={rating === value}
                  onClick={() => setRating(value)}
                  disabled={ratingSaving || Boolean(ratingMessage)}
                >
                  {value <= rating ? "★" : "☆"}
                </button>
              ))}
            </div>

            <button
              type="button"
              onClick={handleSaveRating}
              disabled={
                rating === 0 ||
                ratingSaving ||
                Boolean(ratingMessage)
              }
            >
              {ratingSaving ? "Saving..." : "Save rating"}
            </button>

            {ratingMessage && <p>{ratingMessage}</p>}
            {ratingError && <p className="error">{ratingError}</p>}
          </div>
        )}
      </div>
    </article>
  );
}

export default BookCard;