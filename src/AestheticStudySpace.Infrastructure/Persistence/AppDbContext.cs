using AestheticStudySpace.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AestheticStudySpace.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<RoomAssetMapping> RoomAssetMappings => Set<RoomAssetMapping>();
    public DbSet<UserRoomConfig> UserRoomConfigs => Set<UserRoomConfig>();
    public DbSet<Todo> Todos => Set<Todo>();
    public DbSet<PomodoroSession> PomodoroSessions => Set<PomodoroSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
