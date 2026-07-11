# Themed Snackbar & Better Error Messages

**Date:** 2026-07-12
**Status:** Approved for implementation

## Problem

1. Backend `UnauthorizedAccessException` falls through to ASP.NET Core's default
   handler → generic `401` with no structured JSON message. Frontend shows
   "Failed to update task status: You are not authorized to access this resource."
2. The snackbar uses plain `snackBar.open()` at bottom of screen — no glass
   panel, no gold accent, doesn't match project's dark luxury theme.

## Root Cause

- Controllers only catch `ArgumentException`, not `UnauthorizedAccessException`
- Frontend uses bare `snackBar.open()` instead of `openFromComponent(NotificationSnackbarComponent)`

## Fix — 4 files

### Backend Controllers (2 files)

**HousekeepingController.cs** — Add catch before `Ok`:
```csharp
catch (UnauthorizedAccessException ex)
{
    return Unauthorized(new { message = ex.Message });
}
```

**MaintenanceController.cs** — Same catch clause.

### Frontend Error Message (1 file)

**task-dashboard.component.ts** — Add private helper method and update error/success calls:

```typescript
private showSnackbar(title: string, message: string): void {
    this.snackBar.openFromComponent(NotificationSnackbarComponent, {
        data: { title, message },
        duration: 5000,
        horizontalPosition: 'right',
        verticalPosition: 'top',
        panelClass: 'notification-snackbar',
    });
}
```

Replace the success snackbar:
```typescript
// Before:
this.snackBar.open('Task status updated successfully.', 'Close', { duration: 3000 });
// After:
this.showSnackbar('SUCCESS', 'Task status updated successfully.');
```

Replace the error snackbar:
```typescript
// Before:
this.snackBar.open('Failed to update task status: ' + (err.error?.message || err.message), 'Close', { duration: 5000 });
// After:
const msg = err.error?.message || '';
if (msg.includes('own tasks')) {
    this.showSnackbar('ERROR', 'This task belongs to another staff member and cannot be completed by you.');
} else {
    this.showSnackbar('ERROR', 'Failed to update task status: ' + (msg || err.message));
}
```

Also add `NotificationSnackbarComponent` to the component's `imports` array.

### No SCSS Changes

`NotificationSnackbarComponent` already has glass-panel / gold-accent / dark-luxury
styling baked in. No additional CSS needed.

## Execution Order

1. Edit `HousekeepingController.cs` — add UnauthorizedAccessException catch
2. Edit `MaintenanceController.cs` — add UnauthorizedAccessException catch
3. Edit `task-dashboard.component.ts` — add import, helper method, update calls
4. Run `dotnet build Backend/HotelManagement.BLL/` — verify
5. Run `npm run build` (Frontend) — verify
6. Commit
