# DatePipe "Invalid Date" Crash (NG02311)

## Error

```
NG02311: Unable to convert "Invalid Date" into a date
```

Navigates to `/error/500` via `GlobalErrorHandler`.

## Root Cause

When a `Booking.checkOutDate` (or any date string piped to `| date:`) contains an unparseable value — empty string `""`, `DateTime.MinValue` serialized as `"0001-01-01T00:00:00Z"`, or any format `new Date()` cannot parse — Angular's `DatePipe` throws `NG02311`.

The `GlobalErrorHandler` catches the unhandled runtime error and navigates to `/error/500`.

### How Invalid Date propagates

```ts
// GuestBillingComponent.parseDate()
private parseDate(dateStr: string): Date {
  const parts = dateStr.split('-');
  if (parts.length === 3) {
    return new Date(+parts[2], +parts[1] - 1, +parts[0]);  // dd-MM-yyyy
  }
  return new Date(dateStr);  // ISO-8601 fallback
}
```

- `dateStr = ""` → `new Date("")` → **Invalid Date** (truthy object with `.getTime()` === `NaN`)
- `dateStr = "0001-01-01T00:00:00Z"` → `new Date(...)` → **Invalid Date** on most JS engines

The Invalid Date is stored in `BillingRecord.checkOutDate` (type `Date`). Template `{{ b.checkOutDate | date:'mediumDate' }}` passes it to `DatePipe` → crash.

## Affected Files (from this fix)

- `Frontend/src/app/features/front-desk/components/guest-billing/guest-billing.component.ts`
- `Frontend/src/app/features/front-desk/components/guest-billing/guest-billing.component.html`
- `Frontend/src/app/features/front-desk/pages/guest-details.component.html`

## Fix Applied

### 1. `parseDate()` returns `null` for invalid dates

```ts
private parseDate(dateStr: string): Date | null {
  if (!dateStr) return null;
  const parts = dateStr.split('-');
  if (parts.length === 3) {
    const date = new Date(+parts[2], +parts[1] - 1, +parts[0]);
    if (!isNaN(date.getTime())) return date;
  }
  const date = new Date(dateStr);
  return !isNaN(date.getTime()) ? date : null;
}
```

Key guard: `!isNaN(date.getTime())` ensures only valid Date objects pass through. Empty string, `DateTime.MinValue`, or bad formats all return `null`.

### 2. `BillingRecord.checkOutDate` type widened

```ts
interface BillingRecord extends BillingFolio {
  checkOutDate: Date | null;
}
```

### 3. All booking/guest `date:` pipes guarded

```html
<!-- Before (crashes if value is Invalid Date / empty string) -->
{{ b.checkOutDate | date:'mediumDate' }}

<!-- After (safe) -->
{{ b.checkOutDate ? (b.checkOutDate | date:'mediumDate') : '—' }}
```

Null is falsy, so `null` checkOutDate skips the pipe and renders `'—'`.

### 4. Sort null-safe

```ts
valid.sort((a, b) => (b.checkOutDate?.getTime() ?? 0) - (a.checkOutDate?.getTime() ?? 0));
```

## Prevention Checklist

When adding new `| date:` pipes for fields that come from API responses:

- [ ] Can the API return `null` / empty string / `DateTime.MinValue`?
- [ ] Is the value type `string` (not `Date`)? Angular's `DatePipe` can parse ISO-8601 strings, but empty strings crash.
- [ ] Has the template guard been applied? `{{ val ? (val | date:'mediumDate') : '—' }}`
- [ ] If the value goes through a `parseDate()`-like utility, does it guard against Invalid Date with `!isNaN(date.getTime())`?

## Also Fixed: Interceptor Import Bug

`error-page.interceptor.ts` imported HTTP types from `@angular/core` instead of `@angular/common/http`, blocking the dev server build:

```ts
// Wrong
import { HttpErrorResponse } from '@angular/core';

// Correct
import { HttpErrorResponse } from '@angular/common/http';
```
