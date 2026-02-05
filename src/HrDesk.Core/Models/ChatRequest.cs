namespace HrDesk.Core.Models;

/// <summary>
/// Represents a chat request from a user.
/// </summary>
public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
    public string? TicketId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
