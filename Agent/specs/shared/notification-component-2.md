# Specsheet B: TaskDashboardComponent – Refresh Input Patch

## 1. Purpose
- Add a `refresh` input to the shared `TaskDashboardComponent` so that an external signal (e.g., from a notification) can trigger a full data and summary reload.
- The input is a `signal<number>` or `input<number>`. When its value changes, the component re‑fetches the table data and the summary counts.
- This patch is backward‑compatible; if no `refresh` input is provided, the component behaves exactly as before.

## 2. Files to Modify
| File | Change |
|------|--------|
| `src/app/shared/components/task-dashboard/task-dashboard.component.ts` | Add `refresh` input; add effect to react to changes. |

## 3. Changes

### 3.1 Add `refresh` input
```typescript
refresh = input(0);
```

### 3.2 Add an `effect` to watch `refresh`
In the constructor or as a field initializer (ensure injection context):

```typescript
constructor() {
  effect(() => {
    // Read the refresh value to register dependency
    this.refresh();
    // Re-fetch data and summary counts
    this.fetchData();
    this.refreshSummaryCounts();
  });
}
```

**Important:** Use `effect` from `@angular/core`. Ensure the component already has a `constructor` or use the new `inject(…).constructor` pattern. To keep it simple, add a private method `setupRefreshEffect()` and call it from the constructor:

```typescript
constructor() {
  this.setupRefreshEffect();
}

private setupRefreshEffect(): void {
  effect(() => {
    this.refresh(); // read to track
    this.pageIndex.set(0); // reset to first page when refreshed
    this.fetchData();
    this.refreshSummaryCounts();
  });
}
```

### 3.3 Import `effect` from `@angular/core`
Add to existing imports.

### 3.4 Ensure `fetchData` and `refreshSummaryCounts` are already defined
They are – as per the original spec. No changes needed to their signatures.

### 3.5 Remove any existing initialization calls if they conflict
The `ngOnInit` already calls `fetchData()` and `refreshSummaryCounts()`. Keep that for the initial load. The effect will also fire on initialization because `refresh()` is read and has an initial value of `0`. That’s fine—it will cause a double fetch on first load (one from `ngOnInit`, one from effect). To avoid this, we can either not call them in `ngOnInit` and rely solely on the effect, or guard the effect with a flag. Simplest: remove `ngOnInit` entirely and let the effect handle the first fetch. The effect runs after the component is fully initialized, so it will fetch data correctly. We'll adopt that approach.

**Remove `ngOnInit`** and move the initial fetch into the effect. The effect will run once on startup, then again whenever `refresh` changes.

**Updated lifecycle:** No `ngOnInit`. All data loading is driven by the effect. This ensures a single source of truth for data fetching.

### 3.6 Update component documentation
Add comment in code: “Data fetching is driven by the `refresh` input via an effect. Increment the signal in the parent to trigger a reload.”

## 4. Self‑Review Checklist
- [ ] `refresh` input accepts a number.
- [ ] When the parent increments the value, the component re‑fetches table data and summary counts.
- [ ] Initial load still works (effect runs on creation).
- [ ] No duplicate initial fetches.
- [ ] No console errors.

## 5. Integration Notes
- This change is fully contained in `TaskDashboardComponent`. Parents that do not pass `refresh` will see no change in behavior (the effect still runs once on init).
- The effect uses `takeUntilDestroyed` internally? The `effect` function automatically cleans up when the component is destroyed. No extra subscription management is needed.

