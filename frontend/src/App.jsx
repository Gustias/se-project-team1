import BookSearch from "./components/BookSearch.jsx";
import ReadingStats from "./components/ReadingStats.jsx";
import {NavLink, Route, Routes} from "react-router-dom";
import Login from "./components/Login.jsx";
import Register from "./components/Register.jsx";

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
          <NavLink to="/login">Login</NavLink>
          <NavLink to="/register">Register</NavLink>
        </nav>
      </header>

      <Routes>
        <Route path="/" element={<BookSearch />} />
        <Route path="/stats" element={<ReadingStats />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
      </Routes>

    </main>
  );
}

export default App;
