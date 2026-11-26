import { useEffect, useState } from "react";
import {
  getRatingList,
  getReadingSummary,
} from "../api/client.js";
import {
  Bar,
  BarChart,
  CartesianGrid,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";

function toIso(date) {
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");

  return `${date.getFullYear()}-${month}-${day}`;
}

function fillDays(days, from, to) {
  const result = [];

  const fromParts = from.split("-");
  const toParts = to.split("-");

  const current = new Date(
    Number(fromParts[0]),
    Number(fromParts[1]) - 1,
    Number(fromParts[2]),
  );

  const end = new Date(
    Number(toParts[0]),
    Number(toParts[1]) - 1,
    Number(toParts[2]),
  );

  while (current <= end) {
    const date = toIso(current);

    const matchingDay = days.find(
      (day) => day.date === date,
    );

    result.push({
      date,
      pagesRead: matchingDay?.pagesRead ?? 0,
    });

    current.setDate(current.getDate() + 1);
  }

  return result;
}

function fillMonths(days, from, to) {
  const result = [];

  const fromParts = from.split("-");
  const toParts = to.split("-");

  const current = new Date(
    Number(fromParts[0]),
    Number(fromParts[1]) - 1,
    1,
  );

  const end = new Date(
    Number(toParts[0]),
    Number(toParts[1]) - 1,
    1,
  );

  while (current <= end) {
    const month = toIso(current).slice(0, 7);

    const pagesRead = days
      .filter((day) => day.date.startsWith(month))
      .reduce(
        (total, day) => total + day.pagesRead,
        0,
      );

    result.push({
      date: month,
      pagesRead,
    });

    current.setMonth(current.getMonth() + 1);
  }

  return result;
}

function getRange(key) {
  const today = new Date();

  if (key === "month") {
    const from = new Date(today);
    from.setDate(today.getDate() - 29);

    return {
      from: toIso(from),
      to: toIso(today),
    };
  }

  if (key === "year") {
    const from = new Date(
      today.getFullYear() - 1,
      today.getMonth(),
      today.getDate(),
    );

    return {
      from: toIso(from),
      to: toIso(today),
    };
  }

  return {
    from: "2000-01-01",
    to: toIso(today),
  };
}

function getHighestRatedBook(ratingList) {
  if (!ratingList || ratingList.length === 0) {
    return null;
  }

  let highest = ratingList[0];

  for (const book of ratingList) {
    if (book.rating > highest.rating) {
      highest = book;
    }
  }

  return highest;
}

function getLowestRatedBook(ratingList) {
  if (!ratingList || ratingList.length === 0) {
    return null;
  }

  let lowest = ratingList[0];

  for (const book of ratingList) {
    if (book.rating < lowest.rating) {
      lowest = book;
    }
  }

  return lowest;
}

export default function ReadingStats() {
  const [range, setRange] = useState("month");

  const [days, setDays] = useState([]);
  const [daysRead, setDaysRead] = useState(0);
  const [totalBooksRead, setTotalBooksRead] =
    useState(0);
  const [totalPagesRead, setTotalPagesRead] =
    useState(0);

  const [ratingList, setRatingList] = useState([]);

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    let cancelled = false;

    async function loadRatings() {
      try {
        const data = await getRatingList();

        if (!cancelled) {
          setRatingList(data);
        }
      } catch (err) {
        if (!cancelled) {
          console.error(
            "Could not load ratings:",
            err,
          );
        }
      }
    }

    loadRatings();

    return () => {
      cancelled = true;
    };
  }, []);

  useEffect(() => {
    let cancelled = false;

    async function loadStats() {
      setLoading(true);
      setError("");

      const { from, to } = getRange(range);

      try {
        const data = await getReadingSummary(
          from,
          to,
        );

        if (cancelled) {
          return;
        }

        setDaysRead(data.days.length);
        setTotalBooksRead(data.totalBooksRead);
        setTotalPagesRead(data.totalPages);

        if (range === "month") {
          setDays(
            fillDays(
              data.days,
              from,
              to,
            ),
          );

          return;
        }

        if (data.days.length === 0) {
          setDays([]);
          return;
        }

        if (range === "year") {
          setDays(
            fillMonths(
              data.days,
              from,
              to,
            ),
          );

          return;
        }

        // All time starts from the first month
        // that actually has reading activity.
        setDays(
          fillMonths(
            data.days,
            data.days[0].date,
            to,
          ),
        );
      } catch (err) {
        if (!cancelled) {
          setError(
            err.message ||
              "Could not load reading statistics.",
          );
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    loadStats();

    return () => {
      cancelled = true;
    };
  }, [range]);

  const averagePagesPerReadingDay =
    daysRead > 0
      ? Math.round(totalPagesRead / daysRead)
      : 0;

  const highestRatedBook =
    getHighestRatedBook(ratingList);

  const lowestRatedBook =
    getLowestRatedBook(ratingList);

  return (
    <>
      <h1>Reading stats</h1>

      <div>
        <button
          type="button"
          onClick={() => setRange("month")}
          disabled={range === "month"}
        >
          Last 30 days
        </button>

        <button
          type="button"
          onClick={() => setRange("year")}
          disabled={range === "year"}
        >
          Last 1 year
        </button>

        <button
          type="button"
          onClick={() => setRange("all")}
          disabled={range === "all"}
        >
          All
        </button>
      </div>

      {loading ? (
        <p>Loading...</p>
      ) : error ? (
        <p className="error">{error}</p>
      ) : daysRead > 0 ? (
        <>
          <div className="stats-grid">
            <h4 className="stats-card">
              Days read: {daysRead}
            </h4>

            <h4 className="stats-card">
              Books with activity: {totalBooksRead}
            </h4>

            <h4 className="stats-card">
              Pages read: {totalPagesRead}
            </h4>

            <h4 className="stats-card">
              Average pages per reading day:{" "}
              {averagePagesPerReadingDay}
            </h4>

            {ratingList.length === 1 ? (
              <h4 className="stats-card centered-card">
                Only rated book:{" "}
                {ratingList[0].title} (
                {ratingList[0].rating}/5)
              </h4>
            ) : ratingList.length > 1 ? (
              <>
                <h4 className="stats-card">
                  Highest rated book (all time):{" "}
                  {highestRatedBook?.title} (
                  {highestRatedBook?.rating}/5)
                </h4>

                <h4 className="stats-card">
                  Lowest rated book (all time):{" "}
                  {lowestRatedBook?.title} (
                  {lowestRatedBook?.rating}/5)
                </h4>
              </>
            ) : (
              <h4 className="stats-card centered-card">
                No ratings yet
              </h4>
            )}
          </div>

          <br />

          <BarChart
            width={1000}
            height={400}
            data={days}
          >
            <CartesianGrid stroke="#484848" />

            <XAxis
              dataKey="date"
              interval={
                range === "month"
                  ? 0
                  : "preserveStartEnd"
              }
              tickFormatter={(date) =>
                range === "month"
                  ? Number(date.slice(8))
                  : date
              }
            />

            <YAxis
              allowDecimals={false}
            />

            <Tooltip />

            <Bar
              name="Pages"
              dataKey="pagesRead"
              fill="#303030"
            />
          </BarChart>
        </>
      ) : (
        <div>
          <h4>No reading activity found.</h4>

          {ratingList.length === 1 && (
            <p>
              Only rated book:{" "}
              {ratingList[0].title} (
              {ratingList[0].rating}/5)
            </p>
          )}

          {ratingList.length > 1 && (
            <>
              <p>
                Highest rated book (all time):{" "}
                {highestRatedBook?.title} (
                {highestRatedBook?.rating}/5)
              </p>

              <p>
                Lowest rated book (all time):{" "}
                {lowestRatedBook?.title} (
                {lowestRatedBook?.rating}/5)
              </p>
            </>
          )}
        </div>
      )}
    </>
  );
}