# Task Ownership Guard on Complete

**Date:** 2026-07-12
**Status:** Approved for implementation

## Problem

Staff can complete InProgress tasks that aren't assigned to them. The `UpdateStatusAsync`
methods in both `HousekeepingService` and `MaintenanceService` resolve the current user
and check `AssignedToUserId` on **Pending→InProgress** (2-task limit), but the
**InProgress→Completed** transition only nulls out `AssignedToUserId` without any
ownership verification.

A staff user can call `PATCH /api/housekeeping/{id}/status { "status": "Completed" }`
and complete another staff member's task.

## Root Cause

- `HousekeepingService.cs:134-135` — `if (status == HousekeepingStatus.Completed && task.Status == HousekeepingStatus.InProgress) task.AssignedToUserId = null;` — no identity check
- `MaintenanceService.cs:262-263` — same pattern

## Fix Scope

### Backend Services (2 files)

**HousekeepingService.cs** — Replace lines 134-135 with:

```csharp
if (status == HousekeepingStatus.Completed && task.Status == HousekeepingStatus.InProgress)
{
    var email = _currentUserService.GetUserEmail();
    if (string.IsNullOrEmpty(email)) throw new UnauthorizedAccessException("Must be logged in.");
    var user = await _userRepository.GetByEmailAsync(email);
    if (user == null) throw new ArgumentException("User not found.");
    if (task.AssignedToUserId != user.Id)
        throw new UnauthorizedAccessException("You can only complete your own tasks.");
    task.AssignedToUserId = null;
}
```

**MaintenanceService.cs** — Same guard on lines 262-263 (adapted to `dto.Status`, `MaintenanceStatus`).

### Existing Tests (2 files)

The existing **Completed** tests (`UpdateStatusAsync_ShouldSetFinishedAt_IfCompleted`,
`UpdateStatusAsync_SetCompleted_SetsFinishedAt`) and the existing **InProgress** tests
(`UpdateStatusAsync_ShouldSetStartedAt_IfInProgress`,
`UpdateStatusAsync_SetInProgress_SetsStartedAt`) mock neither the current user
nor the user repository. Fixing them is a pre-requisite:

- Set `task.AssignedToUserId = 1`
- Mock `GetUserEmail()` → `"test@example.com"`
- Mock `GetByEmailAsync(...)` → `new User { Id = 1 }`
- Mock `FindAsync(...)` → empty list (for InProgress 2-task check)

### No Frontend Changes

Ownership validation is server-side. Unauthorized requests return 401 and the
existing snackbar error handler displays the message.

## Execution Order

1. Edit `HousekeepingService.cs` — add ownership guard
2. Edit `MaintenanceService.cs` — add ownership guard
3. Edit `HousekeepingServiceTests.cs` — fix all 4 broken tests
4. Edit `MaintenanceServiceTests.cs` — fix all 4 broken tests
5. Run `dotnet test` — verify all pass
6. Run `detect_changes()` — verify expected scope only
7. Commit
