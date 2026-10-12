import { useState } from "react";

function BookCard({ book }) {
  const [expanded, setExpanded] = useState(false);
  const [imgFailed, setImgFailed] = useState(false);

  const publishers = book.publishers ?? [];
  const hasDetails = Boolean(book.description) || publishers.length > 0;

  const isbn = book.isbn13 || book.isbn10;

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

      <div>
        <h2>{book.title}</h2>
        <p>{book.author || "Unknown author"}</p>
        {book.year && <p className="meta">First published: {book.year}</p>}
        {book.isbn13 && <p className="meta">ISBN-13: {book.isbn13}</p>}

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
              <div className="meta">
                {book.isbn10 && <p>ISBN-10: {book.isbn10}</p>}

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
      </div>
    </article>
  );
}

export default BookCard;
