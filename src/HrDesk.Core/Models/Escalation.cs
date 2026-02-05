using System;

namespace HrDesk.Core.Models;

public class Escalation
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;
    public string Reason { get; set; } = string.Empty;
    public string? EscalatedTo { get; set; } // Role or specific user
    public DateTime EscalatedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending"; // Pending, Ack, Resolved
    public string? ResolutionNotes { get; set; }
}
