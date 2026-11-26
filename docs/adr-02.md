# ADR 02 – Page-Based Reading Progress Model

- **Status:** Accepted
- **Date:** 2026-11-25

## Context

The Book Club application allows users to track their reading progress. Different editions of the same book may have different page counts and some editions may not have information about the page count.

We needed a reading progress model that works with these differences and still lets users track their progress even when the total page count is unknown. Having percentage values stored in the database also allows books to be sorted, filtered, and summarized by progress in SQL queries.

## Considered options

### 1. Percentage-based progress

- **Pros:** Simple to store and display; also suitable for e-books and audiobooks, which often display progress as a percentage.
- **Cons:** Does not directly show the number of pages read; less convenient for users who track their reading by pages.

### 2. Pages read + total pages, percentage calculated when needed

- **Pros:** Avoids redundant progress data and inconsistencies between stored page counts and percentages.
- **Cons:** Sorting, filtering, and calculating statistics by percentage require additional calculations in database queries.

### 3. Pages read + total pages, stored percentage

- **Pros:** The percentage is stored in the database and can be used directly in queries.
- **Cons:** Introduces redundant data that may become inconsistent if the percentage is not recalculated when the page count changes.

## Decision

We decided to store **pages read**, **optional total page count** and the **percentage** in the database.

Storing the percentage allows progress to be sorted, filtered, and aggregated directly in SQL queries without recalculating it each time.

The frontend calculates the percentage before saving reading progress when the total page count is known. The backend validates and stores the provided values. The frontend also calculates the displayed percentage from the page counts.

If the total page count is unknown, users can still track the number of pages read. In this case, when saving progress, the previous percentage value is retained, or 0 is used for a new entry.

The total page count comes from the selected book edition. If the edition has no page count information, the user can enter it manually. This value is stored in the user's personal reading progress record.

## Consequences

### Positive

- Users can track progress by pages, which is more intuitive when reading printed books.
- Supports different book editions, including those without page count.
- Progress can be shown in both pages and percentages.
- The stored percentage can be used directly in database queries.

### Negative

- The stored percentage is redundant data and may become inconsistent if it is not recalculated after a change.
- Additional frontend logic is required for edition selection, percentage calculation, and input validation.
- The backend validates individual values but does not ensure consistency between the stored percentage and page counts.
- When the total page count is unknown, the percentage is 0 for new entries or stays at its previous value, so it can be misleading in queries.
