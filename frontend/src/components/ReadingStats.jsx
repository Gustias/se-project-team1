import {useEffect, useState} from "react";
import {getReadingSummary} from "../api/client.js";


function toIso(d){
  const m = String(d.getMonth() +1).padStart(2, "0");
  const day = String(d.getDate()).padStart(2, "0");
  return `${d.getFullYear()}-${m}-${day}`;
}

function getRange(key){
  const now = new Date();
  const y = now.getFullYear();
  const m = now.getMonth();

  if (key === "month") return {from: toIso(new Date(y, m, 1)), to: toIso(new Date(y, m + 1, 0)) };
  if (key === "year") return {from: toIso(new Date(y, 0, 1)), to: toIso(new Date(y, 11, 31)) };
  return {from: "2000-01-01", to: toIso(now)};
}



export default function ReadingStats() {
  const [range, setRange] = useState("month"); // month, year, all

  useEffect(() => {
    const {from, to} = getRange(range);
    getReadingSummary(1, from, to)
      .then((data) => console.log(range, data))
      .catch((err) => console.log(err));
  }, [range]);
  return "testing";
}
