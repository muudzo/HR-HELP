namespace HrDesk.Audit;

using HrDesk.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

/// <summary>
/// Console-based audit logger. In Phase 2, will add database sink.
/// </summary>
public class ConsoleAuditLogger : IAuditLogger
{
    private readonly ILogger<ConsoleAuditLogger> _logger;

    public ConsoleAuditLogger(ILogger<ConsoleAuditLogger> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task LogAsync(string action, string userId, string correlationId, object? payload = null, CancellationToken cancellationToken = default)
    {
        var auditEntry = new
        {
            Timestamp = DateTime.UtcNow,
            Action = action,
            UserId = userId,
            CorrelationId = correlationId,
            Payload = payload
        };

        var json = JsonSerializer.Serialize(auditEntry);
        _logger.LogInformation("AUDIT: {AuditEntry}", json);

        return Task.CompletedTask;
    }
}
