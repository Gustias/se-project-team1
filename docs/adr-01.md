# ADR 01 - Authentication using ASP.NET Core Identity and Cookies

- **Status:** Accepted
- **Date:** 2026-11-23

## Context

The Book Club application needs authentication that lets users register, log in, log out and securely access their personal data.

The backend must identify the logged-in user instead of relying on a user ID sent by the frontend. Users must only be able to access and manage their own reading progress, reading logs and book ratings.

The application is browser-based, with REACT served from different domain as the API. Security, ease of implementation and compatibility with the existing ASP.NET Core, EF Core and PostgreSQL stack were the main criteria.

## Considered options

### 1. JWT + custom user management

- **Pros:** Stateless; works well for mobile and cross-domain clients.
- **Cons:** The frontend must store and renew tokens; tokens in JavaScript-accessible storage can be stolen via XSS; we must build password hashing, validation and lockout ourselves.

### 2. Cookies + custom user management

- **Pros:** The browser sends cookies automatically; no token handling in the frontend.
- **Cons:** Needs CSRF protection; we must build password hashing, validation and lockout ourselves.

### 3. ASP.NET Core Identity + Cookies

- **Pros:** Ready-made user management, password hashing, validation and account lockout; works with EF Core and PostgreSQL; HttpOnly cookies cannot be read by JavaScript.
- **Cons:** Adds Identity tables to the database; needs CSRF protection and careful cookie configuration; ties us to the Identity framework.

## Decision

We decided to use **ASP.NET Core Identity with Cookie Authentication**.

Security was the main factor. Identity provides tested password hashing, validation and lockout, so we did not build them ourselves. HttpOnly cookies also protect the session from token theft via XSS.

JWT was rejected because we have no mobile or third-party clients that would benefit from stateless tokens and it would add token management to the frontend. Custom user management was rejected because it duplicates security-critical functionality that Identity already provides.

## Consequences

### Positive

- Less custom authentication code to maintain.
- User and password management is built in.
- Login persists across page refreshes.
- Session cookies are not accessible to JavaScript (HttpOnly).

### Negative

- Identity adds extra tables to the database.
- Requests that change data need CSRF protection (antiforgery tokens).
- Because the frontend and API are on different domains, we must configure CORS (with credentials allowed) and cookie settings (`Secure`, `SameSite`, HTTPS) carefully.
- Switching to another approach later will be harder.
- Running several backend instances requires shared Data Protection keys.
