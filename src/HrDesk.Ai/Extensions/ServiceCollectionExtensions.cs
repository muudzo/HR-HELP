using HrDesk.Core.Models;
using Microsoft.Extensions.Configuration;

namespace HrDesk.Ai.Extensions;

using Microsoft.Extensions.DependencyInjection;
using HrDesk.Core.Interfaces;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAiOrchestration(this IServiceCollection services, IConfiguration configuration)
    {
        var featureFlags = configuration.GetSection("FeatureFlags").Get<FeatureFlags>();

        if (featureFlags?.EnableLiveAI == true)
        {
            services.AddScoped<IAiOrchestrator, LlmOrchestrator>();
        }
        else
        {
            services.AddScoped<IAiOrchestrator, StubAiOrchestrator>();
        }

        return services;
    }
}
