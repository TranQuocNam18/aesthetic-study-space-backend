using AestheticStudySpace.Application.Interfaces.Services;
using AestheticStudySpace.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AestheticStudySpace.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IAssetService, AssetService>();
        services.AddScoped<IWorkspaceService, WorkspaceService>();
        services.AddScoped<IRoomLayoutService, RoomLayoutService>();
        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<IAdminStoreService, AdminStoreService>();
        services.AddScoped<IUserThemeService, UserThemeService>();
        services.AddScoped<IUserComponentService, UserComponentService>();
        services.AddScoped<IAdminMissionService, AdminMissionService>();
        services.AddScoped<ICoinService, CoinService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IPaymentFulfillmentService, PaymentFulfillmentService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IMissionService, MissionService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<ITodoService, TodoService>();
        services.AddScoped<IPomodoroService, PomodoroService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IWelcomeBackService, WelcomeBackService>();
        return services;
    }
}
