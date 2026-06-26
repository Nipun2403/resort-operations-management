using HotelManagement.DAL.Context;
using HotelManagement.DAL.Entities;
using HotelManagement.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HotelManagement.TestingWorkspace.Utilities;

public static class DatabaseSeeder
{
    public static void SeedTestData(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Only seed if empty
        if (context.Rooms.Any()) return; 

        var adminUser = new User
        {
            Email = "admin@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            FirstName = "Test",
            LastName = "Admin",
            Role = "Admin",
            IsActive = true
        };
        context.Users.Add(adminUser);
        context.SaveChanges();

        var roomType1 = new RoomType { Name = "Deluxe Suite", BasePrice = 150m, MaxOccupancy = 2 };
        var roomType2 = new RoomType { Name = "Presidential", BasePrice = 500m, MaxOccupancy = 4 };
        context.RoomTypes.AddRange(roomType1, roomType2);
        context.SaveChanges();

        var room1 = new Room { RoomNumber = "101", RoomTypeId = roomType1.Id };
        var room2 = new Room { RoomNumber = "102", RoomTypeId = roomType2.Id };
        context.Rooms.AddRange(room1, room2);
        context.SaveChanges();

        var booking = new Booking
        {
            GuestName = "Test Guest",
            GuestEmail = "test@example.com",
            RoomId = room1.Id,
            CheckInDate = DateTime.UtcNow.AddDays(-2),
            CheckOutDate = DateTime.UtcNow.AddDays(2),
            BookingStatus = BookingStatus.CheckedIn
        };
        context.Bookings.Add(booking);
        context.SaveChanges();

        context.HousekeepingTasks.Add(new Housekeeping
        {
            RoomId = room1.Id,
            OriginType = HousekeepingOriginType.GuestRequested,
            Status = HousekeepingStatus.Completed
        });

        context.Feedbacks.Add(new Feedback { BookingId = booking.Id, Rating = 5, Comments = "Excellent!" });
        context.SaveChanges();
    }
}
