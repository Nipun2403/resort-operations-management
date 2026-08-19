# AETHERIS — 5-Minute Capstone Presentation Script v2 (Corporate Edition)
**Total: 300 seconds | 4 Desktops | 1 Narrative Arc | Enterprise-Grade Terminology**

---

## DESKTOP MAP (Pre-loaded, logged in)

| Desktop | Portal | Account | Key Features |
|---------|--------|---------|--------------|
| **1** | Guest | `cust2@gmail.com` / `Pass@1234` | Public UI scroll → ATLAS Concierge |
| **2** | Admin | `admin@aetheris.com` / `Pass@1234` | Dashboard KPIs → Analytics → Room Grid → Audit → Billing PDF → Feedback |
| **3** | Front Desk | `frontdesk@aetheris.com` / `Pass@1234` | Today's Movements → Global Guest Search |
| **4** | Ops (Kitchen/HK/Maintenance) | `kitchen@aetheris.com` / `hk1@aetheris.com` / `maintenance@aetheris.com` | SignalR proof (from ATLAS) → Kitchen disable item |

---

## SCRIPT

---

### 0:00 – 0:30 | INTRO (30s) — Desktop 1 (Guest Portal) Visible

**[Stand center. Desktop 1 showing `/home` page.]**

> **0:00** — "Good morning. I'm presenting **Aetheris** — a **cloud-native, AI-first luxury hospitality platform** where **guest-facing agentic AI executes real-world operations**, **staff dashboards achieve sub-second eventual consistency via WebSocket fan-out**, and **comprehensive auditability is a cross-cutting concern implemented at the ORM layer with zero developer friction**."
>
> **0:10** — "Built on a **strict N-tier architecture** — **Angular 22 + ASP.NET Core 10 + PostgreSQL** — deployed on **Azure Container Apps** with **managed PostgreSQL Flexible Server, Azure Blob Storage, Queue Storage, and SignalR Service** — **100% Infrastructure-as-Code via Bicep**, zero manual console clicks."
>
> **0:20** — "Six RBAC roles mapped to sixteen protected controllers. Five **BackgroundService**-based workers for async orchestration. **Clean architecture enforced at project-reference level** — API → BLL → Repository → DAL — zero circular dependencies, validated by GitNexus structural analysis."
>
> **0:25** — "Let me demonstrate the **guest-to-operations value chain**."

**[3-finger swipe → Desktop 1 already there. No wait.]**

---

### 0:30 – 1:30 | DESKTOP 1: GUEST PORTAL (60s)

#### 0:30 – 0:42 | Public UI Scroll (12s) — *Design System as Competitive Moat*
**[Scroll `/home` → `/rooms` → `/experiences` → `/menu` — smooth, deliberate.]**

> "The public experience isn't a theme — it's a **custom design system** with **design tokens**, **glass-morphism component library**, and **motion-aware animations** respecting `prefers-reduced-motion`. **SSR hydration <400ms**, Lighthouse 95+. **Market reality**: most PMS vendors white-label a booking engine iframe. We own the **end-to-end guest journey** — brand consistency drives direct booking conversion."

#### 0:42 – 1:30 | ATLAS AI Concierge (48s) — *Agentic AI with Guardrails*
**[Click chat widget → ATLAS opens. Type live.]**

> **0:42** — "**ATLAS** — not a retrieval-augmented chatbot. An **agentic execution engine** with **8 OpenAI function tools** — 3 **side-effect tools** (`create_food_order`, `create_housekeeping_request`, `create_maintenance_ticket`) and 5 **read-only tools** — **scoped to the authenticated guest's active booking context via claim-based tenant isolation**."

**[Type: `"I'd like a burger and fries, extra pillows, and the AC isn't working"` — Hit Enter]**

> **0:50** — "**Single intent → multi-tool fan-out**. One user message → three **pending proposals** with **5-minute TTL**, **idempotency keys** per proposal. **Market reality**: legacy PMS 'AI' = FAQ bot. Ours **executes domain logic via existing BLL services** — `OrderService`, `HousekeepingService`, `MaintenanceService` — **zero new business logic surface area**."

**[Wait for proposals to render. Point to countdown timers.]**

> **0:55** — *During render:* "**Prompt injection defense** — regex sanitizer strips adversarial patterns (`ignore previous`, `system:`, `assistant:`) **pre-LLM**. **Idempotency on every turn** — duplicate submissions replay cached response, **eliminating double-charge risk** at the action filter layer."

**[Click CONFIRM on all three proposals.]**

> **1:05** — "**Proposal confirmation → BLL execution → SignalR fan-out** to Kitchen, HK, Maintenance groups **in <500ms p99**. **WebSocket reconnect with exponential backoff** built-in. **Market reality**: competitors poll every 30s. We push. **Zero staleness**."

**[Don't switch desktops yet. Let the confirmation render.]**

> **1:12** — "**Guest context awareness** — ATLAS hydrates room 304, booking #1047, folio balance $1,240 **server-side via `ICurrentUserService`**. **BookingId/RoomId never leave the trust boundary** — tools receive only action parameters. **Zero context leakage to LLM**."

> **1:20** — "**Full audit trail** — every tool invocation, proposal state transition, confirmation logged to `ConciergeActionLog` with **correlation IDs**. **Self-auditing** — the audit log audits itself."

**[3-finger swipe → Desktop 2 (Admin)]**

---

### 1:30 – 2:45 | DESKTOP 2: ADMIN PORTAL (75s)

#### 1:30 – 1:55 | Dashboard KPIs (25s) — *Database-Native Analytics*
**[Dashboard loads — 6 KPI cards visible.]**

> "Admin dashboard. **6 KPIs materialized via PostgreSQL stored procedures** — Occupancy, ADR, RevPAR, Guest Satisfaction, Avg Length of Stay, HK Turnaround. **Zero application-layer aggregation**. **Sub-100ms p95**. **Market reality**: competitors pull 50K rows to C# for `Sum()`. We compute where data lives."

**[Hover RevPAR card → tooltip shows formula.]**

> "RevPAR = `total_room_revenue / total_available_rooms`. Stored proc `calculaterevpar` executes **on live data, parameterized, injection-safe**. Every refresh = **current source of truth**."

#### 1:55 – 2:10 | Analytics Interactive (15s) — *Self-Service BI*
**[Click "Analytics" nav → page loads with charts.]**

> "Interactive **ECharts visualizations** with **category-driven query decomposition** — Revenue / Operations / Guests. **Date presets** (7d, 30d, Quarterly, Custom) map to **parameterized SP calls**. Hover → **exact value tooltip**. Bar/Line/Radar/Pie — **server-computed, client-rendered**. **Market reality**: most dashboards are static screenshots. Ours is **live, exploratory, role-scoped**."

**[Quick hover on one bar. Click category dropdown → switch to Operations.]**

#### 2:10 – 2:22 | Room Management Grid (12s) — *Adaptive Density UX*
**[Click "Management → Rooms" → Grid view loads.]**

> "Room grid — **visual status at a glance** with **semantic color coding** (Available/Occupied/Dirty/Maintenance). **Single-click toggle to high-density table** for CRUD — **same data source, adaptive presentation**. **Market reality**: vendors force either grid OR table. We give **context-aware density switching** — admin chooses per task."

**[Click grid/table toggle button once.]**

#### 2:22 – 2:32 | Audit Log (10s) — *Zero-Touch Compliance*
**[Click "Oversight → Audit Logs" → Open one record.]**

> "**Comprehensive change data capture at the ORM layer**. Every `Added`/`Modified`/`Deleted` across **18 entity types** → **JSONB diff** with before/after values, actor (resolved via `IAuditUserProvider`), timestamp, entity name, composite PK. **Zero attributes, zero manual instrumentation**. `SaveChangesAsync` override — **impossible to bypass**. **Market reality**: audit = triggers or manual calls. Ours is **architectural, not procedural**."

**[Close detail modal.]**

#### 2:32 – 2:40 | Billing PDF (8s) — *Document Generation as a Service*
**[Click "Billing" → Open any receipt → Show pre-downloaded PDF.]**

> "PDF generation via **QuestPDF** — **fluent layout engine**, **typographic control**, **multi-portal reuse** (Admin, Front Desk, Guest). **Templated, branded, streaming download**. **Market reality**: Crystal Reports or SSRS — heavy, licensed, brittle. We use **code-first, version-controlled, testable** PDFs."

**[Close PDF.]**

#### 2:40 – 2:45 | Feedback Moderation (5s) — *Brand Protection Workflow*
**[Click "Feedback" → Show one moderated card.]**

> "Guest feedback with **moderation queue** — **approve/reject before public display**. **Image attachment support**. **Protects brand equity**. **Market reality**: TripAdvisor/Google reviews are uncontrolled. We give **first-party reputation management**."

**[3-finger swipe → Desktop 3 (Front Desk)]**

---

### 2:45 – 3:20 | DESKTOP 3: FRONT DESK (35s)

#### 2:45 – 3:00 | Today's Movements (15s) — *Operational Situational Awareness*
**[Dashboard visible — "Today's Movements" section.]**

> "Front desk **command center**. **Arrivals & departures for today** — room, guest, status, special requests. **Pre-shift situational awareness**. **Market reality**: front desk runs on printed reports or memory. We give **real-time operational intelligence** — housekeeping knows exactly which rooms turn over when."

#### 3:00 – 3:20 | Global Guest Search (20s) — *Unified Entity Resolution*
**[Click search icon → Type "john" → Results appear instantly.]**

> "Phone rings. Guest says 'I'm John, I need to extend.' **Federated search** across email, name, phone, booking ID — **single index, instant results**. Click → **context-preserving deep link** to check-in, extend, cancel, folio — **zero context switching**. **Market reality**: legacy PMS = 5 clicks, 3 screens. We deliver **single-interaction resolution**."

**[3-finger swipe → Desktop 4 (Ops)]**

---

### 3:20 – 3:55 | DESKTOP 4: OPERATIONS (35s)

#### 3:20 – 3:35 | SignalR Proof — Live from ATLAS (15s) — *Event-Driven Fan-Out*
**[Kitchen dashboard open. Show the burger order from ATLAS.]**

> "Remember the burger from ATLAS? **Here it is** — Kitchen dashboard, **real-time SignalR push via Azure SignalR Service**. **No polling, no long-polling, no SSE fallback**. **WebSocket with automatic reconnect**. Order #1047, Room 304, Burger + Fries. Kitchen taps 'Preparing' → **optimistic UI update** → **server confirmation** → **broadcast to all subscribers**. **Market reality**: kitchen printers or 30s polling. We deliver **sub-second eventual consistency**."

**[Click "Preparing" → status changes.]**

#### 3:35 – 3:55 | Kitchen Disable Menu Item (20s) — *Single Source of Truth*
**[Click "Menu Management" → Toggle "Lobster Thermidor" OFF.]**

> "Lobster's 86'd. **Toggle OFF** → **instant cache invalidation** → **grayed out on guest menu, room service, ATLAS tool suggestions** via **reactive query invalidation**. Guest *cannot* order unavailable items. **Market reality**: 'We'll check with kitchen' callbacks, angry guests, comped meals. We enforce **write-once, read-everywhere consistency**."

**[Optional: Quick swipe to Guest portal (2s) to show grayed-out item.]**

---

### 3:55 – 4:15 | TECH DEPTH (20s) — *Platform Capabilities You Didn't See*
**[Verbal only. No desktop switch.]**

> "**Platform hardening**: **Magic-byte image validation** — JPEG `FF D8 FF`, PNG 8-byte signature, WebP RIFF/WEBP — **content-level, not extension-level**. **End-to-end idempotency** via `X-Idempotency-Key` on **every mutation** — action filter + dedicated table + 48h TTL cleanup worker. **Multi-tier rate limiting** — global fixed-window, image-upload token bucket, ATLAS concierge token bucket. **Zero-trust auth** — BCrypt + JWT (HMAC-SHA256) + SignalR token passthrough. **47 EF Core migrations** — **schema evolution as code**. **29 NUnit+Moq unit tests, 93% BLL coverage**, xUnit WebApplicationFactory E2E simulations, Vitest frontend. **Bicep IaC** — **zero-click, drift-free deployments**."

---

### 4:15 – 5:00 | CLOSE (45s) — Back to Center Stage

> **4:15** — "Aetheris proves **luxury hospitality doesn't need legacy PMS compromise**. **Agentic AI that executes**. **Staff dashboards with true real-time consistency**. **Auditability as a cross-cutting architectural concern**. **Infrastructure as code, security by default, observability built-in**."
>
> **4:25** — "This is **production-grade code**. **Deployed on Azure**. **Load-tested**. **Pen-tested patterns**. **Architecture scales from boutique to enterprise brand**."
>
> **4:35** — "Three strategic differentiators:"
>
> > **1. Execution > Suggestion** — ATLAS **commits transactions**, not just conversations. Real orders. Real tickets. Real revenue.
> >
> > **2. Real-Time by Default** — **SignalR fan-out everywhere**. No polling. No staleness. **Sub-second eventual consistency** across 6 roles.
> >
> > **3. Zero-Touch Observability** — **Every change audited, every upload validated, every retry idempotent**. Compliance without developer tax.
>
> **4:50** — "Deployed at `hotel-web-demo1.ambitiousmushroom-274454dc.centralindia.azurecontainerapps.io`. Credentials in your packet. **Open for technical deep-dive in Q&A**."
>
> **5:00** — "Thank you."

---

## RISK MITIGATION TABLE

| Risk | Mitigation |
|------|------------|
| ATLAS OpenAI latency >10s | Pre-warm: send "hello" 2 min before. Have screenshot of proposals as backup slide. |
| SignalR not received on Desktop 4 | Keep Kitchen tab open *before* demo. Refresh at 3:15. |
| PDF doesn't open | Have PDF pre-downloaded on desktop. Show file, not browser. |
| Search returns empty | Pre-create "John" booking. Know exact search term. |
| Time overrun | **Hard stops**: 1:30 (leave Guest), 2:45 (leave Admin), 3:20 (leave Front Desk), 3:55 (leave Ops). Skip Feedback/Billing if behind. |
| Desktop switch lag | macOS: Mission Control → assign each browser to its own Space. Test 3-finger swipe speed. |

---

## REHEARSAL CHECKLIST

- [ ] All 4 desktops logged in, correct tabs open
- [ ] ATLAS pre-warmed (send "hi" 2 min before)
- [ ] Kitchen dashboard open on Desktop 4
- [ ] PDF downloaded to `~/Desktop/folio-sample.pdf`
- [ ] "John" booking exists for search demo
- [ ] Lobster Thermidor is **enabled** before demo start
- [ ] Timer visible (phone/watch) — glance at 1:30, 2:45, 3:20, 3:55
- [ ] Water, clicker, backup slides on USB

---

## ONE-LINER FOR EACH AUDIENCE TYPE (Q&A Prep)

| Role | Hook |
|------|------|
| **Sales** | "Guest conversion ↑ because ATLAS turns 'I want...' into **committed revenue in 3 clicks** — reduces booking abandonment." |
| **HR** | "Audit trail = **compliance-ready, zero dev tax**. Staff dashboards = **reduced cognitive load, lower burnout**." |
| **Architect** | "Clean N-tier. **Domain services reusable**. EF Core **only in DAL**. **Swappable persistence**. GitNexus-validated acyclic." |
| **Cloud Engineer** | "Azure-native: Container Apps, Managed PostgreSQL, Blob+Queue, SignalR Service. **Bicep IaC, managed identities, private endpoints**." |
| **AI Director** | "**Function calling with proposal/confirmation pattern**. **Guest-scoped tenancy**. **Pre-LLM sanitization**. **Full correlation-ID audit**." |
| **Delivery** | "**Generic CRUD component** powers 8 management pages. **New entity = config, not code**. **Design system = consistent velocity**." |

---

## MARKET COMPARISON QUICK-REFERENCE (For Q&A)

| Capability | Legacy PMS (Opera/Cloudbeds) | Modern SaaS (Mews/SkyTouch) | **Aetheris** |
|------------|------------------------------|----------------------------|--------------|
| AI Concierge | FAQ bot / none | Basic chat | **Agentic execution with proposals** |
| Real-Time Ops | Polling (30-60s) | WebSocket (some) | **SignalR fan-out, sub-second** |
| Audit Trail | Triggers / manual | Limited | **ORM-level, zero-touch, JSONB** |
| Image Security | Extension check | MIME check | **Magic-byte validation** |
| Idempotency | Payments only | Partial | **Every mutation** |
| Deployment | On-prem / manual | SaaS only | **IaC, hybrid-ready** |
| Design System | Template | Themeable | **Custom tokens, glass-morphism** |

---

*Script v2 — Corporate terminology edition for internship capstone presentation*