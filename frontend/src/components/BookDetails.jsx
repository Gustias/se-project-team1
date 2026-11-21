import { useEffect, useState } from "react";
import { Link, useLocation, useParams } from "react-router-dom";
import { client } from "../api/index.js";

const PAGE_SIZE = 20;

function descriptionText(description) {
  if (!description) return "";
  return typeof description === "string" ? description : description.value ?? "";
}

function EditionRow({ edition }) {
  const [imgFailed, setImgFailed] = useState(false);

  const meta = [
    edition.publishers.join(", "),
    edition.publishDate,
    edition.pages ? `${edition.pages} pages` : null,
    edition.format,
    edition.languages.length ? edition.languages.join(", ").toUpperCase() : null,
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
        {edition.isbn10 && <p className="meta">ISBN-10: {edition.isbn10}</p>}
        <a href={edition.url} target="_blank" rel="noreferrer">
          View on Open Library
        </a>
      </div>
    </li>
  );
}

function BookDetails() {
  const { externalId } = useParams();
  const location = useLocation();

  // The search card passes the book along so the page renders instantly.
  const [book, setBook] = useState(location.state?.book ?? null);
  const [bookError, setBookError] = useState("");
  const [imgFailed, setImgFailed] = useState(false);

  const [editions, setEditions] = useState([]);
  const [total, setTotal] = useState(0);
  const [loadingEditions, setLoadingEditions] = useState(true);
  const [editionsError, setEditionsError] = useState("");

  // Fetch the work itself when we arrived via a direct link / refresh.
  useEffect(() => {
    if (location.state?.book) return;

    const controller = new AbortController();
    setBookError("");

    client
      .getWork(externalId, controller.signal)
      .then((work) => {
        setBook({
          externalId,
          title: work.title,
          author: null,
          description: descriptionText(work.description),
          coverUrl: work.covers?.find((id) => id > 0)
            ? `https://covers.openlibrary.org/b/id/${work.covers.find((id) => id > 0)}-L.jpg`
            : null,
        });
      })
      .catch((err) => {
        if (err.name !== "AbortError") setBookError(err.message);
      });

    return () => controller.abort();
  }, [externalId, location.state]);

  // Load the first page of editions.
  useEffect(() => {
    const controller = new AbortController();

    setEditions([]);
    setTotal(0);
    setEditionsError("");
    setLoadingEditions(true);

    client
      .getWorkEditions(externalId, { limit: PAGE_SIZE, offset: 0 }, controller.signal)
      .then((result) => {
        setEditions(result.editions);
        setTotal(result.total);
      })
      .catch((err) => {
        if (err.name !== "AbortError") setEditionsError(err.message);
      })
      .finally(() => {
        if (!controller.signal.aborted) setLoadingEditions(false);
      });

    return () => controller.abort();
  }, [externalId]);

  async function handleLoadMore() {
    setLoadingEditions(true);
    setEditionsError("");

    try {
      const result = await client.getWorkEditions(externalId, {
        limit: PAGE_SIZE,
        offset: editions.length,
      });
      setEditions((prev) => [...prev, ...result.editions]);
      setTotal(result.total);
    } catch (err) {
      setEditionsError(err.message);
    } finally {
      setLoadingEditions(false);
    }
  }

  const description = descriptionText(book?.description);

  return (
    <section className="book-details">
      <Link className="back-link" to="/">← Back to search</Link>

      {bookError && <p className="error">{bookError}</p>}

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
            {description && <p className="description">{description}</p>}
          </div>
        </div>
      )}

      <h2 className="editions-title">
        Editions{total > 0 && ` (${total})`}
      </h2>

      {editionsError && <p className="error">{editionsError}</p>}

      {!loadingEditions && !editionsError && editions.length === 0 && (
        <p>No editions found.</p>
      )}

      <ul className="editions">
        {editions.map((edition) => (
          <EditionRow key={edition.key} edition={edition} />
        ))}
      </ul>

      {loadingEditions && <p>Loading editions...</p>}

      {!loadingEditions && editions.length < total && (
        <button type="button" className="load-more" onClick={handleLoadMore}>
          Load more editions
        </button>
      )}
    </section>
  );
}

export default BookDetails;
