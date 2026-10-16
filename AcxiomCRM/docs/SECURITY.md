# Security Architecture & Hardening Guide

This document describes the security controls, authentication safeguards, session management, CSRF mitigation, and production deployment checklists implemented in **AcxiomCRM**.

---

## 1. Authentication Architecture

AcxiomCRM implements **First-Party ASP.NET Core Identity Cookie Authentication**:
- **Cookie Security**:
  - `HttpOnly = true`: Completely shields authentication cookies from client-side script execution (mitigating XSS theft).
  - `SameSite = SameSiteMode.Lax`: Mitigates Cross-Site Request Forgery (CSRF) while permitting legitimate cross-origin local development and top-level navigations.
  - `Secure = CookieSecurePolicy.SameAsRequest`: Ensures cookies are marked `Secure` when operating under HTTPS in staging/production environments.
  - `SlidingExpiration = true` with an 8-hour expiration lifetime.
- **REST Behavior**: Cookie redirect middleware is overridden to return explicit `401 Unauthorized` and `403 Forbidden` HTTP responses rather than HTML login redirect responses (302).

---

## 2. Cross-Site Request Forgery (CSRF) Mitigation

Because credentials (cookies) are sent automatically by web browsers:
1. State-changing requests (`POST`, `PUT`, `DELETE`, `PATCH`) must validate an anti-forgery token.
2. The endpoint `GET /api/v1/auth/csrf` issues an anti-forgery request token.
3. The React client intercepts mutating calls and passes this token in the `X-XSRF-TOKEN` HTTP header.
4. If an attacker submits a forged cross-site form, the missing or invalid header causes request termination before reaching business controllers.

---

## 3. Account Lockout & Credential Defense

- Identity tracks failed password attempts per account.
- 5 consecutive invalid login attempts automatically trigger a 15-minute account lockout.
- All failed logins, lockouts, and successful logins are logged in the `AuditLogs` table with caller IP addresses.
- Passwords are never logged in plain text or structured log attributes.

---

## 4. Cross-Origin Resource Sharing (CORS) Policy

CORS is strictly configuration-driven via `appsettings.json` or environment variables (`Cors:AllowedOrigins`):
- Explicit origins are specified (e.g. `http://localhost:5173`).
- `AllowCredentials()` is enabled only with explicit origins.
- `AllowAnyOrigin()` (`*`) is **never** combined with `AllowCredentials()`.
- CORS middleware is positioned immediately after routing and before authentication.

---

## 5. Production Hardening Checklist

- [ ] Change all default seed passwords via environment variables (`SEED_ADMIN_PASSWORD`).
- [ ] Enforce HTTPS across all ingress routes; set `CookieSecurePolicy.Always`.
- [ ] Ensure database connection strings use encrypted connections (`Encrypt=True;TrustServerCertificate=False`).
- [ ] Rotate database credentials and avoid hard-coding passwords in source files.
- [ ] Disable Swagger UI in production environments (`app.Environment.IsDevelopment()`).
- [ ] Verify that reverse proxies pass standard forwarding headers (`X-Forwarded-For`, `X-Forwarded-Proto`).
- [ ] Regularly monitor `AuditLogs` table for unauthorized access attempts.
