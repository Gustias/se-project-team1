import { useOutletContext } from "react-router-dom";
import { descriptionText } from "./BookPage.jsx";

function BookOverview() {
  const { book } = useOutletContext();
  const description = descriptionText(book?.description);
  const publishers = book?.publishers ?? [];

  if (!book) return null;

  return (
    <div className="book-section">
      {description ? (
        <p className="description">{description}</p>
      ) : (
        <p className="meta">No description available.</p>
      )}

      {book.isbn13 && <p className="meta">ISBN-13: {book.isbn13}</p>}
      {book.isbn10 && <p className="meta">ISBN-10: {book.isbn10}</p>}
      {publishers.length > 0 && (
        <p className="meta">Publishers: {publishers.join(", ")}</p>
      )}
    </div>
  );
}

export default BookOverview;
