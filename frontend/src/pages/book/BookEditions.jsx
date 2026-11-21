import { useEffect, useState } from "react";
import { useOutletContext } from "react-router-dom";
import { client } from "../../api/index.js";

const PAGE_SIZE = 20;

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

function BookEditions() {
  const { externalId } = useOutletContext();

  const [editions, setEditions] = useState([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  // Opening the Editions tab loads the first page.
  useEffect(() => {
    const controller = new AbortController();

    setEditions([]);
    setTotal(0);
    setError("");
    setLoading(true);

    client
      .getWorkEditions(externalId, { limit: PAGE_SIZE, offset: 0 }, controller.signal)
      .then((result) => {
        setEditions(result.editions);
        setTotal(result.total);
      })
      .catch((err) => {
        if (err.name !== "AbortError") setError(err.message);
      })
      .finally(() => {
        if (!controller.signal.aborted) setLoading(false);
      });

    return () => controller.abort();
  }, [externalId]);

  async function handleLoadMore() {
    setLoading(true);
    setError("");

    try {
      const result = await client.getWorkEditions(externalId, {
        limit: PAGE_SIZE,
        offset: editions.length,
      });
      setEditions((prev) => [...prev, ...result.editions]);
      setTotal(result.total);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="book-section">
      <h3 className="editions-title">
        {total > 0 ? `${total} editions` : "Editions"}
      </h3>

      {error && <p className="error">{error}</p>}

      {!loading && !error && editions.length === 0 && <p>No editions found.</p>}

      <ul className="editions">
        {editions.map((edition) => (
          <EditionRow key={edition.key} edition={edition} />
        ))}
      </ul>

      {loading && <p>Loading editions...</p>}

      {!loading && editions.length > 0 && editions.length < total && (
        <button type="button" className="load-more" onClick={handleLoadMore}>
          Load more editions
        </button>
      )}
    </div>
  );
}

export default BookEditions;
