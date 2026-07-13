# Automated State Mutation Report

This file chronologically logs automated E2E testing scenarios.

### [08:01:16] Anonymous Guest -> Book Room
- **Endpoint Triggered:** `POST /api/v1/bookings`
- **Payload:** `{"RoomTypeId":1,"CheckInDate":"2026-07-13T08:01:16.384334Z","CheckOutDate":"2026-07-15T08:01:16.384355Z","GuestName":"Automated Tester","GuestEmail":"auto@test.com"}`
- **Expected Result:** `201 Created`
- **Actual Result:** `Created`
- **Data Affected:** Created Booking #2

---

### [08:01:16] Front Desk -> Assign Room
- **Endpoint Triggered:** `PATCH /api/v1/bookings/2/room`
- **Payload:** `1`
- **Expected Result:** `200 OK`
- **Actual Result:** `OK`
- **Data Affected:** Assigned Room 1 to Booking 2

---

### [08:01:16] Front Desk -> Check In Guest
- **Endpoint Triggered:** `PATCH /api/v1/bookings/2`
- **Payload:** `{ status: CheckedIn }`
- **Expected Result:** `200 OK`
- **Actual Result:** `OK`
- **Data Affected:** Booking 2 status changed to CheckedIn

---

### [08:01:16] Guest via FrontDesk -> Order Food
- **Endpoint Triggered:** `POST /api/v1/orders`
- **Payload:** `{"BookingId":2,"Items":[{"MenuItemId":1,"Quantity":2}]}`
- **Expected Result:** `200 OK`
- **Actual Result:** `OK`
- **Data Affected:** Created food order and attached to folio

---

### [08:01:16] Front Desk -> Check Out Guest
- **Endpoint Triggered:** `PATCH /api/v1/bookings/2`
- **Payload:** `{ status: CheckedOut }`
- **Expected Result:** `200 OK`
- **Actual Result:** `OK`
- **Data Affected:** Generated Folio, changed status to CheckedOut, freed room.

---

### [08:01:16] Automated Sweeper -> Coverage Ping
- **Endpoint Triggered:** `GET /api/v1/bookings`
- **Payload:** `null`
- **Expected Result:** `200 OK (or 400)`
- **Actual Result:** `OK`
- **Data Affected:** No specific data modified.

---

### [08:01:16] Automated Sweeper -> Coverage Ping
- **Endpoint Triggered:** `GET /api/v1/rooms`
- **Payload:** `null`
- **Expected Result:** `200 OK (or 400)`
- **Actual Result:** `OK`
- **Data Affected:** No specific data modified.

---

### [08:01:16] Automated Sweeper -> Coverage Ping
- **Endpoint Triggered:** `GET /api/v1/room-types`
- **Payload:** `null`
- **Expected Result:** `200 OK (or 400)`
- **Actual Result:** `OK`
- **Data Affected:** No specific data modified.

---

### [08:01:16] Automated Sweeper -> Coverage Ping
- **Endpoint Triggered:** `GET /api/v1/staff`
- **Payload:** `null`
- **Expected Result:** `200 OK (or 400)`
- **Actual Result:** `OK`
- **Data Affected:** No specific data modified.

---

### [08:01:16] Automated Sweeper -> Coverage Ping
- **Endpoint Triggered:** `GET /api/v1/menu-items`
- **Payload:** `null`
- **Expected Result:** `200 OK (or 400)`
- **Actual Result:** `OK`
- **Data Affected:** No specific data modified.

---

### [08:01:16] Automated Sweeper -> Coverage Ping
- **Endpoint Triggered:** `GET /api/v1/orders`
- **Payload:** `null`
- **Expected Result:** `200 OK (or 400)`
- **Actual Result:** `OK`
- **Data Affected:** No specific data modified.

---

### [08:01:16] Automated Sweeper -> Coverage Ping
- **Endpoint Triggered:** `GET /api/v1/housekeeping`
- **Payload:** `null`
- **Expected Result:** `200 OK (or 400)`
- **Actual Result:** `OK`
- **Data Affected:** No specific data modified.

---

### [08:01:16] Automated Sweeper -> Coverage Ping
- **Endpoint Triggered:** `GET /api/v1/feedback`
- **Payload:** `null`
- **Expected Result:** `200 OK (or 400)`
- **Actual Result:** `OK`
- **Data Affected:** No specific data modified.

---

