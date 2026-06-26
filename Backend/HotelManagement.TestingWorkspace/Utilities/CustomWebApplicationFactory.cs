using HotelManagement.DAL.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HotelManagement.TestingWorkspace.Utilities;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            var dbName = "HotelManagementDb_Testing_" + Guid.NewGuid().ToString("N");
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql($"Host=localhost;Database={dbName};Username=peewee;Password=");
            });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<ApplicationDbContext>();
            var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory>>();

            db.Database.EnsureDeleted();
            db.Database.Migrate();

            try
            {
                DatabaseSeeder.SeedTestData(sp);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding the in-memory test database. Error: {Message}", ex.Message);
            }
        });
    }
}
