# ACXIOM

ACXIOM is a customer relationship management project built with a .NET 8 backend and a React + TypeScript frontend. The solution is structured to keep business logic, infrastructure, and API concerns separated, while providing a modern, responsive CRM interface for sales and management workflows.

## Overview

This repository contains two main application sections:

- `AcxiomCRM/` — ASP.NET Core 8 backend following Clean Architecture
- `react-crm/` — React frontend built with Vite, TypeScript, and React Router

The project is intended to support CRM-style operations such as customer management, leads, sales tracking, role-based access, and API-driven frontend integration.

## Tech Stack

### Backend
- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- Docker support
- Clean Architecture and layered project structure

### Frontend
- React 19
- TypeScript
- Vite
- React Router
- Axios
- Lucide React

## Repository Structure

```text
ACXIOM/
├── AcxiomCRM/                     # Backend solution and API
│   ├── src/
│   │   ├── AcxiomCRM.Api/
│   │   ├── AcxiomCRM.Application/
│   │   ├── AcxiomCRM.Domain/
│   │   └── AcxiomCRM.Infrastructure/
│   ├── tests/
│   ├── docs/
│   ├── Dockerfile
│   ├── docker-compose.yml
│   ├── .env.example
│   ├── AcxiomCRM.sln
│   └── README.md
├── react-crm/                    # Frontend app
│   ├── src/
│   ├── public/
│   ├── package.json
│   ├── vite.config.ts
│   ├── tsconfig.json
│   └── README.md
├── .gitignore
└── README.md                     # This file
```

## Backend Architecture

The backend is organized into the standard layers used in Clean Architecture:

- `AcxiomCRM.Domain` — business entities, rules, and core models
- `AcxiomCRM.Application` — application services, DTOs, interfaces, and validation logic
- `AcxiomCRM.Infrastructure` — EF Core, data access, identity configuration, and external integrations
- `AcxiomCRM.Api` — controllers, API wiring, middleware, and Swagger setup

## Frontend Architecture

The frontend app uses a modern React setup with Vite and TypeScript. It is designed to communicate with the backend API and render CRM screens and workflows in a responsive single-page application.

## Prerequisites

Before running the project, ensure you have the following:

- .NET 8 SDK
- Node.js 18+ and npm
- SQL Server or Docker Desktop
- Optional: Docker Compose

## Getting Started

### 1. Backend Setup

From the repository root:

```bash
cd AcxiomCRM

dotnet restore
dotnet build
dotnet run --project src/AcxiomCRM.Api
```

The API is typically served on a local ASP.NET Core port and exposes Swagger for testing and exploration.

### 2. Frontend Setup

From the repository root:

```bash
cd react-crm
npm install
npm run dev
```

This starts the Vite development server for the React CRM application.

### 3. Docker Setup

The backend includes Docker configuration for quick local setup:

```bash
cd AcxiomCRM
docker compose up --build -d
```

For more details, refer to the backend README in `AcxiomCRM/README.md`.

## Configuration

The backend includes environment configuration files such as `.env.example` and may require database connection values and app settings to be configured before startup. The React app also contains a `.env` file for frontend environment configuration.

## Features

This project is structured around a typical CRM use case and includes support for:

- Customer and lead management
- Sales / opportunity workflows
- Authentication and role-based authorization
- Clean separation between API, domain, and data layers
- Swagger-based API documentation
- Docker-based local development support

## Documentation

Detailed backend and architecture documentation is available in:

- `AcxiomCRM/README.md`
- `AcxiomCRM/docs/`

## License

This project currently does not include a license file. If you plan to distribute or reuse the project publicly, consider adding an appropriate license.

## Contributing

Contributions are welcome. If you want to improve features, fix bugs, or refactor the project, open a pull request with a clear description of the change.

## Contact

For questions or collaboration, please use the repository owner and the GitHub project page associated with this repo.
