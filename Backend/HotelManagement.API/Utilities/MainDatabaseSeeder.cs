using HotelManagement.DAL.Context;
using HotelManagement.DAL.Entities;
using HotelManagement.DAL.Enums;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace HotelManagement.API.Utilities;

public static class MainDatabaseSeeder
{
  public static void Seed(IServiceProvider serviceProvider)
  {
    using var scope = serviceProvider.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (context.Users.Any()) return;

    var now = DateTime.UtcNow;

    // ======================================================
    // 1. USERS
    // ======================================================

    var admin = new User
    {
      Email = "admin@aetheris.com",
      PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1234"),
      Role = "Admin",
      FirstName = "Elara",
      LastName = "Voss",
      IsActive = true
    };

    var fd1 = new User
    {
      Email = "fd1@aetheris.com",
      PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1234"),
      Role = "FrontDesk",
      FirstName = "Margaux",
      LastName = "Lefevre",
      IsActive = true
    };

    var fd2 = new User
    {
      Email = "fd2@aetheris.com",
      PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1234"),
      Role = "FrontDesk",
      FirstName = "Caspian",
      LastName = "Reed",
      IsActive = true
    };

    var fd3 = new User
    {
      Email = "fd3@aetheris.com",
      PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1234"),
      Role = "FrontDesk",
      FirstName = "Silke",
      LastName = "Berg",
      IsActive = true
    };

    var kitchen = new User
    {
      Email = "kitchen@aetheris.com",
      PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1234"),
      Role = "Kitchen",
      FirstName = "Riku",
      LastName = "Nakamura",
      IsActive = true
    };

    var kitchenAsst = new User
    {
      Email = "kitchen2@aetheris.com",
      PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1234"),
      Role = "Kitchen",
      FirstName = "Petra",
      LastName = "Wolff",
      IsActive = true
    };

    var hk1 = new User
    {
      Email = "hk1@aetheris.com",
      PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1234"),
      Role = "Housekeeping",
      FirstName = "Daria",
      LastName = "Morel",
      IsActive = true
    };

    var hk2 = new User
    {
      Email = "hk2@aetheris.com",
      PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1234"),
      Role = "Housekeeping",
      FirstName = "Ivan",
      LastName = "Kuznetsov",
      IsActive = true
    };

    var maintenance = new User
    {
      Email = "maintenance@aetheris.com",
      PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1234"),
      Role = "Maintenance",
      FirstName = "Felix",
      LastName = "Schreiber",
      IsActive = true
    };

    var inactiveStaff = new User
    {
      Email = "inactive@aetheris.com",
      PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1234"),
      Role = "FrontDesk",
      FirstName = "Olivier",
      LastName = "Renaud",
      IsActive = false
    };

    var cust1 = new User
    {
      Email = "cust1@gmail.com",
      PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1234"),
      Role = "RegisteredUser",
      FirstName = "Isabelle",
      LastName = "Fontaine",
      IsActive = true
    };

    var cust2 = new User
    {
      Email = "cust2@gmail.com",
      PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1234"),
      Role = "RegisteredUser",
      FirstName = "Haruto",
      LastName = "Katsuragi",
      IsActive = true
    };

    var cust3 = new User
    {
      Email = "cust3@gmail.com",
      PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1234"),
      Role = "RegisteredUser",
      FirstName = "Aleksei",
      LastName = "Volkov",
      IsActive = true
    };

    var cust4 = new User
    {
      Email = "cust4@gmail.com",
      PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1234"),
      Role = "RegisteredUser",
      FirstName = "Nadine",
      LastName = "El-Amin",
      IsActive = true
    };

    var cust5 = new User
    {
      Email = "cust5@gmail.com",
      PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1234"),
      Role = "RegisteredUser",
      FirstName = "Constance",
      LastName = "Morrow",
      IsActive = true
    };

    var guestNoBookings = new User
    {
      Email = "prospective@gmail.com",
      PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1234"),
      Role = "RegisteredUser",
      FirstName = "Theo",
      LastName = "Sander",
      IsActive = true
    };

    var guestBanned = new User
    {
      Email = "banned@gmail.com",
      PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1234"),
      Role = "RegisteredUser",
      FirstName = "Banned",
      LastName = "Account",
      IsActive = false
    };

    var guestPending = new User
    {
      Email = "pending@gmail.com",
      PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@1234"),
      Role = "RegisteredUser",
      FirstName = "Awaiting",
      LastName = "Verification",
      IsActive = true
    };

    context.Users.AddRange(
        admin, fd1, fd2, fd3,
        kitchen, kitchenAsst,
        hk1, hk2,
        maintenance, inactiveStaff,
        cust1, cust2, cust3, cust4, cust5,
        guestNoBookings, guestBanned, guestPending
    );
    context.SaveChanges();

    // ======================================================
    // 2. ROOM TYPES
    // ======================================================

    var hollow = new RoomType
    {
      Name = "The Hollow",
      BasePrice = 3500m,
      MaxOccupancy = 1,
      Description = "A single, windowless-feeling cocoon carved for those who wish to disappear entirely — even from their own itinerary. Charcoal limewash walls absorb sound; a single shaft of light falls across the room at dawn and vanishes by ten. There is no minibar, no television, no clock. Aetheris removes what most resorts add, so that solitude has nothing left to compete with.",
      ImageUrls = new List<string>
            {
                "https://images.unsplash.com/photo-1744310825781-99d47a5b19d4?q=80&w=1287&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                "https://images.unsplash.com/photo-1682787283049-d37de81646b4?q=80&w=2574&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                "https://plus.unsplash.com/premium_photo-1722593856418-05d6d47eec59?q=80&w=3270&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
            },
      SquareFootage = 320,
      BedConfigurationJson = JsonSerializer.Serialize(new Dictionary<string, int> { { "King", 1 } }),
      IsActive = true
    };

    var obsidian = new RoomType
    {
      Name = "The Obsidian Chamber",
      BasePrice = 4200m,
      MaxOccupancy = 2,
      Description = "Named for the volcanic stone that lines its private bath, the Obsidian Chamber sits half-sunken into the hillside so that no window meets another window. A recessed plunge pool of black granite mirrors the night sky. Guests are given no reason to look outward — only inward.",
      ImageUrls = new List<string>
            {
                "https://images.unsplash.com/photo-1640357960494-9242650846d3?q=80&w=2543&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                "https://images.unsplash.com/photo-1680946496238-5272d3c407fc?q=80&w=3271&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                "https://images.unsplash.com/photo-1644057501622-dfa7dd26dbfb?q=80&w=2581&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
            },
      SquareFootage = 480,
      BedConfigurationJson = JsonSerializer.Serialize(new Dictionary<string, int> { { "King", 1 } }),
      IsActive = true
    };

    var ember = new RoomType
    {
      Name = "The Ember Suite",
      BasePrice = 5800m,
      MaxOccupancy = 2,
      Description = "A private hearth burns without smoke or sound behind reinforced glass, its glow the only source of light after dusk. The Ember Suite is reserved for guests who wish to be warmed, not entertained — a room built entirely around the absence of urgency.",
      ImageUrls = new List<string>
            {
                "https://plus.unsplash.com/premium_photo-1736194029299-d30668ff6d94?q=80&w=1287&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                "https://plus.unsplash.com/premium_photo-1683917068755-c2890e4824e1?q=80&w=3270&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                "https://plus.unsplash.com/premium_photo-1684508638760-72ad80c0055f?q=80&w=3271&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
            },
      SquareFootage = 620,
      BedConfigurationJson = JsonSerializer.Serialize(new Dictionary<string, int> { { "King", 1 }, { "Daybed", 1 } }),
      IsActive = true
    };

    var vantage = new RoomType
    {
      Name = "The Vantage Loft",
      BasePrice = 7200m,
      MaxOccupancy = 2,
      Description = "Elevated above the treeline on stilts of blackened steel, the Vantage Loft offers a single, uninterrupted view of the valley below and nothing else — no neighboring structure has ever entered its sightline, and none ever will.",
      ImageUrls = new List<string>
            {
                "https://images.unsplash.com/photo-1504896400264-d7f7eee9da97?q=80&w=3270&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                "https://images.unsplash.com/photo-1763935323738-5694f39b0cc8?q=80&w=3270&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                "https://images.unsplash.com/photo-1658928482061-d99bf2b79fcb?q=80&w=1470&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
            },
      SquareFootage = 750,
      BedConfigurationJson = JsonSerializer.Serialize(new Dictionary<string, int> { { "King", 1 } }),
      IsActive = true
    };

    var stillwater = new RoomType
    {
      Name = "The Stillwater Villa",
      BasePrice = 9500m,
      MaxOccupancy = 4,
      Description = "A private villa built around a courtyard of unmoving black water, so still it is often mistaken for stone. Two bedrooms open onto the courtyard independently, allowing guests to share proximity without ever sharing presence — togetherness on one's own terms.",
      ImageUrls = new List<string>
            {
                "https://images.unsplash.com/photo-1761240960690-4d2cd3c93911?q=80&w=1287&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                "https://images.unsplash.com/photo-1690751099556-e47ce07ee71f?q=80&w=3270&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                "https://images.unsplash.com/photo-1565357545450-69b6af3e7288?q=80&w=3270&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
            },
      SquareFootage = 1400,
      BedConfigurationJson = JsonSerializer.Serialize(new Dictionary<string, int> { { "King", 2 } }),
      IsActive = true
    };

    var ashwood = new RoomType
    {
      Name = "The Ashwood Residence",
      BasePrice = 14000m,
      MaxOccupancy = 6,
      Description = "A three-bedroom residence of ash-grey timber and stone, designed for families or retinues who require space between them as much as shelter above them. A private staff entrance ensures the household is served without ever being seen.",
      ImageUrls = new List<string>
            {
                "https://plus.unsplash.com/premium_photo-1673014202078-2ab12abbb43d?q=80&w=2604&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                "https://images.unsplash.com/photo-1650429356490-f461d720fcc9?q=80&w=1470&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                "https://plus.unsplash.com/premium_photo-1721487064014-3c067e680049?q=80&w=3270&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                "https://images.unsplash.com/photo-1772703003096-fe4096559f81?q=80&w=3262&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
            },
      SquareFootage = 2600,
      BedConfigurationJson = JsonSerializer.Serialize(new Dictionary<string, int> { { "King", 3 } }),
      IsActive = true
    };

    var monolith = new RoomType
    {
      Name = "The Monolith Penthouse",
      BasePrice = 22000m,
      MaxOccupancy = 4,
      Description = "Occupying the retreat's highest point, the Monolith is a single slab of dark concrete and glass that appears, from any distance, to be uninhabited. Inside, every surface has been chosen to disappear at night, leaving only the guest and the horizon.",
      ImageUrls = new List<string>
            {
                "https://plus.unsplash.com/premium_photo-1733320822557-e4ccfb5f20d1?q=80&w=3271&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                "https://images.unsplash.com/photo-1589312290984-c1ae35fb0de8?q=80&w=3293&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                "https://images.unsplash.com/photo-1756679207077-b20c69378f8c?q=80&w=3270&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
            },
      SquareFootage = 3200,
      BedConfigurationJson = JsonSerializer.Serialize(new Dictionary<string, int> { { "King", 2 }, { "Queen", 1 } }),
      IsActive = true
    };

    var sanctum = new RoomType
    {
      Name = "The Sanctum",
      BasePrice = 30000m,
      MaxOccupancy = 8,
      Description = "A standalone estate at the farthest edge of the property, reachable only by private escort. The Sanctum is the only accommodation at Aetheris with its own perimeter, its own staff quarters, and its own silence — an entire world reserved for a single party.",
      ImageUrls = new List<string>
            {
                "https://images.unsplash.com/photo-1601701119495-d6e39b664001?q=80&w=1528&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                "https://images.unsplash.com/photo-1578815552961-cce5ebaa835b?q=80&w=3269&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                "https://images.unsplash.com/photo-1679428767634-b3f574467067?q=80&w=2574&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
            },
      SquareFootage = 6500,
      BedConfigurationJson = JsonSerializer.Serialize(new Dictionary<string, int> { { "King", 4 } }),
      IsActive = true
    };

    context.RoomTypes.AddRange(hollow, obsidian, ember, vantage, stillwater, ashwood, monolith, sanctum);
    context.SaveChanges();

    // ======================================================
    // 3. ROOMS
    //    H=Hollow, O=Obsidian, E=Ember, V=Vantage,
    //    SW=Stillwater, AW=Ashwood, MP=Monolith, SX=Sanctum
    // ======================================================
    var rooms = new List<Room>
        {
            new Room { RoomNumber = "H01", RoomTypeId = hollow.Id, IsActive = true },
            new Room { RoomNumber = "H02", RoomTypeId = hollow.Id, IsActive = true },
            new Room { RoomNumber = "H03", RoomTypeId = hollow.Id, IsActive = true },
            new Room { RoomNumber = "O01", RoomTypeId = obsidian.Id, IsActive = true },
            new Room { RoomNumber = "O02", RoomTypeId = obsidian.Id, IsActive = true },
            new Room { RoomNumber = "O03", RoomTypeId = obsidian.Id, IsActive = false },
            new Room { RoomNumber = "E01", RoomTypeId = ember.Id, IsActive = true },
            new Room { RoomNumber = "E02", RoomTypeId = ember.Id, IsActive = true },
            new Room { RoomNumber = "E03", RoomTypeId = ember.Id, IsActive = true },
            new Room { RoomNumber = "V01", RoomTypeId = vantage.Id, IsActive = true },
            new Room { RoomNumber = "V02", RoomTypeId = vantage.Id, IsActive = true },
            new Room { RoomNumber = "V03", RoomTypeId = vantage.Id, IsActive = true },
            new Room { RoomNumber = "SW01", RoomTypeId = stillwater.Id, IsActive = true },
            new Room { RoomNumber = "SW02", RoomTypeId = stillwater.Id, IsActive = true },
            new Room { RoomNumber = "SW03", RoomTypeId = stillwater.Id, IsActive = false },
            new Room { RoomNumber = "AW01", RoomTypeId = ashwood.Id, IsActive = true },
            new Room { RoomNumber = "AW02", RoomTypeId = ashwood.Id, IsActive = true },
            new Room { RoomNumber = "AW03", RoomTypeId = ashwood.Id, IsActive = true },
            new Room { RoomNumber = "MP01", RoomTypeId = monolith.Id, IsActive = true },
            new Room { RoomNumber = "MP02", RoomTypeId = monolith.Id, IsActive = true },
            new Room { RoomNumber = "MP03", RoomTypeId = monolith.Id, IsActive = true },
            new Room { RoomNumber = "SX01", RoomTypeId = sanctum.Id, IsActive = true },
            new Room { RoomNumber = "SX02", RoomTypeId = sanctum.Id, IsActive = true },
            new Room { RoomNumber = "SX03", RoomTypeId = sanctum.Id, IsActive = true },
        };

    context.Rooms.AddRange(rooms);
    context.SaveChanges();

    // ======================================================
    // 4. MENU ITEMS
    // ======================================================
    var menu = new List<MenuItem>
        {
            new MenuItem { Name = "Midnight Caviar Spoon", Price = 180m, Category = "Amuse-Bouche", IsAvailable = true, Description = "A single spoon of Osetra caviar over potato blini and crème fraîche, finished with one shard of edible gold. Served alone, on a black slate, with nothing beside it.", ImageUrl = "https://picsum.photos/seed/aetheris-caviar-spoon/900/700" },
            new MenuItem { Name = "Charred Bone Marrow & Black Truffle", Price = 240m, Category = "Appetizer", IsAvailable = true, Description = "Roasted marrow bone split tableside, finished with shaved black truffle and a whisper of smoked sea salt. Eaten with nothing but a spoon and silence.", ImageUrl = "https://picsum.photos/seed/aetheris-bone-marrow/900/700" },
            new MenuItem { Name = "Hokkaido Scallop, Ash-Roasted", Price = 260m, Category = "Appetizer", IsAvailable = true, Description = "Diver scallop roasted in hearth ash, served in its own shell over a pool of brown butter and yuzu kosho.", ImageUrl = "https://picsum.photos/seed/aetheris-scallop-ash/900/700" },
            new MenuItem { Name = "Smoked Wild Mushroom Consommé", Price = 150m, Category = "Soup", IsAvailable = true, Description = "A clarified broth of foraged mushrooms, smoked over embers for six hours and poured tableside from a blackened kettle.", ImageUrl = "https://picsum.photos/seed/aetheris-consomme/900/700" },
            new MenuItem { Name = "Dry-Aged Wagyu, Ember-Seared", Price = 620m, Category = "Main Course", IsAvailable = true, Description = "A5 wagyu, dry-aged sixty days and seared directly over open ember, sliced tableside and served without garnish — the ingredient is the entire statement.", ImageUrl = "https://picsum.photos/seed/aetheris-wagyu-ember/900/700" },
            new MenuItem { Name = "Whole Roasted Turbot, Champagne Butter", Price = 540m, Category = "Main Course", IsAvailable = true, Description = "Whole turbot roasted on the bone and finished tableside with a champagne beurre blanc, deboned in front of the guest.", ImageUrl = "https://picsum.photos/seed/aetheris-turbot/900/700" },
            new MenuItem { Name = "Squab en Croûte with Foie Gras", Price = 580m, Category = "Main Course", IsAvailable = false, Description = "Roasted squab wrapped in foie gras and pastry, rested and carved in the room, paired with a reduction of aged port.", ImageUrl = "https://picsum.photos/seed/aetheris-squab-croute/900/700" },
            new MenuItem { Name = "Dark Chocolate & Gold Dust Sphere", Price = 190m, Category = "Dessert", IsAvailable = true, Description = "A sphere of 72% dark chocolate that dissolves at the table under a pour of warm salted caramel, dusted with edible gold.", ImageUrl = "https://picsum.photos/seed/aetheris-chocolate-sphere/900/700" },
            new MenuItem { Name = "Champagne Sabayon with Black Fig", Price = 170m, Category = "Dessert", IsAvailable = true, Description = "A warm, airy sabayon whisked with vintage champagne, served over roasted black figs and toasted hazelnut.", ImageUrl = "https://picsum.photos/seed/aetheris-sabayon-fig/900/700" },
            new MenuItem { Name = "Aetheris Reserve Champagne", Price = 950m, Category = "Beverage", IsAvailable = true, Description = "A single vintage bottle, cellared exclusively for Aetheris and released to no other property in the world.", ImageUrl = "https://picsum.photos/seed/aetheris-champagne/900/700" },
            new MenuItem { Name = "Single-Origin Kyoto Matcha Ceremony", Price = 120m, Category = "Beverage", IsAvailable = true, Description = "A formal, silent matcha preparation performed tableside using ceremonial-grade leaves from a single Kyoto estate.", ImageUrl = "https://picsum.photos/seed/aetheris-matcha/900/700" },
            new MenuItem { Name = "25-Year Single Malt, Neat", Price = 480m, Category = "Beverage", IsAvailable = true, Description = "A quarter-century single malt from a private cask, poured neat and left to breathe in silence before service.", ImageUrl = "https://picsum.photos/seed/aetheris-single-malt/900/700" },
            new MenuItem { Name = "The Stillness Tasting Menu", Price = 1200m, Category = "Tasting Menu", IsAvailable = true, Description = "A seven-course progression built around absence rather than abundance — each course simpler than the last, ending in a single unadorned course meant to be eaten in complete silence.", ImageUrl = "https://picsum.photos/seed/aetheris-stillness-menu/900/700" }
        };

    context.MenuItems.AddRange(menu);
    context.SaveChanges();

    // ======================================================
    // 5. AMENITIES
    // ======================================================
    var amenities = new List<Amenity>
        {
            new Amenity { Name = "Private Chef In-Suite Dining", Description = "A dedicated chef prepares and serves a personalized multi-course dinner entirely within your suite, so that even the dining room becomes unnecessary.", Price = 2500m, IsAvailable = true, ImageUrl = "https://picsum.photos/seed/aetheris-private-chef/900/700" },
            new Amenity { Name = "Personal Butler Service", Description = "A single, permanently assigned butler attends to every need for the duration of your stay, communicating only when spoken to. (24h)", Price = 1800m, IsAvailable = true, ImageUrl = "https://images.unsplash.com/photo-1544986581-efac024faf62?q=80&w=3270&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D" },
            new Amenity { Name = "Helicopter Transfer", Description = "Direct, private helicopter transfer between Aetheris and the nearest discreet airfield, bypassing all public arrival points.", Price = 8500m, IsAvailable = true, ImageUrl = "https://images.unsplash.com/photo-1559756037-4bb8f3d22863?q=80&w=2117&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D" },
            new Amenity { Name = "Private Yacht Charter (Half-Day)", Description = "A crewed private yacht reserved exclusively for your party, departing from Aetheris's unmarked private dock.", Price = 12000m, IsAvailable = true, ImageUrl = "https://picsum.photos/seed/aetheris-yacht/900/700" },
            new Amenity { Name = "Floatation Sensory Deprivation Therapy", Description = "A private session in a sealed floatation chamber, removing sound, light, and gravity to return the mind to true stillness.", Price = 650m, IsAvailable = false, ImageUrl = "https://picsum.photos/seed/aetheris-floatation/900/700" },
            new Amenity { Name = "In-Suite Spa and Massage", Description = "A full therapeutic massage delivered entirely within your suite by a private therapist, using oils blended for your stay alone.", Price = 900m, IsAvailable = true, ImageUrl = "https://plus.unsplash.com/premium_photo-1723514505301-682c69fc8edd?q=80&w=3270&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D" },
            new Amenity { Name = "Private Wine Cellar Tasting", Description = "An exclusive after-hours tasting inside Aetheris's subterranean cellar, guided by the resident sommelier for your party only.", Price = 1200m, IsAvailable = true, ImageUrl = "https://picsum.photos/seed/aetheris-wine-cellar/900/700" },
            new Amenity { Name = "Digital Detox Concierge", Description = "A dedicated concierge takes custody of all devices for the duration of your stay and curates an entirely offline itinerary in their place.", Price = 300m, IsAvailable = true, ImageUrl = "https://images.unsplash.com/photo-1654738762506-9370558654ce?q=80&w=2080&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D" },
            new Amenity { Name = "Stargazing & Telescope Session", Description = "A private astronomer guides a late-night observation session using Aetheris's observatory-grade telescope, far from any ambient light.", Price = 550m, IsAvailable = true, ImageUrl = "https://picsum.photos/seed/aetheris-stargazing/900/700" },
            new Amenity { Name = "Private Cinema Screening", Description = "The resort's single-screen cinema reserved entirely for your party, with any film sourced and screened on request.", Price = 1600m, IsAvailable = true, ImageUrl = "https://picsum.photos/seed/aetheris-cinema/900/700" },
            new Amenity { Name = "Meditation and Sound Bath Session", Description = "A private, guided meditation and sound bath performed with hand-forged singing bowls, held in the resort's stone chamber.", Price = 400m, IsAvailable = true, ImageUrl = "https://images.unsplash.com/photo-1597682886233-61b9023db181?q=80&w=3268&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D" },
            new Amenity { Name = "Executive Protection Detail", Description = "A discreet, professionally trained security detail assigned solely to your party, present but never visible.", Price = 3200m, IsAvailable = false, ImageUrl = "https://picsum.photos/seed/aetheris-security/900/700" },
            new Amenity { Name = "Falconry Experience", Description = "A private falconry session on the resort's private grounds, led by a resident falconer, held at dawn when the grounds are entirely empty.", Price = 750m, IsAvailable = true, ImageUrl = "https://images.unsplash.com/photo-1575574867579-44f92f224938?q=80&w=1724&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D" },
            new Amenity { Name = "Private Art Curator Tour", Description = "A one-on-one walkthrough of Aetheris's private art collection, guided by a resident curator, arranged exclusively around your schedule.", Price = 950m, IsAvailable = true, ImageUrl = "https://picsum.photos/seed/aetheris-art-curator/900/700" }
        };

    context.Amenities.AddRange(amenities);
    context.SaveChanges();

    // ======================================================
    // 6. BOOKINGS
    //
    //  [0]  cust1 — CheckedIn, Pending       (H01)
    //  [1]  cust2 — CheckedIn, Pending        (O01)
    //  [2]  cust3 — Future, Paid              (E01)
    //  [3]  cust4 — Future, Pending           (V01)
    //  [4]  cust5 — CheckedOut, Paid          (E02)
    //  [5]  Walk-in — CheckedOut, Paid        (H02)
    //  [6]  Cancelled, Pending                (SW01)
    //  [7]  Cancelled, NoShow                 (O02)
    //  [8]  cust1 (2nd) — CheckedIn, Paid    (V02)
    //  [9]  cust2 (2nd) — CheckedIn, Paid    (AW01)
    // [10]  Arriving today — CheckedIn       (H03)
    // [11]  Departing today — CheckedIn      (O02)
    // [12]  Long stay 30d — CheckedIn        (E03)
    // [13]  CheckedOut, Unpaid (runner)      (H01)
    // [14]  Cancelled, Paid (needs refund)   (MP01)
    // [15]  Cancelled, Refunded              (MP02)
    // [16]  VVIP Sanctum (cust3)             (SX01+SX02)
    // [17]  Multi-room family (cust4)        (SW01+SW02)
    // [18]  Far-future (cust5)               (Monolith)
    // [19]  Guest, booked, no room assigned
    // ======================================================
    var bookings = new List<Booking>
        {
            // [0] cust1 — CheckedIn, Pending, The Hollow
            new Booking
            {
                GuestName = "Isabelle Fontaine",
                GuestEmail = "cust1@gmail.com",
                UserId = cust1.Id,
                GuestCount = 1,
                CheckInDate = now.AddDays(-2),
                CheckOutDate = now.AddDays(3),
                BookingStatus = BookingStatus.CheckedIn,
                PaymentStatus = PaymentStatus.Pending,
                Origin = BookingOrigin.RegisteredUser,
                BookedAt = now.AddDays(-14),
                BookingRooms = new List<BookingRoom>
                {
                    new BookingRoom { RoomTypeId = hollow.Id, RoomId = rooms[0].Id, LockedInPrice = 3500m }
                }
            },

            // [1] cust2 — CheckedIn, Pending, The Obsidian Chamber
            new Booking
            {
                GuestName = "Haruto Katsuragi",
                GuestEmail = "cust2@gmail.com",
                UserId = cust2.Id,
                GuestCount = 2,
                CheckInDate = now.AddDays(-1),
                CheckOutDate = now.AddDays(4),
                BookingStatus = BookingStatus.CheckedIn,
                PaymentStatus = PaymentStatus.Pending,
                Origin = BookingOrigin.RegisteredUser,
                BookedAt = now.AddDays(-30),
                BookingRooms = new List<BookingRoom>
                {
                    new BookingRoom { RoomTypeId = obsidian.Id, RoomId = rooms[3].Id, LockedInPrice = 4200m }
                }
            },

            // [2] cust3 — future, paid, Ember Suite
            new Booking
            {
                GuestName = "Aleksei Volkov",
                GuestEmail = "cust3@gmail.com",
                UserId = cust3.Id,
                GuestCount = 2,
                CheckInDate = now.AddDays(8),
                CheckOutDate = now.AddDays(13),
                BookingStatus = BookingStatus.Booked,
                PaymentStatus = PaymentStatus.Paid,
                Origin = BookingOrigin.RegisteredUser,
                BookedAt = now.AddDays(-20),
                BookingRooms = new List<BookingRoom>
                {
                    new BookingRoom { RoomTypeId = ember.Id, RoomId = null, LockedInPrice = 5800m }
                }
            },

            // [3] cust4 — future, pending, Vantage Loft
            new Booking
            {
                GuestName = "Nadine El-Amin",
                GuestEmail = "cust4@gmail.com",
                UserId = cust4.Id,
                GuestCount = 2,
                CheckInDate = now.AddDays(15),
                CheckOutDate = now.AddDays(19),
                BookingStatus = BookingStatus.Booked,
                PaymentStatus = PaymentStatus.Pending,
                Origin = BookingOrigin.RegisteredUser,
                BookedAt = now.AddDays(-5),
                BookingRooms = new List<BookingRoom>
                {
                    new BookingRoom { RoomTypeId = vantage.Id, RoomId = null, LockedInPrice = 7200m }
                }
            },

            // [4] cust5 — checked out, paid, Ember Suite
            new Booking
            {
                GuestName = "Constance Morrow",
                GuestEmail = "cust5@gmail.com",
                UserId = cust5.Id,
                GuestCount = 1,
                CheckInDate = now.AddDays(-12),
                CheckOutDate = now.AddDays(-7),
                BookingStatus = BookingStatus.CheckedOut,
                PaymentStatus = PaymentStatus.Paid,
                Origin = BookingOrigin.RegisteredUser,
                BookedAt = now.AddDays(-40),
                BookingRooms = new List<BookingRoom>
                {
                    new BookingRoom { RoomTypeId = ember.Id, RoomId = rooms[7].Id, LockedInPrice = 5800m }
                }
            },

            // [5] Walk-in — checked out, paid, The Hollow
            new Booking
            {
                GuestName = "Emile Renard",
                GuestEmail = "emile.renard@gmail.com",
                GuestCount = 1,
                CheckInDate = now.AddDays(-22),
                CheckOutDate = now.AddDays(-18),
                BookingStatus = BookingStatus.CheckedOut,
                PaymentStatus = PaymentStatus.Paid,
                Origin = BookingOrigin.WalkIn,
                BookedAt = now.AddDays(-22),
                BookingRooms = new List<BookingRoom>
                {
                    new BookingRoom { RoomTypeId = hollow.Id, RoomId = rooms[1].Id, LockedInPrice = 3500m }
                }
            },

            // [6] Cancelled, Pending — Stillwater Villa
            new Booking
            {
                GuestName = "Saoirse Brennan",
                GuestEmail = "saoirse.brennan@gmail.com",
                GuestCount = 3,
                CheckInDate = now.AddDays(2),
                CheckOutDate = now.AddDays(5),
                BookingStatus = BookingStatus.Cancelled,
                PaymentStatus = PaymentStatus.Pending,
                Origin = BookingOrigin.Guest,
                BookedAt = now.AddDays(-8),
                BookingRooms = new List<BookingRoom>
                {
                    new BookingRoom { RoomTypeId = stillwater.Id, RoomId = null, LockedInPrice = 9500m }
                }
            },

            // [7] Cancelled, NoShow — Obsidian Chamber
            new Booking
            {
                GuestName = "Viktor Strauss",
                GuestEmail = "viktor.strauss@gmail.com",
                GuestCount = 2,
                CheckInDate = now.AddDays(-5),
                CheckOutDate = now.AddDays(-2),
                BookingStatus = BookingStatus.Cancelled,
                PaymentStatus = PaymentStatus.Pending,
                Origin = BookingOrigin.Guest,
                BookedAt = now.AddDays(-25),
                BookingRooms = new List<BookingRoom>
                {
                    new BookingRoom { RoomTypeId = obsidian.Id, RoomId = null, LockedInPrice = 4200m }
                }
            },

            // [8] cust1 second stay — CheckedIn, Paid, Vantage Loft
            new Booking
            {
                GuestName = "Isabelle Fontaine",
                GuestEmail = "cust1@gmail.com",
                UserId = cust1.Id,
                GuestCount = 2,
                CheckInDate = now.AddDays(-3),
                CheckOutDate = now.AddDays(1),
                BookingStatus = BookingStatus.CheckedIn,
                PaymentStatus = PaymentStatus.Paid,
                Origin = BookingOrigin.RegisteredUser,
                BookedAt = now.AddDays(-60),
                BookingRooms = new List<BookingRoom>
                {
                    new BookingRoom { RoomTypeId = vantage.Id, RoomId = rooms[10].Id, LockedInPrice = 7200m }
                }
            },

            // [9] cust2 extended — CheckedIn, Paid, Ashwood Residence
            new Booking
            {
                GuestName = "Haruto Katsuragi",
                GuestEmail = "cust2@gmail.com",
                UserId = cust2.Id,
                GuestCount = 4,
                CheckInDate = now.AddDays(-4),
                CheckOutDate = now.AddDays(2),
                BookingStatus = BookingStatus.CheckedIn,
                PaymentStatus = PaymentStatus.Paid,
                Origin = BookingOrigin.RegisteredUser,
                BookedAt = now.AddDays(-45),
                BookingRooms = new List<BookingRoom>
                {
                    new BookingRoom { RoomTypeId = ashwood.Id, RoomId = rooms[15].Id, LockedInPrice = 14000m }
                }
            },

            // [10] Arriving today — H03
            new Booking
            {
                GuestName = "Lena Bergstrom",
                GuestEmail = "lena.bergstrom@gmail.com",
                GuestCount = 1,
                CheckInDate = now.Date,
                CheckOutDate = now.Date.AddDays(4),
                BookingStatus = BookingStatus.CheckedIn,
                PaymentStatus = PaymentStatus.Pending,
                Origin = BookingOrigin.Guest,
                BookedAt = now.AddDays(-7),
                BookingRooms = new List<BookingRoom>
                {
                    new BookingRoom { RoomTypeId = hollow.Id, RoomId = rooms[2].Id, LockedInPrice = 3500m }
                }
            },

            // [11] Departing today — O02
            new Booking
            {
                GuestName = "Marcus de Vries",
                GuestEmail = "marcus.devries@gmail.com",
                GuestCount = 2,
                CheckInDate = now.Date.AddDays(-4),
                CheckOutDate = now.Date,
                BookingStatus = BookingStatus.CheckedIn,
                PaymentStatus = PaymentStatus.Pending,
                Origin = BookingOrigin.WalkIn,
                BookedAt = now.Date.AddDays(-4),
                BookingRooms = new List<BookingRoom>
                {
                    new BookingRoom { RoomTypeId = obsidian.Id, RoomId = rooms[4].Id, LockedInPrice = 4200m }
                }
            },

            // [12] Long stay — 30 nights, Ember E01
            new Booking
            {
                GuestName = "Priya Subramaniam",
                GuestEmail = "priya.subramaniam@gmail.com",
                GuestCount = 2,
                CheckInDate = now.Date.AddDays(-15),
                CheckOutDate = now.Date.AddDays(15),
                BookingStatus = BookingStatus.CheckedIn,
                PaymentStatus = PaymentStatus.Pending,
                Origin = BookingOrigin.Guest,
                BookedAt = now.AddDays(-90),
                BookingRooms = new List<BookingRoom>
                {
                    new BookingRoom { RoomTypeId = ember.Id, RoomId = rooms[6].Id, LockedInPrice = 5800m }
                }
            },

            // [13] Checked out, UNPAID — guest left without settling
            new Booking
            {
                GuestName = "Conrad Hale",
                GuestEmail = "conrad.hale@gmail.com",
                GuestCount = 1,
                CheckInDate = now.Date.AddDays(-35),
                CheckOutDate = now.Date.AddDays(-33),
                BookingStatus = BookingStatus.CheckedOut,
                PaymentStatus = PaymentStatus.Pending,
                Origin = BookingOrigin.WalkIn,
                BookedAt = now.Date.AddDays(-35),
                BookingRooms = new List<BookingRoom>
                {
                    new BookingRoom { RoomTypeId = hollow.Id, RoomId = rooms[0].Id, LockedInPrice = 3500m }
                }
            },

            // [14] Cancelled, Paid — awaiting refund, Monolith MP01
            new Booking
            {
                GuestName = "Antoinette Bellerose",
                GuestEmail = "antoinette.bellerose@gmail.com",
                GuestCount = 3,
                CheckInDate = now.Date.AddDays(25),
                CheckOutDate = now.Date.AddDays(28),
                BookingStatus = BookingStatus.Cancelled,
                PaymentStatus = PaymentStatus.Paid,
                Origin = BookingOrigin.Guest,
                BookedAt = now.AddDays(-50),
                BookingRooms = new List<BookingRoom>
                {
                    new BookingRoom { RoomTypeId = monolith.Id, RoomId = null, LockedInPrice = 22000m }
                }
            },

            // [15] Cancelled, Refunded — Monolith MP02
            new Booking
            {
                GuestName = "Dmitri Orloff",
                GuestEmail = "dmitri.orloff@gmail.com",
                GuestCount = 2,
                CheckInDate = now.Date.AddDays(30),
                CheckOutDate = now.Date.AddDays(33),
                BookingStatus = BookingStatus.Cancelled,
                PaymentStatus = PaymentStatus.Refunded,
                Origin = BookingOrigin.Guest,
                BookedAt = now.AddDays(-60),
                BookingRooms = new List<BookingRoom>
                {
                    new BookingRoom { RoomTypeId = monolith.Id, RoomId = null, LockedInPrice = 22000m }
                }
            },

            // [16] VVIP Sanctum — cust3, multi-room (SX01 + SX02)
            new Booking
            {
                GuestName = "Aleksei Volkov",
                GuestEmail = "cust3@gmail.com",
                UserId = cust3.Id,
                GuestCount = 6,
                CheckInDate = now.AddDays(-1),
                CheckOutDate = now.AddDays(5),
                BookingStatus = BookingStatus.CheckedIn,
                PaymentStatus = PaymentStatus.Paid,
                Origin = BookingOrigin.RegisteredUser,
                BookedAt = now.AddDays(-120),
                BookingRooms = new List<BookingRoom>
                {
                    new BookingRoom { RoomTypeId = sanctum.Id, RoomId = rooms[21].Id, LockedInPrice = 30000m },
                    new BookingRoom { RoomTypeId = sanctum.Id, RoomId = rooms[22].Id, LockedInPrice = 30000m }
                }
            },

            // [17] Multi-room family — cust4, Stillwater SW01 + SW02
            new Booking
            {
                GuestName = "Nadine El-Amin",
                GuestEmail = "cust4@gmail.com",
                UserId = cust4.Id,
                GuestCount = 4,
                CheckInDate = now.AddDays(-2),
                CheckOutDate = now.AddDays(2),
                BookingStatus = BookingStatus.CheckedIn,
                PaymentStatus = PaymentStatus.Pending,
                Origin = BookingOrigin.RegisteredUser,
                BookedAt = now.AddDays(-30),
                BookingRooms = new List<BookingRoom>
                {
                    new BookingRoom { RoomTypeId = stillwater.Id, RoomId = rooms[12].Id, LockedInPrice = 9500m },
                    new BookingRoom { RoomTypeId = stillwater.Id, RoomId = rooms[13].Id, LockedInPrice = 9500m }
                }
            },

            // [18] Far-future pre-booking — cust5, Monolith
            new Booking
            {
                GuestName = "Constance Morrow",
                GuestEmail = "cust5@gmail.com",
                UserId = cust5.Id,
                GuestCount = 2,
                CheckInDate = now.AddDays(90),
                CheckOutDate = now.AddDays(95),
                BookingStatus = BookingStatus.Booked,
                PaymentStatus = PaymentStatus.Paid,
                Origin = BookingOrigin.RegisteredUser,
                BookedAt = now.AddDays(-10),
                BookingRooms = new List<BookingRoom>
                {
                    new BookingRoom { RoomTypeId = monolith.Id, RoomId = null, LockedInPrice = 22000m }
                }
            },

            // [19] Guest-origin future booking — no room assigned
            new Booking
            {
                GuestName = "Florian Czekaj",
                GuestEmail = "florian.czekaj@gmail.com",
                GuestCount = 1,
                CheckInDate = now.AddDays(20),
                CheckOutDate = now.AddDays(23),
                BookingStatus = BookingStatus.Booked,
                PaymentStatus = PaymentStatus.Pending,
                Origin = BookingOrigin.Guest,
                BookedAt = now.AddDays(-2),
                BookingRooms = new List<BookingRoom>
                {
                    new BookingRoom { RoomTypeId = hollow.Id, RoomId = null, LockedInPrice = 3500m }
                }
            }
        };

    context.Bookings.AddRange(bookings);
    context.SaveChanges();

    // ======================================================
    // 7. FOOD ORDERS
    // ======================================================
    var foodOrders = new List<FoodOrder>
        {
            // [0] Delivered — cust1, The Hollow (Booking 0)
            new FoodOrder { BookingId = bookings[0].Id, RoomId = bookings[0].BookingRooms.FirstOrDefault()!.RoomId, OrderStatus = FoodOrderStatus.Delivered, GeneratedAt = now.AddDays(-1).AddHours(-2), OrderItems = new List<FoodOrderItem> { new FoodOrderItem { MenuItemId = menu[0].Id, Quantity = 1, PriceAtPurchase = 180m }, new FoodOrderItem { MenuItemId = menu[4].Id, Quantity = 1, PriceAtPurchase = 620m }, new FoodOrderItem { MenuItemId = menu[9].Id, Quantity = 1, PriceAtPurchase = 950m } } },

            // [1] Preparing — cust1, second order (Booking 0)
            new FoodOrder { BookingId = bookings[0].Id, RoomId = bookings[0].BookingRooms.FirstOrDefault()!.RoomId, OrderStatus = FoodOrderStatus.Preparing, GeneratedAt = now.AddMinutes(-40), OrderItems = new List<FoodOrderItem> { new FoodOrderItem { MenuItemId = menu[10].Id, Quantity = 1, PriceAtPurchase = 120m }, new FoodOrderItem { MenuItemId = menu[7].Id, Quantity = 1, PriceAtPurchase = 190m } } },

            // [2] Pending — cust2, Obsidian (Booking 1)
            new FoodOrder { BookingId = bookings[1].Id, RoomId = bookings[1].BookingRooms.FirstOrDefault()!.RoomId, OrderStatus = FoodOrderStatus.Pending, GeneratedAt = now.AddMinutes(-8), OrderItems = new List<FoodOrderItem> { new FoodOrderItem { MenuItemId = menu[3].Id, Quantity = 2, PriceAtPurchase = 150m }, new FoodOrderItem { MenuItemId = menu[5].Id, Quantity = 1, PriceAtPurchase = 540m } } },

            // [3] Delivered — cust2, earlier (Booking 1)
            new FoodOrder { BookingId = bookings[1].Id, RoomId = bookings[1].BookingRooms.FirstOrDefault()!.RoomId, OrderStatus = FoodOrderStatus.Delivered, GeneratedAt = now.AddDays(-1).AddHours(-5), OrderItems = new List<FoodOrderItem> { new FoodOrderItem { MenuItemId = menu[1].Id, Quantity = 2, PriceAtPurchase = 240m }, new FoodOrderItem { MenuItemId = menu[11].Id, Quantity = 2, PriceAtPurchase = 480m } } },

            // [4] Delivered — Priya, long-stay (Booking 12)
            new FoodOrder { BookingId = bookings[12].Id, RoomId = bookings[12].BookingRooms.FirstOrDefault()!.RoomId, OrderStatus = FoodOrderStatus.Delivered, GeneratedAt = now.AddDays(-6), OrderItems = new List<FoodOrderItem> { new FoodOrderItem { MenuItemId = menu[12].Id, Quantity = 2, PriceAtPurchase = 1200m } } },

            // [5] Pending — Lena (Arriving Today, Booking 10)
            new FoodOrder { BookingId = bookings[10].Id, RoomId = bookings[10].BookingRooms.FirstOrDefault()!.RoomId, OrderStatus = FoodOrderStatus.Pending, GeneratedAt = now.AddMinutes(-12), OrderItems = new List<FoodOrderItem> { new FoodOrderItem { MenuItemId = menu[0].Id, Quantity = 1, PriceAtPurchase = 180m }, new FoodOrderItem { MenuItemId = menu[10].Id, Quantity = 1, PriceAtPurchase = 120m } } },

            // [6] Preparing — Marcus (Departing Today, Booking 11)
            new FoodOrder { BookingId = bookings[11].Id, RoomId = bookings[11].BookingRooms.FirstOrDefault()!.RoomId, OrderStatus = FoodOrderStatus.Preparing, GeneratedAt = now.AddMinutes(-25), OrderItems = new List<FoodOrderItem> { new FoodOrderItem { MenuItemId = menu[8].Id, Quantity = 1, PriceAtPurchase = 170m }, new FoodOrderItem { MenuItemId = menu[10].Id, Quantity = 1, PriceAtPurchase = 120m } } },

            // [7] Delivered — Marcus, prior dinner (Booking 11)
            new FoodOrder { BookingId = bookings[11].Id, RoomId = bookings[11].BookingRooms.FirstOrDefault()!.RoomId, OrderStatus = FoodOrderStatus.Delivered, GeneratedAt = now.AddDays(-2), OrderItems = new List<FoodOrderItem> { new FoodOrderItem { MenuItemId = menu[2].Id, Quantity = 2, PriceAtPurchase = 260m }, new FoodOrderItem { MenuItemId = menu[9].Id, Quantity = 1, PriceAtPurchase = 950m } } },

            // [8] VVIP Sanctum — cust3 first night (Booking 16)
            new FoodOrder { BookingId = bookings[16].Id, RoomId = bookings[16].BookingRooms.FirstOrDefault()!.RoomId, OrderStatus = FoodOrderStatus.Delivered, GeneratedAt = now.AddDays(-1).AddHours(-6), OrderItems = new List<FoodOrderItem> { new FoodOrderItem { MenuItemId = menu[12].Id, Quantity = 6, PriceAtPurchase = 1200m }, new FoodOrderItem { MenuItemId = menu[9].Id, Quantity = 6, PriceAtPurchase = 950m }, new FoodOrderItem { MenuItemId = menu[11].Id, Quantity = 2, PriceAtPurchase = 480m } } },

            // [9] VVIP Sanctum — morning pending (Booking 16)
            new FoodOrder { BookingId = bookings[16].Id, RoomId = bookings[16].BookingRooms.FirstOrDefault()!.RoomId, OrderStatus = FoodOrderStatus.Pending, GeneratedAt = now.AddMinutes(-5), OrderItems = new List<FoodOrderItem> { new FoodOrderItem { MenuItemId = menu[0].Id, Quantity = 6, PriceAtPurchase = 180m }, new FoodOrderItem { MenuItemId = menu[10].Id, Quantity = 6, PriceAtPurchase = 120m } } },

            // [10] cust4 family villa (Booking 17)
            new FoodOrder { BookingId = bookings[17].Id, RoomId = bookings[17].BookingRooms.FirstOrDefault()!.RoomId, OrderStatus = FoodOrderStatus.Delivered, GeneratedAt = now.AddDays(-1), OrderItems = new List<FoodOrderItem> { new FoodOrderItem { MenuItemId = menu[4].Id, Quantity = 2, PriceAtPurchase = 620m }, new FoodOrderItem { MenuItemId = menu[5].Id, Quantity = 2, PriceAtPurchase = 540m }, new FoodOrderItem { MenuItemId = menu[7].Id, Quantity = 4, PriceAtPurchase = 190m }, new FoodOrderItem { MenuItemId = menu[9].Id, Quantity = 2, PriceAtPurchase = 950m } } }
        };

    context.FoodOrders.AddRange(foodOrders);
    context.SaveChanges();

    // ======================================================
    // 8. BOOKING AMENITIES
    // ======================================================
    context.BookingAmenities.AddRange(
        // cust1 (Booking 0) — Spa, Sound Bath, Digital Detox
        new BookingAmenity { BookingId = bookings[0].Id, AmenityId = amenities[5].Id, PriceAtPurchase = 900m },
        new BookingAmenity { BookingId = bookings[0].Id, AmenityId = amenities[10].Id, PriceAtPurchase = 400m },
        new BookingAmenity { BookingId = bookings[0].Id, AmenityId = amenities[7].Id, PriceAtPurchase = 300m },

        // cust2 (Booking 1) — Butler, Wine Cellar, Art Curator
        new BookingAmenity { BookingId = bookings[1].Id, AmenityId = amenities[1].Id, PriceAtPurchase = 1800m },
        new BookingAmenity { BookingId = bookings[1].Id, AmenityId = amenities[6].Id, PriceAtPurchase = 1200m },
        new BookingAmenity { BookingId = bookings[1].Id, AmenityId = amenities[13].Id, PriceAtPurchase = 950m },

        // cust1 2nd stay (Booking 8) — Floatation, Stargazing
        new BookingAmenity { BookingId = bookings[8].Id, AmenityId = amenities[4].Id, PriceAtPurchase = 650m },
        new BookingAmenity { BookingId = bookings[8].Id, AmenityId = amenities[8].Id, PriceAtPurchase = 550m },

        // cust2 Ashwood (Booking 9) — Butler, Private Chef, Yacht
        new BookingAmenity { BookingId = bookings[9].Id, AmenityId = amenities[1].Id, PriceAtPurchase = 1800m },
        new BookingAmenity { BookingId = bookings[9].Id, AmenityId = amenities[0].Id, PriceAtPurchase = 2500m },
        new BookingAmenity { BookingId = bookings[9].Id, AmenityId = amenities[3].Id, PriceAtPurchase = 12000m },

        // VVIP Sanctum — cust3 (Booking 16)
        new BookingAmenity { BookingId = bookings[16].Id, AmenityId = amenities[2].Id, PriceAtPurchase = 8500m },
        new BookingAmenity { BookingId = bookings[16].Id, AmenityId = amenities[11].Id, PriceAtPurchase = 3200m },
        new BookingAmenity { BookingId = bookings[16].Id, AmenityId = amenities[0].Id, PriceAtPurchase = 2500m },
        new BookingAmenity { BookingId = bookings[16].Id, AmenityId = amenities[1].Id, PriceAtPurchase = 1800m },
        new BookingAmenity { BookingId = bookings[16].Id, AmenityId = amenities[3].Id, PriceAtPurchase = 12000m },
        new BookingAmenity { BookingId = bookings[16].Id, AmenityId = amenities[9].Id, PriceAtPurchase = 1600m },
        new BookingAmenity { BookingId = bookings[16].Id, AmenityId = amenities[12].Id, PriceAtPurchase = 750m },

        // cust4 family villa (Booking 17)
        new BookingAmenity { BookingId = bookings[17].Id, AmenityId = amenities[5].Id, PriceAtPurchase = 900m },
        new BookingAmenity { BookingId = bookings[17].Id, AmenityId = amenities[8].Id, PriceAtPurchase = 550m },
        new BookingAmenity { BookingId = bookings[17].Id, AmenityId = amenities[9].Id, PriceAtPurchase = 1600m },

        // Priya long-stay (Booking 12)
        new BookingAmenity { BookingId = bookings[12].Id, AmenityId = amenities[7].Id, PriceAtPurchase = 300m },
        new BookingAmenity { BookingId = bookings[12].Id, AmenityId = amenities[4].Id, PriceAtPurchase = 650m },
        new BookingAmenity { BookingId = bookings[12].Id, AmenityId = amenities[10].Id, PriceAtPurchase = 400m },

        // cust5 checked-out (Booking 4)
        new BookingAmenity { BookingId = bookings[4].Id, AmenityId = amenities[5].Id, PriceAtPurchase = 900m },
        new BookingAmenity { BookingId = bookings[4].Id, AmenityId = amenities[6].Id, PriceAtPurchase = 1200m }
    );

    // ======================================================
    // 9. HOUSEKEEPING TASKS
    // ======================================================
    context.HousekeepingTasks.AddRange(
        new Housekeeping { RoomId = rooms[0].Id, OriginType = HousekeepingOriginType.CheckoutAutomated, Status = HousekeepingStatus.Completed, Description = "Full suite reset after extended stay", StartedAt = now.AddDays(-33).AddHours(10), FinishedAt = now.AddDays(-33).AddHours(12) },
        new Housekeeping { RoomId = rooms[3].Id, OriginType = HousekeepingOriginType.GuestRequested, Status = HousekeepingStatus.InProgress, Description = "Evening turndown and obsidian bath preparation", StartedAt = now.AddMinutes(-30) },
        new Housekeeping { RoomId = rooms[6].Id, OriginType = HousekeepingOriginType.GuestRequested, Status = HousekeepingStatus.Pending, Description = "Daily linen refresh and hearth restoke" },
        new Housekeeping { RoomId = rooms[10].Id, OriginType = HousekeepingOriginType.CheckoutAutomated, Status = HousekeepingStatus.Pending, Description = "Full checkout reset — Vantage Loft" },
        new Housekeeping { RoomId = rooms[7].Id, OriginType = HousekeepingOriginType.CheckoutAutomated, Status = HousekeepingStatus.Completed, Description = "Post-checkout suite deep clean", StartedAt = now.AddDays(-7).AddHours(11), FinishedAt = now.AddDays(-7).AddHours(14) },
        new Housekeeping { RoomId = rooms[1].Id, OriginType = HousekeepingOriginType.CheckoutAutomated, Status = HousekeepingStatus.Completed, Description = "Post-checkout limewash inspection and reset", StartedAt = now.AddDays(-18).AddHours(10), FinishedAt = now.AddDays(-18).AddHours(11) },
        new Housekeeping { RoomId = rooms[2].Id, OriginType = HousekeepingOriginType.GuestRequested, Status = HousekeepingStatus.InProgress, Description = "Pre-arrival personalization: dawn-shaft alignment check, room scent setting", StartedAt = now.AddHours(-1) },
        new Housekeeping { RoomId = rooms[4].Id, OriginType = HousekeepingOriginType.CheckoutAutomated, Status = HousekeepingStatus.Pending, Description = "Checkout reset — Obsidian Chamber, plunge pool drain and refill" },
        new Housekeeping { RoomId = rooms[4].Id, OriginType = HousekeepingOriginType.StaffRequested, Status = HousekeepingStatus.Pending, Description = "Stonework inspection after guest departure" },
        new Housekeeping { RoomId = rooms[15].Id, OriginType = HousekeepingOriginType.GuestRequested, Status = HousekeepingStatus.InProgress, Description = "Private dining room layout for evening chef service", StartedAt = now.AddMinutes(-45) },
        new Housekeeping { RoomId = rooms[21].Id, OriginType = HousekeepingOriginType.StaffRequested, Status = HousekeepingStatus.Pending, Description = "Sanctum perimeter grounds inspection and morning preparation" },
        new Housekeeping { RoomId = rooms[22].Id, OriginType = HousekeepingOriginType.GuestRequested, Status = HousekeepingStatus.Pending, Description = "Guest suite refresh — south wing" },
        new Housekeeping { RoomId = rooms[5].Id, OriginType = HousekeepingOriginType.StaffRequested, Status = HousekeepingStatus.Pending, Description = "Post-sealing inspection before O03 returns to active inventory", Location = "Obsidian Chamber O03 — Renovation Wing" },
        new Housekeeping { RoomId = null, OriginType = HousekeepingOriginType.StaffRequested, Status = HousekeepingStatus.Pending, Description = "Observatory telescope lens cleaning — pre-Stargazing session tonight", Location = "North Observatory Dome" }
    );

    // ======================================================
    // 10. MAINTENANCE TASKS
    // ======================================================
    context.MaintenanceTasks.AddRange(
        new MaintenanceTask { RoomId = rooms[5].Id, OriginType = MaintenanceOriginType.StaffRequested, Status = MaintenanceStatus.InProgress, Description = "Volcanic obsidian stone panel re-sealing — specialist contractor on-site", StartedAt = now.AddDays(-3) },
        new MaintenanceTask { RoomId = rooms[14].Id, OriginType = MaintenanceOriginType.StaffRequested, Status = MaintenanceStatus.InProgress, Description = "Stillwater courtyard basin waterproof lining replacement", StartedAt = now.AddDays(-5) },
        new MaintenanceTask { RoomId = rooms[14].Id, OriginType = MaintenanceOriginType.StaffRequested, Status = MaintenanceStatus.Pending, Description = "Courtyard stone refinishing after waterproof lining cures (blocked by lining task)" },
        new MaintenanceTask { RoomId = rooms[0].Id, OriginType = MaintenanceOriginType.SystemAutomated, Status = MaintenanceStatus.Completed, Description = "HEPA filter replacement — Hollow room climate system", StartedAt = now.AddDays(-10), FinishedAt = now.AddDays(-10).AddHours(1) },
        new MaintenanceTask { RoomId = rooms[6].Id, OriginType = MaintenanceOriginType.GuestRequested, Status = MaintenanceStatus.InProgress, Description = "Ember Suite hearth glass seal inspection — minor smoke trace reported", StartedAt = now.AddHours(-3) },
        new MaintenanceTask { RoomId = rooms[15].Id, OriginType = MaintenanceOriginType.StaffRequested, Status = MaintenanceStatus.Completed, Description = "Ashwood exterior timber joint re-caulking (annual)", StartedAt = now.AddDays(-8), FinishedAt = now.AddDays(-8).AddHours(4) },
        new MaintenanceTask { RoomId = rooms[9].Id, OriginType = MaintenanceOriginType.SystemAutomated, Status = MaintenanceStatus.Pending, Description = "Quarterly structural inspection of Vantage Loft blackened steel stilts" },
        new MaintenanceTask { RoomId = rooms[18].Id, OriginType = MaintenanceOriginType.GuestRequested, Status = MaintenanceStatus.Pending, Description = "Monolith terrace drain partially blocked — pre-arrival clearance required" },
        new MaintenanceTask { RoomId = rooms[21].Id, OriginType = MaintenanceOriginType.StaffRequested, Status = MaintenanceStatus.Completed, Description = "Sanctum perimeter low-profile lighting circuit check pre-VVIP arrival", StartedAt = now.AddDays(-2), FinishedAt = now.AddDays(-2).AddHours(2) },
        new MaintenanceTask { RoomId = rooms[23].Id, OriginType = MaintenanceOriginType.SystemAutomated, Status = MaintenanceStatus.Pending, Description = "Standby suite SX03 — annual mechanical systems test" },
        new MaintenanceTask { RoomId = null, OriginType = MaintenanceOriginType.StaffRequested, Status = MaintenanceStatus.Pending, Description = "Private dock mooring cleats replacement before yacht charter season", Location = "Private Dock — South Perimeter" }
    );

    // ======================================================
    // 11. FEEDBACK
    // ======================================================
    context.Feedbacks.AddRange(
        new Feedback { BookingId = bookings[4].Id, Rating = 5, Comments = "The Ember Suite erased all other properties from memory. The hearth burns without sound. The silence is total. I return.", CreatedAt = now.AddDays(-7), IsHidden = false },
        new Feedback { BookingId = bookings[5].Id, Rating = 5, Comments = "The Hollow is not a room. It is a state of being. I arrived exhausted. I left restored.", CreatedAt = now.AddDays(-18), IsHidden = false },
        new Feedback { BookingId = bookings[13].Id, Rating = 1, Comments = "Overpriced. The staff asked me to settle my bill twice. I refuse to be treated like a debtor.", CreatedAt = now.AddDays(-33), IsHidden = true },
        new Feedback { BookingId = bookings[0].Id, Rating = 5, Comments = "The light at dawn through the single aperture. I have been awake for it twice now. I will not be able to leave without it.", CreatedAt = now.AddDays(-1), IsHidden = false },
        new Feedback { BookingId = bookings[1].Id, Rating = 4, Comments = "The obsidian bath is extraordinary. The plunge pool needs three more degrees in winter.", CreatedAt = now.AddHours(-12), IsHidden = false },
        new Feedback { BookingId = bookings[8].Id, Rating = 5, Comments = "The Vantage has the clearest horizon I have seen from indoors. The valley is not scenic. It is absolute.", CreatedAt = now.AddDays(-2), IsHidden = false },
        new Feedback { BookingId = bookings[9].Id, Rating = 5, Comments = "The Ashwood Residence accommodated our entire delegation. Chef's in-suite dinner was an event, not a meal.", CreatedAt = now.AddDays(-2), IsHidden = false },
        new Feedback { BookingId = bookings[12].Id, Rating = 4, Comments = "", CreatedAt = now.AddDays(-10), IsHidden = false },
        new Feedback { BookingId = bookings[16].Id, Rating = 5, Comments = "The Sanctum staff are present without being visible. The perimeter is genuine. This is what I require.", CreatedAt = now.AddHours(-18), IsHidden = false },
        new Feedback { BookingId = bookings[17].Id, Rating = 5, Comments = "Two villas sharing a courtyard of still water. My children did not fight once.", CreatedAt = now.AddDays(-1), IsHidden = false },
        new Feedback { BookingId = bookings[7].Id, Rating = 3, Comments = "Could not make the dates. Aetheris declined to waive the cancellation fee.", CreatedAt = now.AddDays(-5), IsHidden = false }
    );

    // ======================================================
    // 12. RECEIPTS
    // ======================================================
    context.Receipts.AddRange(
        new Receipt { BookingId = bookings[4].Id, AmountPaid = 31100m, PaymentMethod = "Private Bank Transfer", TransactionId = "AE-TXN-0001", PaidAt = now.AddDays(-7) },
        new Receipt { BookingId = bookings[5].Id, AmountPaid = 14000m, PaymentMethod = "Amex Centurion", TransactionId = "AE-TXN-0002", PaidAt = now.AddDays(-18) },
        new Receipt { BookingId = bookings[2].Id, AmountPaid = 29000m, PaymentMethod = "Wire Transfer", TransactionId = "AE-TXN-0003", PaidAt = now.AddDays(-18) },
        new Receipt { BookingId = bookings[8].Id, AmountPaid = 28800m, PaymentMethod = "Private Bank Transfer", TransactionId = "AE-TXN-0004", PaidAt = now.AddDays(-60) },
        new Receipt { BookingId = bookings[9].Id, AmountPaid = 84000m, PaymentMethod = "Wire Transfer", TransactionId = "AE-TXN-0005", PaidAt = now.AddDays(-4) },
        new Receipt { BookingId = bookings[0].Id, AmountPaid = 10500m, PaymentMethod = "Amex Centurion", TransactionId = "AE-TXN-0006", PaidAt = now.AddDays(-14) },
        new Receipt { BookingId = bookings[1].Id, AmountPaid = 8400m, PaymentMethod = "Wire Transfer", TransactionId = "AE-TXN-0007", PaidAt = now.AddDays(-30) },
        new Receipt { BookingId = bookings[14].Id, AmountPaid = 66000m, PaymentMethod = "Wire Transfer", TransactionId = "AE-TXN-0008", PaidAt = now.AddDays(-50) },
        new Receipt { BookingId = bookings[15].Id, AmountPaid = -66000m, PaymentMethod = "Wire Transfer", TransactionId = "AE-REF-0001", PaidAt = now.AddDays(-40) },
        new Receipt { BookingId = bookings[16].Id, AmountPaid = 390350m, PaymentMethod = "Private Bank Transfer", TransactionId = "AE-TXN-VIP-001", PaidAt = now.AddDays(-120) },
        new Receipt { BookingId = bookings[18].Id, AmountPaid = 110000m, PaymentMethod = "Amex Centurion", TransactionId = "AE-TXN-0009", PaidAt = now.AddDays(-10) },
        new Receipt { BookingId = bookings[12].Id, AmountPaid = 87000m, PaymentMethod = "Wire Transfer", TransactionId = "AE-TXN-0010", PaidAt = now.AddDays(-12) },
        new Receipt { BookingId = bookings[17].Id, AmountPaid = 19000m, PaymentMethod = "Amex Centurion", TransactionId = "AE-TXN-0011", PaidAt = now.AddDays(-30) }
    );

    context.SaveChanges();
  }
}
