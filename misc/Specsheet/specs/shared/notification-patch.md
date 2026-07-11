# Patch Specsheet: SignalR Notification Service – Connection Fix

## 1. Purpose
- Fix the SignalR connection error in `NotificationService` caused by passing a `ws://` URL to the SignalR client, which triggers HTTP-based negotiation that fails.
- The backend WebSocket endpoint works directly (as verified in Postman), so we will configure the SignalR client to skip negotiation and use WebSocket transport directly.
- Also correct the hub URL to use the proper HTTP base URL format (not `ws://`).

## 2. File to Modify
- `src/app/core/services/notification.service.ts`

## 3. Changes

### 3.1 Import `HttpTransportType`
Add at the top:
```typescript
import { HubConnection, HubConnectionBuilder, HttpTransportType, LogLevel } from '@microsoft/signalr';
```

### 3.2 Update URL Construction
Replace:
```typescript
const wsUrl = environment.baseUrl.replace('http', 'ws') + '/notifications';
```
with:
```typescript
const hubUrl = environment.baseUrl.replace(/\/api\/v1$/, '') + '/notifications';
// Example: 'http://localhost:5264/api/v1' → 'http://localhost:5264/notifications'
```

### 3.3 Update `HubConnectionBuilder` Options
Replace the `withUrl` call:
```typescript
.withUrl(wsUrl, { accessTokenFactory: () => token })
```
with:
```typescript
.withUrl(hubUrl, {
  accessTokenFactory: () => token,
  skipNegotiation: true,
  transport: HttpTransportType.WebSockets,
})
```

This tells SignalR to skip the HTTP negotiation and connect directly via WebSocket, using the provided token as a query parameter or header depending on SignalR version. The token will be sent as a query string parameter `access_token` by default. However, the backend may expect it as a Bearer header. If the backend is configured to read the token from the query string during WebSocket handshake (which is standard for SignalR), this will work. If not, we may need to pass the token differently. But the Postman success with Bearer header suggests the backend expects the token in the `Authorization: Bearer` header during the WebSocket upgrade. SignalR's default when `skipNegotiation: true` sends the token as a query parameter `?access_token=...`. If the backend only accepts header-based auth, we need to set the `AccessTokenFactory` to return the token, and the SignalR client will send it in the `Authorization` header? Actually, SignalR does not send the token as a header for WebSocket transport; it uses query parameter. To pass token in the header during WebSocket connection, we can't, because the WebSocket API does not support custom headers. So the backend must accept the token via query parameter. If not, we'll need to adjust backend. But the user's Postman test used a Bearer header? In Postman, when connecting to a WebSocket, you can set a header. That might work. Browsers do not allow custom headers on WebSocket connections. So SignalR uses the query parameter approach. This is standard and should be supported by the backend if it's a proper SignalR hub. Since the backend team mentioned SignalR integration and token for authorization, they likely accept the token in the query string.

So the above fix should resolve the connection issue.

### 3.4 Update `startConnection` method signature (if needed)
No other changes; the rest of the service remains identical.

## 4. Self‑Review Checklist
- [ ] The `NotificationService` now builds the hub URL correctly (e.g., `http://localhost:5264/notifications`).
- [ ] Connection uses `skipNegotiation: true` and `WebSockets` transport.
- [ ] Authentication token is passed via `accessTokenFactory` (sent as query parameter).
- [ ] Connection establishes successfully without negotiation errors.
- [ ] Incoming real‑time events trigger the observables as expected.
- [ ] Custom snackbar notifications appear for new tasks/orders.
- [ ] No console errors related to SignalR.

## 5. Integration Notes
- If the backend requires the token in the `Authorization` header rather than a query parameter, the backend must be updated to support the query parameter approach, because browsers do not allow custom headers on WebSocket connections. This is a standard SignalR behavior. If the backend team insists on header‑only auth, they would need to configure ASP.NET Core to read the token from the query string for WebSocket requests (which is configurable). We'll proceed with this patch assuming the backend supports SignalR's default token passing via query parameter. The Postman test with a header might have been a simple raw WebSocket, not a SignalR hub. If issues persist, we can investigate further, but this is the correct frontend fix.