# Emergency Ticket Feature

## Summary

Add an "Emergency" boolean flag to maintenance and housekeeping tickets (not kitchen orders). Emergency tickets sort to the top within their status group in all portals. A red badge indicates emergency tickets at a glance.

## Scope

- **Included:** Maintenance tickets, Housekeeping tickets — both internal (staff) and user-generated (guest)
- **Excluded:** Food orders / kitchen orders entirely

## Backend Changes

### Entities (`HotelManagement.DAL`)

| File | Change |
|------|--------|
| `Entities/MaintenanceTask.cs` | Add `public bool IsEmergency { get; set; }` (default `false`) |
| `Entities/Housekeeping.cs` | Add `public bool IsEmergency { get; set; }` (default `false`) |
| EF Migration | `AddIsEmergencyToMaintenanceTasks`, `AddIsEmergencyToHousekeepingTasks` |

### DTOs (`HotelManagement.BLL`)

| File | Change |
|------|--------|
| `DTOs/MaintenanceDTOs.cs` | Add `IsEmergency` to `MaintenanceTaskDTO`, `CreateMaintenanceTaskDTO`, `CreateInternalMaintenanceTaskDTO` |
| `DTOs/HousekeepingDTOs.cs` | Add `IsEmergency` to `HousekeepingDTO`, `CreateHousekeepingTaskDTO`, `CreateInternalHousekeepingTaskDTO` |

### Services (`HotelManagement.BLL`)

| File | Change |
|------|--------|
| `Services/MaintenanceService.cs` | Default sort: `OrderByDescending(t => t.IsEmergency).ThenBy(t => t.CreatedAt)`. Apply when `sortBy` is null/empty in `GetAllTasksAsync` and `GetActiveTasksAsync`. |
| `Services/HousekeepingService.cs` | Same default sort in `GetAllAsync` and `GetActiveAsync`. |

Sort rule: Emergency tickets first, then oldest `CreatedAt` first (FIFO). The `sortBy`/`sortDescending` params override only the secondary sort (emergency always primary).

### Mapping

AutoMapper `MappingProfile.cs` — `IsEmergency` should auto-map by convention (same name in entity and DTO). Verify.

## Frontend Models

| File | Change |
|------|--------|
| `admin/models/maintenance-task.model.ts` | Add `isEmergency: boolean` |
| `admin/models/housekeeping-task.model.ts` | Add `isEmergency: boolean` |
| `shared/models/task.model.ts` | Add `isEmergency: boolean` |
| `user/models/customer-request.model.ts` | Add `isEmergency: boolean` |
| `admin/models/create-internal-ticket-request.dto.ts` | Add `isEmergency?: boolean` |

## Frontend Forms (add "Emergency" checkbox)

All five creation modals get a single `mat-checkbox` labeled "Emergency" above/before the submit button. The checkbox value is passed through to the API body as `isEmergency: true`.

| Component | Files | Notes |
|-----------|-------|-------|
| `internal-ticket-panel` | `.ts`, `.html` | Front desk — type radio (housekeeping/maintenance) |
| `maintenance-request-panel` | `.ts`, `.html` | Front desk — room + description |
| `housekeeping-request-panel` | `.ts`, `.html` | Front desk — room + description |
| `request-service` | `.ts`, `.html` | User — type toggle + room + description |
| `request-service-dialog` | `.ts`, `.html` | User — description only |

## Frontend Display (red badge)

### TaskDashboard (shared — maintenance portal, housekeeping portal)

- Add `isEmergency` column definition in `.ts`
- Add column to `displayedColumns`
- Add `<td>` in `.html` with red badge styled span

### TicketList (front-desk active tickets)

- Add `isEmergency` column with red badge

### User Dashboard "Pulse of Service"

- In `dashboard.component.html`, add red badge next to each housekeeping/maintenance item when `item.isEmergency` is true

### MyRequests (user room service)

- Add `isEmergency` column with red badge
- Map `isEmergency` in the API response → `CustomerRequest` spread

## Frontend Sorting

| Component | Sort | Type |
|-----------|------|------|
| TaskDashboard (maintenance) | Backend handles: `IsEmergency DESC, CreatedAt ASC` | server-side |
| TaskDashboard (housekeeping) | Backend handles | server-side |
| TicketList | Backend handles | server-side |
| MyRequests | Client: `isEmergency DESC, createdAt DESC` | client-side |

## Kitchen Exclusion

No changes to:
- `FoodOrder.cs`, `FoodOrderDTOs.cs`, `OrderService.cs`
- `food-order-panel` (front desk)
- `food-order` (user)
- Kitchen dashboard
- `OrderApiService`
