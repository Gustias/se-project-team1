import BookSearch from "./components/BookSearch.jsx";
import ReadingStats from "./components/ReadingStats.jsx";
import {NavLink, Route, Routes} from "react-router-dom";



function App() {
  return (
    <main className="container">

      <header className="site-header">
        <NavLink to="/" end>
           <h1>Book Club</h1>
        </NavLink>
        <nav>
          <NavLink to="/" end>Search</NavLink>
          <NavLink to="/stats">Stats</NavLink>
        </nav>
      </header>

      <Routes>
        <Route path="/" element={<BookSearch />} />
        <Route path="/stats" element={<ReadingStats />} />
      </Routes>

    </main>
  );
}

export default App;
