using System.Text;
using AestheticStudySpace.Application.Interfaces;
using AestheticStudySpace.Application.Interfaces.Repositories;
using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Application.Common;
using AestheticStudySpace.Infrastructure.BackgroundServices;
using AestheticStudySpace.Infrastructure.Integrations;
using AestheticStudySpace.Infrastructure.Identity;
using AestheticStudySpace.Infrastructure.Persistence;
using AestheticStudySpace.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PayOS;

namespace AestheticStudySpace.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<CloudinarySettings>(configuration.GetSection(CloudinarySettings.SectionName));
        services.Configure<ResendSettings>(configuration.GetSection(ResendSettings.SectionName));
        services.Configure<GoogleAuthSettings>(configuration.GetSection(GoogleAuthSettings.SectionName));
        services.Configure<VnPaySettings>(configuration.GetSection(VnPaySettings.SectionName));
        services.Configure<PayOsSettings>(configuration.GetSection(PayOsSettings.SectionName));
        services.Configure<GeminiSettings>(configuration.GetSection(GeminiSettings.SectionName));

        // PayOS SDK client (singleton — thread-safe)
        var payOsSection = configuration.GetSection(PayOsSettings.SectionName);
        services.AddSingleton(_ => new PayOSClient(
            payOsSection["ClientId"]    ?? string.Empty,
            payOsSection["ApiKey"]      ?? string.Empty,
            payOsSection["ChecksumKey"] ?? string.Empty));

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        var databaseProvider = configuration["Database:Provider"] ?? "SqlServer";

        services.AddDbContext<AppDbContext>(options =>
        {
            if (string.Equals(databaseProvider, "PostgreSQL", StringComparison.OrdinalIgnoreCase))
                options.UseNpgsql(connectionString, npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
            else
                options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IRoomLayoutRepository, RoomLayoutRepository>();
        services.AddScoped<IAssetRepository, AssetRepository>();
        services.AddScoped<IStoreRepository, StoreRepository>();
        services.AddScoped<ICoinTransactionRepository, CoinTransactionRepository>();
        services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IMissionRepository, MissionRepository>();
        services.AddScoped<IUserMissionRepository, UserMissionRepository>();
        services.AddScoped<IAdminRepository, AdminRepository>();
        services.AddScoped<IAdminAnalyticsRepository, AdminAnalyticsRepository>();
        services.AddScoped<IRoomAssetMappingRepository, RoomAssetMappingRepository>();
        services.AddScoped<IUserRoomConfigRepository, UserRoomConfigRepository>();
        services.AddScoped<ITodoRepository, TodoRepository>();
        services.AddScoped<IPomodoroRepository, PomodoroRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IMediaStorageService, CloudinaryMediaStorageService>();
        
        // Email sending via Resend
        services.AddHttpClient<ResendEmailSender>();
        services.AddScoped<IEmailSender, ResendEmailSender>();

        // AI Services (Gemini)
        services.AddHttpClient<GeminiService>();
        services.AddScoped<IAiService, GeminiService>();

        var jwt = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT settings are not configured.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };

                // SignalR can pass token via query string in future phase
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                            context.Token = accessToken;
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();
        services.AddHostedService<MissionResetWorker>();
        services.AddHostedService<SubscriptionExpirationWorker>();

        return services;
    }
}
