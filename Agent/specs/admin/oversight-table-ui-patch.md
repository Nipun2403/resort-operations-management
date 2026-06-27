# Patch Specsheet: Oversight Pages – Table Column Width Consistency

## 1. Purpose
- Apply the same table column width stability fix to the oversight pages: **Audit Logs**, **Billing & Receipts**, and **Feedback**.
- After this patch, clicking column headers to sort will no longer cause column widths to shift, matching the behaviour of the management CRUD pages.

## 2. Files to Modify

| Page | Component SCSS |
|------|----------------|
| Audit Logs | `src/app/features/admin/pages/oversight/audit-logs.component.scss` |
| Billing & Receipts | `src/app/features/admin/pages/oversight/billing-receipts.component.scss` |
| Feedback | `src/app/features/admin/pages/oversight/feedback.component.scss` |

## 3. Changes

### 3.1 Common CSS Rules
For each page’s SCSS file, add the following block. The selectors are scoped to the specific tables using the component’s host class or a containing class.

#### Audit Logs
```scss
.audit-logs-page table {
  table-layout: fixed;
  width: 100%;
}
.audit-logs-page th,
.audit-logs-page td {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
// 6 columns: id, entityName, action, changedBy, timestamp, actions
.audit-logs-page th:nth-child(1) { width: 10%; }  // id
.audit-logs-page th:nth-child(2) { width: 15%; }  // entity
.audit-logs-page th:nth-child(3) { width: 15%; }  // action
.audit-logs-page th:nth-child(4) { width: 20%; }  // changedBy
.audit-logs-page th:nth-child(5) { width: 25%; }  // timestamp
.audit-logs-page th:nth-child(6) { width: 15%; }  // actions
```

#### Feedback
```scss
.feedback-page table {
  table-layout: fixed;
  width: 100%;
}
.feedback-page th,
.feedback-page td {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
// 7 columns: id, bookingId, rating, comments, createdAt, isHidden, actions
.feedback-page th:nth-child(1) { width: 8%; }   // id
.feedback-page th:nth-child(2) { width: 10%; }  // bookingId
.feedback-page th:nth-child(3) { width: 10%; }  // rating
.feedback-page th:nth-child(4) { width: 30%; }  // comments (longer)
.feedback-page th:nth-child(5) { width: 17%; }  // createdAt
.feedback-page th:nth-child(6) { width: 10%; }  // isHidden
.feedback-page th:nth-child(7) { width: 15%; }  // actions
```

#### Billing & Receipts
There are two tables. We’ll scope each.

**Bookings table** (inside `.bookings-view`):
```scss
.bookings-view table {
  table-layout: fixed;
  width: 100%;
}
.bookings-view th,
.bookings-view td {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
// 7 columns: id, guestName, checkIn, checkOut, status, rooms, actions
.bookings-view th:nth-child(1) { width: 8%; }   // id
.bookings-view th:nth-child(2) { width: 18%; }  // guestName
.bookings-view th:nth-child(3) { width: 14%; }  // checkIn
.bookings-view th:nth-child(4) { width: 14%; }  // checkOut
.bookings-view th:nth-child(5) { width: 12%; }  // status
.bookings-view th:nth-child(6) { width: 19%; }  // rooms
.bookings-view th:nth-child(7) { width: 15%; }  // actions
```

**Receipts table** (inside `.receipts-view`):
```scss
.receipts-view table {
  table-layout: fixed;
  width: 100%;
}
.receipts-view th,
.receipts-view td {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
// 6 columns: id, bookingId, amountPaid, paymentMethod, paidAt, actions
.receipts-view th:nth-child(1) { width: 10%; }   // id
.receipts-view th:nth-child(2) { width: 15%; }   // bookingId
.receipts-view th:nth-child(3) { width: 15%; }   // amountPaid
.receipts-view th:nth-child(4) { width: 20%; }   // paymentMethod
.receipts-view th:nth-child(5) { width: 25%; }   // paidAt
.receipts-view th:nth-child(6) { width: 15%; }   // actions
```

### 3.2 Ensure No Horizontal Scroll on Desktop
The `table-layout: fixed` and explicit widths will keep columns stable. For smaller screens, the existing responsive rules already allow horizontal scrolling.

## 4. Self‑Review Checklist (for the agent)
- [ ] Audit Logs table columns do not shift when sorting by any column.
- [ ] Feedback table columns do not shift when sorting.
- [ ] Billing & Receipts: both the bookings and receipts tables maintain column widths during sorting.
- [ ] No text is cut off with the defined widths; long strings are truncated with ellipsis.
- [ ] Responsive scrolling still works on mobile (the table container allows horizontal scroll).
- [ ] No visual regressions in other parts of these pages.

## 5. Integration Notes
- The CSS rules are scoped to the specific page containers to avoid affecting other components.
- The percentages assigned may be adjusted if some columns need more space, but the current values are based on typical content length.
- No TypeScript changes are required. This patch is purely CSS.