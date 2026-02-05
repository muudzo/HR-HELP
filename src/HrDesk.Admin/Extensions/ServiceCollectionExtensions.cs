namespace HrDesk.Admin.Extensions;

using Microsoft.Extensions.DependencyInjection;
using HrDesk.Admin.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAdminServices(this IServiceCollection services)
    {
        services.AddScoped<AdminService>();
        return services;
    }
}
