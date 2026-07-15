# "Room That Knows You" Personalization Engine - Implementation Analysis

## Executive Summary
A predictive personalization engine that auto-generates housekeeping/maintenance tasks before guest check-in based on historical preferences inferred from booking history, orders, and audit logs.

## Time Estimate: ~16 days (3.5 weeks single dev / 2 weeks with 2 devs)

## Phase Breakdown

| Phase | Tasks | Est. Days |
|-------|-------|-----------|
| 1. Data Layer | 3 new entities (`GuestPreference`, `PreferenceInferenceLog`, `PersonalizationTask`), 1 migration, 1 repository + interface | 2 |
| 2. Core Engine | `IPersonalizationEngine` + `PersonalizationEngine` with rule-based + OpenAI fallback, preference inference from `AuditLog` + `FoodOrder` + `Booking` history | 3 |
| 3. Background Worker | `PreCheckInPersonalizationWorker` (runs 30min before check-in), creates Housekeeping/Maintenance tasks via existing services | 2 |
| 4. Service Integration | Wire into `BookingService.CheckInGuestAsync` → trigger worker, add `PersonalizationEngine` to `HousekeepingService`/`MaintenanceService`/`OrderService` | 2 |
| 5. Real-time Dashboard | SignalR `PersonalizationHub` + Admin Oversight page showing auto-created tasks with 🎯 "AI-Generated" badges | 2 |
| 6. Guest Preference UI | User portal: "My Preferences" page (explicit + inferred toggle), pre-arrival email with preference confirmation link | 2 |
| 7. Predictive Kitchen | `OrderService` integration: analyze order patterns → SignalR nudge to Kitchen 15min before predicted order time | 1.5 |
| 8. Tests & Polish | Unit tests (engine, worker), E2E test (check-in → tasks appear), seed data for demo | 1.5 |

## Key Architectural Decisions

| Decision | Rationale |
|----------|-----------|
| Rule-based first, OpenAI as fallback | Zero cold-start problem, explainable, cheaper, works Day 1 |
| Reuse `HousekeepingService.CreateInternalTriggerAsync` | No new task types — personalization tasks = staff-requested tasks with `OriginType = StaffRequested` + `Description` prefix "🤖 AI: " |
| Background worker, not synchronous | Check-in stays fast; personalization is async "bonus" |
| AuditLog as inference source | Already captures every entity change — mine it for "User ordered X 3×" patterns |
| New `GuestPreference` entity (not User extension) | Clean separation, GDPR-ready, supports multi-stay history |

## Demo Moments (Jaw-Dropping)

| Demo Moment | What Audience Sees |
|-------------|-------------------|
| **T-30min to check-in** | Admin dashboard: "🤖 AI preparing Room 304 for Sarah Chen" — 4 tasks auto-created (minibar, thermostat, yoga mat, TV welcome) |
| **Guest walks in** | Room already at 21°C, favorite sparkling water in minibar, "Welcome Sarah! Your 7:30 AM latte will be ready" on TV |
| **Morning of Day 2** | Kitchen tablet: "🤖 Predictive: Sarah usually orders latte at 7:32 AM — prep starting now" |
| **Guest taps "Same as yesterday?"** | Push notification → one-tap reorder → kitchen gets instant SignalR alert |

## Risk Mitigation

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| Preference inference accuracy | Medium | Start with explicit preferences + high-confidence rules only (e.g., "ordered latte 3+ stays") |
| Staff ignoring AI tasks | Low | Tasks look identical to manual ones; "AI" badge is admin-only |
| OpenAI latency/cost | Low | Rule engine handles 80%+; OpenAI only for ambiguous cases |
| Thermostat/TV integration | High (if no IoT) | **Scope**: Tasks are *instructions to staff* ("Set thermostat to 21°C"), not direct IoT control — works Day 1 |

## Fastest Path to Demo (2-Week Sprint)

**Week 1 (Backend):**
- Days 1-2: Entities, migration, repository, engine skeleton
- Days 3-4: Rule engine + OpenAI client + inference from `AuditLog`/`FoodOrder`
- Days 5: Background worker + `BookingService` integration + Housekeeping/Maintenance task creation

**Week 2 (Frontend + Polish):**
- Days 1-2: Admin dashboard "AI Personalization" tab + SignalR hub
- Days 3: Guest preference page + pre-arrival email template
- Days 4: Predictive kitchen nudge + E2E test
- Day 5: Demo rehearsal + seed data (3 guests with rich histories)
