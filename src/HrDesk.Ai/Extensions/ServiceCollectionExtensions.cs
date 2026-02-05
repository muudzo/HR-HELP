namespace HrDesk.Ai.Extensions;

using Microsoft.Extensions.DependencyInjection;
using HrDesk.Core.Interfaces;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAiOrchestration(this IServiceCollection services)
    {
        services.AddScoped<IAiOrchestrator, StubAiOrchestrator>();
        return services;
    }
}
