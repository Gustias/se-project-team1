import BookSearch from "./components/BookSearch.jsx";
import ReadingStats from "./components/ReadingStats.jsx";
import {Link, Route, Routes} from "react-router-dom";



function App() {
  return (
    <main className="container">

      <Routes>
        <Route path="/" element={<BookSearch />} />
        <Route path="/stats" element={<ReadingStats />} />
      </Routes>

    </main>
  );
}

export default App;
