# Staff Task Limit Plan

Limit each Housekeeping/Maintenance staff to max 2 active (InProgress) tasks.

## Global Constraints

- No user has both Housekeeping and Maintenance roles
- Roles only see their own task type
- `AssignedToUserId` = nullable FK to `User.Id`
- `Housekeeping.Status` and `MaintenanceTask.Status` are string-enum conversions stored in DB
- AutoMapper convention maps `AssignedToUserId` automatically (no profile change needed)
- Frontend uses `HttpParams` for query params
- Shared `TaskDashboardComponent` drives both portals
- Max 2 tasks **per staff**, checked when transitioning Pending → InProgress

## Tasks

### Task 1: Backend entities + DbContext + DTOs

**Files:**
- `Backend/HotelManagement.DAL/Entities/Housekeeping.cs` — add:
  ```csharp
  public int? AssignedToUserId { get; set; }
  public User? AssignedToUser { get; set; }
  ```
- `Backend/HotelManagement.DAL/Entities/MaintenanceTask.cs` — add same fields
- `Backend/HotelManagement.DAL/Context/ApplicationDbContext.cs` — add FK configs after line 76 (after `HasConversion<string>()` for Housekeeping.Status):
  ```csharp
  modelBuilder.Entity<Housekeeping>()
      .HasOne(h => h.AssignedToUser)
      .WithMany()
      .HasForeignKey(h => h.AssignedToUserId)
      .OnDelete(DeleteBehavior.SetNull);

  modelBuilder.Entity<MaintenanceTask>()
      .HasOne(m => m.AssignedToUser)
      .WithMany()
      .HasForeignKey(m => m.AssignedToUserId)
      .OnDelete(DeleteBehavior.SetNull);
  ```
- `Backend/HotelManagement.BLL/DTOs/HousekeepingDTOs.cs` — add:
  ```csharp
  public int? AssignedToUserId { get; set; }
  ```
- `Backend/HotelManagement.BLL/DTOs/MaintenanceDTOs.cs` — add same:
  ```csharp
  public int? AssignedToUserId { get; set; }
  ```

### Task 2: Backend service interfaces + `assignedToMe` param

**Files:**
- `Backend/HotelManagement.BLL/Interfaces/IHousekeepingService.cs` — add `bool assignedToMe = false` to `GetAllAsync` signature
- `Backend/HotelManagement.BLL/Interfaces/IMaintenanceService.cs` — add `bool assignedToMe = false` to `GetAllTasksAsync` signature

### Task 3: Backend service logic — enforcement + filtering

**Files:**
- `Backend/HotelManagement.BLL/Services/HousekeepingService.cs`
- `Backend/HotelManagement.BLL/Services/MaintenanceService.cs`

**`UpdateStatusAsync` logic in both:**
```
if (status == InProgress && task.Status != InProgress):
  get current user by email
  count existing InProgress tasks assigned to that user
  if count >= 2: throw "You can only work on up to 2 tasks at a time."
  set task.AssignedToUserId = user.Id
if (status == Completed && task.Status == InProgress):
  set task.AssignedToUserId = null
```

**`GetAllAsync` / `GetAllTasksAsync` logic in both:**
```
add bool assignedToMe = false param
when assignedToMe && isStaff:
  get current user by email
  override filter to: AssignedToUserId == user.Id && Status == InProgress
```

### Task 4: Backend controllers — wire query param

**Files:**
- `Backend/HotelManagement.API/Controllers/HousekeepingController.cs` — add `[FromQuery] bool assignedToMe = false` to `GetAllTasks`, pass to service
- `Backend/HotelManagement.API/Controllers/MaintenanceController.cs` — same

### Task 5: Frontend API services + task model

**Files:**
- `Frontend/src/app/features/user/services/housekeeping-api.service.ts` — add `assignedToMe?: boolean` to `getAll` params, pass as http param
- `Frontend/src/app/features/user/services/maintenance-api.service.ts` — same
- `Frontend/src/app/shared/models/task.model.ts`:
  - Add `assignedToUserId?: number` to `Task`
  - Add `assignedToMe?: boolean` to `fetchTasks` params type

### Task 6: Frontend TaskDashboardComponent — toggle + counter

**File:** `Frontend/src/app/shared/components/task-dashboard/task-dashboard.component.ts`

Changes:
1. Add `showMyTasks = signal(false)` state
2. Add toggle button in template — "All Tasks" | "My Tasks"
3. When active: pass `assignedToMe: true`, force `status: 'InProgress'` in fetch params
4. Show "My Tasks (X/2)" badge (X = data.length)
5. Toggle resets page to 0

### Task 7: Frontend dashboard components — map field

**Files:**
- `Frontend/src/app/features/housekeeping/pages/dashboard.component.ts` — add `assignedToUserId: task.assignedToUserId` in Task mapping
- `Frontend/src/app/features/maintenance/pages/dashboard.component.ts` — same

### Task 8: Migration + verify

- `dotnet ef migrations add AddAssignedToUserId` from DAL project
- `dotnet build` backend
- Frontend build check
