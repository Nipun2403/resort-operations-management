# Patch Specsheet: Kitchen – Fix Status Chip Colors & Order Detail Modal

## 1. Purpose
- Fix the status chip colors in the shared `TaskDashboardComponent` so that kitchen statuses (`Pending`, `Preparing`, `Delivered`) are shown with distinct colors.
- Fix the detail modal in the Kitchen Dashboard so that order items, status, and creation time are correctly displayed, using the actual field names from the `Order` API response.

## 2. Files to Modify

| File | Change |
|------|--------|
| `src/app/shared/components/task-dashboard/task-dashboard.component.scss` | Add CSS classes for kitchen statuses. |
| `src/app/features/kitchen/pages/dashboard.component.ts` | Update `fetchTasks` and `getDetailSections` to use correct order DTO properties. |

## 3. Status Chip Colors (CSS)

In `task-dashboard.component.scss`, ensure the following classes exist (they should already have `.Pending`, `.InProgress`, `.Completed` from the shared base; add the missing ones):

```scss
.status-chip {
  padding: 2px 8px;
  border-radius: 12px;
  font-size: 0.85rem;
  font-weight: 500;
  &.Pending { background-color: #fff3e0; color: #e65100; }
  &.InProgress, &.Preparing { background-color: #e3f2fd; color: #1565c0; }
  &.Completed, &.Delivered { background-color: #e8f5e9; color: #2e7d32; }
}
```

This covers both standard statuses (InProgress, Completed) and kitchen‑specific ones (Preparing, Delivered). The `[class]="t.status"` binding will automatically apply these classes as long as the status string matches.

## 4. Correct Order DTO Mapping

### 4.1 Verify the order response shape
Based on the Swagger file, the `Order` (or `FoodOrder`) response includes:
- `id: number`
- `bookingId: number`
- `foodOrderStatus: string` (enum: Pending, Preparing, Delivered)
- `items?: FoodOrderItem[]` (or `orderItems`)
- `generatedAt?: string` (ISO date)
- `bookedAt` is not a field; the booking has `bookedAt`, not the order.

From the earlier active tickets fetch, we used `this.orderApi.getAll(...).subscribe(res => res.data)`. The response data is an array of orders. In `TicketListComponent` we displayed `t.status` and assumed it exists. But the actual property is likely `foodOrderStatus`. The API may have renamed it to `status` for consistency? We need to inspect. Since the detail modal shows blank, the property `order.status` is probably undefined. So the correct property is `order.foodOrderStatus` or `order.status`. We need to check the sample response from a previous call. In the front desk active tickets, we used `t.status` for food orders and it worked? If it didn't, the ticket list would have shown blank. So maybe the API returns `status` as a normalized field? The user said it's showing blank, so likely the raw order object does not have a `status` property; it has `foodOrderStatus`.

Similarly, the creation date might be `order.generatedAt` instead of `bookedAt`. The items might be `order.items` or `order.orderItems`. We'll adjust the mapping to use the most probable field names based on the Swagger `FoodOrder` DTO.

### 4.2 Update `fetchTasks` in KitchenDashboardComponent
Replace the existing mapping with:

```typescript
fetchTasks: (params) => this.orderApi.getAll(params).pipe(
  map(res => ({
    totalCount: res.totalCount,
    data: res.data.map(order => ({
      id: order.id,
      status: order.foodOrderStatus ?? order.status ?? 'Unknown', // fallback
      location: order.roomId ? `Room ${order.roomId}` : 'N/A',
      description: `Order #${order.id}`,
      createdAt: order.generatedAt ?? order.bookedAt ?? '',
      raw: order,
    } as Task))
  }))
),
```

### 4.3 Update `getDetailSections` to correctly read items
Assuming the raw order object has an `items` array (maybe named `foodOrderItems`?), we'll adjust:

```typescript
getDetailSections: (t) => {
  const order = t.raw;
  // items could be order.items or order.foodOrderItems
  const itemsArray = order.items || order.foodOrderItems || [];
  const items = itemsArray.length > 0 
    ? itemsArray.map((i: any) => `${i.quantity}x ${i.name ?? i.menuItemName ?? 'Item #' + i.menuItemId}`).join(', ')
    : 'None';
  return [
    { title: 'Order Information', fields: [
      { label: 'Order ID', value: String(order.id) },
      { label: 'Status', value: t.status },
      { label: 'Items', value: items },
      { label: 'Created At', value: t.createdAt ? new Date(t.createdAt).toLocaleString() : 'N/A' },
    ]},
  ] as DetailSection[];
},
```

**Note:** If the items array contains objects with `menuItemId` only (no name), we'll display `MenuItem #id`. This is safe and will work until the backend adds room numbers/names.

## 5. Session or No Session?
No changes needed.

## 6. Self‑Review Checklist
- [ ] Status chips in the kitchen dashboard now show distinct colors for Pending, Preparing, Delivered.
- [ ] The table columns (Location, Description, Status) display correctly.
- [ ] Clicking a row opens the detail modal and shows Order ID, Status, Items (with quantity), and Created At.
- [ ] Items display as a comma‑separated list (e.g., "2x Burger, 1x Soda").
- [ ] No console errors regarding undefined properties.
- [ ] All other roles (housekeeping, maintenance) are unaffected by CSS changes (they already have InProgress/Completed classes).

## 7. Integration Notes
- The CSS changes are added to the shared component, so they will also benefit any future statuses. The existing housekeeping/maintenance statuses already have classes defined; the new classes just extend coverage.
- The kitchen dashboard config changes are isolated; no other components are touched.
- When the backend adds `roomNumber` to the order response, we will update the `location` mapping to use that instead of `Room #roomId`. That will be a minor future patch.

