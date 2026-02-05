# HrDesk Phase 2: Quick-Start Implementation Guide

## Week 1-2: Foundation (Database Layer)

### Task 1: Set up Database Infrastructure

#### Step 1.1: Add NuGet Packages
```bash
cd /Users/michaelnyemudzo/HR-HELP

# Add EF Core packages
dotnet add src/HrDesk.Infrastructure/HrDesk.Infrastructure.csproj package Microsoft.EntityFrameworkCore
dotnet add src/HrDesk.Infrastructure/HrDesk.Infrastructure.csproj package Microsoft.EntityFrameworkCore.SqlServer
dotnet add src/HrDesk.Infrastructure/HrDesk.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Tools
dotnet add src/HrDesk.Api/HrDesk.Api.csproj package Microsoft.EntityFrameworkCore.Design
```

#### Step 1.2: Create HrDeskDbContext
**File**: `src/HrDesk.Infrastructure/Data/HrDeskDbContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using HrDesk.Core.Models;

namespace HrDesk.Infrastructure.Data;

public class HrDeskDbContext : DbContext
{
    public HrDeskDbContext(DbContextOptions<HrDeskDbContext> options) : base(options) { }

    public DbSet<EmployeeCache> EmployeeCaches { get; set; }
    public DbSet<ChatHistory> ChatHistories { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Escalation> Escalations { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // EmployeeCache
        modelBuilder.Entity<EmployeeCache>()
            .HasKey(e => e.Id);
        modelBuilder.Entity<EmployeeCache>()
            .HasMany(e => e.ChatHistories)
            .WithOne(c => c.Employee)
            .HasForeignKey(c => c.EmployeeId);

        // ChatHistory
        modelBuilder.Entity<ChatHistory>()
            .HasKey(c => c.Id);
        modelBuilder.Entity<ChatHistory>()
            .HasOne(c => c.Ticket)
            .WithMany(t => t.ChatHistories)
            .HasForeignKey(c => c.TicketId)
            .IsRequired(false);

        // Ticket
        modelBuilder.Entity<Ticket>()
            .HasKey(t => t.Id);
        modelBuilder.Entity<Ticket>()
            .HasMany(t => t.Escalations)
            .WithOne(e => e.Ticket)
            .HasForeignKey(e => e.TicketId);

        // Escalation
        modelBuilder.Entity<Escalation>()
            .HasKey(e => e.Id);

        // AuditLog
        modelBuilder.Entity<AuditLog>()
            .HasKey(a => a.Id);

        // Indexes for performance
        modelBuilder.Entity<ChatHistory>()
            .HasIndex(c => c.EmployeeId);
        modelBuilder.Entity<ChatHistory>()
            .HasIndex(c => c.CorrelationId);
        modelBuilder.Entity<Ticket>()
            .HasIndex(t => t.Status);
        modelBuilder.Entity<Escalation>()
            .HasIndex(e => e.TicketId);
    }
}
```

#### Step 1.3: Add Connection String
**File**: `src/HrDesk.Api/appsettings.json`

Add/modify the `ConnectionStrings` section:
```json
{
  "ConnectionStrings": {
    "HrDeskDb": "Server=(localdb)\\mssqllocaldb;Database=HrDesk;Integrated Security=true;"
  },
  ...
}
```

For development on macOS with Docker:
```json
{
  "ConnectionStrings": {
    "HrDeskDb": "Server=localhost,1433;Database=HrDesk;User Id=sa;Password=HrDesk@2024;"
  },
  ...
}
```

#### Step 1.4: Create EF Core Migration
```bash
cd /Users/michaelnyemudzo/HR-HELP

# Create migration
dotnet ef migrations add InitialCreate \
  --project src/HrDesk.Infrastructure \
  --startup-project src/HrDesk.Api \
  --context HrDeskDbContext \
  --output-dir Data/Migrations

# Verify migration was created
ls src/HrDesk.Infrastructure/Data/Migrations/
```

#### Step 1.5: Register DbContext in DI
**File**: `src/HrDesk.Api/Program.cs`

Add after line 11 (after other service configurations):
```csharp
// Add Database
var connectionString = builder.Configuration.GetConnectionString("HrDeskDb") 
    ?? throw new InvalidOperationException("Connection string 'HrDeskDb' not found.");

builder.Services.AddDbContext<HrDeskDbContext>(options =>
    options.UseSqlServer(connectionString));
```

#### Step 1.6: Add Database Initialization
**File**: `src/HrDesk.Api/Program.cs`

Add before `app.Run()`:
```csharp
// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HrDeskDbContext>();
    
    // Create database if it doesn't exist and run migrations
    db.Database.Migrate();
    
    Log.Information("Database initialized");
}
```

### Task 2: Create Database Models

#### Step 2.1: Create/Update Core Models
**Files to create/update** in `src/HrDesk.Core/Models/`:

**EmployeeCache.cs**:
```csharp
namespace HrDesk.Core.Models;

public class EmployeeCache
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public DateTime CachedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<ChatHistory> ChatHistories { get; set; } = new List<ChatHistory>();
}
```

**ChatHistory.cs** (new file):
```csharp
namespace HrDesk.Core.Models;

public class ChatHistory
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EmployeeId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public string Intent { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
    public bool NeedsEscalation { get; set; }
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }

    // Foreign keys
    public string? TicketId { get; set; }

    // Navigation properties
    public virtual Ticket? Ticket { get; set; }
    public virtual EmployeeCache? Employee { get; set; }
}
```

**Ticket.cs** (new file):
```csharp
namespace HrDesk.Core.Models;

public class Ticket
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public string? AssignedTo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    // Navigation properties
    public virtual ICollection<ChatHistory> ChatHistories { get; set; } = new List<ChatHistory>();
    public virtual ICollection<Escalation> Escalations { get; set; } = new List<Escalation>();
}

public enum TicketStatus
{
    Open,
    InProgress,
    Resolved,
    Escalated,
    Closed
}

public enum TicketPriority
{
    Low,
    Medium,
    High,
    Critical
}
```

**Escalation.cs** (new file):
```csharp
namespace HrDesk.Core.Models;

public class Escalation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TicketId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string EscalatedBy { get; set; } = string.Empty;
    public string? EscalatedTo { get; set; }
    public DateTime EscalatedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending";

    // Navigation properties
    public virtual Ticket? Ticket { get; set; }
}
```

**AuditLog.cs** (new file):
```csharp
namespace HrDesk.Core.Models;

public class AuditLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

#### Step 2.2: Update AiResponse Model
**File**: `src/HrDesk.Core/Models/AiResponse.cs`

```csharp
namespace HrDesk.Core.Models;

public class AiResponse
{
    public string Response { get; set; } = string.Empty;
    public string Intent { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
    public bool Escalated { get; set; }
    public string? EscalationReason { get; set; }
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    public DateTime RespondedAt { get; set; } = DateTime.UtcNow;
}
```

#### Step 2.3: Update PeopleHumClient Interface
**File**: `src/HrDesk.Core/Interfaces/IPeopleHumClient.cs`

Add this method:
```csharp
Task<Employee> GetEmployeeAsync(string employeeId, CancellationToken cancellationToken = default);
Task<IEnumerable<Employee>> GetAllEmployeesAsync(CancellationToken cancellationToken = default);
```

Add Employee model to Core:
**File**: `src/HrDesk.Core/Models/Employee.cs` (new):
```csharp
namespace HrDesk.Core.Models;

public class Employee
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
}
```

### Task 3: Test Database Connection

#### Step 3.1: Build Solution
```bash
cd /Users/michaelnyemudzo/HR-HELP
dotnet build
```

#### Step 3.2: Apply Migration (Create Database)
```bash
# Option 1: Using local SQL Server
dotnet ef database update \
  --project src/HrDesk.Infrastructure \
  --startup-project src/HrDesk.Api

# Option 2: Using Docker SQL Server (if set up)
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=HrDesk@2024" \
  -p 1433:1433 \
  -d mcr.microsoft.com/mssql/server:2022-latest

# Then run migration
dotnet ef database update \
  --project src/HrDesk.Infrastructure \
  --startup-project src/HrDesk.Api
```

#### Step 3.3: Verify Database
```bash
# Connect to database and verify tables exist
# Using sqlcmd (Windows) or other SQL client
sqlcmd -S localhost -U sa -P HrDesk@2024 -Q "SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'"
```

Expected output:
```
EmployeeCaches
ChatHistories
Tickets
Escalations
AuditLogs
__EFMigrationsHistory
```

### Task 4: Update API to Use Database

#### Step 4.1: Create Chat Service
**File**: `src/HrDesk.Api/Services/ChatService.cs` (new):

```csharp
using HrDesk.Core.Models;
using HrDesk.Infrastructure.Data;

namespace HrDesk.Api.Services;

public interface IChatService
{
    Task<ChatHistory> SaveChatAsync(string employeeId, string message, AiResponse aiResponse, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChatHistory>> GetChatHistoryAsync(string employeeId, CancellationToken cancellationToken = default);
}

public class ChatService : IChatService
{
    private readonly HrDeskDbContext _dbContext;
    private readonly ILogger<ChatService> _logger;

    public ChatService(HrDeskDbContext dbContext, ILogger<ChatService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ChatHistory> SaveChatAsync(
        string employeeId,
        string message,
        AiResponse aiResponse,
        CancellationToken cancellationToken = default)
    {
        var chatHistory = new ChatHistory
        {
            EmployeeId = employeeId,
            Message = message,
            Response = aiResponse.Response,
            Intent = aiResponse.Intent,
            Confidence = aiResponse.Confidence,
            NeedsEscalation = aiResponse.Escalated,
            CorrelationId = aiResponse.CorrelationId,
            RespondedAt = DateTime.UtcNow
        };

        await _dbContext.ChatHistories.AddAsync(chatHistory, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Chat saved: Employee={EmployeeId}, CorrelationId={CorrelationId}",
            employeeId,
            aiResponse.CorrelationId);

        return chatHistory;
    }

    public async Task<IEnumerable<ChatHistory>> GetChatHistoryAsync(
        string employeeId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ChatHistories
            .Where(c => c.EmployeeId == employeeId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
```

#### Step 4.2: Register Services in DI
**File**: `src/HrDesk.Api/Program.cs`

Add after DbContext registration:
```csharp
// Add Application Services
builder.Services.AddScoped<IChatService, ChatService>();
```

#### Step 4.3: Update ChatController
**File**: `src/HrDesk.Api/Controllers/ChatController.cs`

Update to use database:
```csharp
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
    private readonly IChatService _chatService;
    private readonly UserContextService _userContextService;
    private readonly IAuditLogger _auditLogger;

    public ChatController(
        IAiOrchestrator aiOrchestrator,
        IChatService chatService,
        UserContextService userContextService,
        IAuditLogger auditLogger)
    {
        _aiOrchestrator = aiOrchestrator;
        _chatService = chatService;
        _userContextService = userContextService;
        _auditLogger = auditLogger;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        var userContext = _userContextService.GetUserContext(User);

        var aiResponse = await _aiOrchestrator.HandleAsync(request, userContext, cancellationToken);

        // Save to database
        var chatHistory = await _chatService.SaveChatAsync(
            userContext.EmployeeId,
            request.Message,
            aiResponse,
            cancellationToken);

        await _auditLogger.LogAsync(
            userContext.EmployeeId,
            "CHAT_MESSAGE_SENT",
            "ChatMessage",
            $"Intent: {aiResponse.Intent}",
            aiResponse.CorrelationId,
            cancellationToken);

        return Ok(new
        {
            id = chatHistory.Id,
            response = aiResponse.Response,
            intent = aiResponse.Intent,
            confidence = aiResponse.Confidence,
            escalated = aiResponse.Escalated,
            correlationId = aiResponse.CorrelationId,
            respondedAt = aiResponse.RespondedAt
        });
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(CancellationToken cancellationToken)
    {
        var userContext = _userContextService.GetUserContext(User);
        var history = await _chatService.GetChatHistoryAsync(userContext.EmployeeId, cancellationToken);

        return Ok(history.Select(c => new
        {
            id = c.Id,
            message = c.Message,
            response = c.Response,
            intent = c.Intent,
            confidence = c.Confidence,
            createdAt = c.CreatedAt,
            respondedAt = c.RespondedAt
        }));
    }
}
```

### Task 5: Build and Test

```bash
cd /Users/michaelnyemudzo/HR-HELP
dotnet clean
dotnet build
dotnet run --project src/HrDesk.Api

# In another terminal, test the API
curl -X GET https://localhost:5001/health
```

---

## Week 3-4: AI Integration

### (Instructions for AI Orchestration implementation coming next...)

---

## Checklist

### Week 1-2 Database Foundation
- [ ] EF Core packages added
- [ ] HrDeskDbContext created
- [ ] Database models created (EmployeeCache, ChatHistory, Ticket, Escalation, AuditLog)
- [ ] Migration created and applied
- [ ] Database verified with tables
- [ ] ChatService implemented
- [ ] ChatController updated with database calls
- [ ] Application builds successfully
- [ ] API tests passing

### Success Criteria
✅ `dotnet build` succeeds  
✅ Database tables exist  
✅ POST /api/chat/send saves to database  
✅ GET /api/chat/history returns chat history  
✅ Unit tests pass for ChatService

---

## Troubleshooting

### Issue: "Invalid connection string"
**Solution**: Verify `appsettings.json` has correct `ConnectionStrings:HrDeskDb`

### Issue: "Package not found"
**Solution**: Run `dotnet restore` then `dotnet add` commands

### Issue: Migration conflicts
**Solution**: 
```bash
dotnet ef migrations remove --project src/HrDesk.Infrastructure --startup-project src/HrDesk.Api
dotnet ef migrations add InitialCreate --project src/HrDesk.Infrastructure --startup-project src/HrDesk.Api
```

### Issue: "DbContext not found"
**Solution**: Ensure `using HrDesk.Infrastructure.Data;` is in Program.cs

---

## Next: Week 3-4 AI Integration Guide
(Will follow after database foundation is complete)
