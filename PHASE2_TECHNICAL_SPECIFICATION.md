# HrDesk Phase 2 Technical Specification

## 1. Database Design

### 1.1 Entity-Relationship Diagram (Conceptual)

```
┌─────────────┐
│  Employee   │ (from PeopleHum API cache)
├─────────────┤
│ ID (PK)     │
│ Name        │
│ Email       │
│ Department  │
│ CachedAt    │
└─────────────┘
       │
       │ 1:N
       ▼
┌──────────────────┐
│   ChatHistory    │
├──────────────────┤
│ ID (PK)          │
│ EmployeeId (FK)  │
│ Message          │
│ Response         │
│ Intent           │
│ CorrelationId    │
│ CreatedAt        │
│ RespondedAt      │
└──────────────────┘
       │
       │ 1:N
       ▼
┌──────────────────┐
│     Ticket       │
├──────────────────┤
│ ID (PK)          │
│ ChatHistoryId(FK)│
│ Subject          │
│ Description      │
│ Status           │ (Open, InProgress, Resolved, Escalated)
│ Priority         │ (Low, Medium, High, Critical)
│ AssignedTo       │ (Admin user)
│ CreatedAt        │
│ UpdatedAt        │
│ ResolvedAt       │
└──────────────────┘
       │
       │ 1:N
       ▼
┌──────────────────┐
│   Escalation     │
├──────────────────┤
│ ID (PK)          │
│ TicketId (FK)    │
│ Reason           │
│ EscalatedBy      │
│ EscalatedTo      │
│ EscalatedAt      │
│ Status           │
└──────────────────┘

┌──────────────────┐
│  AuditLog        │
├──────────────────┤
│ ID (PK)          │
│ UserId           │
│ Action           │
│ Resource         │
│ Details          │
│ CorrelationId    │
│ CreatedAt        │
└──────────────────┘
```

### 1.2 Database Models (C# Classes)

**Location**: `src/HrDesk.Core/Models/`

```csharp
// EmployeeCache.cs
public class EmployeeCache
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Department { get; set; }
    public DateTime CachedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public ICollection<ChatHistory> ChatHistories { get; set; } = new List<ChatHistory>();
}

// ChatHistory.cs
public class ChatHistory
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EmployeeId { get; set; }
    public string Message { get; set; }
    public string Response { get; set; }
    public string Intent { get; set; }
    public decimal Confidence { get; set; }
    public bool NeedsEscalation { get; set; }
    public string CorrelationId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }
    
    // Foreign keys
    public string? TicketId { get; set; }
    public virtual Ticket? Ticket { get; set; }
    public virtual EmployeeCache Employee { get; set; }
}

// Ticket.cs
public class Ticket
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Subject { get; set; }
    public string Description { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public string? AssignedTo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    
    // Navigation
    public ICollection<ChatHistory> ChatHistories { get; set; } = new List<ChatHistory>();
    public ICollection<Escalation> Escalations { get; set; } = new List<Escalation>();
}

// Ticket Status Enum
public enum TicketStatus
{
    Open,
    InProgress,
    Resolved,
    Escalated,
    Closed
}

// Ticket Priority Enum
public enum TicketPriority
{
    Low,
    Medium,
    High,
    Critical
}

// Escalation.cs
public class Escalation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TicketId { get; set; }
    public string Reason { get; set; }
    public string EscalatedBy { get; set; }
    public string? EscalatedTo { get; set; }
    public DateTime EscalatedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending";
    
    // Navigation
    public virtual Ticket Ticket { get; set; }
}

// AuditLog.cs
public class AuditLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; }
    public string Action { get; set; }
    public string Resource { get; set; }
    public string? Details { get; set; }
    public string CorrelationId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

---

## 2. AI Orchestration Layer

### 2.1 LlmOrchestrator Implementation

**Location**: `src/HrDesk.Ai/LlmOrchestrator.cs`

**Key Features**:
- Wraps OpenAI API
- Implements retry logic
- Adds feature flag checks
- Structures responses

```csharp
public class LlmOrchestrator : IAiOrchestrator
{
    private readonly IConfiguration _configuration;
    private readonly OpenAIClient _openAiClient;
    private readonly ILogger<LlmOrchestrator> _logger;
    private readonly IAuditLogger _auditLogger;
    private readonly IPeopleHumClient _peopleHumClient;

    public async Task<AiResponse> HandleAsync(
        ChatRequest request,
        UserContext userContext,
        CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        
        try
        {
            // Check feature flag
            var enableLiveAI = _configuration.GetValue<bool>("FeatureFlags:EnableLiveAI");
            if (!enableLiveAI)
            {
                return GenerateStubResponse(request, correlationId);
            }

            // Get employee context from PeopleHum
            var employee = await _peopleHumClient.GetEmployeeAsync(userContext.EmployeeId, cancellationToken);

            // Build prompt with context
            var prompt = BuildPrompt(request.Message, employee, userContext);

            // Call OpenAI
            var response = await CallOpenAiAsync(prompt, cancellationToken);

            // Parse structured response
            var aiResponse = ParseStructuredResponse(response, correlationId);

            // Log to audit
            await _auditLogger.LogAsync(
                userContext.EmployeeId,
                "AI_QUERY_PROCESSED",
                "ChatRequest",
                $"Intent: {aiResponse.Intent}, Escalated: {aiResponse.Escalated}",
                correlationId,
                cancellationToken);

            return aiResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI orchestration failed for employee {EmployeeId}", userContext.EmployeeId);
            throw;
        }
    }

    private string BuildPrompt(string userMessage, Employee employee, UserContext context)
    {
        // Build a system prompt with HR knowledge and context
        var systemPrompt = @"You are an HR assistant for a company. 
Your job is to:
1. Classify user queries into intents (Leave, Payroll, Benefits, General, Other)
2. Provide relevant, helpful responses based on company HR policies
3. Determine if the issue needs escalation to HR admin
4. Return JSON response with: { intent, confidence (0-1), response, needs_escalation }";

        var userPrompt = $@"Employee: {employee.Name} ({employee.Department})
Message: {userMessage}";

        return $"{systemPrompt}\n\n{userPrompt}";
    }

    private async Task<string> CallOpenAiAsync(string prompt, CancellationToken cancellationToken)
    {
        // Implementation with retry logic (Polly)
        var response = await _openAiClient.GetChatCompletionsAsync(
            new ChatCompletionsOptions
            {
                DeploymentName = "gpt-3.5-turbo",
                Messages = { new ChatMessage(ChatRole.User, prompt) },
                Temperature = 0.7f,
                MaxTokens = 500
            },
            cancellationToken);

        return response.Value.Choices[0].Message.Content;
    }

    private AiResponse ParseStructuredResponse(string rawResponse, string correlationId)
    {
        // Parse JSON from OpenAI response
        // Example: { "intent": "Leave", "confidence": 0.95, "response": "...", "needs_escalation": false }
        var jsonResponse = JsonConvert.DeserializeObject<dynamic>(rawResponse);

        return new AiResponse
        {
            Intent = jsonResponse.intent,
            Response = jsonResponse.response,
            Escalated = jsonResponse.needs_escalation,
            CorrelationId = correlationId,
            RespondedAt = DateTime.UtcNow
        };
    }

    private AiResponse GenerateStubResponse(ChatRequest request, string correlationId)
    {
        return new AiResponse
        {
            Intent = "General",
            Response = "[STUB] Unable to process - Live AI disabled",
            Escalated = false,
            CorrelationId = correlationId,
            RespondedAt = DateTime.UtcNow
        };
    }
}
```

### 2.2 Prompt Engineering Strategy

**System Prompt Template**:
```
You are an HR assistant for {CompanyName}.

Your responsibilities:
1. Classify queries into: Leave, Payroll, Benefits, General, Technical, Other
2. Provide accurate HR policy guidance
3. Escalate complex or compliance-critical issues
4. Always be professional and empathetic

Return JSON:
{
  "intent": "string",
  "confidence": 0.0-1.0,
  "response": "string",
  "needs_escalation": boolean,
  "escalation_reason": "string"
}

Company Policies:
- Annual Leave: 20 days
- Sick Leave: 10 days
- Maternity: 3 months
```

---

## 3. PeopleHum Integration

### 3.1 Real PeopleHumClient Implementation

**Location**: `src/HrDesk.PeopleHum/PeopleHumClient.cs`

```csharp
public class PeopleHumClient : IPeopleHumClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly HrDeskDbContext _dbContext;
    private readonly ILogger<PeopleHumClient> _logger;
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;

    public PeopleHumClient(
        HttpClient httpClient,
        IConfiguration configuration,
        HrDeskDbContext dbContext,
        ILogger<PeopleHumClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _dbContext = dbContext;
        _logger = logger;
        _retryPolicy = CreateRetryPolicy();
        
        var baseUrl = configuration["PeopleHum:BaseUrl"];
        var apiKey = configuration["PeopleHum:ApiKey"];
        
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }

    public async Task<decimal> GetLeaveBalanceAsync(
        string employeeId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _retryPolicy.WrapAsync(
                _httpClient.GetAsync($"/api/v1/employees/{employeeId}/leave-balance", cancellationToken)
            );

            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var data = JsonConvert.DeserializeObject<dynamic>(json);

            return (decimal)data.balance;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "PeopleHum API error fetching leave balance for {EmployeeId}", employeeId);
            throw;
        }
    }

    public async Task<Employee> GetEmployeeAsync(
        string employeeId,
        CancellationToken cancellationToken = default)
    {
        // Check cache first (valid for 1 hour)
        var cached = await _dbContext.EmployeeCaches
            .FirstOrDefaultAsync(e => e.Id == employeeId && 
                DateTime.UtcNow.Subtract(e.CachedAt).TotalHours < 1, 
            cancellationToken);

        if (cached != null)
        {
            return new Employee 
            { 
                Id = cached.Id, 
                Name = cached.Name, 
                Email = cached.Email, 
                Department = cached.Department 
            };
        }

        // Fetch from API
        var response = await _retryPolicy.WrapAsync(
            _httpClient.GetAsync($"/api/v1/employees/{employeeId}", cancellationToken)
        );

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var data = JsonConvert.DeserializeObject<dynamic>(json);

        var employee = new Employee
        {
            Id = data.id,
            Name = data.name,
            Email = data.email,
            Department = data.department
        };

        // Update cache
        var cacheEntry = new EmployeeCache
        {
            Id = employee.Id,
            Name = employee.Name,
            Email = employee.Email,
            Department = employee.Department,
            CachedAt = DateTime.UtcNow
        };

        await _dbContext.EmployeeCaches.AddAsync(cacheEntry, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return employee;
    }

    private IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "PeopleHum API retry {RetryCount} after {Delay}ms",
                        retryCount,
                        timespan.TotalMilliseconds);
                });
    }
}
```

### 3.2 HttpClient Factory Configuration

**In Program.cs**:
```csharp
builder.Services.AddHttpClient<IPeopleHumClient, PeopleHumClient>()
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .AddTransientHttpErrorPolicy(p =>
        p.WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(2)));
```

---

## 4. Background Jobs

### 4.1 SyncPeopleHumJob

**Location**: `src/HrDesk.BackgroundJobs/Jobs/SyncPeopleHumJob.cs`

```csharp
public class SyncPeopleHumJob
{
    private readonly IPeopleHumClient _peopleHumClient;
    private readonly HrDeskDbContext _dbContext;
    private readonly ILogger<SyncPeopleHumJob> _logger;

    public SyncPeopleHumJob(
        IPeopleHumClient peopleHumClient,
        HrDeskDbContext dbContext,
        ILogger<SyncPeopleHumJob> logger)
    {
        _peopleHumClient = peopleHumClient;
        _dbContext = dbContext;
        _logger = logger;
    }

    [JobDisplayName("Sync PeopleHum Employee Cache")]
    public async Task Execute(PerformContext context)
    {
        _logger.LogInformation("Starting PeopleHum sync job");
        var jobId = context.BackgroundJob.Id;

        try
        {
            // Fetch all employees from PeopleHum
            var employees = await _peopleHumClient.GetAllEmployeesAsync();

            // Update cache
            foreach (var employee in employees)
            {
                var existing = await _dbContext.EmployeeCaches
                    .FirstOrDefaultAsync(e => e.Id == employee.Id);

                if (existing != null)
                {
                    existing.Name = employee.Name;
                    existing.Email = employee.Email;
                    existing.Department = employee.Department;
                    existing.CachedAt = DateTime.UtcNow;
                }
                else
                {
                    await _dbContext.EmployeeCaches.AddAsync(new EmployeeCache
                    {
                        Id = employee.Id,
                        Name = employee.Name,
                        Email = employee.Email,
                        Department = employee.Department,
                        CachedAt = DateTime.UtcNow
                    });
                }
            }

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("PeopleHum sync completed. Synced {Count} employees", employees.Count);
            context.SetJobParameter("SyncedCount", employees.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PeopleHum sync job failed");
            throw;
        }
    }
}
```

### 4.2 TicketEscalationJob

**Location**: `src/HrDesk.BackgroundJobs/Jobs/TicketEscalationJob.cs`

```csharp
public class TicketEscalationJob
{
    private readonly HrDeskDbContext _dbContext;
    private readonly IAdminService _adminService;
    private readonly ILogger<TicketEscalationJob> _logger;

    [JobDisplayName("Auto-Escalate Old Tickets")]
    public async Task Execute(PerformContext context)
    {
        _logger.LogInformation("Starting ticket escalation job");

        try
        {
            // Find tickets open for > 24 hours
            var escalationThreshold = DateTime.UtcNow.AddHours(-24);
            var oldTickets = await _dbContext.Tickets
                .Where(t => t.Status == TicketStatus.Open && t.CreatedAt < escalationThreshold)
                .ToListAsync();

            foreach (var ticket in oldTickets)
            {
                ticket.Status = TicketStatus.Escalated;
                
                var escalation = new Escalation
                {
                    TicketId = ticket.Id,
                    Reason = "Auto-escalation: Ticket open for 24+ hours",
                    EscalatedBy = "System",
                    EscalatedAt = DateTime.UtcNow
                };

                await _dbContext.Escalations.AddAsync(escalation);
            }

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Escalated {Count} tickets", oldTickets.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ticket escalation job failed");
            throw;
        }
    }
}
```

### 4.3 Hangfire Configuration

**In Program.cs**:
```csharp
// Add Hangfire
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("HrDeskDb")));

builder.Services.AddHangfireServer();

// ... later in middleware setup ...

app.UseHangfireDashboard("/hangfire");

// Schedule recurring jobs
RecurringJob.AddOrUpdate<SyncPeopleHumJob>(
    "sync-peoplehum",
    job => job.Execute(null),
    Cron.Hourly,
    new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

RecurringJob.AddOrUpdate<TicketEscalationJob>(
    "escalate-tickets",
    job => job.Execute(null),
    Cron.MinuteInterval(30),
    new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
```

---

## 5. Feature Flags

### 5.1 Feature Flag Storage

**appsettings.json**:
```json
{
  "FeatureFlags": {
    "EnableLiveAI": false,
    "EnablePeopleHumSync": true,
    "EnableEscalations": true,
    "EnableRAG": false,
    "EnableApplicationInsights": false
  }
}
```

### 5.2 Feature Flag Service (Optional)

```csharp
public interface IFeatureFlagService
{
    bool IsEnabled(string featureName);
    Task SetAsync(string featureName, bool enabled);
}

public class FeatureFlagService : IFeatureFlagService
{
    private readonly IConfiguration _configuration;

    public bool IsEnabled(string featureName)
    {
        return _configuration.GetValue<bool>($"FeatureFlags:{featureName}");
    }

    public async Task SetAsync(string featureName, bool enabled)
    {
        // Store in database or cache
        // Allows runtime feature toggle without restart
    }
}
```

---

## 6. Appsettings Structure

```json
{
  "ConnectionStrings": {
    "HrDeskDb": "Server=localhost;Database=HrDesk;Integrated Security=true;"
  },
  "Jwt": {
    "SecretKey": "your-very-secret-key-minimum-32-characters",
    "Issuer": "HrDesk",
    "Audience": "HrDesk-Users",
    "ExpirationMinutes": 60
  },
  "OpenAI": {
    "ApiKey": "sk-...",
    "Model": "gpt-3.5-turbo",
    "Temperature": 0.7,
    "MaxTokens": 500
  },
  "PeopleHum": {
    "BaseUrl": "https://api.peoplehum.com",
    "ApiKey": "ph-...",
    "SyncIntervalMinutes": 60
  },
  "ApplicationInsights": {
    "InstrumentationKey": "..."
  },
  "Hangfire": {
    "ConnectionString": "Server=localhost;Database=Hangfire;Integrated Security=true;"
  },
  "FeatureFlags": {
    "EnableLiveAI": false,
    "EnablePeopleHumSync": true,
    "EnableEscalations": true,
    "EnableRAG": false,
    "EnableApplicationInsights": false
  }
}
```

---

## 7. NuGet Packages Required

### Phase 2.1 (Database)
```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

### Phase 2.2 (AI)
```bash
dotnet add package Azure.AI.OpenAI
dotnet add package Newtonsoft.Json
```

### Phase 2.3 (PeopleHum)
```bash
dotnet add package Polly
dotnet add package Polly.CircuitBreaker
```

### Phase 2.4 (Background Jobs)
```bash
dotnet add package Hangfire.Core
dotnet add package Hangfire.SqlServer
```

### Phase 2.6 (Logging)
```bash
dotnet add package Microsoft.ApplicationInsights.AspNetCore
dotnet add package Serilog.Sinks.ApplicationInsights
dotnet add package FluentValidation
```

---

## 8. Migration & Deployment

### 8.1 EF Core Migrations

```bash
# Create initial migration
dotnet ef migrations add InitialCreate --project src/HrDesk.Infrastructure --startup-project src/HrDesk.Api

# Apply migrations
dotnet ef database update --project src/HrDesk.Infrastructure --startup-project src/HrDesk.Api

# In production, run on startup in Program.cs:
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HrDeskDbContext>();
    db.Database.Migrate();
}
```

### 8.2 Docker Deployment

**Dockerfile**:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY . .
EXPOSE 80 443
ENTRYPOINT ["dotnet", "HrDesk.Api.dll"]
```

**docker-compose.yml**:
```yaml
version: '3.8'
services:
  api:
    build: .
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__HrDeskDb=Server=db;Database=HrDesk;User=sa;Password=...
    depends_on:
      - db

  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=...
    ports:
      - "1433:1433"
```

---

## 9. Security Considerations

### 9.1 Secrets Management

**Development (appsettings.Development.json)**:
```json
{
  "OpenAI:ApiKey": "sk-test-...",
  "PeopleHum:ApiKey": "ph-test-..."
}
```

**Production (Environment Variables)**:
```bash
export OPENAI_API_KEY="sk-prod-..."
export PEOPLEHUM_API_KEY="ph-prod-..."
export JWT_SECRET_KEY="..."
export APPLICATIONINSIGHTS_CONNECTION_STRING="..."
```

### 9.2 Role-Based Authorization

```csharp
public enum UserRole
{
    Employee,
    HRAdmin,
    SystemAdmin
}

// In Controllers
[Authorize(Roles = "HRAdmin,SystemAdmin")]
[HttpGet("/admin/tickets")]
public async Task<IActionResult> GetTickets()
{
    // Implementation
}
```

---

## Next Steps

1. Choose database platform (SQL Server, PostgreSQL)
2. Provision database instance
3. Create `HrDeskDbContext` and run migrations
4. Implement `LlmOrchestrator` with OpenAI integration
5. Implement real `PeopleHumClient`
6. Configure Hangfire and background jobs
7. Add comprehensive logging and Application Insights
8. Create integration tests
9. Deploy to production infrastructure
