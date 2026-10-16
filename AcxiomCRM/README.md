# AcxiomCRM — Backend Starter Template

AcxiomCRM is a production-ready, frontend-independent ASP.NET Core 8.0 Web API starter solution built following Clean Architecture principles. It delivers an enterprise-grade backend with secure cookie authentication, role-based authorization, Entity Framework Core with SQL Server persistence, comprehensive validation, Docker Compose orchestration, and extensive documentation for decoupled React + Vite integration.

---

## 1. Solution Architecture Overview

```
AcxiomCRM/
├── AcxiomCRM.sln
├── src/
│   ├── AcxiomCRM.Domain/          # Pure business entities, enums, domain rules & constants
│   ├── AcxiomCRM.Application/     # Application contracts, DTOs, FluentValidators & interfaces
│   ├── AcxiomCRM.Infrastructure/  # EF Core, Identity, SQL Server configs, Repositories, Services
│   └── AcxiomCRM.Api/             # Thin controllers, Middlewares, DI composition, Swagger
├── tests/
│   ├── AcxiomCRM.UnitTests/       # Unit tests for validators, business rules, calculations
│   └── AcxiomCRM.IntegrationTests/# Integration tests via WebApplicationFactory
├── docs/                          # Comprehensive architectural and integration guides
├── docker-compose.yml             # Containerized SQL Server & API setup
└── Dockerfile                     # Multi-stage optimized .NET 8.0 image
```

### Request Flow
```
HTTP Request 
  -> ExceptionHandlingMiddleware 
  -> CORS (SpaCorsPolicy) 
  -> Authentication (Identity Cookie) 
  -> Authorization (Role / Owner Policies) 
  -> Controller 
  -> FluentValidation 
  -> Application Service 
  -> EF Core ApplicationDbContext 
  -> SQL Server
```

---

## 2. Prerequisites

- **.NET 8.0 SDK** (LTS) or higher: [Download .NET 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server 2019/2022** or **Docker Desktop**
- (Optional for Docker orchestration) **Docker Engine 24+** & **Docker Compose v2+**

---

## 3. Quickstart: Running Outside Docker (.NET CLI)

1. **Clone or Extract the Solution**:
   ```bash
   cd AcxiomCRM
   ```

2. **Configure Database Connection**:
   Update `src/AcxiomCRM.Api/appsettings.json` or create `appsettings.Development.json` with your local SQL Server instance:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost,1433;Database=AcxiomCrmDb;User Id=sa;Password=Your_strong_password123!;TrustServerCertificate=True;MultipleActiveResultSets=true;"
   }
   ```

3. **Restore Dependencies & Build**:
   ```bash
   dotnet restore
   dotnet build
   ```

4. **Apply EF Core Migrations**:
   ```bash
   dotnet tool install --global dotnet-ef  # If not already installed
   dotnet ef migrations add InitialCreate --project src/AcxiomCRM.Infrastructure --startup-project src/AcxiomCRM.Api
   dotnet ef database update --project src/AcxiomCRM.Infrastructure --startup-project src/AcxiomCRM.Api
   ```

5. **Run the API**:
   ```bash
   dotnet run --project src/AcxiomCRM.Api
   ```
   The API will listen at `http://localhost:5000` (or configured HTTPS port).
   Open Swagger UI at: `http://localhost:5000/swagger`

---

## 4. Quickstart: Running via Docker Compose

AcxiomCRM is configured with Docker Compose orchestration including health checks and Apple Silicon (ARM64) support via `mcr.microsoft.com/azure-sql-edge`.

1. **Prepare Environment File**:
   ```bash
   cp .env.example .env
   ```

2. **Start SQL Server and API Containers**:
   ```bash
   docker compose up --build -d
   ```

3. **Check Container Status**:
   ```bash
   docker compose ps
   ```
   Both `acxiomcrm-sqlserver` and `acxiomcrm-api` should show healthy/running.

4. **Explore Swagger UI**:
   Navigate to: `http://localhost:5000/swagger`

5. **Stop Containers**:
   ```bash
   docker compose down
   ```

---

## 5. Seeded Development Accounts

Upon first launch, `ApplicationDbContextSeed` automatically provisions the core roles and accounts:

| Role | Email | Password | Allowed Capabilities |
|---|---|---|---|
| **Admin** | `admin@acxiomcrm.local` | `CrmDev@2024Secure!` | Full access to all CRM records, user admin, audit logs, reports |
| **Manager** | `manager@acxiomcrm.local` | `CrmDev@2024Secure!` | Team CRM records, sales pipeline, management reports |
| **SalesExecutive** | `sales@acxiomcrm.local` | `CrmDev@2024Secure!` | Access limited to assigned customers, leads, opportunities, follow-ups |

> [!WARNING]
> Default seed passwords are strictly prevented in production environments unless explicitly supplied via `SEED_ADMIN_PASSWORD` environment variable.

---

## 6. Running Automated Tests

Run unit tests and integration tests using standard dotnet test commands:

```bash
dotnet test
```

---

## 7. Documentation Index

Detailed documentation files located in `/docs`:
- [`ARCHITECTURE.md`](file:///c:/Users/MAHNISH/Desktop/rendushe/AcxiomCRM/docs/ARCHITECTURE.md): Architectural rules, layer boundaries, and request lifecycle.
- [`API_REFERENCE.md`](file:///c:/Users/MAHNISH/Desktop/rendushe/AcxiomCRM/docs/API_REFERENCE.md): Complete catalog of REST endpoints, DTO contracts, and HTTP status codes.
- [`REACT_INTEGRATION.md`](file:///c:/Users/MAHNISH/Desktop/rendushe/AcxiomCRM/docs/REACT_INTEGRATION.md): Complete guide for frontend developers with Axios/fetch patterns, CORS, and CSRF.
- [`FEATURE_IMPLEMENTATION_GUIDE.md`](file:///c:/Users/MAHNISH/Desktop/rendushe/AcxiomCRM/docs/FEATURE_IMPLEMENTATION_GUIDE.md): Step-by-step tutorial on adding new entities and endpoints (e.g. Tasks module).
- [`VALIDATION_AND_AUTHORIZATION.md`](file:///c:/Users/MAHNISH/Desktop/rendushe/AcxiomCRM/docs/VALIDATION_AND_AUTHORIZATION.md): Detailed validation matrix and ownership vs role authorization rules.
- [`DATABASE_SCHEMA.md`](file:///c:/Users/MAHNISH/Desktop/rendushe/AcxiomCRM/docs/DATABASE_SCHEMA.md): Entity relationships, indexes, keys, and migration commands.
- [`SECURITY.md`](file:///c:/Users/MAHNISH/Desktop/rendushe/AcxiomCRM/docs/SECURITY.md): Security posture, cookie policies, CSRF mitigation, lockout, and audit trails.
- [`DOCKER_GUIDE.md`](file:///c:/Users/MAHNISH/Desktop/rendushe/AcxiomCRM/docs/DOCKER_GUIDE.md): Docker architecture, network configuration, and Apple Silicon compatibility.
- [`TROUBLESHOOTING.md`](file:///c:/Users/MAHNISH/Desktop/rendushe/AcxiomCRM/docs/TROUBLESHOOTING.md): Diagnosis and remedies for common deployment, CORS, and connection issues.
