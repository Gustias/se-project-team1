import { useEffect, useRef, useState } from "react";
import { useOutletContext } from "react-router-dom";
import { client } from "../../api/index.js";
import { ReadingStatus } from "../../constants/readingStatus.js";

const PAGE_SIZE = 20;
const MAX_CHAPTER_LENGTH = 200;

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

// Finished books stay finished; a new or "want to read" entry becomes "reading".
function nextStatus(entry, completed) {
  if (completed || entry?.status === ReadingStatus.Read) {
    return ReadingStatus.Read;
  }
  if (!entry || entry.status === ReadingStatus.WantToRead) {
    return ReadingStatus.Reading;
  }
  return entry.status;
}

function getEditionYear(edition) {
  const match = edition.publishDate?.match(/\b(\d{4})\b/);
  return match ? Number(match[1]) : Number.MAX_SAFE_INTEGER;
}

function sortEditions(editions) {
  return [...editions].sort((a, b) => {
    const aHasCover = Boolean(a.coverUrl);
    const bHasCover = Boolean(b.coverUrl);

    const aHasPages = a.pageCount > 0;
    const bHasPages = b.pageCount > 0;

    // Best entries first: cover + page count.
    const aQuality =
      (aHasCover ? 1 : 0) +
      (aHasPages ? 2 : 0);

    const bQuality =
      (bHasCover ? 1 : 0) +
      (bHasPages ? 2 : 0);

    if (aQuality !== bQuality) {
      return bQuality - aQuality;
    }

    // Among similarly useful editions, prefer older/original releases.
    return getEditionYear(a) - getEditionYear(b);
  });
}

function getDefaultEdition(editions) {
  if (editions.length === 0) {
    return undefined;
  }

  // Prefer the earliest edition that has a usable page count.
  const withPages = editions
    .filter((edition) => edition.pageCount > 0)
    .sort(
      (a, b) =>
        getEditionYear(a) - getEditionYear(b),
    );

  if (withPages.length > 0) {
    return withPages[0];
  }

  // Fallback to the earliest known edition.
  return [...editions].sort(
    (a, b) =>
      getEditionYear(a) - getEditionYear(b),
  )[0];
}

function BookReadingProgress() {
  const { book, externalId } = useOutletContext();

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
  const [customTotal, setCustomTotal] = useState("");
  const [saving, setSaving] = useState(false);
  const [confirmingReset, setConfirmingReset] = useState(false);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const [pickerOpen, setPickerOpen] = useState(false);

  // Lets us cancel an in-flight "Load more" when the book changes.
  const loadMoreController = useRef(null);

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
    setLoadingMore(false);
    setConfirmingReset(false);
    setCustomTotal("");
    setLastChapter("");
    setPickerOpen(false);

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

    return () => {
      controller.abort();
      loadMoreController.current?.abort();
    };
  }, [externalId]);

  // Editions load separately, so the progress UI never waits on them.
  // Needed for new entries and for old entries that have no total yet.
  const wantsEditions =
    Boolean(user) &&
    !loading &&
    (!entry || entry.totalPages == null || pickerOpen);

  useEffect(() => {
    if (!wantsEditions) return;
    const controller = new AbortController();

    setEditionsLoading(true);
    client
      .getWorkEditions(
        externalId,
        { limit: PAGE_SIZE, offset: 0 },
        controller.signal,
      )
      .then((result) => {
        const sortedEditions = sortEditions(result.editions);

        setEditions(sortedEditions);
        setTotalEditions(result.total);
        setNextOffset(result.editions.length);

        setEditionId((currentId) => {
          if (currentId) {
            return currentId;
          }

          // Existing saved Book contains the ISBN of the edition
          // that was originally selected.
          if (entry && book?.isbn13) {
            const savedEdition = result.editions.find(
              (edition) => edition.isbn13 === book.isbn13,
            );

            if (savedEdition) {
              return savedEdition.externalId;
            }
          }

          // New entries use the best/main edition automatically.
          if (!entry) {
            return (
              getDefaultEdition(result.editions)?.externalId ?? ""
            );
          }

          return "";
        });
      })
      .catch((err) => {
        if (err.name !== "AbortError") setError(err.message);
      })
      .finally(() => {
        if (!controller.signal.aborted) setEditionsLoading(false);
      });

    return () => controller.abort();
  }, [
    externalId,
    wantsEditions,
    entry?.id,
    book?.isbn13,
  ]);

  async function handleLoadMore() {
    loadMoreController.current?.abort();
    const controller = new AbortController();
    loadMoreController.current = controller;

    setLoadingMore(true);
    setError("");

    try {
      const result = await client.getWorkEditions(
        externalId,
        { limit: PAGE_SIZE, offset: nextOffset },
        controller.signal,
      );
      setEditions((prev) =>
        sortEditions([
          ...prev,
          ...result.editions,
        ]),
      );
      setTotalEditions(result.total);
      // Advance by what we actually received, not by the requested page size.
      setNextOffset((prev) => prev + result.editions.length);
    } catch (err) {
      if (err.name !== "AbortError") setError(err.message);
    } finally {
      if (!controller.signal.aborted) setLoadingMore(false);
    }
  }

  const pagesNumber = Number(pagesRead);

  const selectedEdition =
    editions.find((e) => e.externalId === editionId) ??
    (!entry && editions.length === 1 ? editions[0] : undefined);

  const currentEdition =
    selectedEdition ??
    (entry
      ? {
          externalId: "",
          title: book?.title ?? "Current edition",
          coverUrl: book?.coverUrl ?? null,
          isbn13: book?.isbn13 ?? null,
          pageCount: entry.totalPages ?? null,
          publishDate: book?.year
            ? String(book.year)
            : null,
          format: null,
          publishers: [],
        }
      : null);

  const showPicker =
    pickerOpen &&
    (editionsLoading || editions.length > 0);

  const needsEdition = !entry && (editionsLoading || editions.length > 0);

  const needsCustomTotal =
    wantsEditions && Boolean(selectedEdition) && !(selectedEdition.pageCount > 0);
  const customTotalNumber = Number(customTotal);
  const customTotalValid =
    Number.isInteger(customTotalNumber) && customTotalNumber > 0;

  const totalPages = needsCustomTotal
    ? customTotalValid
      ? customTotalNumber
      : null
    : selectedEdition?.pageCount || entry?.totalPages || null;
  const hasTotal = totalPages > 0;

  // One place decides what is wrong, so the message always matches the check.
  let validationError = null;
  if (needsEdition && !selectedEdition) {
    validationError = editionsLoading
      ? "Loading editions..."
      : "Choose an edition first.";
  } else if (needsCustomTotal && customTotal !== "" && !customTotalValid) {
    validationError = "Enter the total pages as a whole number above 0.";
  } else if (pagesRead === "" || !Number.isInteger(pagesNumber) || pagesNumber < 0) {
    validationError = "Enter a whole number, 0 or more.";
  } else if (hasTotal && pagesNumber > totalPages) {
    validationError = `This edition has ${totalPages} pages.`;
  } else if (lastChapter.length > MAX_CHAPTER_LENGTH) {
    validationError = `Last chapter must be ${MAX_CHAPTER_LENGTH} characters or fewer.`;
  }
  const valid = validationError === null;

  const percent =
    entry && hasTotal
      ? Math.min(100, Math.round(((entry.pagesRead ?? 0) / totalPages) * 100))
      : null;

  // Without a known total we fall back to the typed value, which must be positive.
  const finishPages = hasTotal ? totalPages : pagesNumber;
  const canFinish = Number.isInteger(finishPages) && finishPages > 0;

  async function handleSave(event) {
    event.preventDefault();
    if (!valid) return;

    setSaving(true);
    setMessage("");
    setError("");

    const completed = hasTotal && pagesNumber >= totalPages;
    const status = nextStatus(entry, completed);

    const progress = hasTotal
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
      setMessage(
        status === ReadingStatus.Read ? "Book completed!" : "Progress saved.",
      );
    } catch (err) {
      setError(err.message || "Could not save progress.");
    } finally {
      setSaving(false);
    }
  }

  async function handleMarkAsFinished() {
    if (!entry || !canFinish) return;

    setSaving(true);
    setMessage("");
    setError("");

    try {
      const saved = await client.updateReadingProgress(entry.id, {
        status: ReadingStatus.Read,
        progress: 100,
        pagesRead: finishPages,
        totalPages,
      });

      setEntry(saved);
      setPagesRead(String(finishPages));
      setMessage("Book completed!");
    } catch (err) {
      setError(err.message || "Could not finish book.");
    } finally {
      setSaving(false);
    }
  }

  // A typed total belongs to one edition, so picking another clears it.
  function handleSelectEdition(id) {
    if (id !== editionId) {
      setCustomTotal("");
    }

    setEditionId(id);
    setPickerOpen(false);
    setMessage("");
  }

  // Deletes the entry. With no entry, the edition picker comes back on its own.
  async function handleReset() {
    if (!entry) return;

    setSaving(true);
    setMessage("");
    setError("");

    try {
      await client.deleteReadingProgress(entry.id);

      setEntry(null);
      setEditionId("");
      setCustomTotal("");
      setPagesRead("0");
      setLastChapter("");
      setConfirmingReset(false);
      setMessage("Progress cleared.");
    } catch (err) {
      setError(err.message || "Could not clear progress.");
    } finally {
      setSaving(false);
    }
  }

  // Editing a field makes any earlier "saved" message out of date.
  function handlePagesChange(event) {
    setPagesRead(event.target.value);
    setMessage("");
  }

  function handleChapterChange(event) {
    setLastChapter(event.target.value);
    setMessage("");
  }

  return (
    <div className="book-section">
      <h3 className="editions-title">
        Reading progress
      </h3>

      {!user && !loading ? (
        <p className="meta">
          Log in to track your reading progress.
        </p>
      ) : (
        <>
          <div className="reading-progress-layout">
            <section className="reading-progress-panel">
              <h4>Progress</h4>

              {entry ? (
                <>
                  <p className="reading-progress-summary">
                    {hasTotal
                      ? `You've read ${entry.pagesRead ?? 0} of ${totalPages} pages (${percent}%).`
                      : `You've read ${entry.pagesRead ?? 0} pages so far.`}
                  </p>

                  {hasTotal && (
                    <progress
                      className="reading-progress-bar"
                      value={entry.pagesRead ?? 0}
                      max={totalPages}
                      aria-label="Reading progress"
                    />
                  )}

                  {entry.lastChapterRead && (
                    <p className="meta">
                      Last chapter read:{" "}
                      {entry.lastChapterRead}
                    </p>
                  )}

                  {entry.status ===
                    ReadingStatus.Read && (
                    <p className="progress-success">
                      You've finished this book!
                    </p>
                  )}
                </>
              ) : (
                <p className="meta">
                  You haven't started tracking this
                  book yet.
                </p>
              )}

              <form
                className="progress-form"
                onSubmit={handleSave}
              >
                {needsCustomTotal && (
                  <label>
                    Total pages
                    <input
                      type="number"
                      min="1"
                      step="1"
                      placeholder="e.g. 320"
                      value={customTotal}
                      onChange={(event) => {
                        setCustomTotal(
                          event.target.value,
                        );
                        setMessage("");
                      }}
                    />
                  </label>
                )}

                <label>
                  {hasTotal
                    ? `Pages read (of ${totalPages})`
                    : "Pages read"}
                  <input
                    type="number"
                    min="0"
                    step="1"
                    max={
                      hasTotal
                        ? totalPages
                        : undefined
                    }
                    value={pagesRead}
                    onChange={handlePagesChange}
                  />
                </label>

                <label>
                  Last chapter read
                  <input
                    type="text"
                    maxLength={MAX_CHAPTER_LENGTH}
                    placeholder="e.g. Chapter 12"
                    value={lastChapter}
                    onChange={handleChapterChange}
                  />
                </label>

                <div className="progress-actions">
                  <button
                    type="submit"
                    className="edition-pick"
                    disabled={!valid || saving}
                  >
                    {saving
                      ? "Saving..."
                      : entry
                        ? "Update progress"
                        : "Start tracking"}
                  </button>

                  {entry &&
                    entry.status !==
                      ReadingStatus.Read && (
                      <button
                        type="button"
                        className="edition-pick"
                        disabled={
                          saving || !canFinish
                        }
                        onClick={
                          handleMarkAsFinished
                        }
                      >
                        Mark as finished
                      </button>
                    )}

                  {entry &&
                    (confirmingReset ? (
                      <>
                        <span className="meta">
                          Clear all progress?
                        </span>

                        <button
                          type="button"
                          className="edition-pick"
                          disabled={saving}
                          onClick={handleReset}
                        >
                          Yes, clear
                        </button>

                        <button
                          type="button"
                          className="edition-pick"
                          disabled={saving}
                          onClick={() =>
                            setConfirmingReset(false)
                          }
                        >
                          Cancel
                        </button>
                      </>
                    ) : (
                      <button
                        type="button"
                        className="edition-pick"
                        disabled={saving}
                        onClick={() =>
                          setConfirmingReset(true)
                        }
                      >
                        Clear progress
                      </button>
                    ))}
                </div>
              </form>

              {validationError && (
                <p className="error">
                  {validationError}
                </p>
              )}

              {message && (
                <p className="progress-success">
                  {message}
                </p>
              )}

              {error && (
                <p className="error">{error}</p>
              )}
            </section>

            <aside className="current-edition-card">
              <h4>Current edition</h4>

              {editionsLoading &&
              !currentEdition ? (
                <p className="meta">
                  Finding main edition...
                </p>
              ) : currentEdition ? (
                <>
                  {currentEdition.coverUrl ? (
                    <img
                      className="current-edition-cover"
                      src={currentEdition.coverUrl}
                      alt=""
                    />
                  ) : (
                    <div className="current-edition-no-cover">
                      No cover
                    </div>
                  )}

                  <strong>
                    {currentEdition.title ??
                      book?.title ??
                      "Unknown edition"}
                  </strong>

                  <div className="current-edition-meta">
                    {currentEdition.publishDate && (
                      <span>
                        {currentEdition.publishDate}
                      </span>
                    )}

                    {currentEdition.pageCount >
                      0 && (
                      <span>
                        {currentEdition.pageCount} pages
                      </span>
                    )}

                    {currentEdition.format && (
                      <span>
                        {currentEdition.format}
                      </span>
                    )}

                    {currentEdition.isbn13 && (
                      <span>
                        ISBN-13:{" "}
                        {currentEdition.isbn13}
                      </span>
                    )}
                  </div>
                </>
              ) : (
                <p className="meta">
                  No edition information available.
                </p>
              )}

              <button
                type="button"
                className="change-edition-btn"
                onClick={() =>
                  setPickerOpen((open) => !open)
                }
              >
                {pickerOpen
                  ? "Hide editions"
                  : "Change edition"}
              </button>
            </aside>
          </div>

          {showPicker && (
            <section className="edition-picker-panel">
              <div className="edition-picker-header">
                <div>
                  <h4>Choose another edition</h4>
                  <p className="meta">
                    Editions with page counts and
                    covers are shown first.
                  </p>
                </div>

                <button
                  type="button"
                  className="edition-picker-close"
                  onClick={() =>
                    setPickerOpen(false)
                  }
                >
                  Close
                </button>
              </div>

              {editionsLoading &&
                editions.length === 0 && (
                  <p>Loading editions...</p>
                )}

              <ul className="editions">
                {editions.map((edition) => (
                  <EditionPickRow
                    key={edition.externalId}
                    edition={edition}
                    selected={
                      selectedEdition?.externalId ===
                      edition.externalId
                    }
                    onSelect={
                      handleSelectEdition
                    }
                  />
                ))}
              </ul>

              {loadingMore && (
                <p>Loading editions...</p>
              )}

              {!loadingMore &&
                nextOffset < totalEditions && (
                  <button
                    type="button"
                    className="load-more"
                    onClick={handleLoadMore}
                  >
                    Load more editions
                  </button>
                )}
            </section>
          )}
        </>
      )}
    </div>
  );
}

export default BookReadingProgress;