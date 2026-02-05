namespace HrDesk.Core.Constants;

public static class FeatureFlags
{
    public const string EnableAiOrchestration = "EnableAiOrchestration";
    public const string EnablePeopleHumIntegration = "EnablePeopleHumIntegration";
    public const string EnableAuditLogging = "EnableAuditLogging";
}

public static class ClaimTypes
{
    public const string EmployeeId = "employee_id";
    public const string Country = "country";
    public const string Role = "role";
}

public static class IntentClassification
{
    public const string LeaveRequest = "leave_request";
    public const string PayslipQuery = "payslip_query";
    public const string Escalation = "escalation";
    public const string Unknown = "unknown";
}
