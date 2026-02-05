namespace HrDesk.Core.Interfaces;

/// <summary>
/// Interface for audit logging.
/// </summary>
public interface IAuditLogger
{
    Task LogAsync(string action, string userId, string correlationId, object? payload = null, CancellationToken cancellationToken = default);
}
