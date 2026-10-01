import { useEffect } from "react";
import { client } from "./api/index.js";

function App() {
  useEffect(() => {
    client.searchBooks("dune")
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