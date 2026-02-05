# HrDesk - AI-Powered HR Help Desk

**Phase 1: Skeleton Infrastructure & System Boundaries**

A production-ready ASP.NET Core 8 backend for an AI-powered HR Help Desk system, designed to integrate with PeopleHum. Phase 1 establishes the architectural skeleton, system boundaries, and traceable commit history without implementing real AI logic or PeopleHum calls.

---

## 📋 Project Overview

**HrDesk** is a modular, clean architecture solution with:

- ✅ API Gateway / Backend-for-Frontend (BFF) pattern
- ✅ JWT/OIDC authentication stub
- ✅ Chat endpoint (`/api/chat`) with intent-based routing
- ✅ AI Orchestrator interface + hard-coded intent classifier
- ✅ PeopleHum client interface + fake adapter
- ✅ Audit logging middleware + console sink
- ✅ Background jobs skeleton (Hangfire)
- ✅ HR Admin API endpoints (`/admin/tickets`, `/admin/escalations`)
- ✅ Swagger UI + health checks
- ✅ Full dependency injection setup
- ✅ Structured logging with Serilog
- ✅ Correlation IDs across requests
- ✅ Unit tests for core components

**Out of Scope (Phase 1):**
- Real LLM integration
- Vector DB / RAG
- Real PeopleHum credentials
- Frontend UI
- Business logic implementation
- Database persistence (Phase 2)

---

## 🏗️ Project Structure

```
HrDesk/
├── HrDesk.sln
├── README.md
├── Directory.Build.props
├── src/
│   ├── HrDesk.Core/
│   │   ├── Models/
│   │   │   ├── UserContext.cs
│   │   │   ├── ChatRequest.cs
│   │   │   └── AiResponse.cs
│   │   ├── Interfaces/
│   │   │   ├── IAiOrchestrator.cs
│   │   │   ├── IPeopleHumClient.cs
│   │   │   └── IAuditLogger.cs
│   │   └── Constants/
│   │       └── Constants.cs
│   ├── HrDesk.Api/
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   ├── Controllers/
│   │   │   ├── ChatController.cs
│   │   │   ├── AdminController.cs
│   │   │   └── HealthController.cs
│   │   └── Services/
│   │       └── UserContextService.cs
│   ├── HrDesk.Ai/
│   │   ├── StubAiOrchestrator.cs
│   │   └── Extensions/ServiceCollectionExtensions.cs
│   ├── HrDesk.PeopleHum/
│   │   ├── FakePeopleHumClient.cs
│   │   └── Extensions/ServiceCollectionExtensions.cs
│   ├── HrDesk.Audit/
│   │   ├── ConsoleAuditLogger.cs
│   │   ├── Middleware/AuditLoggingMiddleware.cs
│   │   └── Extensions/ServiceCollectionExtensions.cs
│   ├── HrDesk.BackgroundJobs/
│   │   ├── SyncPeopleHumJob.cs
│   │   └── Extensions/ServiceCollectionExtensions.cs
│   ├── HrDesk.Admin/
│   │   ├── Models/AdminModels.cs
│   │   ├── Services/AdminService.cs
│   │   └── Extensions/ServiceCollectionExtensions.cs
│   └── HrDesk.Infrastructure/
│       └── InfrastructureMarker.cs
└── tests/
    └── HrDesk.Tests/
        ├── StubAiOrchestratorTests.cs
        ├── FakePeopleHumClientTests.cs
        └── ConsoleAuditLoggerTests.cs
```

---

## 🚀 Getting Started

### Prerequisites

- .NET 8 SDK
- Visual Studio 2022 or VS Code with C# extensions
- Git

### Local Development Setup

1. **Clone the repository:**
   ```bash
   git clone <repo-url>
   cd HR-HELP
   ```

2. **Build the solution:**
   ```bash
   dotnet build
   ```

3. **Run tests:**
   ```bash
   dotnet test
   ```

4. **Start the API:**
   ```bash
   cd src/HrDesk.Api
   dotnet run
   ```

   The API will start on `https://localhost:5001` (HTTPS) or `http://localhost:5000` (HTTP).

### Access the Application

- **Swagger UI:** http://localhost:5000/swagger
- **Health Check:** http://localhost:5000/health
- **Hangfire Dashboard:** http://localhost:5000/hangfire

---

## 🔐 Authentication

The API uses JWT bearer tokens. Since Phase 1 is a stub, tokens are validated but not strictly enforced.

### Example JWT Generation (for testing)

```csharp
// Pseudocode - use a JWT library to generate
var token = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
    issuer: "HrDesk",
    audience: "HrDesk.Api",
    claims: new[]
    {
        new Claim("sub", "EMP-001"),
        new Claim("email", "emp@company.com"),
        new Claim("employee_id", "EMP-001"),
        new Claim("country", "US"),
        new Claim(ClaimTypes.Role, "user")
    },
    expires: DateTime.UtcNow.AddHours(1),
    signingCredentials: new SigningCredentials(
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes("STUB_SECRET_KEY_FOR_DEVELOPMENT_ONLY")),
        SecurityAlgorithms.HmacSha256)
));
```

### Using Tokens with Swagger

1. Generate a JWT token (use jwt.io for testing)
2. Click **Authorize** in Swagger UI
3. Paste: `Bearer YOUR_TOKEN_HERE`

---

## 📡 API Endpoints

### Chat Endpoint

**POST** `/api/chat`

**Headers:**
```
Authorization: Bearer <token>
Content-Type: application/json
```

**Request:**
```json
{
  "message": "I need to request leave next week",
  "ticketId": null,
  "timestamp": "2024-02-05T10:00:00Z"
}
```

**Response:**
```json
{
  "response": "I can help you with leave requests. As an employee in US, your leave policy includes standard annual leave. How many days would you like to request?",
  "intent": "leave_request",
  "escalated": false,
  "escalationReason": null,
  "correlationId": "550e8400-e29b-41d4-a716-446655440000",
  "respondedAt": "2024-02-05T10:00:05Z"
}
```

### Admin Endpoints

**GET** `/api/admin/tickets` (requires `admin` role)

**Response:**
```json
[
  {
    "id": "TKT-001",
    "employeeId": "EMP-001",
    "subject": "Leave Request Pending",
    "status": "Open",
    "createdAt": "2024-02-05T10:00:00Z"
  }
]
```

**GET** `/api/admin/escalations` (requires `admin` role)

**Response:**
```json
[
  {
    "id": "ESC-001",
    "ticketId": "TKT-002",
    "assignedTo": "HR-Manager-001",
    "reason": "Complex leave policy case",
    "escalatedAt": "2024-02-05T10:00:00Z"
  }
]
```

### Health Check

**GET** `/health`

**Response:**
```json
{
  "status": "Healthy"
}
```

---

## 🧪 Testing

### Run All Tests

```bash
dotnet test
```

### Test Coverage

- **StubAiOrchestratorTests**: Intent classification routing
- **FakePeopleHumClientTests**: Stub adapter responses
- **ConsoleAuditLoggerTests**: Audit logging functionality

---

## 🏛️ Architecture Principles

### Clean Architecture
- **Core Project**: Domain models + interfaces (no external dependencies)
- **Implementation Projects**: Concrete implementations
- **API Project**: Entry point, orchestration

### SOLID Principles
- **Single Responsibility**: Each service handles one concern
- **Open/Closed**: Extensible via interfaces
- **Liskov Substitution**: Stub implementations honor contracts
- **Interface Segregation**: Focused, cohesive interfaces
- **Dependency Inversion**: All dependencies injected via DI container

### Async-First
- All I/O operations are async
- Proper use of `CancellationToken`

### Structured Logging
- Serilog for semantic logging
- Correlation IDs for request tracing
- Audit middleware logs all requests

---

## 🔄 Feature Flags

Configuration in `appsettings.json`:

```json
{
  "FeatureFlags": {
    "EnableAiOrchestration": true,
    "EnablePeopleHumIntegration": true,
    "EnableAuditLogging": true
  }
}
```

---

## 📝 Core Models

### UserContext
Extracted from JWT claims:
```csharp
public class UserContext
{
    public string EmployeeId { get; set; }
    public string Email { get; set; }
    public string[] Roles { get; set; }
    public string Country { get; set; }
    public string CorrelationId { get; set; }
}
```

### ChatRequest
User input to the chat system:
```csharp
public class ChatRequest
{
    public string Message { get; set; }
    public string? TicketId { get; set; }
    public DateTime Timestamp { get; set; }
}
```

### AiResponse
Orchestrator output:
```csharp
public class AiResponse
{
    public string Response { get; set; }
    public string Intent { get; set; }          // leave_request, payslip_query, escalation, unknown
    public bool Escalated { get; set; }
    public string? EscalationReason { get; set; }
    public string CorrelationId { get; set; }
    public DateTime RespondedAt { get; set; }
}
```

---

## 🔌 Key Interfaces

### IAiOrchestrator
Handles intent classification and response generation.

```csharp
Task<AiResponse> HandleAsync(ChatRequest request, UserContext userContext, CancellationToken cancellationToken);
```

**Current Implementation:** `StubAiOrchestrator`
- Hard-coded keyword matching for intent classification
- Returns mock responses
- **Phase 2:** Replace with LLM integration

### IPeopleHumClient
Integrates with HR platform.

```csharp
Task<decimal> GetLeaveBalanceAsync(string employeeId, CancellationToken cancellationToken);
Task<bool> SubmitLeaveRequestAsync(string employeeId, DateTime fromDate, DateTime toDate, string reason, CancellationToken cancellationToken);
Task<string> GetPayslipAsync(string employeeId, int month, int year, CancellationToken cancellationToken);
Task<string> CreateTicketAsync(string employeeId, string subject, string description, CancellationToken cancellationToken);
```

**Current Implementation:** `FakePeopleHumClient`
- Returns stub data
- **Phase 2:** Add retry/circuit breaker, real API calls

### IAuditLogger
Logs system events for compliance and debugging.

```csharp
Task LogAsync(string action, string userId, string correlationId, object? payload, CancellationToken cancellationToken);
```

**Current Implementation:** `ConsoleAuditLogger`
- Writes to console via Serilog
- **Phase 2:** Add database sink

---

## 🎯 Intent Classification (Phase 1)

The stub orchestrator uses simple keyword matching:

| Intent | Keywords | Example |
|--------|----------|---------|
| `leave_request` | leave, vacation, time off | "I need to take leave next week" |
| `payslip_query` | payslip, salary, wage | "Can I see my payslip?" |
| `escalation` | urgent, escalate, manager | "Please escalate this" |
| `unknown` | (no match) | "Tell me a joke" |

---

## 🛠️ Background Jobs (Hangfire)

Recurring jobs can be scheduled and monitored via the Hangfire dashboard.

**Current Job:**
- `sync-peoplehum`: Runs every hour (stub implementation)

**Dashboard:** http://localhost:5000/hangfire

---

## 📊 Audit Logging

All requests are logged to console in JSON format:

```
[10:00:05 INF] AUDIT: {"timestamp":"2024-02-05T10:00:05.000Z","action":"HTTP_REQUEST","userId":"EMP-001","correlationId":"550e8400-e29b-41d4-a716-446655440000","payload":{"method":"POST","path":"/api/chat","queryString":""}}
```

---

## 🧬 Dependency Injection

All services are registered in `Program.cs` using extension methods:

```csharp
builder.Services.AddAiOrchestration();         // from HrDesk.Ai
builder.Services.AddPeopleHumConnector();       // from HrDesk.PeopleHum
builder.Services.AddAuditLogging();             // from HrDesk.Audit
builder.Services.AddBackgroundJobs();           // from HrDesk.BackgroundJobs
builder.Services.AddAdminServices();            // from HrDesk.Admin
```

---

## 🚦 Configuration

### appsettings.json

```json
{
  "Serilog": { ... },
  "AllowedHosts": "*",
  "Jwt": {
    "Issuer": "HrDesk",
    "Audience": "HrDesk.Api",
    "SecretKey": "STUB_SECRET_KEY_FOR_DEVELOPMENT_ONLY_REPLACE_IN_PRODUCTION"
  },
  "FeatureFlags": {
    "EnableAiOrchestration": true,
    "EnablePeopleHumIntegration": true,
    "EnableAuditLogging": true
  }
}
```

---

## 📈 Phase 2 Roadmap

- [ ] Real LLM integration (OpenAI, Claude, etc.)
- [ ] Vector database for RAG (embeddings, retrieval)
- [ ] PeopleHum API client with retry/circuit breaker
- [ ] Database persistence (EF Core + SQL Server/PostgreSQL)
- [ ] Redis caching
- [ ] Event-driven architecture (message queue integration)
- [ ] Admin portal UI
- [ ] Employee portal UI
- [ ] Comprehensive error handling
- [ ] Rate limiting and throttling
- [ ] Multi-language support

---

## 🔍 Troubleshooting

### Port Already in Use

```bash
# Find and kill the process on port 5000
lsof -ti:5000 | xargs kill -9
```

### JWT Token Invalid

Ensure:
1. Token issuer matches `Jwt:Issuer` in appsettings.json
2. Token audience matches `Jwt:Audience`
3. Secret key matches the configuration
4. Token has not expired

### Tests Failing

```bash
# Clean and rebuild
dotnet clean
dotnet build
dotnet test --logger "console;verbosity=detailed"
```

---

## 📚 References

- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)
- [JWT Bearer Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/secure-data)
- [Serilog](https://serilog.net/)
- [Hangfire](https://www.hangfire.io/)
- [xUnit.net](https://xunit.net/)

---

## 📄 License

Internal - HrDesk Phase 1 Skeleton Infrastructure

---

## 👥 Contributors

- HR Tech Team

---

## 📞 Support

For issues or questions:
1. Check the README and documentation
2. Review audit logs for correlation IDs
3. Check Swagger UI for API documentation
4. Review unit tests for usage examples

---

**Last Updated:** February 5, 2024
