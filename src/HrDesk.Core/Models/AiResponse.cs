namespace HrDesk.Core.Models;

/// <summary>
/// Represents an AI response with intent classification and escalation flag.
/// </summary>
public class AiResponse
{
    public string Response { get; set; } = string.Empty;
    public string Intent { get; set; } = string.Empty;
    public bool Escalated { get; set; }
    public string? EscalationReason { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public DateTime RespondedAt { get; set; } = DateTime.UtcNow;
}
