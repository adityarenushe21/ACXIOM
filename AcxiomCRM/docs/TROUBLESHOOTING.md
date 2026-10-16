# Troubleshooting Guide

Common issues, diagnosis procedures, and solutions for **AcxiomCRM**.

---

## 1. Authentication & Cookie Issues

### Symptom: Calling `/api/v1/auth/login` succeeds, but `/api/v1/customers` returns `401 Unauthorized`

- **Cause 1**: The browser or API client is not sending the authentication cookie back.
  - **Remedy**: In React `fetch()` requests, ensure `{ credentials: "include" }` is passed. In Axios, set `{ withCredentials: true }`.
- **Cause 2**: CORS headers are missing `Access-Control-Allow-Credentials: true`.
  - **Remedy**: Check that the frontend origin (`http://localhost:5173`) is listed under `Cors:AllowedOrigins` in `appsettings.json`.

---

## 2. Cross-Site Request Forgery (CSRF) 400 Bad Request

### Symptom: `POST`, `PUT`, or `DELETE` requests fail with `400 Bad Request` or anti-forgery token failure

- **Cause**: State-changing requests require the `X-XSRF-TOKEN` header.
- **Remedy**: Call `GET /api/v1/auth/csrf` once upon application load or retrieve the cookie value named `XSRF-TOKEN`, and attach it to the `X-XSRF-TOKEN` header on mutating requests.

---

## 3. Database Connection & Migration Errors

### Symptom: `SqlException: Cannot open database "AcxiomCrmDb" requested by the login.`

- **Cause**: The database has not been created yet or migrations have not been applied.
- **Remedy**: Run `dotnet ef database update --project src/AcxiomCRM.Infrastructure --startup-project src/AcxiomCRM.Api`.

### Symptom: `A connection was successfully established with the server, but then an error occurred during the login process. (provider: SSL Provider, error: 0 - The certificate chain was issued by an authority that is not trusted.)`

- **Cause**: SQL Server development certificate is self-signed.
- **Remedy**: Ensure `TrustServerCertificate=True` is included in the connection string during local development.

---

## 4. Docker & Apple Silicon Issues

### Symptom: SQL Server container exits immediately or reports `qemu: uncaught target signal 4 (Illegal instruction)` on Mac M1/M2/M3

- **Cause**: Using x86_64 MSSQL image under ARM64 emulation.
- **Remedy**: Use `mcr.microsoft.com/azure-sql-edge:latest` as configured in `docker-compose.yml`.

### Symptom: API container logs `SqlException: A network-related or instance-specific error occurred while establishing a connection to SQL Server.`

- **Cause**: Inside Docker, `localhost` refers to the API container itself, not the database container.
- **Remedy**: The connection string in Docker must specify `Server=sqlserver,1433`.

---

## 5. Port Conflicts

### Symptom: `port is already allocated: 0.0.0.0:1433` or `0.0.0.0:5000`

- **Cause**: Another local SQL Server instance or web server is holding port 1433 or 5000.
- **Remedy**: Update the host port mapping in `docker-compose.yml` (e.g. `"11433:1433"` or `"5050:5000"`) or stop conflicting local background services.
