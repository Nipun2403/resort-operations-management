# Emergency Ticket Feature — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add emergency flag to maintenance & housekeeping tickets (not kitchen). Emergency tickets sort to top within their status group. Red badge shown in all dashboards.

**Architecture:** Boolean `IsEmergency` field on `MaintenanceTask` and `Housekeeping` entities. Backend default sort: `IsEmergency DESC, CreatedAt ASC`. Frontend forms add checkbox, display adds red badge column.

**Tech Stack:** .NET 8 + EF Core (Backend), Angular 17 + Angular Material (Frontend)

## Global Constraints

- Kitchen/food-order code never touched
- Emergency sorting: backend = `IsEmergency DESC, CreatedAt ASC` (staff FIFO); MyRequests client = `isEmergency DESC, createdAt DESC` (user newest-first)
- Red badge uses warn/red color scheme matching current theme
- All Angular components are standalone

---

### Task 1: Backend Entities + DTOs

**Files:**
- Modify: `Backend/HotelManagement.DAL/Entities/MaintenanceTask.cs`
- Modify: `Backend/HotelManagement.DAL/Entities/Housekeeping.cs`
- Modify: `Backend/HotelManagement.BLL/DTOs/MaintenanceDTOs.cs`
- Modify: `Backend/HotelManagement.BLL/DTOs/HousekeepingDTOs.cs`

**Interfaces:**
- Consumes: None
- Produces: `MaintenanceTask.IsEmergency`, `Housekeeping.IsEmergency`, DTOs with `IsEmergency`

- [ ] **Step 1: Add IsEmergency to MaintenanceTask**

Edit `MaintenanceTask.cs` — add property after `CreatedAt`:

```csharp
public bool IsEmergency { get; set; }
```

- [ ] **Step 2: Add IsEmergency to Housekeeping**

Edit `Housekeeping.cs` — add property after `CreatedAt`:

```csharp
public bool IsEmergency { get; set; }
```

- [ ] **Step 3: Add IsEmergency to Maintenance DTOs**

`MaintenanceDTOs.cs` — add to `MaintenanceTaskDTO`:
```csharp
public bool IsEmergency { get; set; }
```

Add to `CreateMaintenanceTaskDTO`:
```csharp
public bool IsEmergency { get; set; }
```

Add to `CreateInternalMaintenanceTaskDTO`:
```csharp
public bool IsEmergency { get; set; }
```

- [ ] **Step 4: Add IsEmergency to Housekeeping DTOs**

`HousekeepingDTOs.cs` — add to `HousekeepingDTO`:
```csharp
public bool IsEmergency { get; set; }
```

Add to `CreateHousekeepingTaskDTO`:
```csharp
public bool IsEmergency { get; set; }
```

Add to `CreateInternalHousekeepingTaskDTO`:
```csharp
public bool IsEmergency { get; set; }
```

- [ ] **Step 5: Verify build**

```bash
cd Backend/HotelManagement.API
dotnet build
```
Expected: Build succeeds (AutoMapper maps by convention, no MappingProfile change needed).

- [ ] **Step 6: Commit**

```bash
git add Backend/HotelManagement.DAL/Entities/MaintenanceTask.cs \
        Backend/HotelManagement.DAL/Entities/Housekeeping.cs \
        Backend/HotelManagement.BLL/DTOs/MaintenanceDTOs.cs \
        Backend/HotelManagement.BLL/DTOs/HousekeepingDTOs.cs
git commit -m "feat: add IsEmergency to maintenance/housekeeping entities and DTOs"
```

---

### Task 2: Backend EF Migration

**Files:**
- Generate: EF migration files (automatic)

**Interfaces:**
- Consumes: `MaintenanceTask.IsEmergency`, `Housekeeping.IsEmergency`
- Produces: Database columns `IsEmergency` (bit, not null, default 0)

- [ ] **Step 1: Generate migration**

```bash
cd Backend/HotelManagement.DAL
dotnet ef migrations add AddIsEmergencyFields --startup-project ../HotelManagement.API
```

- [ ] **Step 2: Verify migration SQL is correct**

Check the generated migration — should contain:
```csharp
migrationBuilder.AddColumn<bool>(
    name: "IsEmergency",
    table: "MaintenanceTasks",
    type: "bit",
    nullable: false,
    defaultValue: false);

migrationBuilder.AddColumn<bool>(
    name: "IsEmergency",
    table: "HousekeepingTasks",
    type: "bit",
    nullable: false,
    defaultValue: false);
```

- [ ] **Step 3: Apply migration (dev)**

```bash
cd Backend/HotelManagement.DAL
dotnet ef database update --startup-project ../HotelManagement.API
```

- [ ] **Step 4: Commit**

```bash
git add Backend/HotelManagement.DAL/Migrations/
git commit -m "feat: add IsEmergency migration for MaintenanceTasks and HousekeepingTasks"
```

---

### Task 3: Backend Sorting + ThenByDynamic Extension

**Files:**
- Create: `Backend/HotelManagement.Repository/Utilities/QueryableExtensions.cs` (append ThenByDynamic)
- Modify: `Backend/HotelManagement.BLL/Services/MaintenanceService.cs:56-60,103-107`
- Modify: `Backend/HotelManagement.BLL/Services/HousekeepingService.cs:139-143,185-189`

**Interfaces:**
- Consumes: `MaintenanceTask.IsEmergency`, `Housekeeping.IsEmergency`
- Produces: API responses sorted `IsEmergency DESC` primary, `CreatedAt ASC` secondary

- [ ] **Step 1: Add ThenByDynamic extension**

Append to `Backend/HotelManagement.Repository/Utilities/QueryableExtensions.cs`:

```csharp
public static IOrderedQueryable<T> ThenByDynamic<T>(this IOrderedQueryable<T> source, string propertyName, bool descending)
{
    if (string.IsNullOrWhiteSpace(propertyName))
        throw new ArgumentException("Property name cannot be null or empty.");

    var parameter = Expression.Parameter(typeof(T), "x");
    var property = typeof(T).GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                            .FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));

    if (property == null)
        throw new ArgumentException($"Property '{propertyName}' does not exist on type '{typeof(T).Name}'.");

    var propertyAccess = Expression.MakeMemberAccess(parameter, property);
    var orderByExpression = Expression.Lambda(propertyAccess, parameter);

    var methodName = descending ? "ThenByDescending" : "ThenBy";

    var resultExpression = Expression.Call(
        typeof(Queryable),
        methodName,
        new Type[] { typeof(T), property.PropertyType },
        source.Expression,
        Expression.Quote(orderByExpression)
    );

    return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(resultExpression);
}
```

- [ ] **Step 2: Update MaintenanceService.GetAllTasksAsync sort**

Edit lines 56-60. Change from:
```csharp
Func<IQueryable<MaintenanceTask>, IOrderedQueryable<MaintenanceTask>>? orderBy = null;
if (!string.IsNullOrEmpty(sortBy))
{
    orderBy = q => q.OrderByDynamic(sortBy, sortDescending);
}
```

To:
```csharp
Func<IQueryable<MaintenanceTask>, IOrderedQueryable<MaintenanceTask>>? orderBy = null;
if (!string.IsNullOrEmpty(sortBy))
{
    orderBy = q => q.OrderByDescending(t => t.IsEmergency).ThenByDynamic(sortBy, sortDescending);
}
else
{
    orderBy = q => q.OrderByDescending(t => t.IsEmergency).ThenBy(t => t.CreatedAt);
}
```

- [ ] **Step 3: Update MaintenanceService.GetActiveTasksAsync sort**

Edit lines 103-107. Same change as Step 2.

- [ ] **Step 4: Update HousekeepingService.GetAllAsync sort**

Edit lines 139-143:
```csharp
Func<IQueryable<Housekeeping>, IOrderedQueryable<Housekeeping>>? orderBy = null;
if (!string.IsNullOrEmpty(sortBy))
{
    orderBy = q => q.OrderByDescending(t => t.IsEmergency).ThenByDynamic(sortBy, sortDescending);
}
else
{
    orderBy = q => q.OrderByDescending(t => t.IsEmergency).ThenBy(t => t.CreatedAt);
}
```

- [ ] **Step 5: Update HousekeepingService.GetActiveAsync sort**

Edit lines 185-189. Same change as Step 4.

- [ ] **Step 5: CreateTicketAsync — pass IsEmergency from DTO**

Edit `MaintenanceService.cs` line 187. Change task creation to include IsEmergency:
```csharp
var task = new MaintenanceTask
{
    RoomId = roomId,
    Location = $"Room {roomExists.RoomNumber}",
    OriginType = originType,
    Status = MaintenanceStatus.Pending,
    Description = dto.Description,
    IsEmergency = dto.IsEmergency
};
```

- [ ] **Step 6: CreateInternalTicketAsync — pass IsEmergency**

Edit `MaintenanceService.cs` lines 204-211 to add `IsEmergency = dto.IsEmergency`.

- [ ] **Step 7: CreateGuestTriggerAsync — pass IsEmergency in Housekeeping**

Edit `HousekeepingService.cs` lines 77-84 to add `IsEmergency = dto.IsEmergency`.

- [ ] **Step 8: CreateInternalTriggerAsync — pass IsEmergency in Housekeeping**

Edit `HousekeepingService.cs` lines 96-103 to add `IsEmergency = dto.IsEmergency`.

- [ ] **Step 9: Verify build**

```bash
cd Backend/HotelManagement.API
dotnet build
```

- [ ] **Step 10: Commit**

```bash
git add Backend/HotelManagement.BLL/Services/MaintenanceService.cs \
        Backend/HotelManagement.BLL/Services/HousekeepingService.cs
git commit -m "feat: sort emergency tickets first in maintenance/housekeeping services"
```

---

### Task 4: Frontend TypeScript Models

**Files:**
- Modify: `Frontend/src/app/features/admin/models/maintenance-task.model.ts`
- Modify: `Frontend/src/app/features/admin/models/housekeeping-task.model.ts`
- Modify: `Frontend/src/app/shared/models/task.model.ts`
- Modify: `Frontend/src/app/features/user/models/customer-request.model.ts`
- Modify: `Frontend/src/app/features/admin/models/create-internal-ticket-request.dto.ts`

**Interfaces:**
- Consumes: Backend API response includes `isEmergency`
- Produces: All frontend models have `isEmergency` field

- [ ] **Step 1: Update MaintenanceTask model**

`maintenance-task.model.ts` — add after `location`:
```typescript
isEmergency: boolean;
```

- [ ] **Step 2: Update HousekeepingTask model**

`housekeeping-task.model.ts` — add after `location`:
```typescript
isEmergency: boolean;
```

- [ ] **Step 3: Update shared Task model**

`task.model.ts` `Task` interface — add after `createdAt`:
```typescript
isEmergency: boolean;
```

- [ ] **Step 4: Update CustomerRequest model**

`customer-request.model.ts` — add after `createdAt`:
```typescript
isEmergency: boolean;
```

- [ ] **Step 5: Update CreateInternalTicketRequest DTO**

`create-internal-ticket-request.dto.ts`:
```typescript
export interface CreateInternalTicketRequest {
  location: string;
  description: string;
  isEmergency?: boolean;
}
```

- [ ] **Step 6: Verify frontend build**

```bash
cd Frontend
npx tsc --noEmit 2>&1 | head -50
```
Expected: No type errors for missing `isEmergency`.

- [ ] **Step 7: Commit**

```bash
git add Frontend/src/app/features/admin/models/maintenance-task.model.ts \
        Frontend/src/app/features/admin/models/housekeeping-task.model.ts \
        Frontend/src/app/shared/models/task.model.ts \
        Frontend/src/app/features/user/models/customer-request.model.ts \
        Frontend/src/app/features/admin/models/create-internal-ticket-request.dto.ts
git commit -m "feat: add isEmergency to frontend TypeScript models"
```

---

### Task 5: Frontend Forms — Front Desk (3 components)

**Files:**
- Modify: `Frontend/src/app/features/front-desk/components/booking-action-modal/internal-ticket-panel/internal-ticket-panel.component.ts`
- Modify: `Frontend/src/app/features/front-desk/components/booking-action-modal/internal-ticket-panel/internal-ticket-panel.component.html`
- Modify: `Frontend/src/app/features/front-desk/components/booking-action-modal/maintenance-request-panel/maintenance-request-panel.component.ts`
- Modify: `Frontend/src/app/features/front-desk/components/booking-action-modal/maintenance-request-panel/maintenance-request-panel.component.html`
- Modify: `Frontend/src/app/features/front-desk/components/booking-action-modal/housekeeping-request-panel/housekeeping-request-panel.component.ts`
- Modify: `Frontend/src/app/features/front-desk/components/booking-action-modal/housekeeping-request-panel/housekeeping-request-panel.component.html`

**Interfaces:**
- Consumes: API services accept `isEmergency` in body
- Produces: Front desk can mark tickets as emergency

- [ ] **Step 1: InternalTicketPanelComponent — add checkbox form control**

`internal-ticket-panel.component.ts`:
- Add `MatCheckboxModule` to imports array
- Add `emergency` control to form group:

```typescript
form = new FormGroup({
    type: new FormControl<'housekeeping' | 'maintenance'>('housekeeping', Validators.required),
    location: new FormControl('', [Validators.required, Validators.maxLength(200)]),
    description: new FormControl('', [Validators.required, Validators.minLength(5)]),
    emergency: new FormControl(false, { nonNullable: true }),
});
```

- Update `submit()` — destructure and pass `emergency`:
```typescript
const { type, location, description, emergency } = this.form.value;
const body = { location: location!, description: description!, isEmergency: emergency! };
```

- [ ] **Step 2: InternalTicketPanelComponent — add checkbox to HTML**

`internal-ticket-panel.component.html` — add after description textarea, before closing `</form>`:
```html
<div class="field-group">
  <mat-checkbox formControlName="emergency" color="warn">
    <span style="color: #f44336; font-weight: 500;">Emergency</span>
  </mat-checkbox>
</div>
```

- [ ] **Step 3: MaintenanceRequestPanelComponent — add checkbox**

`maintenance-request-panel.component.ts`:
- Add `MatCheckboxModule` to imports
- Add form control:
```typescript
isEmergency = new FormControl(false, { nonNullable: true });
```
- Update `submit()` API call: pass `{ description: this.description.value, isEmergency: this.isEmergency.value }`

`maintenance-request-panel.component.html` — add before submit button:
```html
<mat-checkbox [formControl]="isEmergency" color="warn" style="display: block; margin-bottom: 12px;">
  <span style="color: #f44336; font-weight: 500;">Emergency</span>
</mat-checkbox>
```

- [ ] **Step 4: HousekeepingRequestPanelComponent — add checkbox**

`housekeeping-request-panel.component.ts`:
- Add `MatCheckboxModule` to imports
- Add form control and update API call (same pattern as Step 3)

`housekeeping-request-panel.component.html` — same checkbox as Step 3 before submit button.

- [ ] **Step 5: Verify frontend build**

```bash
cd Frontend
npx tsc --noEmit 2>&1 | head -50
```
Expected: No errors.

- [ ] **Step 6: Commit**

```bash
git add Frontend/src/app/features/front-desk/components/booking-action-modal/internal-ticket-panel/ \
        Frontend/src/app/features/front-desk/components/booking-action-modal/maintenance-request-panel/ \
        Frontend/src/app/features/front-desk/components/booking-action-modal/housekeeping-request-panel/
git commit -m "feat: add Emergency checkbox to front desk ticket forms"
```

---

### Task 6: Frontend Forms — User (2 components)

**Files:**
- Modify: `Frontend/src/app/features/user/components/request-service/request-service.component.ts`
- Modify: `Frontend/src/app/features/user/components/request-service/request-service.component.html`
- Modify: `Frontend/src/app/features/user/components/request-service-dialog.component.ts`
- Modify: `Frontend/src/app/features/user/components/request-service-dialog.component.html`

**Interfaces:**
- Consumes: API services accept `isEmergency` in body
- Produces: Users can mark their requests as emergency

- [ ] **Step 1: RequestServiceComponent — add checkbox**

`request-service.component.ts`:
- Add `MatCheckboxModule` to imports
- Add emergency control:
```typescript
isEmergency = new FormControl(false, { nonNullable: true });
```
- Update `performSubmit()`: pass `{ description: desc, isEmergency: this.isEmergency.value }`

- [ ] **Step 2: RequestServiceComponent — checkbox in HTML**

`request-service.component.html` — add before `.actions-row`:
```html
<div class="form-row">
  <mat-checkbox [formControl]="isEmergency" color="warn">
    <span style="color: #f44336; font-weight: 500;">Emergency</span>
  </mat-checkbox>
</div>
```

- [ ] **Step 3: RequestServiceDialogComponent — add checkbox**

`request-service-dialog.component.ts`:
- Add `MatCheckboxModule` to imports
- Add form control:
```typescript
isEmergencyControl = new FormControl(false, { nonNullable: true });
```
- Update `submit()` result to include `isEmergency`:
```typescript
export interface RequestServiceDialogResult {
  description: string;
  isEmergency: boolean;
}
// ...
const result: RequestServiceDialogResult = { description: this.descriptionControl.value, isEmergency: this.isEmergencyControl.value };
```

- [ ] **Step 4: RequestServiceDialogComponent — checkbox in HTML**

`request-service-dialog.component.html` — add after description textarea, before `</mat-dialog-content>`:
```html
<mat-checkbox [formControl]="isEmergencyControl" color="warn" style="margin-bottom: 8px;">
  <span style="color: #f44336; font-weight: 500;">Emergency</span>
</mat-checkbox>
```

- [ ] **Step 5: Update caller to pass isEmergency**

`dashboard.component.ts` `openServiceRequest()` — update API call (line 239):
```typescript
const api$ = type === 'housekeeping'
  ? this.housekeepingApi.trigger(roomId, { description: result.description, isEmergency: result.isEmergency })
  : this.maintenanceApi.trigger(roomId, { description: result.description, isEmergency: result.isEmergency });
```

- [ ] **Step 6: Update user API services' trigger method signatures**

`user/services/housekeeping-api.service.ts` and `user/services/maintenance-api.service.ts` — change:
```typescript
trigger(roomId: number, body: { description: string; isEmergency?: boolean }): Observable<void> {
```

- [ ] **Step 7: Verify frontend build**

```bash
cd Frontend
npx tsc --noEmit 2>&1 | head -50
```
Expected: No errors.

- [ ] **Step 8: Commit**

```bash
git add Frontend/src/app/features/user/components/request-service/ \
        Frontend/src/app/features/user/components/request-service-dialog.component.ts \
        Frontend/src/app/features/user/components/request-service-dialog.component.html \
        Frontend/src/app/features/user/services/housekeeping-api.service.ts \
        Frontend/src/app/features/user/services/maintenance-api.service.ts \
        Frontend/src/app/features/user/pages/dashboard.component.ts
git commit -m "feat: add Emergency checkbox to user ticket forms"
```

---

### Task 7: Shared Task Dashboard — Emergency Badge Column

**Files:**
- Modify: `Frontend/src/app/shared/components/task-dashboard/task-dashboard.component.ts`
- Modify: `Frontend/src/app/shared/components/task-dashboard/task-dashboard.component.html`

**Interfaces:**
- Consumes: `Task.isEmergency`
- Produces: Red emergency badge column in maintenance & housekeeping dashboards

- [ ] **Step 1: Add column to displayedColumns**

`task-dashboard.component.ts` — insert `'isEmergency'` before `'actions'`:
```typescript
displayedColumns = ['id', 'location', 'description', 'status', 'isEmergency', 'actions'];
```

- [ ] **Step 2: Add column template to HTML**

`task-dashboard.component.html` — add after status column (after closing `</ng-container>` of `status`):
```html
<ng-container matColumnDef="isEmergency">
  <th mat-header-cell *matHeaderCellDef class="text-center-header">URGENT</th>
  <td mat-cell *matCellDef="let t" class="text-center-cell">
    @if (t.isEmergency) {
    <span class="emergency-badge">Emergency</span>
    }
  </td>
</ng-container>
```

- [ ] **Step 3: Add emergency CSS**

`task-dashboard.component.scss` (create or append):
```scss
.emergency-badge {
  display: inline-block;
  background-color: #f44336;
  color: #fff;
  font-size: 11px;
  font-weight: 600;
  padding: 2px 8px;
  border-radius: 12px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}
```

If no SCSS file exists, add inline style to the span instead: `style="background:#f44336;color:#fff;font-size:11px;font-weight:600;padding:2px 8px;border-radius:12px;text-transform:uppercase;"`

- [ ] **Step 4: Verify frontend build**

```bash
cd Frontend
npx tsc --noEmit 2>&1 | head -50
```

- [ ] **Step 5: Commit**

```bash
git add Frontend/src/app/shared/components/task-dashboard/
git commit -m "feat: add emergency badge column to task dashboard"
```

---

### Task 8: Front Desk Ticket List — Emergency Badge Column

**Files:**
- Modify: `Frontend/src/app/features/front-desk/components/ticket-list/ticket-list.component.ts`
- Modify: `Frontend/src/app/features/front-desk/components/ticket-list/ticket-list.component.html`

**Interfaces:**
- Consumes: API response includes `isEmergency`
- Produces: Front desk active tickets show emergency badge

- [ ] **Step 1: Track isEmergency in normalized data**

`ticket-list.component.ts` — update normalization block (lines 121-128). `isEmergency` is already spread via `...t` so it flows through automatically if present in API response. Verify data mapping preserves it.

- [ ] **Step 2: Add column to displayed columns**

Change line 107 and line 111 — replace hardcoded `['id','room','description','status','createdAt']` with a class field:
```typescript
displayedColumns = ['id', 'room', 'description', 'status', 'createdAt'];
```

Change to:
```typescript
displayedColumns = ['id', 'room', 'description', 'status', 'isEmergency', 'createdAt'];
```

- [ ] **Step 3: Add emergency column HTML**

`ticket-list.component.html` — add after status column:
```html
<!-- Emergency Column -->
<ng-container matColumnDef="isEmergency">
  <th mat-header-cell *matHeaderCellDef>Urgent</th>
  <td mat-cell *matCellDef="let t">
    @if (t.isEmergency) {
    <span style="background:#f44336;color:#fff;font-size:11px;font-weight:600;padding:2px 8px;border-radius:12px;">Emergency</span>
    }
  </td>
</ng-container>
```

- [ ] **Step 4: Update header and row definitions**

Update the `mat-header-row-def` and `mat-row-def` to use `displayedColumns`:
```html
<tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
<tr mat-row *matRowDef="let row; columns: displayedColumns"></tr>
```

- [ ] **Step 5: Verify frontend build**

```bash
cd Frontend
npx tsc --noEmit 2>&1 | head -50
```

- [ ] **Step 6: Commit**

```bash
git add Frontend/src/app/features/front-desk/components/ticket-list/
git commit -m "feat: add emergency badge column to front desk ticket list"
```

---

### Task 9: User Dashboard & My Requests

**Files:**
- Modify: `Frontend/src/app/features/user/pages/dashboard.component.html`
- Modify: `Frontend/src/app/features/user/components/my-requests/my-requests.component.ts`
- Modify: `Frontend/src/app/features/user/components/my-requests/my-requests.component.html`

**Interfaces:**
- Consumes: `CustomerRequest.isEmergency`
- Produces: Pulse of Service shows emergency badge per item; MyRequests table has badge column + sort

- [ ] **Step 1: Pulse of Service — add emergency badge to housekeeping items**

`dashboard.component.html` lines 117-122. Change:
```html
<div class="request-item">
  <span class="request-desc">{{ item.description || 'No description' }}</span>
  <div class="request-status">
    <span class="status-dot" [class]="item.status.toLowerCase()"></span>
    <span class="status-label" [class]="item.status.toLowerCase()">{{ item.status }}</span>
  </div>
</div>
```

To:
```html
<div class="request-item">
  <span class="request-desc">{{ item.description || 'No description' }}</span>
  @if (item.isEmergency) {
  <span class="emergency-pill">Emergency</span>
  }
  <div class="request-status">
    <span class="status-dot" [class]="item.status.toLowerCase()"></span>
    <span class="status-label" [class]="item.status.toLowerCase()">{{ item.status }}</span>
  </div>
</div>
```

- [ ] **Step 2: Same for maintenance items**

`dashboard.component.html` lines 138-143 — same change as Step 1.

- [ ] **Step 3: Add emergency pill CSS**

`dashboard.component.scss` — add:
```scss
.emergency-pill {
  display: inline-block;
  background-color: #f44336;
  color: #fff;
  font-size: 10px;
  font-weight: 600;
  padding: 1px 6px;
  border-radius: 10px;
  text-transform: uppercase;
  margin-right: 8px;
}
```

- [ ] **Step 4: MyRequestsComponent — map isEmergency from API**

`my-requests.component.ts` — update `fetchRequests()` data mapping. For housekeeping (lines 75-83), the spread `...hk` already passes `isEmergency` through. Same for maintenance (lines 93-101). The `isEmergency` field flows automatically since `hk` now includes it from the API.

- [ ] **Step 5: MyRequestsComponent — update sort**

Change line 143 from:
```typescript
merged.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
```

To:
```typescript
merged.sort((a, b) => {
  if (a.isEmergency !== b.isEmergency) {
    return a.isEmergency ? -1 : 1; // emergency first
  }
  return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(); // then newest first
});
```

- [ ] **Step 6: MyRequestsComponent — add isEmergency column**

`my-requests.component.ts` — add to `displayedColumns`:
```typescript
displayedColumns = ['type', 'room', 'description', 'status', 'isEmergency', 'createdAt'];
```

- [ ] **Step 7: MyRequestsComponent — add emergency column HTML**

`my-requests.component.html` — add after status column, before created column:
```html
<!-- Emergency Column -->
<ng-container matColumnDef="isEmergency">
  <th mat-header-cell *matHeaderCellDef class="header-cell">Urgent</th>
  <td mat-cell *matCellDef="let r" class="status-cell">
    @if (r.isEmergency) {
    <span style="background:#f44336;color:#fff;font-size:11px;font-weight:600;padding:2px 8px;border-radius:12px;">Emergency</span>
    }
  </td>
</ng-container>
```

- [ ] **Step 8: Verify frontend build**

```bash
cd Frontend
npx tsc --noEmit 2>&1 | head -50
```
Expected: No errors.

- [ ] **Step 9: Commit**

```bash
git add Frontend/src/app/features/user/pages/dashboard.component.html \
        Frontend/src/app/features/user/pages/dashboard.component.scss \
        Frontend/src/app/features/user/components/my-requests/
git commit -m "feat: add emergency badge to user dashboard and my-requests"
```
