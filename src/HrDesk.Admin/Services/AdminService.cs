namespace HrDesk.Admin.Services;

using HrDesk.Admin.Models;

/// <summary>
/// Stub service for admin operations returning mock data.
/// In Phase 2, will integrate with database.
/// </summary>
public class AdminService
{
    public Task<List<Ticket>> GetTicketsAsync()
    {
        var tickets = new List<Ticket>
        {
            new() { Id = "TKT-001", EmployeeId = "EMP-001", Subject = "Leave Request Pending", Status = "Open" },
            new() { Id = "TKT-002", EmployeeId = "EMP-002", Subject = "Payslip Issue", Status = "In Progress" },
            new() { Id = "TKT-003", EmployeeId = "EMP-003", Subject = "Benefits Inquiry", Status = "Resolved" }
        };

        return Task.FromResult(tickets);
    }

    public Task<List<Escalation>> GetEscalationsAsync()
    {
        var escalations = new List<Escalation>
        {
            new() { Id = "ESC-001", TicketId = "TKT-002", AssignedTo = "HR-Manager-001", Reason = "Complex leave policy case" },
            new() { Id = "ESC-002", TicketId = "TKT-003", AssignedTo = "HR-Manager-002", Reason = "International benefits question" }
        };

        return Task.FromResult(escalations);
    }
}
