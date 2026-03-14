# Bags-Shop-API

ASP.NET Core **.NET 10** Web API for managing bags, collections, discounts, orders, and payments with CQRS, background jobs, and integrated image and payment services.

> **Freelance Project** — Built for a real client and actively serving production traffic.
>
> - 🛍️ **Client Store**: [https://ziko-store.vercel.app/](https://ziko-store.vercel.app/)
> - 📸 **Client Instagram**: [https://www.instagram.com/zyko_alkomdyan](https://www.instagram.com/zyko_alkomdyan)

- **Live Swagger API**: [http://bags-shop.runasp.net/swagger](http://bags-shop.runasp.net/swagger)
- **Live Store Frontend**: [https://ziko-store.vercel.app/](https://ziko-store.vercel.app/)

---

## 1. Overview & Features

- **Modern .NET 10 stack**
  - ASP.NET Core Web API targeting `net10.0`
  - OpenAPI/Swagger documentation and UI
- **CQRS with MediatR**
  - Separation of commands and queries using MediatR handlers
  - Pipeline behaviors for caching and cache invalidation
- **Data access with EF Core**
  - SQL Server via Entity Framework Core 10
  - Repository pattern (`IMainRepository<T>`, `MainRepository<T>`)
  - Unit of Work abstraction for transactional consistency
- **Background jobs with Hangfire**
  - Hangfire + SQL Server storage
  - Scheduled discount jobs and background processing
- **Image handling with Cloudinary**
  - Image upload, storage, and management using Cloudinary
  - Dedicated image services for validation and upload orchestration
- **Payments & PayMob integration**
  - Payment service layer for handling charges and callbacks
  - PayMob integration and payment webhooks
- **Cross-cutting concerns**
  - Global exception handling middleware
  - API key authentication middleware
  - CORS configuration for frontend integration
  - Centralized logging via console logging

---

## 2. AI Assistance Disclaimer

- **Primary author**: All architecture, domain modeling, and final design decisions for Bags-Shop-API were made by me.
- **Role of AI**:
  - AI was used **only as a coding assistant** to speed up tasks such as drafting boilerplate, suggesting validation rules (especially for **image validation**), refining error handling, and improving documentation.
  - All AI-generated suggestions were **reviewed, adapted, or rewritten** by the developer before being included.
- **Ownership**: I retain full responsibility for the project’s logic, design, and behavior.

---

## 3. Getting Started

### Prerequisites

- **.NET SDK**: `.NET 10` SDK (matching `net10.0` in the project file)
- **Database**: SQL Server instance
- **Tools (optional)**:
  - `dotnet-ef` CLI for running migrations

### Installation & Setup

From the project root (where the `.sln` and `.csproj` reside):

```bash
dotnet restore
```

Apply Entity Framework Core migrations (if migrations are present in the `Migrations` folder):

```bash
dotnet ef database update
```

Run the API:

```bash
dotnet run --project Bags-Shop-API/Bags-Shop-API.csproj
```

By default, Swagger UI and OpenAPI should be available at the configured URL (e.g. `/swagger` or via `MapOpenApi`).

---

## 4. High-Level Project Structure

```text
Bags-Shop-API/
├─ Bags-Shop-API.sln
├─ Bags-Shop-API/
│  ├─ Program.cs
│  ├─ Bags-Shop-API.csproj
│  ├─ Controllers/
│  ├─ Services/
│  │  ├─ ProductServices/
│  │  ├─ ImageServices/
│  │  ├─ DiscountServices/
│  │  ├─ CollectionServices/
│  │  ├─ OrderServices/
│  │  ├─ PaymentServices/
│  │  └─ Shared/
│  ├─ Repo/
│  ├─ UnitOfWorkService/
│  ├─ Specification/
│  ├─ ContextFile/
│  ├─ Middleware/
│  ├─ Migrations/
│  ├─ Models/
│  └─ Email.cs
```

- **`Program.cs`**: Application entry point and dependency injection configuration.
- **`Controllers/`**: HTTP endpoints for bags, collections, discounts, orders, payments, etc.
- **`Services/`**: Domain/application logic (products, images, discounts, collections, orders, payments, accounts, shared utilities).
- **`Repo/`**: Generic repository implementations for EF Core.
- **`UnitOfWorkService/`**: Unit of Work pattern for managing transactions.
- **`Specification/`**: Specifications for complex querying and filtering.
- **`ContextFile/`**: EF Core `DbContext` and database configuration.
- **`Middleware/`**: Global exception handling, API key auth, and other middleware components.
- **`Migrations/`**: EF Core migration files.

---

## 5. Configuration & Environments

- **Configuration files**
  - `appsettings.json`: Base configuration (logging, connection strings, external services).
  - `appsettings.Development.json`: Development overrides.
- **Environment selection**
  - Use the `ASPNETCORE_ENVIRONMENT` variable to select environment:
    - `Development`
    - `Production`
- **Connection strings & providers**
  - SQL Server connection strings (e.g. `Monster`, `LocalSql`) should be provided via configuration.
  - Hangfire storage uses a SQL Server connection string from configuration.
- **Secrets & sensitive data**
  - Cloudinary credentials (`CloudName`, `ApiKey`, `ApiSecret`)
  - PayMob/payment keys
  - API keys and other secrets
  - **Recommended**: Do **not** commit secrets to source control.
    - Use **user secrets** in development (e.g. `dotnet user-secrets`).
    - Use environment variables, Azure Key Vault, or similar secret stores in production.

---

## 6. Skills Showcased

- **Backend & .NET**
  - ASP.NET Core Web API targeting `.NET 10`
  - Dependency Injection and configuration management
- **Architecture & Patterns**
  - CQRS with MediatR
  - Repository pattern and Unit of Work
  - Specification pattern for querying
  - Middleware for cross-cutting concerns
- **Data & Infrastructure**
  - Entity Framework Core 10 with SQL Server
  - Database migrations and schema management
- **Background Processing**
  - Hangfire with SQL Server storage
  - Scheduled jobs (e.g. discount scheduling and cleanup)
- **Integration & Services**
  - Cloudinary for image uploads and management
  - PayMob (and payment webhooks) for payment processing
  - Email and notification services
- **Quality & Robustness**
  - Centralized error handling
  - Validation layers (including AI-assisted image validation rules)
  - CORS and API key authentication

This is a live, production Web API built for a real client, showcasing modern .NET patterns, integrations, and API design.
