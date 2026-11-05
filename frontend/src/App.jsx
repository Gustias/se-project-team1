import BookSearch from "./components/BookSearch.jsx";
import ReadingStats from "./components/ReadingStats.jsx";

function App() {
  return (
    <main className="container">
      <h1>Book Club</h1>
      <BookSearch />
      <ReadingStats></ReadingStats>
    </main>
  );
}

export default App;
