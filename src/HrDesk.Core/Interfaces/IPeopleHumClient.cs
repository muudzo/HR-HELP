namespace HrDesk.Core.Interfaces;

using HrDesk.Core.Models;

/// <summary>
/// Interface for PeopleHum integration.
/// </summary>
public interface IPeopleHumClient
{
    Task<decimal> GetLeaveBalanceAsync(string employeeId, CancellationToken cancellationToken = default);
    Task<bool> SubmitLeaveRequestAsync(string employeeId, DateTime fromDate, DateTime toDate, string reason, CancellationToken cancellationToken = default);
    Task<string> GetPayslipAsync(string employeeId, int month, int year, CancellationToken cancellationToken = default);
    Task<string> CreateTicketAsync(string employeeId, string subject, string description, CancellationToken cancellationToken = default);
}
