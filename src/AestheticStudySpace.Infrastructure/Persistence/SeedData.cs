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
    public static readonly Guid StoreThemeStarterId = Guid.Parse("44444444-4444-4444-4444-444444444401");
    public static readonly Guid StoreBackgroundRainId = Guid.Parse("44444444-4444-4444-4444-444444444402");
    public static readonly Guid StoreStickerCatId = Guid.Parse("44444444-4444-4444-4444-444444444403");
    public static readonly Guid StoreEffectGlowId = Guid.Parse("44444444-4444-4444-4444-444444444404");
    public static readonly Guid StoreAmbientRainId = Guid.Parse("44444444-4444-4444-4444-444444444405");
    public static readonly Guid StoreBackgroundSunsetId = Guid.Parse("44444444-4444-4444-4444-444444444406");
    public static readonly Guid MissionDailyLoginId = Guid.Parse("55555555-5555-5555-5555-555555555501");
    public static readonly Guid MissionPomodoroId = Guid.Parse("55555555-5555-5555-5555-555555555502");
    public static readonly Guid MissionWeeklyStudyId = Guid.Parse("55555555-5555-5555-5555-555555555503");

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
            // IgnoreQueryFilters() so the soft-delete filter doesn't hide already-seeded rows
            if (!await context.Roles.IgnoreQueryFilters().AnyAsync(r => r.Id == role.Id))
                await context.Roles.AddAsync(role);
        }

        await context.SaveChangesAsync();

        if (!await context.Users.IgnoreQueryFilters().AnyAsync(u => u.Id == AdminUserId))
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
            Category = AssetCategory.Rain.ToString(),
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
            Category = AssetCategory.Cafe.ToString(),
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
            Category = AssetCategory.WhiteNoise.ToString(),
            DefaultVolume = 60,
            IsPremium = false
        };

        var cat = new Asset
        {
            Id = CatAssetId,
            Name = "Sleeping Cat",
            Description = "Animated cat visual layer",
            Url = "https://res.cloudinary.com/demo/image/upload/cat-sleeping.gif",
            AssetType = AssetType.Sticker,
            Category = AssetCategory.Pet.ToString(),
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
            Category = AssetCategory.Lofi.ToString(),
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

        // IgnoreQueryFilters() ensures soft-deleted rows are counted, preventing duplicate PK inserts
        if (!await context.Rooms.IgnoreQueryFilters().AnyAsync())
            await context.Rooms.AddRangeAsync(cozyAttic, neonLoft);

        if (!await context.Assets.IgnoreQueryFilters().AnyAsync())
            await context.Assets.AddRangeAsync(rain, cafe, whiteNoise, cat, lofiPremium);

        if (!await context.RoomAssetMappings.IgnoreQueryFilters().AnyAsync())
            await context.RoomAssetMappings.AddRangeAsync(mappings);

        if (!await context.StoreItems.IgnoreQueryFilters().AnyAsync())
        {
            await context.StoreItems.AddRangeAsync(
                new StoreItem
                {
                    Id = StoreThemeStarterId,
                    Category = StoreCategory.Theme,
                    ThemeSource = StoreThemeSource.Official,
                    Name = "Cozy Starter Theme",
                    Description = "Free warm theme combo with sticker, background, effect, and ambient sound.",
                    AssetUrl = "https://res.cloudinary.com/demo/image/upload/theme-cozy.jpg",
                    ThemeStickerItemId = StoreStickerCatId,
                    ThemeBackgroundItemId = StoreBackgroundRainId,
                    ThemeEffectItemId = StoreEffectGlowId,
                    ThemeAmbientSoundItemId = StoreAmbientRainId,
                    IsPremium = false,
                    CoinPrice = null,
                    RealMoneyPriceVnd = null,
                    IsActive = true
                },
                new StoreItem
                {
                    Id = StoreBackgroundRainId,
                    Category = StoreCategory.Background,
                    ThemeSource = null,
                    Name = "Rainy Window",
                    Description = "Calm rain background.",
                    AssetUrl = "https://res.cloudinary.com/demo/image/upload/bg-rain.jpg",
                    IsPremium = false,
                    CoinPrice = 150,
                    RealMoneyPriceVnd = null,
                    IsActive = true
                },
                new StoreItem
                {
                    Id = StoreBackgroundSunsetId,
                    Category = StoreCategory.Background,
                    ThemeSource = null,
                    Name = "Sunset Horizon",
                    Description = "Golden hour background for calm evening sessions.",
                    AssetUrl = "https://res.cloudinary.com/demo/image/upload/bg-sunset.jpg",
                    IsPremium = true,
                    CoinPrice = 220,
                    RealMoneyPriceVnd = 39000,
                    IsActive = true
                },
                new StoreItem
                {
                    Id = StoreStickerCatId,
                    Category = StoreCategory.Sticker,
                    ThemeSource = null,
                    Name = "Premium Cat Sticker",
                    Description = "Animated cat sticker — Premium users only.",
                    AssetUrl = "https://res.cloudinary.com/demo/image/upload/sticker-cat.gif",
                    IsPremium = true,
                    CoinPrice = 300,
                    RealMoneyPriceVnd = 29000,
                    IsActive = true
                },
                new StoreItem
                {
                    Id = StoreEffectGlowId,
                    Category = StoreCategory.Effect,
                    ThemeSource = null,
                    Name = "Soft Glow",
                    Description = "A subtle glow effect for theme combos.",
                    AssetUrl = "https://res.cloudinary.com/demo/image/upload/effect-glow.png",
                    IsPremium = false,
                    CoinPrice = 120,
                    RealMoneyPriceVnd = null,
                    IsActive = true
                },
                new StoreItem
                {
                    Id = StoreAmbientRainId,
                    Category = StoreCategory.AmbientSound,
                    ThemeSource = null,
                    Name = "Gentle Rain Loop",
                    Description = "Ambient rain sound for theme combos.",
                    AssetUrl = "https://res.cloudinary.com/demo/audio/upload/ambient-rain.mp3",
                    IsPremium = false,
                    CoinPrice = 140,
                    RealMoneyPriceVnd = null,
                    IsActive = true
                });
        }

        if (!await context.Missions.IgnoreQueryFilters().AnyAsync())
        {
            await context.Missions.AddRangeAsync(
                new Mission
                {
                    Id = MissionDailyLoginId,
                    Code = "daily_login",
                    Name = "Daily Login",
                    Description = "Log in once per day.",
                    RewardCoins = 10,
                    TriggerKey = "daily_login",
                    TargetValue = 1,
                    Frequency = "daily",
                    IsActive = true
                },
                new Mission
                {
                    Id = MissionPomodoroId,
                    Code = "pomodoro_daily",
                    Name = "Focus Sessions",
                    Description = "Complete 4 Pomodoro sessions today.",
                    RewardCoins = 25,
                    TriggerKey = "pomodoro_complete",
                    TargetValue = 4,
                    Frequency = "daily",
                    IsActive = true
                },
                new Mission
                {
                    Id = MissionWeeklyStudyId,
                    Code = "weekly_study",
                    Name = "Weekly Study Goal",
                    Description = "Accumulate 300 minutes of study this week.",
                    RewardCoins = 100,
                    TriggerKey = "study_minutes",
                    TargetValue = 300,
                    Frequency = "weekly",
                    IsActive = true
                });
        }

        await context.SaveChangesAsync();

        logger.LogInformation("Database seeded with sample rooms, assets, store items, missions, and admin user.");
    }
}
