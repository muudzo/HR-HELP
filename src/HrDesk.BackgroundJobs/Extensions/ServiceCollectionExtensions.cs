namespace HrDesk.BackgroundJobs.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Hangfire;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBackgroundJobs(this IServiceCollection services)
    {
        // Use in-memory job storage for Phase 1 (non-production)
        services.AddHangfire(configuration =>
        {
            configuration.UseInMemoryStorage();
        });

        services.AddHangfireServer();
        services.AddScoped<SyncPeopleHumJob>();

        return services;
    }

    public static void ScheduleRecurringJobs(IRecurringJobManager recurringJobManager)
    {
        // Schedule sync job to run every hour
        recurringJobManager.AddOrUpdate<SyncPeopleHumJob>(
            "sync-peoplehum",
            job => job.ExecuteAsync(CancellationToken.None),
            "0 * * * *" // Every hour
        );
    }
}
