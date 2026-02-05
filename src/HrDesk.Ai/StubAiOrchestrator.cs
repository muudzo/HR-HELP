namespace HrDesk.Ai;

using HrDesk.Core.Models;
using HrDesk.Core.Interfaces;
using HrDesk.Core.Constants;
using Microsoft.Extensions.Logging;

/// <summary>
/// Stub implementation of AI orchestrator with hard-coded intent classification.
/// In Phase 2, this will integrate with LLM.
/// </summary>
public class StubAiOrchestrator : IAiOrchestrator
{
    private readonly ILogger<StubAiOrchestrator> _logger;

    public StubAiOrchestrator(ILogger<StubAiOrchestrator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<AiResponse> HandleAsync(ChatRequest request, UserContext userContext, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing chat request from {EmployeeId}: {Message}", userContext.EmployeeId, request.Message);

        var intent = ClassifyIntent(request.Message);
        var response = GenerateResponse(intent, userContext);

        var aiResponse = new AiResponse
        {
            Response = response,
            Intent = intent,
            Escalated = intent == IntentClassification.Escalation,
            EscalationReason = intent == IntentClassification.Escalation ? "Complex query requires human intervention" : null,
            CorrelationId = userContext.CorrelationId
        };

        _logger.LogInformation("Generated AI response with intent {Intent}, escalated: {Escalated}", intent, aiResponse.Escalated);

        return Task.FromResult(aiResponse);
    }

    private string ClassifyIntent(string message)
    {
        // Stub classifier - in Phase 2, use actual LLM
        var lowerMessage = message.ToLowerInvariant();

        if (lowerMessage.Contains("leave") || lowerMessage.Contains("vacation") || lowerMessage.Contains("time off"))
            return IntentClassification.LeaveRequest;

        if (lowerMessage.Contains("payslip") || lowerMessage.Contains("salary") || lowerMessage.Contains("wage"))
            return IntentClassification.PayslipQuery;

        if (lowerMessage.Contains("urgent") || lowerMessage.Contains("escalate") || lowerMessage.Contains("manager"))
            return IntentClassification.Escalation;

        return IntentClassification.Unknown;
    }

    private string GenerateResponse(string intent, UserContext userContext)
    {
        return intent switch
        {
            IntentClassification.LeaveRequest => $"I can help you with leave requests. As an employee in {userContext.Country}, your leave policy includes standard annual leave. How many days would you like to request?",
            IntentClassification.PayslipQuery => "I can retrieve your recent payslips. Which month would you like to check?",
            IntentClassification.Escalation => "I see this requires special attention. Let me escalate this to your HR manager.",
            _ => "I'm not sure how to help with that. Could you please rephrase your question or would you like me to escalate this?"
        };
    }
}
