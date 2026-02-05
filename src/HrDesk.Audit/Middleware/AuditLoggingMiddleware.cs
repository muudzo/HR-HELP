namespace HrDesk.Audit.Middleware;

using Microsoft.AspNetCore.Http;
using HrDesk.Core.Interfaces;
using HrDesk.Core.Models;

/// <summary>
/// Middleware that logs all incoming requests to the audit system.
/// </summary>
public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public AuditLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuditLogger auditLogger)
    {
        var correlationId = context.Request.Headers.ContainsKey("X-Correlation-ID")
            ? context.Request.Headers["X-Correlation-ID"].ToString()
            : Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;

        // Extract user from claims if available
        var userId = context.User?.FindFirst("sub")?.Value ?? "Anonymous";

        var auditPayload = new
        {
            Method = context.Request.Method,
            Path = context.Request.Path,
            QueryString = context.Request.QueryString.ToString()
        };

        await auditLogger.LogAsync("HTTP_REQUEST", userId, correlationId, auditPayload);

        await _next(context);

        await auditLogger.LogAsync("HTTP_RESPONSE", userId, correlationId, new
        {
            StatusCode = context.Response.StatusCode
        });
    }
}
