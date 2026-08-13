# AETHERIS — Luxury Resort Hotel Management System

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-10.0-512BD4?logo=asp.net-core&logoColor=white)](https://dotnet.microsoft.com/apps/aspnet)
[![Angular](https://img.shields.io/badge/Angular-22.0-DD0031?logo=angular&logoColor=white)](https://angular.dev/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-9.0-512BD4?logo=entity-framework&logoColor=white)](https://learn.microsoft.com/en-us/ef/core/)
[![Azure Blob Storage](https://img.shields.io/badge/Azure%20Blob-Storage-0078D4?logo=microsoft-azure&logoColor=white)](https://azure.microsoft.com/en-us/products/storage/blobs)
[![SignalR](https://img.shields.io/badge/SignalR-Real--time-512BD4?logo=microsoft&logoColor=white)](https://dotnet.microsoft.com/apps/aspnet/signalr)
[![OpenAI](https://img.shields.io/badge/OpenAI-gpt--4o--mini-412991?logo=openai&logoColor=white)](https://openai.com/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)
[![NUnit](https://img.shields.io/badge/NUnit-Tested-22A04C?logo=nunit&logoColor=white)](https://nunit.org/)
[![Vitest](https://img.shields.io/badge/Vitest-Tested-6E9F18?logo=vitest&logoColor=white)](https://vitest.dev/)
[![Serilog](https://img.shields.io/badge/Serilog-Logging-4A4A4A?logo=serilog&logoColor=white)](https://serilog.net/)
[![AutoMapper](https://img.shields.io/badge/AutoMapper-Enabled-4A4A4A?logo=automapper&logoColor=white)](https://automapper.org/)
[![OpenTelemetry](https://img.shields.io/badge/OpenTelemetry-Metrics-0A74DA?logo=opentelemetry&logoColor=white)](https://opentelemetry.io/)


---

## Table of Contents

- [Overview](#overview)
- [Hall of Fame — Showstopper Features](#hall-of-fame--showstopper-features)
- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Screenshots](#screenshots)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [API Documentation](#api-documentation)
- [Deployment](#deployment)
- [Testing](#testing)
- [Contributing](#contributing)
- [License](#license)

---

## Overview

**AETHERIS** is a complete hotel management ecosystem designed for **luxury resorts serving the top 0.0001% wealth bracket**. It handles every operational aspect — from guest bookings and billing to housekeeping, maintenance, room service, and AI-powered concierge services — with real-time coordination across all departments.

### Key Metrics

| Metric | Value |
|--------|-------|
| **Backend Lines of Code** | ~15,000+ |
| **Frontend Lines of Code** | ~12,000+ |
| **Total Symbols (GitNexus)** | ~10,000 |
| **Execution Flows** | 300+ |
| **Files** | 801 |
| **Database Migrations** | 47 |
| **Background Workers** | 5 |
| **User Roles** | 6 |
| **Protected Controllers** | 16 |
| **Angular Route Guards** | 7 |
| **Unit Test Coverage (BLL)** | 93% |
| **NUnit Tests** | 29 |

---

## Hall of Fame — Showstopper Features

<details>
<summary><strong>🏆 1. Async Image Upload Pipeline with Magic Byte Validation</strong> (Click to expand)</summary>

A zero-trust, production-grade image pipeline using **Azure Blob Storage + Azure Queue + SAS tokens + 3 background workers**.

### Core Highlight: Magic Byte Validation

Unlike extension-only checks (which a renamed `.exe` passes), this pipeline reads the actual binary content of every uploaded file and validates its **magic bytes** — the unique file-type signatures embedded in the first few bytes of every valid image.

| Format | Magic Bytes (Hex) | Checked |
|--------|------------------|---------|
| JPEG | `FF D8 FF` | ✅ First 3 bytes |
| PNG | `89 50 4E 47 0D 0A 1A 0A` | ✅ Full 8-byte signature |
| WebP | `52 49 46 46 .... 57 45 42 50` | ✅ RIFF container + WEBP marker |

Files that fail magic byte validation are **automatically deleted** from Azure Blob Storage with a rejection reason recorded in the database.

### Defense-in-Depth Layers (9 Layers)

| Layer | What It Prevents | Where |
|-------|-----------------|-------|
| MIME accept attribute | Browser-only hint (easily bypassed) | Frontend `<input accept>` |
| Extension whitelist | `.jpg`/`.jpeg`/`.png`/`.webp` only | `AzureStorageOptions.cs:14` |
| Size limit | Max 10MB declared | `AzureStorageOptions.cs:12` |
| SAS URL expiry | 15-minute window | `ImageUploadService.cs` |
| SAS permission scope | Write+Create only (no list/delete) | `ImageUploadService.cs` |
| Ownership check | Only uploader can confirm | `ImageUploadService.cs` |
| **Magic byte validation** | **Content-level check (renamed `.exe` detected)** | **`ImageValidationWorker.cs:133-146`** |
| Post-upload size verification | Actual size matches declared | `ImageValidationWorker.cs:98-104` |
| Orphan blob cleanup | No storage leaks | `BlobCleanupWorker.cs` |
| Expired session cleanup | No DB bloat | `OrphanImageCleanupWorker.cs` |

### Pipeline Flow

```mermaid
flowchart TD
    A["📤 User selects image"] --> B["🖥️ Frontend: ImageUploadOrUrlComponent"]
    B --> C{"Mode?"}
    C -->|"URL"| D["Validate URL format<br/>Show preview"]
    C -->|"Upload"| E["📡 POST /images/upload-sas"]
    E --> F["🔐 Backend: RequestUploadAsync"]
    F --> G{"Validation Gate 1"}
    G -->|"❌ Extension not in whitelist"| H["Reject: Invalid extension"]
    G -->|"❌ Size exceeds 10MB"| I["Reject: Too large"]
    G -->|"❌ RoomType has 5+ images"| J["Reject: Max images reached"]
    G -->|"✅ Passes validation"| K["Generate SAS URL<br/>(Write+Create, 15min expiry)<br/>Create UploadSession (Status: Pending)"]
    K --> L["🔗 SAS URL returned to frontend"]
    L --> M["⬆️ Frontend uploads directly<br/>to Azure Blob Storage"]
    M --> N["📡 POST /images/{sessionId}/confirm"]
    N --> O["✅ ConfirmUpload<br/>Validates ownership<br/>Enqueues to Azure Queue"]
    O --> P["⚙️ ImageValidationWorker<br/>(Background Service, polls every 2s)"]
    P --> Q{"Validation Gate 2"}
    Q -->|"📏 Actual size vs declared"| R["Size mismatch check"]
    Q -->|"🔍 Magic Byte Validation"| S["Read first 512 bytes from blob"]
    S --> T{"Content Check"}
    T -->|"✅ JPEG: starts with FF D8 FF"| U["Status: Confirmed"]
    T -->|"✅ PNG: matches 8-byte PNG signature"| U
    T -->|"✅ WebP: RIFF....WEBP pattern"| U
    T -->|"❌ No valid magic bytes"| V["Status: Rejected<br/>Delete blob from storage<br/>Record rejection reason"]
    U --> W["Image ready for entity attachment"]
    V --> X["❌ Blob deleted,<br/>frontend shows rejection reason"]
    Y["⏰ OrphanImageCleanupWorker<br/>(Runs hourly)"] --> Z["Expires Pending sessions >1h<br/>Expires Confirmed/Rejected >24h<br/>with no entity attached<br/>→ Deletes orphaned blobs"]
    AA["⏰ BlobCleanupWorker<br/>(Runs hourly)"] --> AB["Full container scan:<br/>All blobs vs active DB references<br/>→ Deletes unreferenced blobs"]
```

</details>

<details>
<summary><strong>🏆 2. AI Concierge — Conversational Guest Assistant with Real Action Execution</strong> (Click to expand)</summary>

A guest-facing AI concierge that doesn't just answer questions — it **executes real hotel operations** via existing BLL services. Guests chat naturally; the AI places room-service orders, creates housekeeping/maintenance tickets, retrieves billing/booking info — all scoped to the authenticated guest's active booking. Staff receive instant real-time SignalR alerts.

**Jaw-dropping demo**: Guest types one message → three departments (Kitchen, Housekeeping, Maintenance) instantly receive real-time SignalR alerts → AI replies with confirmation — all visible on projected staff dashboards.

### Core Highlight: Two-Step Action Pattern with LLM Tool Calling

Unlike chatbots that only reply, this concierge uses **OpenAI function calling** with a strict two-step pattern:

1. **Propose** — LLM calls side-effect tools (`create_food_order`, `create_housekeeping_request`, `create_maintenance_ticket`) → system creates pending proposals with 5-minute TTL
2. **Confirm** — Guest confirms in chat → proposals executed via existing BLL services → real-time SignalR alerts fire to Kitchen/Housekeeping/Maintenance staff

Read-only tools (`get_booking_info`, `get_folio_balance`, `get_menu_items`, etc.) execute immediately without confirmation.

### Architecture & Flow

```mermaid
flowchart TD
    subgraph "🔵 Guest Portal (Angular)"
        A["💬 ConciergeChatComponent<br/>SignalR + LocalStorage persistence"]
    end

    subgraph "🟢 API Layer (ASP.NET Core)"
        B["POST /api/v1/concierge/chat<br/>[Authorize(Roles=RegisteredUser)]<br/>[Idempotent] + InputSanitizer"]
        C["POST /api/v1/concierge/confirm<br/>[Idempotent]"]
        D["GET /api/v1/concierge/context<br/>GuestContextDTO"]
    end

    subgraph "🟡 BLL — Concierge Orchestrator"
        E["IConciergeService<br/>ProcessMessageAsync / ConfirmProposalsAsync"]
        F["PromptBuilder<br/>System prompt + guest context + tool defs"]
        G["ToolExecutor<br/>Centralized dispatch to 8 tools"]
        H["InputSanitizer<br/>Regex strip: ignore previous, system:, assistant:"]
    end

    subgraph "🟠 LLM & Tools (OpenAI gpt-4o-mini)"
        I["ChatClient + 8 Function Tools"]
        I -->|Side-Effect| J["create_food_order"]
        I -->|Side-Effect| K["create_housekeeping_request"]
        I -->|Side-Effect| L["create_maintenance_ticket"]
        I -->|Read-Only| M["get_booking_info"]
        I -->|Read-Only| N["get_folio_balance"]
        I -->|Read-Only| O["get_housekeeping_status"]
        I -->|Read-Only| P["get_menu_items"]
        I -->|Read-Only| Q["get_active_orders"]
    end

    subgraph "🔴 Existing BLL Services (Reused — Zero New Business Logic)"
        R["OrderService.CreateOrderAsync<br/>→ SignalR KitchenGroup alert"]
        S["HousekeepingService.CreateGuestTriggerAsync<br/>→ SignalR HousekeepingGroup alert"]
        T["MaintenanceService.CreateTicketAsync<br/>→ SignalR MaintenanceGroup alert"]
        U["BookingService / BillingService / MenuItemRepo"]
    end

    subgraph "🟣 Persistence & Audit"
        V["ConversationMessage table<br/>Token-window: last 8 turns"]
        W["ConciergeProposal table<br/>TTL 5 min, status: pending/confirmed/expired"]
        X["ConciergeActionLog table<br/>Every tool call + outcome"]
    end

    A --> B
    B --> E
    E --> F
    F --> I
    G --> R
    G --> S
    G --> T
    G --> U
    E --> V
    E --> W
    E --> X
    R --> A
    S --> A
    T --> A
```

### Tool Catalog (8 Tools — 3 Side-Effect, 5 Read-Only)

| Tool | Type | Description | Confirmation Required | Real-Time Alert To |
|------|------|-------------|----------------------|-------------------|
| `create_food_order` | Side-Effect | Place room-service order (items + quantities) | ✅ Yes | **KitchenGroup** (SignalR) |
| `create_housekeeping_request` | Side-Effect | Extra towels, cleaning, amenities, emergency | ✅ Yes | **HousekeepingGroup** (SignalR) |
| `create_maintenance_ticket` | Side-Effect | Report issue (AC, plumbing, TV), emergency flag | ✅ Yes | **MaintenanceGroup** (SignalR) |
| `get_booking_info` | Read-Only | Check-in/out, room number, stay dates, status | ❌ No | — |
| `get_folio_balance` | Read-Only | Current bill total, payment status, nights stayed | ❌ No | — |
| `get_housekeeping_status` | Read-Only | Room cleaning status, pending requests | ❌ No | — |
| `get_menu_items` | Read-Only | Browse menu (category, search, availability filter) | ❌ No | — |
| `get_active_orders` | Read-Only | Guest's pending/delivered room-service orders | ❌ No | — |

### Defense-in-Depth Security Layers

| Layer | What It Prevents | Where |
|-------|-----------------|-------|
| JWT + Role guard | Only RegisteredUser guests can access | `ConciergeController.cs:14` |
| Idempotency per turn | Duplicate LLM calls / double-execution | `ConciergeApiService.ts:79-84`, `IdempotentAttribute` |
| Input sanitization | Prompt injection (ignore previous, system:, assistant:) | `InputSanitizer.cs:5-26`, `ConciergeController.cs:36` |
| Guest context scoping | Guest A cannot access Guest B's booking/room | `BuildGuestContextAsync` resolves from `ICurrentUserService` |
| No context leakage to LLM | bookingId, roomId, userId never in tool args | `PromptBuilder.cs:991` — "NEVER include bookingId, roomId" |
| Proposal TTL (5 min) | Stale proposals auto-expire | `ConciergeProposalDTO.ExpiresAt`, `ConciergeProposalRepository` |
| Audit log (own table) | Every tool call + outcome traceable | `ConciergeActionLog.cs`, `ConciergeActionLogRepository.cs` |
| Conversation isolation | `concierge:conv:{userId}:{conversationId}` key | `PostgresConversationStore.cs:1053-1061` |
| Max 5 tool calls/turn | Prevent runaway LLM loops | `ConciergeTools.MaxToolCallsPerTurn` |

</details>

<details>
<summary><strong>🏆 3. Automated Audit Logging — ORM-Level Change Data Capture</strong> (Click to expand)</summary>

Every entity change across the entire system is **automatically captured** with old/new values — no manual audit code, no annotations, no developer effort.

### Core Highlight: Zero-Touch JSONB Change Capture

The `SaveChangesAsync()` override in `ApplicationDbContext` intercepts every entity state change (Added, Modified, Deleted) before it hits the database. It captures:

- **Old values** — the complete entity state before the change (for updates/deletes)
- **New values** — the complete entity state after the change (for adds/updates)
- **Primary key** — as a JSONB document (handles composite keys)
- **Who changed it** — automatically resolved from `IHttpContextAccessor` via `CurrentUserService`
- **When** — precise timestamp with offset
- **What entity** — entity name + action type

These are stored as **PostgreSQL JSONB columns** — a native binary JSON format with indexing support and no injection risk.

### Data Capture Flow

```mermaid
flowchart TD
    A["👤 User performs action"] --> B["Controller action executes"]
    B --> C["📦 BLL Service method"]
    C --> D["🔄 DbContext.SaveChangesAsync()"]
    D --> E{"Override interceptor"}
    E -->|"Entry.State == Added"| F["Capture: EntityName, Action='Added'<br/>Capture: NewValues (all properties)<br/>Capture: PrimaryKey (temporary → final)"]
    E -->|"Entry.State == Modified"| G["Capture: EntityName, Action='Modified'<br/>Capture: OldValues (original values)<br/>Capture: NewValues (current values)"]
    E -->|"Entry.State == Deleted"| H["Capture: EntityName, Action='Deleted'<br/>Capture: OldValues (values before delete)"]
    F --> I["✏️ Enrich: ChangedByEmail, ChangedByName<br/>(resolved from HttpContext.User claims)"]
    G --> I
    H --> I
    I --> J["🗄️ INSERT into AuditLogs table<br/>PrimaryKey → jsonb<br/>OldValues → jsonb<br/>NewValues → jsonb"]
    J --> K["Base SaveChanges continues<br/>with real database operation"]
    L["👑 Admin user"] --> M["📡 GET /api/v1/auditlogs"]
    M --> N["AuditLogService<br/>Pagination • Search by name/email • Sort • Filter"]
    N --> O["🖥️ Frontend: Admin Audit Log Viewer"]
    O --> P["📋 Sortable columns<br/>🔍 Search by user/email<br/>📄 Detail dialog with old/new JSON diff"]
```

### AuditLog Schema

```
┌─────────────────────────────────────────────┐
│                  AuditLog                     │
├─────────────────────────────────────────────┤
│ Id             │ GUID (PK)                    │
│ EntityName     │ string ("Booking", "User")   │
│ Action         │ string ("Added"/"Modified"/  │
│                │          "Deleted")          │
│ PrimaryKey     │ jsonb {"Id": "..."}          │
│ OldValues      │ jsonb {"Email": "old@..."}   │
│ NewValues      │ jsonb {"Email": "new@..."}   │
│ ChangedByEmail │ string ("admin@hotel.com")   │
│ ChangedByName  │ string ("John Admin")        │
│ Timestamp      │ DateTimeOffset               │
└─────────────────────────────────────────────┘
```

### What Gets Audited (Every Entity)

All 18 entity types are automatically tracked — every create, update, and delete across the entire system, including `Booking`, `User`, `Room`, `Receipt`, `AuditLog` entries themselves (yes, audit is self-auditing), `Housekeeping`, `MaintenanceTask`, `FoodOrder`, `Feedback`, etc.

**Benefits:**
- **Complete change history** — every modification to every record is recoverable
- **Forensic traceability** — know exactly who changed what, when, and the before/after values
- **Billing dispute resolution** — see exactly when and by whom a charge was entered or modified
- **Compliance-ready** — meet audit trail requirements for hospitality data
- **Zero developer effort** — no `[Audit]` attributes, no manual logging calls, no decorators. It just works.

</details>

<details>
<summary><strong>🏆 4. End-to-End Idempotency System</strong> (Click to expand)</summary>

Network retries, browser double-clicks, and payment gateway duplicates **never result in double-processing**. The system transparently detects and rejects duplicates by replaying the original response.

### Core Highlight: Transparent Response Replay

Every mutation request (POST/PUT/PATCH) carries a unique `X-Idempotency-Key` header (UUID generated by the frontend). When a request with a previously-seen key arrives, the system **does not re-execute the handler** — instead it replays the **exact original response** (status code + body) from the database.

This is especially critical for:
- **Booking creation** — preventing duplicate room reservations
- **Payment processing** — preventing double charges
- **Any concurrent mutation** — the filter handles race conditions where two identical requests arrive simultaneously

### Request Flow

```mermaid
flowchart TD
    A["👤 User clicks 'Submit Booking'"] --> B["🖥️ Frontend: IdempotencyInterceptor"]
    B --> C["📝 Generates crypto.randomUUID()<br/>Sets header: X-Idempotency-Key: <uuid>"]
    C --> D["📡 HTTP POST/PUT/PATCH request"]
    D --> E["🔍 Backend: IdempotentAttribute<br/>(Global Action Filter)"]
    E --> F{"Check: Key exists in DB?"}
    F -->|"❌ New request"| G["✅ Execute controller action normally"]
    G --> H["📦 Store response in IdempotentRequest table<br/>Key: userId:uuid or anon:uuid<br/>Path, StatusCode, ResponseBody, CreatedAt"]
    H --> I["📤 Return real response to client"]
    F -->|"⚠️ Duplicate key detected"| J["📋 Fetch cached response from DB<br/>StatusCode + ResponseBody + CreatedAt"]
    J --> K["📤 Return CACHED response<br/>(same status, same body)<br/>No action re-executed"]
    L["⏰ IdempotencyCleanupService<br/>(Runs every 6 hours)"] --> M["🗑️ Deletes records older than 48 hours"]
    N["🔒 Race condition handling"] --> O["DbUpdateException on PK conflict<br/>→ silently falls through<br/>→ returns cached response<br/>(first writer wins)"]
```

### The Idempotency Contract

| Request 1 | Request 2 (Duplicate) | Result |
|-----------|----------------------|--------|
| `POST /bookings` → 201 Created | `POST /bookings` same key → 201 Created | 1 booking created, same response returned |
| `POST /billing/{id}/pay` → 200 OK | `POST /billing/{id}/pay` same key → 200 OK | 1 payment processed, receipt replayed |
| `PATCH /bookings/{id}/cancel` → 200 OK | `PATCH /bookings/{id}/cancel` same key → 200 OK | 1 cancellation, cancellation email sent once |

</details>

<details>
<summary><strong>🏆 5. Real-Time SignalR Notifications with Role-Based Broadcasting</strong> (Click to expand)</summary>

Live push events to specific staff roles — housekeeping, kitchen, and maintenance — the moment a relevant event occurs. No polling, no page refreshes.

### Core Highlight: Event-Driven Role-Based Fan-Out

When a business event occurs (guest checks out, food order placed, maintenance ticket created), the service layer calls `SignalRNotificationService` which broadcasts only to the relevant **SignalR group**. Because users are added to groups based on their role at connection time, notifications are precisely targeted:

- **Housekeeping** receives alerts for: checkout-triggered cleaning tasks, guest-requested housekeeping
- **Kitchen** receives alerts for: new food orders, order status changes
- **Maintenance** receives alerts for: new maintenance tickets, priority changes

The frontend receives these as RxJS event streams and displays them as **styled glass-panel toast notifications** with automatic stacking.

### Event Flow

```mermaid
flowchart TD
    subgraph "🔵 Event Source"
        A["👨‍💼 Front Desk checks out guest"]
        B["🧑 Guest orders room service"]
        C["🛠️ Staff creates maintenance ticket"]
    end

    subgraph "🟢 Backend Processing"
        D["BookingService.UnifiedCheckoutAsync()"]
        E["OrderService.CreateOrderAsync()"]
        F["MaintenanceService.CreateTicketAsync()"]
    end

    subgraph "🟡 Business Logic Hooks"
        A --> D
        B --> E
        C --> F
    end

    D --> G["📢 SignalRNotificationService<br/>.SendAlertAsync()"]
    E --> G
    F --> G

    G --> H{"👥 Target Group?"}

    H -->|"🛏️ HousekeepingGroup"| I["New checkout →<br/>auto-create housekeeping tasks<br/>→ alert housekeeping staff"]
    H -->|"🍳 KitchenGroup"| J["New food order →<br/>alert kitchen staff<br/>with order details"]
    H -->|"🔧 MaintenanceGroup"| K["New ticket →<br/>alert maintenance staff<br/>with priority & location"]

    subgraph "🟠 Frontend Reception"
        I --> L["📡 NotificationService<br/>(SignalR HubConnection)"]
        J --> L
        K --> L

        L --> M["RxJS Subject →<br/>ReceiveAlert$"]
        M --> N["🔔 NotificationSnackbarComponent<br/>Glass-panel toast<br/>with title, message, close"]
        M --> O["🔄 Auto-refresh<br/>relevant dashboard"]
    end
```

### Connection Architecture

```
┌─────────────────────────────────────────────────────┐
│ SignalR Connection Setup                             │
├─────────────────────────────────────────────────────┤
│ 1. User authenticates → receives JWT                 │
│ 2. Frontend connects: new HubConnection(             │
│      "/notifications?access_token={jwt}")            │
│ 3. Server: NotificationHub.OnConnectedAsync()        │
│    → Reads user role from claims                     │
│    → Groups.AddToGroupAsync(Context, role + "Group") │
│ 4. Connection: withAutomaticReconnect()              │
│    → Retries: 0s, 2s, 10s, 30s (built into SignalR) │
│ 5. Lifecycle: effect() tracks auth state             │
│    → Stop on logout → Start on login                 │
└─────────────────────────────────────────────────────┘
```

</details>

<details>
<summary><strong>🏆 6. Multi-Dimensional Analytics with PostgreSQL Stored Procedures</strong> (Click to expand)</summary>

Hotel-grade Key Performance Indicators computed via **native PostgreSQL functions** and rendered in **interactive ECharts dashboards** — all in real-time.

### Core Highlight: Database-Native Metric Computation

Instead of pulling thousands of rows into application memory and computing aggregates in C#, the analytics layer delegates computation to **4 PostgreSQL stored procedures** that run directly on the database server. This means:

- **Speed** — aggregation runs in-database, no data transfer overhead
- **Accuracy** — window functions and set operations produce precise results
- **Freshness** — every dashboard load computes current values from live data

The computed metrics include industry-standard hotel KPIs: **Occupancy Rate**, **Average Daily Rate (ADR)**, **Revenue Per Available Room (RevPAR)**, **Guest Satisfaction Index**, **Average Length of Stay**, **Cancellation Rate**, and **Housekeeping Turnaround Time**.

### KPI Metrics Breakdown

| Metric | Formula | Source | What It Tells You |
|--------|---------|--------|-------------------|
| **Occupancy Rate** | `(occupied rooms / total rooms) × 100` | Stored Procedure | How full is your hotel? |
| **ADR (Avg Daily Rate)** | `total room revenue / occupied rooms` | LINQ | Average price per sold room |
| **RevPAR** | `total room revenue / total available rooms` | Stored Procedure | Revenue efficiency metric |
| **Guest Satisfaction** | `(avg rating / 5) × 100` | Stored Procedure | How happy are guests? |
| **Avg Length of Stay** | `total nights / total bookings` | LINQ | How long do guests stay? |
| **Cancellation Rate** | `cancelled / total bookings × 100` | LINQ | How many bookings cancel? |
| **Housekeeping Turnaround** | `avg(FinishedAt - StartedAt) in minutes` | Stored Procedure | How fast are rooms turned over? |
| **Non-Room Expenditure** | Food Orders vs Amenities revenue | LINQ | What do guests spend on besides rooms? |

### Dashboard Visualizations

- **6 KPI Glass Cards** with trend indicators
- **Revenue Bar Chart** — daily revenue timeline
- **Expenditure Donut Chart** — category breakdown (Room, Food, Amenities)
- **Admin Analytics Page** — 4 chart types (Bar, Line, Radar, Pie), 3 date presets, category filter

</details>


---

## Architecture

### N-Tier Architecture with Strict Dependency Direction

```
Client ──▶ API ──▶ BLL ──▶ Repository ──▶ DAL ──▶ PostgreSQL
   │          │        │           │            │
   │      Controllers  │       GenericRepo    EF Core
   │      Middleware   │       Interfaces     DbContext
   │      Hubs        Services   Impls        Entities
   │      Filters     DTOs                    Migrations
   │      Services    Mapper                  Enums
   │      Seeder      Workers                 Constants
```

The architecture enforces a **strict unidirectional dependency rule**: API → BLL → Repository → DAL. Each layer only knows about the layer directly below it. This is enforced at the project-reference level in the `.csproj` files — you literally cannot write code that bypasses a layer.

### Layer Responsibilities

| Layer | Project | Responsibility |
|-------|---------|----------------|
| **API** | `HotelManagement.API` | HTTP entry point, controllers, middleware, SignalR hubs, filters, DI registration |
| **BLL** | `HotelManagement.BLL` | Business logic, domain services, DTOs, AutoMapper profiles, background workers |
| **Repository** | `HotelManagement.Repository` | Data access abstraction, generic + specific repositories, dynamic ordering |
| **DAL** | `HotelManagement.DAL` | EF Core DbContext, 18 entities, 8 enums, 47 migrations, audit interceptors |
| **Tests** | `HotelManagement.UnitTesting` | 29 unit tests (NUnit + Moq), 93% BLL coverage |
| **Integration** | `HotelManagement.TestingWorkspace` | E2E simulation tests (xUnit + WebApplicationFactory) |

### Key Patterns Used

| Pattern | Implementation |
|---------|---------------|
| Generic Repository | `IGenericRepository<T>` + `GenericRepository<T>` — single base for all 13 repositories |
| Unit of Work | `ApplicationDbContext.SaveChangesAsync()` — implicit via EF Core |
| DTO Pattern | 15 DTO files in `BLL/DTOs/` mapped via AutoMapper |
| Service Layer | 19 services with interfaces in `BLL/Interfaces/` and implementations in `BLL/Services/` |
| Background Workers | 5 `BackgroundService` inheritors for async processing |
| Action Filters | `IdempotentAttribute`, `SkipIdempotencyAttribute` |
| Middleware Pipeline | `ExceptionMiddleware → Serilog → CORS → RateLimit → Auth → Controller` |
| Soft Delete | `IsActive` flag on `User`, `Room`, `RoomType` — records never truly deleted |
| Concurrency Tokens | `[Timestamp]` row version on 4 entities — optimistic locking |
| Dynamic Ordering | `QueryableExtensions.OrderByDynamic<T>()` — expression trees from property name strings |

---

## Tech Stack

### Backend

| Technology | Version | Purpose |
|------------|---------|---------|
| **.NET** | 10.0 | Runtime & SDK |
| **ASP.NET Core** | 10.0 | Web API framework |
| **Entity Framework Core** | 9.0 | ORM |
| **PostgreSQL** | 16 | Primary database |
| **Npgsql** | 9.0 | PostgreSQL driver |
| **AutoMapper** | 13.0 | Object-object mapping |
| **Serilog** | 4.0 | Structured logging |
| **SignalR** | 10.0 | Real-time WebSocket communication |
| **OpenAI .NET SDK** | 2.0 | GPT-4o-mini integration |
| **Azure.Storage.Blobs** | 12.0 | Azure Blob Storage client |
| **Azure.Storage.Queues** | 12.0 | Azure Queue Storage client |
| **QuestPDF** | 2024 | PDF generation (folios, receipts) |
| **BCrypt.Net-Next** | 4.0 | Password hashing |
| **OpenTelemetry** | 1.9 | Metrics & observability |
| **Swashbuckle/Swagger** | 7.0 | API documentation |
| **NUnit** | 4.0 | Unit testing framework |
| **Moq** | 4.20 | Mocking framework |

### Frontend

| Technology | Version | Purpose |
|------------|---------|---------|
| **Angular** | 22.0 | Frontend framework |
| **Angular Material** | 22.0 | UI component library |
| **Angular CDK** | 22.0 | Component Dev Kit |
| **RxJS** | 7.8 | Reactive programming |
| **SignalR Client** | 10.0 | Real-time client |
| **ECharts** | 6.1 | Charting library |
| **ngx-echarts** | 22.0 | ECharts Angular wrapper |
| **marked** | 18.0 | Markdown rendering |
| **DOMPurify** | 3.4 | XSS sanitization |
| **TypeScript** | 6.0 | Type-safe JavaScript |
| **Vitest** | 4.0 | Unit test runner |
| **Prettier** | 3.8 | Code formatting |

### Infrastructure

| Technology | Purpose |
|------------|---------|
| **Docker** | Containerization |
| **Azure Container Apps** | Cloud hosting |
| **Azure Blob Storage** | Image storage |
| **Azure Queue Storage** | Background job queue |
| **PostgreSQL (Azure Database)** | Managed database |

---

## Screenshots

<details>
<summary><strong>🖼️ Guest Portal & AI Concierge</strong> (Click to expand)</summary>


| Feature | Preview |
|---------|---------|
| **Public Home Page** | Luxury landing with room catalogue, experiences, availability checker |
| **Room Catalogue** | Responsive grid with filtering, room details modal |
| **Booking Wizard** | 4-step stepper: Guest Details → Dates → Guests → Rooms |
| **AI Concierge Chat** | Glassmorphism chat panel with proposal cards, countdown timers, quick actions |
| **Concierge Proposals** | Side-effect action confirmations with 5-min TTL countdown rings |
| **User Dashboard** | Active booking, room service orders, billing folio, profile |
| **Room Service Menu** | Category-filtered menu with real-time availability |

<br>

![User Dashboard](/assets/user-dashboard.png)
![AI Prompt Injection Security](assets/PromptInjection_Security.png)
![Action Proposal](assets/Action_Proposal.png)
</details>

<details>
<summary><strong>🖼️ Admin Operations Dashboard</strong> (Click to expand)</summary>

| Feature | Preview |
|---------|---------|
| **Admin Dashboard** | 6 KPI glass cards (Occupancy, ADR, RevPAR, Revenue, Satisfaction, Turnaround) |
| **Analytics Page** | 4 chart types (Bar, Line, Radar, Pie), date presets, category filters |
| **Audit Logs** | Searchable, sortable table with JSON diff detail dialog |
| **Room Type Management** | Generic CRUD with image upload widget, amenities, bed config |
| **Staff Management** | Role-based user management with activation toggle |
| **Billing & Receipts** | Receipt table with PDF folio download |
| **Feedback Moderation** | Approve/reject guest feedback with rating display |

![User Dashboard](/assets/Admin_Dashboard.png)

</details>

<details>
<summary><strong>🖼️ Public Landing Page</strong> (Click to expand)</summary>

![home](assets/home-fullpage.png)
![room catalogue](assets/rooms-catalogue.png)
![experiences](assets/experiences.png)
![room detailed 1](assets/room-detail-carousel.png)
![room detailed 2](assets/Room_Detailed.png)
</details>

<details>
<summary><strong>🖼️ Architecture & Flow Diagrams</strong> (Click to expand)</summary>

```mermaid
graph TB
    subgraph "Frontend (Angular 22)"
        UI1[Public Shell<br/>Home, Rooms, Booking]
        UI2[Admin Shell<br/>Dashboard, Management, Oversight]
        UI3[Operations Shells<br/>FrontDesk, Kitchen, Housekeeping, Maintenance]
        UI4[User Shell<br/>Dashboard, Bookings, Concierge, Room Service]
        UI5[Concierge Chat<br/>SignalR + LocalStorage]
    end

    subgraph "Backend (ASP.NET Core 10)"
        API[API Controllers<br/>16 controllers]
        MW[Middleware Pipeline<br/>Exception, Serilog, CORS, RateLimit, Auth]
        HUB[SignalR Hub<br/>NotificationHub]
        BL[BLL Services<br/>19 domain services]
        BG[Background Workers<br/>5 workers]
    end

    subgraph "Data Layer"
        DB[(PostgreSQL 16<br/>47 migrations)]
        REPO[Repository Layer<br/>13 repositories]
        DAL[DAL / EF Core<br/>18 entities]
    end

    subgraph "External Services"
        AZB[Azure Blob Storage]
        AZQ[Azure Queue Storage]
        OAI[OpenAI API]
        EMAIL[Email Service]
    end

    UI1 --> API
    UI2 --> API
    UI3 --> API
    UI4 --> API
    UI5 --> API
    UI5 --> HUB
    UI3 --> HUB
    UI2 --> HUB
    API --> MW
    MW --> BL
    BL --> REPO
    REPO --> DAL
    DAL --> DB
    BL --> AZB
    BL --> AZQ
    BL --> OAI
    BL --> EMAIL
    BG --> AZB
    BG --> AZQ
    BG --> DB
```

</details>

---

## Getting Started

### Prerequisites

- **.NET 10.0 SDK** or later
- **Node.js 20+** and **npm 10+**
- **PostgreSQL 16+** (local or Azure Database for PostgreSQL)
- **Azure Storage Account** (for image upload pipeline)
- **OpenAI API Key** (for AI Concierge)

### Backend Setup

```bash
cd Backend/HotelManagement.API

# Restore dependencies
dotnet restore

# Configure appsettings.json with your connection strings:
# - DefaultConnection (PostgreSQL)
# - Jwt:Key, Jwt:Issuer, Jwt:Audience
# - AzureStorage:AccountUrl, AccountKey, ContainerName, QueueName
# - OpenAI:ApiKey, Model, Endpoint (optional for Azure OpenAI)

# Run EF Core migrations
dotnet ef database update

# Seed database (optional)
dotnet run -- --seed

# Start API
dotnet run
```

### Frontend Setup

```bash
cd Frontend

# Install dependencies
npm ci

# Configure environment files:
# - src/environments/environment.ts (production)
# - src/environments/environment.development.ts (development)

# Start development server
npm start
# Opens at http://localhost:4200
```

### Docker Compose (Full Stack)

```bash
# From repository root
docker-compose up -d

# Services started:
# - postgres:5432
# - backend:8080
# - frontend:80
```

### Background Workers

Workers run as hosted services by default. To run individually:

```bash
# Image validation worker
dotnet run -- --worker ImageValidation

# Orphan cleanup worker
dotnet run -- --worker OrphanCleanup

# Blob cleanup worker
dotnet run -- --worker BlobCleanup

# Proposal cleanup (Concierge)
dotnet run -- --worker ProposalCleanup

# Idempotency cleanup
dotnet run -- --worker IdempotencyCleanup
```

---

## Project Structure

```
Hotel_Management_Full/
├── Backend/
│   ├── HotelManagement.API/           # Presentation Layer
│   │   ├── Controllers/               # 16 API controllers
│   │   ├── Middleware/                # GlobalExceptionMiddleware
│   │   ├── Filters/                   # IdempotentAttribute, SkipIdempotencyAttribute
│   │   ├── Hubs/                      # NotificationHub (SignalR)
│   │   ├── Services/                  # CurrentUserService, SignalRNotificationService
│   │   ├── Utilities/                 # MainDatabaseSeeder, AzureCredentialFactory
│   │   └── Program.cs                 # DI registration, pipeline, config
│   ├── HotelManagement.BLL/           # Business Logic Layer
│   │   ├── Interfaces/                # 19 service interfaces
│   │   ├── Services/                  # 19 service implementations
│   │   │   └── Concierge/             # AI Concierge services (7 files)
│   │   ├── DTOs/                      # 15 DTO files
│   │   ├── Options/                   # Configuration classes
│   │   ├── Profiles/                  # AutoMapper MappingProfile
│   │   ├── Workers/                   # 5 BackgroundService implementations
│   │   └── Exceptions/                # Custom exceptions
│   ├── HotelManagement.Repository/    # Repository Layer
│   │   ├── Interfaces/                # 13 repository interfaces
│   │   ├── Implementations/           # 13 repository implementations
│   │   └── Models/                    # PaginatedResult, QueryParams
│   ├── HotelManagement.DAL/           # Data Access Layer
│   │   ├── Context/                   # ApplicationDbContext (audit override)
│   │   ├── Entities/                  # 18 entity classes
│   │   ├── Enums/                     # 8 enum definitions
│   │   └── Migrations/                # 47 EF Core migrations
│   ├── HotelManagement.UnitTesting/   # Unit Tests
│   │   └── Services/                  # 15 test classes (NUnit + Moq)
│   └── HotelManagement.TestingWorkspace/ # Integration/E2E Tests
│
├── Frontend/
│   ├── src/
│   │   ├── app/
│   │   │   ├── core/                  # Guards, interceptors, services, utils
│   │   │   │   ├── guards/            # 7 route guards
│   │   │   │   ├── interceptors/      # 3 HTTP interceptors
│   │   │   │   ├── services/          # Auth, Notification, ErrorHandler
│   │   │   │   └── utils/             # jwt-decode, validators
│   │   │   ├── features/              # Feature modules
│   │   │   │   ├── admin/             # Admin dashboard, management, oversight
│   │   │   │   ├── auth/              # Login, register, auth redirect
│   │   │   │   ├── concierge/         # AI Concierge chat component
│   │   │   │   ├── front-desk/        # Front desk operations
│   │   │   │   ├── kitchen/           # Kitchen dashboard
│   │   │   │   ├── housekeeping/      # Housekeeping dashboard
│   │   │   │   ├── maintenance/       # Maintenance dashboard
│   │   │   │   ├── public/            # Home, rooms, booking wizard
│   │   │   │   └── user/              # User portal, concierge, room service
│   │   │   ├── shared/                # Reusable components
│   │   │   │   ├── components/        # GenericCRUD, TaskDashboard, etc.
│   │   │   │   ├── components/custom-cursor/
│   │   │   │   └── components/notification-snackbar/
│   │   │   └── styles/theme/          # SCSS design tokens
│   │   ├── environments/              # Environment configs
│   │   └── main.ts                    # Bootstrap
│   ├── angular.json
│   ├── package.json
│   └── Dockerfile
│
├── assets/                         # Screenshots of the project
│    
└── README.md                           # Readme for the project
```

---

## API Documentation

### Base URL

```
Development:  http://localhost:5264/api/v1
Production:   https://your-domain.com/api/v1
```

### Swagger UI

Available at `/swagger` in development: `http://localhost:5264/swagger`

### Core Endpoints

| Category | Endpoints |
|----------|-----------|
| **Authentication** | `POST /auth/login`, `POST /auth/register`, `GET /auth/me`, `PUT /auth/me`, `POST /auth/change-password` |
| **Bookings** | `GET /bookings`, `POST /bookings`, `POST /bookings/{id}/checkin`, `POST /bookings/{id}/checkout`, `POST /bookings/{id}/extend-stay`, `DELETE /bookings/{id}/cancel` |
| **Rooms** | `GET /rooms`, `POST /rooms`, `PATCH /rooms/{id}`, `GET /rooms/status` |
| **Room Types** | `GET /room-types`, `POST /room-types`, `PATCH /room-types/{id}`, `GET /room-types/availability` |
| **Orders (Room Service)** | `GET /orders`, `POST /orders`, `PATCH /orders/{id}` |
| **Housekeeping** | `GET /housekeeping`, `POST /housekeeping`, `PATCH /housekeeping/{id}`, `POST /housekeeping/trigger/{roomId}` |
| **Maintenance** | `GET /maintenance`, `POST /maintenance`, `PATCH /maintenance/{id}`, `POST /maintenance/trigger/{roomId}` |
| **Menu Items** | `GET /menu-items`, `POST /menu-items`, `PUT /menu-items/{id}`, `PATCH /menu-items/{id}/status` |
| **Staff** | `GET /staff`, `POST /staff`, `PATCH /staff/{id}` |
| **Amenities** | `GET /amenities`, `POST /amenities`, `PUT /amenities/{id}` |
| **Analytics** | `GET /analytics` (Admin only) |
| **Audit Logs** | `GET /auditlogs` (Admin only) |
| **Billing** | `GET /billing/receipts`, `GET /billing/{id}/folio/pdf` |
| **Feedback** | `GET /feedback`, `PATCH /feedback/{id}/moderate` |
| **Images** | `POST /images/upload-sas`, `POST /images/{sessionId}/confirm` |
| **AI Concierge** | `POST /concierge/chat`, `POST /concierge/confirm`, `GET /concierge/proposals`, `GET /concierge/context` |

### Rate Limiting

| Policy | Limit | Scope |
|--------|-------|-------|
| Global | 100 req / 10 sec | All endpoints |
| Image Upload | 20 req / 5 min | `/images/*` |
| Concierge | 30 tokens / min (token bucket) | `/concierge/*` |

---

## Deployment

### Azure Container Apps (Recommended)

```bash
# Build images
docker build -t aetheris-backend:latest ./Backend/HotelManagement.API
docker build -t aetheris-frontend:latest ./Frontend

# Push to Azure Container Registry
az acr login --name <acr-name>
docker tag aetheris-backend:latest <acr-name>.azurecr.io/aetheris-backend:latest
docker tag aetheris-frontend:latest <acr-name>.azurecr.io/aetheris-frontend:latest
docker push <acr-name>.azurecr.io/aetheris-backend:latest
docker push <acr-name>.azurecr.io/aetheris-frontend:latest

# Deploy via Azure CLI or Bicep/ARM templates
```

### Environment Variables (Production)

| Variable | Description | Required |
|----------|-------------|----------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | ✅ |
| `Jwt__Key` | JWT signing key (min 32 chars) | ✅ |
| `Jwt__Issuer` | JWT issuer | ✅ |
| `Jwt__Audience` | JWT audience | ✅ |
| `AzureStorage__AccountUrl` | Blob storage account URL | ✅ |
| `AzureStorage__AccountKey` | Storage account key | ✅ |
| `AzureStorage__ContainerName` | Container for images | ✅ |
| `AzureStorage__QueueName` | Queue for validation jobs | ✅ |
| `OpenAI__ApiKey` | OpenAI API key | ✅ |
| `OpenAI__Model` | Model name (default: gpt-4o-mini) | ✅ |
| `OpenAI__Endpoint` | Azure OpenAI endpoint (optional) | ❌ |
| `Concierge__MaxConversationTurns` | Max turns per conversation (default: 20) | ❌ |
| `Concierge__ProposalTtlMinutes` | Proposal TTL (default: 5) | ❌ |

### Database Migrations

```bash
# Apply migrations on startup (automatic in Program.cs)
# Or manually:
dotnet ef database update --project Backend/HotelManagement.DAL --startup-project Backend/HotelManagement.API
```

---

## Testing

### Backend Unit Tests

```bash
cd Backend/HotelManagement.UnitTesting
dotnet test --logger "console;verbosity=detailed"
```

**Coverage**: 93% BLL coverage across 29 tests (NUnit + Moq)

### Frontend Unit Tests

```bash
cd Frontend
npm test
# Uses Vitest + jsdom
```

### Integration Tests

```bash
cd Backend/HotelManagement.TestingWorkspace
dotnet test
# Uses xUnit + WebApplicationFactory for E2E simulation
```

### Test Categories

| Category | Count | Framework |
|----------|-------|-----------|
| Auth Service | 2 | NUnit |
| Booking Service | 3 | NUnit |
| Concierge Service | 7 | NUnit |
| Analytics Service | 2 | NUnit |
| Order Service | 2 | NUnit |
| Housekeeping Service | 2 | NUnit |
| Maintenance Service | 2 | NUnit |
| Billing Service | 2 | NUnit |
| Feedback Service | 2 | NUnit |
| Room/RoomType Service | 2 | NUnit |
| Staff/Amenity/MenuItem Service | 3 | NUnit |
| Audit Log Service | 2 | NUnit |
| **Total** | **29** | |

---

## Contributing

We welcome contributions! Please follow these guidelines:

1. **Fork the repository** and create a feature branch
2. **Follow the existing architecture** — respect the N-Tier boundaries
3. **Write tests** for new functionality (unit + integration)
4. **Update documentation** in `/Documents` for significant changes
5. **Run linting/formatting**: `dotnet format` (backend), `npm run format` (frontend)
6. **Submit a PR** with a clear description of changes

### Code Standards

- **Backend**: C# 13, .NET 10, nullable reference types enabled
- **Frontend**: Angular 22, Signals, standalone components, strict TypeScript
- **Git**: Conventional commits (`feat:`, `fix:`, `refactor:`, `docs:`, `test:`)

---

## License

This project is licensed under the **MIT License** — see the [LICENSE](LICENSE) file for details.

---

## Acknowledgments

- **Antropic** for Opus 4.8 powering the AI Concierge
- **Azure** for Blob Storage, Queue Storage, and Container Apps
- **Angular Team** for the incredible Angular 22 + Signals DX
- **ASP.NET Core Team** for the robust web framework
- **PostgreSQL** for the rock-solid database engine
- **QuestPDF** for beautiful PDF generation
- **ECharts** for stunning visualizations
- **Serilog** for structured logging excellence

---

