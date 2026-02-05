using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using HrDesk.PeopleHum;
using HrDesk.Core.Models;

namespace HrDesk.Tests;

public class FakePeopleHumClientTests
{
    private readonly Mock<ILogger<FakePeopleHumClient>> _loggerMock;
    private readonly FakePeopleHumClient _client;

    public FakePeopleHumClientTests()
    {
        _loggerMock = new Mock<ILogger<FakePeopleHumClient>>();
        _client = new FakePeopleHumClient(_loggerMock.Object);
    }

    [Fact]
    public async Task GetLeaveBalanceAsync_ReturnsValidBalanceAsync()
    {
        // Act
        var balance = await _client.GetLeaveBalanceAsync("EMP-001");

        // Assert
        Assert.True(balance > 0);
        Assert.IsType<decimal>(balance);
    }

    [Fact]
    public async Task SubmitLeaveRequestAsync_ReturnsTrueAsync()
    {
        // Act
        var result = await _client.SubmitLeaveRequestAsync("EMP-001", DateTime.Now.AddDays(1), DateTime.Now.AddDays(5), "Vacation");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GetPayslipAsync_ReturnsPdfUrlAsync()
    {
        // Act
        var url = await _client.GetPayslipAsync("EMP-001", 1, 2024);

        // Assert
        Assert.NotEmpty(url);
        Assert.Contains(".pdf", url);
        Assert.StartsWith("https://", url);
    }

    [Fact]
    public async Task CreateTicketAsync_ReturnsValidTicketIdAsync()
    {
        // Act
        var ticketId = await _client.CreateTicketAsync("EMP-001", "Test Issue", "Test Description");

        // Assert
        Assert.NotEmpty(ticketId);
        Assert.StartsWith("TKT-", ticketId);
    }
}
