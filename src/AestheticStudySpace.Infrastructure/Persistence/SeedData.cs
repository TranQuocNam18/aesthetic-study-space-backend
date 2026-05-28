using AestheticStudySpace.Domain.Entities;
using AestheticStudySpace.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AestheticStudySpace.Infrastructure.Persistence;

public static class SeedData
{
    public static readonly Guid AdminUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid GuestRoleId = Guid.Parse("aaaaaaa1-aaaa-aaaa-aaaa-aaaaaaaaaaa1");
    public static readonly Guid UserRoleId = Guid.Parse("aaaaaaa1-aaaa-aaaa-aaaa-aaaaaaaaaaa2");
    public static readonly Guid PremiumUserRoleId = Guid.Parse("aaaaaaa1-aaaa-aaaa-aaaa-aaaaaaaaaaa3");
    public static readonly Guid AdminRoleId = Guid.Parse("aaaaaaa1-aaaa-aaaa-aaaa-aaaaaaaaaaa4");
    public static readonly Guid CozyAtticRoomId = Guid.Parse("22222222-2222-2222-2222-222222222201");
    public static readonly Guid NeonLoftRoomId = Guid.Parse("22222222-2222-2222-2222-222222222202");
    public static readonly Guid RainAssetId = Guid.Parse("33333333-3333-3333-3333-333333333301");
    public static readonly Guid CafeAssetId = Guid.Parse("33333333-3333-3333-3333-333333333302");
    public static readonly Guid WhiteNoiseAssetId = Guid.Parse("33333333-3333-3333-3333-333333333303");
    public static readonly Guid CatAssetId = Guid.Parse("33333333-3333-3333-3333-333333333304");
    public static readonly Guid LofiPremiumAssetId = Guid.Parse("33333333-3333-3333-3333-333333333305");

    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        await context.Database.MigrateAsync();

        var roles = new List<Role>
        {
            new() { Id = GuestRoleId, Name = "Guest", Description = "Demo mode only; cannot persist layouts.", IsSystem = true },
            new() { Id = UserRoleId, Name = "User", Description = "Freemium user.", IsSystem = true },
            new() { Id = PremiumUserRoleId, Name = "PremiumUser", Description = "Premium user.", IsSystem = true },
            new() { Id = AdminRoleId, Name = "Admin", Description = "Administrator.", IsSystem = true }
        };

        foreach (var role in roles)
        {
            if (!await context.Roles.AnyAsync(r => r.Id == role.Id))
                await context.Roles.AddAsync(role);
        }

        await context.SaveChangesAsync();

        if (!await context.Users.AnyAsync(u => u.Id == AdminUserId))
        {
            var adminRole = await context.Roles.FirstAsync(r => r.Id == AdminRoleId);
            var admin = new User
            {
                Id = AdminUserId,
                Username = "admin",
                Email = "admin@aestheticstudy.space",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@12345"),
                RoleId = adminRole.Id,
                Role = adminRole,
                AccountTier = AccountTier.Premium,
                AvatarUrl = "https://res.cloudinary.com/demo/image/upload/sample.jpg"
            };

            await context.Users.AddAsync(admin);
        }

        var cozyAttic = new Room
        {
            Id = CozyAtticRoomId,
            Name = "Cozy Attic",
            Description = "Warm wooden attic with rain on the window — perfect for deep focus.",
            ThumbnailUrl = "https://res.cloudinary.com/demo/image/upload/attic-thumb.jpg",
            BackgroundUrl = "https://res.cloudinary.com/demo/image/upload/attic-bg.jpg",
            IsPremium = false
        };

        var neonLoft = new Room
        {
            Id = NeonLoftRoomId,
            Name = "Neon Loft",
            Description = "Premium cyber-loft with ambient neon lighting and lofi beats.",
            ThumbnailUrl = "https://res.cloudinary.com/demo/image/upload/neon-thumb.jpg",
            BackgroundUrl = "https://res.cloudinary.com/demo/image/upload/neon-bg.jpg",
            IsPremium = true
        };

        var rain = new Asset
        {
            Id = RainAssetId,
            Name = "Gentle Rain",
            Description = "Soft rainfall ambience",
            Url = "https://res.cloudinary.com/demo/video/upload/rain.mp3",
            AssetType = AssetType.Audio,
            Category = AssetCategory.Rain,
            DefaultVolume = 70,
            IsPremium = false
        };

        var cafe = new Asset
        {
            Id = CafeAssetId,
            Name = "Cafe Murmur",
            Description = "Low café background chatter",
            Url = "https://res.cloudinary.com/demo/video/upload/cafe.mp3",
            AssetType = AssetType.Audio,
            Category = AssetCategory.Cafe,
            DefaultVolume = 55,
            IsPremium = false
        };

        var whiteNoise = new Asset
        {
            Id = WhiteNoiseAssetId,
            Name = "White Noise",
            Description = "Steady white noise for focus",
            Url = "https://res.cloudinary.com/demo/video/upload/whitenoise.mp3",
            AssetType = AssetType.Audio,
            Category = AssetCategory.WhiteNoise,
            DefaultVolume = 60,
            IsPremium = false
        };

        var cat = new Asset
        {
            Id = CatAssetId,
            Name = "Sleeping Cat",
            Description = "Animated cat visual layer",
            Url = "https://res.cloudinary.com/demo/image/upload/cat-sleeping.gif",
            AssetType = AssetType.Visual,
            Category = AssetCategory.Pet,
            DefaultVolume = 0,
            IsPremium = false
        };

        var lofiPremium = new Asset
        {
            Id = LofiPremiumAssetId,
            Name = "Midnight Lofi",
            Description = "Premium lofi hip-hop stream",
            Url = "https://res.cloudinary.com/demo/video/upload/lofi-premium.mp3",
            AssetType = AssetType.Audio,
            Category = AssetCategory.Lofi,
            DefaultVolume = 65,
            IsPremium = true
        };

        var mappings = new List<RoomAssetMapping>
        {
            new() { RoomId = cozyAttic.Id, AssetId = rain.Id, DefaultPositionX = 0, DefaultPositionY = 0, DefaultLayerIndex = 0 },
            new() { RoomId = cozyAttic.Id, AssetId = cafe.Id, DefaultPositionX = 0, DefaultPositionY = 0, DefaultLayerIndex = 1 },
            new() { RoomId = cozyAttic.Id, AssetId = cat.Id, DefaultPositionX = 120, DefaultPositionY = 330, DefaultScale = 1, DefaultOpacity = 1, DefaultLayerIndex = 2 },
            new() { RoomId = neonLoft.Id, AssetId = whiteNoise.Id, DefaultLayerIndex = 0 },
            new() { RoomId = neonLoft.Id, AssetId = lofiPremium.Id, DefaultLayerIndex = 1 }
        };

        if (!await context.Rooms.AnyAsync())
            await context.Rooms.AddRangeAsync(cozyAttic, neonLoft);

        if (!await context.Assets.AnyAsync())
            await context.Assets.AddRangeAsync(rain, cafe, whiteNoise, cat, lofiPremium);

        if (!await context.RoomAssetMappings.AnyAsync())
            await context.RoomAssetMappings.AddRangeAsync(mappings);

        await context.SaveChangesAsync();

        logger.LogInformation("Database seeded with sample rooms, assets, and admin user.");
    }
}
