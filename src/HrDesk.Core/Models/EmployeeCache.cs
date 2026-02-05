using System;

namespace HrDesk.Core.Models;

public class EmployeeCache
{
    public int Id { get; set; }
    public string EmployeeId { get; set; } = string.Empty; // ID from PeopleHum
    public string DataJson { get; set; } = string.Empty; // Full JSON response
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
