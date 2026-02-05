using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using HrDesk.Ai;
using HrDesk.Core.Models;
using HrDesk.Core.Constants;

namespace HrDesk.Tests;

public class StubAiOrchestratorTests
{
    private readonly Mock<ILogger<StubAiOrchestrator>> _loggerMock;
    private readonly StubAiOrchestrator _orchestrator;

    public StubAiOrchestratorTests()
    {
        _loggerMock = new Mock<ILogger<StubAiOrchestrator>>();
        _orchestrator = new StubAiOrchestrator(_loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithLeaveKeyword_ReturnsLeaveIntentAsync()
    {
        // Arrange
        var chatRequest = new ChatRequest { Message = "I need to take leave next week" };
        var userContext = new UserContext 
        { 
            EmployeeId = "EMP-001", 
            Email = "test@company.com",
            Country = "US",
            CorrelationId = Guid.NewGuid().ToString()
        };

        // Act
        var response = await _orchestrator.HandleAsync(chatRequest, userContext);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(IntentClassification.LeaveRequest, response.Intent);
        Assert.False(response.Escalated);
        Assert.Contains("leave", response.Response.ToLower());
    }

    [Fact]
    public async Task HandleAsync_WithPayslipKeyword_ReturnsPayslipIntentAsync()
    {
        // Arrange
        var chatRequest = new ChatRequest { Message = "Can I see my salary payslip?" };
        var userContext = new UserContext 
        { 
            EmployeeId = "EMP-002", 
            Email = "test2@company.com",
            Country = "UK",
            CorrelationId = Guid.NewGuid().ToString()
        };

        // Act
        var response = await _orchestrator.HandleAsync(chatRequest, userContext);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(IntentClassification.PayslipQuery, response.Intent);
        Assert.False(response.Escalated);
    }

    [Fact]
    public async Task HandleAsync_WithEscalationKeyword_ReturnsEscalationIntentAsync()
    {
        // Arrange
        var chatRequest = new ChatRequest { Message = "This is urgent and needs escalation to my manager" };
        var userContext = new UserContext 
        { 
            EmployeeId = "EMP-003", 
            Email = "test3@company.com",
            Country = "DE",
            CorrelationId = Guid.NewGuid().ToString()
        };

        // Act
        var response = await _orchestrator.HandleAsync(chatRequest, userContext);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(IntentClassification.Escalation, response.Intent);
        Assert.True(response.Escalated);
        Assert.NotNull(response.EscalationReason);
    }

    [Fact]
    public async Task HandleAsync_WithUnknownKeyword_ReturnsUnknownIntentAsync()
    {
        // Arrange
        var chatRequest = new ChatRequest { Message = "Tell me about the weather" };
        var userContext = new UserContext 
        { 
            EmployeeId = "EMP-004", 
            Email = "test4@company.com",
            Country = "US",
            CorrelationId = Guid.NewGuid().ToString()
        };

        // Act
        var response = await _orchestrator.HandleAsync(chatRequest, userContext);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(IntentClassification.Unknown, response.Intent);
        Assert.False(response.Escalated);
    }

    [Fact]
    public async Task HandleAsync_PreservesCorrelationIdAsync()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        var chatRequest = new ChatRequest { Message = "test message" };
        var userContext = new UserContext 
        { 
            EmployeeId = "EMP-005", 
            Email = "test5@company.com",
            Country = "US",
            CorrelationId = correlationId
        };

        // Act
        var response = await _orchestrator.HandleAsync(chatRequest, userContext);

        // Assert
        Assert.Equal(correlationId, response.CorrelationId);
    }
}
