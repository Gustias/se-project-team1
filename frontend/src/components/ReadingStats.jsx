import {useEffect, useState} from "react";
import {getReadingSummary} from "../api/client.js";
import {Bar, BarChart, CartesianGrid, Legend, Line, LineChart, Tooltip, XAxis, YAxis} from "recharts";


function toIso(d){
  const m = String(d.getMonth() +1).padStart(2, "0");
  const day = String(d.getDate()).padStart(2, "0");
  return `${d.getFullYear()}-${m}-${day}`;
}

function getRange(key) {
  const today = new Date();

  if (key === "month") {
    const from = new Date(today);
    from.setDate(today.getDate() - 29); // 30 days including today
    return { from: toIso(from), to: toIso(today) };
  }

  if (key === "year") {
    const from = new Date(today.getFullYear() - 1, today.getMonth(), today.getDate());
    return { from: toIso(from), to: toIso(today) };
  }

  // "all"
  return { from: "2000-01-01", to: toIso(today) };
}



export default function ReadingStats() {
  const [range, setRange] = useState("month"); // month, year, all
  const [days, setDays] = useState([]);

  useEffect(() => {
    const {from, to} = getRange(range);
    getReadingSummary(1, from, to)
      .then((data) => setDays(data.days))
      .then((data) => { console.log(range, from, to, data.days.length); setDays(data.days); })
      .catch((err) => console.log(err));
  }, [range]);
  return (
    <>
      <h1>Pages read</h1>
      <div>
        <BarChart width={1000} height={400} data={days}>
          <Bar dataKey="pagesRead" fill="#303030" />
          <XAxis dataKey="date" allowDecimals={false}/>
          <YAxis dataKey="pagesRead"/>
          <Tooltip />
          <CartesianGrid stroke="#000000  " />
        </BarChart>
      </div>
      <div>
        <button onClick={() => setRange("month")}>Last 30 days</button>
        <button onClick={() => setRange("year")}>Last 1 year</button>
        <button onClick={() => setRange("all")}>All</button>
      </div>
    </>
  );
}
