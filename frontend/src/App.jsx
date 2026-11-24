import BookSearch from "./components/BookSearch.jsx";
import ReadingStats from "./components/ReadingStats.jsx";
import { NavLink, Route, Routes, useNavigate } from "react-router-dom";
import Login from "./components/Login.jsx";
import Register from "./components/Register.jsx";
import BookPage from "./pages/book/BookPage.jsx";
import BookOverview from "./pages/book/BookOverview.jsx";
import BookEditions from "./pages/book/BookEditions.jsx";
import BookReadingProgress from "./pages/book/BookReadingProgress.jsx";
import { useEffect, useState } from "react";
import { getCurrentUser, logout } from "./api/client.js";

function App() {
  
  const [user, setUser] = useState(null);
  const [loadingUser, setLoadingUser] = useState(true);
  
  const navigate = useNavigate();

  async function handleLogout() {
    try {
      await logout();
      setUser(null);
      navigate("/login");
    } catch (error) {
      console.error("Logout failed:", error);
    }
  }

  useEffect(() => {
    async function checkUser() {
      try {
        const currentUser = await getCurrentUser();
        setUser(currentUser);
      } catch (error) {
        console.error("Failed to check current user:", error);
      } finally {
        setLoadingUser(false);
      }
    }

    checkUser();
  }, 
  []);

  return (
    <main className="container">

      <header className="site-header">
        <NavLink to="/" end>
           <h1>Book Club</h1>
        </NavLink>
        <nav>
          <NavLink to="/" end>Search</NavLink>
          <NavLink to="/stats">Stats</NavLink>

          {!loadingUser && (
            user ? (
              <>
              <span>{user.userName}</span>
              <button type="button" onClick={handleLogout}>
                Logout
              </button>
              </>
            ) : (
              <>
              <NavLink to="/login">Login</NavLink>
              <NavLink to="/register">Register</NavLink>
              </>
            )
          )}
        </nav>
      </header>

      <Routes>
        <Route path="/" element={<BookSearch />} />
        <Route path="/books/:externalId" element={<BookPage />}>
          <Route index element={<BookOverview />} />
          <Route path="editions" element={<BookEditions />} />
          <Route path="progress" element={<BookReadingProgress />} />
        </Route>
        <Route path="/stats" element={<ReadingStats />} />
        <Route 
          path="/login" 
          element={<Login onLogin={setUser} />} 
        />
        <Route path="/register" element={<Register />} />
      </Routes>

    </main>
  );
}

export default App;
