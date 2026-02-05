using System;
using System.Collections.Generic;

namespace HrDesk.Core.Models;

public enum TicketStatus
{
    New,
    InProgress,
    Resolved,
    Closed,
    Escalated
}

public enum TicketSeverity
{
    Low,
    Medium,
    High,
    Critical
}

public class Ticket
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // e.g., "SoftwareRequest", "Payroll"
    public string Description { get; set; } = string.Empty;
    public TicketStatus Status { get; set; } = TicketStatus.New;
    public TicketSeverity Severity { get; set; } = TicketSeverity.Medium;
    public string? AssignedTo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    // Relationship to ChatHistory
    public ICollection<ChatHistory> ChatHistories { get; set; } = new List<ChatHistory>();
    
    // Relationship to Escalations
    public ICollection<Escalation> Escalations { get; set; } = new List<Escalation>();
}
