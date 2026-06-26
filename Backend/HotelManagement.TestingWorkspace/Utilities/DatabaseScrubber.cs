using HotelManagement.DAL.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HotelManagement.TestingWorkspace.Utilities;

public static class DatabaseScrubber
{
    /// <summary>
    /// WARNING: This forcefully deletes all data from the database. 
    /// This is strictly for the isolated TestingWorkspace and should ONLY be called manually by the user.
    /// </summary>
    public static void ScrubDatabase(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // We use raw SQL to truncate tables and CASCADE to handle foreign keys
        context.Database.ExecuteSqlRaw(@"
            TRUNCATE TABLE ""Feedbacks"", ""FoodOrderItems"", ""FoodOrders"", ""MenuItems"", 
                           ""Housekeepings"", ""MaintenanceRequests"", ""Bookings"", 
                           ""Rooms"", ""RoomTypes"" RESTART IDENTITY CASCADE;
        ");
    }
}
