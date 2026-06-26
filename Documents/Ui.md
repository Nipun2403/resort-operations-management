The Website is Divided into 3 Segments : Public Website / landing Page, Customer Portal, and Operations Portal

Structure for all 3 :

PUBLIC WEBSITE
├── Home[/] (Landing)
├── Room Type [/room-type]
│   └── Individual Room Type Page [/room-type/{id/name}]
├── Dining [/Dining]
├── Amenities [/amenities]
├── Room Availability Search [/availability]
├── Booking Checkout [/checkout]
├── Login/Register [/auth]

GUEST PORTAL
├── Dashboard [/user]
├── My Bookings [/user/bookings]
│   └── Individual bookings [/user/bookings/{id/name}]
├── Room Service & Tickets [/user/room-service]
├── Feedback [/user/feedback]
├── Profile [/user/profile]

STAFF PORTAL [/operations]
├── Admin [/operations/admin]
│   ├── Dashboard [/operations/admin/dashboard]
|   └── Management (CRUD) [/operations/admin/management]
|   |   ├── Rooms [/operations/admin/management/room]
|   |   ├── Room Types [/operations/admin/management/room-type]
|   |   ├── Staff [/operations/admin/management/staff]
|   |   ├── Amenities [/operations/admin/management/amenities]
|   |   ├── Menu [/operations/admin/management/menu]
|   |
│   └── Oversight (Data) [/operations/admin/oversight]
|   |   ├── Analytics [/operations/admin/oversight/analytics]
|   |   ├── Audit Logs [/operations/admin/oversight/auditlogs]
|   |   ├── Billings & Receipts [/operations/admin/billings-receipts]
|   |   ├── Feedback [/operations/admin/oversight/feedback]
|   |
│   ├── Profile [/operations/admin/profile]
|
├── Front Desk [/operations/front-desk]
│   ├── Dashboard [/operations/front-desk/dashboard]
│   ├── Bookings [/operations/front-desk/bookings]
│   ├── Billing & Receipt [/operations/front-desk/billings-receipts]
│   └── Room Service [/operations/front-desk/room-service]
│
├── Housekeeping [/operations/housekeeping]
│   └── Dashboard [/operations/housekeeping/dashboard]
│
├── Maintenance [/operations/maintenance]
│   └── Dashboard [/operations/maintenance/dashboard]
│
└── Kitchen [/operations/kitchen]
    ├── Dashboard [/operations/kitchen/dashboard]
    └── Menu Management [/operations/kitchen/menu]


