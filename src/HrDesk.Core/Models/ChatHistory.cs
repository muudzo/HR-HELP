using System;

namespace HrDesk.Core.Models;

public class ChatHistory
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string TraceId { get; set; } = string.Empty;
    public string UserMessage { get; set; } = string.Empty;
    public string AiResponse { get; set; } = string.Empty;
    public string Intent { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public int? TicketId { get; set; }
    public Ticket? Ticket { get; set; }
}
