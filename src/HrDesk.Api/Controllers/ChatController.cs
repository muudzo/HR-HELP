namespace HrDesk.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HrDesk.Core.Models;
using HrDesk.Core.Interfaces;
using HrDesk.Api.Services;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IAiOrchestrator _aiOrchestrator;
    private readonly UserContextService _userContextService;
    private readonly IAuditLogger _auditLogger;

    public ChatController(
        IAiOrchestrator aiOrchestrator,
        UserContextService userContextService,
        IAuditLogger auditLogger)
    {
        _aiOrchestrator = aiOrchestrator ?? throw new ArgumentNullException(nameof(aiOrchestrator));
        _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
        _auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
    }

    /// <summary>
    /// Process a chat message and return an AI-orchestrated response.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] ChatRequest request, CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
        var userContext = _userContextService.ExtractFromClaims(User, correlationId);

        if (userContext == null)
            return Unauthorized("Unable to extract user context from token");

        await _auditLogger.LogAsync("CHAT_REQUEST", userContext.EmployeeId, correlationId, new { request.Message });

        var response = await _aiOrchestrator.HandleAsync(request, userContext, cancellationToken);

        await _auditLogger.LogAsync("CHAT_RESPONSE", userContext.EmployeeId, correlationId, new { response.Intent, response.Escalated });

        return Ok(response);
    }
}
