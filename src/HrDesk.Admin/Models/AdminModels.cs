namespace HrDesk.Admin.Models;

public class Ticket
{
    public string Id { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Escalation
{
    public string Id { get; set; } = string.Empty;
    public string TicketId { get; set; } = string.Empty;
    public string AssignedTo { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime EscalatedAt { get; set; } = DateTime.UtcNow;
}
