# Hotel Management System - Frontend to Backend Mapping

This document provides an exhaustive map of the endpoints required by each frontend role and dashboard section, based on the backend controllers, services, API documentation, and Role-Based Access Control (RBAC) definitions.

---

## 1. Operations Side

### 1.1 Admin Dashboard

The Admin acts as the superuser with visibility over finances, system health, and staff.

**Flow & Services:**

- The frontend will utilize the `ApiService` to fetch analytical data and staff records.
- **Role:** `Admin`

**Endpoints:**

- `GET /api/v1/analytics`
  - **Purpose**: Fetches the core executive summary (Occupancy rate, RevPAR, Total Revenue, non-room expenditure).
  - **Parameters**:

  ```json
  {
    "startDate": {
      "In": "Query",
      "Type": "string",
      "Format": "date-time (expects a valid ISO 8601 date-time string, e.g., 2026-06-25T00:00:00Z)"
    },
    "endDate": {
      "In": "Query",
      "Type": "string",
      "Format": "date-time (expects a valid ISO 8601 date-time string, e.g., 2026-06-25T23:59:59Z)"
    }
  }
  ```
  - **Roles**: `Admin`
  - **Response Body** :
    {
    "occupancyRate": 57.14,
    "averageDailyRate": 230.38,
    "revPAR": 12096.43,
    "totalRevenue": 183171,
    "grossTurnover": 7035,
    "averageLengthOfStay": 14.29,
    "cancellationRate": 16.36,
    "guestSatisfactionScore": 77.78,
    "averageHousekeepingTurnaroundMinutes": 100,
    "nonRoomExpenditure": {
    "totalFoodSpend": 866,
    "totalAmenitySpend": 1230,
    "highestSpendCategory": "Amenities"
    }
    }

- `GET /api/v1/auditlogs`
  - **Purpose**: View system-wide operations logs in a paginated table.
  - **Parameters**: `pageNumber=1`, `pageSize=10`, `sortBy=null`, `sortDescending=false`
  - **Roles**: `Admin`

- `GET /api/v1/auditlogs/{id}`
  - **Purpose**: View details of a specific audit log.
  - **Parameters**: `id` (int, path)
  - **Roles**: `Admin`

- `GET /api/v1/staff`
  - **Purpose**: View staff directory.
  - **Parameters**: `pageNumber=1`, `pageSize=10`, `includeFired=false`, `sortBy=null`, `sortDescending=false`
  - **Roles**: `Admin`

- `POST /api/v1/staff`
  - **Purpose**: Hire a new staff member.
  - **Parameters**: `StaffRegisterRequestDTO` body (`firstName`, `lastName`, `email`, `password`, `role`)
  - **Roles**: `Admin`

- `PATCH /api/v1/staff/{id}`
  - **Purpose**: Update staff details.
  - **Parameters**: `id` (int, path), `UpdateStaffDTO` body (`firstName`, `lastName`, `role`, `isActive`)
  - **Roles**: `Admin`

- `DELETE /api/v1/staff/{id}`
  - **Purpose**: Fire or terminate a staff member.
  - **Parameters**: `id` (int, path)
  - **Roles**: `Admin`

- `GET /api/v1/billing`
  - **Purpose**: View overall financial ledgers.
  - **Parameters**: `paymentStatus`, `startDate`, `endDate`, `search`, `detailed=false`, `pageNumber=1`, `pageSize=10`, `sortBy=null`, `sortDescending=false`
  - **Roles**: `Admin`, `FrontDesk`

- `GET /api/v1/billing/receipts`
  - **Purpose**: Fetch receipt history.
  - **Parameters**: `startDate`, `endDate`, `pageNumber=1`, `pageSize=10`, `sortBy=null`, `sortDescending=false`
  - **Roles**: `Admin`

- `PATCH /api/v1/feedback/{id}/moderate`
  - **Purpose**: Moderate a review before it is displayed publicly.
  - **Parameters**: `id` (int, path), `ModerateFeedbackRequestDTO` body (`isApproved`, `moderationNotes`)
  - **Roles**: `Admin`

- `GET /api/v1/housekeeping`
  - **Purpose**: Use the various status to give admin the snapshot of current houskeeeping progress and report.
  - **Parameters**:

  ```json
  {
    "pageNumber": {
      "In": "Query",
      "Type": "integer",
      "Format": "int32",
      "Default": 1
    },
    "pageSize": {
      "In": "Query",
      "Type": "integer",
      "Format": "int32",
      "Default": 10
    },
    "status": {
      "In": "Query",
      "Type": "string",
      "Format": "Optional status filter string (pending, inprogress, completed)"
    },
    "sortBy": {
      "In": "Query",
      "Type": "string",
      "Format": "Optional field name to sort results by"
    },
    "sortDescending": {
      "In": "Query",
      "Type": "boolean",
      "Default": false
    }
  }
  ```

- `GET /api/v1/maintenance`
  - **Purpose**: Similar to housekeeping, show the current progress as tasks are getting generated and finished.
  - **Parameters**:
    {"pageNumber": {"In": "Query", "Type": "integer", "Format": "int32", "Default": 1}, "pageSize": {"In": "Query", "Type": "integer", "Format": "int32", "Default": 10}, "status": {"In": "Query", "Type": "string", "Format": "Optional status filter string"}, "sortBy": {"In": "Query", "Type": "string", "Format": "Optional field name to sort results by"}, "sortDescending": {"In": "Query", "Type": "boolean", "Default": false}}

_(Note: Admins implicitly have access to all endpoints available to Front Desk, Kitchen, Housekeeping, and Maintenance.)_

---

### 1.2 Front Desk Dashboard

The Front Desk is the primary orchestration role (Check-ins, Check-outs, billing).

**Flow & Services:**

- When a guest arrives, the front desk uses the `BookingsService` to find the booking and the `RoomsService` to identify an available room.
- They check the guest in, binding the room to the booking.
- During checkout, they use the `BillingService` to generate the folio, process payment, and then explicitly complete the checkout workflow.
- **Role:** `FrontDesk`, `Admin`

**Endpoints:**

- `GET /api/v1/bookings`
  - **Purpose**: Fetch lists of arrivals, departures, or in-house guests.
  - **Parameters**: `status` ("Booked", "CheckedIn", "CheckedOut", "Cancelled"),`guestQuery` (search by name / email), `pageNumber=1`, `pageSize=10`, `sortBy=null`, `sortDescending=false`
- `POST /api/v1/bookings`
  - **Purpose**: Create a walk-in reservation.
  - **Parameters**: `CreateBookingDTO` body (`guestId`, `roomTypeId`, `checkInDate`, `checkOutDate`)

- `POST /api/v1/bookings/{id}/checkin?specificRoomId={roomId}`
  - **Purpose**: Assigns the physical room exclusively during this step and officially checks the guest into the system.
  - **Parameters**: `id` (int, path), `specificRoomId` (int, query)

- `PATCH /api/v1/bookings/{id}/extend-stay`
  - **Purpose**: Extend the stay of an already checked-in user (updates check-out date).
  - **Parameters**: `id` (int, path), `UpdateBookingDTO` body (`checkOutDate`)

- `GET /api/v1/billing/{bookingId}`
  - **Purpose**: STEP 1 OF CHECKOUT. Generate the exact folio of the booking prior to checkout.
  - **Parameters**: `bookingId` (int, path)

- `POST /api/v1/billing/{bookingId}/pay`
  - **Purpose**: STEP 2 OF CHECKOUT. Process the payment for the folio.
  - **Parameters**: `bookingId` (int, path), `PaymentRequestDTO` body

- `POST /api/v1/bookings/{id}/checkout`
  - **Purpose**: STEP 3 OF CHECKOUT. Finalize the checkout only if the payment was successful.
  - **Parameters**: `id` (int, path)

- `GET /api/v1/room-types/available` & `GET /api/v1/rooms/available-for-booking/{bookingId}`
  - **Purpose**: Find physical inventory for walk-ins or room assignment.
  - **Parameters**: `bookingId` (int, path)

- `GET /api/v1/guests`
  - **Purpose**: Search for guest profiles.
  - **Parameters**: `search`, `status`, `pageNumber=1`, `pageSize=10`

- `POST /api/v1/orders`
  - **Purpose**: Front desk placing a food order on behalf of a guest.
  - **Parameters**: `CreateFoodOrderDTO` body (`bookingId`, `menuItemIds`, `specialInstructions`)

- `POST /api/v1/housekeeping/trigger/{roomId}`
  - **Purpose**: Front desk creating a housekeeping ticket on behalf of a guest request.
  - **Parameters**: `roomId` (int, path), `CreateHousekeepingTaskDTO` body (`notes`)

- `POST /api/v1/maintenance/trigger/{roomId}`
  - **Purpose**: Front desk reporting a physical issue in a room.
  - **Parameters**: `roomId` (int, path), `CreateMaintenanceTaskDTO` body (`description`, `priority`)

- `DELETE /api/v1/bookings/{id}/cancel`
  - **Purpose**: Front desk cancelling a guest's reservation.
  - **Parameters**: `id` (int, path)

---

### 1.3 Maintenance Dashboard

Bob focuses purely on resolving physical property issues.

**Flow & Services:**

- Uses the `SignalRNotificationService` via `ws://localhost:5264/notifications` to receive real-time ticket alerts.
- Uses `MaintenanceService` to view and update tickets.
- **Role:** `Maintenance`, `Admin`

**Endpoints:**

- `GET /api/v1/maintenance/active`
  - **Purpose**: Fetch pending maintenance tickets.
  - **Parameters**: `pageNumber=1`, `pageSize=10`, `sortBy=null`, `sortDescending=false`

- `PATCH /api/v1/maintenance/{id}/status`
  - **Purpose**: Update a ticket state (`Pending` -> `InProgress` -> `Resolved`).
  - **Parameters**: `id` (int, path), `UpdateMaintenanceStatusDTO` body (`status`)

---

### 1.4 Housekeeping Dashboard

Maria focuses on turning over rooms and fulfilling guest amenity requests.

**Flow & Services:**

- Uses the `SignalRNotificationService` via `ws://localhost:5264/notifications` to receive real-time checkout alerts requiring room flips.
- Uses `HousekeepingService` to view and update cleaning tasks.
- If an item is broken in a room, she can trigger a maintenance ticket.
- **Role:** `Housekeeping`, `Admin`

**Endpoints:**

- `GET /api/v1/housekeeping/active`
  - **Purpose**: Fetch rooms needing cleaning (often populated by the checkout orchestrator).
  - **Parameters**: `pageNumber=1`, `pageSize=10`, `sortBy=null`, `sortDescending=false`

- `PATCH /api/v1/housekeeping/{id}/status`
  - **Purpose**: Update cleaning task (`Pending` -> `InProgress` -> `Completed`).
  - **Parameters**: `id` (int, path), `UpdateHousekeepingStatusDTO` body (`status`)

- `POST /api/v1/maintenance/trigger/{roomId}`
  - **Purpose**: Housekeeper creating a ticket for a broken item discovered while cleaning.
  - **Parameters**: `roomId` (int, path), `CreateMaintenanceTaskDTO` body (`description`, `priority`)

---

### 1.5 Kitchen Dashboard

Chef Gordon manages food orders.

**Flow & Services:**

- Uses the `SignalRNotificationService` via `ws://localhost:5264/notifications` to receive real-time food orders.
- Uses `KitchenService` to view pending orders and update their statuses.
- **Role:** `Kitchen`, `Admin`

**Endpoints:**

- `GET /api/v1/orders`
  - **Purpose**: Fetch the active orders queue.
  - **Parameters**: `status=Pending` (or `Preparing`, `Delivered`), `pageNumber=1`, `pageSize=10`, `sortDescending=false`

- `PATCH /api/v1/orders/{id}`
  - **Purpose**: Move order from `Pending` -> `Preparing` -> `Delivered` based on `FoodOrderStatus`.
  - **Parameters**: `id` (int, path), `UpdateOrderStatusDTO` body (`status`)

- `PATCH /api/v1/menu-items/{id}/status`
  - **Purpose**: Rapidly 86 an item (mark it unavailable).
  - **Parameters**: `id` (int, path), `isAvailable=false` (query)

---

## 2. Customer Side

### 2.1 Landing Page

The unauthenticated public view for users.

**Flow & Services:**

- Browsing rooms, menus, and amenities does not require a JWT. Uses public services.
- **Role:** `AllowAnonymous`

**Endpoints:**

- `GET /api/v1/room-types`
  - **Purpose**: Browse available room types and their descriptions.
  - **Parameters**: `pageNumber=1`, `pageSize=10`

- `GET /api/v1/menu-items`
  - **Purpose**: View the restaurant menu.
  - **Parameters**: None

- `GET /api/v1/amenities`
  - **Purpose**: View the spa/gym services.
  - **Parameters**: None

- `POST /api/v1/bookings`
  - **Purpose**: Allow anonymous/unregistered users to create a new reservation online.
  - **Parameters**: `CreateBookingDTO` body (`roomTypeId`, `checkInDate`, `checkOutDate`)

- `POST /api/v1/bookings/{id}/amenities`
  - **Purpose**: Purchase a spa pass or gym access for a booking as an anonymous user.
  - **Parameters**: `id` (int, path), `SubscribeAmenityDTO` body (`amenityId`)

---

### 2.2 Registered User Dashboard

Sarah's personal portal after creating an account.

**Flow & Services:**

- The backend automatically intercepts the user's JWT to filter their specific bookings and data.
- **Role:** `RegisteredUser`

**Endpoints:**

- `GET /api/v1/auth/me`
  - **Purpose**: Fetch user profile.
  - **Parameters**: None

- `GET /api/v1/bookings`
  - **Purpose**: View upcoming and past stays. (Backend automatically filters by `UserId` claim).
  - **Parameters**: `pageNumber=1`, `pageSize=10`, `sortBy=null`, `sortDescending=false`

- `POST /api/v1/bookings`
  - **Purpose**: Create a new reservation online.
  - **Parameters**: `CreateBookingDTO` body (`roomTypeId`, `checkInDate`, `checkOutDate`)

- `POST /api/v1/bookings/{id}/amenities`
  - **Purpose**: Purchase a spa pass or gym access.
  - **Parameters**: `id` (int, path), `SubscribeAmenityDTO` body (`amenityId`)

- `GET /api/v1/housekeeping`
  - **Purpose**: Get your housekeeping request status
  - **Parameters**: `pageNumber=1`, `pageSize=10`, `sortBy=null`, `sortDescending=false`

- `POST /api/v1/housekeeping/trigger/{roomId}`
  - **Purpose**: Request extra towels or room cleaning via the portal.
  - **Parameters**: `CreateHousekeepingTaskDTO` body (`notes`)

  - `GET /api/v1/maintenance`
  - **Purpose**: Get your housekeeping request status
  - **Parameters**: `pageNumber=1`, `pageSize=10`, `sortBy=null`, `sortDescending=false`

  - `POST /api/v1/maintenance/trigger/{roomId}`
  - **Purpose**: Get status of your raised ticket
  - **Parameters**: `CreateHousekeepingTaskDTO` body (`notes`)

  - `GET /api/v1/orders`
  - **Purpose**: Fetch orders posted by user (backend onyl shows order assoiciated with the user)
  - **Parameters**: `status=Pending` (or `Preparing`, `Delivered`), `pageNumber=1`, `pageSize=10`, `sortDescending=false`

- `POST /api/v1/orders`
  - **Purpose**: Order room service via the app.
  - **Parameters**: `CreateFoodOrderDTO` body (`bookingId`, `menuItemIds`, `specialInstructions`)

- `DELETE /api/v1/bookings/{id}/cancel`
  - **Purpose**: Cancel an upcoming reservation.
  - **Parameters**: `id` (int, path)

- `POST /api/v1/feedback`
  - **Purpose**: Submit post-stay feedback.
  - **Parameters**: `CreateFeedbackDTO` body (`bookingId`, `rating`, `comments`)

---

## 3. Common

### 3.1 Common Unified Login Page

Handles auth for both staff and guests.

**Flow & Services:**

- Forms post to the `AuthService` endpoints to exchange credentials for a JWT.
- The JWT is then stored in `localStorage` or memory, and its claims define the frontend routing payload.
- **Role:** `AllowAnonymous`

**Endpoints:**

- `POST /api/v1/auth/login`
  - **Purpose**: Exchanges email/password for a JWT.
  - **Parameters**: `LoginRequestDTO` body (`email`, `password`)

- `POST /api/v1/auth/register`
  - **Purpose**: Registers a new public guest account (not staff).
  - **Parameters**: `RegisterRequestDTO` body (`email`, `password`, `firstName`, `lastName`)
