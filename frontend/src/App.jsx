import { useEffect } from "react";
import { searchBooks } from "./api.js";

function App() {
  useEffect(() => {
    searchBooks("dune")
      .then((books) => {
        console.log(books);
      })
      .catch((error) => {
        console.error(error);
      });
  }, []);
  return <h1>Book Club</h1>;
}

export default App;