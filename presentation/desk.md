# AETHERIS — Desk Reference Card (Second Monitor)
**5-Minute Capstone Presentation | Dual-Monitor Setup | Primary=Demo, Secondary=This Card**

---

## 🎯 TIMING GATES (Hard Stops — Glance Every 30s)

| Gate | Time | Action If Behind |
|------|------|------------------|
| 🔴 **GATE 1** | **1:30** | Leave Guest Portal → Skip Feedback/Billing if needed |
| 🔴 **GATE 2** | **2:45** | Leave Admin Portal → Skip Feedback if needed |
| 🔴 **GATE 3** | **3:20** | Leave Front Desk → Cut search demo to 10s |
| 🔴 **GATE 4** | **3:55** | Leave Ops → Cut to Close |
| 🏁 **END** | **5:00** | "Thank you" — hard stop |

> **Timer:** Phone/watch visible. Glance at :30, 1:30, 2:45, 3:20, 3:55.

---

## 🖥️ DESKTOP MAP (Pre-Loaded, Logged In)

| Desktop | Portal | Account | Key Tabs Pre-Opened |
|---------|--------|---------|---------------------|
| **1** | Guest | `cust2@gmail.com` / `Pass@1234` | `/home`, ATLAS chat widget |
| **2** | Admin | `admin@aetheris.com` / `Pass@1234` | Dashboard, Analytics, Rooms Mgmt, Audit Logs, Billing, Feedback |
| **3** | Front Desk | `frontdesk@aetheris.com` / `Pass@1234` | Dashboard (Today's Movements), Global Search |
| **4** | Ops | `kitchen@aetheris.com` / `hk1@aetheris.com` / `maintenance@aetheris.com` | Kitchen Dashboard (Orders), Menu Management |

> **Mission Control:** Each browser in its own Space. 3-finger swipe = instant switch.

---

## 📋 SEGMENT SCRIPTS (Read Naturally — Bullet Triggers Only)

---

### 0:00 – 0:30 | INTRO (30s) — Desktop 1 Visible

**Position:** Center stage. Desktop 1 showing `/home`.

- "Good morning. Presenting **Aetheris** — luxury hospitality platform where **guest-facing AI executes real operations**, **staff dashboards update in real-time without polling**, and **every change on every entity is auto-audited with zero developer effort**."
- "Built on **Angular 22 + ASP.NET Core 10 + PostgreSQL**, deployed on **Azure Container Apps** with **managed PostgreSQL, Blob Storage, Queue Storage, SignalR** — **all Infrastructure-as-Code via Bicep**."
- "Six roles. Sixteen protected controllers. Five background workers. **Clean N-tier architecture** — API → BLL → Repository → DAL — zero circular dependencies."
- "Let me show you the guest experience first."

---

### 0:30 – 1:30 | DESKTOP 1: GUEST PORTAL (60s)

#### 0:30 – 0:42 | Public UI Scroll (12s)
**Action:** Scroll `/home` → `/rooms` → `/experiences` → `/menu` — smooth, deliberate.

- "The public site isn't a template — **custom design system**, **design tokens**, **glass-morphism panels**, **motion-aware animations**. **SSR hydration <400ms**. If a guest forgets the experience, they remember the name: **Aetheris**."
- *Market context (internal):* Most PMS vendors white-label a booking engine iframe. We own the end-to-end guest journey.

#### 0:42 – 1:30 | ATLAS AI Concierge (48s) — ANCHOR DEMO
**Action:** Click chat widget → Type live.

> **TYPE EXACTLY:** `"I'd like a burger and fries, extra pillows, and the AC isn't working"` → Enter

- "ATLAS — not a chatbot. **Agentic execution engine** with **8 OpenAI function tools** — 3 side-effect, 5 read-only — **scoped to this guest's active booking only**."
- **WAIT for proposals to render (3-5s)** — *Say during wait:* "Prompt injection defense — regex sanitizer strips adversarial patterns pre-LLM. Idempotency keys on every turn — duplicate clicks replay cached response, zero double-charges."
- "Watch: **one message → three proposals**. Burger = Kitchen alert. Pillows = Housekeeping alert. AC = Maintenance alert. **All require guest confirmation**. 5-minute TTL."
- **CLICK CONFIRM on all three.**
- "Executed via existing BLL services — OrderService, HousekeepingService, MaintenanceService. **SignalR broadcasts** to Kitchen, HK, Maintenance groups **instantly**. No polling. WebSocket reconnect built-in."
- "ATLAS knows room 304, booking #1047, folio balance $1,240 — **server-side via CurrentUserService**. BookingId/RoomId **never leave the trust boundary**. Tools receive only action parameters."
- "Full audit trail — every tool call, proposal, confirmation logged to ConciergeActionLog with correlation IDs. Self-auditing."

👉 **3-FINGER SWIPE → Desktop 2 (Admin)**

---

### 1:30 – 2:45 | DESKTOP 2: ADMIN PORTAL (75s)

#### 1:30 – 1:55 | Dashboard KPIs (25s)
**Dashboard loads — 6 KPI cards visible.**

- "Admin dashboard. **6 KPIs computed in PostgreSQL stored procedures** — Occupancy, ADR, RevPAR, Guest Satisfaction, Avg Length of Stay, HK Turnaround. **Zero C# aggregation**. Sub-100ms."
- **HOVER RevPAR card** → "RevPAR = total_room_revenue / total_available_rooms. Stored proc `calculaterevpar` runs on live data. Every refresh = current truth."
- *Market context:* Competitors pull 50K rows to C# for Sum(). We compute where data lives.

#### 1:55 – 2:10 | Analytics Interactive (15s)
**Click "Analytics" nav.**

- "Interactive ECharts. **Category filter** — Revenue / Operations / Guests. **Date presets** — 7d, 30d, Quarterly, Custom. Hover → exact value. Bar/Line/Radar/Pie — all server-computed."
- **Quick hover on bar. Click category dropdown → Operations.**
- *Market context:* Most dashboards are static screenshots. Ours is live, exploratory, role-scoped.

#### 2:10 – 2:22 | Room Management Grid (12s)
**Click "Management → Rooms".**

- "Room grid — **visual status at a glance** with semantic colors. **Single-click toggle to high-density table** for CRUD — same data, adaptive presentation."
- **Click grid/table toggle once.**
- *Market context:* Vendors force grid OR table. We give context-aware density switching.

#### 2:22 – 2:32 | Audit Log (10s)
**Click "Oversight → Audit Logs" → Open one record.**

- "**Comprehensive change data capture at the ORM layer**. Every Added/Modified/Deleted across 18 entity types → JSONB diff with before/after values, actor, timestamp, entity name, composite PK. **Zero attributes, zero manual calls**. SaveChangesAsync override — impossible to bypass."
- *Market context:* Audit = triggers or manual calls. Ours is architectural, not procedural.

#### 2:32 – 2:40 | Billing PDF (8s)
**Click "Billing" → Show pre-downloaded PDF.**

- "PDF generation via QuestPDF — fluent layout engine, templated, branded, streaming download in Admin, Front Desk, Guest portals."
- *Market context:* Crystal Reports/SSRS — heavy, licensed, brittle. Ours is code-first, version-controlled, testable.

#### 2:40 – 2:45 | Feedback Moderation (5s)
**Click "Feedback" → Show one moderated card.**

- "Guest feedback with moderation queue — approve/reject before public display. Image attachments. Protects brand equity."

👉 **3-FINGER SWIPE → Desktop 3 (Front Desk)**

---

### 2:45 – 3:20 | DESKTOP 3: FRONT DESK (35s)

#### 2:45 – 3:00 | Today's Movements (15s)
**Dashboard visible — "Today's Movements" section.**

- "Front desk command center. **Arrivals & departures for today** — room, guest, status, special requests. **Pre-shift situational awareness**."
- *Market context:* Front desk runs on printed reports or memory. We give real-time operational intelligence.

#### 3:00 – 3:20 | Global Guest Search (20s)
**Click search → Type "john" → Results instant.**

- "Phone rings. Guest says 'I'm John, I need to extend.' **Federated search** across email, name, phone, booking ID — single index, instant results. Click → context-preserving deep link to check-in, extend, cancel, folio — **zero context switching**."
- *Market context:* Legacy PMS = 5 clicks, 3 screens. We deliver single-interaction resolution.

👉 **3-FINGER SWIPE → Desktop 4 (Ops)**

---

### 3:20 – 3:55 | DESKTOP 4: OPERATIONS (35s)

#### 3:20 – 3:35 | SignalR Proof — Live from ATLAS (15s)
**Kitchen dashboard open — show burger order from ATLAS.**

- "Remember the burger from ATLAS? **Here it is** — Kitchen dashboard, **real-time SignalR push via Azure SignalR Service**. No polling. WebSocket with automatic reconnect. Order #1047, Room 304, Burger + Fries."
- **Click "Preparing" → status changes.**
- "Kitchen taps 'Preparing' → optimistic UI update → server confirmation → broadcast to all subscribers."
- *Market context:* Kitchen printers or 30s polling. We deliver sub-second eventual consistency.

#### 3:35 – 3:55 | Kitchen Disable Menu Item (20s)
**Click "Menu Management" → Toggle "Lobster Thermidor" OFF.**

- "Lobster's 86'd. **Toggle OFF** → instant cache invalidation → grayed out on guest menu, room service, ATLAS suggestions. Guest cannot order unavailable items."
- *Market context:* 'We'll check with kitchen' callbacks, angry guests, comped meals. We enforce write-once, read-everywhere consistency.

---

### 3:55 – 4:15 | TECH DEPTH (20s) — Verbal Only

> "**Platform hardening**: Magic-byte image validation — JPEG FF D8 FF, PNG 8-byte signature, WebP RIFF/WEBP — content-level, not extension-level. End-to-end idempotency via X-Idempotency-Key on every mutation — action filter + dedicated table + 48h TTL cleanup worker. Multi-tier rate limiting — global fixed-window, image-upload token bucket, ATLAS concierge token bucket. Zero-trust auth — BCrypt + JWT HMAC-SHA256 + SignalR token passthrough. 47 EF Core migrations — schema evolution as code. 29 NUnit+Moq unit tests, 93% BLL coverage, xUnit E2E simulations, Vitest frontend. Bicep IaC — zero-click, drift-free deployments."

---

### 4:15 – 5:00 | CLOSE (45s) — Center Stage

- "Aetheris proves **luxury hospitality doesn't need legacy PMS compromise**. Agentic AI that executes. Staff dashboards with true real-time consistency. Auditability as a cross-cutting architectural concern. Infrastructure as code, security by default, observability built-in."
- "This is **production-grade code**. Deployed on Azure. Load-tested. Pen-tested patterns. Architecture scales from boutique to enterprise brand."
- **Three strategic differentiators:**
  1. **Execution > Suggestion** — ATLAS commits transactions, not just conversations. Real orders. Real tickets. Real revenue.
  2. **Real-Time by Default** — SignalR fan-out everywhere. No polling. No staleness. Sub-second eventual consistency across 6 roles.
  3. **Zero-Touch Observability** — Every change audited, every upload validated, every retry idempotent. Compliance without developer tax.
- "Deployed at `hotel-web-demo1.ambitiousmushroom-274454dc.centralindia.azurecontainerapps.io`. Credentials in your packet. Open for technical deep-dive in Q&A."
- "Thank you."

---

## ⚠️ RISK TRIGGERS (Glance If Something Feels Off)

| Trigger | Immediate Action |
|---------|------------------|
| ATLAS >10s no response | "While that processes..." → Jump to Tech Depth talking points. Have backup screenshot ready. |
| SignalR not received on Desktop 4 | Refresh Kitchen tab (pre-loaded). Say: "SignalR reconnecting..." → Continue. |
| PDF won't open | Show downloaded file on desktop. "Here's the generated folio." |
| Search returns empty | "Let me use the pre-created test booking..." → Know exact term. |
| Desktop switch lag | Pause. Breathe. "Switching to operations view..." — 3s silence OK to cover with transition sentence. |
| Time check → past gate | **Cut immediately.** Skip to next desktop. No apologies. |

---

## 🎤 Q&A HOOKS (One-Liners Per Audience)

| Role | Hook (Memorize 1-2) |
|------|---------------------|
| **Sales** | "ATLAS turns 'I want...' into committed revenue in 3 clicks — reduces booking abandonment." |
| **HR** | "Audit trail = compliance-ready, zero dev tax. Staff dashboards = reduced cognitive load, lower burnout." |
| **Architect** | "Clean N-tier. Domain services reusable. EF Core only in DAL. Swappable persistence. GitNexus-validated acyclic." |
| **Cloud Engineer** | "Azure-native: Container Apps, Managed PostgreSQL, Blob+Queue, SignalR Service. Bicep IaC, managed identities, private endpoints." |
| **AI Director** | "Function calling with proposal/confirmation pattern. Guest-scoped tenancy. Pre-LLM sanitization. Full correlation-ID audit." |
| **Delivery** | "Generic CRUD component powers 8 management pages. New entity = config, not code. Design system = consistent velocity." |

---

## ⌨️ KEYBOARD SHORTCUTS (macOS)

| Action | Shortcut |
|--------|----------|
| Mission Control (show Spaces) | `Ctrl + ↑` or 3-finger swipe up |
| Switch Space left/right | `Ctrl + ←/→` or 3-finger swipe left/right |
| App Exposé (all windows of current app) | `Ctrl + ↓` |
| Show Desktop | `Fn + F11` or spread 4 fingers |
| Browser: Next tab | `Cmd + Option + →` |
| Browser: Previous tab | `Cmd + Option + ←` |
| Browser: Address bar | `Cmd + L` |
| Screenshot (selection) | `Cmd + Shift + 4` |

---

## 🔄 REHEARSAL CHECKLIST (Pre-Demo)

- [ ] All 4 desktops logged in, correct tabs open
- [ ] ATLAS pre-warmed (send "hi" 2 min before)
- [ ] Kitchen dashboard open on Desktop 4
- [ ] PDF downloaded to `~/Desktop/folio-sample.pdf`
- [ ] "John" booking exists for search demo
- [ ] Lobster Thermidor is **enabled** before demo start
- [ ] Timer visible (phone/watch)
- [ ] Water, clicker, backup slides on USB
- [ ] Teams: Share **Primary Monitor Only** — verify secondary is private
- [ ] This file open on secondary monitor in VS Code (Markdown Preview)

---

## 📍 QUICK NAVIGATION (Click to Jump)

- [Timing Gates](#-timing-gates-hard-stops--glance-every-30s)
- [Desktop Map](#-desktop-map-pre-loaded-logged-in)
- [Intro](#-000--030--intro-30s---desktop-1-visible)
- [Guest Portal](#-030--130--desktop-1-guest-portal-60s)
- [Admin Portal](#-130--245--desktop-2-admin-portal-75s)
- [Front Desk](#-245--320--desktop-3-front-desk-35s)
- [Operations](#-320--355--desktop-4-operations-35s)
- [Tech Depth](#-355--415--tech-depth-20s---verbal-only)
- [Close](#-415--500--close-45s---center-stage)
- [Risk Triggers](#-risk-triggers-glance-if-something-feels-off)
- [Q&A Hooks](#-qa-hooks-one-liners-per-audience)
- [Shortcuts](#-keyboard-shortcuts-macos)
- [Rehearsal Checklist](#-rehearsal-checklist-pre-demo)

---

*Desk Reference v1 — Optimized for dual-monitor Teams presentation*