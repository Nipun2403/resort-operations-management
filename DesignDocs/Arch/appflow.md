# Hotel Management System - Business Flow

This document outlines the business processes and operational flows as implemented in the application code.

## 0. Auth Flow


- Single login system for both Customer + Operations
- JWT authentication
- After login:
  - CUSTOMER → /customer
  - OPERATIONS → /operations/{respective-role}

```mermaid
graph TD
   A[ login via /auth] -->  B{Verify Token / Credentials}
   B-- Invalid --> C[Throw Error]
   B -- Valid --> D{Route Based on Role}
   D -- RegisteredUser --> E[ /user/dashboard ]
   D -- Admin --> F[ /operations/admin/dashboard ]
   D -- Front Desk --> G[ /operations/front-desk/dashboard ]
   D -- Housekeeping --> H[ /operations/housekeeping/dashboard ]
   D -- Maintenance --> I[ /operations/maintenance/dashboard ]
   D -- Kitchen --> J[ /operations/kitchen/dashboard ]

```

## 1. Reservation & Booking Flow
**Entry Point:** `BookingService.CreateBookingAsync`

```mermaid
graph TD
    A[Start: Create Booking] --> B{Date Valid?}
    B -- No --> C[Throw ArgumentException]
    B -- Yes --> D{Duplicate Check}
    D -- Duplicate Found --> E[Throw InvalidOperationException]
    D -- Clear --> F{Room Type Availability}
    F -- Insufficient --> G[Throw InvalidOperationException]
    F -- Available --> H{Capacity Check}
    H -- Exceeded --> I[Throw ArgumentException]
    H -- OK --> J[Create Booking Record]
    J --> K[Lock In Prices & Amenities]
    K --> L[Status: Booked / Payment: Pending]
    L --> M[End]
```

- **Validation**: 
    - Ensures check-in dates are not in the past and check-out is after check-in.
    - Prevents duplicate bookings from the same email within a 1-minute window.
- **Availability Check**:
    - Verifies that the requested number of rooms for each `RoomType` are available for the selected date range to prevent overbooking.
    - Validates that the total occupancy capacity of selected rooms accommodates the guest count.
- **Booking Creation**:
    - Handles both `RegisteredUser` and `Guest` origins.
    - Locks in the `BasePrice` of the room type at the time of booking.
    - Associates optional amenities with their prices at the time of purchase.
    - Sets initial status to `Booked` and payment to `Pending`.

## 2. Check-In Flow
**Entry Point:** `BookingService.CheckInGuestAsync`

```mermaid
graph TD
    A[Start: Check-In] --> B{Arrival Date == Today?}
    B -- No --> C[Throw InvalidOperationException]
    B -- Yes --> D{Room Assigned?}
    D -- No --> E[Auto-Assign Available Room of Type]
    D -- Yes --> F{Validate Specific Room}
    F -- Invalid/Occupied --> G[Throw Exception]
    F -- Valid --> H[Assign Room]
    E --> H
    H --> I[Status: CheckedIn]
    I --> J[End]
```

- **Arrival Validation**: Guests can only check in on their scheduled arrival date.
- **Room Assignment**:
    - If a room was not pre-assigned, the system automatically assigns an available room of the correct type that is not occupied by another active booking.
    - If a room was pre-assigned, it validates that the room is active and matches the booked type.
- **Status Update**: The booking status is transitioned from `Booked` to `CheckedIn`.

## 3. Guest Stay Operations

### 3.1 Room Service & Dining
**Entry Point:** `OrderService.CreateOrderAsync`

```mermaid
graph TD
    A[Start: Place Order] --> B{Status == CheckedIn?}
    B -- No --> C[Throw InvalidOperationException]
    B -- Yes --> D{PaymentStatus != Paid?}
    D -- Paid --> E[Throw InvalidOperationException]
    D -- OK --> F{Duplicate Order?}
    F -- Exists --> G[Throw InvalidOperationException]
    F -- New --> H[Create FoodOrder & Lock Prices]
    H --> I[Send Kitchen Alert via SignalR]
    I --> J[End]
```
- **Eligibility**: Only guests with a `CheckedIn` status can place food orders.
- **Price Locking**: MenuItem prices are captured at the moment of the order to ensure billing consistency.
- **Notifications**: Triggers a kitchen alert via `INotificationService` upon order placement.

### 3.2 Housekeeping Services
**Entry Point:** `HousekeepingService`

```mermaid
graph TD
    A[Request Service] --> B{Request Origin}
    B -- Guest --> C{Active Booking in Room?}
    B -- Staff --> D[Create Task]
    B -- Auto-Checkout --> D
    C -- No --> E[Throw ArgumentException]
    C -- Yes --> F{Identical Task Pending?}
    F -- Yes --> G[Throw InvalidOperationException]
    F -- No --> D
    D --> H[Status: Pending]
    H --> I[Status: InProgress]
    I --> J[Status: Completed]
```
- **Guest Requests**: Checked-in guests can request cleaning/services for their assigned room.
- **Internal Requests**: Staff can create housekeeping tasks for any location.
- **Workflow**: Tasks move through `Pending` $\rightarrow$ `InProgress` $\rightarrow$ `Completed`.

### 3.3 Room Maintenance
**Entry Point:** `MaintenanceService`

```mermaid
graph TD
    A[Request Maintenance] --> B{Description Valid?}
    B -- No --> C[Throw ArgumentException]
    B -- Yes --> D{Origin}
    D -- Guest --> E{CheckedIn in Room?}
    D -- Staff/Sys --> F[Create Ticket]
    E -- No --> G[Throw ArgumentException]
    E -- Yes --> F
    F --> H[Status: Pending]
    H --> I[Send Maintenance Alert]
    I --> J[End]
```
- **Ticket Creation**: Guests (if checked in) or staff can create maintenance tickets.
- **Validation**: Descriptions must contain at least one alphabet character to prevent empty/symbol-only tickets.
- **Notifications**: Triggers maintenance alerts for the engineering team.

## 4. Billing & Settlement Flow
**Entry Point:** `BillingService`

```mermaid
graph TD
    A[Start: Process Payment] --> B[Generate Folio]
    B --> C{Sum: RoomPrice*Nights + Food + Amenities}
    C --> D{Payment Amount == Total Bill?}
    D -- No --> E[Throw InvalidOperationException]
    D -- Yes --> F[Create Receipt Record]
    F --> G[Update PaymentStatus: Paid]
    G --> H[End]
```

- **Folio Generation**:
    - Calculates total cost: `(Room Base Price * Number of Nights) + Σ(Food Order Items) + Σ(Booking Amenities)`.
    - Handles refunded status for cancelled bookings that were previously paid.
- **Payment Processing**:
    - Validates that the payment amount exactly matches the total bill.
    - Generates a `Receipt` record upon successful payment.
    - Updates `PaymentStatus` to `Paid`.

## 5. Checkout Flow
**Entry Point:** `BookingService.UnifiedCheckoutAsync`

```mermaid
graph TD
    A[Start: Checkout] --> B{PaymentStatus == Paid?}
    B -- No --> C[Throw InvalidOperationException]
    B -- Yes --> D[Update Status: CheckedOut]
    D --> E[Create Housekeeping Task: CheckoutAutomated]
    E --> F[Send Housekeeping Alert]
    F --> G[Return Final Folio]
    G --> H[End]
```

- **Payment Verification**: Checkout is strictly forbidden unless the `PaymentStatus` is `Paid`.
- **Process**:
    - Updates booking status to `CheckedOut`.
    - **Automated Trigger**: Automatically creates a `Housekeeping` task with `CheckoutAutomated` origin to prepare the room for the next guest.
    - **Notification**: Sends a housekeeping alert that the room is ready for cleaning.
    - **Finalization**: Returns the final billing folio to the guest.

## 6. Stay Extension
**Entry Point:** `BookingService.ExtendStayAsync`

```mermaid
graph TD
    A[Start: Extend Stay] --> B{Status == CheckedIn?}
    B -- No --> C[Throw InvalidOperationException]
    B -- Yes --> D{New Date > Current Checkout?}
    D -- No --> E[Throw ArgumentException]
    D -- Yes --> F{Room Available for New Range?}
    F -- Conflict --> G[Throw ArgumentException]
    F -- OK --> H[Update CheckOutDate]
    H --> I[End]
```
- **Eligibility**: Only guests currently `CheckedIn` can extend.
- **Conflict Check**: Verifies that the room is not reserved by another guest for the extension period.
- **Update**: Updates the `CheckOutDate` and saves the booking.