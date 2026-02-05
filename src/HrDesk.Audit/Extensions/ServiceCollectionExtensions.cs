namespace HrDesk.Audit.Extensions;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using HrDesk.Core.Interfaces;
using HrDesk.Audit.Middleware;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuditLogging(this IServiceCollection services)
    {
        services.AddScoped<IAuditLogger, ConsoleAuditLogger>();
        return services;
    }

    public static IApplicationBuilder UseAuditLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<AuditLoggingMiddleware>();
        return app;
    }
}
