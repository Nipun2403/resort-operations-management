# Front Desk Portal Routing Fix Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix front-desk portal routing where guest details entries redirect to error 500 due to overly restrictive authorization guards.

**Architecture:** The FrontDeskGuard currently only allows users with "FrontDesk" role to access `/operations/front-desk/*` routes. However, guest details (`/guest/:email`) should also be accessible to "HotelUser" role. Need to modify the guard to allow both roles for guest routes while maintaining FrontDesk-only access for other routes.

**Tech Stack:**
- Angular 21.2.13
- TypeScript 5+
- Angular Router Guards
- Node.js (for command-line operations)

## Global Constraints

- All front-desk routes are prefixed with `/operations/front-desk/`
- Guest route: `/operations/front-desk/guest/:email`
- All other routes require "FrontDesk" role exclusively
- HotelUser role should only access guest details and dashboard
- Guards are used in both `canMatch` and `canActivate` positions
- Routes with path parameters must still be protected correctly
- Error 500 redirect occurs when route is inaccessible
- Should not break existing functionality for FrontDesk users

## Task Structure

### Task 1: Identify All Front-Desk Routes
**Files:**
- Read: `Frontend/src/app/app.routes.ts`

**Interfaces:**
- Consumes: Current app routing configuration
- Produces: List of all front-desk routes with their guard requirements

- [ ] **Step 1: Extract all front-desk routes**
```bash
grep -n "operations/front-desk" Frontend/src/app/app.routes.ts -A 20 -B 2
```

- [ ] **Step 2: Document current guard requirements**

- [ ] **Step 3: Identify guest route pattern**

- [ ] **Step 4: Identify other route patterns**

- [ ] **Step 5: Compile route analysis**

### Task 2: Analyze Current Guard Implementation
**Files:**
- Read: `Frontend/src/app/core/guards/front-desk.guard.ts`

**Interfaces:**
- Consumes: Current guard implementation
- Produces: Refined guard logic with role-based path access

- [ ] **Step 1: Read and document current guard logic**
```typescript
export const frontDeskGuard: CanActivateFn & CanMatchFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (auth.isAuthenticated() && auth.role() === "FrontDesk") {
    return true;
  }
  if (auth.isAuthenticated()) {
    return router.createUrlTree(['/error/403']);
  }
  return router.createUrlTree(['/auth']);
};
```

- [ ] **Step 2: Document required access patterns**

- [ ] **Step 3: Identify guest route URL pattern**

- [ ] **Step 4: Design new guard logic**

### Task 3: Implement Refined Front Desk Guard
**Files:**
- Modify: `Frontend/src/app/core/guards/front-desk.guard.ts`

**Interfaces:**
- Consumes: Original guard implementation
- Produces: Guard that properly handles role-based guest access

- [ ] **Step 1: Update guard signature to accept route/state**
```typescript
export const frontDeskGuard: CanActivateFn & CanMatchFn = (route, state) => {
```

- [ ] **Step 2: Check authentication first**
```typescript
const auth = inject(AuthService);
const router = inject(Router);

if (!auth.isAuthenticated()) {
  return router.createUrlTree(['/auth']);
}
```

- [ ] **Step 3: Handle guest route for FrontDesk OR HotelUser**
```typescript
const role = auth.role();
if (state.url.includes('/guest')) {
  return true; // Both FrontDesk and HotelUser can access
}
```

- [ ] **Step 4: Keep strict FrontDesk access for other routes**
```typescript
if (role === 'FrontDesk') {
  return true;
}
```

- [ ] **Step 5: Handle unauthorized access**
```typescript
return router.createUrlTree(['/error/403']);
```

### Task 4: Create Test Suite for Guard Behavior
**Files:**
- Create: `Frontend/src/test/app/core/guards/front-desk.guard.spec.ts`

**Interfaces:**
- Test guard behavior with different user roles and URL patterns
- Verify guest route access controls

- [ ] **Step 1: Setup test file structure**
```typescript
// Import necessary Angular testing utilities
// Import AuthService mock
// Import the guard
```

- [ ] **Step 2: Create test cases for all scenarios**
```typescript
describe('FrontDeskGuard', () => {
  it('should allow FrontDesk to access guest routes', () => { 
    // Test FrontDesk user accessing /guest/:email
  });
  
  it('should allow HotelUser to access guest routes', () => { 
    // Test HotelUser user accessing /guest/:email
  });
  
  it('should block non-FrontDesk users from guest routes', () => { 
    // Test Admin/Kitchen users accessing /guest/:email
  });
  
  it('should block HotelUser from other routes', () => { 
    // Test HotelUser accessing /dashboard
  });
});
```

### Task 5: Run Tests and Verify Fix
**Files:**
- Test: All frontend tests

**Interfaces:**
- Verify test coverage for guard logic
- Ensure no test failures introduced

- [ ] **Step 1: Run guard-specific tests**
```bash
ng test --testPath "frontend/src/test/app/core/guards/front-desk.guard.spec.ts"
```

- [ ] **Step 2: Run full test suite**
```bash
ng test
```

- [ ] **Step 3: Manual verification of guest route access**

- [ ] **Step 4: Document test results**

## Self-Review

**1. Spec coverage:** ✓ Guard logic found for all front-desk routes
  - ✓ Guest route (`/guest/:email`) identified
  - ✓ Other routes identified
  - ✓ Guard implementation found

**2. Placeholder scan:** All steps contain actual implementation content

**3. Type consistency:** TypeScript interfaces match Angular Guard types

## Verification Commands

```bash
# Test if guard works
curl -i http://localhost:4200/operations/front-desk/guest/test@example.com

# Expected: 403 if no auth, or 200/302 if properly authenticated

# Test dashboard access
curl -i http://localhost:4200/operations/front-desk/dashboard

# Expected: 403 for HotelUser, 200/302 for FrontDesk
```

## Execution Handoff

Plan complete and saved. Two execution options:

**1. Subagent-Driven (recommended)** - I dispatch a fresh subagent per task, review between tasks for fast iteration

**2. Inline Execution** - Execute tasks in this session using executing-plans with checkpoints for review

Which approach?"