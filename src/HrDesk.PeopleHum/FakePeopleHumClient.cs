namespace HrDesk.PeopleHum;

using HrDesk.Core.Interfaces;
using Microsoft.Extensions.Logging;

/// <summary>
/// Fake PeopleHum client adapter returning stub data.
/// In Phase 2, this will integrate with real PeopleHum API with retry and circuit breaker.
/// </summary>
public class FakePeopleHumClient : IPeopleHumClient
{
    private readonly ILogger<FakePeopleHumClient> _logger;

    public FakePeopleHumClient(ILogger<FakePeopleHumClient> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<decimal> GetLeaveBalanceAsync(string employeeId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching leave balance for employee {EmployeeId}", employeeId);

        // Stub: Return mock data
        var balance = 15.5m;

        _logger.LogInformation("Leave balance for {EmployeeId}: {Balance} days", employeeId, balance);

        return Task.FromResult(balance);
    }

    public Task<bool> SubmitLeaveRequestAsync(string employeeId, DateTime fromDate, DateTime toDate, string reason, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Submitting leave request for {EmployeeId} from {FromDate} to {ToDate}", employeeId, fromDate, toDate);

        // Stub: Always succeed
        var success = true;

        _logger.LogInformation("Leave request submission result: {Success}", success);

        return Task.FromResult(success);
    }

    public Task<string> GetPayslipAsync(string employeeId, int month, int year, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching payslip for {EmployeeId} for {Month}/{Year}", employeeId, month, year);

        // Stub: Return mock URL
        var payslipUrl = $"https://peoplehum-stub.local/payslips/{employeeId}/{year}-{month:D2}.pdf";

        _logger.LogInformation("Payslip URL: {PayslipUrl}", payslipUrl);

        return Task.FromResult(payslipUrl);
    }

    public Task<string> CreateTicketAsync(string employeeId, string subject, string description, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating support ticket for {EmployeeId}: {Subject}", employeeId, subject);

        // Stub: Generate mock ticket ID
        var ticketId = $"TKT-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

        _logger.LogInformation("Ticket created with ID: {TicketId}", ticketId);

        return Task.FromResult(ticketId);
    }
}
