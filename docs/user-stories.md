# User Stories

This document defines the main user stories for the Book Club application and their acceptance criteria.

The application is intended for people who read books, want to track their reading activity, discover new books, and interact with a reading community.

## Status Definitions

- **Implemented** - currently available and demonstrable in the application.
- **In Progress** - currently being developed.
- **Planned** - planned for a later development stage.

---

## US-01 - Search for books

**User Story**

As a user, I want to search for books by title or author so that I can find books I am interested in.

**Acceptance Criteria**

- The user can enter a title or author into the search field.
- The application sends the search request to the backend.
- Matching books are displayed to the user.
- Each result displays the book title and author.
- A book cover is displayed when available.
- An appropriate message is shown when no matching books are found.

**Status:** Implemented

---

## US-02 - Create an account

**User Story**

As a new user, I want to create an account so that I can use personalized features of the application.

**Acceptance Criteria**

- The user can provide the required registration information.
- Required fields are validated.
- Invalid registration data displays an appropriate error message.
- Duplicate accounts are not created for the same credentials.
- After successful registration, the user can use their account to log in.

**Status:** Planned

---

## US-03 - Log in to an account

**User Story**

As a user, I want to log in to my account so that I can access my personal library, reading progress, clubs, and other account-specific data.

**Acceptance Criteria**

- The user can enter their login credentials.
- Valid credentials allow the user to access their account.
- Invalid credentials display an appropriate error message.
- The authenticated user remains logged in while using the application.
- Account-specific data is available only to the authenticated user.

**Status:** Planned

---

## US-04 - Add a book to Want to Read

**User Story**

As a user, I want to add a book to my Want to Read list so that I can keep track of books I plan to read.

**Acceptance Criteria**

- The user can add a book from the search results.
- The selected book is stored in the personal library.
- The book is assigned the Want to Read status.
- The same book cannot be added multiple times.

**Status:** In Progress

---

## US-05 - View personal library

**User Story**

As a user, I want to view my personal library so that I can see the books I have saved and their reading statuses.

**Acceptance Criteria**

- The user can open their personal library.
- Saved books are displayed.
- Each saved book displays its current reading status.
- Books from both Want to Read and Read categories can be identified.
- An appropriate message is displayed when the library is empty.

**Status:** In Progress

---

## US-06 - Remove a book from Want to Read

**User Story**

As a user, I want to remove a book from my Want to Read list so that I can keep the list relevant.

**Acceptance Criteria**

- The user can remove a saved book from the Want to Read list.
- The removed book no longer appears in the personal library.
- Removing one book does not affect other saved books.

**Status:** In Progress

---

## US-07 - Mark a book as read

**User Story**

As a user, I want to mark a book as read so that I can keep a record of books I have completed.

**Acceptance Criteria**

- A saved book can be changed from Want to Read to Read.
- The book appears as a completed book in the user's library.
- The updated reading status is displayed to the user.
- The updated status remains available after the application is refreshed.

**Status:** In Progress

---

## US-08 - Rate a finished book

**User Story**

As a user, I want to rate a finished book on a five-point scale so that I can record my opinion about it.

**Acceptance Criteria**

- The user can rate a finished book from 1 to 5.
- Ratings outside the 1–5 range are rejected.
- A valid rating is stored.
- The saved rating is displayed to the user.
- The user can only rate a book that has been marked as read.

**Status:** In Progress

---

## US-09 - Track reading progress

**User Story**

As a user, I want to record how many pages of a book I have read so that I can track my reading progress.

**Acceptance Criteria**

- The user can enter the number of pages they have read.
- The application stores the current reading progress.
- The user can update their progress later.
- The number of pages read cannot be negative.
- The number of pages read cannot exceed the total number of pages in the book.

**Status:** Planned

---

## US-10 - View reading progress as a percentage

**User Story**

As a user, I want to see my reading progress as a percentage so that I can understand how much of the book I have completed and how much remains.

**Acceptance Criteria**

- The application calculates progress using the number of pages read and the total number of pages.
- The current completion percentage is displayed.
- The application shows how much of the book remains.
- The percentage updates when reading progress changes.
- The displayed progress cannot exceed 100%.

**Status:** Planned

---

## US-11 - Create a book club

**User Story**

As a user, I want to create a book club so that I can read and discuss books together with other people.

**Acceptance Criteria**

- The user can create a new book club.
- The user can provide a name for the club.
- A book can be associated with the club.
- The creator becomes a member of the club.
- The created club can be viewed after creation.

**Status:** Planned

---

## US-12 - Choose public or private club visibility

**User Story**

As a club creator, I want to choose whether my club is public or private so that I can control who can participate.

**Acceptance Criteria**

- A club can be created as either public or private.
- Public clubs can be discovered by other users.
- Access to private clubs is restricted.
- The club's visibility is displayed to users.
- The selected visibility is stored with the club.

**Status:** Planned

---

## US-13 - Participate in book club discussions

**User Story**

As a club member, I want to post comments and insights about a book so that I can discuss it with other readers.

**Acceptance Criteria**

- Club members can post comments.
- Comments are associated with the relevant club and book.
- Other club members can view the discussion.
- Each comment displays its author.
- Comments remain available when the discussion is opened again.

**Status:** Planned

---

## US-14 - Avoid spoilers based on reading progress

**User Story**

As a club member, I want discussion comments to respect my reading progress so that I do not see spoilers for parts of the book I have not reached yet.

**Acceptance Criteria**

- Each discussion comment contains reading-progress metadata.
- The application compares the comment progress with the user's current reading progress.
- Comments associated with progress beyond the user's current progress are hidden or restricted.
- Comments become available after the user reaches the required progress.
- Users who have already reached the required progress can view the comment normally.

**Status:** Planned

---

## US-15 - View friends' reading progress

**User Story**

As a user, I want to see my friends' reading progress so that I can follow how far they have progressed through a book.

**Acceptance Criteria**

- The user can view reading progress shared by their friends.
- Progress is displayed for the relevant book.
- Updated progress is reflected when a friend records new progress.
- The application clearly identifies which friend the progress belongs to.

**Status:** Planned

---

## US-16 - View personal reading statistics

**User Story**

As a user, I want to view statistics about my reading activity so that I can understand my reading habits.

**Acceptance Criteria**

- The user can see how many books they have read during the current year.
- The user can see the total number of pages they have read.
- The user can see their most common book genres when genre information is available.
- The user can see their highest-rated books.
- The user can see their lowest-rated books.

**Status:** Planned

---

## US-17 - Create an in-person book club meeting

**User Story**

As a user, I want to create an in-person book club meeting so that readers can meet and discuss books face to face.

**Acceptance Criteria**

- The user can specify a meeting location.
- The user can specify a meeting date.
- The user can specify a meeting time.
- The user can specify the maximum number of participants.
- Other users can view the meeting information.
- The meeting is associated with its creator.

**Status:** Planned

---

## US-18 - Discover book club meetings on a map

**User Story**

As a user, I want to view book club meetings on a map of Vilnius so that I can find meetings that are convenient for me.

**Acceptance Criteria**

- Book club meetings are displayed on a map of Vilnius.
- Each meeting marker represents the meeting's specified location.
- The user can view basic meeting information from the map.
- Meetings can be filtered by day.
- Meetings can be filtered by week.
- Meetings can be filtered by month.

**Status:** Planned