# AGENT.md — AI Coding Agent Governing Protocol

> **Applies to:** All AI coding agents (Gemini, GPT, Claude, etc.) working on this project.
> **Mandatory read.** Every agent MUST read and internalize this entire document before writing a single line of code. No exceptions.

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Core Directives — Read These First](#2-core-directives--read-these-first)
3. [The Spec Sheet Contract](#3-the-spec-sheet-contract)
4. [Implementation Protocol — Part-by-Part Workflow](#4-implementation-protocol--part-by-part-workflow)
5. [Pre-Implementation Checklist](#5-pre-implementation-checklist)
6. [Angular Implementation Standards](#6-angular-implementation-standards)
7. [Backend Integration Standards (ASP.NET C#)](#7-backend-integration-standards-aspnet-c)
8. [Code Quality Standards](#8-code-quality-standards)
9. [Anti-Hallucination Protocol](#9-anti-hallucination-protocol)
10. [Ambiguity Escalation Protocol](#10-ambiguity-escalation-protocol)
11. [Forbidden Patterns](#11-forbidden-patterns)
12. [Self-Review & Audit Requirements](#12-self-review--audit-requirements)
13. [Completion Report Format](#13-completion-report-format)
14. [Violations Reference — What NOT to Do](#14-violations-reference--what-not-to-do)

---

## 1. Project Overview

**Frontend:** Angular (v20+), standalone component architecture, Angular Material, signals-based state management, reactive forms.

**Backend:** ASP.NET C# Web API. All API contracts (endpoints, request/response DTOs, status codes) are documented in spec sheets. Do not infer or assume any backend behavior beyond what is explicitly written in the active spec sheet.

**Purpose of this file:** To govern agent behavior so that every implementation is:

- Exactly what the spec says — nothing more, nothing less.
- Architecturally sound and free of logical flaws.
- High quality, maintainable, and bug-free.
- Delivered incrementally to ensure precision at every step.

---

## 2. Core Directives — Read These First

These directives are absolute. They override any internal tendency to be helpful, creative, or efficient in ways not requested.

### DIRECTIVE 1 — The Spec is the Law

The active spec sheet is the single source of truth. Every decision — file names, folder structure, component selectors, API endpoints, validation rules, UI states, data flow — is determined by the spec. If it is not in the spec, it does not get built. Use "Halt and ask" as an Emergency Stop only for critical security flaw or a breaking architectural paradox or for severe logical impossibilities.

### DIRECTIVE 2 — No Unsolicited Improvements

Do not refactor, optimize, redesign, or extend anything beyond what the spec requires. Do not add extra utility functions "just in case." Do not add animations, extra UI polish, extra error messages, or extra validation rules unless the spec explicitly calls for them. Good intent does not justify scope creep.

### DIRECTIVE 3 — No Hallucination of API Contracts

Never invent an API endpoint, a request field, a response field, or a status code. If the spec defines `POST /auth/login` with body `{ email, password }` returning `{ token }`, that is the only contract that exists. Do not add fields. Do not assume other endpoints.

### DIRECTIVE 4 — Part-by-Part Execution

Work in clearly defined sequential parts. Complete one part fully, verify it against the spec, then move to the next. Do not write code for Part 3 while Part 2 is incomplete or unverified.

### DIRECTIVE 5 — Zero Logical Flaws

Every piece of logic — guards, state transitions, form validation, API error handling, loading states, race conditions — must be airtight. Before writing a function, reason through every code path it can take and verify each path is correct per the spec.

### DIRECTIVE 6 — Explicit over Implicit

When the spec is explicit, follow it exactly. When the spec is silent, do the minimum necessary and flag it (see [Ambiguity Escalation Protocol](#10-ambiguity-escalation-protocol)). Never silently fill in gaps with assumptions.

### DIRECTIVE 7 — Honest Self-Reporting

Do not claim a requirement is implemented unless the code for that requirement demonstrably exists and is correct. A compliance report that says "✓ Done" when the code is missing or incorrect is a critical failure.

---

## 3. The Spec Sheet Contract

### 3.1 What a Spec Sheet Is

A spec sheet is a named markdown document (e.g., `auth-page.md`) that fully describes a single feature, page, or system to be built. It contains:

- Purpose and scope
- Route and navigation (if applicable)
- Authorization requirements
- API endpoints with full request/response contracts
- Component architecture and file structure
- Template structure (often literal or near-literal)
- State management
- Data flow
- Forms and validation rules (including exact regex patterns)
- UI states (loading, error, success, default)
- Responsive breakpoints
- Accessibility requirements
- Integration notes and cross-cutting concerns
- Dependencies and imports
- A verification checklist
- A compliance report template

### 3.2 How to Read a Spec Sheet

1. Read the **entire spec sheet top to bottom** before writing any code.
2. Identify all files to be created (Section: File Structure).
3. Identify all dependencies — both internal (services, guards, utilities) and external (npm packages, Angular Material modules).
4. Map every requirement to a concrete code artifact (class, method, template block, style rule).
5. Identify any cross-spec dependencies (e.g., `AuthService` built in `auth-page.md` will be used in future specs).
6. Flag any ambiguities using the Ambiguity Escalation Protocol before writing code.

### 3.3 The Dead-Letter Rule

> **If it is not written in the spec sheet, it does not get built.**

This applies to:

- Extra files not in the spec's file structure
- Extra form fields the spec doesn't mention
- Extra API calls not documented
- Extra routes not specified
- Extra UI components not required
- Extra error handling strategies not specified
- Performance optimizations not requested
- Comments explaining business logic not in the spec

The only exception is boilerplate required by the Angular framework itself (e.g., `ngOnInit`, lifecycle hooks, required constructor syntax), which must still conform to spec intent.

### 3.4 Spec Sheet Sections are All Mandatory

If a spec section is marked ‘Optional’ or ‘Recommend’, the agent may skip it but should explictly address the skipping after specsheet task is completed.

---

## 4. Implementation Protocol — Part-by-Part Workflow

This is the mandatory execution order for every spec sheet. Do not deviate.

---

### PART 0 — Spec Ingestion & Planning

**Before writing code:**

1. Parse the entire spec sheet.
2. List every file that must be created, in order of dependency (lowest-level first: models → utilities → services → guards → components → pages).
3. List every external dependency (npm packages, Angular Material modules) and confirm they are available or note that they need installation.
4. List every API endpoint with its full contract (method, path, body, response, error cases).
5. List every validation rule with its exact regex or constraint.
6. List every cross-spec dependency (other services/guards/models this spec references but doesn't build).
7. Identify every ambiguity and apply the Ambiguity Escalation Protocol.
8. Only after all the above is complete, proceed to Part 1.

**Output at end of Part 0:**

```
Part 0 Complete — Spec Ingestion Summary
Files to create: [list]
Dependencies: [list]
API contracts: [list]
Ambiguities flagged: [list or "None"]
Proceeding to Part 1.
```

---

### PART 1 — Models, Interfaces & Types

Build all TypeScript interfaces, types, enums, and DTOs defined in the spec.

Rules:

- Field names must exactly match the spec (camelCase on frontend, matching backend DTO names).
- Do not add optional (`?`) or required markers beyond what the spec specifies.
- Do not add extra fields.
- Place files in the location specified by the spec's file structure.

**Verify before proceeding:**

- [ ] Every interface defined in the spec exists in code.
- [ ] No extra fields added.
- [ ] File locations match the spec.

---

### PART 2 — Utilities & Pure Functions

Build all utility functions defined in the spec (e.g., `jwtDecode`, date formatters, mappers).

Rules:

- Utility functions must be pure unless the spec requires side effects.
- Implement exactly the logic described. Do not add overloads or extra parameters.
- Include only the error handling that the spec describes.
- If the spec shows a code example, treat it as the required implementation — not a suggestion.

**Verify before proceeding:**

- [ ] Every utility function defined in the spec exists and behaves exactly as specified.
- [ ] No extra utilities added.

---

### PART 3 — Services (API Services & Domain Services)

Build all injectable services defined in the spec.

Rules:

- API service methods must call exactly the endpoints in the spec — no more, no less.
- HTTP method (GET/POST/PUT/DELETE/PATCH) must match exactly.
- Request body shape must match the DTO exactly.
- Response type must match the interface exactly.
- Base URL must come from the Angular environment file (`environment.ts` / `environment.development.ts`), never hardcoded.
- Use `HttpClient` for all HTTP calls. Do not use `fetch` or any other HTTP mechanism unless the spec explicitly requires it.
- Observable operators (e.g., `finalize`, `catchError`, `map`) must be used only where the spec requires them or where they are mandatory for the described behavior.
- `providedIn: 'root'` unless the spec specifies otherwise.

**Verify before proceeding:**

- [ ] Every service method exists.
- [ ] Every endpoint matches spec exactly (method + path + body + response type).
- [ ] Base URL sourced from environment.
- [ ] No extra endpoints added.
- [ ] `providedIn` matches spec.

---

### PART 4 — Guards & Interceptors

Build all route guards and HTTP interceptors defined in the spec.

Rules:

- Functional guards must use `inject()` not constructor injection.
- Guard logic must cover every branch defined in the spec (authenticated, unauthenticated, role-based, etc.).
- Role-to-route mappings must match the spec exactly — every role, every path.
- Fallback routes must match the spec.
- Do not add extra roles, extra redirects, or extra guard conditions.

**Verify before proceeding:**

- [ ] Every guard exists and covers all branches exactly as specified.
- [ ] Role-to-route map matches spec exactly.
- [ ] Functional guard syntax used (unless spec specifies class-based).

---

### PART 5 — Standalone Sub-components

Build all child/sub-components defined in the spec before building the parent page component.

Rules:

- Component selector must match the spec exactly.
- Inputs and outputs must match the spec's type signatures exactly.
- Template structure must match the spec's HTML structure as closely as possible — only deviate if technically required by Angular syntax.
- Do not add extra `@Input()` or `@Output()` properties.
- CSS class names used in the template must match the spec (for styling hooks).
- Standalone components must import exactly the modules listed in the spec.

**Verify before proceeding:**

- [ ] Every sub-component exists.
- [ ] Selectors match.
- [ ] Inputs/outputs match spec types.
- [ ] No extra props added.
- [ ] Import lists match spec.

---

### PART 6 — Page/Feature Component

Build the main page component described in the spec.

Rules:

- Signals must be used where the spec specifies signals. `@Input` signals vs regular signals vs `computed` vs `effect` — match exactly.
- Local state must cover all states in the spec (loading, error, success, default, etc.) — no states may be omitted.
- The template must implement every UI state described in Section "UI States."
- Form initialization and validation must exactly match the spec's validator descriptions, including the exact regex strings.
- Method names in the component class should match the spec's described methods (e.g., `onLogin`, `onRegister`).
- Lifecycle and cleanup: use `takeUntilDestroyed()` wherever the spec or good practice requires subscription cleanup. Every `subscribe()` must be cleaned up.
- Do not add extra methods, computed signals, or effects beyond what the spec requires.

**Verify before proceeding:**

- [ ] Component class matches spec structure.
- [ ] All signals declared as specified.
- [ ] All methods implemented as specified.
- [ ] Form controls and validators match spec exactly (including regex).
- [ ] All UI states implemented.
- [ ] All subscriptions cleaned up.

---

### PART 7 — Templates

Write or finalize the HTML templates for all components.

Rules:

- If the spec provides a literal template block, match it as closely as possible. Structural directives (`@if`, `@for`, `@switch`), attribute bindings, event bindings, and class bindings must all follow the spec.
- Angular Material component usage must match the spec — use `mat-card`, `mat-form-field`, `mat-button`, etc. exactly as named.
- CSS classes must match the spec's template to serve as styling hooks.
- Accessibility attributes (`aria-*`, `role`, `aria-live`, `aria-describedby`, `aria-pressed`) must be placed exactly where the spec defines them.
- `@if` / `@else` / `@for` blocks must be used (Angular 20+ control flow syntax) unless the spec explicitly shows `*ngIf` / `*ngFor`.

**Verify before proceeding:**

- [ ] Every template UI element specified exists in the template.
- [ ] Angular Material components used correctly.
- [ ] Accessibility attributes in place.
- [ ] No extra UI elements added beyond spec.

---

### PART 8 — Styles (SCSS)

Write the component-scoped SCSS.

Rules:

- Implement every responsive breakpoint defined in the spec.
- Implement every state style (loading, active, error, success) described in the spec.
- Use the spec's specified max-width, margin, padding values exactly.
- Do not add styles for elements not in the spec's template.
- Do not add animations or transitions unless the spec specifies them.
- CSS class names must match those used in the templates you wrote in Part 7.

**Verify before proceeding:**

- [ ] All breakpoints implemented.
- [ ] All specified dimensions/spacing used.
- [ ] No extra styles for non-spec elements.

---

### PART 9 — Routing Configuration

Update `app.routes.ts` (or the relevant routing module) with the route(s) defined in the spec.

Rules:

- Path must match exactly (`/auth` → `'auth'`, not `'authentication'` or `'login'`).
- Lazy loading (`loadComponent`) must be used if the spec shows it.
- Guards must be attached exactly as described.
- Do not add extra route properties (e.g., `title`, `data`) unless the spec includes them.

**Verify before proceeding:**

- [ ] Route path matches spec exactly.
- [ ] Lazy loading used if spec requires it.
- [ ] Guards attached exactly as specified.

---

### PART 10 — Integration & End-to-End Logic Verification

Before declaring implementation complete, trace every user flow described in the spec:

For each flow (e.g., "Login Submit", "Register Submit", "Already Logged In"):

1. Start at the user action.
2. Trace through every component method, service call, observable chain, state mutation, and route navigation.
3. Verify each step against the spec.
4. Verify error paths.
5. Verify loading state is set before async work and cleared after (in `finalize` or equivalent).
6. Verify no race condition is possible (e.g., double submit, stale state).

**Verify before proceeding:**

- [ ] Every flow traces end-to-end correctly.
- [ ] No logical gaps or undefined behavior.
- [ ] No race conditions.

---

### PART 11 — Final Audit & Compliance Report

Run the full self-review checklist from the spec sheet. Every checkbox must be explicitly evaluated — not rubber-stamped. If any checkbox cannot be checked, explain why and list it as a Known Deviation.

Write and output the Compliance Report in the format defined in Section 13 of this document.

---

## 5. Pre-Implementation Checklist

Answer every question before writing code. If any answer is "No" or "Unknown," resolve it first.

- [ ] Have I read the entire spec sheet top to bottom?
- [ ] Do I know the exact list of files to create?
- [ ] Do I know the correct folder path for every file?
- [ ] Do I know every API endpoint (method, path, body, response, error codes)?
- [ ] Do I know every validation rule including exact regex strings?
- [ ] Do I know every role-to-route mapping?
- [ ] Do I know every signal, computed, and effect the component needs?
- [ ] Do I know every Angular Material module to import?
- [ ] Do I know every UI state and what triggers each transition?
- [ ] Have I confirmed all spec dependencies exist (e.g., `AuthService` must exist before a guard can use it)?
- [ ] Are there any ambiguities I need to escalate before starting?

---

## 6. Angular Implementation Standards

### 6.1 Version and Architecture

- Angular 20+ with standalone component architecture.
- Do not create `NgModule` declarations unless a spec explicitly requires one.
- Use `@if`, `@for`, `@switch` control flow syntax (not `*ngIf`, `*ngFor`) unless a spec explicitly uses the old syntax.

### 6.2 Signals

- Use `signal()` for mutable local state described in the spec.
- Use `computed()` for derived values described in the spec.
- Use `effect()` only when the spec describes a reactive side effect.
- Do not use `BehaviorSubject` or `Subject` for local component state — use signals as specified.
- Signal naming must match the spec.

### 6.3 Reactive Forms

- Use `FormGroup` and `FormControl` with explicit typing (`FormControl<string>`, not `FormControl<any>`).
- Validators must be applied exactly as specified — same list, same order.
- Regex validators must use the exact regex string from the spec — character for character.
- Use `Validators.pattern()` with the spec's regex, not a manually coded equivalent.
- Mark all controls as touched before showing validation errors on submit.
- Focus the first invalid control on failed submit, as specified.

### 6.4 Services & Dependency Injection

- Use `inject()` in functional guards and standalone component constructors.
- Do not mix constructor injection and `inject()` in the same context.
- `providedIn: 'root'` is the default unless the spec specifies otherwise.

### 6.5 Observables & Subscriptions

- Every `.subscribe()` must be cleaned up. Use `takeUntilDestroyed()` from `@angular/core/rxjs-interop`.
- Use `finalize()` to reset loading state — this ensures loading is cleared on both success and error.
- Do not use `tap()` for side effects that the spec assigns to `subscribe` success/error callbacks — keep the intent clear.
- Use `catchError` at the service level only if the spec calls for it. Otherwise, handle errors in the component's `error` callback.

### 6.6 Angular Material

- Import only the Material modules listed in the spec.
- Use the exact Material component as named (`mat-card`, not `div` with card styling).
- Do not use Material components not listed in the spec.

### 6.7 Environments

- Base API URL must always come from `environment.ts` (production) and `environment.development.ts` (dev).
- Never hardcode base URLs in services or components.
- The environment structure is: `environment.apiUrl = 'http://localhost:XXXX/api/v1'` (exact port and base path as specified in the spec).

### 6.8 File Naming Conventions

- Components: `kebab-case.component.ts` / `.html` / `.scss`
- Services: `kebab-case.service.ts`
- Guards: `kebab-case.guard.ts`
- Models/Interfaces: `kebab-case.models.ts` or `feature.models.ts`
- Utilities: `kebab-case.ts` in `core/utils/`
- Always match the exact filenames in the spec's File Structure section.

---

## 7. Backend Integration Standards (ASP.NET C#)

### 7.1 API Contract Fidelity

- Every API call must exactly match the spec:
  - HTTP method (GET, POST, PUT, PATCH, DELETE)
  - Full URL path (appended to the base URL from environment)
  - Request body field names (case-sensitive, matching C# DTO property names as they appear in Swagger/spec)
  - Response type mapping to the TypeScript interface defined in the spec

### 7.2 DTO Naming

- C# DTOs use `PascalCase` for properties. The Angular frontend sends JSON which is automatically camelCase when serialized unless the backend is configured otherwise.
- If the spec's Swagger shows `{ Email: string, Password: string }` (PascalCase), send `{ email: string, password: string }` (camelCase) — ASP.NET Core's JSON serializer defaults to camelCase deserialization.
- If the spec explicitly shows the JSON field names, use those exact names.
- Never rename or alias API fields without explicit instruction in the spec.

### 7.3 Error Handling

- Parse error response as `error.error?.message` by default unless the spec defines a different error response shape.
- Always have a fallback error message (e.g., `'An unexpected error occurred.'`) for cases where the server returns no body.
- Do not swallow errors silently — always surface them to the UI state as described in the spec.
- Do not retry requests unless the spec explicitly calls for retry logic.

### 7.4 Authentication Headers

- If the spec defines an auth interceptor, implement it exactly as specified.
- If no interceptor is defined in the spec, do not add auth headers manually in service methods — wait for the interceptor spec.
- JWT token retrieval must use the exact mechanism specified (e.g., `AuthService.token()` signal, `localStorage.getItem()`).

### 7.5 HTTP Status Code Handling

- Only handle status codes explicitly mentioned in the spec.
- Do not add special handling for codes the spec doesn't mention (e.g., do not add a `401` redirect unless the spec defines it).

---

## 8. Code Quality Standards

### 8.1 TypeScript Strictness

- All variables, parameters, and return types must be explicitly typed.
- Do not use `any`. If a type is unknown, use `unknown` and narrow it.
- Avoid non-null assertions (`!`) unless the control flow makes nullability impossible and the spec context makes it safe.
- Enable and respect `strict: true` TypeScript settings.

### 8.2 Logic Integrity

Before implementing any method or function, answer:

1. What are all the possible inputs?
2. What is the expected output for each input?
3. What side effects does this produce?
4. Does every code path terminate correctly?
5. Are there any null/undefined dereferences possible?
6. Are there any race conditions (e.g., two API calls in flight)?

If any answer reveals a gap, resolve it by referencing the spec — not by inventing a solution.

### 8.3 No Dead Code

Do not write code that is unreachable, unused, or commented out. If a method is defined in the spec but its caller isn't built yet, note it in the compliance report under Known Deviations.

### 8.4 Consistent Error Boundaries

Every async operation that can fail must have an error handler. "It probably won't fail" is not an error strategy. The spec defines error states — implement them all.

### 8.5 Single Responsibility

Each file must do one thing. A component handles the view and local state. A service handles API communication or domain logic. A guard handles route authorization. Do not collapse these into each other unless the spec explicitly defines a combined file.

### 8.6 Imports

- Every import must be intentional and used.
- No wildcard imports (`import * as X`) unless required by a third-party library.
- Organize imports: Angular core → Angular common/router → Angular Material → RxJS → project internals.
- Unused imports must be removed.

---

## 9. Anti-Hallucination Protocol

This section defines specific thought checks the agent must perform to prevent hallucination.

### 9.1 The "Where Is This In the Spec?" Check

Before writing any of the following, stop and locate the exact sentence or section in the spec that requires it:

- A new file
- A new class or interface
- A new method or function
- A new API call
- A new route
- A new form field or validator
- A new UI element
- A new CSS class or style

If you cannot point to a specific spec location, **do not write it.**

### 9.2 The "Exact Match" Check

When implementing something the spec defines explicitly (e.g., a regex, a role name, a route path, a selector name), compare the spec value and your implementation character by character. They must be identical.

Examples of hallucination failures:

- Spec says route: `'auth'` → Agent writes `'login'` ❌
- Spec says selector: `app-auth-page` → Agent writes `app-auth` ❌
- Spec says role: `'RegisteredUser'` → Agent writes `'User'` ❌
- Spec says redirect delay: `800ms` → Agent writes `1000ms` ❌
- Spec says password regex: `/^(?=.*[A-Za-z])(?=.*\d).{8,}$/` → Agent writes a different pattern ❌
- Spec says `POST /auth/login` → Agent also creates `GET /auth/profile` without spec instruction ❌

### 9.3 The "Future Feature" Check

If the spec mentions future features, dashboards, or endpoints that are not part of the current spec task:

- Do not build them.
- Do not stub them beyond what the spec explicitly asks for.
- Reference them only in routing redirects if the spec directs those redirects.

### 9.4 The "Import Hallucination" Check

Do not import Angular modules, Material components, or RxJS operators you haven't explicitly needed for the spec's requirements. Every import in every file must be traceable to a specific spec requirement.

---

## 10. Ambiguity Escalation Protocol

When the spec is silent, unclear, or self-contradictory on a point:

### Step 1 — Identify the ambiguity

State it clearly: _"The spec does not define the exact error message for a 500 response from the register endpoint."_

### Step 2 — Choose the minimum-safe default

Select the option that:

- Does the least beyond what is written
- Does not introduce new behavior not implied by the spec
- Does not break any other spec requirement

### Step 3 — Flag it

Record the ambiguity and the chosen default in the compliance report under **Known Deviations with Defaults Applied**.

### Step 4 — Do not block progress

Unless the ambiguity prevents the feature from functioning at all, apply the default and proceed.

### Examples of Ambiguity Handling

| Ambiguity                                                      | Minimum-Safe Default                                             | Flag? |
| -------------------------------------------------------------- | ---------------------------------------------------------------- | ----- |
| Spec doesn't define a CSS class for the card container         | Use `auth-container` as implied by the spec's template structure | Yes   |
| Spec doesn't specify error message for 500 on register         | Use `'Registration failed.'` (the spec's catch-all fallback)     | Yes   |
| Spec says "console.log token and role" but doesn't show format | Use `console.log('Token:', token); console.log('Role:', role);`  | Yes   |
| Spec doesn't specify if `aria-describedby` uses a dynamic ID   | Generate ID as `fieldName-error` (e.g., `email-error`)           | Yes   |

---

## 11. Forbidden Patterns

The following patterns are explicitly forbidden regardless of their technical merit:

### FORBIDDEN-1: Speculative Generation

> Writing code for a feature not yet specified because "it will probably be needed later."

### FORBIDDEN-2: Implicit Enhancement

> Improving the UX, performance, or readability of spec-defined UI elements beyond what the spec describes. Example: adding a "show password" toggle on a password field the spec does not mention.

### FORBIDDEN-3: API Shape Assumption

> Adding request fields, response fields, query parameters, or headers to an API call because "the backend probably needs them."

### FORBIDDEN-4: Structural Invention

> Creating folders, modules, barrel exports (`index.ts`), shared utility files, or configuration files not defined in the spec's file structure.

### FORBIDDEN-5: Silently Skipping Requirements

> Omitting any spec requirement — accessibility attributes, a particular validation rule, a particular UI state — without documenting it as a Known Deviation.

### FORBIDDEN-6: Cross-Spec Contamination

> Modifying files created by a previous spec without explicit instruction in the current spec to modify them. Only append to `app.routes.ts` the route(s) defined in the current spec.

### FORBIDDEN-7: Invented Error Handling

> Adding `try/catch` blocks, `catchError` operators, HTTP interceptors, or retry logic beyond what the spec defines.

### FORBIDDEN-8: Framework Default Over-Application

> Adding `HttpClientModule` to `app.config.ts`, `provideAnimations()`, or other framework providers unless the spec requires them or they are documented in the project's bootstrap configuration.
> Do not remove existing providers from app.config.ts unless a spec explicitly instructs to do so.

---

## 12. Self-Review & Audit Requirements

After completing all 11 parts of the Implementation Protocol, the agent must perform a full self-audit.

### 12.1 Checklist Execution

Run through the spec sheet's own verification checklist (Section: "Final Self-Review" or equivalent). For every item:

- Read the checklist item.
- Find the relevant code in the implementation.
- Confirm the code satisfies the requirement exactly.
- Check the box only if it is genuinely satisfied.

### 12.2 Logic Trace Audit

For every user flow defined in the spec, trace it through the code:

```
Flow: [Flow Name]
1. User action: [describe]
2. Component method called: [method name]
3. State before: [describe signals/variables]
4. Service call: [method + endpoint]
5. On success path: [step by step]
6. On error path: [step by step]
7. Final state: [describe UI state]
8. Spec match: ✓ / ✗ [reason if ✗]
```

### 12.3 File Structure Audit

Compare the files actually created against the spec's file structure section. List any file in the spec not created, or any file created not in the spec.

### 12.4 Type Safety Audit

Confirm that no `any` types, implicit `any`s, or missing type annotations exist in the generated code.

### 12.5 Import Audit

For every file, confirm that every import is used and every used symbol is imported.

---

## 13. Completion Report Format

After the implementation is complete and the self-audit is done, output this report. Do not skip any section.

```
================================================================================
SPEC IMPLEMENTATION COMPLIANCE REPORT
Spec: [spec file name, e.g., auth-page.md]
Date: [date]
================================================================================

FILES CREATED
-------------
✓ [file path relative to src/]
✓ [file path]
... (list every file)

FILES MODIFIED (existing files updated per spec)
-------------------------------------------------
✓ [file path] — [brief description of change, e.g., "added /auth route"]
... (or "None" if no existing files were modified)

REQUIREMENTS IMPLEMENTED
------------------------
✓ [Requirement group, e.g., "Auth Infrastructure"]
  ✓ AuthService with token signal, role signal, isAuthenticated computed
  ✓ handleLogin() stores token, decodes, sets role signal
  ✓ logout() clears storage and resets signals
  ✓ isTokenExpired() checks exp claim
✓ [Next requirement group]
  ... (mirror the spec's checklist sections)

API INTEGRATION
---------------
✓ POST [path] — [DTO shape] → [response shape]
✓ POST [path] — ...
... (list every endpoint implemented)

LOGIC TRACES
------------
Flow: [flow name]
  Entry: [action]
  Path: [summarized trace]
  Result: ✓ Matches spec

... (one trace per major user flow)

KNOWN DEVIATIONS
----------------
[If none]:
  None. All requirements implemented exactly as specified.

[If any]:
  DEVIATION-1: [Requirement reference]
    Reason: [Why it could not be implemented as specified]
    Applied Default: [What was done instead, or "Not implemented"]
    Impact: [How this affects the feature]

  DEVIATION-2: ...

DEFAULTS APPLIED FOR AMBIGUITIES
---------------------------------
[If none]:
  None.

[If any]:
  AMBIGUITY-1: [What was unclear]
    Default Applied: [What was chosen]
    Rationale: [Why this is the minimum-safe choice]

CRITICAL RULE COMPLIANCE CONFIRMATION
--------------------------------------
☐ I confirm that every ✓ in the requirements section corresponds to code
  that exists and is correct. No requirement has been marked complete
  without implementation evidence.
☐ I confirm that no file, function, or feature was added beyond what
  the spec defines.
☐ I confirm that all API calls match the spec contracts exactly.
☐ I confirm that all regex validators are character-for-character matches
  to the spec.
☐ I confirm that all role-to-route mappings match the spec exactly.
================================================================================
```

---

## 14. Violations Reference — What NOT to Do

These are concrete examples of behaviors this document is designed to prevent. Treat each as a permanent anti-pattern.

### VIOLATION A — Creative Routing

**Spec says:** Route path is `'auth'`
**Violation:** Agent adds `'login'` as an alias because "users might type /login"
**Why wrong:** Not in spec. Creates untested behavior. Breaks spec intent.

### VIOLATION B — Helpful Extra Fields

**Spec says:** Register DTO is `{ email, password, firstName, lastName }`
**Violation:** Agent adds `phoneNumber?: string` field to the form because "registration forms usually have this"
**Why wrong:** Not in spec. Will cause TypeScript errors. Might break backend DTO binding.

### VIOLATION C — Assumed API Response

**Spec says:** Register endpoint returns `200 OK` with no body on success
**Violation:** Agent writes `tap(response => { if (response.success) { ... } })` because "APIs usually return a status object"
**Why wrong:** The spec says no body. This code will fail or produce undefined behavior.

### VIOLATION D — Silent Checklist Checkbox

**Spec says:** `[ ] aria-live="polite" implemented`
**Violation:** Agent checks this box but the attribute is missing from the template
**Why wrong:** Dishonest self-reporting. Accessibility violation. Critical failure.

### VIOLATION E — Proactive Infrastructure

**Spec says:** The file structure does not include a `core/interceptors/` folder
**Violation:** Agent creates `core/interceptors/error.interceptor.ts` "to improve error handling"
**Why wrong:** Not in spec. Adds complexity. May interfere with future spec deliveries that define their own interceptors.

### VIOLATION F — Regex Alteration

**Spec says:** Password pattern: `/^(?=.*[A-Za-z])(?=.*\d).{8,}$/`
**Violation:** Agent writes `/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$/` because "it seems more secure"
**Why wrong:** Changes validation behavior. Users who set passwords per the original spec would fail validation. Introduces divergence from backend's own password validation rules.

### VIOLATION G — Unspecified Redirect Timing

**Spec says:** Redirect after `800ms`
**Violation:** Agent uses `1500ms` because "it gives the user more time to read the success message"
**Why wrong:** The spec is explicit. The agent's preference is irrelevant.

### VIOLATION H — Bundled Implementation

**Violation:** Agent writes all 11 files simultaneously before verifying any of them
**Why wrong:** Errors in Part 1 (models) cascade into all subsequent parts. Part-by-part execution exists to catch these early.

---

## Closing Statement

This document is the contract between this project and every AI agent that works on it. It exists because:

- AI agents have a strong tendency to "improve" things not asked of them.
- AI agents hallucinate API contracts, validation rules, and file structures.
- AI agents skip requirements they find tedious (especially accessibility and error states).
- AI agents produce high-level code quickly but introduce subtle logical flaws.

By following this document precisely, an agent can deliver production-quality, spec-exact implementations that require zero rework and zero debugging. Every minute spent on spec compliance is ten minutes saved in code review and bug fixes.

**The measure of a good implementation is not how much was built. It is how precisely what was asked was built.**

---

_End of AGENT.md_

