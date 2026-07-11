# Patch Specsheet B: Oversight Pages – Server‑Side Column Sorting

## 1. Purpose

Enable sorting by clicking column headers on three read‑only oversight pages: **Billing & Receipts**, **Audit Logs**, and **Feedback**. Sorting is server‑side, matching management CRUD behaviour. Every sortable column cycles `asc` ↔ `desc` on click (no blank third click) and triggers a new API call with the correct `sortBy` / `sortDescending` parameters.

## 2. Global Rule (All MatSort Tables)

**Every `<table mat-table>` in the entire system must include the `matSortDisableClear` attribute.**
This ensures sorting never emits an empty direction; the cycle is always `asc` ↔ `desc`. No custom toggle logic is needed anywhere.

## 3. Files to Modify

| Page               | Component TS                        | Component HTML                        |
| ------------------ | ----------------------------------- | ------------------------------------- |
| Billing & Receipts | `.../billing-receipts.component.ts` | `.../billing-receipts.component.html` |
| Audit Logs         | `.../audit-logs.component.ts`       | `.../audit-logs.component.html`       |
| Feedback           | `.../feedback.component.ts`         | `.../feedback.component.html`         |

## 4. Explicit Backend Sort Fields (Per Page)

### Audit Logs

Allowed `sortBy` values: `'id' | 'timestamp'`

**State signals:**

```ts
sortField = signal<AuditSortField>("timestamp");
sortDescending = signal(false);
```

Update `onSortChange` to use `AuditSortField`:

```ts
onSortChange(event: Sort): void {
  if (!event.active || !event.direction) return;
  const field = event.active as AuditSortField;
  if (!['id','timestamp'].includes(field)) return;
  this.sortField.set(field);
  this.sortDescending.set(event.direction === 'desc');
  this.pageIndex.set(0);
  this.saveState();
  this.fetchData();
}
```

### Feedback

Allowed `sortBy` values: `'id' | 'rating' | 'createdAt'`

**State signals:**

```ts
sortField = signal<FeedbackSortField>("createdAt");
sortDescending = signal(true);
```

Update `onSortChange`:

```ts
onSortChange(event: Sort): void {
  if (!event.active || !event.direction) return;
  const field = event.active as FeedbackSortField;
  if (!['id','rating','createdAt'].includes(field)) return;
  this.sortField.set(field);
  this.sortDescending.set(event.direction === 'desc');
  this.pageIndex.set(0);
  this.saveState();
  this.fetchData();
}
```

### Billing & Receipts – Dual‑Table State (Normalized)

**Bookings sort state:**

```ts
bookingSortField = signal<BookingSortField>("bookedAt");
bookingSortDescending = signal(true);
```

**Receipts sort state:**

```ts
receiptSortField = signal<ReceiptSortField>("id");
receiptSortDescending = signal(true);
```

**Enforced rule:** Never cross‑reference booking sort state with receipts sort state, and vice versa. Each table’s sort method updates only its own signals and calls only its own fetch method.

**Bookings sort handler:**

```ts
onBookingSort(event: Sort): void {
  if (!event.active || !event.direction) return;
  const field = event.active as BookingSortField;
  if (!['id','bookingStatus','bookedAt'].includes(field)) return;
  this.bookingSortField.set(field);
  this.bookingSortDescending.set(event.direction === 'desc');
  this.bookingPage.set(0);
  this.saveState();
  this.fetchBookings();
}
```

**Receipts sort handler:**

```ts
onReceiptSort(event: Sort): void {
  if (!event.active || !event.direction) return;
  const field = event.active as ReceiptSortField;
  if (!['id','amountPaid','paidAt'].includes(field)) return;
  this.receiptSortField.set(field);
  this.receiptSortDescending.set(event.direction === 'desc');
  this.receiptPage.set(0);
  this.saveState();
  this.fetchReceipts();
}
```

## 5. Column Configuration – Mark Sortable Columns

**Billing – Bookings table**: columns `id`, `bookingStatus`, `bookedAt` have `mat-sort-header`.  
**Billing – Receipts table**: columns `id`, `amountPaid`, `paidAt` have `mat-sort-header`.  
**Audit Logs**: columns `id`, `timestamp` have `mat-sort-header`.  
**Feedback**: columns `id`, `rating`, `createdAt` have `mat-sort-header`.

Add `matSortDisableClear` to every `<table>` element.

## 6. Session Storage

Ensure `saveState()` / `restoreState()` include the sort signals for each table (as previously defined). No cross‑table leakage.

## 7. Self‑Review Checklist

- [ ] Every table’s sortable columns toggle direction correctly; no blank click.
- [ ] API requests include correct `sortBy` and `sortDescending` values from the allowed enums.
- [ ] Sorting resets pagination to page 0.
- [ ] Billing page’s booking sort does not affect receipt table state, and vice versa.
- [ ] Session storage correctly persists and restores sort state for each table.
- [ ] No `sortBy` values outside the defined enums are ever sent.

## 8. Integration Notes

- This patch adds no new dependencies.
- The explicit enums remove any chance of hallucination by the agent.
- The `matSortDisableClear` rule is now a system‑wide standard, applied consistently.
- No other pages are affected.

