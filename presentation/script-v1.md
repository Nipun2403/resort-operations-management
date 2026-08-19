# AETHERIS — 5-Minute Capstone Presentation Script v1
**Total: 300 seconds | 4 Desktops | 1 Narrative Arc**

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

> **0:00** — "Good morning. I'm presenting **Aetheris** — a luxury hotel management system where **guest-facing AI executes real operations**, **staff dashboards update in real-time without polling**, and **every change on every entity is auto-audited with zero developer effort**."
>
> **0:10** — "Built on **Angular 22 + ASP.NET Core 10 + PostgreSQL**, deployed on **Azure Container Apps** with **managed PostgreSQL, Blob Storage, Queue Storage, and SignalR** — all infrastructure as Bicep code."
>
> **0:20** — "Six roles. Sixteen protected controllers. Five background workers. One architecture: **API → BLL → Repository → DAL**. Zero circular dependencies."
>
> **0:25** — "Let me show you the guest experience first."

**[3-finger swipe → Desktop 1 already there. No wait.]**

---

### 0:30 – 1:30 | DESKTOP 1: GUEST PORTAL (60s)

#### 0:30 – 0:42 | Public UI Scroll (12s)
**[Scroll `/home` → `/rooms` → `/experiences` → `/menu` — smooth, deliberate.]**

> "The public site isn't a template. **Custom design system** — obsidian/gold theme, glass-morphism panels, motion-aware animations. Every page loads in **<400ms** with SSR hydration. If a guest forgets the experience, they remember the name: **Aetheris**."

#### 0:42 – 1:30 | ATLAS AI Concierge (48s) — THE ANCHOR DEMO
**[Click chat widget → ATLAS opens. Type live.]**

> **0:42** — "**ATLAS**. Not a chatbot. An **action engine** with **8 OpenAI function tools** — 3 side-effect, 5 read-only — scoped to *this guest's active booking only*."

**[Type: `"I'd like a burger and fries, extra pillows, and the AC isn't working"` — Hit Enter]**

> **0:50** — "Watch: **one message → three proposals**. Burger = `create_food_order` → Kitchen alert. Pillows = `create_housekeeping_request` → HK alert. AC = `create_maintenance_ticket` → Maintenance alert. **All require guest confirmation.** No auto-execution. 5-minute TTL."

**[Wait for proposals to render. Point to countdown timers.]**

> **0:58** — "**Prompt injection protection** — regex sanitizer strips `ignore previous`, `system:`, `assistant:` before LLM sees it. **Idempotency key** on every turn — duplicate clicks = replayed response, zero double-charges."

**[Click CONFIRM on all three proposals.]**

> **1:05** — "**Executed via existing BLL services** — `OrderService`, `HousekeepingService`, `MaintenanceService`. **SignalR broadcasts** to Kitchen, HK, Maintenance groups *instantly*. No polling. WebSocket reconnect built-in."

**[Don't switch desktops yet. Let the confirmation render.]**

> **1:12** — "**Guest context awareness** — ATLAS knows room 304, booking #1047, folio balance $1,240. Never sends IDs to LLM. Tools execute against *current user only*."

> **1:20** — "**Audit trail** — every tool call, proposal, confirmation logged to `ConciergeActionLog` with outcome. Self-auditing."

**[3-finger swipe → Desktop 2 (Admin)]**

---

### 1:30 – 2:45 | DESKTOP 2: ADMIN PORTAL (75s)

#### 1:30 – 1:55 | Dashboard KPIs (25s)
**[Dashboard loads — 6 KPI cards visible.]**

> "Admin dashboard. **6 KPIs computed in PostgreSQL stored procedures** — Occupancy, ADR, RevPAR, Guest Satisfaction, Avg Length of Stay, HK Turnaround. **Zero C# aggregation**. Sub-100ms response."

**[Hover RevPAR card → tooltip shows formula.]**

> "RevPAR = `total_room_revenue / total_available_rooms`. Stored proc `calculaterevpar` runs on live data. Every refresh = current truth."

#### 1:55 – 2:10 | Analytics Interactive (15s)
**[Click "Analytics" nav → page loads with charts.]**

> "Interactive ECharts. **Category filter** — Revenue / Operations / Guests. **Date presets** — 7d, 30d, Quarterly, Custom. Hover any bar → exact value. **Bar / Line / Radar / Pie** — all server-computed, client-rendered."

**[Quick hover on one bar. Click category dropdown → switch to Operations.]**

#### 2:10 – 2:22 | Room Management Grid (12s)
**[Click "Management → Rooms" → Grid view loads.]**

> "Room grid — **visual status at a glance**. Green=Available, Blue=Occupied, Orange=Dirty, Red=Maintenance. **Switch to table** for detailed CRUD — same data, density toggle. Admin chooses view per task."

**[Click grid/table toggle button once.]**

#### 2:22 – 2:32 | Audit Log (10s)
**[Click "Oversight → Audit Logs" → Open one record.]**

> "**Every change on every entity — auto-logged**. Before/after JSONB diff. Who, when, what entity, PK. **Zero attributes, zero manual calls**. `SaveChangesAsync` override captures all 18 entity types. Self-auditing too."

**[Close detail modal.]**

#### 2:32 – 2:40 | Billing PDF (8s)
**[Click "Billing" → Open any receipt → Show pre-downloaded PDF.]**

> "PDF generation via **QuestPDF** — folios, receipts, invoices in **Admin, Front Desk, Guest** portals. Templated, branded, downloadable."

**[Close PDF.]**

#### 2:40 – 2:45 | Feedback Moderation (5s)
**[Click "Feedback" → Show one moderated card.]**

> "Guest feedback with **moderation queue** — approve/reject before public display. Protects brand. Images included."

**[3-finger swipe → Desktop 3 (Front Desk)]**

---

### 2:45 – 3:20 | DESKTOP 3: FRONT DESK (35s)

#### 2:45 – 3:00 | Today's Movements (15s)
**[Dashboard visible — "Today's Movements" section.]**

> "Front desk starts here. **Arrivals & departures for today** — room, guest, status, special requests. **Pre-shift clarity**. No surprise rushes. Housekeeping knows exactly which rooms turn over when."

#### 3:00 – 3:20 | Global Guest Search (20s)
**[Click search icon → Type "john" → Results appear instantly.]**

> "Phone rings. Guest says 'I'm John, I need to extend.' **Global search** — email, name, phone, booking ID. **Instant results** across all bookings. Click → check-in, extend, cancel, folio — **one context, zero navigation.** Built for the overwhelmed front desk."

**[3-finger swipe → Desktop 4 (Ops)]**

---

### 3:20 – 3:55 | DESKTOP 4: OPERATIONS (35s)

#### 3:20 – 3:35 | SignalR Proof — Live from ATLAS (15s)
**[Kitchen dashboard open. Show the burger order from ATLAS.]**

> "Remember the burger from ATLAS? **Here it is** — Kitchen dashboard, **real-time SignalR push**. No refresh. Order #1047, Room 304, Burger + Fries. Kitchen taps 'Preparing' → status syncs everywhere."

**[Click "Preparing" → status changes.]**

#### 3:35 – 3:55 | Kitchen Disable Menu Item (20s)
**[Click "Menu Management" → Toggle "Lobster Thermidor" OFF.]**

> "Lobster's out. **Toggle OFF** → instantly **grayed out on guest menu, room service, ATLAS suggestions**. Guest *cannot* order unavailable items. No more 'we'll check with kitchen' callbacks. **Single source of truth, instant propagation.**"

**[Switch back to Guest portal (Desktop 1) for 2s to show grayed-out item — optional if time.]**

---

### 3:55 – 4:15 | TECH DEPTH SLIDE (20s) — No Demo, Just Talk
**[Optional: Quick slide or verbal. No desktop switch.]**

> "What you didn't see: **Magic-byte image validation** (JPEG/PNG/WebP signatures, not extensions). **End-to-end idempotency** on every mutation. **Rate limiting** — global, image upload, ATLAS token bucket. **BCrypt + JWT + SignalR token auth**. **47 EF migrations**. **29 unit tests, 93% BLL coverage**. **Bicep IaC** — zero-click deploy."

---

### 4:15 – 5:00 | CLOSE (45s) — Back to Center Stage

> **4:15** — "Aetheris proves **luxury hospitality doesn't need legacy PMS compromise**. AI that acts. Staff dashboards that breathe. Audit trails that write themselves. Infrastructure that deploys itself."
>
> **4:25** — "This is **production code**. Deployed. Tested. Secured. The architecture scales from boutique to brand."
>
> **4:35** — "Three things to remember:"
>
> > **1. ATLAS executes** — not suggests. Real orders. Real tickets. Real alerts.
> >
> > **2. Real-time is default** — SignalR everywhere. No polling. No staleness.
> >
> > **3. Zero-touch observability** — every change audited, every upload validated, every retry idempotent.
>
> **4:50** — "Deployed at `hotel-web-demo1.ambitiousmushroom-274454dc.centralindia.azurecontainerapps.io`. Credentials in your packet. **Happy to dive deeper in Q&A.**"
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
| **Sales** | "Guest conversion ↑ because ATLAS turns 'I want...' into confirmed revenue in 3 clicks." |
| **HR** | "Audit trail = compliance ready. Zero dev maintenance. Staff dashboards reduce burnout." |
| **Architect** | "Clean N-tier. Domain services reusable. EF Core only in DAL. Swappable." |
| **Cloud Engineer** | "Azure-native: Container Apps, Managed PostgreSQL, Blob+Queue, SignalR Service. Bicep IaC." |
| **AI Director** | "Function calling with proposals + confirmations. Guest-scoped. Sanitized. Audited." |
| **Delivery** | "Generic CRUD component powers 8 management pages. New entity = config, not code." |

---

*Script v1 — Created for internship capstone presentation*