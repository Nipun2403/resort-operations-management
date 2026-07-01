using HotelManagement.DAL.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using HotelManagement.API.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using HotelManagement.BLL.Interfaces;
using HotelManagement.BLL.Services;
using HotelManagement.Repository.Implementations;
using HotelManagement.Repository.Interfaces;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

#region Logger Configuration
builder.Host.UseSerilog((context, configuration) =>
    configuration.MinimumLevel.Debug()
                 .WriteTo.Console()
                 .WriteTo.File("logs/hotel-management-log-.txt", rollingInterval: RollingInterval.Day));
#endregion

#region Globalization & Culture Setup
var cultureInfo = new System.Globalization.CultureInfo("en-GB");
cultureInfo.DateTimeFormat.ShortDatePattern = "dd-MM-yyyy";
cultureInfo.DateTimeFormat.DateSeparator = "-";
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
#endregion

// Add services to the container.

#region Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
#endregion

#region Repositories
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IRoomTypeRepository, RoomTypeRepository>();
builder.Services.AddScoped<IAmenityRepository, AmenityRepository>();
builder.Services.AddScoped<IHousekeepingRepository, HousekeepingRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMenuItemRepository, MenuItemRepository>();
builder.Services.AddScoped<IFoodOrderRepository, FoodOrderRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IMaintenanceRepository, MaintenanceRepository>();
builder.Services.AddScoped<IReceiptRepository, ReceiptRepository>();
builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
#endregion

#region Services
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IRoomTypeService, RoomTypeService>();
builder.Services.AddScoped<IStaffService, StaffService>();

builder.Services.AddScoped<HotelManagement.API.Filters.IdempotentAttribute>();

builder.Services.AddScoped<IGuestService, GuestService>();
builder.Services.AddScoped<IHousekeepingService, HousekeepingService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IMenuItemService, MenuItemService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<IAmenityService, AmenityService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
#endregion

#region Core Infrastructure & SignalR
builder.Services.AddScoped<INotificationService, HotelManagement.API.Services.SignalRNotificationService>();
builder.Services.AddHostedService<HotelManagement.API.Services.IdempotencyCleanupService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, HotelManagement.API.Services.CurrentUserService>();
builder.Services.AddScoped<IAuditUserProvider>(sp => (IAuditUserProvider)sp.GetRequiredService<ICurrentUserService>());
builder.Services.AddSignalR();
#endregion

#region Mappers
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<HotelManagement.BLL.Profiles.MappingProfile>();
});
#endregion

#region Rate Limiter
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("GlobalPolicy", opt =>
    {
        opt.Window = TimeSpan.FromSeconds(10);
        opt.PermitLimit = 100;
        opt.QueueLimit = 2;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});
#endregion

#region Controllers & Routing
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers(options =>
{
    options.Filters.Add<HotelManagement.API.Filters.IdempotentAttribute>();
    options.ModelBinderProviders.Insert(0, new CustomDateTimeModelBinderProvider());
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new CustomDateTimeConverter());
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});
#endregion

#region Authentication & Authorization
// JWT Authentication Configuration
var keyString = builder.Configuration["Jwt:Key"] ?? "super_secret_fallback_key_that_should_be_long_enough_for_hmacsha256_hotel_management";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "HotelManagementAPI",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "HotelManagementClients",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString)),
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notifications"))
                {
                    context.Token = accessToken;
                }
                else
                {
                    // Workaround for Postman sending multiple Authorization headers (e.g. "Bearer <token>, Bearer ")
                    var authHeader = context.Request.Headers.Authorization.ToString();
                    if (!string.IsNullOrEmpty(authHeader) && authHeader.Contains(','))
                    {
                        var tokens = authHeader.Split(',');
                        var validToken = tokens.FirstOrDefault(t => t.Trim().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) && t.Trim().Length > 10);
                        if (validToken != null)
                        {
                            context.Token = validToken.Trim().Substring("Bearer ".Length).Trim();
                        }
                    }
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
#endregion

#region CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200", "http://localhost:4201")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
});
#endregion

#region Swagger & OpenAPI
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. \r\n\r\n Paste your token directly below.\r\n\r\nExample: \"eyJhbGciOiJIUzI1...\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});
#endregion

#region Application Pipeline
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Seed the main database
    HotelManagement.API.Utilities.MainDatabaseSeeder.Seed(app.Services);
}

// app.UseHttpsRedirection();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSerilogRequestLogging();
app.UseRouting();
app.UseCors("AllowAll");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers().RequireRateLimiting("GlobalPolicy");
app.MapHub<HotelManagement.API.Hubs.NotificationHub>("/notifications").RequireCors("AllowAll");

app.Run();
#endregion

#region Custom Utilities & Binders
public class CustomDateTimeConverter : System.Text.Json.Serialization.JsonConverter<DateTime>
{
    private readonly string _format = "dd-MM-yyyy";

    public override DateTime Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
    {
        if (DateTime.TryParseExact(reader.GetString(), _format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var date))
            return DateTime.SpecifyKind(date, DateTimeKind.Utc);

        // Fallback to default ISO parsing
        if (DateTime.TryParse(reader.GetString(), out var defaultDate))
            return DateTime.SpecifyKind(defaultDate, DateTimeKind.Utc);

        return DateTime.SpecifyKind(reader.GetDateTime(), DateTimeKind.Utc);
    }

    public override void Write(System.Text.Json.Utf8JsonWriter writer, DateTime value, System.Text.Json.JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(_format));
    }
}

public class CustomDateTimeModelBinder : Microsoft.AspNetCore.Mvc.ModelBinding.IModelBinder
{
    public Task BindModelAsync(Microsoft.AspNetCore.Mvc.ModelBinding.ModelBindingContext bindingContext)
    {
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult == Microsoft.AspNetCore.Mvc.ModelBinding.ValueProviderResult.None)
            return Task.CompletedTask;

        var value = valueProviderResult.FirstValue;
        if (string.IsNullOrEmpty(value))
            return Task.CompletedTask;

        if (DateTime.TryParseExact(value, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var date))
        {
            bindingContext.Result = Microsoft.AspNetCore.Mvc.ModelBinding.ModelBindingResult.Success(DateTime.SpecifyKind(date, DateTimeKind.Utc));
            return Task.CompletedTask;
        }

        if (DateTime.TryParse(value, out var defaultDate))
        {
            bindingContext.Result = Microsoft.AspNetCore.Mvc.ModelBinding.ModelBindingResult.Success(DateTime.SpecifyKind(defaultDate, DateTimeKind.Utc));
            return Task.CompletedTask;
        }

        bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, $"The value '{value}' is not valid for {bindingContext.ModelName}.");
        return Task.CompletedTask;
    }
}

public class CustomDateTimeModelBinderProvider : Microsoft.AspNetCore.Mvc.ModelBinding.IModelBinderProvider
{
    public Microsoft.AspNetCore.Mvc.ModelBinding.IModelBinder? GetBinder(Microsoft.AspNetCore.Mvc.ModelBinding.ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType == typeof(DateTime) || context.Metadata.ModelType == typeof(DateTime?))
            return new CustomDateTimeModelBinder();

        return null;
    }
}

public partial class Program { }
#endregion