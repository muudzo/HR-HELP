namespace HrDesk.BackgroundJobs;

using Microsoft.Extensions.Logging;

/// <summary>
/// Stub background job to sync data from PeopleHum.
/// In Phase 2, this will be implemented with real sync logic.
/// </summary>
public class SyncPeopleHumJob
{
    private readonly ILogger<SyncPeopleHumJob> _logger;

    public SyncPeopleHumJob(ILogger<SyncPeopleHumJob> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting PeopleHum sync job");

        // Stub: No-op in Phase 1
        _logger.LogInformation("PeopleHum sync completed");

        return Task.CompletedTask;
    }
}
