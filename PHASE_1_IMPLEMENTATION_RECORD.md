# AI Concierge — Phase 1 (P0: Core Engine) Implementation Record

## Overview
This document records the complete implementation of Phase 1 for the AI Concierge feature, following the design in `AI_CONCIERGE_DESIGN.md`.

**Status**: ✅ Complete — Both backend and frontend compile successfully.

---

## Files Created (Backend: 25 new files)

### DAL Entities & Migration
| File | Purpose |
|------|---------|
| `HotelManagement.DAL/Entities/ConciergeActionLog.cs` | Audit log entity (separate from `AuditLog`) |
| `HotelManagement.DAL/Entities/ConversationMessage.cs` | Conversation persistence entity |
| `HotelManagement.DAL/Entities/ConciergeProposal.cs` | Pending proposals awaiting user confirmation |
| `HotelManagement.DAL/Migrations/20260713173820_AddConciergeTables.cs` | EF Core migration with indexes |

### Repository Layer (Interfaces + Implementations)
| File | Purpose |
|------|---------|
| `HotelManagement.Repository/Interfaces/IConciergeActionLogRepository.cs` | `AddAsync`, `GetByConversationAsync` |
| `HotelManagement.Repository/Implementations/ConciergeActionLogRepository.cs` | EF Core impl |
| `HotelManagement.Repository/Interfaces/IConversationRepository.cs` | `GetRecentAsync` (token-window), `AddRangeAsync` |
| `HotelManagement.Repository/Implementations/ConversationRepository.cs` | EF Core impl |
| `HotelManagement.Repository/Interfaces/IConciergeProposalRepository.cs` | `SaveAsync`, `GetByIdsAsync`, `MarkConfirmedAsync`, `CleanupExpiredAsync` |
| `HotelManagement.Repository/Implementations/ConciergeProposalRepository.cs` | EF Core impl with GUID-based queries |

### BLL — DTOs & Interfaces
| File | Purpose |
|------|---------|
| `HotelManagement.BLL/DTOs/ConciergeDTOs.cs` | All request/response/proposal/action DTOs |
| `HotelManagement.BLL/Interfaces/IConciergeService.cs` | `ProcessMessageAsync`, `ConfirmProposalsAsync`, `GetPendingProposalsAsync`, `GetGuestContextAsync` |
| `HotelManagement.BLL/Interfaces/IConciergeActionLogRepository.cs` | BLL-facing audit log interface |

### BLL — Core Services (Concierge folder)
| File | Purpose |
|------|---------|
| `HotelManagement.BLL/Services/Concierge/ConciergeService.cs` | Main orchestrator (500+ lines) — two-step tool pattern, LLM calls, validation, audit logging |
| `HotelManagement.BLL/Services/Concierge/PostgresConversationStore.cs` | Implements `IConversationStore` via repository |
| `HotelManagement.BLL/Services/Concierge/PostgresProposalStore.cs` | Implements `IProposalStore` via repository |
| `HotelManagement.BLL/Services/Concierge/ConciergeTools.cs` | 8 OpenAI function definitions (3 side-effect, 5 read-only) + `MaxToolCallsPerTurn = 5` |
| `HotelManagement.BLL/Services/Concierge/PromptBuilder.cs` | System prompt with guest context + two-step pattern instructions |
| `HotelManagement.BLL/Services/Concierge/ToolExecutor.cs` | Static dispatcher mapping tool names → service methods |
| `HotelManagement.BLL/Services/Concierge/InputSanitizer.cs` | Inline regex stripping `ignore previous instructions`, `system:`, `assistant:`, etc. |
| `HotelManagement.BLL/Services/Concierge/ConciergeToolArgs.cs` | Tool argument types (moved from nested classes) |

### BLL Options
| File | Purpose |
|------|---------|
| `HotelManagement.BLL/Options/OpenAIOptions.cs` | `{ ApiKey, Model }` |
| `HotelManagement.BLL/Options/ConciergeOptions.cs` | `{ MaxConversationTurns, ConversationTtlHours, RateLimitPerMinute, MaxToolCallsPerTurn, ProposalTtlMinutes }` |

### API Layer
| File | Purpose |
|------|---------|
| `HotelManagement.API/Controllers/ConciergeController.cs` | 4 endpoints with `[Idempotent]` attribute |

### Configuration
| File | Change |
|------|--------|
| `HotelManagement.API/Program.cs` | Added 6 scoped services, `OpenAIOptions`/`ConciergeOptions` config, rate limiter policy |

---

## Files Created (Frontend: 3 new files)

| File | Purpose |
|------|---------|
| `Frontend/src/app/features/user/services/concierge-api.service.ts` | Typed HTTP client with `chat()`, `confirm()`, `getContext()` |
| `Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.ts` | Signal-based chat component with two-step confirmation gate |
| `Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.html` | Template with proposal cards, action chips, quick actions, context bar |
| `Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.scss` | Styling for bubbles, proposal cards, chips |

---

## Key Design Decisions Implemented

| Decision | Implementation |
|----------|----------------|
| **Two-step tool pattern** | Side-effect tools (`CreateFoodOrder`, `CreateHousekeepingRequest`, `CreateMaintenanceTicket`) return proposals → user confirms via `POST /confirm` → action executes |
| **Per-turn idempotency** | `[Idempotent]` attribute on `POST /chat` and `POST /confirm`; frontend sends `X-Idempotency-Key: concierge:turn:{conversationId}:{turnNumber}` |
| **Conversation key scoping** | `concierge:conv:{userId}:{conversationId}` prevents cross-user conversation access |
| **Model name from config** | `_openAIOptions.Value.Model` (not hardcoded) |
| **Tool argument validation** | `ValidateFoodOrderArgsAsync` checks menu item existence + `IsAvailable` before `OrderService.CreateOrderAsync` |
| **Max tool calls/turn** | Hard cap of 5 enforced in `ConciergeService.ProcessMessageAsync` |
| **Audit trail** | Separate `ConciergeActionLog` table (not extending `AuditLog`) |
| **Conversation persistence** | PostgreSQL `conversation_messages` table; token-window retrieval (last 8 messages) |
| **Proposal TTL** | 5 minutes; `CleanupExpiredAsync` marks expired proposals |
| **Streaming** | Deferred to P3; plain JSON request/response for MVP |
| **Frontend logout cleanup** | `localStorage.removeItem('concierge_conversation_id')` on auth logout |

---

## API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| `POST` | `/api/v1/concierge/chat` | Process message, return proposals + read-only action results |
| `POST` | `/api/v1/concierge/confirm` | Execute confirmed proposals |
| `GET` | `/api/v1/concierge/proposals` | Get pending proposals for conversation |
| `GET` | `/api/v1/concierge/context` | Get guest context (booking, room, status) |

All mutation endpoints require `X-Idempotency-Key` header and `RegisteredUser` role.

---

## OpenAI Function Definitions (8 tools)

**Side-effect (require confirmation):**
1. `CreateFoodOrder` — items: `{menuItemId, quantity}[]`
2. `CreateHousekeepingRequest` — description, isEmergency
3. `CreateMaintenanceTicket` — description, isEmergency

**Read-only (execute immediately):**
4. `GetBookingInfo` — no args
5. `GetFolioBalance` — no args
6. `GetHousekeepingStatus` — no args
7. `GetMenuItems` — category?, search?, availableOnly?
8. `GetActiveOrders` — no args

---

## Verification

```bash
# Backend
cd Backend/HotelManagement.API && dotnet build --no-restore
# Build succeeded. 0 Warning(s), 0 Error(s)

# Frontend
cd Frontend && npm run build
# Build succeeded (bundle size warnings only)
```

---

## Next Phases (Deferred)

| Phase | Scope |
|-------|-------|
| **P1** | API hardening, rate limiting, proposal TTL background job |
| **P2** | Frontend polish: loading states, error toasts, accessibility |
| **P3** | SSE/SignalR streaming for token-by-token replies, menu carousel |
| **Post-MVP** | Proactive nudges, voice input, multilingual, PMS integration |

---

## Notes

- All tool argument types moved from nested classes to `ConciergeToolArgs.cs` to satisfy CA1034
- `ChatCompletionOptions` uses `MaxTokens` (not `MaxOutputTokens`) per OpenAI SDK v2.12
- `ChatToolCall` created via `ChatToolCall.CreateFunctionToolCall` factory
- `BookingDTO` uses `Rooms` (not `BookingRooms`) per existing DTO structure
- `HousekeepingService.GetActiveTasksAsync` fixed to use `Expression<Func<>>` correctly
- `MenuItemRepository.GetPaginatedMenuItemsAsync` signature aligned with 7-parameter interface