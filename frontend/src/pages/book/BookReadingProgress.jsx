import { useEffect, useState } from "react";
import { Link, useLocation, useOutletContext } from "react-router-dom";
import { client } from "../../api/index.js";
import { ReadingStatus } from "../../constants/readingStatus.js";

const PAGE_SIZE = 20;

// Same look as the Editions tab, plus a button to choose the edition.
function EditionPickRow({ edition, selected, onSelect }) {
  const [imgFailed, setImgFailed] = useState(false);

  const meta = [
    (edition.publishers ?? []).join(", "),
    edition.publishDate,
    edition.pageCount ? `${edition.pageCount} pages` : null,
    edition.format,
    edition.language ? edition.language.toUpperCase() : null,
  ].filter(Boolean);

  return (
    <li className="edition">
      {edition.coverUrl && !imgFailed ? (
        <img
          src={edition.coverUrl}
          alt=""
          loading="lazy"
          onError={() => setImgFailed(true)}
        />
      ) : (
        <div className="no-cover">No cover</div>
      )}

      <div className="edition-info">
        <h3>
          {edition.title}
          {edition.subtitle ? `: ${edition.subtitle}` : ""}
        </h3>
        {meta.length > 0 && <p className="meta">{meta.join(" · ")}</p>}
        {edition.isbn13 && <p className="meta">ISBN-13: {edition.isbn13}</p>}
        <button
          type="button"
          className="edition-pick"
          aria-pressed={selected}
          onClick={() => onSelect(edition.externalId)}
        >
          {selected ? "Selected" : "Select this edition"}
        </button>
      </div>
    </li>
  );
}

function BookReadingProgress() {
  const { book, externalId } = useOutletContext();
  const location = useLocation();

  const [loading, setLoading] = useState(true);
  const [user, setUser] = useState(null);
  const [entry, setEntry] = useState(null);
  const [editions, setEditions] = useState([]);
  const [totalEditions, setTotalEditions] = useState(0);
  const [nextOffset, setNextOffset] = useState(0);
  const [editionsLoading, setEditionsLoading] = useState(false);
  const [loadingMore, setLoadingMore] = useState(false);
  const [editionId, setEditionId] = useState("");
  const [pagesRead, setPagesRead] = useState("0");
  const [lastChapter, setLastChapter] = useState("");
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  useEffect(() => {
    const controller = new AbortController();

    setLoading(true);
    setError("");
    setMessage("");
    setEntry(null);
    setEditions([]);
    setTotalEditions(0);
    setNextOffset(0);
    setEditionId("");
    setEditionsLoading(false);
    setLastChapter("");

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
        setLastChapter(existing?.lastChapterRead ?? "");
      } catch (err) {
        if (err.name !== "AbortError") setError(err.message);
      } finally {
        if (!controller.signal.aborted) setLoading(false);
      }
    })();

    return () => controller.abort();
  }, [externalId]);

  // Editions load separately, so the progress UI never waits on them.
  // Needed for new entries and for old entries that have no total yet.
  const wantsEditions = Boolean(user) && !loading && (!entry || entry.totalPages == null);

  useEffect(() => {
    if (!wantsEditions) return;
    const controller = new AbortController();

    setEditionsLoading(true);
    client
      .getWorkEditions(externalId, { limit: PAGE_SIZE, offset: 0 }, controller.signal)
      .then((result) => {
        setEditions(result.editions);
        setTotalEditions(result.total);
        setNextOffset(PAGE_SIZE);
      })
      .catch((err) => {
        if (err.name !== "AbortError") setError(err.message);
      })
      .finally(() => {
        if (!controller.signal.aborted) setEditionsLoading(false);
      });

    return () => controller.abort();
  }, [externalId, wantsEditions]);

  async function handleLoadMore() {
    setLoadingMore(true);
    setError("");

    try {
      const result = await client.getWorkEditions(externalId, {
        limit: PAGE_SIZE,
        offset: nextOffset,
      });
      setEditions((prev) => [...prev, ...result.editions]);
      setTotalEditions(result.total);
      setNextOffset((prev) => prev + PAGE_SIZE);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoadingMore(false);
    }
  }

  const pagesNumber = Number(pagesRead);
  // A lone edition is picked automatically for a new entry.
  const selectedEdition =
    editions.find((e) => e.externalId === editionId) ??
    (!entry && editions.length === 1 ? editions[0] : undefined);
  // Picker area shows for new entries and for old entries that have no total yet.
  const showPicker = wantsEditions && (editionsLoading || editions.length > 0);
  // Only a brand-new entry must pick one; old entries can still update without it.
  const needsEdition = !entry && (editionsLoading || editions.length > 0);
  // Chosen edition on first save, then the total stored on the entry.
  const totalPages = selectedEdition?.pageCount ?? entry?.totalPages ?? null;

  const valid =
    pagesRead !== "" &&
    Number.isInteger(pagesNumber) &&
    pagesNumber >= 0 &&
    (!totalPages || pagesNumber <= totalPages) &&
    lastChapter.length <= 200 &&
    (!needsEdition || Boolean(selectedEdition));

  const percent =
    entry && totalPages
      ? Math.min(100, Math.round(((entry.pagesRead ?? 0) / totalPages) * 100))
      : null;

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

    // Backend "progress" is a 0-100 percent; keep it in sync when we know the total.
    const progress = totalPages
      ? Math.min(100, Math.round((pagesNumber / totalPages) * 100))
      : (entry?.progress ?? 0);

    const chapter = lastChapter.trim() || null;

    try {
      let saved;

      if (entry) {
        saved = await client.updateReadingProgress(entry.id, {
          progress,
          lastChapterRead: chapter,
          status,
          pagesRead: pagesNumber,
          totalPages,
        });
      } else {
        // Not on the user's list yet: save the book, then start tracking it.
        const savedBook = await client.createBook({
          externalId: client.toWorkId(externalId),
          isbn10: selectedEdition?.isbn10 ?? null,
          isbn13: selectedEdition?.isbn13 ?? null,
          title: selectedEdition?.title ?? book?.title ?? "Untitled",
          author: book?.author || null,
        });

        saved = await client.createReadingProgress({
          bookId: savedBook.id,
          progress,
          lastChapterRead: chapter,
          status,
          pagesRead: pagesNumber,
          totalPages,
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

      {entry ? (
        <>
          <p className="meta">
            {totalPages
              ? `You've read ${entry.pagesRead ?? 0} of ${totalPages} pages (${percent}%).`
              : `You've read ${entry.pagesRead ?? 0} pages so far.`}
          </p>
          {totalPages && (
            <progress
              value={entry.pagesRead ?? 0}
              max={totalPages}
              aria-label="Reading progress"
              style={{ width: "100%" }}
            />
          )}
          {entry.lastChapterRead && (
            <p className="meta">Last chapter read: {entry.lastChapterRead}</p>
          )}
        </>
      ) : (
        <p className="meta">You haven't started tracking this book yet.</p>
      )}

      {showPicker && (
        <>
          <h4>
            {entry
              ? "Choose your edition to see the total pages"
              : "Which edition are you reading?"}
          </h4>
          {editionsLoading && editions.length === 0 && <p>Loading editions...</p>}

          <ul className="editions">
            {editions.map((edition) => (
              <EditionPickRow
                key={edition.externalId}
                edition={edition}
                selected={selectedEdition?.externalId === edition.externalId}
                onSelect={setEditionId}
              />
            ))}
          </ul>

          {loadingMore && <p>Loading editions...</p>}

          {!loadingMore && nextOffset < totalEditions && (
            <button type="button" className="load-more" onClick={handleLoadMore}>
              Load more editions
            </button>
          )}
        </>
      )}

      <form className="progress-form" onSubmit={handleSave}>
        <label>
          {totalPages ? `Pages read (of ${totalPages})` : "Pages read"}
          <input
            type="number"
            min="0"
            step="1"
            max={totalPages || undefined}
            value={pagesRead}
            onChange={(e) => setPagesRead(e.target.value)}
          />
        </label>
        <label>
          Last chapter read
          <input
            type="text"
            maxLength={200}
            placeholder="e.g. Chapter 12"
            value={lastChapter}
            onChange={(e) => setLastChapter(e.target.value)}
          />
        </label>
        <button type="submit" className="edition-pick" disabled={!valid || saving}>
          {saving ? "Saving..." : entry ? "Update progress" : "Start tracking"}
        </button>
      </form>

      {!valid && pagesRead !== "" && (
        <p className="error">
          {needsEdition && !selectedEdition
            ? editionsLoading
              ? "Loading editions..."
              : "Choose an edition first."
            : totalPages && pagesNumber > totalPages
              ? `This edition has ${totalPages} pages.`
              : "Enter a whole number, 0 or more."}
        </p>
      )}
      {message && <p className="progress-success">{message}</p>}
      {error && <p className="error">{error}</p>}
    </div>
  );
}

export default BookReadingProgress;
