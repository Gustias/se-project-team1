# ADR 02 – Page-Based Reading Progress Model

- **Status:** Accepted
- **Date:** 2026-11-25

## Context

The Book Club application allows users to track their reading progress. Different editions of the same book may have different page counts and some editions may not have information about the page count.

We needed a reading progress model that works with these differences and still lets users track their progress even when the total page count is unknown. We also need progress values in the database, so books can be sorted, filtered and summarized by progress in SQL queries.

## Considered options

### 1. Percentage-based progress

- **Pros:** Simple to store and display; also suits e-books and audiobooks, which often show only a percentage.
- **Cons:** Does not show the number of pages read; less convenient for users who track reading by pages.

### 2. Pages read + total pages, percentage calculated when needed

- **Pros:** No redundant data, so no risk of inconsistency.
- **Cons:** The percentage is not stored, so sorting, filtering and statistics by progress require calculating it in every query.

### 3. Pages read + total pages, percentage stored by the backend

- **Pros:** The percentage is stored in the database and can be used directly in queries.
- **Cons:** Redundant data that can become inconsistent, for example when the total page count changes.

### 4. Pages read + total pages, percentage as a database computed column

- **Pros:** The percentage is available in the database and always consistent.
- **Cons:** The calculation lives in the database, which is harder to change and test; ties us to PostgreSQL features.

## Decision

We will store **pages read** and **optional total page count**, and also store the **percentage** in the database.

We store the percentage so that progress can be sorted, filtered and aggregated directly in SQL queries without recalculating it each time. The backend is the only place that calculates it and it recalculates the value whenever pages read or the total page count change. The frontend only displays it. If the total page count is unknown, the percentage stays empty and users can still track pages read.

The total page count comes from the selected book edition. If the edition has no page count, the user can enter it manually [for their own progress record / for the edition].

## Consequences

### Positive

- Users track progress by pages, which matches how they read printed books.
- Works with different editions, including those without a page count.
- Progress can be shown as pages and as a percentage.
- The percentage can be used directly in database queries.

### Negative

- The stored percentage is redundant data and can become inconsistent if it's not recalculated after a change.
- The backend needs extra logic for edition selection, percentage calculation and validation (pages read cannot be negative or larger than the total).
- The percentage is only as accurate as the total page count.
