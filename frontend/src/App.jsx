import { useEffect } from "react";
import * as api from "./api/index.js";

function App() {
  useEffect(() => {
    api.searchBooks("dune")
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