# Leave Management System

Production-style leave management application built with ASP.NET Core MVC (.NET 8), SQL Server, and a layered architecture.

## Overview

This project helps organizations manage employee leave end-to-end:
- Employee leave application and tracking
- Manager/Admin approval workflow
- Leave balance and policy enforcement
- Dashboard analytics and reports
- Audit/history visibility

The repository contains:
- `LeaveManagementSystem/` - main web application
- `LeaveManagementSystem.Tests/` - xUnit test project

## Tech Stack

- ASP.NET Core MVC (.NET 8)
- C#
- Entity Framework Core 8
- SQL Server + Stored Procedures
- ASP.NET Core Identity (role-based auth)
- AutoMapper
- FluentValidation
- Serilog
- Hangfire (background jobs)
- JWT (API auth)
- EPPlus (Excel export)
- QuestPDF (PDF export)
- Razor Views + AdminLTE + Bootstrap + jQuery + DataTables + Chart.js

## Architecture

Layered design with clean separation:
- `Controllers` - HTTP endpoints (thin orchestration only)
- `Services` - business logic and rules
- `Repositories` - data access (EF + stored procedures)
- `Interfaces` - contracts for DI and testability
- `ViewModels` / `DTOs` - UI/API models
- `Middleware` - global exception handling, correlation id

## Key Features

- Leave apply, approve, reject, cancel
- Multi-role dashboards (Admin, Manager, Employee)
- Multi-level approval support (Manager -> Admin)
- Half-day leave support and overlap prevention
- Sandwich-rule style weekend counting
- Notification pipeline (Hangfire-backed async email jobs)
- Report filters + summary chart + Excel/PDF export
- API endpoints with JWT
- Caching (leave types, holidays, dashboard data)
- Structured logging and correlation IDs

## Project Structure

```text
LeaveManagementSystem/
  Controllers/
  Data/
  Database/
  DTOs/
  Helpers/
  Interfaces/
  Mappings/
  Middleware/
  Models/
  Options/
  Repositories/
  Services/
  ViewModels/
  Views/
  wwwroot/
LeaveManagementSystem.Tests/
```

## Prerequisites

- .NET SDK 8.x
- SQL Server (Express or full)
- Visual Studio / Rider / VS Code

## Setup

1. Clone repository:
   ```bash
   git clone https://github.com/jassi2244/LeaveManagementSystem.git
   cd LeaveManagementSystem
   ```

2. Update connection strings:
   - `LeaveManagementSystem/appsettings.json`
   - `LeaveManagementSystem/appsettings.Development.json`

3. Ensure database objects exist:
   - Run `LeaveManagementSystem/Database/LeaveManagementDB_Full.sql`
   - Or use EF migration flow if you prefer.

4. Configure SMTP (required for email jobs):
   - Update `Smtp` section with real values.

5. Configure JWT secrets:
   - Update `Jwt:SecretKey` and related values.

6. Run app:
   ```bash
   cd LeaveManagementSystem
   dotnet run
   ```

## Default Roles and Access

The app uses roles:
- Admin
- Manager
- Employee

Seeded/demo users may be created during startup in development mode (see `Program.cs`).

## API Endpoints (JWT)

Base path: `/api`

- `POST /api/auth/token`
- `POST /api/leave/apply`
- `POST /api/leave/{leaveRequestId}/approve`
- `POST /api/leave/{leaveRequestId}/reject`
- `GET /api/leave/balance`

## Background Jobs

Hangfire dashboard:
- `/hangfire` (Admin only)

Configured jobs:
- Daily leave summary
- Yearly leave balance reset

## Testing

Run tests:
```bash
cd LeaveManagementSystem.Tests
dotnet test
```

## Notes

- This project includes both cookie auth (MVC) and JWT auth (API).
- Avoid committing real SMTP passwords and production secrets in plain text.
- Use environment variables or secret storage for production deployments.

## License

This repository currently has no explicit license file.
