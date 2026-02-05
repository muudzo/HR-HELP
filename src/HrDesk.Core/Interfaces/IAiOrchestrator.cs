namespace HrDesk.Core.Interfaces;

using HrDesk.Core.Models;

/// <summary>
/// Interface for AI orchestration logic.
/// </summary>
public interface IAiOrchestrator
{
    Task<AiResponse> HandleAsync(ChatRequest request, UserContext userContext, CancellationToken cancellationToken = default);
}
