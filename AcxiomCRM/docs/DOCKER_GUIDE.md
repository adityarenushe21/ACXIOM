# Docker & Container Orchestration Guide

This guide details Docker and Docker Compose workflows, networking concepts, persistent volumes, and Apple Silicon / multi-architecture support for **AcxiomCRM**.

---

## 1. Container Topology

The solution provides two orchestrated container services defined in `docker-compose.yml`:

```
[ Developer Host / Browser (localhost) ]
          |                     |
     port 5000              port 1433
          v                     v
   +---------------+     +---------------+
   | acxiomcrm-api | --> |  sqlserver    |
   |  (.NET 8.0)   |     | (Azure SQL /  |
   |               |     |  MSSQL 2022)  |
   +---------------+     +---------------+
            \                   /
         [ Docker Compose Internal Network ]
```

### Critical Network Distinction: Container URLs vs Host URLs

- **Inside the Docker Compose Network**:
  - The API reaches the database using its internal service name: `Server=sqlserver,1433`.
  - The API does not use `localhost` to connect to SQL Server inside the container network.
- **Outside on Developer Host (Mac/Windows/Linux)**:
  - Your browser or local React dev server connects to the API via `http://localhost:5000`.
  - Management tools (Azure Data Studio, SSMS) connect to SQL Server via `localhost,1433`.

---

## 2. Apple Silicon (ARM64) & Cross-Platform Support

Standard `mcr.microsoft.com/mssql/server:2022-latest` images are x86_64 only and run slowly or fail with memory errors under Rosetta 2 emulation on Apple Silicon (M1/M2/M3/M4) Macs.

To ensure seamless compatibility across all architectures:
- The default `docker-compose.yml` uses **`mcr.microsoft.com/azure-sql-edge:latest`**, which provides full, native multi-arch support (ARM64 and AMD64) and uses fewer system resources while preserving T-SQL and EF Core compatibility.
- For pure x86_64 Intel/AMD Linux/Windows production servers, the image can be swapped to `mcr.microsoft.com/mssql/server:2022-latest` in `.env` without modifying application code.

---

## 3. Dependency Readiness & Health Checks

SQL Server takes 10–20 seconds to initialize internal master databases upon startup. To prevent the API container from crashing with transient connection errors, `docker-compose.yml` configures an active SQL query healthcheck:

```yaml
healthcheck:
  test: ["CMD-SHELL", "/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P \"${MSSQL_SA_PASSWORD}\" -Q \"SELECT 1\" || exit 1"]
  interval: 10s
  timeout: 5s
  retries: 5
  start_period: 15s
```

The API service defines:
```yaml
depends_on:
  sqlserver:
    condition: service_healthy
```
This guarantees the API will not boot until SQL Server is actively accepting connections.

---

## 4. Lifecycle Commands

### Start All Services in Background
```bash
docker compose up --build -d
```

### Stream Live Logs
```bash
docker compose logs -f api
```

### Stop Services (Preserving Database Volume)
```bash
docker compose down
```

### Destroy Database Volume (Reset Database to Seed State)
```bash
docker compose down -v
```
