namespace HrDesk.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HrDesk.Admin.Services;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "admin")]
public class AdminController : ControllerBase
{
    private readonly AdminService _adminService;

    public AdminController(AdminService adminService)
    {
        _adminService = adminService ?? throw new ArgumentNullException(nameof(adminService));
    }

    /// <summary>
    /// Get all support tickets.
    /// </summary>
    [HttpGet("tickets")]
    public async Task<IActionResult> GetTicketsAsync()
    {
        var tickets = await _adminService.GetTicketsAsync();
        return Ok(tickets);
    }

    /// <summary>
    /// Get all escalations.
    /// </summary>
    [HttpGet("escalations")]
    public async Task<IActionResult> GetEscalationsAsync()
    {
        var escalations = await _adminService.GetEscalationsAsync();
        return Ok(escalations);
    }
}
