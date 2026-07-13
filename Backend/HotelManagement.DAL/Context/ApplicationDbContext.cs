using HotelManagement.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace HotelManagement.DAL.Context;

public class ApplicationDbContext : DbContext
{
    private readonly IAuditUserProvider? _auditUserProvider;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IAuditUserProvider? auditUserProvider = null) : base(options)
    {
        _auditUserProvider = auditUserProvider;
    }

    public DbSet<RoomType> RoomTypes => Set<RoomType>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<FoodOrder> FoodOrders => Set<FoodOrder>();
    public DbSet<FoodOrderItem> FoodOrderItems => Set<FoodOrderItem>();
    public DbSet<Housekeeping> HousekeepingTasks => Set<Housekeeping>();
    public DbSet<MaintenanceTask> MaintenanceTasks => Set<MaintenanceTask>();
    public DbSet<Feedback> Feedbacks => Set<Feedback>();
    public DbSet<Receipt> Receipts => Set<Receipt>();
    public DbSet<User> Users => Set<User>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Amenity> Amenities => Set<Amenity>();
    public DbSet<BookingAmenity> BookingAmenities => Set<BookingAmenity>();
    public DbSet<BookingRoom> BookingRooms => Set<BookingRoom>();
    public DbSet<IdempotentRequest> IdempotentRequests => Set<IdempotentRequest>();
    public DbSet<UploadSession> UploadSessions => Set<UploadSession>();
    public DbSet<ConciergeActionLog> ConciergeActionLogs => Set<ConciergeActionLog>();
    public DbSet<ConversationMessage> ConversationMessages => Set<ConversationMessage>();
    public DbSet<ConciergeProposal> ConciergeProposals => Set<ConciergeProposal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<IdempotentRequest>().HasKey(i => i.IdempotencyKey);

        modelBuilder.Entity<ConciergeActionLog>().HasKey(c => c.Id);
        modelBuilder.Entity<ConciergeActionLog>()
            .HasIndex(c => new { c.UserId, c.ConversationId, c.CreatedAt });

        modelBuilder.Entity<ConversationMessage>().HasKey(c => c.Id);
        modelBuilder.Entity<ConversationMessage>()
            .HasIndex(c => new { c.UserId, c.ConversationId, c.CreatedAt });

        modelBuilder.Entity<ConciergeProposal>().HasKey(c => c.Id);
        modelBuilder.Entity<ConciergeProposal>()
            .HasIndex(c => new { c.UserId, c.ConversationId, c.Status })
            .HasFilter("status = 'pending'");

        modelBuilder.Entity<AuditLog>().Property(a => a.PrimaryKey).HasColumnType("jsonb");
        modelBuilder.Entity<AuditLog>().Property(a => a.OldValues).HasColumnType("jsonb");
        modelBuilder.Entity<AuditLog>().Property(a => a.NewValues).HasColumnType("jsonb");


        modelBuilder.Entity<Booking>()
            .Property(b => b.BookingStatus)
            .HasConversion<string>();

        modelBuilder.Entity<Booking>()
            .Property(b => b.Origin)
            .HasConversion<string>();

        modelBuilder.Entity<Booking>()
            .Property(b => b.PaymentStatus)
            .HasConversion<string>();

        modelBuilder.Entity<Booking>()
            .Property(b => b.ServicePaymentStatus)
            .HasConversion<string>();

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.User)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<FoodOrder>()
            .Property(f => f.OrderStatus)
            .HasConversion<string>();

        modelBuilder.Entity<Housekeeping>()
            .Property(h => h.OriginType)
            .HasConversion<string>();
        modelBuilder.Entity<Housekeeping>()
            .Property(h => h.Status)
            .HasConversion<string>();

        modelBuilder.Entity<MaintenanceTask>()
            .Property(m => m.OriginType)
            .HasConversion<string>();

        modelBuilder.Entity<Housekeeping>()
            .HasOne(h => h.AssignedToUser)
            .WithMany()
            .HasForeignKey(h => h.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<MaintenanceTask>()
            .HasOne(m => m.AssignedToUser)
            .WithMany()
            .HasForeignKey(m => m.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<MaintenanceTask>()
            .Property(m => m.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Feedback>()
            .HasIndex(f => f.BookingId)
            .IsUnique();

        modelBuilder.Entity<RoomType>()
            .Property(r => r.BasePrice)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<RoomType>()
        .Property(r => r.ImageUrls)
        .HasConversion(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null));

        modelBuilder.Entity<RoomType>()
            .Property(r => r.BedConfigurationJson)
            .HasColumnName("BedConfiguration");

        modelBuilder.Entity<MenuItem>()
            .Property(m => m.Price)
            .HasColumnType("decimal(18,2)");



        modelBuilder.Entity<FoodOrderItem>()
            .Property(f => f.PriceAtPurchase)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Amenity>()
            .Property(a => a.Price)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<BookingAmenity>()
            .Property(ba => ba.PriceAtPurchase)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<BookingRoom>()
            .Property(br => br.LockedInPrice)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<BookingRoom>()
            .HasOne(br => br.Booking)
            .WithMany(b => b.BookingRooms)
            .HasForeignKey(br => br.BookingId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = OnBeforeSaveChanges();
        var result = await base.SaveChangesAsync(cancellationToken);
        await OnAfterSaveChanges(auditEntries, cancellationToken);
        return result;
    }

    public override int SaveChanges()
    {
        var auditEntries = OnBeforeSaveChanges();
        var result = base.SaveChanges();
        OnAfterSaveChanges(auditEntries, default).GetAwaiter().GetResult();
        return result;
    }

    private List<AuditEntryHelper> OnBeforeSaveChanges()
    {
        ChangeTracker.DetectChanges();
        var auditEntries = new List<AuditEntryHelper>();
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var auditEntry = new AuditEntryHelper(entry)
            {
                TableName = entry.Entity.GetType().Name,
                Action = entry.State.ToString(),
                ChangedByEmail = _auditUserProvider?.GetUserEmail() ?? "System/Anonymous",
                ChangedByName = _auditUserProvider?.GetUserName() ?? "System/Anonymous"
            };
            auditEntries.Add(auditEntry);

            foreach (var property in entry.Properties)
            {
                if (property.IsTemporary)
                {
                    // value will be generated by the database, get the value after saving
                    auditEntry.TemporaryProperties.Add(property);
                    continue;
                }

                string propertyName = property.Metadata.Name;
                if (property.Metadata.IsPrimaryKey())
                {
                    auditEntry.KeyValues[propertyName] = property.CurrentValue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.NewValues[propertyName] = property.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        auditEntry.OldValues[propertyName] = property.OriginalValue;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                        }
                        break;
                }
            }
        }

        // Save audit entities that have all the modifications
        foreach (var auditEntry in auditEntries.Where(_ => !_.HasTemporaryProperties))
        {
            AuditLogs.Add(auditEntry.ToAuditLog());
        }

        // keep a list of entries where the value of some properties are unknown at this step
        return auditEntries.Where(_ => _.HasTemporaryProperties).ToList();
    }

    private Task OnAfterSaveChanges(List<AuditEntryHelper> auditEntries, CancellationToken cancellationToken)
    {
        if (auditEntries == null || auditEntries.Count == 0)
            return Task.CompletedTask;

        foreach (var auditEntry in auditEntries)
        {
            // Get the final value of the temporary properties
            foreach (var prop in auditEntry.TemporaryProperties)
            {
                if (prop.Metadata.IsPrimaryKey())
                {
                    auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                }
                else
                {
                    auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                }
            }

            // Save the Audit entry
            AuditLogs.Add(auditEntry.ToAuditLog());
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}

public class AuditEntryHelper
{
    public AuditEntryHelper(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        Entry = entry;
    }

    public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Entry { get; }
    public string TableName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string ChangedByEmail { get; set; } = string.Empty;
    public string ChangedByName { get; set; } = string.Empty;
    public Dictionary<string, object?> KeyValues { get; } = new Dictionary<string, object?>();
    public Dictionary<string, object?> OldValues { get; } = new Dictionary<string, object?>();
    public Dictionary<string, object?> NewValues { get; } = new Dictionary<string, object?>();
    public List<Microsoft.EntityFrameworkCore.ChangeTracking.PropertyEntry> TemporaryProperties { get; } = new List<Microsoft.EntityFrameworkCore.ChangeTracking.PropertyEntry>();

    public bool HasTemporaryProperties => TemporaryProperties.Count != 0;

    public AuditLog ToAuditLog()
    {
        var audit = new AuditLog
        {
            EntityName = TableName,
            Action = Action,
            Timestamp = DateTimeOffset.UtcNow,
            ChangedByEmail = ChangedByEmail,
            ChangedByName = ChangedByName,

            PrimaryKey = KeyValues.Count == 0 ? null : System.Text.Json.JsonSerializer.SerializeToDocument(KeyValues)
        };

        audit.OldValues = OldValues.Count == 0 ? null : System.Text.Json.JsonSerializer.SerializeToDocument(OldValues);
        audit.NewValues = NewValues.Count == 0 ? null : System.Text.Json.JsonSerializer.SerializeToDocument(NewValues);

        return audit;
    }
}
