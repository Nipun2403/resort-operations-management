using HotelManagement.DAL.Context;
using HotelManagement.DAL.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace HotelManagement.API.Utilities;

public static class MainDatabaseSeeder
{
  public static void Seed(IServiceProvider serviceProvider)
  {
    using var scope = serviceProvider.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (context.Users.Any()) return;

    var admin = new User
    {
      Email = "admin@hotel.com",
      PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
      Role = "Admin",
      FirstName = "System",
      LastName = "Admin",
      IsActive = true
    };

    context.Users.Add(admin);
    context.SaveChanges();
  }
}
