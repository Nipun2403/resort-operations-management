# Aetheris Retreat — MainDatabaseSeeder.cs Regeneration Prompt

Use this prompt to regenerate `Backend/HotelManagement.API/Utilities/MainDatabaseSeeder.cs` from scratch. Preserve the old seeder as `ignore.MainDatabaseSeeder.cs` before overwriting.

## Constraints

1. **Simplified Emails**:
   - Staff: `{role}@aetheris.com` (admin, fd1, fd2, fd3, kitchen, kitchen2, hk1, hk2, maintenance, inactive)
   - Registered customers: `cust{1-5}@gmail.com`
   - Edge-case guests: `prospective@gmail.com`, `banned@gmail.com`, `pending@gmail.com`
   - Walk-ins/guest bookings: descriptive real-name emails (e.g., `lena.bergstrom@gmail.com`)

2. **Universal Password**: `Pass@1234` (BCrypt hashed)

3. **Seed Data Source**: `Agent/aetheris-seed-data.md` — contains:
   - Room type images (Unsplash URLs)
   - Amenity descriptions and image URLs
   - Menu item data (names, prices, categories, availability flags)

4. **Amenity Availability Overrides** (from seed-data.md):
   - Floatation Sensory Deprivation Therapy — IsAvailable = false
   - Executive Protection Detail — IsAvailable = false

5. **Old Seeder Backup**: Save previous version as `ignore.MainDatabaseSeeder.cs` in same directory.

## Users (18 total)

| Variable | Email | Name | Role | Active |
|----------|-------|------|------|:------:|
| admin | admin@aetheris.com | Elara Voss | Admin | true |
| fd1 | fd1@aetheris.com | Margaux Lefevre | FrontDesk | true |
| fd2 | fd2@aetheris.com | Caspian Reed | FrontDesk | true |
| fd3 | fd3@aetheris.com | Silke Berg | FrontDesk | true |
| kitchen | kitchen@aetheris.com | Riku Nakamura | Kitchen | true |
| kitchenAsst | kitchen2@aetheris.com | Petra Wolff | Kitchen | true |
| hk1 | hk1@aetheris.com | Daria Morel | Housekeeping | true |
| hk2 | hk2@aetheris.com | Ivan Kuznetsov | Housekeeping | true |
| maintenance | maintenance@aetheris.com | Felix Schreiber | Maintenance | true |
| inactiveStaff | inactive@aetheris.com | Olivier Renaud | FrontDesk | false |
| cust1 | cust1@gmail.com | Isabelle Fontaine | RegisteredUser | true |
| cust2 | cust2@gmail.com | Haruto Katsuragi | RegisteredUser | true |
| cust3 | cust3@gmail.com | Aleksei Volkov | RegisteredUser | true |
| cust4 | cust4@gmail.com | Nadine El-Amin | RegisteredUser | true |
| cust5 | cust5@gmail.com | Constance Morrow | RegisteredUser | true |
| guestNoBookings | prospective@gmail.com | Theo Sander | RegisteredUser | true |
| guestBanned | banned@gmail.com | Banned Account | RegisteredUser | false |
| guestPending | pending@gmail.com | Awaiting Verification | RegisteredUser | true |

## Room Types (8)

Use descriptions from `Agent/aetheris-seed-data.md` and Unsplash image URLs from same file.

| Variable | Name | BasePrice | MaxOcc | Bed Config | SqFt |
|----------|------|----------:|:------:|:----------:|:----:|
| hollow | The Hollow | 3500 | 1 | King:1 | 320 |
| obsidian | The Obsidian Chamber | 4200 | 2 | King:1 | 480 |
| ember | The Ember Suite | 5800 | 2 | King:1, Daybed:1 | 620 |
| vantage | The Vantage Loft | 7200 | 2 | King:1 | 750 |
| stillwater | The Stillwater Villa | 9500 | 4 | King:2 | 1400 |
| ashwood | The Ashwood Residence | 14000 | 6 | King:3 | 2600 |
| monolith | The Monolith Penthouse | 22000 | 4 | King:2, Queen:1 | 3200 |
| sanctum | The Sanctum | 30000 | 8 | King:4 | 6500 |

## Rooms (24 total, by code prefix)

- H=Hollow (H01-H03), O=Obsidian (O01-O03), E=Ember (E01-E03)
- V=Vantage (V01-V03), SW=Stillwater (SW01-SW03), AW=Ashwood (AW01-AW03)
- MP=Monolith (MP01-MP03), SX=Sanctum (SX01-SX03)
- Inactive rooms: O03, SW03

## Menu Items (13 items)

From `Agent/aetheris-seed-data.md` — exact names, prices, categories, descriptions, image URLs, and IsAvailable flags. Squab en Croûte (`IsAvailable = false`) is the only unavailable menu item.

## Amenities (14 items)

From `Agent/aetheris-seed-data.md` — exact names, descriptions, images, and:
- Floatation Sensory Deprivation: IsAvailable = false
- Executive Protection Detail: IsAvailable = false
- All others: IsAvailable = true

## Booking Scenarios (20 bookings total)

Booking array indices matter — food orders, amenities, housekeeping, maintenance, feedback, and receipts reference them by index.

Index mapping:
- [0] cust1 — The Hollow (H01) — CheckedIn, Pending — arrived -2d, leaving +3d
- [1] cust2 — The Obsidian Chamber (O01) — CheckedIn, Pending — arrived -1d, leaving +4d
- [2] cust3 — The Ember Suite (E01, no room) — Booked, Paid — future +8d
- [3] cust4 — The Vantage Loft (V01, no room) — Booked, Pending — future +15d
- [4] cust5 — The Ember Suite (E02) — CheckedOut, Paid — was -12d to -7d
- [5] Walk-in (Emile Renard, emile.renard@gmail.com) — The Hollow (H02) — CheckedOut, Paid — was -22d to -18d
- [6] Guest (Saoirse Brennan, saoirse.brennan@gmail.com) — Stillwater Villa (no room) — Cancelled, Pending
- [7] Guest (Viktor Strauss, viktor.strauss@gmail.com) — Obsidian Chamber (no room) — Cancelled, Pending (no-show, -5d to -2d)
- [8] cust1 second stay — The Vantage Loft (V02) — CheckedIn, Paid — arrived -3d, leaving +1d
- [9] cust2 extended — Ashwood Residence (AW01) — CheckedIn, Paid — arrived -4d, leaving +2d
- [10] Guest (Lena Bergstrom, lena.bergstrom@gmail.com) — The Hollow (H03) — CheckedIn, Pending — arrives today, leaving +4d
- [11] Walk-in (Marcus de Vries, marcus.devries@gmail.com) — Obsidian Chamber (O02) — CheckedIn, Pending — arrived -4d, departing today
- [12] Guest (Priya Subramaniam, priya.subramaniam@gmail.com) — Ember Suite (E03) — CheckedIn, Pending — arrived -15d, leaving +15d (30d long stay)
- [13] Walk-in (Conrad Hale, conrad.hale@gmail.com) — The Hollow (H01) — CheckedOut, Pending (UNPAID — runner) — was -35d to -33d
- [14] Guest (Antoinette Bellerose, antoinette.bellerose@gmail.com) — Monolith (MP01, no room) — Cancelled, Paid (needs refund)
- [15] Guest (Dmitri Orloff, dmitri.orloff@gmail.com) — Monolith (MP02, no room) — Cancelled, Refunded
- [16] cust3 VVIP — Sanctum (SX01+SX02, both rooms) — CheckedIn, Paid — arrived -1d, leaving +5d, multi-room
- [17] cust4 family — Stillwater Villa (SW01+SW02, both rooms) — CheckedIn, Pending — arrived -2d, leaving +2d, multi-room
- [18] cust5 far-future — Monolith (MP03, no room) — Booked, Paid — in 90d
- [19] Guest (Florian Czekaj, florian.czekaj@gmail.com) — The Hollow (no room) — Booked, Pending — in 20d

## Food Orders (11 orders)

- [0] Booking[0], Delivered — Caviar(1) + Wagyu(1) + Champagne(1)
- [1] Booking[0], Preparing — Matcha(1) + Chocolate Sphere(1)
- [2] Booking[1], Pending — Consommé(2) + Turbot(1)
- [3] Booking[1], Delivered — Bone Marrow(2) + Malt(2)
- [4] Booking[12], Delivered — Stillness Menu(2)
- [5] Booking[10], Pending — Caviar(1) + Matcha(1)
- [6] Booking[11], Preparing — Sabayon(1) + Matcha(1)
- [7] Booking[11], Delivered — Scallop(2) + Champagne(1)
- [8] Booking[16], Delivered — Stillness Menu(6) + Champagne(6) + Malt(2)
- [9] Booking[16], Pending — Caviar(6) + Matcha(6)
- [10] Booking[17], Delivered — Wagyu(2) + Turbot(2) + Chocolate(4) + Champagne(2)

## Booking Amenities

- Booking[0]: Spa + Sound Bath + Digital Detox
- Booking[1]: Butler + Wine Cellar + Art Curator
- Booking[8]: Floatation + Stargazing
- Booking[9]: Butler + Private Chef + Yacht
- Booking[16]: Helicopter + Exec Protection + Private Chef + Butler + Yacht + Cinema + Falconry
- Booking[17]: Spa + Stargazing + Cinema
- Booking[12]: Digital Detox + Floatation + Sound Bath
- Booking[4]: Spa + Wine Cellar

## Housekeeping Tasks (14 tasks)

Include variety: Completed/InProgress/Pending, various origin types (CheckoutAutomated, GuestRequested, StaffRequested, SystemAutomated), rooms 0-22, and one null-room task (Observatory telescope cleaning).

## Maintenance Tasks (11 tasks)

Include variety: InProgress/Completed/Pending, origin types (StaffRequested, SystemAutomated, GuestRequested), rooms 0-23, and one null-room task (Private dock mooring cleats).

## Feedback (11 entries)

Feedback for bookings: [4], [5], [13], [0], [1], [8], [9], [12], [16], [17], [7]
- [13] is hidden (negative feedback from unpaid runner Conrad Hale)
- [12] has empty comment (Priya, long stay at Ember)
- Multiple 5-star reviews for various stays

## Receipts (13 receipts)

Receipts for bookings: [4], [5], [2], [8], [9], [0], [1], [14], [15], [16], [18], [12], [17]
- [15] is a refund (negative AmountPaid, TransactionId AE-REF-0001)
- Payment methods: Private Bank Transfer, Amex Centurion, Wire Transfer
- Transaction IDs: AE-TXN-0001 through AE-TXN-0011, AE-TXN-VIP-001, AE-REF-0001

## Code Structure

```csharp
public static class MainDatabaseSeeder
{
  public static void Seed(IServiceProvider serviceProvider)
  {
    using var scope = serviceProvider.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (context.Users.Any()) return;

    var now = DateTime.UtcNow;

    // 1. USERS (18 users, BCrypt hash for "Pass@1234")
    // 2. ROOM TYPES (8 room types with Unsplash images)
    // 3. ROOMS (24 rooms, 2 inactive)
    // 4. MENU ITEMS (13 items from seed-data.md)
    // 5. AMENITIES (14 items from seed-data.md)
    // 6. BOOKINGS (20 bookings, indexed [0]-[19])
    // 7. FOOD ORDERS (11 orders)
    // 8. BOOKING AMENITIES
    // 9. HOUSEKEEPING TASKS (14 tasks)
    // 10. MAINTENANCE TASKS (11 tasks)
    // 11. FEEDBACK (11 entries)
    // 12. RECEIPTS (13 receipts)

    context.SaveChanges();
  }
}
```

## References

- Seeder file: `Backend/HotelManagement.API/Utilities/MainDatabaseSeeder.cs`
- Seed data: `Agent/aetheris-seed-data.md`
- Entity models: `Backend/HotelManagement.DAL/Entities/`
- Login credentials: `Agent/aetheris-seed-login-credentials.md`
- Old seeder backup: `Backend/HotelManagement.API/Utilities/ignore.MainDatabaseSeeder.cs`
