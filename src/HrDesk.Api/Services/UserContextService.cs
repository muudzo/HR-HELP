namespace HrDesk.Api.Services;

using HrDesk.Core.Models;
using HrDesk.Core.Constants;
using System.Security.Claims;

/// <summary>
/// Service to extract UserContext from JWT claims.
/// </summary>
public class UserContextService
{
    public UserContext? ExtractFromClaims(ClaimsPrincipal? user, string correlationId)
    {
        if (user == null || !user.Identity?.IsAuthenticated ?? false)
            return null;

        var employeeId = user.FindFirst(ClaimTypes.EmployeeId)?.Value ?? user.FindFirst("sub")?.Value ?? "Unknown";
        var email = user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "unknown@company.com";
        var country = user.FindFirst(ClaimTypes.Country)?.Value ?? "US";
        var roles = user.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToArray();

        return new UserContext
        {
            EmployeeId = employeeId,
            Email = email,
            Roles = roles,
            Country = country,
            CorrelationId = correlationId
        };
    }
}
