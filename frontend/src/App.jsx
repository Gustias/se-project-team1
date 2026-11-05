import BookSearch from "./components/BookSearch.jsx";
import ReadingStats from "./components/ReadingStats.jsx";
import {Link, Route, Routes} from "react-router-dom";



function App() {
  return (
    <main className="container">
      <h1>Book Club</h1>

      <Routes>
        <Route path="/" element={<BookSearch />} />
        <Route path="/stats" element={<ReadingStats />} />
      </Routes>

    </main>
  );
}

export default App;
