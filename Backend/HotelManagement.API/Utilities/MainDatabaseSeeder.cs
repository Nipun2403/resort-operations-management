// using HotelManagement.DAL.Context;
// using HotelManagement.DAL.Entities;
// using HotelManagement.DAL.Enums;
// using Microsoft.Extensions.DependencyInjection;
// using System.Text.Json;

// namespace HotelManagement.API.Utilities;

// // ============================================================
// // Aetheris Retreat — Main Database Seeder
// // A standalone estate experience for those who require
// // the absolute removal of all that is unnecessary.
// // ============================================================
// public static class MainDatabaseSeeder
// {
//     public static void Seed(IServiceProvider serviceProvider)
//     {
//         using var scope = serviceProvider.CreateScope();
//         var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

//         if (context.Users.Any()) return;

//         var now = DateTime.UtcNow;

//         // ======================================================
//         // 1. USERS — Staff and high-net-worth guests
//         // ======================================================

//         // --- Staff ---
//         var admin = new User
//         {
//             Email = "director@aetheris.com",
//             PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
//             Role = "Admin",
//             FirstName = "Elara",
//             LastName = "Voss",
//             IsActive = true
//         };

//         var frontdesk1 = new User
//         {
//             Email = "margaux.lefevre@aetheris.com",
//             PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1234"),
//             Role = "FrontDesk",
//             FirstName = "Margaux",
//             LastName = "Lefèvre",
//             IsActive = true
//         };

//         var frontdesk2 = new User
//         {
//             Email = "caspian.reed@aetheris.com",
//             PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1234"),
//             Role = "FrontDesk",
//             FirstName = "Caspian",
//             LastName = "Reed",
//             IsActive = true
//         };

//         var frontdesk3 = new User
//         {
//             Email = "silke.berg@aetheris.com",
//             PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1234"),
//             Role = "FrontDesk",
//             FirstName = "Silke",
//             LastName = "Berg",
//             IsActive = true
//         };

//         var kitchen = new User
//         {
//             Email = "riku.nakamura@aetheris.com",
//             PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1234"),
//             Role = "Kitchen",
//             FirstName = "Riku",
//             LastName = "Nakamura",
//             IsActive = true
//         };

//         var kitchenAsst = new User
//         {
//             Email = "petra.wolff@aetheris.com",
//             PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1234"),
//             Role = "Kitchen",
//             FirstName = "Petra",
//             LastName = "Wolff",
//             IsActive = true
//         };

//         var housekeeping1 = new User
//         {
//             Email = "daria.morel@aetheris.com",
//             PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1234"),
//             Role = "Housekeeping",
//             FirstName = "Daria",
//             LastName = "Morel",
//             IsActive = true
//         };

//         var housekeeping2 = new User
//         {
//             Email = "ivan.kuznetsov@aetheris.com",
//             PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1234"),
//             Role = "Housekeeping",
//             FirstName = "Ivan",
//             LastName = "Kuznetsov",
//             IsActive = true
//         };

//         var maintenance = new User
//         {
//             Email = "felix.schreiber@aetheris.com",
//             PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1234"),
//             Role = "Maintenance",
//             FirstName = "Felix",
//             LastName = "Schreiber",
//             IsActive = true
//         };

//         // Edge-case staff: deactivated concierge
//         var inactiveStaff = new User
//         {
//             Email = "former.concierge@aetheris.com",
//             PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1234"),
//             Role = "FrontDesk",
//             FirstName = "Olivier",
//             LastName = "Renaud",
//             IsActive = false
//         };

//         // --- Guests (registered members) ---
//         var guest1 = new User
//         {
//             Email = "isabelle.fontaine@fontaine-capital.com",
//             PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1234"),
//             Role = "RegisteredUser",
//             FirstName = "Isabelle",
//             LastName = "Fontaine",
//             IsActive = true
//         };

//         var guest2 = new User
//         {
//             Email = "haruto.katsuragi@katsuragi-group.jp",
//             PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1234"),
//             Role = "RegisteredUser",
//             FirstName = "Haruto",
//             LastName = "Katsuragi",
//             IsActive = true
//         };

//         var guest3 = new User
//         {
//             Email = "aleksei.volkov@volkovpartners.ru",
//             PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1234"),
//             Role = "RegisteredUser",
//             FirstName = "Aleksei",
//             LastName = "Volkov",
//             IsActive = true
//         };

//         var guest4 = new User
//         {
//             Email = "nadine.el-amin@privat.ae",
//             PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1234"),
//             Role = "RegisteredUser",
//             FirstName = "Nadine",
//             LastName = "El-Amin",
//             IsActive = true
//         };

//         var guest5 = new User
//         {
//             Email = "constance.morrow@morrow-trust.co.uk",
//             PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1234"),
//             Role = "RegisteredUser",
//             FirstName = "Constance",
//             LastName = "Morrow",
//             IsActive = true
//         };

//         // Edge-case guests
//         var guestNoBookings = new User
//         {
//             Email = "prospective@aetheris-member.com",
//             PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1234"),
//             Role = "RegisteredUser",
//             FirstName = "Théo",
//             LastName = "Sander",
//             IsActive = true
//         };

//         var guestBanned = new User
//         {
//             Email = "flagged.account@domain.com",
//             PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1234"),
//             Role = "RegisteredUser",
//             FirstName = "Banned",
//             LastName = "Account",
//             IsActive = false
//         };

//         var guestPending = new User
//         {
//             Email = "pending.verification@domain.com",
//             PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1234"),
//             Role = "RegisteredUser",
//             FirstName = "Awaiting",
//             LastName = "Verification",
//             IsActive = true
//         };

//         context.Users.AddRange(
//             admin, frontdesk1, frontdesk2, frontdesk3,
//             kitchen, kitchenAsst,
//             housekeeping1, housekeeping2,
//             maintenance, inactiveStaff,
//             guest1, guest2, guest3, guest4, guest5,
//             guestNoBookings, guestBanned, guestPending
//         );
//         context.SaveChanges();

//         // ======================================================
//         // 2. ROOM TYPES — From aetheris.data.json verbatim
//         // ======================================================

//         var hollow = new RoomType
//         {
//             Name = "The Hollow",
//             BasePrice = 3500m,
//             MaxOccupancy = 1,
//             Description = "A single, windowless-feeling cocoon carved for those who wish to disappear entirely — even from their own itinerary. Charcoal limewash walls absorb sound; a single shaft of light falls across the room at dawn and vanishes by ten. There is no minibar, no television, no clock. Aetheris removes what most resorts add, so that solitude has nothing left to compete with.",
//             ImageUrls = new List<string>
//             {
//                 "https://picsum.photos/seed/aetheris-hollow-exterior/1200/800",
//                 "https://picsum.photos/seed/aetheris-hollow-bedroom/1200/800",
//                 "https://picsum.photos/seed/aetheris-hollow-bathroom/1200/800",
//                 "https://picsum.photos/seed/aetheris-hollow-reading-nook/1200/800",
//                 "https://picsum.photos/seed/aetheris-hollow-stone-detail/1200/800",
//                 "https://picsum.photos/seed/aetheris-hollow-dawn-light/1200/800"
//             },
//             SquareFootage = 320,
//             BedConfigurationJson = JsonSerializer.Serialize(new Dictionary<string, int> { { "King", 1 } }),
//             IsActive = true
//         };

//         var obsidian = new RoomType
//         {
//             Name = "The Obsidian Chamber",
//             BasePrice = 4200m,
//             MaxOccupancy = 2,
//             Description = "Named for the volcanic stone that lines its private bath, the Obsidian Chamber sits half-sunken into the hillside so that no window meets another window. A recessed plunge pool of black granite mirrors the night sky. Guests are given no reason to look outward — only inward.",
//             ImageUrls = new List<string>
//             {
//                 "https://picsum.photos/seed/aetheris-obsidian-exterior/1200/800",
//                 "https://picsum.photos/seed/aetheris-obsidian-bedroom/1200/800",
//                 "https://picsum.photos/seed/aetheris-obsidian-plunge-pool/1200/800",
//                 "https://picsum.photos/seed/aetheris-obsidian-bathroom/1200/800",
//                 "https://picsum.photos/seed/aetheris-obsidian-lounge/1200/800",
//                 "https://picsum.photos/seed/aetheris-obsidian-stone-texture/1200/800",
//                 "https://picsum.photos/seed/aetheris-obsidian-night-view/1200/800"
//             },
//             SquareFootage = 480,
//             BedConfigurationJson = JsonSerializer.Serialize(new Dictionary<string, int> { { "King", 1 } }),
//             IsActive = true
//         };

//         var ember = new RoomType
//         {
//             Name = "The Ember Suite",
//             BasePrice = 5800m,
//             MaxOccupancy = 2,
//             Description = "A private hearth burns without smoke or sound behind reinforced glass, its glow the only source of light after dusk. The Ember Suite is reserved for guests who wish to be warmed, not entertained — a room built entirely around the absence of urgency.",
//             ImageUrls = new List<string>
//             {
//                 "https://picsum.photos/seed/aetheris-ember-exterior/1200/800",
//                 "https://picsum.photos/seed/aetheris-ember-bedroom/1200/800",
//                 "https://picsum.photos/seed/aetheris-ember-hearth/1200/800",
//                 "https://picsum.photos/seed/aetheris-ember-daybed/1200/800",
//                 "https://picsum.photos/seed/aetheris-ember-bathroom/1200/800",
//                 "https://picsum.photos/seed/aetheris-ember-firelight-detail/1200/800"
//             },
//             SquareFootage = 620,
//             BedConfigurationJson = JsonSerializer.Serialize(new Dictionary<string, int> { { "King", 1 }, { "Daybed", 1 } }),
//             IsActive = true
//         };

//         var vantage = new RoomType
//         {
//             Name = "The Vantage Loft",
//             BasePrice = 7200m,
//             MaxOccupancy = 2,
//             Description = "Elevated above the treeline on stilts of blackened steel, the Vantage Loft offers a single, uninterrupted view of the valley below and nothing else — no neighboring structure has ever entered its sightline, and none ever will.",
//             ImageUrls = new List<string>
//             {
//                 "https://picsum.photos/seed/aetheris-vantage-exterior/1200/800",
//                 "https://picsum.photos/seed/aetheris-vantage-bedroom/1200/800",
//                 "https://picsum.photos/seed/aetheris-vantage-panoramic-window/1200/800",
//                 "https://picsum.photos/seed/aetheris-vantage-bathroom/1200/800",
//                 "https://picsum.photos/seed/aetheris-vantage-lounge-deck/1200/800",
//                 "https://picsum.photos/seed/aetheris-vantage-valley-view/1200/800"
//             },
//             SquareFootage = 750,
//             BedConfigurationJson = JsonSerializer.Serialize(new Dictionary<string, int> { { "King", 1 } }),
//             IsActive = true
//         };

//         var stillwater = new RoomType
//         {
//             Name = "The Stillwater Villa",
//             BasePrice = 9500m,
//             MaxOccupancy = 4,
//             Description = "A private villa built around a courtyard of unmoving black water, so still it is often mistaken for stone. Two bedrooms open onto the courtyard independently, allowing guests to share proximity without ever sharing presence — togetherness on one's own terms.",
//             ImageUrls = new List<string>
//             {
//                 "https://picsum.photos/seed/aetheris-stillwater-exterior/1200/800",
//                 "https://picsum.photos/seed/aetheris-stillwater-courtyard/1200/800",
//                 "https://picsum.photos/seed/aetheris-stillwater-bedroom-1/1200/800",
//                 "https://picsum.photos/seed/aetheris-stillwater-bedroom-2/1200/800",
//                 "https://picsum.photos/seed/aetheris-stillwater-bathroom/1200/800",
//                 "https://picsum.photos/seed/aetheris-stillwater-living-area/1200/800",
//                 "https://picsum.photos/seed/aetheris-stillwater-water-detail/1200/800"
//             },
//             SquareFootage = 1400,
//             BedConfigurationJson = JsonSerializer.Serialize(new Dictionary<string, int> { { "King", 2 } }),
//             IsActive = true
//         };

//         var ashwood = new RoomType
//         {
//             Name = "The Ashwood Residence",
//             BasePrice = 14000m,
//             MaxOccupancy = 6,
//             Description = "A three-bedroom residence of ash-grey timber and stone, designed for families or retinues who require space between them as much as shelter above them. A private staff entrance ensures the household is served without ever being seen.",
//             ImageUrls = new List<string>
//             {
//                 "https://picsum.photos/seed/aetheris-ashwood-exterior/1200/800",
//                 "https://picsum.photos/seed/aetheris-ashwood-living-room/1200/800",
//                 "https://picsum.photos/seed/aetheris-ashwood-primary-bedroom/1200/800",
//                 "https://picsum.photos/seed/aetheris-ashwood-second-bedroom/1200/800",
//                 "https://picsum.photos/seed/aetheris-ashwood-bathroom/1200/800",
//                 "https://picsum.photos/seed/aetheris-ashwood-dining-area/1200/800",
//                 "https://picsum.photos/seed/aetheris-ashwood-timber-detail/1200/800"
//             },
//             SquareFootage = 2600,
//             BedConfigurationJson = JsonSerializer.Serialize(new Dictionary<string, int> { { "King", 3 } }),
//             IsActive = true
//         };

//         var monolith = new RoomType
//         {
//             Name = "The Monolith Penthouse",
//             BasePrice = 22000m,
//             MaxOccupancy = 4,
//             Description = "Occupying the retreat's highest point, the Monolith is a single slab of dark concrete and glass that appears, from any distance, to be uninhabited. Inside, every surface has been chosen to disappear at night, leaving only the guest and the horizon.",
//             ImageUrls = new List<string>
//             {
//                 "https://picsum.photos/seed/aetheris-monolith-exterior/1200/800",
//                 "https://picsum.photos/seed/aetheris-monolith-living-room/1200/800",
//                 "https://picsum.photos/seed/aetheris-monolith-primary-bedroom/1200/800",
//                 "https://picsum.photos/seed/aetheris-monolith-bathroom/1200/800",
//                 "https://picsum.photos/seed/aetheris-monolith-terrace/1200/800",
//                 "https://picsum.photos/seed/aetheris-monolith-concrete-detail/1200/800",
//                 "https://picsum.photos/seed/aetheris-monolith-horizon-view/1200/800"
//             },
//             SquareFootage = 3200,
//             BedConfigurationJson = JsonSerializer.Serialize(new Dictionary<string, int> { { "King", 2 }, { "Queen", 1 } }),
//             IsActive = true
//         };

//         var sanctum = new RoomType
//         {
//             Name = "The Sanctum",
//             BasePrice = 30000m,
//             MaxOccupancy = 8,
//             Description = "A standalone estate at the farthest edge of the property, reachable only by private escort. The Sanctum is the only accommodation at Aetheris with its own perimeter, its own staff quarters, and its own silence — an entire world reserved for a single party.",
//             ImageUrls = new List<string>
//             {
//                 "https://picsum.photos/seed/aetheris-sanctum-estate-exterior/1200/800",
//                 "https://picsum.photos/seed/aetheris-sanctum-great-room/1200/800",
//                 "https://picsum.photos/seed/aetheris-sanctum-primary-suite/1200/800",
//                 "https://picsum.photos/seed/aetheris-sanctum-guest-suite/1200/800",
//                 "https://picsum.photos/seed/aetheris-sanctum-bathroom/1200/800",
//                 "https://picsum.photos/seed/aetheris-sanctum-private-perimeter/1200/800",
//                 "https://picsum.photos/seed/aetheris-sanctum-staff-wing/1200/800",
//                 "https://picsum.photos/seed/aetheris-sanctum-estate-grounds/1200/800"
//             },
//             SquareFootage = 6500,
//             BedConfigurationJson = JsonSerializer.Serialize(new Dictionary<string, int> { { "King", 4 } }),
//             IsActive = true
//         };

//         context.RoomTypes.AddRange(hollow, obsidian, ember, vantage, stillwater, ashwood, monolith, sanctum);
//         context.SaveChanges();

//         // ======================================================
//         // 3. ROOMS — 3 per type (24 total). Mix of active,
//         //    offline-for-renovation, and one retired.
//         //    Numbering: [TypeCode][01-03]
//         //    H=Hollow, O=Obsidian, E=Ember, V=Vantage,
//         //    S=Stillwater, A=Ashwood, M=Monolith, X=Sanctum
//         // ======================================================
//         var rooms = new List<Room>
//         {
//             // The Hollow (H01-H03)
//             new Room { RoomNumber = "H01", RoomTypeId = hollow.Id, IsActive = true },
//             new Room { RoomNumber = "H02", RoomTypeId = hollow.Id, IsActive = true },
//             new Room { RoomNumber = "H03", RoomTypeId = hollow.Id, IsActive = true },

//             // The Obsidian Chamber (O01-O03)
//             new Room { RoomNumber = "O01", RoomTypeId = obsidian.Id, IsActive = true },
//             new Room { RoomNumber = "O02", RoomTypeId = obsidian.Id, IsActive = true },
//             new Room { RoomNumber = "O03", RoomTypeId = obsidian.Id, IsActive = false }, // Offline: volcanic stone re-sealing

//             // The Ember Suite (E01-E03)
//             new Room { RoomNumber = "E01", RoomTypeId = ember.Id, IsActive = true },
//             new Room { RoomNumber = "E02", RoomTypeId = ember.Id, IsActive = true },
//             new Room { RoomNumber = "E03", RoomTypeId = ember.Id, IsActive = true },

//             // The Vantage Loft (V01-V03)
//             new Room { RoomNumber = "V01", RoomTypeId = vantage.Id, IsActive = true },
//             new Room { RoomNumber = "V02", RoomTypeId = vantage.Id, IsActive = true },
//             new Room { RoomNumber = "V03", RoomTypeId = vantage.Id, IsActive = true },

//             // The Stillwater Villa (SW01-SW03)
//             new Room { RoomNumber = "SW01", RoomTypeId = stillwater.Id, IsActive = true },
//             new Room { RoomNumber = "SW02", RoomTypeId = stillwater.Id, IsActive = true },
//             new Room { RoomNumber = "SW03", RoomTypeId = stillwater.Id, IsActive = false }, // Under renovation: courtyard reseal

//             // The Ashwood Residence (AW01-AW03)
//             new Room { RoomNumber = "AW01", RoomTypeId = ashwood.Id, IsActive = true },
//             new Room { RoomNumber = "AW02", RoomTypeId = ashwood.Id, IsActive = true },
//             new Room { RoomNumber = "AW03", RoomTypeId = ashwood.Id, IsActive = true },

//             // The Monolith Penthouse (MP01-MP03)
//             new Room { RoomNumber = "MP01", RoomTypeId = monolith.Id, IsActive = true },
//             new Room { RoomNumber = "MP02", RoomTypeId = monolith.Id, IsActive = true },
//             new Room { RoomNumber = "MP03", RoomTypeId = monolith.Id, IsActive = true },

//             // The Sanctum (SX01-SX03)
//             new Room { RoomNumber = "SX01", RoomTypeId = sanctum.Id, IsActive = true },
//             new Room { RoomNumber = "SX02", RoomTypeId = sanctum.Id, IsActive = true },
//             new Room { RoomNumber = "SX03", RoomTypeId = sanctum.Id, IsActive = true },
//         };

//         // Room index reference (0-based):
//         // H01=0, H02=1, H03=2
//         // O01=3, O02=4, O03=5 (offline)
//         // E01=6, E02=7, E03=8
//         // V01=9, V02=10, V03=11
//         // SW01=12, SW02=13, SW03=14 (offline)
//         // AW01=15, AW02=16, AW03=17
//         // MP01=18, MP02=19, MP03=20
//         // SX01=21, SX02=22, SX03=23

//         context.Rooms.AddRange(rooms);
//         context.SaveChanges();

//         // ======================================================
//         // 4. MENU ITEMS — From aetheris.data.json verbatim
//         // ======================================================
//         var menu = new List<MenuItem>
//         {
//             // [0] Amuse-Bouche
//             new MenuItem
//             {
//                 Name = "Midnight Caviar Spoon",
//                 Price = 180m,
//                 Category = "Amuse-Bouche",
//                 IsAvailable = true,
//                 Description = "A single spoon of Osetra caviar over potato blini and crème fraîche, finished with one shard of edible gold. Served alone, on a black slate, with nothing beside it.",
//                 ImageUrl = "https://picsum.photos/seed/aetheris-caviar-spoon/900/700"
//             },
//             // [1] Appetizer
//             new MenuItem
//             {
//                 Name = "Charred Bone Marrow & Black Truffle",
//                 Price = 240m,
//                 Category = "Appetizer",
//                 IsAvailable = true,
//                 Description = "Roasted marrow bone split tableside, finished with shaved black truffle and a whisper of smoked sea salt. Eaten with nothing but a spoon and silence.",
//                 ImageUrl = "https://picsum.photos/seed/aetheris-bone-marrow/900/700"
//             },
//             // [2] Appetizer
//             new MenuItem
//             {
//                 Name = "Hokkaido Scallop, Ash-Roasted",
//                 Price = 260m,
//                 Category = "Appetizer",
//                 IsAvailable = true,
//                 Description = "Diver scallop roasted in hearth ash, served in its own shell over a pool of brown butter and yuzu kosho.",
//                 ImageUrl = "https://picsum.photos/seed/aetheris-scallop-ash/900/700"
//             },
//             // [3] Soup
//             new MenuItem
//             {
//                 Name = "Smoked Wild Mushroom Consommé",
//                 Price = 150m,
//                 Category = "Soup",
//                 IsAvailable = true,
//                 Description = "A clarified broth of foraged mushrooms, smoked over embers for six hours and poured tableside from a blackened kettle.",
//                 ImageUrl = "https://picsum.photos/seed/aetheris-consomme/900/700"
//             },
//             // [4] Main Course
//             new MenuItem
//             {
//                 Name = "Dry-Aged Wagyu, Ember-Seared",
//                 Price = 620m,
//                 Category = "Main Course",
//                 IsAvailable = true,
//                 Description = "A5 wagyu, dry-aged sixty days and seared directly over open ember, sliced tableside and served without garnish — the ingredient is the entire statement.",
//                 ImageUrl = "https://picsum.photos/seed/aetheris-wagyu-ember/900/700"
//             },
//             // [5] Main Course
//             new MenuItem
//             {
//                 Name = "Whole Roasted Turbot, Champagne Butter",
//                 Price = 540m,
//                 Category = "Main Course",
//                 IsAvailable = true,
//                 Description = "Whole turbot roasted on the bone and finished tableside with a champagne beurre blanc, deboned in front of the guest.",
//                 ImageUrl = "https://picsum.photos/seed/aetheris-turbot/900/700"
//             },
//             // [6] Main Course
//             new MenuItem
//             {
//                 Name = "Squab en Croûte with Foie Gras",
//                 Price = 580m,
//                 Category = "Main Course",
//                 IsAvailable = false, // Seasonal — currently unavailable
//                 Description = "Roasted squab wrapped in foie gras and pastry, rested and carved in the room, paired with a reduction of aged port.",
//                 ImageUrl = "https://picsum.photos/seed/aetheris-squab-croute/900/700"
//             },
//             // [7] Dessert
//             new MenuItem
//             {
//                 Name = "Dark Chocolate & Gold Dust Sphere",
//                 Price = 190m,
//                 Category = "Dessert",
//                 IsAvailable = true,
//                 Description = "A sphere of 72% dark chocolate that dissolves at the table under a pour of warm salted caramel, dusted with edible gold.",
//                 ImageUrl = "https://picsum.photos/seed/aetheris-chocolate-sphere/900/700"
//             },
//             // [8] Dessert
//             new MenuItem
//             {
//                 Name = "Champagne Sabayon with Black Fig",
//                 Price = 170m,
//                 Category = "Dessert",
//                 IsAvailable = true,
//                 Description = "A warm, airy sabayon whisked with vintage champagne, served over roasted black figs and toasted hazelnut.",
//                 ImageUrl = "https://picsum.photos/seed/aetheris-sabayon-fig/900/700"
//             },
//             // [9] Beverage
//             new MenuItem
//             {
//                 Name = "Aetheris Reserve Champagne",
//                 Price = 950m,
//                 Category = "Beverage",
//                 IsAvailable = true,
//                 Description = "A single vintage bottle, cellared exclusively for Aetheris and released to no other property in the world.",
//                 ImageUrl = "https://picsum.photos/seed/aetheris-champagne/900/700"
//             },
//             // [10] Beverage
//             new MenuItem
//             {
//                 Name = "Single-Origin Kyoto Matcha Ceremony",
//                 Price = 120m,
//                 Category = "Beverage",
//                 IsAvailable = true,
//                 Description = "A formal, silent matcha preparation performed tableside using ceremonial-grade leaves from a single Kyoto estate.",
//                 ImageUrl = "https://picsum.photos/seed/aetheris-matcha/900/700"
//             },
//             // [11] Beverage
//             new MenuItem
//             {
//                 Name = "25-Year Single Malt, Neat",
//                 Price = 480m,
//                 Category = "Beverage",
//                 IsAvailable = true,
//                 Description = "A quarter-century single malt from a private cask, poured neat and left to breathe in silence before service.",
//                 ImageUrl = "https://picsum.photos/seed/aetheris-single-malt/900/700"
//             },
//             // [12] Tasting Menu
//             new MenuItem
//             {
//                 Name = "The Stillness Tasting Menu",
//                 Price = 1200m,
//                 Category = "Tasting Menu",
//                 IsAvailable = true,
//                 Description = "A seven-course progression built around absence rather than abundance — each course simpler than the last, ending in a single unadorned course meant to be eaten in complete silence.",
//                 ImageUrl = "https://picsum.photos/seed/aetheris-stillness-menu/900/700"
//             }
//         };

//         context.MenuItems.AddRange(menu);
//         context.SaveChanges();

//         // ======================================================
//         // 5. AMENITIES — From aetheris.data.json verbatim
//         // ======================================================
//         var amenities = new List<Amenity>
//         {
//             // [0]
//             new Amenity
//             {
//                 Name = "Private Chef In-Suite Dining",
//                 Description = "A dedicated chef prepares and serves a personalized multi-course dinner entirely within your suite, so that even the dining room becomes unnecessary.",
//                 Price = 2500m,
//                 IsAvailable = true,
//                 ImageUrl = "https://picsum.photos/seed/aetheris-private-chef/900/700"
//             },
//             // [1]
//             new Amenity
//             {
//                 Name = "Personal Butler Service (24hr)",
//                 Description = "A single, permanently assigned butler attends to every need for the duration of your stay, communicating only when spoken to.",
//                 Price = 1800m,
//                 IsAvailable = true,
//                 ImageUrl = "https://picsum.photos/seed/aetheris-butler/900/700"
//             },
//             // [2]
//             new Amenity
//             {
//                 Name = "Helicopter Transfer",
//                 Description = "Direct, private helicopter transfer between Aetheris and the nearest discreet airfield, bypassing all public arrival points.",
//                 Price = 8500m,
//                 IsAvailable = true,
//                 ImageUrl = "https://picsum.photos/seed/aetheris-helicopter/900/700"
//             },
//             // [3]
//             new Amenity
//             {
//                 Name = "Private Yacht Charter (Half-Day)",
//                 Description = "A crewed private yacht reserved exclusively for your party, departing from Aetheris's unmarked private dock.",
//                 Price = 12000m,
//                 IsAvailable = true,
//                 ImageUrl = "https://picsum.photos/seed/aetheris-yacht/900/700"
//             },
//             // [4]
//             new Amenity
//             {
//                 Name = "Floatation Sensory Deprivation Therapy",
//                 Description = "A private session in a sealed floatation chamber, removing sound, light, and gravity to return the mind to true stillness.",
//                 Price = 650m,
//                 IsAvailable = true,
//                 ImageUrl = "https://picsum.photos/seed/aetheris-floatation/900/700"
//             },
//             // [5]
//             new Amenity
//             {
//                 Name = "In-Suite Spa & Massage",
//                 Description = "A full therapeutic massage delivered entirely within your suite by a private therapist, using oils blended for your stay alone.",
//                 Price = 900m,
//                 IsAvailable = true,
//                 ImageUrl = "https://picsum.photos/seed/aetheris-spa-massage/900/700"
//             },
//             // [6]
//             new Amenity
//             {
//                 Name = "Private Wine Cellar Tasting",
//                 Description = "An exclusive after-hours tasting inside Aetheris's subterranean cellar, guided by the resident sommelier for your party only.",
//                 Price = 1200m,
//                 IsAvailable = true,
//                 ImageUrl = "https://picsum.photos/seed/aetheris-wine-cellar/900/700"
//             },
//             // [7]
//             new Amenity
//             {
//                 Name = "Digital Detox Concierge",
//                 Description = "A dedicated concierge takes custody of all devices for the duration of your stay and curates an entirely offline itinerary in their place.",
//                 Price = 300m,
//                 IsAvailable = true,
//                 ImageUrl = "https://picsum.photos/seed/aetheris-digital-detox/900/700"
//             },
//             // [8]
//             new Amenity
//             {
//                 Name = "Stargazing & Telescope Session",
//                 Description = "A private astronomer guides a late-night observation session using Aetheris's observatory-grade telescope, far from any ambient light.",
//                 Price = 550m,
//                 IsAvailable = true,
//                 ImageUrl = "https://picsum.photos/seed/aetheris-stargazing/900/700"
//             },
//             // [9]
//             new Amenity
//             {
//                 Name = "Private Cinema Screening",
//                 Description = "The resort's single-screen cinema reserved entirely for your party, with any film sourced and screened on request.",
//                 Price = 1600m,
//                 IsAvailable = true,
//                 ImageUrl = "https://picsum.photos/seed/aetheris-cinema/900/700"
//             },
//             // [10]
//             new Amenity
//             {
//                 Name = "Meditation & Sound Bath Session",
//                 Description = "A private, guided meditation and sound bath performed with hand-forged singing bowls, held in the resort's stone chamber.",
//                 Price = 400m,
//                 IsAvailable = true,
//                 ImageUrl = "https://picsum.photos/seed/aetheris-sound-bath/900/700"
//             },
//             // [11]
//             new Amenity
//             {
//                 Name = "Executive Protection Detail (Per Day)",
//                 Description = "A discreet, professionally trained security detail assigned solely to your party, present but never visible.",
//                 Price = 3200m,
//                 IsAvailable = true,
//                 ImageUrl = "https://picsum.photos/seed/aetheris-security/900/700"
//             },
//             // [12]
//             new Amenity
//             {
//                 Name = "Falconry Experience",
//                 Description = "A private falconry session on the resort's private grounds, led by a resident falconer, held at dawn when the grounds are entirely empty.",
//                 Price = 750m,
//                 IsAvailable = true,
//                 ImageUrl = "https://picsum.photos/seed/aetheris-falconry/900/700"
//             },
//             // [13]
//             new Amenity
//             {
//                 Name = "Private Art Curator Tour",
//                 Description = "A one-on-one walkthrough of Aetheris's private art collection, guided by a resident curator, arranged exclusively around your schedule.",
//                 Price = 950m,
//                 IsAvailable = true,
//                 ImageUrl = "https://picsum.photos/seed/aetheris-art-curator/900/700"
//             }
//         };

//         context.Amenities.AddRange(amenities);
//         context.SaveChanges();

//         // ======================================================
//         // 6. BOOKINGS — 20 bookings covering all scenarios
//         //
//         // Booking index legend:
//         //  [0]  Isabelle Fontaine — CheckedIn, Pending       (H01)
//         //  [1]  Haruto Katsuragi — CheckedIn, Pending        (O01)
//         //  [2]  Aleksei Volkov   — Future, Paid              (E01)
//         //  [3]  Nadine El-Amin   — Future, Pending           (V01)
//         //  [4]  Constance Morrow — CheckedOut, Paid          (E02)
//         //  [5]  Walk-in guest    — CheckedOut, Paid          (H02)
//         //  [6]  Walk-in guest    — Cancelled, Pending        (SW01)
//         //  [7]  Walk-in guest    — Cancelled, NoShow         (O02)
//         //  [8]  Isabelle (2nd)   — CheckedIn, Paid           (V02)
//         //  [9]  Haruto (2nd)     — CheckedIn, Paid           (AW01)
//         // [10]  Arriving today   — CheckedIn, Pending        (H03)
//         // [11]  Departing today  — CheckedIn, Pending        (O01 — shared, guest #7 gone)
//         // [12]  Long stay 30d    — CheckedIn, Pending        (E03)
//         // [13]  CheckedOut, Unpaid (runner)                  (H01)
//         // [14]  Cancelled, Paid (needs refund)               (MP01)
//         // [15]  Cancelled, Refunded                          (MP02)
//         // [16]  VVIP Sanctum (multi-room, estate booking)    (SX01+SX02)
//         // [17]  Multi-room family stay                       (SW01+SW02)
//         // [18]  Future far-ahead (pre-booked)                (Monolith)
//         // [19]  Registered guest, booked, room not yet assigned
//         // ======================================================
//         var bookings = new List<Booking>
//         {
//             // ── CHECKED-IN / ACTIVE ───────────────────────────────

//             // [0] Isabelle Fontaine — solitude seeker, The Hollow
//             new Booking
//             {
//                 GuestName = "Isabelle Fontaine",
//                 GuestEmail = "isabelle.fontaine@fontaine-capital.com",
//                 UserId = guest1.Id,
//                 GuestCount = 1,
//                 CheckInDate = now.AddDays(-2),
//                 CheckOutDate = now.AddDays(3),
//                 BookingStatus = BookingStatus.CheckedIn,
//                 PaymentStatus = PaymentStatus.Pending,
//                 Origin = BookingOrigin.RegisteredUser,
//                 BookedAt = now.AddDays(-14),
//                 BookingRooms = new List<BookingRoom>
//                 {
//                     new BookingRoom { RoomTypeId = hollow.Id, RoomId = rooms[0].Id, LockedInPrice = 3500m }
//                 }
//             },

//             // [1] Haruto Katsuragi — corporate retreat, The Obsidian Chamber
//             new Booking
//             {
//                 GuestName = "Haruto Katsuragi",
//                 GuestEmail = "haruto.katsuragi@katsuragi-group.jp",
//                 UserId = guest2.Id,
//                 GuestCount = 2,
//                 CheckInDate = now.AddDays(-1),
//                 CheckOutDate = now.AddDays(4),
//                 BookingStatus = BookingStatus.CheckedIn,
//                 PaymentStatus = PaymentStatus.Pending,
//                 Origin = BookingOrigin.RegisteredUser,
//                 BookedAt = now.AddDays(-30),
//                 BookingRooms = new List<BookingRoom>
//                 {
//                     new BookingRoom { RoomTypeId = obsidian.Id, RoomId = rooms[3].Id, LockedInPrice = 4200m }
//                 }
//             },

//             // [2] Aleksei Volkov — future confirmed, pre-paid, Ember Suite
//             new Booking
//             {
//                 GuestName = "Aleksei Volkov",
//                 GuestEmail = "aleksei.volkov@volkovpartners.ru",
//                 UserId = guest3.Id,
//                 GuestCount = 2,
//                 CheckInDate = now.AddDays(8),
//                 CheckOutDate = now.AddDays(13),
//                 BookingStatus = BookingStatus.Booked,
//                 PaymentStatus = PaymentStatus.Paid,
//                 Origin = BookingOrigin.RegisteredUser,
//                 BookedAt = now.AddDays(-20),
//                 BookingRooms = new List<BookingRoom>
//                 {
//                     new BookingRoom { RoomTypeId = ember.Id, RoomId = null, LockedInPrice = 5800m }
//                 }
//             },

//             // [3] Nadine El-Amin — future pending, Vantage Loft
//             new Booking
//             {
//                 GuestName = "Nadine El-Amin",
//                 GuestEmail = "nadine.el-amin@privat.ae",
//                 UserId = guest4.Id,
//                 GuestCount = 2,
//                 CheckInDate = now.AddDays(15),
//                 CheckOutDate = now.AddDays(19),
//                 BookingStatus = BookingStatus.Booked,
//                 PaymentStatus = PaymentStatus.Pending,
//                 Origin = BookingOrigin.RegisteredUser,
//                 BookedAt = now.AddDays(-5),
//                 BookingRooms = new List<BookingRoom>
//                 {
//                     new BookingRoom { RoomTypeId = vantage.Id, RoomId = null, LockedInPrice = 7200m }
//                 }
//             },

//             // [4] Constance Morrow — checked out, paid, Ember Suite
//             new Booking
//             {
//                 GuestName = "Constance Morrow",
//                 GuestEmail = "constance.morrow@morrow-trust.co.uk",
//                 UserId = guest5.Id,
//                 GuestCount = 1,
//                 CheckInDate = now.AddDays(-12),
//                 CheckOutDate = now.AddDays(-7),
//                 BookingStatus = BookingStatus.CheckedOut,
//                 PaymentStatus = PaymentStatus.Paid,
//                 Origin = BookingOrigin.RegisteredUser,
//                 BookedAt = now.AddDays(-40),
//                 BookingRooms = new List<BookingRoom>
//                 {
//                     new BookingRoom { RoomTypeId = ember.Id, RoomId = rooms[7].Id, LockedInPrice = 5800m }
//                 }
//             },

//             // [5] Émile Renard — walk-in, checked out, paid, The Hollow
//             new Booking
//             {
//                 GuestName = "Émile Renard",
//                 GuestEmail = "emile.renard@privat.fr",
//                 GuestCount = 1,
//                 CheckInDate = now.AddDays(-22),
//                 CheckOutDate = now.AddDays(-18),
//                 BookingStatus = BookingStatus.CheckedOut,
//                 PaymentStatus = PaymentStatus.Paid,
//                 Origin = BookingOrigin.WalkIn,
//                 BookedAt = now.AddDays(-22),
//                 BookingRooms = new List<BookingRoom>
//                 {
//                     new BookingRoom { RoomTypeId = hollow.Id, RoomId = rooms[1].Id, LockedInPrice = 3500m }
//                 }
//             },

//             // [6] Cancelled, Pending — Saoirse Brennan, Stillwater Villa
//             new Booking
//             {
//                 GuestName = "Saoirse Brennan",
//                 GuestEmail = "saoirse.brennan@domain.ie",
//                 GuestCount = 3,
//                 CheckInDate = now.AddDays(2),
//                 CheckOutDate = now.AddDays(5),
//                 BookingStatus = BookingStatus.Cancelled,
//                 PaymentStatus = PaymentStatus.Pending,
//                 Origin = BookingOrigin.Guest,
//                 BookedAt = now.AddDays(-8),
//                 BookingRooms = new List<BookingRoom>
//                 {
//                     new BookingRoom { RoomTypeId = stillwater.Id, RoomId = null, LockedInPrice = 9500m }
//                 }
//             },

//             // [7] Cancelled — No-Show, Obsidian Chamber
//             new Booking
//             {
//                 GuestName = "Viktor Strauss",
//                 GuestEmail = "v.strauss@domain.de",
//                 GuestCount = 2,
//                 CheckInDate = now.AddDays(-5),
//                 CheckOutDate = now.AddDays(-2),
//                 BookingStatus = BookingStatus.Cancelled,
//                 PaymentStatus = PaymentStatus.Pending,
//                 Origin = BookingOrigin.Guest,
//                 BookedAt = now.AddDays(-25),
//                 BookingRooms = new List<BookingRoom>
//                 {
//                     new BookingRoom { RoomTypeId = obsidian.Id, RoomId = null, LockedInPrice = 4200m }
//                 }
//             },

//             // [8] Isabelle's second stay — CheckedIn, Paid, Vantage Loft
//             new Booking
//             {
//                 GuestName = "Isabelle Fontaine",
//                 GuestEmail = "isabelle.fontaine@fontaine-capital.com",
//                 UserId = guest1.Id,
//                 GuestCount = 2,
//                 CheckInDate = now.AddDays(-3),
//                 CheckOutDate = now.AddDays(1),
//                 BookingStatus = BookingStatus.CheckedIn,
//                 PaymentStatus = PaymentStatus.Paid,
//                 Origin = BookingOrigin.RegisteredUser,
//                 BookedAt = now.AddDays(-60),
//                 BookingRooms = new List<BookingRoom>
//                 {
//                     new BookingRoom { RoomTypeId = vantage.Id, RoomId = rooms[10].Id, LockedInPrice = 7200m }
//                 }
//             },

//             // [9] Haruto — extended stay, Ashwood Residence
//             new Booking
//             {
//                 GuestName = "Haruto Katsuragi",
//                 GuestEmail = "haruto.katsuragi@katsuragi-group.jp",
//                 UserId = guest2.Id,
//                 GuestCount = 4,
//                 CheckInDate = now.AddDays(-4),
//                 CheckOutDate = now.AddDays(2),
//                 BookingStatus = BookingStatus.CheckedIn,
//                 PaymentStatus = PaymentStatus.Paid,
//                 Origin = BookingOrigin.RegisteredUser,
//                 BookedAt = now.AddDays(-45),
//                 BookingRooms = new List<BookingRoom>
//                 {
//                     new BookingRoom { RoomTypeId = ashwood.Id, RoomId = rooms[15].Id, LockedInPrice = 14000m }
//                 }
//             },

//             // ── TODAY ARRIVAL & DEPARTURE ─────────────────────────

//             // [10] Arriving today — The Hollow H03
//             new Booking
//             {
//                 GuestName = "Lena Bergström",
//                 GuestEmail = "lena.bergstrom@privat.se",
//                 GuestCount = 1,
//                 CheckInDate = now.Date,
//                 CheckOutDate = now.Date.AddDays(4),
//                 BookingStatus = BookingStatus.CheckedIn,
//                 PaymentStatus = PaymentStatus.Pending,
//                 Origin = BookingOrigin.Guest,
//                 BookedAt = now.AddDays(-7),
//                 BookingRooms = new List<BookingRoom>
//                 {
//                     new BookingRoom { RoomTypeId = hollow.Id, RoomId = rooms[2].Id, LockedInPrice = 3500m }
//                 }
//             },

//             // [11] Departing today — Obsidian O02
//             new Booking
//             {
//                 GuestName = "Marcus de Vries",
//                 GuestEmail = "marcus.devries@privat.nl",
//                 GuestCount = 2,
//                 CheckInDate = now.Date.AddDays(-4),
//                 CheckOutDate = now.Date,
//                 BookingStatus = BookingStatus.CheckedIn,
//                 PaymentStatus = PaymentStatus.Pending,
//                 Origin = BookingOrigin.WalkIn,
//                 BookedAt = now.Date.AddDays(-4),
//                 BookingRooms = new List<BookingRoom>
//                 {
//                     new BookingRoom { RoomTypeId = obsidian.Id, RoomId = rooms[4].Id, LockedInPrice = 4200m }
//                 }
//             },

//             // ── EDGE CASES ────────────────────────────────────────

//             // [12] Long stay — 30 nights, Ember E01
//             new Booking
//             {
//                 GuestName = "Priya Subramaniam",
//                 GuestEmail = "priya.subramaniam@domain.in",
//                 GuestCount = 2,
//                 CheckInDate = now.Date.AddDays(-15),
//                 CheckOutDate = now.Date.AddDays(15),
//                 BookingStatus = BookingStatus.CheckedIn,
//                 PaymentStatus = PaymentStatus.Pending,
//                 Origin = BookingOrigin.Guest,
//                 BookedAt = now.AddDays(-90),
//                 BookingRooms = new List<BookingRoom>
//                 {
//                     new BookingRoom { RoomTypeId = ember.Id, RoomId = rooms[6].Id, LockedInPrice = 5800m }
//                 }
//             },

//             // [13] Checked out, UNPAID — guest left without settling
//             new Booking
//             {
//                 GuestName = "Conrad Hale",
//                 GuestEmail = "c.hale@unknown.com",
//                 GuestCount = 1,
//                 CheckInDate = now.Date.AddDays(-35),
//                 CheckOutDate = now.Date.AddDays(-33),
//                 BookingStatus = BookingStatus.CheckedOut,
//                 PaymentStatus = PaymentStatus.Pending,
//                 Origin = BookingOrigin.WalkIn,
//                 BookedAt = now.Date.AddDays(-35),
//                 BookingRooms = new List<BookingRoom>
//                 {
//                     new BookingRoom { RoomTypeId = hollow.Id, RoomId = rooms[0].Id, LockedInPrice = 3500m }
//                 }
//             },

//             // [14] Cancelled, Paid — awaiting refund, Monolith MP01
//             new Booking
//             {
//                 GuestName = "Antoinette Bellerose",
//                 GuestEmail = "a.bellerose@maison-b.fr",
//                 GuestCount = 3,
//                 CheckInDate = now.Date.AddDays(25),
//                 CheckOutDate = now.Date.AddDays(28),
//                 BookingStatus = BookingStatus.Cancelled,
//                 PaymentStatus = PaymentStatus.Paid,
//                 Origin = BookingOrigin.Guest,
//                 BookedAt = now.AddDays(-50),
//                 BookingRooms = new List<BookingRoom>
//                 {
//                     new BookingRoom { RoomTypeId = monolith.Id, RoomId = null, LockedInPrice = 22000m }
//                 }
//             },

//             // [15] Cancelled, Refunded — Monolith MP02
//             new Booking
//             {
//                 GuestName = "Dmitri Orloff",
//                 GuestEmail = "d.orloff@domain.ru",
//                 GuestCount = 2,
//                 CheckInDate = now.Date.AddDays(30),
//                 CheckOutDate = now.Date.AddDays(33),
//                 BookingStatus = BookingStatus.Cancelled,
//                 PaymentStatus = PaymentStatus.Refunded,
//                 Origin = BookingOrigin.Guest,
//                 BookedAt = now.AddDays(-60),
//                 BookingRooms = new List<BookingRoom>
//                 {
//                     new BookingRoom { RoomTypeId = monolith.Id, RoomId = null, LockedInPrice = 22000m }
//                 }
//             },

//             // [16] VVIP Sanctum — estate booking, multi-room (SX01 + SX02)
//             //      Aleksei Volkov + party of 6, helicopter in, full retinue
//             new Booking
//             {
//                 GuestName = "Aleksei Volkov",
//                 GuestEmail = "aleksei.volkov@volkovpartners.ru",
//                 UserId = guest3.Id,
//                 GuestCount = 6,
//                 CheckInDate = now.AddDays(-1),
//                 CheckOutDate = now.AddDays(5),
//                 BookingStatus = BookingStatus.CheckedIn,
//                 PaymentStatus = PaymentStatus.Paid,
//                 Origin = BookingOrigin.RegisteredUser,
//                 BookedAt = now.AddDays(-120),
//                 BookingRooms = new List<BookingRoom>
//                 {
//                     new BookingRoom { RoomTypeId = sanctum.Id, RoomId = rooms[21].Id, LockedInPrice = 30000m },
//                     new BookingRoom { RoomTypeId = sanctum.Id, RoomId = rooms[22].Id, LockedInPrice = 30000m }
//                 }
//             },

//             // [17] Multi-room family — Stillwater SW01 + SW02
//             new Booking
//             {
//                 GuestName = "Nadine El-Amin",
//                 GuestEmail = "nadine.el-amin@privat.ae",
//                 UserId = guest4.Id,
//                 GuestCount = 4,
//                 CheckInDate = now.AddDays(-2),
//                 CheckOutDate = now.AddDays(2),
//                 BookingStatus = BookingStatus.CheckedIn,
//                 PaymentStatus = PaymentStatus.Pending,
//                 Origin = BookingOrigin.RegisteredUser,
//                 BookedAt = now.AddDays(-30),
//                 BookingRooms = new List<BookingRoom>
//                 {
//                     new BookingRoom { RoomTypeId = stillwater.Id, RoomId = rooms[12].Id, LockedInPrice = 9500m },
//                     new BookingRoom { RoomTypeId = stillwater.Id, RoomId = rooms[13].Id, LockedInPrice = 9500m }
//                 }
//             },

//             // [18] Far-future pre-booking — Monolith, Constance
//             new Booking
//             {
//                 GuestName = "Constance Morrow",
//                 GuestEmail = "constance.morrow@morrow-trust.co.uk",
//                 UserId = guest5.Id,
//                 GuestCount = 2,
//                 CheckInDate = now.AddDays(90),
//                 CheckOutDate = now.AddDays(95),
//                 BookingStatus = BookingStatus.Booked,
//                 PaymentStatus = PaymentStatus.Paid,
//                 Origin = BookingOrigin.RegisteredUser,
//                 BookedAt = now.AddDays(-10),
//                 BookingRooms = new List<BookingRoom>
//                 {
//                     new BookingRoom { RoomTypeId = monolith.Id, RoomId = null, LockedInPrice = 22000m }
//                 }
//             },

//             // [19] Guest-origin future booking — room type only, no room assigned yet
//             new Booking
//             {
//                 GuestName = "Florian Czekaj",
//                 GuestEmail = "florian.czekaj@domain.pl",
//                 GuestCount = 1,
//                 CheckInDate = now.AddDays(20),
//                 CheckOutDate = now.AddDays(23),
//                 BookingStatus = BookingStatus.Booked,
//                 PaymentStatus = PaymentStatus.Pending,
//                 Origin = BookingOrigin.Guest,
//                 BookedAt = now.AddDays(-2),
//                 BookingRooms = new List<BookingRoom>
//                 {
//                     new BookingRoom { RoomTypeId = hollow.Id, RoomId = null, LockedInPrice = 3500m }
//                 }
//             }
//         };

//         context.Bookings.AddRange(bookings);
//         context.SaveChanges();

//         // ======================================================
//         // 7. FOOD ORDERS — Pipeline coverage:
//         //    Pending → Preparing → Delivered
//         //    Includes luxury items, multi-item orders, VVIP order
//         // ======================================================
//         var foodOrders = new List<FoodOrder>
//         {
//             // [0] Delivered — Isabelle, The Hollow (Booking 0)
//             //     Caviar + Wagyu + Champagne — a solitary tasting
//             new FoodOrder
//             {
//                 BookingId = bookings[0].Id,
//                 RoomId = bookings[0].BookingRooms.FirstOrDefault()!.RoomId,
//                 OrderStatus = FoodOrderStatus.Delivered,
//                 GeneratedAt = now.AddDays(-1).AddHours(-2),
//                 OrderItems = new List<FoodOrderItem>
//                 {
//                     new FoodOrderItem { MenuItemId = menu[0].Id, Quantity = 1, PriceAtPurchase = 180m },
//                     new FoodOrderItem { MenuItemId = menu[4].Id, Quantity = 1, PriceAtPurchase = 620m },
//                     new FoodOrderItem { MenuItemId = menu[9].Id, Quantity = 1, PriceAtPurchase = 950m }
//                 }
//             },

//             // [1] Preparing — Isabelle, second order this stay (Booking 0)
//             new FoodOrder
//             {
//                 BookingId = bookings[0].Id,
//                 RoomId = bookings[0].BookingRooms.FirstOrDefault()!.RoomId,
//                 OrderStatus = FoodOrderStatus.Preparing,
//                 GeneratedAt = now.AddMinutes(-40),
//                 OrderItems = new List<FoodOrderItem>
//                 {
//                     new FoodOrderItem { MenuItemId = menu[10].Id, Quantity = 1, PriceAtPurchase = 120m },
//                     new FoodOrderItem { MenuItemId = menu[7].Id, Quantity = 1, PriceAtPurchase = 190m }
//                 }
//             },

//             // [2] Pending — Haruto, Obsidian Chamber (Booking 1)
//             new FoodOrder
//             {
//                 BookingId = bookings[1].Id,
//                 RoomId = bookings[1].BookingRooms.FirstOrDefault()!.RoomId,
//                 OrderStatus = FoodOrderStatus.Pending,
//                 GeneratedAt = now.AddMinutes(-8),
//                 OrderItems = new List<FoodOrderItem>
//                 {
//                     new FoodOrderItem { MenuItemId = menu[3].Id, Quantity = 2, PriceAtPurchase = 150m },
//                     new FoodOrderItem { MenuItemId = menu[5].Id, Quantity = 1, PriceAtPurchase = 540m }
//                 }
//             },

//             // [3] Delivered — Haruto, earlier order (Booking 1)
//             new FoodOrder
//             {
//                 BookingId = bookings[1].Id,
//                 RoomId = bookings[1].BookingRooms.FirstOrDefault()!.RoomId,
//                 OrderStatus = FoodOrderStatus.Delivered,
//                 GeneratedAt = now.AddDays(-1).AddHours(-5),
//                 OrderItems = new List<FoodOrderItem>
//                 {
//                     new FoodOrderItem { MenuItemId = menu[1].Id, Quantity = 2, PriceAtPurchase = 240m },
//                     new FoodOrderItem { MenuItemId = menu[11].Id, Quantity = 2, PriceAtPurchase = 480m }
//                 }
//             },

//             // [4] Delivered — Priya, long-stay (Booking 12)
//             new FoodOrder
//             {
//                 BookingId = bookings[12].Id,
//                 RoomId = bookings[12].BookingRooms.FirstOrDefault()!.RoomId,
//                 OrderStatus = FoodOrderStatus.Delivered,
//                 GeneratedAt = now.AddDays(-6),
//                 OrderItems = new List<FoodOrderItem>
//                 {
//                     new FoodOrderItem { MenuItemId = menu[12].Id, Quantity = 2, PriceAtPurchase = 1200m }
//                 }
//             },

//             // [5] Pending — Lena (Today Arrival, Booking 10)
//             new FoodOrder
//             {
//                 BookingId = bookings[10].Id,
//                 RoomId = bookings[10].BookingRooms.FirstOrDefault()!.RoomId,
//                 OrderStatus = FoodOrderStatus.Pending,
//                 GeneratedAt = now.AddMinutes(-12),
//                 OrderItems = new List<FoodOrderItem>
//                 {
//                     new FoodOrderItem { MenuItemId = menu[0].Id, Quantity = 1, PriceAtPurchase = 180m },
//                     new FoodOrderItem { MenuItemId = menu[10].Id, Quantity = 1, PriceAtPurchase = 120m }
//                 }
//             },

//             // [6] Preparing — Marcus (Departing Today, Booking 11) — final breakfast
//             new FoodOrder
//             {
//                 BookingId = bookings[11].Id,
//                 RoomId = bookings[11].BookingRooms.FirstOrDefault()!.RoomId,
//                 OrderStatus = FoodOrderStatus.Preparing,
//                 GeneratedAt = now.AddMinutes(-25),
//                 OrderItems = new List<FoodOrderItem>
//                 {
//                     new FoodOrderItem { MenuItemId = menu[8].Id, Quantity = 1, PriceAtPurchase = 170m },
//                     new FoodOrderItem { MenuItemId = menu[10].Id, Quantity = 1, PriceAtPurchase = 120m }
//                 }
//             },

//             // [7] Delivered — Marcus, prior dinner (Booking 11)
//             new FoodOrder
//             {
//                 BookingId = bookings[11].Id,
//                 RoomId = bookings[11].BookingRooms.FirstOrDefault()!.RoomId,
//                 OrderStatus = FoodOrderStatus.Delivered,
//                 GeneratedAt = now.AddDays(-2),
//                 OrderItems = new List<FoodOrderItem>
//                 {
//                     new FoodOrderItem { MenuItemId = menu[2].Id, Quantity = 2, PriceAtPurchase = 260m },
//                     new FoodOrderItem { MenuItemId = menu[9].Id, Quantity = 1, PriceAtPurchase = 950m }
//                 }
//             },

//             // [8] VVIP Sanctum — Aleksei's first night feast (Booking 16)
//             //     The Stillness Tasting Menu × 6, Reserve Champagne × 6, Single Malt × 2
//             new FoodOrder
//             {
//                 BookingId = bookings[16].Id,
//                 RoomId = bookings[16].BookingRooms.FirstOrDefault()!.RoomId,
//                 OrderStatus = FoodOrderStatus.Delivered,
//                 GeneratedAt = now.AddDays(-1).AddHours(-6),
//                 OrderItems = new List<FoodOrderItem>
//                 {
//                     new FoodOrderItem { MenuItemId = menu[12].Id, Quantity = 6, PriceAtPurchase = 1200m },
//                     new FoodOrderItem { MenuItemId = menu[9].Id,  Quantity = 6, PriceAtPurchase = 950m  },
//                     new FoodOrderItem { MenuItemId = menu[11].Id, Quantity = 2, PriceAtPurchase = 480m  }
//                 }
//             },

//             // [9] VVIP Sanctum — morning order pending (Booking 16)
//             new FoodOrder
//             {
//                 BookingId = bookings[16].Id,
//                 RoomId = bookings[16].BookingRooms.FirstOrDefault()!.RoomId,
//                 OrderStatus = FoodOrderStatus.Pending,
//                 GeneratedAt = now.AddMinutes(-5),
//                 OrderItems = new List<FoodOrderItem>
//                 {
//                     new FoodOrderItem { MenuItemId = menu[0].Id,  Quantity = 6, PriceAtPurchase = 180m },
//                     new FoodOrderItem { MenuItemId = menu[10].Id, Quantity = 6, PriceAtPurchase = 120m }
//                 }
//             },

//             // [10] Nadine — family villa order, multi-item (Booking 17)
//             new FoodOrder
//             {
//                 BookingId = bookings[17].Id,
//                 RoomId = bookings[17].BookingRooms.FirstOrDefault()!.RoomId,
//                 OrderStatus = FoodOrderStatus.Delivered,
//                 GeneratedAt = now.AddDays(-1),
//                 OrderItems = new List<FoodOrderItem>
//                 {
//                     new FoodOrderItem { MenuItemId = menu[4].Id, Quantity = 2, PriceAtPurchase = 620m },
//                     new FoodOrderItem { MenuItemId = menu[5].Id, Quantity = 2, PriceAtPurchase = 540m },
//                     new FoodOrderItem { MenuItemId = menu[7].Id, Quantity = 4, PriceAtPurchase = 190m },
//                     new FoodOrderItem { MenuItemId = menu[9].Id, Quantity = 2, PriceAtPurchase = 950m }
//                 }
//             }
//         };

//         context.FoodOrders.AddRange(foodOrders);
//         context.SaveChanges();

//         // ======================================================
//         // 8. BOOKING AMENITIES
//         // ======================================================
//         context.BookingAmenities.AddRange(
//             // Isabelle (Booking 0) — Spa, Sound Bath, Wine Cellar
//             new BookingAmenity { BookingId = bookings[0].Id, AmenityId = amenities[5].Id, PriceAtPurchase = 900m },
//             new BookingAmenity { BookingId = bookings[0].Id, AmenityId = amenities[10].Id, PriceAtPurchase = 400m },
//             new BookingAmenity { BookingId = bookings[0].Id, AmenityId = amenities[7].Id, PriceAtPurchase = 300m },

//             // Haruto (Booking 1) — Butler, Wine Cellar, Art Curator
//             new BookingAmenity { BookingId = bookings[1].Id, AmenityId = amenities[1].Id, PriceAtPurchase = 1800m },
//             new BookingAmenity { BookingId = bookings[1].Id, AmenityId = amenities[6].Id, PriceAtPurchase = 1200m },
//             new BookingAmenity { BookingId = bookings[1].Id, AmenityId = amenities[13].Id, PriceAtPurchase = 950m },

//             // Isabelle 2nd stay (Booking 8) — Floatation, Stargazing
//             new BookingAmenity { BookingId = bookings[8].Id, AmenityId = amenities[4].Id, PriceAtPurchase = 650m },
//             new BookingAmenity { BookingId = bookings[8].Id, AmenityId = amenities[8].Id, PriceAtPurchase = 550m },

//             // Haruto Ashwood (Booking 9) — Butler, Private Chef, Yacht
//             new BookingAmenity { BookingId = bookings[9].Id, AmenityId = amenities[1].Id, PriceAtPurchase = 1800m },
//             new BookingAmenity { BookingId = bookings[9].Id, AmenityId = amenities[0].Id, PriceAtPurchase = 2500m },
//             new BookingAmenity { BookingId = bookings[9].Id, AmenityId = amenities[3].Id, PriceAtPurchase = 12000m },

//             // VVIP Sanctum — Aleksei (Booking 16) — the full retinue
//             new BookingAmenity { BookingId = bookings[16].Id, AmenityId = amenities[2].Id, PriceAtPurchase = 8500m  },  // Helicopter
//             new BookingAmenity { BookingId = bookings[16].Id, AmenityId = amenities[11].Id, PriceAtPurchase = 3200m },  // Security Detail
//             new BookingAmenity { BookingId = bookings[16].Id, AmenityId = amenities[0].Id, PriceAtPurchase = 2500m  },  // Private Chef
//             new BookingAmenity { BookingId = bookings[16].Id, AmenityId = amenities[1].Id, PriceAtPurchase = 1800m  },  // Butler
//             new BookingAmenity { BookingId = bookings[16].Id, AmenityId = amenities[3].Id, PriceAtPurchase = 12000m },  // Yacht
//             new BookingAmenity { BookingId = bookings[16].Id, AmenityId = amenities[9].Id, PriceAtPurchase = 1600m  },  // Cinema
//             new BookingAmenity { BookingId = bookings[16].Id, AmenityId = amenities[12].Id, PriceAtPurchase = 750m  },  // Falconry

//             // Nadine family villa (Booking 17) — Spa, Stargazing, Cinema
//             new BookingAmenity { BookingId = bookings[17].Id, AmenityId = amenities[5].Id, PriceAtPurchase = 900m  },
//             new BookingAmenity { BookingId = bookings[17].Id, AmenityId = amenities[8].Id, PriceAtPurchase = 550m  },
//             new BookingAmenity { BookingId = bookings[17].Id, AmenityId = amenities[9].Id, PriceAtPurchase = 1600m },

//             // Priya long-stay (Booking 12) — Digital Detox, Floatation, Sound Bath
//             new BookingAmenity { BookingId = bookings[12].Id, AmenityId = amenities[7].Id, PriceAtPurchase = 300m  },
//             new BookingAmenity { BookingId = bookings[12].Id, AmenityId = amenities[4].Id, PriceAtPurchase = 650m  },
//             new BookingAmenity { BookingId = bookings[12].Id, AmenityId = amenities[10].Id, PriceAtPurchase = 400m },

//             // Constance checked-out (Booking 4) — Spa, Wine Cellar
//             new BookingAmenity { BookingId = bookings[4].Id, AmenityId = amenities[5].Id, PriceAtPurchase = 900m  },
//             new BookingAmenity { BookingId = bookings[4].Id, AmenityId = amenities[6].Id, PriceAtPurchase = 1200m }
//         );

//         // ======================================================
//         // 9. HOUSEKEEPING TASKS — Pending / InProgress / Completed
//         // ======================================================
//         context.HousekeepingTasks.AddRange(
//             // Completed — H01 after Conrad Hale checked out [booking 13]
//             new Housekeeping { RoomId = rooms[0].Id, OriginType = HousekeepingOriginType.CheckoutAutomated, Status = HousekeepingStatus.Completed, Description = "Full suite reset after extended stay", StartedAt = now.AddDays(-33).AddHours(10), FinishedAt = now.AddDays(-33).AddHours(12) },

//             // InProgress — O01 (Haruto checking in, turndown in progress)
//             new Housekeeping { RoomId = rooms[3].Id, OriginType = HousekeepingOriginType.GuestRequested, Status = HousekeepingStatus.InProgress, Description = "Evening turndown and obsidian bath preparation", StartedAt = now.AddMinutes(-30) },

//             // Pending — E01 (Priya, mid long-stay refresh)
//             new Housekeeping { RoomId = rooms[6].Id, OriginType = HousekeepingOriginType.GuestRequested, Status = HousekeepingStatus.Pending, Description = "Daily linen refresh and hearth restoke" },

//             // Pending — V02 (Isabelle 2nd stay, checkout prep)
//             new Housekeeping { RoomId = rooms[10].Id, OriginType = HousekeepingOriginType.CheckoutAutomated, Status = HousekeepingStatus.Pending, Description = "Full checkout reset — Vantage Loft" },

//             // Completed — E02 after Constance checked out
//             new Housekeeping { RoomId = rooms[7].Id, OriginType = HousekeepingOriginType.CheckoutAutomated, Status = HousekeepingStatus.Completed, Description = "Post-checkout suite deep clean", StartedAt = now.AddDays(-7).AddHours(11), FinishedAt = now.AddDays(-7).AddHours(14) },

//             // Completed — H02 after Émile checked out
//             new Housekeeping { RoomId = rooms[1].Id, OriginType = HousekeepingOriginType.CheckoutAutomated, Status = HousekeepingStatus.Completed, Description = "Post-checkout limewash inspection and reset", StartedAt = now.AddDays(-18).AddHours(10), FinishedAt = now.AddDays(-18).AddHours(11) },

//             // InProgress — H03 (Lena arrived today, pre-arrival prep)
//             new Housekeeping { RoomId = rooms[2].Id, OriginType = HousekeepingOriginType.GuestRequested, Status = HousekeepingStatus.InProgress, Description = "Pre-arrival personalization: dawn-shaft alignment check, room scent setting", StartedAt = now.AddHours(-1) },

//             // Pending — O02 (Marcus departing today — room needs immediate reset)
//             new Housekeeping { RoomId = rooms[4].Id, OriginType = HousekeepingOriginType.CheckoutAutomated, Status = HousekeepingStatus.Pending, Description = "Checkout reset — Obsidian Chamber, plunge pool drain and refill" },

//             // Pending — O02 duplicate (multiple same-room tasks scenario)
//             new Housekeeping { RoomId = rooms[4].Id, OriginType = HousekeepingOriginType.StaffRequested, Status = HousekeepingStatus.Pending, Description = "Stonework inspection after guest departure" },

//             // InProgress — AW01 (Haruto — mid-stay, dining prep)
//             new Housekeeping { RoomId = rooms[15].Id, OriginType = HousekeepingOriginType.GuestRequested, Status = HousekeepingStatus.InProgress, Description = "Private dining room layout for evening chef service", StartedAt = now.AddMinutes(-45) },

//             // Pending — SX01/SX02 (Aleksei estate — daily grounds sweep)
//             new Housekeeping { RoomId = rooms[21].Id, OriginType = HousekeepingOriginType.StaffRequested, Status = HousekeepingStatus.Pending, Description = "Sanctum perimeter grounds inspection and morning preparation" },
//             new Housekeeping { RoomId = rooms[22].Id, OriginType = HousekeepingOriginType.GuestRequested, Status = HousekeepingStatus.Pending, Description = "Guest suite refresh — south wing" },

//             // Offline room — StaffRequested while renovation ongoing
//             new Housekeeping { RoomId = rooms[5].Id, OriginType = HousekeepingOriginType.StaffRequested, Status = HousekeepingStatus.Pending, Description = "Post-sealing inspection before O03 returns to active inventory", Location = "Obsidian Chamber O03 — Renovation Wing" },

//             // No-room task (location only)
//             new Housekeeping { RoomId = null, OriginType = HousekeepingOriginType.StaffRequested, Status = HousekeepingStatus.Pending, Description = "Observatory telescope lens cleaning — pre-Stargazing session tonight", Location = "North Observatory Dome" }
//         );

//         // ======================================================
//         // 10. MAINTENANCE TASKS — Mix of origins and statuses
//         // ======================================================
//         context.MaintenanceTasks.AddRange(
//             // Completed — O03 volcanic stone re-sealing (reason room is offline)
//             new MaintenanceTask { RoomId = rooms[5].Id, OriginType = MaintenanceOriginType.StaffRequested, Status = MaintenanceStatus.InProgress, Description = "Volcanic obsidian stone panel re-sealing — specialist contractor on-site", StartedAt = now.AddDays(-3) },

//             // Completed — SW03 courtyard waterproofing (reason SW03 offline)
//             new MaintenanceTask { RoomId = rooms[14].Id, OriginType = MaintenanceOriginType.StaffRequested, Status = MaintenanceStatus.InProgress, Description = "Stillwater courtyard basin waterproof lining replacement", StartedAt = now.AddDays(-5) },

//             // Pending painting follow-up on SW03
//             new MaintenanceTask { RoomId = rooms[14].Id, OriginType = MaintenanceOriginType.StaffRequested, Status = MaintenanceStatus.Pending, Description = "Courtyard stone refinishing after waterproof lining cures (blocked by lining task)" },

//             // Completed — H01 air circulation (filter)
//             new MaintenanceTask { RoomId = rooms[0].Id, OriginType = MaintenanceOriginType.SystemAutomated, Status = MaintenanceStatus.Completed, Description = "HEPA filter replacement — Hollow room climate system", StartedAt = now.AddDays(-10), FinishedAt = now.AddDays(-10).AddHours(1) },

//             // InProgress — E01 hearth (Priya's suite)
//             new MaintenanceTask { RoomId = rooms[6].Id, OriginType = MaintenanceOriginType.GuestRequested, Status = MaintenanceStatus.InProgress, Description = "Ember Suite hearth glass seal inspection — minor smoke trace reported", StartedAt = now.AddHours(-3) },

//             // Completed — AW01 timber exterior joint
//             new MaintenanceTask { RoomId = rooms[15].Id, OriginType = MaintenanceOriginType.StaffRequested, Status = MaintenanceStatus.Completed, Description = "Ashwood exterior timber joint re-caulking (annual)", StartedAt = now.AddDays(-8), FinishedAt = now.AddDays(-8).AddHours(4) },

//             // Pending — V01 steel stilt inspection (quarterly)
//             new MaintenanceTask { RoomId = rooms[9].Id, OriginType = MaintenanceOriginType.SystemAutomated, Status = MaintenanceStatus.Pending, Description = "Quarterly structural inspection of Vantage Loft blackened steel stilts" },

//             // Pending — MP01 terrace drain
//             new MaintenanceTask { RoomId = rooms[18].Id, OriginType = MaintenanceOriginType.GuestRequested, Status = MaintenanceStatus.Pending, Description = "Monolith terrace drain partially blocked — pre-arrival clearance required" },

//             // Completed — SX01 perimeter lighting
//             new MaintenanceTask { RoomId = rooms[21].Id, OriginType = MaintenanceOriginType.StaffRequested, Status = MaintenanceStatus.Completed, Description = "Sanctum perimeter low-profile lighting circuit check pre-VVIP arrival", StartedAt = now.AddDays(-2), FinishedAt = now.AddDays(-2).AddHours(2) },

//             // Pending — SX03 standby suite prep (unoccupied)
//             new MaintenanceTask { RoomId = rooms[23].Id, OriginType = MaintenanceOriginType.SystemAutomated, Status = MaintenanceStatus.Pending, Description = "Standby suite SX03 — annual mechanical systems test" },

//             // No-room task
//             new MaintenanceTask { RoomId = null, OriginType = MaintenanceOriginType.StaffRequested, Status = MaintenanceStatus.Pending, Description = "Private dock mooring cleats replacement before yacht charter season", Location = "Private Dock — South Perimeter" }
//         );

//         // ======================================================
//         // 11. FEEDBACK — Ratings 1–5, brand-voice comments,
//         //     one hidden, one empty comment, all edge cases
//         // ======================================================
//         context.Feedbacks.AddRange(
//             // Booking 4 — Constance, checked out, Ember Suite
//             new Feedback { BookingId = bookings[4].Id, Rating = 5, Comments = "I have stayed in the finest properties on four continents. The Ember Suite erased all of them from memory. The hearth burns without sound. The silence is total. I return.", CreatedAt = now.AddDays(-7), IsHidden = false },

//             // Booking 5 — Émile, checked out, The Hollow
//             new Feedback { BookingId = bookings[5].Id, Rating = 5, Comments = "The Hollow is not a room. It is a state of being. I arrived exhausted. I left restored. The absence of everything is, in fact, everything.", CreatedAt = now.AddDays(-18), IsHidden = false },

//             // Booking 13 — Conrad Hale, checked out UNPAID — scathing
//             new Feedback { BookingId = bookings[13].Id, Rating = 1, Comments = "Overpriced. The so-called 'solitude' is just a fancy word for having no amenities. The staff asked me to settle my bill twice. I refuse to be treated like a debtor.", CreatedAt = now.AddDays(-33), IsHidden = true },

//             // Booking 0 — Isabelle, currently in-stay (left interim feedback)
//             new Feedback { BookingId = bookings[0].Id, Rating = 5, Comments = "The light at dawn through the single aperture. I have been awake for it twice now. I did not come here for it. I will not be able to leave without it.", CreatedAt = now.AddDays(-1), IsHidden = false },

//             // Booking 1 — Haruto, currently in-stay (interim)
//             new Feedback { BookingId = bookings[1].Id, Rating = 4, Comments = "The obsidian bath is extraordinary. The plunge pool needs three more degrees in winter. Otherwise, perfection.", CreatedAt = now.AddHours(-12), IsHidden = false },

//             // Booking 8 — Isabelle 2nd stay, Vantage Loft
//             new Feedback { BookingId = bookings[8].Id, Rating = 5, Comments = "I booked the Vantage on a whim. It has the clearest horizon I have seen from indoors. The valley is not scenic. It is absolute.", CreatedAt = now.AddDays(-2), IsHidden = false },

//             // Booking 9 — Haruto Ashwood extended
//             new Feedback { BookingId = bookings[9].Id, Rating = 5, Comments = "The Ashwood Residence accommodated our entire delegation without once feeling crowded. Chef Nakamura's in-suite dinner was an event, not a meal.", CreatedAt = now.AddDays(-2), IsHidden = false },

//             // Booking 12 — Priya long-stay, no comment (edge: empty string)
//             new Feedback { BookingId = bookings[12].Id, Rating = 4, Comments = "", CreatedAt = now.AddDays(-10), IsHidden = false },

//             // Booking 16 — Aleksei VVIP (interim rating mid-stay)
//             new Feedback { BookingId = bookings[16].Id, Rating = 5, Comments = "The Sanctum is the only property I have occupied where the staff are present without being visible. The perimeter is genuine. The silence is guarded. This is what I require.", CreatedAt = now.AddHours(-18), IsHidden = false },

//             // Booking 17 — Nadine family villa
//             new Feedback { BookingId = bookings[17].Id, Rating = 5, Comments = "Two villas sharing a courtyard of still water. My children did not fight once. The stillwater does something to the atmosphere I cannot name scientifically.", CreatedAt = now.AddDays(-1), IsHidden = false },

//             // Booking 7 — No-show, left automated feedback at 3 stars (edge)
//             new Feedback { BookingId = bookings[7].Id, Rating = 3, Comments = "Could not make the dates due to a change in schedule. Aetheris declined to waive the late cancellation. Three stars for the process, not the property.", CreatedAt = now.AddDays(-5), IsHidden = false }
//         );

//         // ======================================================
//         // 12. RECEIPTS — Covers paid, refunded, partial, VVIP
//         // ======================================================
//         context.Receipts.AddRange(
//             // Booking 4 — Constance checked out (5 nights Ember @5800 + Spa + Wine)
//             new Receipt { BookingId = bookings[4].Id, AmountPaid = 31100m, PaymentMethod = "Private Bank Transfer", TransactionId = "AE-TXN-0001", PaidAt = now.AddDays(-7) },

//             // Booking 5 — Émile checked out (4 nights Hollow @3500)
//             new Receipt { BookingId = bookings[5].Id, AmountPaid = 14000m, PaymentMethod = "Amex Centurion", TransactionId = "AE-TXN-0002", PaidAt = now.AddDays(-18) },

//             // Booking 2 — Aleksei future, pre-paid deposit (5 nights Ember @5800)
//             new Receipt { BookingId = bookings[2].Id, AmountPaid = 29000m, PaymentMethod = "Wire Transfer", TransactionId = "AE-TXN-0003", PaidAt = now.AddDays(-18) },

//             // Booking 8 — Isabelle 2nd stay, Vantage, pre-paid
//             new Receipt { BookingId = bookings[8].Id, AmountPaid = 28800m, PaymentMethod = "Private Bank Transfer", TransactionId = "AE-TXN-0004", PaidAt = now.AddDays(-60) },

//             // Booking 9 — Haruto Ashwood, paid on check-in
//             new Receipt { BookingId = bookings[9].Id, AmountPaid = 84000m, PaymentMethod = "Wire Transfer", TransactionId = "AE-TXN-0005", PaidAt = now.AddDays(-4) },

//             // Booking 0 — Isabelle, partial deposit
//             new Receipt { BookingId = bookings[0].Id, AmountPaid = 10500m, PaymentMethod = "Amex Centurion", TransactionId = "AE-TXN-0006", PaidAt = now.AddDays(-14) },

//             // Booking 1 — Haruto Obsidian, partial deposit
//             new Receipt { BookingId = bookings[1].Id, AmountPaid = 8400m, PaymentMethod = "Wire Transfer", TransactionId = "AE-TXN-0007", PaidAt = now.AddDays(-30) },

//             // Booking 14 — Antoinette cancelled, awaiting refund (paid upfront)
//             new Receipt { BookingId = bookings[14].Id, AmountPaid = 66000m, PaymentMethod = "Wire Transfer", TransactionId = "AE-TXN-0008", PaidAt = now.AddDays(-50) },

//             // Booking 15 — Dmitri refunded (negative amount = refund transaction)
//             new Receipt { BookingId = bookings[15].Id, AmountPaid = -66000m, PaymentMethod = "Wire Transfer", TransactionId = "AE-REF-0001", PaidAt = now.AddDays(-40) },

//             // Booking 16 — Aleksei VVIP Sanctum, full estate settlement
//             //   2 rooms × 6 nights × 30000 + helicopter + security + chef + butler + yacht + cinema + falconry
//             new Receipt { BookingId = bookings[16].Id, AmountPaid = 390350m, PaymentMethod = "Private Bank Transfer", TransactionId = "AE-TXN-VIP-001", PaidAt = now.AddDays(-120) },

//             // Booking 18 — Constance future, pre-paid (5 nights Monolith @22000)
//             new Receipt { BookingId = bookings[18].Id, AmountPaid = 110000m, PaymentMethod = "Amex Centurion", TransactionId = "AE-TXN-0009", PaidAt = now.AddDays(-10) },

//             // Booking 12 — Priya long-stay, partial settlement
//             new Receipt { BookingId = bookings[12].Id, AmountPaid = 87000m, PaymentMethod = "Wire Transfer", TransactionId = "AE-TXN-0010", PaidAt = now.AddDays(-12) },

//             // Booking 17 — Nadine family villa, partial deposit
//             new Receipt { BookingId = bookings[17].Id, AmountPaid = 19000m, PaymentMethod = "Amex Centurion", TransactionId = "AE-TXN-0011", PaidAt = now.AddDays(-30) }
//         );

//         context.SaveChanges();
//     }
// }
