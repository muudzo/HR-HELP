namespace HrDesk.Core.Models;

/// <summary>
/// Represents a user context extracted from JWT claims.
/// </summary>
public class UserContext
{
    public string EmployeeId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string[] Roles { get; set; } = [];
    public string Country { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
}
