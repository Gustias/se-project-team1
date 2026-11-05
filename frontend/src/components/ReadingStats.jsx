import {useEffect, useState} from "react";
import {getReadingSummary} from "../api/client.js";
import {Bar, BarChart, CartesianGrid, Tooltip, XAxis, YAxis} from "recharts";

function fillMonths(days, from, to) {
  const result = [];

  const fromParts = from.split("-");
  const toParts = to.split("-");
  const current = new Date(Number(fromParts[0]), Number(fromParts[1]) - 1, 1);
  const end = new Date(Number(toParts[0]), Number(toParts[1]) - 1, 1);

  while (current <= end) {
    const month = toIso(current).slice(0, 7); // "get current month"

    // add up every day that belongs to this month
    let total = 0;
    for (const day of days) {
      if (day.date.startsWith(month)) {
        total = total + day.pagesRead;
      }
    }

    result.push({ date: month, pagesRead: total });
    current.setMonth(current.getMonth() + 1); // next month
  }

  return result;
}
function fillDays(days, from, to) {
  const result = [];

  // turn "2026-10-07" into a Date
  const fromParts = from.split("-");
  const toParts = to.split("-");
  const current = new Date(Number(fromParts[0]), Number(fromParts[1]) - 1, Number(fromParts[2]));
  const end = new Date(Number(toParts[0]), Number(toParts[1]) - 1, Number(toParts[2]));

  // go through every date
  while (current <= end) {
    const date = toIso(current);

    // look for this date in the data from the API, if not found make it 0 pages read
    let pagesRead = 0;
    for (const day of days) {
      if (day.date === date) {
        pagesRead = day.pagesRead;
      }
    }

    result.push({ date: date, pagesRead: pagesRead });
    current.setDate(current.getDate() + 1); // move to the next day
  }
  return result;
}

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
      .then((data) => {
        if (range === "month") {
          setDays(fillDays(data.days, from, to));
        } else if (data.days.length === 0) {
          setDays([]);
        } else if (range === "year") {
          setDays(fillMonths(data.days, from, to));
        } else {
          // "all": start at the first month that has a log, not at 2000
          setDays(fillMonths(data.days, data.days[0].date, to));
        }
      })
      .catch((err) => console.log(err));
  }, [range]);
  return (
    <>
      <h1>Stats for pages read</h1>
      <div>
        <BarChart width={1000} height={400} data={days}>
          <Bar dataKey="pagesRead" fill="#303030" />
          <XAxis dataKey="date" interval={range === "month" ? 0 : "preserveStartEnd"} tickFormatter={(d) => (range === "month" ? Number(d.slice(8)) : d)} />
          <YAxis dataKey="pagesRead" allowDecimals={false}/>
          <Tooltip />
          <CartesianGrid stroke="#484848  " />
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
