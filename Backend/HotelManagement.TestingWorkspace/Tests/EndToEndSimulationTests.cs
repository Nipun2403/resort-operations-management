using System.Net.Http.Json;
using HotelManagement.BLL.DTOs;
using HotelManagement.TestingWorkspace.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace HotelManagement.TestingWorkspace.Tests;

public class EndToEndSimulationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly E2ETestLogger _logger;
    private readonly ITestOutputHelper _output;

    public EndToEndSimulationTests(CustomWebApplicationFactory factory, ITestOutputHelper output)
    {
        _client = factory.CreateClient();
        _output = output;
        
        // Ensure isolation by resolving to the project root of the testing workspace
        var workspaceRoot = Directory.GetCurrentDirectory(); 
        // We will output to the main solution directory to easily view the artifacts
        var solutionRoot = Path.Combine(workspaceRoot, "../../../../../"); 
        _logger = new E2ETestLogger(solutionRoot);
    }

    [Fact]
    public async Task ExecuteMasterOrchestrator()
    {
        _output.WriteLine("Starting Automated E2E Orchestrator...");

        // 1. Authenticate as Admin to get tokens for setup
        var adminToken = await GetAuthToken("admin@test.com", "Admin@123");

        // Provision Front Desk User for the story
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
        var fdDto = new StaffRegisterRequestDTO { Email = "fd@test.com", Password = "Password123", FirstName = "Front", LastName = "Desk", Role = "FrontDesk" };
        await _client.PostAsJsonAsync("/api/v1/staff", fdDto);
        var frontDeskToken = await GetAuthToken("fd@test.com", "Password123");

        // PHASE A: SEEDING 10+ RECORDS & EDGE CASES
        await ExecuteSeedingPhase(adminToken);

        // PHASE B: CHRONOLOGICAL STATE MUTATION (The Story)
        await ExecuteChronologicalStory(frontDeskToken);

        // PHASE C: COVERAGE SWEEP
        await ExecuteCoverageSweep(adminToken);

        _output.WriteLine("Orchestrator Finished Successfully. Please view seed_experiment.md and experiment_testing.md.");
    }

    private async Task ExecuteSeedingPhase(string adminToken)
    {
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Seed 10 Room Types (Edge cases: negative prices, massive capacities)
        for (int i = 1; i <= 10; i++)
        {
            var dto = new RoomTypeDTO 
            { 
                Name = $"Test Suite {i}", 
                BasePrice = i == 5 ? -100 : 150 + (i * 10), // Edge case: invalid price on 5
                MaxOccupancy = i == 9 ? 1000 : 2,            // Edge case: massive capacity
                Description = $"Suite number {i}",
                BedConfiguration = new Dictionary<string, int> { { "King", 1 } }
            };

            var response = await _client.PostAsJsonAsync("/api/v1/room-types", dto);
            _logger.LogSeedData("RoomType", System.Text.Json.JsonSerializer.Serialize(dto), response.StatusCode.ToString(), 
                response.IsSuccessStatusCode ? "Successfully created room type." : await response.Content.ReadAsStringAsync());
        }

        // Fetch valid room type ID
        var roomTypesResponse = await _client.GetFromJsonAsync<List<dynamic>>("/api/v1/room-types");
        var validTypeId = roomTypesResponse?.FirstOrDefault()?.GetProperty("id").GetInt32() ?? 1;

        // Seed 20 Rooms (Edge cases: duplicate numbers)
        for (int i = 101; i <= 120; i++)
        {
            var dto = new CreateUpdateRoomDTO { RoomNumber = i.ToString(), RoomTypeId = validTypeId };
            if (i == 115) dto.RoomNumber = "101"; // Edge case: duplicate room number

            var response = await _client.PostAsJsonAsync("/api/v1/rooms", dto);
            _logger.LogSeedData("Room", System.Text.Json.JsonSerializer.Serialize(dto), response.StatusCode.ToString(), 
                response.IsSuccessStatusCode ? $"Successfully created physical room {dto.RoomNumber}." : await response.Content.ReadAsStringAsync());
        }

        // Seed 15 Menu Items
        for (int i = 1; i <= 15; i++)
        {
            var dto = new MenuItemDTO { Name = $"Test Burger {i}", Price = 10 + i, Category = "Food", IsAvailable = true };
            var response = await _client.PostAsJsonAsync("/api/v1/menu-items", dto);
            _logger.LogSeedData("MenuItem", System.Text.Json.JsonSerializer.Serialize(dto), response.StatusCode.ToString(), 
                response.IsSuccessStatusCode ? "Successfully created menu item." : await response.Content.ReadAsStringAsync());
        }
    }

    private async Task ExecuteChronologicalStory(string frontDeskToken)
    {
        // 1. Guest Books Anonymously
        _client.DefaultRequestHeaders.Authorization = null; // Anonymous
        var bookReq = new CreateBookingRequestDTO 
        { 
            RoomTypeIds = new List<int> { 1 }, 
            CheckInDate = DateTime.UtcNow.AddDays(1), 
            CheckOutDate = DateTime.UtcNow.AddDays(3),
            GuestName = "Automated Tester",
            GuestEmail = "auto@test.com"
        };
        
        var bookRes = await _client.PostAsJsonAsync("/api/v1/bookings", bookReq);
        var booking = await bookRes.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        int bookingId = booking.GetProperty("id").GetInt32();

        _logger.LogExperimentStep(DateTime.UtcNow.ToString("T"), "Anonymous Guest", "Book Room", "POST /api/v1/bookings",
            System.Text.Json.JsonSerializer.Serialize(bookReq), "201 Created", bookRes.StatusCode.ToString(), "Created Booking #" + bookingId);

        // 2. Front Desk Assigns Room & Checks In
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", frontDeskToken);
        var assignRes = await _client.PatchAsJsonAsync($"/api/v1/bookings/{bookingId}/room", 1);
        _logger.LogExperimentStep(DateTime.UtcNow.ToString("T"), "Front Desk", "Assign Room", $"PATCH /api/v1/bookings/{bookingId}/room",
            "1", "200 OK", assignRes.StatusCode.ToString(), $"Assigned Room 1 to Booking {bookingId}");

        var checkInRes = await _client.PatchAsJsonAsync($"/api/v1/bookings/{bookingId}", new UpdateBookingDTO { BookingStatus = DAL.Enums.BookingStatus.CheckedIn });
        _logger.LogExperimentStep(DateTime.UtcNow.ToString("T"), "Front Desk", "Check In Guest", $"PATCH /api/v1/bookings/{bookingId}",
            "{ status: CheckedIn }", "200 OK", checkInRes.StatusCode.ToString(), $"Booking {bookingId} status changed to CheckedIn");

        // 3. Guest Orders Food
        var orderReq = new CreateFoodOrderDTO { BookingId = bookingId, Items = new List<CreateFoodOrderItemDTO> { new CreateFoodOrderItemDTO { MenuItemId = 1, Quantity = 2 } } };
        var orderRes = await _client.PostAsJsonAsync("/api/v1/orders", orderReq);
        _logger.LogExperimentStep(DateTime.UtcNow.ToString("T"), "Guest via FrontDesk", "Order Food", "POST /api/v1/orders",
            System.Text.Json.JsonSerializer.Serialize(orderReq), "200 OK", orderRes.StatusCode.ToString(), "Created food order and attached to folio");

        // 4. Checkout
        var checkOutRes = await _client.PatchAsJsonAsync($"/api/v1/bookings/{bookingId}", new UpdateBookingDTO { BookingStatus = DAL.Enums.BookingStatus.CheckedOut });
        _logger.LogExperimentStep(DateTime.UtcNow.ToString("T"), "Front Desk", "Check Out Guest", $"PATCH /api/v1/bookings/{bookingId}",
            "{ status: CheckedOut }", "200 OK", checkOutRes.StatusCode.ToString(), "Generated Folio, changed status to CheckedOut, freed room.");
    }

    private async Task ExecuteCoverageSweep(string adminToken)
    {
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        // Rapid fire GET requests to ensure endpoints return 200/400 (not 500 or 404)
        var endpoints = new[] 
        { 
            "/api/v1/bookings", 
            "/api/v1/rooms", 
            "/api/v1/room-types", 
            "/api/v1/staff", 
            "/api/v1/menu-items", 
            "/api/v1/orders",
            "/api/v1/housekeeping",
            "/api/v1/feedback"
        };

        foreach (var ep in endpoints)
        {
            var res = await _client.GetAsync(new Uri(ep, UriKind.Relative));
            _logger.LogExperimentStep(DateTime.UtcNow.ToString("T"), "Automated Sweeper", "Coverage Ping", $"GET {ep}", "null", "200 OK (or 400)", res.StatusCode.ToString(), "No specific data modified.");
            Assert.True(res.IsSuccessStatusCode || res.StatusCode == System.Net.HttpStatusCode.BadRequest);
        }
    }

    private async Task<string> GetAuthToken(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequestDTO { Email = email, Password = password });
        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        return data.GetProperty("token").GetString() ?? throw new Exception("Token missing");
    }
}
