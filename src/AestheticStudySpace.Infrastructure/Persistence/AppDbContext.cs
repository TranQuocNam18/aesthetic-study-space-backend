using AestheticStudySpace.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AestheticStudySpace.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<RoomAssetMapping> RoomAssetMappings => Set<RoomAssetMapping>();
    public DbSet<UserRoomConfig> UserRoomConfigs => Set<UserRoomConfig>();
    public DbSet<Todo> Todos => Set<Todo>();
    public DbSet<PomodoroSession> PomodoroSessions => Set<PomodoroSession>();

    public DbSet<RoomLayout> RoomLayouts => Set<RoomLayout>();
    public DbSet<RoomThumbnail> RoomThumbnails => Set<RoomThumbnail>();
    public DbSet<StoreItem> StoreItems => Set<StoreItem>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<UserInventory> UserInventories => Set<UserInventory>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Mission> Missions => Set<Mission>();
    public DbSet<UserMission> UserMissions => Set<UserMission>();
    public DbSet<CoinTransaction> CoinTransactions => Set<CoinTransaction>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<Report> Reports => Set<Report>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        ApplySoftDeleteQueryFilters(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        ApplyAuditAndSoftDelete();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditAndSoftDelete();
        return base.SaveChangesAsync(cancellationToken);
    }

    private static void ApplySoftDeleteQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(AppDbContext).GetMethod(
                    nameof(SetSoftDeleteFilter),
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

                method?.MakeGenericMethod(entityType.ClrType).Invoke(null, new object[] { modelBuilder });
            }
        }
    }

    private static void SetSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ISoftDeletable
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
    }

    private void ApplyAuditAndSoftDelete()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is not BaseEntity entity)
                continue;

            switch (entry.State)
            {
                case EntityState.Added:
                    entity.CreatedAt = now;
                    entity.UpdatedAt = null;
                    break;

                case EntityState.Modified:
                    entity.UpdatedAt = now;
                    break;

                case EntityState.Deleted:
                    if (entry.Entity is ISoftDeletable)
                    {
                        entry.State = EntityState.Modified;
                        entity.IsDeleted = true;
                        entity.DeletedAt = now;
                        entity.UpdatedAt = now;
                    }
                    break;
            }
        }
    }
}
