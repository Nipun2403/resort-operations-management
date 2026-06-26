# API Documentation — Hotel Management System

> Base URL: `https://<host>/api/v1`

---

## Analytics

### GET /api/v1/analytics
- **Purpose**: Fetches the core executive summary (Occupancy rate, RevPAR, Total Revenue, non-room expenditure).
- **Parameters**:
```json
{
  "startDate": {
    "In": "Query",
    "Type": "string",
    "Format": "date-time (expects a valid ISO 8601 date-time string, e.g., 2026-06-25T00:00:00Z)",
    "Required": false
  },
  "endDate": {
    "In": "Query",
    "Type": "string",
    "Format": "date-time (expects a valid ISO 8601 date-time string, e.g., 2026-06-25T23:59:59Z)",
    "Required": false
  }
}
```
- **Roles**: Admin
- **Request Body**: None
- **Response Body**:
```json
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
```


---

## Auth

### POST /api/v1/auth/register
- **Purpose**: Registers a new user account (guest self-registration).
- **Parameters**: None
- **Roles**: Public
- **Request Body**:
```json
{
  "email": "string (Required, EmailAddress, MaxLength 100)",
  "password": "string (Required, MinLength 6, MaxLength 100)",
  "firstName": "string (Required, MaxLength 100)",
  "lastName": "string (Required, MaxLength 100)"
}
```
- **Response Body**:
```json
{
  "message": "string"
}
```

### POST /api/v1/auth/login
- **Purpose**: Authenticates a user and returns a JWT token with user details.
- **Parameters**: None
- **Roles**: Public
- **Request Body**:
```json
{
  "email": "string (Required, EmailAddress, MaxLength 100)",
  "password": "string (Required, MaxLength 100)"
}
```
- **Response Body**:
```json
{
  "token": "string",
  "role": "string",
  "firstName": "string",
  "lastName": "string"
}
```

### GET /api/v1/auth/me
- **Purpose**: Returns the current authenticated user's identity and claims.
- **Parameters**: None
- **Roles**: Authenticated (any valid JWT)
- **Request Body**: None
- **Response Body**:
```json
{
  "isAuthenticated": true,
  "name": "string",
  "claims": [
    {
      "type": "string",
      "value": "string"
    }
  ]
}
```


---

## Audit Logs

### GET /api/v1/auditlogs
- **Purpose**: Retrieves a paginated, sortable list of all audit logs.
- **Parameters**:
```json
{
  "pageNumber": { "In": "Query", "Type": "int", "Required": false, "Default": 1 },
  "pageSize": { "In": "Query", "Type": "int", "Required": false, "Default": 10 },
  "sortBy": { "In": "Query", "Type": "string?", "Required": false, "Default": null },
  "sortDescending": { "In": "Query", "Type": "bool", "Required": false, "Default": false }
}
```
- **Roles**: Admin
- **Request Body**: None
- **Response Body**:
```json
{
  "totalCount": 0,
  "pageNumber": 1,
  "pageSize": 10,
  "data": [
    {
      "id": 0,
      "entityName": "string",
      "action": "string",
      "recordId": { "object": "JsonDocument" },
      "oldValues": { "object": "JsonDocument" },
      "newValues": { "object": "JsonDocument" },
      "changedByEmail": "string",
      "changedByName": "string",
      "timestamp": "string (ISO 8601 date-time)"
    }
  ]
}
```

### GET /api/v1/auditlogs/{id}
- **Purpose**: Retrieves a specific audit log by its ID.
- **Parameters**:
```json
{
  "id": { "In": "Path", "Type": "int", "Required": true }
}
```
- **Roles**: Admin
- **Request Body**: None
- **Response Body**:
```json
{
  "id": 0,
  "entityName": "string",
  "action": "string",
  "recordId": { "object": "JsonDocument" },
  "oldValues": { "object": "JsonDocument" },
  "newValues": { "object": "JsonDocument" },
  "changedByEmail": "string",
  "changedByName": "string",
  "timestamp": "string (ISO 8601 date-time)"
}
```


---

## Rooms

### GET /api/v1/rooms
- **Purpose**: Retrieves a paginated list of rooms with optional filtering by room type, inclusion of retired rooms, and sorting.
- **Parameters**:
```json
{
  "pageNumber": { "In": "Query", "Type": "int", "Required": false, "Default": 1 },
  "pageSize": { "In": "Query", "Type": "int", "Required": false, "Default": 10 },
  "roomTypeId": { "In": "Query", "Type": "int?", "Required": false, "Default": null },
  "includeRetired": { "In": "Query", "Type": "bool", "Required": false, "Default": false },
  "sortBy": { "In": "Query", "Type": "string?", "Required": false, "Default": null },
  "sortDescending": { "In": "Query", "Type": "bool", "Required": false, "Default": false }
}
```
- **Roles**: Admin, FrontDesk
- **Request Body**: None
- **Response Body**:
```json
{
  "totalCount": 0,
  "pageNumber": 1,
  "pageSize": 10,
  "data": [
    {
      "id": 0,
      "roomNumber": "string",
      "roomTypeName": "string",
      "basePrice": 0.0,
      "maxOccupancy": 0,
      "isAvailable": true
    }
  ]
}
```

### POST /api/v1/rooms
- **Purpose**: Creates a new room.
- **Parameters**: None
- **Roles**: Admin
- **Request Body**:
```json
{
  "roomNumber": "string (Required, MaxLength 100)",
  "roomTypeId": 0,
  "isActive": true
}
```
- **Response Body**:
```json
{
  "message": "string",
  "roomId": 0
}
```

### PATCH /api/v1/rooms/{id}
- **Purpose**: Updates an existing room by ID.
- **Parameters**:
```json
{
  "id": { "In": "Path", "Type": "int", "Required": true }
}
```
- **Roles**: Admin
- **Request Body**:
```json
{
  "roomNumber": "string (Required, MaxLength 100)",
  "roomTypeId": 0,
  "isActive": true
}
```
- **Response Body**:
```json
{
  "message": "string"
}
```

### DELETE /api/v1/rooms/{id}
- **Purpose**: Permanently retires (soft deletes) a room by ID.
- **Parameters**:
```json
{
  "id": { "In": "Path", "Type": "int", "Required": true }
}
```
- **Roles**: Admin
- **Request Body**: None
- **Response Body**:
```json
{
  "message": "string"
}
```

### GET /api/v1/rooms/status
- **Purpose**: Retrieves a paginated room status dashboard showing availability, occupancy, and upcoming check-ins.
- **Parameters**:
```json
{
  "pageNumber": { "In": "Query", "Type": "int", "Required": false, "Default": 1 },
  "pageSize": { "In": "Query", "Type": "int", "Required": false, "Default": 10 },
  "roomTypeId": { "In": "Query", "Type": "int?", "Required": false, "Default": null },
  "sortBy": { "In": "Query", "Type": "string?", "Required": false, "Default": null },
  "sortDescending": { "In": "Query", "Type": "bool", "Required": false, "Default": false }
}
```
- **Roles**: Admin, FrontDesk
- **Request Body**: None
- **Response Body**:
```json
{
  "totalCount": 0,
  "pageNumber": 1,
  "pageSize": 10,
  "data": [
    {
      "roomId": 0,
      "roomNumber": "string",
      "roomTypeName": "string",
      "status": "string (Available, Occupied, Reserved, Maintenance)",
      "currentGuestName": "string?",
      "nextCheckInDate": "string (ISO 8601 date-time)?"
    }
  ]
}
```

### GET /api/v1/rooms/available-for-booking/{bookingId}
- **Purpose**: Retrieves available rooms that can be assigned to a specific booking for check-in.
- **Parameters**:
```json
{
  "bookingId": { "In": "Path", "Type": "int", "Required": true }
}
```
- **Roles**: Admin, FrontDesk
- **Request Body**: None
- **Response Body**:
```json
[
  {
    "id": 0,
    "roomNumber": "string",
    "roomTypeName": "string",
    "basePrice": 0.0,
    "maxOccupancy": 0,
    "isAvailable": true
  }
]
```


---

## Room Types

### GET /api/v1/room-types
- **Purpose**: Retrieves a paginated list of room types with optional inclusion of retired types and sorting.
- **Parameters**:
```json
{
  "includeRetired": { "In": "Query", "Type": "bool", "Required": false, "Default": false },
  "pageNumber": { "In": "Query", "Type": "int", "Required": false, "Default": 1 },
  "pageSize": { "In": "Query", "Type": "int", "Required": false, "Default": 10 },
  "sortBy": { "In": "Query", "Type": "string?", "Required": false, "Default": null },
  "sortDescending": { "In": "Query", "Type": "bool", "Required": false, "Default": false }
}
```
- **Roles**: Public
- **Request Body**: None
- **Response Body**:
```json
{
  "totalCount": 0,
  "pageNumber": 1,
  "pageSize": 10,
  "data": [
    {
      "id": 0,
      "name": "string",
      "description": "string?",
      "basePrice": 0.0,
      "maxOccupancy": 0,
      "imageUrl": "string?",
      "squareFootage": 0,
      "bedConfiguration": "string?",
      "isActive": true
    }
  ]
}
```

### GET /api/v1/room-types/availability
- **Purpose**: Retrieves available room types for a given date range with inventory counts.
- **Parameters**:
```json
{
  "checkIn": { "In": "Query", "Type": "string", "Format": "dd-MM-yyyy", "Required": true },
  "checkOut": { "In": "Query", "Type": "string", "Format": "dd-MM-yyyy", "Required": true },
  "pageNumber": { "In": "Query", "Type": "int", "Required": false, "Default": 1 },
  "pageSize": { "In": "Query", "Type": "int", "Required": false, "Default": 10 },
  "sortBy": { "In": "Query", "Type": "string?", "Required": false, "Default": "BasePrice" },
  "descending": { "In": "Query", "Type": "bool", "Required": false, "Default": false }
}
```
- **Roles**: Public
- **Request Body**: None
- **Response Body**:
```json
{
  "totalCount": 0,
  "pageNumber": 1,
  "pageSize": 10,
  "data": [
    {
      "roomTypeId": 0,
      "name": "string",
      "basePrice": 0.0,
      "maxOccupancy": 0,
      "description": "string?",
      "imageUrl": "string?",
      "squareFootage": 0,
      "bedConfiguration": "string?",
      "availableCount": 0
    }
  ]
}
```

### POST /api/v1/room-types
- **Purpose**: Creates a new room type.
- **Parameters**: None
- **Roles**: Admin
- **Request Body**:
```json
{
  "name": "string (Required, MaxLength 100)",
  "description": "string? (MaxLength 500)",
  "basePrice": 0.0,
  "maxOccupancy": 0,
  "imageUrl": "string? (MaxLength 500)",
  "squareFootage": 0,
  "bedConfiguration": "string? (MaxLength 100)"
}
```
- **Response Body**:
```json
{
  "id": 0,
  "name": "string",
  "description": "string?",
  "basePrice": 0.0,
  "maxOccupancy": 0,
  "imageUrl": "string?",
  "squareFootage": 0,
  "bedConfiguration": "string?",
  "isActive": true
}
```

### PATCH /api/v1/room-types/{id}
- **Purpose**: Updates an existing room type by ID.
- **Parameters**:
```json
{
  "id": { "In": "Path", "Type": "int", "Required": true }
}
```
- **Roles**: Admin
- **Request Body**:
```json
{
  "name": "string? (MaxLength 100)",
  "description": "string? (MaxLength 500)",
  "basePrice": 0.0,
  "maxOccupancy": 0,
  "imageUrl": "string? (MaxLength 500)",
  "squareFootage": 0,
  "bedConfiguration": "string? (MaxLength 100)",
  "isActive": true
}
```
- **Response Body**:
```json
{
  "id": 0,
  "name": "string",
  "description": "string?",
  "basePrice": 0.0,
  "maxOccupancy": 0,
  "imageUrl": "string?",
  "squareFootage": 0,
  "bedConfiguration": "string?",
  "isActive": true
}
```

### DELETE /api/v1/room-types/{id}
- **Purpose**: Permanently retires (soft deletes) a room type by ID.
- **Parameters**:
```json
{
  "id": { "In": "Path", "Type": "int", "Required": true }
}
```
- **Roles**: Admin
- **Request Body**: None
- **Response Body**:
```json
{
  "message": "string"
}
```


---

## Bookings

### GET /api/v1/bookings
- **Purpose**: Retrieves a paginated list of bookings with optional filtering by status and guest query.
- **Parameters**:
```json
{
  "status": { "In": "Query", "Type": "string?", "Required": false, "Default": null },
  "guestQuery": { "In": "Query", "Type": "string?", "Required": false, "Default": null },
  "pageNumber": { "In": "Query", "Type": "int", "Required": false, "Default": 1 },
  "pageSize": { "In": "Query", "Type": "int", "Required": false, "Default": 10 },
  "sortBy": { "In": "Query", "Type": "string?", "Required": false, "Default": null },
  "sortDescending": { "In": "Query", "Type": "bool", "Required": false, "Default": false }
}
```
- **Roles**: Admin, FrontDesk, RegisteredUser
- **Request Body**: None
- **Response Body**:
```json
{
  "totalCount": 0,
  "pageNumber": 1,
  "pageSize": 10,
  "data": [
    {
      "id": 0,
      "guestCount": 0,
      "rooms": [
        {
          "id": 0,
          "bookingId": 0,
          "roomTypeId": 0,
          "roomId": 0,
          "roomNumber": "string?",
          "lockedInPrice": 0.0
        }
      ],
      "guestName": "string",
      "guestEmail": "string",
      "checkInDate": "string (ISO 8601 date-time)",
      "checkOutDate": "string (ISO 8601 date-time)",
      "bookingStatus": "string (Booked, CheckedIn, CheckedOut, Cancelled)",
      "userId": 0,
      "origin": "string (Guest, RegisteredUser, WalkIn)",
      "bookedAt": "string",
      "amenityIds": [0]
    }
  ]
}
```

### GET /api/v1/bookings/{id}
- **Purpose**: Retrieves a single booking by ID.
- **Parameters**:
```json
{
  "id": { "In": "Path", "Type": "int", "Required": true }
}
```
- **Roles**: Admin, FrontDesk
- **Request Body**: None
- **Response Body**:
```json
{
  "id": 0,
  "guestCount": 0,
  "rooms": [
    {
      "id": 0,
      "bookingId": 0,
      "roomTypeId": 0,
      "roomId": 0,
      "roomNumber": "string?",
      "lockedInPrice": 0.0
    }
  ],
  "guestName": "string",
  "guestEmail": "string",
  "checkInDate": "string (ISO 8601 date-time)",
  "checkOutDate": "string (ISO 8601 date-time)",
  "bookingStatus": "string (Booked, CheckedIn, CheckedOut, Cancelled)",
  "userId": 0,
  "origin": "string (Guest, RegisteredUser, WalkIn)",
  "bookedAt": "string",
  "amenityIds": [0]
}
```

### POST /api/v1/bookings
- **Purpose**: Creates a new booking (room reservation). Supports both anonymous and authenticated users.
- **Parameters**: None
- **Roles**: Public
- **Request Body**:
```json
{
  "roomTypeIds": [0],
  "guestCount": 1,
  "checkInDate": "string (ISO 8601 date-time, must be today or future)",
  "checkOutDate": "string (ISO 8601 date-time, must be after checkInDate)",
  "guestName": "string? (MaxLength 100)",
  "guestEmail": "string? (MaxLength 100, EmailAddress)",
  "amenityIds": [0]
}
```
- **Response Body**:
```json
{
  "id": 0,
  "guestCount": 0,
  "rooms": [
    {
      "id": 0,
      "bookingId": 0,
      "roomTypeId": 0,
      "roomId": 0,
      "roomNumber": "string?",
      "lockedInPrice": 0.0
    }
  ],
  "guestName": "string",
  "guestEmail": "string",
  "checkInDate": "string (ISO 8601 date-time)",
  "checkOutDate": "string (ISO 8601 date-time)",
  "bookingStatus": "string (Booked, CheckedIn, CheckedOut, Cancelled)",
  "userId": 0,
  "origin": "string (Guest, RegisteredUser, WalkIn)",
  "bookedAt": "string",
  "amenityIds": [0]
}
```

### PATCH /api/v1/bookings/{id}/extend-stay
- **Purpose**: Extends a booking's checkout date and/or updates the booking status (excluding CheckedIn/CheckedOut; those require dedicated endpoints).
- **Parameters**:
```json
{
  "id": { "In": "Path", "Type": "int", "Required": true }
}
```
- **Roles**: FrontDesk, Admin
- **Request Body**:
```json
{
  "checkOutDate": "string (ISO 8601 date-time)?",
  "bookingStatus": "string? (Booked, CheckedIn, CheckedOut, Cancelled) — CheckedIn/CheckedOut rejected here"
}
```
- **Response Body**:
```json
{
  "message": "string"
}
```

### POST /api/v1/bookings/{id}/checkin
- **Purpose**: Checks in a guest for the specified booking.
- **Parameters**:
```json
{
  "id": { "In": "Path", "Type": "int", "Required": true }
}
```
- **Roles**: FrontDesk, Admin
- **Request Body**: None
- **Response Body**:
```json
{
  "id": 0,
  "guestCount": 0,
  "rooms": [
    {
      "id": 0,
      "bookingId": 0,
      "roomTypeId": 0,
      "roomId": 0,
      "roomNumber": "string?",
      "lockedInPrice": 0.0
    }
  ],
  "guestName": "string",
  "guestEmail": "string",
  "checkInDate": "string (ISO 8601 date-time)",
  "checkOutDate": "string (ISO 8601 date-time)",
  "bookingStatus": "string (Booked, CheckedIn, CheckedOut, Cancelled)",
  "userId": 0,
  "origin": "string (Guest, RegisteredUser, WalkIn)",
  "bookedAt": "string",
  "amenityIds": [0]
}
```

### POST /api/v1/bookings/{id}/checkout
- **Purpose**: Checks out a guest for the specified booking and returns the final billing folio.
- **Parameters**:
```json
{
  "id": { "In": "Path", "Type": "int", "Required": true }
}
```
- **Roles**: FrontDesk, Admin
- **Request Body**: None
- **Response Body**:
```json
{
  "message": "string",
  "folio": {
    "bookingId": 0,
    "guestName": "string",
    "nightsStayed": 0,
    "roomBasePrice": 0.0,
    "roomTotal": 0.0,
    "foodTotal": 0.0,
    "amenityTotal": 0.0,
    "totalBill": 0.0,
    "paymentStatus": "string",
    "foodItems": ["string"],
    "amenityItems": ["string"]
  }
}
```

### DELETE /api/v1/bookings/{id}/cancel
- **Purpose**: Cancels a booking by ID.
- **Parameters**:
```json
{
  "id": { "In": "Path", "Type": "int", "Required": true }
}
```
- **Roles**: RegisteredUser, FrontDesk, Admin
- **Request Body**: None
- **Response Body**:
```json
{
  "message": "string"
}
```

### POST /api/v1/bookings/{id}/amenities
- **Purpose**: Subscribes an amenity to an existing booking.
- **Parameters**:
```json
{
  "id": { "In": "Path", "Type": "int", "Required": true }
}
```
- **Roles**: FrontDesk, Admin, RegisteredUser
- **Request Body**:
```json
{
  "amenityId": 0
}
```
- **Response Body**:
```json
{
  "message": "string"
}
```


---

## Guests

### GET /api/v1/guests
- **Purpose**: Searches guests or retrieves incoming guests (check-ins today) depending on the `status` query parameter.
- **Parameters**:
```json
{
  "search": { "In": "Query", "Type": "string?", "Required": false, "Default": null },
  "status": { "In": "Query", "Type": "string?", "Required": false, "Default": null },
  "pageNumber": { "In": "Query", "Type": "int", "Required": false, "Default": 1 },
  "pageSize": { "In": "Query", "Type": "int", "Required": false, "Default": 10 }
}
```
- **Roles**: Admin, FrontDesk
- **Request Body**: None
- **Response Body**:
> **Note**: Response shape depends on the `status` parameter.
> - If `status` equals `"incoming"` (case-insensitive): returns paginated list of **BookingDTO** (today's check-ins).
> - Otherwise: returns paginated list of **GuestSearchDTO**.

```json
// status = "incoming"
{
  "totalCount": 0,
  "pageNumber": 1,
  "pageSize": 10,
  "data": [
    {
      "id": 0,
      "guestCount": 0,
      "rooms": [...],
      "guestName": "string",
      "guestEmail": "string",
      "checkInDate": "string (ISO 8601 date-time)",
      "checkOutDate": "string (ISO 8601 date-time)",
      "bookingStatus": "string",
      "userId": 0,
      "origin": "string",
      "bookedAt": "string",
      "amenityIds": [0]
    }
  ]
}

// status != "incoming" (search mode)
{
  "totalCount": 0,
  "pageNumber": 1,
  "pageSize": 10,
  "data": [
    {
      "guestName": "string",
      "guestEmail": "string",
      "totalStays": 0,
      "lastCheckInDate": "string (ISO 8601 date-time)?"
    }
  ]
}
```


---

## Amenities

### GET /api/v1/amenities
- **Purpose**: Retrieves a paginated list of all amenities.
- **Parameters**:
```json
{
  "pageNumber": { "In": "Query", "Type": "int", "Required": false, "Default": 1 },
  "pageSize": { "In": "Query", "Type": "int", "Required": false, "Default": 10 }
}
```
- **Roles**: Public
- **Request Body**: None
- **Response Body**:
```json
{
  "totalCount": 0,
  "pageNumber": 1,
  "pageSize": 10,
  "data": [
    {
      "id": 0,
      "name": "string",
      "description": "string",
      "price": 0.0,
      "isAvailable": true
    }
  ]
}
```

### GET /api/v1/amenities/{id}
- **Purpose**: Retrieves a specific amenity by its ID.
- **Parameters**:
```json
{
  "id": { "In": "Path", "Type": "int", "Required": true }
}
```
- **Roles**: Admin, FrontDesk, RegisteredUser
- **Request Body**: None
- **Response Body**:
```json
{
  "id": 0,
  "name": "string",
  "description": "string",
  "price": 0.0,
  "isAvailable": true
}
```

### POST /api/v1/amenities
- **Purpose**: Creates a new amenity.
- **Parameters**: None
- **Roles**: Admin
- **Request Body**:
```json
{
  "name": "string (Required, MaxLength 100)",
  "description": "string (Required, MaxLength 500)",
  "price": 0.0
}
```
- **Response Body**:
```json
{
  "id": 0,
  "name": "string",
  "description": "string",
  "price": 0.0,
  "isAvailable": true
}
```

### PATCH /api/v1/amenities/{id}
- **Purpose**: Partially updates an existing amenity.
- **Parameters**:
```json
{
  "id": { "In": "Path", "Type": "int", "Required": true }
}
```
- **Roles**: Admin
- **Request Body**:
```json
{
  "name": "string? (MaxLength 100)",
  "description": "string? (MaxLength 500)",
  "price": 0.0,
  "isAvailable": true
}
```
- **Response Body**:
```json
{
  "message": "string"
}
```


---

## Menu Items

### GET /api/v1/menu-items
- **Purpose**: Retrieves a paginated list of menu items with optional filtering by availability and sorting.
- **Parameters**:
```json
{
  "pageNumber": { "In": "Query", "Type": "int", "Required": false, "Default": 1 },
  "pageSize": { "In": "Query", "Type": "int", "Required": false, "Default": 10 },
  "isAvailable": { "In": "Query", "Type": "bool?", "Required": false, "Default": null },
  "sortBy": { "In": "Query", "Type": "string?", "Required": false, "Default": null },
  "sortDescending": { "In": "Query", "Type": "bool", "Required": false, "Default": false }
}
```
- **Roles**: Public
- **Request Body**: None
- **Response Body**:
```json
{
  "totalCount": 0,
  "pageNumber": 1,
  "pageSize": 10,
  "data": [
    {
      "id": 0,
      "name": "string",
      "price": 0.0,
      "category": "string",
      "isAvailable": true
    }
  ]
}
```

### POST /api/v1/menu-items
- **Purpose**: Creates a new menu item.
- **Parameters**: None
- **Roles**: Admin
- **Request Body**:
```json
{
  "name": "string (Required)",
  "price": 0.0,
  "category": "string (Required)",
  "isAvailable": true
}
```
- **Response Body**:
```json
{
  "id": 0,
  "name": "string",
  "price": 0.0,
  "category": "string",
  "isAvailable": true
}
```

### PUT /api/v1/menu-items/{id}
- **Purpose**: Updates an existing menu item.
- **Parameters**:
```json
{
  "id": { "In": "Path", "Type": "int", "Required": true }
}
```
- **Roles**: Admin
- **Request Body**:
```json
{
  "id": 0,
  "name": "string (Required)",
  "price": 0.0,
  "category": "string (Required)",
  "isAvailable": true
}
```
- **Response Body**:
```json
{
  "id": 0,
  "name": "string",
  "price": 0.0,
  "category": "string",
  "isAvailable": true
}
```

### PATCH /api/v1/menu-items/{id}/status
- **Purpose**: Updates the availability status of a specific menu item.
- **Parameters**:
```json
{
  "id": { "In": "Path", "Type": "int", "Required": true },
  "isAvailable": { "In": "Query", "Type": "bool", "Required": true }
}
```
- **Roles**: Admin, Kitchen
- **Request Body**: None
- **Response Body**:
```json
{
  "message": "string"
}
```


---

## Orders (Food Orders)

### GET /api/v1/orders
- **Purpose**: Retrieves a paginated list of food orders, optionally filtered by status and room ID.
- **Parameters**:
```json
{
  "status": { "In": "Query", "Type": "string?", "Required": false, "Default": null },
  "roomId": { "In": "Query", "Type": "int?", "Required": false, "Default": null },
  "pageNumber": { "In": "Query", "Type": "int", "Required": false, "Default": 1 },
  "pageSize": { "In": "Query", "Type": "int", "Required": false, "Default": 10 },
  "sortBy": { "In": "Query", "Type": "string?", "Required": false, "Default": null },
  "sortDescending": { "In": "Query", "Type": "bool", "Required": false, "Default": false }
}
```
- **Roles**: Kitchen, Admin, FrontDesk, RegisteredUser
- **Request Body**: None
- **Response Body**:
```json
{
  "totalCount": 0,
  "pageNumber": 1,
  "pageSize": 10,
  "data": [
    {
      "id": 0,
      "bookingId": 0,
      "generatedAt": "string",
      "finishedAt": "string?",
      "orderStatus": "string (Pending, Preparing, Delivered)",
      "orderItems": [
        {
          "menuItemId": 0,
          "menuItemName": "string",
          "quantity": 0,
          "priceAtPurchase": 0.0
        }
      ]
    }
  ]
}
```

### GET /api/v1/orders/{id}
- **Purpose**: Retrieves a specific food order by its ID.
- **Parameters**:
```json
{
  "id": { "In": "Path", "Type": "int", "Required": true }
}
```
- **Roles**: Kitchen, Admin
- **Request Body**: None
- **Response Body**:
```json
{
  "id": 0,
  "bookingId": 0,
  "generatedAt": "string",
  "finishedAt": "string?",
  "orderStatus": "string (Pending, Preparing, Delivered)",
  "orderItems": [
    {
      "menuItemId": 0,
      "menuItemName": "string",
      "quantity": 0,
      "priceAtPurchase": 0.0
    }
  ]
}
```

### POST /api/v1/orders
- **Purpose**: Creates a new food order for a booking.
- **Parameters**: None
- **Roles**: FrontDesk, Admin, RegisteredUser
- **Request Body**:
```json
{
  "bookingId": 0,
  "items": [
    {
      "menuItemId": 0,
      "quantity": 0
    }
  ]
}
```
- **Response Body**:
```json
{
  "message": "string",
  "orderId": 0
}
```

### PATCH /api/v1/orders/{id}
- **Purpose**: Updates the status of an existing food order.
- **Parameters**:
```json
{
  "id": { "In": "Path", "Type": "int", "Required": true }
}
```
- **Roles**: Kitchen, Admin
- **Request Body**:
```json
{
  "status": "string (Pending, Preparing, Delivered)"
}
```
- **Response Body**:
```json
{
  "message": "string"
}
```


---

## Billing

### GET /api/v1/billing
- **Purpose**: Retrieves a paginated global billing list with optional filtering by payment status, date range, search term, and detail level.
- **Parameters**:
```json
{
  "paymentStatus": { "In": "Query", "Type": "string?", "Required": false, "Default": null },
  "startDate": { "In": "Query", "Type": "string?", "Format": "dd-MM-yyyy", "Required": false, "Default": null },
  "endDate": { "In": "Query", "Type": "string?", "Format": "dd-MM-yyyy", "Required": false, "Default": null },
  "search": { "In": "Query", "Type": "string?", "Required": false, "Default": null },
  "detailed": { "In": "Query", "Type": "bool", "Required": false, "Default": false },
  "pageNumber": { "In": "Query", "Type": "int", "Required": false, "Default": 1 },
  "pageSize": { "In": "Query", "Type": "int", "Required": false, "Default": 10 },
  "sortBy": { "In": "Query", "Type": "string?", "Required": false, "Default": null },
  "sortDescending": { "In": "Query", "Type": "bool", "Required": false, "Default": false }
}
```
- **Roles**: Admin, FrontDesk
- **Request Body**: None
- **Response Body**:
```json
{
  "totalCount": 0,
  "pageNumber": 1,
  "pageSize": 10,
  "data": [
    {
      "bookingId": 0,
      "guestName": "string",
      "checkInDate": "string (ISO 8601 date-time)",
      "checkOutDate": "string (ISO 8601 date-time)",
      "bookingStatus": "string (Booked, CheckedIn, CheckedOut, Cancelled)",
      "paymentStatus": "string",
      "baseRoomTotal": 0.0
    }
  ]
}
```

### GET /api/v1/billing/receipts
- **Purpose**: Retrieves a paginated list of payment receipts with optional date range filtering.
- **Parameters**:
```json
{
  "startDate": { "In": "Query", "Type": "string?", "Format": "dd-MM-yyyy", "Required": false, "Default": null },
  "endDate": { "In": "Query", "Type": "string?", "Format": "dd-MM-yyyy", "Required": false, "Default": null },
  "pageNumber": { "In": "Query", "Type": "int", "Required": false, "Default": 1 },
  "pageSize": { "In": "Query", "Type": "int", "Required": false, "Default": 10 },
  "sortBy": { "In": "Query", "Type": "string?", "Required": false, "Default": null },
  "sortDescending": { "In": "Query", "Type": "bool", "Required": false, "Default": false }
}
```
- **Roles**: Admin
- **Request Body**: None
- **Response Body**:
```json
{
  "totalCount": 0,
  "pageNumber": 1,
  "pageSize": 10,
  "data": [
    {
      "id": 0,
      "bookingId": 0,
      "amountPaid": 0.0,
      "paymentMethod": "string",
      "transactionId": "string",
      "paidAt": "string"
    }
  ]
}
```

### GET /api/v1/billing/receipts/{id}
- **Purpose**: Retrieves a specific payment receipt by its ID.
- **Parameters**:
```json
{
  "id": { "In": "Path", "Type": "int", "Required": true }
}
```
- **Roles**: Admin
- **Request Body**: None
- **Response Body**:
```json
{
  "id": 0,
  "bookingId": 0,
  "amountPaid": 0.0,
  "paymentMethod": "string",
  "transactionId": "string",
  "paidAt": "string"
}
```

### GET /api/v1/billing/{bookingId}
- **Purpose**: Generates and retrieves the billing folio for a specific booking.
- **Parameters**:
```json
{
  "bookingId": { "In": "Path", "Type": "int", "Required": true }
}
```
- **Roles**: FrontDesk, Admin
- **Request Body**: None
- **Response Body**:
```json
{
  "bookingId": 0,
  "guestName": "string",
  "nightsStayed": 0,
  "roomBasePrice": 0.0,
  "roomTotal": 0.0,
  "foodTotal": 0.0,
  "amenityTotal": 0.0,
  "totalBill": 0.0,
  "paymentStatus": "string",
  "foodItems": ["string"],
  "amenityItems": ["string"]
}
```

### POST /api/v1/billing/{bookingId}/pay
- **Purpose**: Processes a payment for a specific booking.
- **Parameters**:
```json
{
  "bookingId": { "In": "Path", "Type": "int", "Required": true }
}
```
- **Roles**: FrontDesk, Admin
- **Request Body**:
```json
{
  "amount": 0.0,
  "paymentMethod": "string (Required, MaxLength 100)",
  "transactionId": "string (Required, MaxLength 100)"
}
```
- **Response Body**:
```json
{
  "message": "string",
  "bookingId": 0
}
```


---

## Staff

### GET /api/v1/staff
- **Purpose**: Retrieves a paginated list of staff members with optional search query, inclusion of fired staff, and sorting.
- **Parameters**:
```json
{
  "staffQuery": { "In": "Query", "Type": "string?", "Required": false, "Default": null },
  "pageNumber": { "In": "Query", "Type": "int", "Required": false, "Default": 1 },
  "pageSize": { "In": "Query", "Type": "int", "Required": false, "Default": 10 },
  "includeFired": { "In": "Query", "Type": "bool", "Required": false, "Default": false },
  "sortBy": { "In": "Query", "Type": "string?", "Required": false, "Default": null },
  "sortDescending": { "In": "Query", "Type": "bool", "Required": false, "Default": false }
}
```
- **Roles**: Admin
- **Request Body**: None
- **Response Body**:
```json
{
  "totalCount": 0,
  "pageNumber": 1,
  "pageSize": 10,
  "data": [
    {
      "id": 0,
      "email": "string",
      "firstName": "string",
      "lastName": "string",
      "role": "string",
      "isActive": true,
      "createdAt": "string (ISO 8601 date-time)"
    }
  ]
}
```

### POST /api/v1/staff
- **Purpose**: Creates a new staff account with a specified role.
- **Parameters**: None
- **Roles**: Admin
- **Request Body**:
```json
{
  "email": "string (Required, EmailAddress, MaxLength 100)",
  "password": "string (Required, MinLength 6, MaxLength 100)",
  "firstName": "string (Required, MaxLength 100)",
  "lastName": "string (Required, MaxLength 100)",
  "role": "string (Required, MaxLength 100)"
}
```
- **Response Body**:
```json
{
  "message": "string"
}
```

### PATCH /api/v1/staff/{id}
- **Purpose**: Updates an existing staff member's details.
- **Parameters**:
```json
{
  "id": { "In": "Path", "Type": "int", "Required": true }
}
```
- **Roles**: Admin
- **Request Body**:
```json
{
  "firstName": "string (Required, MaxLength 100)",
  "lastName": "string (Required, MaxLength 100)",
  "role": "string (Required, MaxLength 100)",
  "isActive": true
}
```
- **Response Body**:
```json
{
  "message": "string"
}
```

### DELETE /api/v1/staff/{id}
- **Purpose**: Soft-deletes (deactivates/fires) a staff member.
- **Parameters**:
```json
{
  "id": { "In": "Path", "Type": "int", "Required": true }
}
```
- **Roles**: Admin
- **Request Body**: None
- **Response Body**:
```json
{
  "message": "string"
}
```


---

## Maintenance

### GET /api/v1/maintenance
- **Purpose**: Retrieves all maintenance tasks with pagination, optional status filter, and sorting.
- **Parameters**:
```json
{
  "pageNumber": { "In": "Query", "Type": "int", "Required": false, "Default": 1 },
  "pageSize": { "In": "Query", "Type": "int", "Required": false, "Default": 10 },
  "status": { "In": "Query", "Type": "string?", "Required": false, "Default": null },
  "sortBy": { "In": "Query", "Type": "string?", "Required": false, "Default": null },
  "sortDescending": { "In": "Query", "Type": "bool", "Required": false, "Default": false }
}
```
- **Roles**: Admin, FrontDesk, Housekeeping, Maintenance, RegisteredUser
- **Request Body**: None
- **Response Body**:
```json
{
  "totalCount": 0,
  "pageNumber": 1,
  "pageSize": 10,
  "data": [
    {
      "id": 0,
      "roomId": 0,
      "location": "string?",
      "originType": "string",
      "status": "string (Pending, InProgress, Completed)",
      "description": "string",
      "createdAt": "string",
      "startedAt": "string?",
      "finishedAt": "string?"
    }
  ]
}
```

### GET /api/v1/maintenance/active
- **Purpose**: Retrieves active (pending) maintenance tasks with pagination and sorting.
- **Parameters**:
```json
{
  "pageNumber": { "In": "Query", "Type": "int", "Required": false, "Default": 1 },
  "pageSize": { "In": "Query", "Type": "int", "Required": false, "Default": 10 },
  "sortBy": { "In": "Query", "Type": "string?", "Required": false, "Default": null },
  "sortDescending": { "In": "Query", "Type": "bool", "Required": false, "Default": false }
}
```
- **Roles**: Admin, Maintenance
- **Request Body**: None
- **Response Body**:
```json
{
  "totalCount": 0,
  "pageNumber": 1,
  "pageSize": 10,
  "data": [
    {
      "id": 0,
      "roomId": 0,
      "location": "string?",
      "originType": "string",
      "status": "string (Pending, InProgress, Completed)",
      "description": "string",
      "createdAt": "string",
      "startedAt": "string?",
      "finishedAt": "string?"
    }
  ]
}
```

### POST /api/v1/maintenance/trigger/{roomId}
- **Purpose**: Creates a maintenance ticket triggered by a guest or staff for a specific room.
- **Parameters**:
```json
{
  "roomId": { "In": "Path", "Type": "int", "Required": true }
}
```
- **Roles**: Admin, FrontDesk, Housekeeping, RegisteredUser
- **Request Body**:
```json
{
  "description": "string (Required, MaxLength 500)"
}
```
- **Response Body**:
```json
{
  "id": 0,
  "roomId": 0,
  "location": "string?",
  "originType": "string",
  "status": "string (Pending, InProgress, Completed)",
  "description": "string",
  "createdAt": "string",
  "startedAt": "string?",
  "finishedAt": "string?"
}
```

### POST /api/v1/maintenance/internal
- **Purpose**: Creates an internal maintenance ticket (e.g., for common areas).
- **Parameters**: None
- **Roles**: Admin, FrontDesk, Maintenance
- **Request Body**:
```json
{
  "location": "string (Required, MaxLength 200)",
  "description": "string (Required, MaxLength 500)"
}
```
- **Response Body**:
```json
{
  "id": 0,
  "roomId": 0,
  "location": "string?",
  "originType": "string",
  "status": "string (Pending, InProgress, Completed)",
  "description": "string",
  "createdAt": "string",
  "startedAt": "string?",
  "finishedAt": "string?"
}
```

### PATCH /api/v1/maintenance/{id}/status
- **Purpose**: Updates the status of a maintenance task.
- **Parameters**:
```json
{
  "id": { "In": "Path", "Type": "int", "Required": true }
}
```
- **Roles**: Admin, Maintenance
- **Request Body**:
```json
{
  "status": "string (Required) — Pending, InProgress, Completed"
}
```
- **Response Body**:
```json
{
  "id": 0,
  "roomId": 0,
  "location": "string?",
  "originType": "string",
  "status": "string (Pending, InProgress, Completed)",
  "description": "string",
  "createdAt": "string",
  "startedAt": "string?",
  "finishedAt": "string?"
}
```


---

## Housekeeping

### GET /api/v1/housekeeping
- **Purpose**: Retrieves all housekeeping tasks with pagination, optional status filter, and sorting.
- **Parameters**:
```json
{
  "pageNumber": { "In": "Query", "Type": "int", "Required": false, "Default": 1 },
  "pageSize": { "In": "Query", "Type": "int", "Required": false, "Default": 10 },
  "status": { "In": "Query", "Type": "string?", "Required": false, "Default": null },
  "sortBy": { "In": "Query", "Type": "string?", "Required": false, "Default": null },
  "sortDescending": { "In": "Query", "Type": "bool", "Required": false, "Default": false }
}
```
- **Roles**: Admin, FrontDesk, Housekeeping, RegisteredUser
- **Request Body**: None
- **Response Body**:
```json
{
  "totalCount": 0,
  "pageNumber": 1,
  "pageSize": 10,
  "data": [
    {
      "id": 0,
      "roomId": 0,
      "location": "string?",
      "description": "string?",
      "originType": "string (Guest, Staff)",
      "status": "string (Pending, InProgress, Completed)",
      "startedAt": "string?",
      "finishedAt": "string?",
      "createdAt": "string"
    }
  ]
}
```

### GET /api/v1/housekeeping/active
- **Purpose**: Retrieves active (pending) housekeeping tasks with pagination and sorting.
- **Parameters**:
```json
{
  "pageNumber": { "In": "Query", "Type": "int", "Required": false, "Default": 1 },
  "pageSize": { "In": "Query", "Type": "int", "Required": false, "Default": 10 },
  "sortBy": { "In": "Query", "Type": "string?", "Required": false, "Default": null },
  "sortDescending": { "In": "Query", "Type": "bool", "Required": false, "Default": false }
}
```
- **Roles**: Admin, FrontDesk, Housekeeping
- **Request Body**: None
- **Response Body**:
```json
{
  "totalCount": 0,
  "pageNumber": 1,
  "pageSize": 10,
  "data": [
    {
      "id": 0,
      "roomId": 0,
      "location": "string?",
      "description": "string?",
      "originType": "string (Guest, Staff)",
      "status": "string (Pending, InProgress, Completed)",
      "startedAt": "string?",
      "finishedAt": "string?",
      "createdAt": "string"
    }
  ]
}
```

### POST /api/v1/housekeeping/trigger/{roomId}
- **Purpose**: Creates a guest-triggered housekeeping request for a specific room.
- **Parameters**:
```json
{
  "roomId": { "In": "Path", "Type": "int", "Required": true }
}
```
- **Roles**: Admin, FrontDesk, RegisteredUser
- **Request Body**:
```json
{
  "description": "string (Required, MaxLength 500)"
}
```
- **Response Body**:
```json
{
  "message": "string"
}
```

### POST /api/v1/housekeeping/internal
- **Purpose**: Creates an internal housekeeping task (e.g., for common areas).
- **Parameters**: None
- **Roles**: Admin, FrontDesk
- **Request Body**:
```json
{
  "location": "string (Required, MaxLength 200)",
  "description": "string (Required, MaxLength 500)"
}
```
- **Response Body**:
```json
{
  "message": "string"
}
```

### PATCH /api/v1/housekeeping/{id}/status
- **Purpose**: Updates the status of a housekeeping task.
- **Parameters**:
```json
{
  "id": { "In": "Path", "Type": "int", "Required": true }
}
```
- **Roles**: Admin, Housekeeping
- **Request Body**:
```json
{
  "status": "string (Pending, InProgress, Completed)"
}
```
- **Response Body**:
```json
{
  "message": "string"
}
```


---

## Feedback

### GET /api/v1/feedback
- **Purpose**: Retrieves all feedback entries with pagination, option to include hidden entries, and sorting.
- **Parameters**:
```json
{
  "pageNumber": { "In": "Query", "Type": "int", "Required": false, "Default": 1 },
  "pageSize": { "In": "Query", "Type": "int", "Required": false, "Default": 10 },
  "includeHidden": { "In": "Query", "Type": "bool", "Required": false, "Default": false },
  "sortBy": { "In": "Query", "Type": "string?", "Required": false, "Default": null },
  "sortDescending": { "In": "Query", "Type": "bool", "Required": false, "Default": false }
}
```
- **Roles**: Admin, FrontDesk
- **Request Body**: None
- **Response Body**:
```json
{
  "totalCount": 0,
  "pageNumber": 1,
  "pageSize": 10,
  "data": [
    {
      "id": 0,
      "bookingId": 0,
      "rating": 0,
      "comments": "string",
      "createdAt": "string",
      "isHidden": true
    }
  ]
}
```

### GET /api/v1/feedback/booking/{bookingId}
- **Purpose**: Retrieves the feedback associated with a specific booking.
- **Parameters**:
```json
{
  "bookingId": { "In": "Path", "Type": "int", "Required": true }
}
```
- **Roles**: Admin, FrontDesk
- **Request Body**: None
- **Response Body**:
```json
{
  "id": 0,
  "bookingId": 0,
  "rating": 0,
  "comments": "string",
  "createdAt": "string",
  "isHidden": true
}
```

### POST /api/v1/feedback
- **Purpose**: Submits new feedback for a booking.
- **Parameters**: None
- **Roles**: Admin, FrontDesk, RegisteredUser
- **Request Body**:
```json
{
  "bookingId": 0,
  "rating": 0,
  "comments": "string (MaxLength 500)"
}
```
- **Response Body**:
```json
{
  "id": 0,
  "bookingId": 0,
  "rating": 0,
  "comments": "string",
  "createdAt": "string",
  "isHidden": true
}
```

### PATCH /api/v1/feedback/{id}/moderate
- **Purpose**: Moderates a feedback entry by hiding or unhiding it.
- **Parameters**:
```json
{
  "id": { "In": "Path", "Type": "int", "Required": true }
}
```
- **Roles**: Admin
- **Request Body**:
```json
{
  "isHidden": true
}
```
- **Response Body**:
```json
{
  "id": 0,
  "bookingId": 0,
  "rating": 0,
  "comments": "string",
  "createdAt": "string",
  "isHidden": true
}
```
