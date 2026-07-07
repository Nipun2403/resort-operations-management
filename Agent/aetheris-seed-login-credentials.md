# Aetheris Retreat — Seed Data Login Credentials & Booking Reference

**Universal Password:** `Pass@1234`

---

## STAFF ACCOUNTS

| Email | Password | Name | Role | Active |
|-------|----------|------|------|:------:|
| admin@aetheris.com | Pass@1234 | Elara Voss | Admin | ✓ |
| fd1@aetheris.com | Pass@1234 | Margaux Lefevre | FrontDesk | ✓ |
| fd2@aetheris.com | Pass@1234 | Caspian Reed | FrontDesk | ✓ |
| fd3@aetheris.com | Pass@1234 | Silke Berg | FrontDesk | ✓ |
| kitchen@aetheris.com | Pass@1234 | Riku Nakamura | Kitchen | ✓ |
| kitchen2@aetheris.com | Pass@1234 | Petra Wolff | Kitchen | ✓ |
| hk1@aetheris.com | Pass@1234 | Daria Morel | Housekeeping | ✓ |
| hk2@aetheris.com | Pass@1234 | Ivan Kuznetsov | Housekeeping | ✓ |
| maintenance@aetheris.com | Pass@1234 | Felix Schreiber | Maintenance | ✓ |
| inactive@aetheris.com | Pass@1234 | Olivier Renaud | FrontDesk | ✗ (Inactive) |

## REGISTERED CUSTOMERS

| Email | Password | Name | Role | Active |
|-------|----------|------|------|:------:|
| cust1@gmail.com | Pass@1234 | Isabelle Fontaine | RegisteredUser | ✓ |
| cust2@gmail.com | Pass@1234 | Haruto Katsuragi | RegisteredUser | ✓ |
| cust3@gmail.com | Pass@1234 | Aleksei Volkov | RegisteredUser | ✓ |
| cust4@gmail.com | Pass@1234 | Nadine El-Amin | RegisteredUser | ✓ |
| cust5@gmail.com | Pass@1234 | Constance Morrow | RegisteredUser | ✓ |

## EDGE-CASE GUEST ACCOUNTS

| Email | Password | Name | Role | Active |
|-------|----------|------|------|:------:|
| prospective@gmail.com | Pass@1234 | Theo Sander | RegisteredUser | ✓ (no bookings) |
| banned@gmail.com | Pass@1234 | Banned Account | RegisteredUser | ✗ (Banned/Inactive) |
| pending@gmail.com | Pass@1234 | Awaiting Verification | RegisteredUser | ✓ (Pending status) |

## WALK-IN / GUEST BOOKINGS (no login)

| Email | Name | Booking Origin |
|-------|------|:--------------:|
| emile.renard@gmail.com | Emile Renard | WalkIn |
| saoirse.brennan@gmail.com | Saoirse Brennan | Guest |
| viktor.strauss@gmail.com | Viktor Strauss | Guest |
| lena.bergstrom@gmail.com | Lena Bergstrom | Guest |
| marcus.devries@gmail.com | Marcus de Vries | WalkIn |
| priya.subramaniam@gmail.com | Priya Subramaniam | Guest |
| conrad.hale@gmail.com | Conrad Hale | WalkIn |
| antoinette.bellerose@gmail.com | Antoinette Bellerose | Guest |
| dmitri.orloff@gmail.com | Dmitri Orloff | Guest |
| florian.czekaj@gmail.com | Florian Czekaj | Guest |

---

## BOOKING SCENARIOS

| # | Guest | Email | Room | Type | Check-In | Check-Out | Status | Payment | Notes |
|:-:|-------|-------|:----:|:----:|:--------:|:---------:|:------:|:-------:|-------|
| 0 | Isabelle Fontaine | cust1@gmail.com | H01 | The Hollow | -2d | +3d | CheckedIn | Pending | Food orders active |
| 1 | Haruto Katsuragi | cust2@gmail.com | O01 | Obsidian | -1d | +4d | CheckedIn | Pending | Butler/Wine/Art amen |
| 2 | Aleksei Volkov | cust3@gmail.com | E01 | Ember | +8d | +13d | Booked | Paid | Future, paid |
| 3 | Nadine El-Amin | cust4@gmail.com | V01 | Vantage | +15d | +19d | Booked | Pending | Future, pending |
| 4 | Constance Morrow | cust5@gmail.com | E02 | Ember | -12d | -7d | CheckedOut | Paid | Past, complete |
| 5 | Emile Renard | emile.renard@gmail.com | H02 | Hollow | -22d | -18d | CheckedOut | Paid | Walk-in, complete |
| 6 | Saoirse Brennan | saoirse.brennan@gmail.com | SW01 | Stillwater | +2d | +5d | Cancelled | Pending | Cancelled pending |
| 7 | Viktor Strauss | viktor.strauss@gmail.com | O02 | Obsidian | -5d | -2d | Cancelled | Pending | No-show |
| 8 | Isabelle Fontaine (2nd) | cust1@gmail.com | V02 | Vantage | -3d | +1d | CheckedIn | Paid | Repeat guest |
| 9 | Haruto Katsuragi (2nd) | cust2@gmail.com | AW01 | Ashwood | -4d | +2d | CheckedIn | Paid | Butler/Chef/Yacht amen |
| 10 | Lena Bergstrom | lena.bergstrom@gmail.com | H03 | Hollow | TODAY | +4d | CheckedIn | Pending | Arriving today |
| 11 | Marcus de Vries | marcus.devries@gmail.com | O02 | Obsidian | -4d | TODAY | CheckedIn | Pending | Departing today |
| 12 | Priya Subramaniam | priya.subramaniam@gmail.com | E03 | Ember | -15d | +15d | CheckedIn | Pending | Long stay 30d |
| 13 | Conrad Hale | conrad.hale@gmail.com | H01 | Hollow | -35d | -33d | CheckedOut | Pending | UNPAID runner |
| 14 | Antoinette Bellerose | antoinette.bellerose@gmail.com | MP01 | Monolith | +25d | +28d | Cancelled | Paid | Awaiting refund |
| 15 | Dmitri Orloff | dmitri.orloff@gmail.com | MP02 | Monolith | +30d | +33d | Cancelled | Refunded | Refund completed |
| 16 | Aleksei Volkov (VVIP) | cust3@gmail.com | SX01+SX02 | Sanctum | -1d | +5d | CheckedIn | Paid | Multi-room, VVIP |
| 17 | Nadine El-Amin (Family) | cust4@gmail.com | SW01+SW02 | Stillwater | -2d | +2d | CheckedIn | Pending | Multi-room family |
| 18 | Constance Morrow (Future) | cust5@gmail.com | MP03 | Monolith | +90d | +95d | Booked | Paid | Far future |
| 19 | Florian Czekaj | florian.czekaj@gmail.com | (none) | Hollow | +20d | +23d | Booked | Pending | No room assigned |

---

## ROOM INVENTORY

| Room | Type | Active | Notes |
|:----:|:----:|:------:|-------|
| H01 | The Hollow | ✓ | See bookings [0], [13] |
| H02 | The Hollow | ✓ | See booking [5] |
| H03 | The Hollow | ✓ | See booking [10] |
| O01 | The Obsidian Chamber | ✓ | See booking [1] |
| O02 | The Obsidian Chamber | ✓ | See booking [7], [11] |
| O03 | The Obsidian Chamber | ✗ | Under renovation |
| E01 | The Ember Suite | ✓ | See booking [2], [12] |
| E02 | The Ember Suite | ✓ | See booking [4] |
| E03 | The Ember Suite | ✓ | See booking [12] |
| V01 | The Vantage Loft | ✓ | See booking [3] |
| V02 | The Vantage Loft | ✓ | See booking [8] |
| V03 | The Vantage Loft | ✓ | (unassigned) |
| SW01 | The Stillwater Villa | ✓ | See booking [6], [17] |
| SW02 | The Stillwater Villa | ✓ | See booking [17] |
| SW03 | The Stillwater Villa | ✗ | (inactive) |
| AW01 | The Ashwood Residence | ✓ | See booking [9] |
| AW02 | The Ashwood Residence | ✓ | (unassigned) |
| AW03 | The Ashwood Residence | ✓ | (unassigned) |
| MP01 | The Monolith Penthouse | ✓ | See booking [14] |
| MP02 | The Monolith Penthouse | ✓ | See booking [15] |
| MP03 | The Monolith Penthouse | ✓ | See booking [18] |
| SX01 | The Sanctum | ✓ | See booking [16] |
| SX02 | The Sanctum | ✓ | See booking [16] |
| SX03 | The Sanctum | ✓ | (standby) |

---

## FOOD ORDER QUICK REFERENCE

| # | Booking | Guest | Status | Items |
|:-:|:-------:|-------|:------:|-------|
| 0 | [0] | Isabelle | Delivered | Caviar + Wagyu + Champagne |
| 1 | [0] | Isabelle | Preparing | Matcha + Chocolate Sphere |
| 2 | [1] | Haruto | Pending | Consommé×2 + Turbot |
| 3 | [1] | Haruto | Delivered | Bone Marrow×2 + Malt×2 |
| 4 | [12] | Priya | Delivered | Stillness Menu×2 |
| 5 | [10] | Lena | Pending | Caviar + Matcha |
| 6 | [11] | Marcus | Preparing | Sabayon + Matcha |
| 7 | [11] | Marcus | Delivered | Scallop×2 + Champagne |
| 8 | [16] | Aleksei (VVIP) | Delivered | Stillness Menu×6 + Champagne×6 + Malt×2 |
| 9 | [16] | Aleksei (VVIP) | Pending | Caviar×6 + Matcha×6 |
| 10 | [17] | Nadine (Family) | Delivered | Wagyu×2 + Turbot×2 + Chocolate×4 + Champagne×2 |
