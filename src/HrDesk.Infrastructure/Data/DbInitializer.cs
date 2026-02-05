using Microsoft.EntityFrameworkCore;
using HrDesk.Core.Models;
using System;
using System.Linq;

namespace HrDesk.Infrastructure.Data;

public static class DbInitializer
{
    public static void Initialize(HrDeskDbContext context)
    {
        context.Database.Migrate();

        // Look for any tickets.
        if (context.Tickets.Any())
        {
            return;   // DB has been seeded
        }

        var tickets = new Ticket[]
        {
            new Ticket
            {
                UserId = "emp-001",
                Type = "SoftwareRequest",
                Description = "Need Visual Studio License",
                Status = TicketStatus.New,
                Severity = TicketSeverity.Medium,
                CreatedAt = DateTime.UtcNow
            },
             new Ticket
            {
                UserId = "emp-002",
                Type = "Payroll",
                Description = "Discrepancy in payslip",
                Status = TicketStatus.InProgress,
                Severity = TicketSeverity.High,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };

        context.Tickets.AddRange(tickets);
        context.SaveChanges();
    }
}
