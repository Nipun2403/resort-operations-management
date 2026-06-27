using HotelManagement.DAL.Context;
using HotelManagement.DAL.Entities;
using HotelManagement.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace HotelManagement.API.Utilities;

public static class MainDatabaseSeeder
{
    public static void Seed(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Only seed if empty
        if (context.Users.Any()) return;

        var now = DateTime.UtcNow;

        // 1. Users
        var admin = new User { Email = "admin@hotel.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"), Role = "Admin", FirstName = "Super", LastName = "Admin", IsActive = true };
        var frontdesk1 = new User { Email = "fd1@hotel.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass123"), Role = "FrontDesk", FirstName = "Sarah", LastName = "Desk", IsActive = true };
        var frontdesk2 = new User { Email = "fd2@hotel.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass123"), Role = "FrontDesk", FirstName = "Mike", LastName = "Desk", IsActive = true };
        var frontdesk3 = new User { Email = "fd3@hotel.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass123"), Role = "FrontDesk", FirstName = "Elena", LastName = "Rostova", IsActive = true };
        var kitchen = new User { Email = "chef@hotel.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass123"), Role = "Kitchen", FirstName = "Gordon", LastName = "Ramsay", IsActive = true };
        var kitchenAsst = new User { Email = "sous@hotel.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass123"), Role = "Kitchen", FirstName = "Marco", LastName = "Pierre", IsActive = true };
        var housekeeping = new User { Email = "clean@hotel.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass123"), Role = "Housekeeping", FirstName = "Maria", LastName = "Clean", IsActive = true };
        var housekeeping2 = new User { Email = "clean2@hotel.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass123"), Role = "Housekeeping", FirstName = "Anna", LastName = "Svensson", IsActive = true };
        var maintenance = new User { Email = "fix@hotel.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass123"), Role = "Maintenance", FirstName = "Bob", LastName = "Fixer", IsActive = true };
        var customer1 = new User { Email = "cust1@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass123"), Role = "RegisteredUser", FirstName = "John", LastName = "Doe", IsActive = true };
        var customer2 = new User { Email = "cust2@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass123"), Role = "RegisteredUser", FirstName = "Jane", LastName = "Smith", IsActive = true };
        var customer3 = new User { Email = "cust3@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass123"), Role = "RegisteredUser", FirstName = "Alex", LastName = "Vance", IsActive = true };
        var customer4 = new User { Email = "cust4@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass123"), Role = "RegisteredUser", FirstName = "David", LastName = "Miller", IsActive = true };
        var customer5 = new User { Email = "cust5@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass123"), Role = "RegisteredUser", FirstName = "Emily", LastName = "Watson", IsActive = true };

        // Edge case users
        var inactiveUser = new User { Email = "fired@hotel.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass123"), Role = "FrontDesk", FirstName = "Fired", LastName = "Guy", IsActive = false };
        var userNoBookings = new User { Email = "nobookings@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass123"), Role = "RegisteredUser", FirstName = "Ghost", LastName = "User", IsActive = true };
        var deletedCustomer = new User { Email = "banned@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass123"), Role = "RegisteredUser", FirstName = "Banned", LastName = "Customer", IsActive = false };
        var pendingActivationUser = new User { Email = "pending@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass123"), Role = "RegisteredUser", FirstName = "Newborn", LastName = "Account", IsActive = true };

        context.Users.AddRange(admin, frontdesk1, frontdesk2, frontdesk3, kitchen, kitchenAsst, housekeeping, housekeeping2, maintenance, customer1, customer2, customer3, customer4, customer5, inactiveUser, userNoBookings, deletedCustomer, pendingActivationUser);
        context.SaveChanges();

        // 2. Room Types
        var standard = new RoomType { Name = "Standard", BasePrice = 100m, MaxOccupancy = 2, Description = "Cozy room.", ImageUrls = new List<string> { "standard.jpg" }, SquareFootage = 300, BedConfigurationJson = JsonSerializer.Serialize(new Dictionary<string, int> { { "Queen", 1 } }), IsActive = true };
        var deluxe = new RoomType { Name = "Deluxe Suite", BasePrice = 150m, MaxOccupancy = 2, Description = "Spacious suite.", ImageUrls = new List<string> { "deluxe.jpg" }, SquareFootage = 500, BedConfigurationJson = JsonSerializer.Serialize(new Dictionary<string, int> { { "King", 1 } }), IsActive = true };
        var family = new RoomType { Name = "Family Suite", BasePrice = 250m, MaxOccupancy = 4, Description = "Great for kids.", ImageUrls = new List<string> { "family.jpg" }, SquareFootage = 700, BedConfigurationJson = JsonSerializer.Serialize(new Dictionary<string, int> { { "Queen", 2 } }), IsActive = true };
        var presidential = new RoomType { Name = "Presidential", BasePrice = 500m, MaxOccupancy = 4, Description = "Top tier.", ImageUrls = new List<string> { "presidential.jpg" }, SquareFootage = 1200, BedConfigurationJson = JsonSerializer.Serialize(new Dictionary<string, int> { { "King", 1 }, { "Twin", 2 } }), IsActive = true };

        // Edge case room types
        var retiredType = new RoomType { Name = "Legacy Economy", BasePrice = 50m, MaxOccupancy = 1, Description = "Old room.", ImageUrls = new List<string> { "legacy.jpg" }, SquareFootage = 200, BedConfigurationJson = JsonSerializer.Serialize(new Dictionary<string, int> { { "Twin", 1 } }), IsActive = false };
        var noRoomsType = new RoomType { Name = "Boutique Suite", BasePrice = 300m, MaxOccupancy = 2, Description = "Concept suite, no rooms yet.", ImageUrls = new List<string> { "boutique.jpg" }, SquareFootage = 450, BedConfigurationJson = JsonSerializer.Serialize(new Dictionary<string, int> { { "King", 1 } }), IsActive = true };

        var executive = new RoomType { Name = "Executive Suite", BasePrice = 350m, MaxOccupancy = 3, Description = "For business.", ImageUrls = new List<string> { "executive.jpg" }, SquareFootage = 600, BedConfigurationJson = JsonSerializer.Serialize(new Dictionary<string, int> { { "King", 1 } }), IsActive = true };
        var connecting = new RoomType { Name = "Connecting Rooms", BasePrice = 280m, MaxOccupancy = 4, Description = "Two rooms connected.", ImageUrls = new List<string> { "connecting.jpg" }, SquareFootage = 600, BedConfigurationJson = JsonSerializer.Serialize(new Dictionary<string, int> { { "Queen", 2 } }), IsActive = true };
        var accessible = new RoomType { Name = "Accessible Room", BasePrice = 120m, MaxOccupancy = 2, Description = "ADA compliant.", ImageUrls = new List<string> { "ada.jpg" }, SquareFootage = 350, BedConfigurationJson = JsonSerializer.Serialize(new Dictionary<string, int> { { "Queen", 1 } }), IsActive = true };
        var penthouse = new RoomType { Name = "Penthouse", BasePrice = 1000m, MaxOccupancy = 6, Description = "Top floor luxury.", ImageUrls = new List<string> { "penthouse.jpg" }, SquareFootage = 2000, BedConfigurationJson = JsonSerializer.Serialize(new Dictionary<string, int> { { "King", 2 }, { "Queen", 2 } }), IsActive = true };

        // Extended options
        var studio = new RoomType { Name = "Studio Apartment", BasePrice = 180m, MaxOccupancy = 2, Description = "Extended stay optimized.", ImageUrls = new List<string> { "studio.jpg" }, SquareFootage = 400, BedConfigurationJson = JsonSerializer.Serialize(new Dictionary<string, int> { { "Murphy King", 1 } }), IsActive = true };
        var villa = new RoomType { Name = "Oceanfront Villa", BasePrice = 1500m, MaxOccupancy = 8, Description = "Detached high-end luxury property.", ImageUrls = new List<string> { "villa.jpg" }, SquareFootage = 3500, BedConfigurationJson = JsonSerializer.Serialize(new Dictionary<string, int> { { "King", 4 } }), IsActive = true };

        context.RoomTypes.AddRange(standard, deluxe, family, presidential, retiredType, noRoomsType, executive, connecting, accessible, penthouse, studio, villa);
        context.SaveChanges();

        // 3. Rooms
        var rooms = new List<Room>
        {
            new Room { RoomNumber = "101", RoomTypeId = standard.Id, IsActive = true },
            new Room { RoomNumber = "102", RoomTypeId = standard.Id, IsActive = true },
            new Room { RoomNumber = "103", RoomTypeId = standard.Id, IsActive = true },
            new Room { RoomNumber = "104", RoomTypeId = standard.Id, IsActive = true },
            new Room { RoomNumber = "105", RoomTypeId = accessible.Id, IsActive = true },
            new Room { RoomNumber = "201", RoomTypeId = deluxe.Id, IsActive = true },
            new Room { RoomNumber = "202", RoomTypeId = deluxe.Id, IsActive = true },
            new Room { RoomNumber = "203", RoomTypeId = deluxe.Id, IsActive = true },
            new Room { RoomNumber = "204", RoomTypeId = executive.Id, IsActive = true },
            new Room { RoomNumber = "205", RoomTypeId = connecting.Id, IsActive = true },
            new Room { RoomNumber = "301", RoomTypeId = family.Id, IsActive = true },
            new Room { RoomNumber = "302", RoomTypeId = family.Id, IsActive = true },
            new Room { RoomNumber = "303", RoomTypeId = family.Id, IsActive = true },
            new Room { RoomNumber = "304", RoomTypeId = studio.Id, IsActive = true },
            new Room { RoomNumber = "401", RoomTypeId = presidential.Id, IsActive = true },
            new Room { RoomNumber = "501", RoomTypeId = penthouse.Id, IsActive = true },
            new Room { RoomNumber = "V01", RoomTypeId = villa.Id, IsActive = true },
            
            // Edge case rooms
            new Room { RoomNumber = "999", RoomTypeId = retiredType.Id, IsActive = false }, // Under renovation forever
            new Room { RoomNumber = "402", RoomTypeId = presidential.Id, IsActive = false }, // Currently offline for maintenance
            new Room { RoomNumber = "502", RoomTypeId = penthouse.Id, IsActive = false } // Offline for deep biological sanitation
        };
        context.Rooms.AddRange(rooms);
        context.SaveChanges();

        // 4. Bookings
        var bookings = new List<Booking>
        {
            // Normal cases
            new Booking { GuestName = "Alice Smith", GuestEmail = "alice@example.com", CheckInDate = now.AddDays(-2), CheckOutDate = now.AddDays(2), BookingStatus = BookingStatus.CheckedIn, PaymentStatus = PaymentStatus.Pending, Origin = BookingOrigin.WalkIn, BookingRooms = new List<BookingRoom> { new BookingRoom { RoomTypeId = standard.Id, RoomId = rooms[0].Id, LockedInPrice = 100m } } },
            new Booking { GuestName = customer1.FirstName, GuestEmail = customer1.Email, UserId = customer1.Id, CheckInDate = now.AddDays(-1), CheckOutDate = now.AddDays(4), BookingStatus = BookingStatus.CheckedIn, PaymentStatus = PaymentStatus.Pending, Origin = BookingOrigin.RegisteredUser, BookingRooms = new List<BookingRoom> { new BookingRoom { RoomTypeId = deluxe.Id, RoomId = rooms[5].Id, LockedInPrice = 150m } } },
            new Booking { GuestName = "Bob Johnson", GuestEmail = "bob@example.com", CheckInDate = now.AddDays(5), CheckOutDate = now.AddDays(10), BookingStatus = BookingStatus.Booked, PaymentStatus = PaymentStatus.Pending, Origin = BookingOrigin.Guest, BookingRooms = new List<BookingRoom> { new BookingRoom { RoomTypeId = family.Id, RoomId = null, LockedInPrice = 250m } } },
            new Booking { GuestName = customer2.FirstName, GuestEmail = customer2.Email, UserId = customer2.Id, CheckInDate = now.AddDays(10), CheckOutDate = now.AddDays(14), BookingStatus = BookingStatus.Booked, PaymentStatus = PaymentStatus.Pending, Origin = BookingOrigin.RegisteredUser, BookingRooms = new List<BookingRoom> { new BookingRoom { RoomTypeId = presidential.Id, RoomId = null, LockedInPrice = 500m } } },
            new Booking { GuestName = "Charlie Davis", GuestEmail = "charlie@example.com", CheckInDate = now.AddDays(-10), CheckOutDate = now.AddDays(-5), BookingStatus = BookingStatus.CheckedOut, PaymentStatus = PaymentStatus.Paid, Origin = BookingOrigin.Guest, BookingRooms = new List<BookingRoom> { new BookingRoom { RoomTypeId = standard.Id, RoomId = rooms[1].Id, LockedInPrice = 90m } } },
            new Booking { GuestName = "David Lee", GuestEmail = "david@example.com", CheckInDate = now.AddDays(-20), CheckOutDate = now.AddDays(-15), BookingStatus = BookingStatus.CheckedOut, PaymentStatus = PaymentStatus.Paid, Origin = BookingOrigin.WalkIn, BookingRooms = new List<BookingRoom> { new BookingRoom { RoomTypeId = deluxe.Id, RoomId = rooms[6].Id, LockedInPrice = 140m } } },
            new Booking { GuestName = "Eve Adams", GuestEmail = "eve@example.com", CheckInDate = now.AddDays(1), CheckOutDate = now.AddDays(3), BookingStatus = BookingStatus.Cancelled, PaymentStatus = PaymentStatus.Pending, Origin = BookingOrigin.Guest, BookingRooms = new List<BookingRoom> { new BookingRoom { RoomTypeId = family.Id, RoomId = null, LockedInPrice = 250m } } },
            new Booking { GuestName = "Frank White", GuestEmail = "frank@example.com", CheckInDate = now.AddDays(-3), CheckOutDate = now.AddDays(1), BookingStatus = BookingStatus.Cancelled, PaymentStatus = PaymentStatus.Pending, Origin = BookingOrigin.Guest, BookingRooms = new List<BookingRoom> { new BookingRoom { RoomTypeId = standard.Id, RoomId = null, LockedInPrice = 100m } } }, // Simulating NoShow
            
            // Additional typical configurations
            new Booking { GuestName = customer3.FirstName, GuestEmail = customer3.Email, UserId = customer3.Id, CheckInDate = now.AddDays(2), CheckOutDate = now.AddDays(5), BookingStatus = BookingStatus.Booked, PaymentStatus = PaymentStatus.Paid, Origin = BookingOrigin.RegisteredUser, BookingRooms = new List<BookingRoom> { new BookingRoom { RoomTypeId = executive.Id, RoomId = rooms[8].Id, LockedInPrice = 350m } } },
            new Booking { GuestName = customer4.FirstName, GuestEmail = customer4.Email, UserId = customer4.Id, CheckInDate = now.AddDays(-3), CheckOutDate = now.AddDays(3), BookingStatus = BookingStatus.CheckedIn, PaymentStatus = PaymentStatus.Paid, Origin = BookingOrigin.RegisteredUser, BookingRooms = new List<BookingRoom> { new BookingRoom { RoomTypeId = studio.Id, RoomId = rooms[13].Id, LockedInPrice = 180m } } },

            // Edge cases
            // Starting today
            new Booking { GuestName = "Today Start", GuestEmail = "today1@hotel.com", CheckInDate = now.Date, CheckOutDate = now.Date.AddDays(2), BookingStatus = BookingStatus.CheckedIn, PaymentStatus = PaymentStatus.Pending, Origin = BookingOrigin.WalkIn, BookingRooms = new List<BookingRoom> { new BookingRoom { RoomTypeId = standard.Id, RoomId = rooms[2].Id, LockedInPrice = 100m } } },
            // Ending today
            new Booking { GuestName = "Today End", GuestEmail = "today2@hotel.com", CheckInDate = now.Date.AddDays(-3), CheckOutDate = now.Date, BookingStatus = BookingStatus.CheckedIn, PaymentStatus = PaymentStatus.Pending, Origin = BookingOrigin.WalkIn, BookingRooms = new List<BookingRoom> { new BookingRoom { RoomTypeId = deluxe.Id, RoomId = rooms[7].Id, LockedInPrice = 150m } } },
            // Long stay (30 days)
            new Booking { GuestName = "Long Stay", GuestEmail = "long@hotel.com", CheckInDate = now.Date.AddDays(-15), CheckOutDate = now.Date.AddDays(15), BookingStatus = BookingStatus.CheckedIn, PaymentStatus = PaymentStatus.Pending, Origin = BookingOrigin.WalkIn, BookingRooms = new List<BookingRoom> { new BookingRoom { RoomTypeId = standard.Id, RoomId = rooms[1].Id, LockedInPrice = 95m } } },
            // CheckedOut but UNPAID (Edge case - guest left without paying bill)
            new Booking { GuestName = "Runner", GuestEmail = "runner@hotel.com", CheckInDate = now.Date.AddDays(-30), CheckOutDate = now.Date.AddDays(-28), BookingStatus = BookingStatus.CheckedOut, PaymentStatus = PaymentStatus.Pending, Origin = BookingOrigin.Guest, BookingRooms = new List<BookingRoom> { new BookingRoom { RoomTypeId = standard.Id, RoomId = rooms[0].Id, LockedInPrice = 100m } } },
            // Cancelled but PAID (Edge case - needs refund)
            new Booking { GuestName = "Refund Plz", GuestEmail = "refund@hotel.com", CheckInDate = now.Date.AddDays(20), CheckOutDate = now.Date.AddDays(22), BookingStatus = BookingStatus.Cancelled, PaymentStatus = PaymentStatus.Paid, Origin = BookingOrigin.Guest, BookingRooms = new List<BookingRoom> { new BookingRoom { RoomTypeId = presidential.Id, RoomId = null, LockedInPrice = 500m } } },
            // Cancelled and Refunded
            new Booking { GuestName = "Refunded", GuestEmail = "refunded@hotel.com", CheckInDate = now.Date.AddDays(25), CheckOutDate = now.Date.AddDays(27), BookingStatus = BookingStatus.Cancelled, PaymentStatus = PaymentStatus.Refunded, Origin = BookingOrigin.Guest, BookingRooms = new List<BookingRoom> { new BookingRoom { RoomTypeId = presidential.Id, RoomId = null, LockedInPrice = 500m } } },
            // High-value whales
            new Booking { GuestName = "VVIP High Roller", GuestEmail = "whale@lux.com", CheckInDate = now.AddDays(-1), CheckOutDate = now.AddDays(6), BookingStatus = BookingStatus.CheckedIn, PaymentStatus = PaymentStatus.Paid, Origin = BookingOrigin.Guest, BookingRooms = new List<BookingRoom> { new BookingRoom { RoomTypeId = villa.Id, RoomId = rooms[16].Id, LockedInPrice = 1500m } } }
        };
        context.Bookings.AddRange(bookings);
        context.SaveChanges();

        // 5. Menu Items
        var menu = new List<MenuItem>
        {
            new MenuItem { Name = "Burger", Price = 15.0m, IsAvailable = true },
            new MenuItem { Name = "Pizza", Price = 20.0m, IsAvailable = true },
            new MenuItem { Name = "Soda", Price = 3.0m, IsAvailable = true },
            new MenuItem { Name = "Steak", Price = 45.0m, IsAvailable = false }, // Out of stock
            new MenuItem { Name = "Old Soup", Price = 5.0m, IsAvailable = false }, // Retired
            // Edge cases
            new MenuItem { Name = "Complimentary Water", Price = 0.0m, IsAvailable = true }, // Free item
            new MenuItem { Name = "White Truffle Caviar", Price = 500.0m, IsAvailable = true }, // Very expensive item
            new MenuItem { Name = "Salad", Price = 12.0m, IsAvailable = true },
            new MenuItem { Name = "Coffee", Price = 4.0m, IsAvailable = true },
            new MenuItem { Name = "Cheesecake", Price = 8.0m, IsAvailable = true },
            // Additional choices
            new MenuItem { Name = "Club Sandwich", Price = 14.5m, IsAvailable = true },
            new MenuItem { Name = "French Fries", Price = 6.0m, IsAvailable = true },
            new MenuItem { Name = "Lobster Thermidor", Price = 120.0m, IsAvailable = true },
            new MenuItem { Name = "Ice Cream Sundae", Price = 9.0m, IsAvailable = true },
            new MenuItem { Name = "Craft Beer", Price = 8.5m, IsAvailable = true }
        };
        context.MenuItems.AddRange(menu);
        context.SaveChanges();

        // 6. Food Orders
        context.FoodOrders.AddRange(
            // Normal orders
            new FoodOrder { BookingId = bookings[0].Id, OrderStatus = FoodOrderStatus.Delivered, GeneratedAt = now.AddDays(-1), OrderItems = new List<FoodOrderItem> { new FoodOrderItem { MenuItemId = menu[0].Id, Quantity = 2, PriceAtPurchase = 15.0m }, new FoodOrderItem { MenuItemId = menu[2].Id, Quantity = 2, PriceAtPurchase = 3.0m } } },
            new FoodOrder { BookingId = bookings[1].Id, OrderStatus = FoodOrderStatus.Preparing, GeneratedAt = now.AddMinutes(-30), OrderItems = new List<FoodOrderItem> { new FoodOrderItem { MenuItemId = menu[1].Id, Quantity = 1, PriceAtPurchase = 20.0m } } },
            new FoodOrder { BookingId = bookings[1].Id, OrderStatus = FoodOrderStatus.Pending, GeneratedAt = now.AddMinutes(-5), OrderItems = new List<FoodOrderItem> { new FoodOrderItem { MenuItemId = menu[2].Id, Quantity = 1, PriceAtPurchase = 3.0m } } },

            // Edge cases
            // Huge order (10+ items)
            new FoodOrder
            {
                BookingId = bookings[10].Id,
                OrderStatus = FoodOrderStatus.Pending,
                GeneratedAt = now.AddMinutes(-2),
                OrderItems = new List<FoodOrderItem> {
                new FoodOrderItem { MenuItemId = menu[0].Id, Quantity = 5, PriceAtPurchase = 15.0m },
                new FoodOrderItem { MenuItemId = menu[1].Id, Quantity = 5, PriceAtPurchase = 20.0m },
                new FoodOrderItem { MenuItemId = menu[2].Id, Quantity = 10, PriceAtPurchase = 3.0m }
            }
            },
            // Order that was just placed
            new FoodOrder { BookingId = bookings[11].Id, OrderStatus = FoodOrderStatus.Pending, GeneratedAt = now.AddDays(-1), OrderItems = new List<FoodOrderItem> { new FoodOrderItem { MenuItemId = menu[0].Id, Quantity = 1, PriceAtPurchase = 15.0m } } },
            // Order with free and expensive items
            new FoodOrder { BookingId = bookings[12].Id, OrderStatus = FoodOrderStatus.Delivered, GeneratedAt = now.AddDays(-5), OrderItems = new List<FoodOrderItem> { new FoodOrderItem { MenuItemId = menu[5].Id, Quantity = 2, PriceAtPurchase = 0.0m }, new FoodOrderItem { MenuItemId = menu[6].Id, Quantity = 1, PriceAtPurchase = 500.0m } } },
            new FoodOrder { BookingId = bookings[0].Id, OrderStatus = FoodOrderStatus.Delivered, GeneratedAt = now.AddDays(-2), OrderItems = new List<FoodOrderItem> { new FoodOrderItem { MenuItemId = menu[7].Id, Quantity = 1, PriceAtPurchase = 12.0m } } },
            new FoodOrder { BookingId = bookings[0].Id, OrderStatus = FoodOrderStatus.Delivered, GeneratedAt = now.AddDays(-1), OrderItems = new List<FoodOrderItem> { new FoodOrderItem { MenuItemId = menu[8].Id, Quantity = 2, PriceAtPurchase = 4.0m } } },
            new FoodOrder { BookingId = bookings[1].Id, OrderStatus = FoodOrderStatus.Preparing, GeneratedAt = now.AddMinutes(-45), OrderItems = new List<FoodOrderItem> { new FoodOrderItem { MenuItemId = menu[9].Id, Quantity = 1, PriceAtPurchase = 8.0m } } },
            new FoodOrder { BookingId = bookings[10].Id, OrderStatus = FoodOrderStatus.Delivered, GeneratedAt = now.AddHours(-1), OrderItems = new List<FoodOrderItem> { new FoodOrderItem { MenuItemId = menu[2].Id, Quantity = 3, PriceAtPurchase = 3.0m } } },

            // Ultra luxury room-service orders
            new FoodOrder
            {
                BookingId = bookings[16].Id,
                OrderStatus = FoodOrderStatus.Delivered,
                GeneratedAt = now.AddHours(-4),
                OrderItems = new List<FoodOrderItem> {
                new FoodOrderItem { MenuItemId = menu[12].Id, Quantity = 2, PriceAtPurchase = 120.0m },
                new FoodOrderItem { MenuItemId = menu[6].Id, Quantity = 2, PriceAtPurchase = 500.0m },
                new FoodOrderItem { MenuItemId = menu[14].Id, Quantity = 4, PriceAtPurchase = 8.5m }
            }
            }
        );

        // 8. Housekeeping
        context.HousekeepingTasks.AddRange(
            // Normal
            new Housekeeping { RoomId = rooms[0].Id, OriginType = HousekeepingOriginType.GuestRequested, Status = HousekeepingStatus.Completed, StartedAt = now.AddHours(-4), FinishedAt = now.AddHours(-1) },
            new Housekeeping { RoomId = rooms[5].Id, OriginType = HousekeepingOriginType.GuestRequested, Status = HousekeepingStatus.InProgress, StartedAt = now.AddMinutes(-15) },
            new Housekeeping { RoomId = rooms[6].Id, OriginType = HousekeepingOriginType.CheckoutAutomated, Status = HousekeepingStatus.Pending },

            // Edge cases
            // Multiple tasks for the same room
            new Housekeeping { RoomId = rooms[7].Id, OriginType = HousekeepingOriginType.CheckoutAutomated, Status = HousekeepingStatus.Pending },
            new Housekeeping { RoomId = rooms[7].Id, OriginType = HousekeepingOriginType.GuestRequested, Status = HousekeepingStatus.Pending },
            // Task for room without an active booking (e.g. deep cleaning after someone left)
            new Housekeeping { RoomId = rooms[11].Id, OriginType = HousekeepingOriginType.CheckoutAutomated, Status = HousekeepingStatus.Pending },
            new Housekeeping { RoomId = rooms[1].Id, OriginType = HousekeepingOriginType.CheckoutAutomated, Status = HousekeepingStatus.Completed, StartedAt = now.AddHours(-2), FinishedAt = now.AddHours(-1) },
            new Housekeeping { RoomId = rooms[2].Id, OriginType = HousekeepingOriginType.GuestRequested, Status = HousekeepingStatus.Completed, StartedAt = now.AddDays(-1), FinishedAt = now.AddDays(-1).AddHours(1) },
            new Housekeeping { RoomId = rooms[12].Id, OriginType = HousekeepingOriginType.CheckoutAutomated, Status = HousekeepingStatus.Pending },
            new Housekeeping { RoomId = rooms[13].Id, OriginType = HousekeepingOriginType.CheckoutAutomated, Status = HousekeepingStatus.InProgress, StartedAt = now.AddMinutes(-20) },
            new Housekeeping { RoomId = rooms[15].Id, OriginType = HousekeepingOriginType.CheckoutAutomated, Status = HousekeepingStatus.Pending },
            new Housekeeping { RoomId = rooms[19].Id, OriginType = HousekeepingOriginType.StaffRequested, Status = HousekeepingStatus.InProgress, StartedAt = now.AddHours(-3) }
        );

        // 8.5 Maintenance Tasks
        context.MaintenanceTasks.AddRange(
            // Normal
            new MaintenanceTask { RoomId = rooms[1].Id, OriginType = MaintenanceOriginType.SystemAutomated, Status = MaintenanceStatus.Completed, Description = "Fix leaking sink", StartedAt = now.AddDays(-1), FinishedAt = now.AddDays(-1).AddHours(2) },
            new MaintenanceTask { RoomId = rooms[2].Id, OriginType = MaintenanceOriginType.StaffRequested, Status = MaintenanceStatus.InProgress, Description = "Replace broken lightbulb", StartedAt = now.AddHours(-1) },
            new MaintenanceTask { RoomId = rooms[12].Id, OriginType = MaintenanceOriginType.GuestRequested, Status = MaintenanceStatus.Pending, Description = "AC not cooling properly" },

            // Edge cases
            // Multiple maintenance issues for one offline room
            new MaintenanceTask { RoomId = rooms[18].Id, OriginType = MaintenanceOriginType.StaffRequested, Status = MaintenanceStatus.InProgress, Description = "Major plumbing overhaul", StartedAt = now.AddDays(-2) },
            new MaintenanceTask { RoomId = rooms[18].Id, OriginType = MaintenanceOriginType.StaffRequested, Status = MaintenanceStatus.Pending, Description = "Repaint walls after plumbing fix" },
            new MaintenanceTask { RoomId = rooms[0].Id, OriginType = MaintenanceOriginType.SystemAutomated, Status = MaintenanceStatus.Completed, Description = "Filter replacement", StartedAt = now.AddDays(-5), FinishedAt = now.AddDays(-5).AddHours(1) },
            new MaintenanceTask { RoomId = rooms[5].Id, OriginType = MaintenanceOriginType.GuestRequested, Status = MaintenanceStatus.Completed, Description = "TV remote not working", StartedAt = now.AddDays(-3), FinishedAt = now.AddDays(-3).AddHours(1) },
            new MaintenanceTask { RoomId = rooms[6].Id, OriginType = MaintenanceOriginType.StaffRequested, Status = MaintenanceStatus.InProgress, Description = "Carpet cleaning", StartedAt = now.AddHours(-2) },
            new MaintenanceTask { RoomId = rooms[7].Id, OriginType = MaintenanceOriginType.GuestRequested, Status = MaintenanceStatus.Pending, Description = "Window won't close" },
            new MaintenanceTask { RoomId = rooms[14].Id, OriginType = MaintenanceOriginType.SystemAutomated, Status = MaintenanceStatus.Pending, Description = "Quarterly inspection" },
            new MaintenanceTask { RoomId = rooms[16].Id, OriginType = MaintenanceOriginType.GuestRequested, Status = MaintenanceStatus.Pending, Description = "Infinity pool deck-chair hinge cracked" }
        );

        // 9. Feedback
        context.Feedbacks.AddRange(
            // Normal
            new Feedback { BookingId = bookings[4].Id, Rating = 4, Comments = "Good, but AC was loud." },
            new Feedback { BookingId = bookings[5].Id, Rating = 5, Comments = "Perfect stay!" },

            // Edge cases
            // 1-star scathing review
            new Feedback { BookingId = bookings[13].Id, Rating = 1, Comments = "Absolutely terrible. Found a bug in the bed, room smelled like smoke, and the receptionist ignored us. Never coming back." },
            // No comments
            new Feedback { BookingId = bookings[14].Id, Rating = 3, Comments = "" },
            new Feedback { BookingId = bookings[0].Id, Rating = 5, Comments = "Great service!" },
            new Feedback { BookingId = bookings[1].Id, Rating = 4, Comments = "Nice room." },
            new Feedback { BookingId = bookings[10].Id, Rating = 2, Comments = "Too noisy." },
            new Feedback { BookingId = bookings[11].Id, Rating = 5, Comments = "Excellent food." },
            new Feedback { BookingId = bookings[12].Id, Rating = 4, Comments = "Comfortable bed." },
            new Feedback { BookingId = bookings[13].Id, Rating = 3, Comments = "Average experience." },
            new Feedback { BookingId = bookings[9].Id, Rating = 5, Comments = "Studio room setup is spectacular for programmers working remotely." }
        );

        // 10. Amenities
        var amenities = new List<Amenity>
        {
            new Amenity { Name = "Swimming Pool", Description = "Olympic-sized heated pool", Price = 25.0m, IsAvailable = true },
            new Amenity { Name = "Sauna", Description = "Relaxing steam room", Price = 15.0m, IsAvailable = true },
            new Amenity { Name = "Gym", Description = "Fully equipped fitness center", Price = 10.0m, IsAvailable = true },
            new Amenity { Name = "Spa Treatment", Description = "1-hour massage session", Price = 40.0m, IsAvailable = true },
            new Amenity { Name = "Business Center", Description = "Quiet workspace with high-speed internet", Price = 20.0m, IsAvailable = true },
            new Amenity { Name = "Airport Shuttle", Description = "Round trip shuttle service", Price = 50.0m, IsAvailable = true },
            new Amenity { Name = "Breakfast Buffet", Description = "All you can eat", Price = 25.0m, IsAvailable = true },
            new Amenity { Name = "Parking", Description = "Valet parking", Price = 30.0m, IsAvailable = true },
            new Amenity { Name = "Laundry", Description = "Next day service", Price = 15.0m, IsAvailable = true },
            new Amenity { Name = "Pet Fee", Description = "Bring your dog", Price = 75.0m, IsAvailable = true },
            // Ultra VIP Extras
            new Amenity { Name = "Private Chef Experience", Description = "In-room custom dining", Price = 450.0m, IsAvailable = true },
            new Amenity { Name = "Helicopter Pad Access", Description = "Exclusive landing coordination", Price = 800.0m, IsAvailable = true }
        };
        context.Amenities.AddRange(amenities);
        context.SaveChanges();

        // 11. Booking Amenities
        context.BookingAmenities.AddRange(
            // Normal
            new BookingAmenity { BookingId = bookings[0].Id, AmenityId = amenities[0].Id, PriceAtPurchase = 25.0m },
            new BookingAmenity { BookingId = bookings[0].Id, AmenityId = amenities[1].Id, PriceAtPurchase = 15.0m },
            new BookingAmenity { BookingId = bookings[1].Id, AmenityId = amenities[2].Id, PriceAtPurchase = 10.0m },
            new BookingAmenity { BookingId = bookings[1].Id, AmenityId = amenities[3].Id, PriceAtPurchase = 40.0m },

            // Edge case: Subscribed to ALL amenities
            new BookingAmenity { BookingId = bookings[10].Id, AmenityId = amenities[0].Id, PriceAtPurchase = 25.0m },
            new BookingAmenity { BookingId = bookings[10].Id, AmenityId = amenities[1].Id, PriceAtPurchase = 15.0m },
            new BookingAmenity { BookingId = bookings[10].Id, AmenityId = amenities[2].Id, PriceAtPurchase = 10.0m },
            new BookingAmenity { BookingId = bookings[10].Id, AmenityId = amenities[3].Id, PriceAtPurchase = 40.0m },
            new BookingAmenity { BookingId = bookings[10].Id, AmenityId = amenities[4].Id, PriceAtPurchase = 20.0m },

            // High tier package assignments
            new BookingAmenity { BookingId = bookings[16].Id, AmenityId = amenities[10].Id, PriceAtPurchase = 450.0m },
            new BookingAmenity { BookingId = bookings[16].Id, AmenityId = amenities[11].Id, PriceAtPurchase = 800.0m },
            new BookingAmenity { BookingId = bookings[16].Id, AmenityId = amenities[7].Id, PriceAtPurchase = 30.0m }
        );

        // 12. Receipts for Paid bookings
        context.Receipts.AddRange(
            // bookings[4] = CheckedOut Paid (Standard room, 5 nights * 90 = 450)
            new Receipt { BookingId = bookings[4].Id, AmountPaid = 450.0m, PaymentMethod = "Credit Card", TransactionId = "TXN-001", PaidAt = now.AddDays(-5) },
            // bookings[5] = CheckedOut Paid (Deluxe room, 5 nights * 140 = 700)
            new Receipt { BookingId = bookings[5].Id, AmountPaid = 700.0m, PaymentMethod = "Cash", TransactionId = "TXN-002", PaidAt = now.AddDays(-15) },
            // bookings[14] = Cancelled Paid (needs refund) (Presidential room, 500)
            new Receipt { BookingId = bookings[14].Id, AmountPaid = 500.0m, PaymentMethod = "Credit Card", TransactionId = "TXN-003", PaidAt = now.AddDays(15) },
            // bookings[15] = Cancelled Refunded
            new Receipt { BookingId = bookings[15].Id, AmountPaid = -500.0m, PaymentMethod = "Credit Card", TransactionId = "REF-001", PaidAt = now.AddDays(16) },
            new Receipt { BookingId = bookings[10].Id, AmountPaid = 200.0m, PaymentMethod = "Cash", TransactionId = "TXN-004", PaidAt = now.AddDays(-1) },
            new Receipt { BookingId = bookings[11].Id, AmountPaid = 450.0m, PaymentMethod = "Credit Card", TransactionId = "TXN-005", PaidAt = now.AddDays(-2) },
            new Receipt { BookingId = bookings[12].Id, AmountPaid = 2850.0m, PaymentMethod = "Bank Transfer", TransactionId = "TXN-006", PaidAt = now.AddDays(-10) },
            new Receipt { BookingId = bookings[0].Id, AmountPaid = 50.0m, PaymentMethod = "Credit Card", TransactionId = "TXN-007", PaidAt = now.AddDays(-1) },
            new Receipt { BookingId = bookings[1].Id, AmountPaid = 100.0m, PaymentMethod = "Cash", TransactionId = "TXN-008", PaidAt = now.AddDays(-1) },
            new Receipt { BookingId = bookings[2].Id, AmountPaid = 1250.0m, PaymentMethod = "Credit Card", TransactionId = "TXN-009", PaidAt = now.AddDays(5) },

            // Whale Booking Settlement (7 nights * 1500 = 10500 + amenities + premium food orders)
            new Receipt { BookingId = bookings[16].Id, AmountPaid = 12500.0m, PaymentMethod = "Bank Transfer", TransactionId = "TXN-VIP-777", PaidAt = now.AddDays(-1) }
        );

        context.SaveChanges();
    }
}