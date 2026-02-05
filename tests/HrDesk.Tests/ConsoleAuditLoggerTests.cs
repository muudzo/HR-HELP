using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using HrDesk.Audit;
using HrDesk.Core.Interfaces;

namespace HrDesk.Tests;

public class ConsoleAuditLoggerTests
{
    private readonly Mock<ILogger<ConsoleAuditLogger>> _loggerMock;
    private readonly ConsoleAuditLogger _auditLogger;

    public ConsoleAuditLoggerTests()
    {
        _loggerMock = new Mock<ILogger<ConsoleAuditLogger>>();
        _auditLogger = new ConsoleAuditLogger(_loggerMock.Object);
    }

    [Fact]
    public async Task LogAsync_WithValidInputs_CompletesSuccessfullyAsync()
    {
        // Arrange
        var action = "TEST_ACTION";
        var userId = "EMP-001";
        var correlationId = Guid.NewGuid().ToString();
        var payload = new { Message = "Test" };

        // Act
        await _auditLogger.LogAsync(action, userId, correlationId, payload);

        // Assert
        // Verify logger was called
        _loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LogAsync_WithoutPayload_CompletesSuccessfullyAsync()
    {
        // Arrange
        var action = "TEST_ACTION";
        var userId = "EMP-002";
        var correlationId = Guid.NewGuid().ToString();

        // Act
        await _auditLogger.LogAsync(action, userId, correlationId);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
