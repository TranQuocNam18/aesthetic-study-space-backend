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
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IPaymentFulfillmentService, PaymentFulfillmentService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IMissionService, MissionService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<ITodoService, TodoService>();
        services.AddScoped<IPomodoroService, PomodoroService>();
        return services;
    }
}
