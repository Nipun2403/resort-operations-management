# Architecture and Flow

## 1. Executive Summary
The Hotel Management System backend is a high-performance monolithic Web API developed using **ASP.NET Core 10**. The system implements an **N-Tier Architecture** combined with **Domain-Driven Design (DDD)** principles to ensure a strict separation of concerns, high maintainability, and robust testability.

The architectural flow is unidirectional:
`Client` → `Presentation Layer (API)` → `Business Logic Layer (BLL)` → `Repository Layer` → `Data Access Layer (DAL)` → `PostgreSQL Database`.

---

## 2. N-Tier Architecture Breakdown

The solution is divided into distinct class libraries (layers) to enforce separation of concerns, orchestrated entirely via the Dependency Injection (DI) container in `HotelManagement.API/Program.cs`.

```mermaid
flowchart TD
    subgraph Presentation ["HotelManagement.API (Presentation Layer)"]
        Controllers["API Controllers"]
        Middlewares["GlobalExceptionMiddleware"]
        Hubs["SignalR NotificationHub"]
    end

    subgraph Business ["HotelManagement.BLL (Business Logic Layer)"]
        Services["Domain Services (IBookingService)"]
        DTOs["Data Transfer Objects (DTOs)"]
        AutoMapper["AutoMapper Profiles"]
    end

    subgraph Repository ["HotelManagement.Repository (Repository Layer)"]
        GenericRepo["GenericRepository<T>"]
        SpecificRepos["Specific Repositories (IRoomRepository)"]
    end

    subgraph DataAccess ["HotelManagement.DAL (Data Access Layer)"]
        DbContext["ApplicationDbContext"]
        Entities["EF Core Entities"]
    end

    DB[(PostgreSQL Database)]

    Client([Web / Mobile Client]) -->|HTTP JSON| Controllers
    Client -->|WebSocket| Hubs
    
    Controllers -->|Injects Interface| Services
    Services -->|AutoMapper Maps| DTOs
    Services -->|Injects Interface| SpecificRepos
    SpecificRepos -->|Inherits| GenericRepo
    SpecificRepos -->|Uses| DbContext
    DbContext -->|EF Core LINQ| Entities
    Entities -->|PostgreSQL Driver| DB
```

---

*This document will be continuously enriched with detailed code references, line numbers, and additional diagrams as we parse each backend source file.*
