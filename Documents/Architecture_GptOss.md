# Architecture and Flow

## 1. Executive Summary
The Hotel Management System backend is a high-performance monolithic Web API developed using **ASP.NET Core 10**. The system implements an **N-Tier Architecture** combined with **Domain-Driven Design (DDD)** principles to ensure a strict separation of concerns, high maintainability, and robust testability.

The architectural flow is unidirectional:
`Client` → `Presentation Layer (API)` → `Business Logic Layer (BLL)` → `Repository Layer` → `Data Access Layer (DAL)` → `PostgreSQL Database`.

---

## 2. N-Tier Architecture Breakdown

The solution is divided into distinct class libraries (layers) to enforce separation of concerns, orchestrated entirely via the Dependency Injection (DI) container in `HotelManagement.API/Program.cs`.

```mermaid
flowchart TD
    subgraph Presentation ["HotelManagement.API (Presentation Layer)"]
        Controllers["API Controllers"]
        Middlewares["GlobalExceptionMiddleware"]
        Hubs["SignalR NotificationHub"]
    end

    subgraph Business ["HotelManagement.BLL (Business Logic Layer)"]
        Services["Domain Services (IBookingService)"]
        DTOs["Data Transfer Objects (DTOs)"]
        AutoMapper["AutoMapper Profiles"]
    end

    subgraph Repository ["HotelManagement.Repository (Repository Layer)"]
        GenericRepo["GenericRepository<T>"]
        SpecificRepos["Specific Repositories (IRoomRepository)"]
    end

    subgraph DataAccess ["HotelManagement.DAL (Data Access Layer)"]
        DbContext["ApplicationDbContext"]
        Entities["EF Core Entities"]
    end

    DB[(PostgreSQL Database)]

    Client([Web / Mobile Client]) -->|HTTP JSON| Controllers
    Client -->|WebSocket| Hubs
    
    Controllers -->|Injects Interface| Services
    Services -->|AutoMapper Maps| DTOs
    Services -->|Injects Interface| SpecificRepos
    SpecificRepos -->|Inherits| GenericRepo
    SpecificRepos -->|Uses| DbContext
    DbContext -->|EF Core LINQ| Entities
    Entities -->|PostgreSQL Driver| DB
```

---

*This document will be continuously enriched with detailed code references, line numbers, and additional diagrams as we parse each backend source file.*

## 3. Detailed Backend Code Overview

### 3.1 Presentation Layer – Controllers

#### File: [Backend/HotelManagement.API/Controllers/AmenitiesController.cs](./Backend/HotelManagement.API/Controllers/AmenitiesController.cs) : lines 1-103
```csharp
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/v1/amenities")]
public class AmenitiesController : ControllerBase
{
    private readonly IAmenityService _amenityService;

    public AmenitiesController(IAmenityService amenityService)
    {
        _amenityService = amenityService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAmenities(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchQuery = null,      // NEW
        [FromQuery] string? sortBy = null,           // NEW
        [FromQuery] bool sortDescending = false)     // NEW
    {
        pageSize = Math.Min(pageSize, 100);
        var amenities = await _amenityService.GetAllAmenitiesAsync(
            pageNumber,
            pageSize,
            searchQuery,
            sortBy,
            sortDescending);
        return Ok(amenities);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,FrontDesk,RegisteredUser")]
    public async Task<IActionResult> GetAmenity(int id)
    {
        var amenity = await _amenityService.GetAmenityByIdAsync(id);
        if (amenity == null) return NotFound("Amenity not found.");
        return Ok(amenity);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateAmenity([FromBody] CreateUpdateAmenityDTO request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var amenity = await _amenityService.CreateAmenityAsync(request);
        return CreatedAtAction(nameof(GetAmenity), new { id = amenity.Id }, amenity);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateAmenity(int id, [FromBody] CreateUpdateAmenityDTO request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            await _amenityService.UpdateAmenityAsync(id, request);
            return Ok(new { Message = "Amenity updated successfully." });
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateAmenityStatus(int id, [FromQuery] bool isAvailable)
    {
        try
        {
            await _amenityService.UpdateAmenityStatusAsync(id, isAvailable);
            return Ok(new { Message = $"Amenity availability updated to {isAvailable}." });
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
```

#### File: [Backend/HotelManagement.API/Controllers/AnalyticsController.cs](./Backend/HotelManagement.API/Controllers/AnalyticsController.cs) : lines 1-32
```csharp
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using HotelManagement.DAL.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = UserRoles.Admin)]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet]
    public async Task<ActionResult<AnalyticsDashboardDTO>> GetDashboardMetrics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
        {
            return BadRequest(new { message = "startDate cannot be after endDate." });
        }

        var metrics = await _analyticsService.GetDashboardMetricsAsync(startDate, endDate);
        return Ok(metrics);
    }
}
```

#### File: [Backend/HotelManagement.API/Controllers/AuthController.cs](./Backend/HotelManagement.API/Controllers/AuthController.cs) : lines 1-59
```csharp
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _authService.RegisterAsync(request);
        if (!result.Success) return Conflict(result.Message);

        return Ok(new { result.Message });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _authService.LoginAsync(request);
        if (!result.Success) return Unauthorized(result.Message);

        return Ok(new 
        { 
            result.Token, 
            result.Role,
            result.FirstName,
            result.LastName
        });
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult GetMe()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value });
        var identity = User.Identity;
        return Ok(new { 
            IsAuthenticated = identity?.IsAuthenticated,
            Name = identity?.Name,
            Claims = claims
        });
    }
}
```

#### File: [Backend/HotelManagement.API/Controllers/BookingsController.cs](./Backend/HotelManagement.API/Controllers/BookingsController.cs) : lines 1-195
```csharp
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using HotelManagement.DAL.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/v1/bookings")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly IBillingService _billingService;
    private readonly IAmenityService _amenityService;

    public BookingsController(
        IBookingService bookingService,
        IBillingService billingService,
        IAmenityService amenityService)
    {
        _bookingService = bookingService;
        _billingService = billingService;
        _amenityService = amenityService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,FrontDesk,RegisteredUser")]
    public async Task<IActionResult> GetBookings(
        [FromQuery] string? status,
        [FromQuery] string? guestQuery,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        pageSize = Math.Min(pageSize, 100);
        try
        {
            var bookings = await _bookingService.GetBookingsAsync(status, guestQuery, pageNumber, pageSize, sortBy, sortDescending);
            return Ok(bookings);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,FrontDesk")]
    public async Task<IActionResult> GetBooking(int id)
    {
        var booking = await _bookingService.GetBookingByIdAsync(id);
        if (booking == null) return NotFound("Booking not found.");

        return Ok(booking);
    }

    [HttpPost]
    [AllowAnonymous]
    [ServiceFilter(typeof(HotelManagement.API.Filters.IdempotentAttribute))]
    public async Task<IActionResult> BookRoom([FromBody] CreateBookingRequestDTO request)
    {
        try
        {
            var booking = await _bookingService.CreateBookingAsync(request);
            return CreatedAtAction(nameof(GetBooking), new { id = booking.Id }, booking);
        }
        catch (Exception ex)
        {
            if (ex is KeyNotFoundException || ex is UnauthorizedAccessException) throw;
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("{id}/extend-stay")]
    [Authorize(Roles = "FrontDesk,Admin")]
    public async Task<IActionResult> UpdateBooking(int id, [FromBody] UpdateBookingDTO dto)
    {
        try
        {
            // Only trigger extension if the date was actually sent
            if (dto.CheckOutDate.HasValue)
            {
                await _bookingService.ExtendStayAsync(id, dto.CheckOutDate.Value);
            }

            // Only trigger status change if the status was actually sent
            if (dto.BookingStatus.HasValue)
            {
                if (dto.BookingStatus.Value == BookingStatus.CheckedIn ||
                    dto.BookingStatus.Value == BookingStatus.CheckedOut)
                    return BadRequest("Use the explicit /checkin and /checkout endpoints.");

                await _bookingService.UpdateBookingStatusAsync(id, dto.BookingStatus.Value);
            }

            return Ok(new { Message = "Booking updated successfully." });
        }
        catch (ArgumentException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            if (ex is KeyNotFoundException || ex is UnauthorizedAccessException) throw;
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/checkin")]
    [Authorize(Roles = "FrontDesk,Admin")]
    public async Task<IActionResult> CheckIn(int id)
    {
        try
        {
            var booking = await _bookingService.CheckInGuestAsync(id);
            return Ok(booking);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/checkout")]
    [Authorize(Roles = "FrontDesk,Admin")]
    public async Task<IActionResult> CheckOut(int id)
    {
        try
        {
            var finalFolio = await _bookingService.UnifiedCheckoutAsync(id);
            return Ok(new { Message = "Checkout successful.", Folio = finalFolio });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}/cancel")]
    [Authorize(Roles = "RegisteredUser,FrontDesk,Admin")]
    public async Task<IActionResult> CancelBooking(int id)
    {
        try
        {
            await _bookingService.CancelBookingAsync(id);
            return Ok(new { Message = "Booking successfully cancelled." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            if (ex is KeyNotFoundException || ex is UnauthorizedAccessException) throw;
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/amenities")]
    [Authorize(Roles = "FrontDesk,Admin,RegisteredUser")]
    public async Task<IActionResult> SubscribeAmenity(int id, [FromBody] SubscribeAmenityDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            await _amenityService.SubscribeAsync(id, dto.AmenityId);
            return Ok(new { Message = "Amenity subscribed successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            if (ex is KeyNotFoundException || ex is UnauthorizedAccessException) throw;
            return BadRequest(ex.Message);
        }
    }
}
```

### 3.2 Presentation Layer – Middleware & Filters

#### File: [Backend/HotelManagement.API/Middleware/GlobalExceptionMiddleware.cs](./Backend/HotelManagement.API/Middleware/GlobalExceptionMiddleware.cs) : lines 1-65
```csharp
using System.Net;
using System.Text.Json;

namespace HotelManagement.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception was intercepted by GlobalExceptionMiddleware.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        // Default to 500 Internal Server Error
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        var message = "A critical internal fault occurred. Our engineers have been notified.";

        // Middleware Exception Interception Matrix Mapping
        if (exception is ArgumentException || exception is InvalidOperationException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            message = exception.Message; // Safe to expose
        }
        else if (exception is UnauthorizedAccessException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            message = "You are not authorized to access this resource.";
        }
        else if (exception is KeyNotFoundException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            message = "The requested resource was not found in the database.";
        }

        var response = new
        {
            statusCode = context.Response.StatusCode,
            message = message,
            errorType = exception.GetType().Name
        };

        var json = JsonSerializer.Serialize(response);
        return context.Response.WriteAsync(json);
    }
}
```

#### File: [Backend/HotelManagement.API/Filters/IdempotentAttribute.cs](./Backend/HotelManagement.API/Filters/IdempotentAttribute.cs) : lines 1-57
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using HotelManagement.DAL.Context;
using HotelManagement.DAL.Entities;

namespace HotelManagement.API.Filters;

public sealed class IdempotentAttribute : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue("X-Idempotency-Key", out var idempotencyKey))
        {
            await next();
            return;
        }

        var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
        var keyString = idempotencyKey.ToString();

        if (await dbContext.IdempotentRequests.FindAsync(keyString) != null)
        {
            context.Result = new ConflictObjectResult(new { Message = "Duplicate request detected." });
            return;
        }

        var executedContext = await next();

        if (executedContext.Exception == null && executedContext.Result is ObjectResult objectResult)
        {
            // Only cache if it was a successful request (e.g. 200/201)
            var statusCode = objectResult.StatusCode;
            if (statusCode == null || (statusCode >= 200 && statusCode < 300))
            {
                dbContext.IdempotentRequests.Add(new IdempotentRequest 
                { 
                    IdempotencyKey = keyString, 
                    Path = context.HttpContext.Request.Path 
                });
                await dbContext.SaveChangesAsync();
            }
        }
        else if (executedContext.Exception == null && executedContext.Result is StatusCodeResult statusCodeResult)
        {
            if (statusCodeResult.StatusCode >= 200 && statusCodeResult.StatusCode < 300)
            {
                dbContext.IdempotentRequests.Add(new IdempotentRequest 
                { 
                    IdempotencyKey = keyString, 
                    Path = context.HttpContext.Request.Path 
                });
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
```

### 3.3 Presentation Layer – SignalR Hub

#### File: [Backend/HotelManagement.API/Hubs/NotificationHub.cs](./Backend/HotelManagement.API/Hubs/NotificationHub.cs) : lines 1-35
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace HotelManagement.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        // Get user role from JWT claims
        var role = Context.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        
        if (!string.IsNullOrEmpty(role))
        {
            // Group connections by Role (e.g. "HousekeepingGroup", "MaintenanceGroup")
            await Groups.AddToGroupAsync(Context.ConnectionId, $"{role}Group");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var role = Context.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        
        if (!string.IsNullOrEmpty(role))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{role}Group");
        }

        await base.OnDisconnectedAsync(exception);
    }
}
```

### 3.4 Presentation Layer – Services (Cross‑Cutting)

#### File: [Backend/HotelManagement.API/Services/CurrentUserService.cs](./Backend/HotelManagement.API/Services/CurrentUserService.cs) : lines 1-43
```csharp
using System.Security.Claims;
using HotelManagement.BLL.Interfaces;
using HotelManagement.DAL.Context;
using Microsoft.AspNetCore.Http;

namespace HotelManagement.API.Services;

public class CurrentUserService : ICurrentUserService, IAuditUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetUserEmail()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.Name;
    }

    public string? GetUserName()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal == null) return null;

        var firstName = principal.FindFirstValue(ClaimTypes.GivenName);
        var lastName = principal.FindFirstValue(ClaimTypes.Surname);
        
        if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName)) return null;
        return $"{firstName} {lastName}".Trim();
    }

    public bool IsInRole(string role)
    {
        return _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
    }

    public bool IsAuthenticated()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }
}
```

---

*The sections above provide a line‑by‑line view of the Presentation Layer source files, each linked to its location in the repository. Subsequent sections will cover the Business Logic Layer, Repository Layer, and Data Access Layer in similar exhaustive detail.*
