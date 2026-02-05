# HrDesk Phase 2 Implementation Roadmap

## Current State Assessment

### ✅ Existing (Good Foundation)
- **HrDesk.Core**: Core interfaces and models defined
  - `IAiOrchestrator`, `IAuditLogger`, `IPeopleHumClient` interfaces
  - `ChatRequest`, `AiResponse`, `UserContext` models
  - Basic constants defined
- **HrDesk.Api**: Entry point with controllers
  - `ChatController`, `AdminController`, `HealthController` stubs
  - JWT authentication configured in `Program.cs`
  - Dependency injection wired up
- **HrDesk.Audit**: Audit logging infrastructure
  - `AuditLoggingMiddleware`
  - `ConsoleAuditLogger` (stub implementation)
- **HrDesk.Admin**: Admin service stubs
- **HrDesk.BackgroundJobs**: Hangfire-ready job structure
- **HrDesk.PeopleHum**: `FakePeopleHumClient` placeholder
- **Build**: Compiles successfully

### ⚠️ Critical Gaps (Phase 2 Requirements)

#### 1. **Database & Persistence Layer**
- ❌ EF Core DbContext not created
- ❌ Database models for:
  - `ChatHistory`
  - `Ticket`
  - `Escalation`
  - `EmployeeCache`
- ❌ Migrations infrastructure missing
- ❌ Connection string configuration incomplete

#### 2. **Real AI Integration**
- ❌ `LlmOrchestrator` not implemented (only stub in `HrDesk.Ai`)
- ❌ OpenAI/LLM API client not created
- ❌ Structured JSON response generation
- ❌ Feature flags not implemented

#### 3. **PeopleHum Integration**
- ❌ Real `PeopleHumClient` implementation (only fake exists)
- ❌ API authentication/key management
- ❌ Employee cache synchronization
- ❌ Cache invalidation logic

#### 4. **Background Jobs**
- ❌ `SyncPeopleHumJob` not fully implemented
- ❌ `TicketEscalationJob` missing
- ❌ Hangfire configuration incomplete
- ❌ Retry policies not defined

#### 5. **Admin & Escalation Services**
- ❌ Ticket management API
- ❌ Escalation workflow logic
- ❌ Admin service implementation

#### 6. **Advanced Logging & Observability**
- ❌ Application Insights integration
- ❌ Structured logging with correlation IDs
- ❌ Error/exception tracking

#### 7. **Security Enhancements**
- ❌ Role-based authorization (Employee vs Admin)
- ❌ Input validation/sanitization
- ❌ Secrets management (LLM keys, PeopleHum keys)

#### 8. **Testing**
- ❌ Integration tests for DB + API + PeopleHum
- ❌ AI orchestration unit tests
- ❌ Mocking strategy for external services

---

## Implementation Phases

### Phase 2.1: Foundation (Database & Models)
**Goal**: Establish persistence layer

**Tasks**:
1. Create `HrDeskDbContext` with EF Core
   - DbSet<ChatHistory>
   - DbSet<Ticket>
   - DbSet<Escalation>
   - DbSet<EmployeeCache>
2. Define domain models in Core layer
3. Configure connection string in `appsettings.json`
4. Create initial EF Core migration
5. Add database seeding for test data

**NuGet Packages to add**:
- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.SqlServer` (or preferred DB)
- `Microsoft.EntityFrameworkCore.Tools`

**Files to create/modify**:
- `src/HrDesk.Infrastructure/Data/HrDeskDbContext.cs` (NEW)
- `src/HrDesk.Core/Models/ChatHistory.cs` (NEW)
- `src/HrDesk.Core/Models/Ticket.cs` (NEW)
- `src/HrDesk.Core/Models/Escalation.cs` (NEW)
- `src/HrDesk.Core/Models/EmployeeCache.cs` (NEW)
- `src/HrDesk.Infrastructure/Migrations/` (NEW - EF Core migrations)
- `appsettings.json` (MODIFY - add connection string)
- `Program.cs` (MODIFY - add DbContext DI)

---

### Phase 2.2: AI Integration (LLM Orchestration)
**Goal**: Replace stub AI with real LLM

**Tasks**:
1. Create `LlmOrchestrator` implementing `IAiOrchestrator`
2. Integrate OpenAI SDK
3. Implement intent classification logic
4. Add feature flag support (`EnableLiveAI`)
5. Implement fallback to stub AI if flag disabled
6. Add structured response parsing

**NuGet Packages to add**:
- `Azure.AI.OpenAI` or `OpenAI` (official SDK)
- `Microsoft.Extensions.Configuration`

**Files to create/modify**:
- `src/HrDesk.Ai/LlmOrchestrator.cs` (NEW)
- `src/HrDesk.Ai/Extensions/ServiceCollectionExtensions.cs` (MODIFY)
- `src/HrDesk.Core/Models/FeatureFlags.cs` (NEW)
- `src/HrDesk.Api/Program.cs` (MODIFY - feature flag configuration)
- `appsettings.json` (MODIFY - add OpenAI keys and feature flags)

---

### Phase 2.3: PeopleHum Live Integration
**Goal**: Replace fake PeopleHum client with real API

**Tasks**:
1. Create `PeopleHumClient` implementing `IPeopleHumClient`
2. Set up HTTP client configuration
3. Implement employee fetch + caching logic
4. Add cache invalidation
5. Create `SyncPeopleHumJob` for periodic sync
6. Handle API errors gracefully

**NuGet Packages to add**:
- `HttpClientFactory` (built-in with .NET)

**Files to create/modify**:
- `src/HrDesk.PeopleHum/PeopleHumClient.cs` (NEW - replace fake)
- `src/HrDesk.PeopleHum/Extensions/ServiceCollectionExtensions.cs` (MODIFY)
- `src/HrDesk.BackgroundJobs/SyncPeopleHumJob.cs` (MODIFY - implement)
- `src/HrDesk.Infrastructure/Data/HrDeskDbContext.cs` (MODIFY - add EmployeeCache DbSet)
- `appsettings.json` (MODIFY - PeopleHum API key and endpoint)

---

### Phase 2.4: Background Jobs & Escalation
**Goal**: Automate recurring tasks

**Tasks**:
1. Configure Hangfire (SQL Server or in-memory for dev)
2. Implement `TicketEscalationJob`
3. Set job scheduling (SyncPeopleHum every hour, Escalation every 30 mins)
4. Add job failure notifications
5. Create job monitoring dashboard
6. Implement retry policies

**NuGet Packages to add**:
- `Hangfire.Core`
- `Hangfire.SqlServer` (if using SQL Server)

**Files to create/modify**:
- `src/HrDesk.BackgroundJobs/Jobs/TicketEscalationJob.cs` (NEW)
- `src/HrDesk.BackgroundJobs/Extensions/ServiceCollectionExtensions.cs` (MODIFY)
- `src/HrDesk.Api/Program.cs` (MODIFY - Hangfire middleware)
- `appsettings.json` (MODIFY - Hangfire settings)

---

### Phase 2.5: Ticket & Escalation Management
**Goal**: Provide admin APIs for ticket management

**Tasks**:
1. Implement `IAdminService` interface
2. Create ticket CRUD operations
3. Implement escalation workflow
4. Add ticket status tracking
5. Create `/admin/tickets` and `/admin/escalations` endpoints
6. Add role-based authorization

**Files to create/modify**:
- `src/HrDesk.Admin/Services/AdminService.cs` (MODIFY - implement)
- `src/HrDesk.Core/Interfaces/IAdminService.cs` (MODIFY - define contract)
- `src/HrDesk.Api/Controllers/AdminController.cs` (MODIFY - implement endpoints)
- `src/HrDesk.Core/Models/Ticket.cs` (MODIFY - add status enum)
- `src/HrDesk.Core/Models/Escalation.cs` (MODIFY - add workflow logic)

---

### Phase 2.6: Logging, Security & Observability
**Goal**: Production-ready observability and security

**Tasks**:
1. Integrate Application Insights
2. Implement structured logging with correlation IDs
3. Add role-based authorization middleware
4. Implement input validation
5. Add secrets management
6. Set up HTTPS enforcement
7. Create security headers middleware

**NuGet Packages to add**:
- `Microsoft.ApplicationInsights.AspNetCore`
- `Serilog.Sinks.ApplicationInsights`
- `FluentValidation` (for input validation)

**Files to create/modify**:
- `src/HrDesk.Audit/Middleware/ApplicationInsightsLogger.cs` (NEW)
- `src/HrDesk.Audit/Middleware/RoleBasedAuthorizationMiddleware.cs` (NEW)
- `src/HrDesk.Api/Middleware/ValidationMiddleware.cs` (NEW)
- `src/HrDesk.Api/Program.cs` (MODIFY - add all middleware)
- `appsettings.json` (MODIFY - Application Insights key)

---

### Phase 2.7: Frontend (Optional)
**Goal**: Provide UI for employees and admins

**Tech**: React or Next.js

**Tasks**:
1. Create React/Next.js app
2. Chat interface for employees
3. Admin dashboard for tickets
4. JWT token management
5. Responsive design
6. Accessibility features

---

## Priority Sequence

### **Sprint 1** (Weeks 1-2): Foundation
- Phase 2.1: Database & Persistence

### **Sprint 2** (Weeks 3-4): Core Features
- Phase 2.2: AI Integration
- Phase 2.3: PeopleHum Live Integration

### **Sprint 3** (Weeks 5-6): Automation
- Phase 2.4: Background Jobs & Escalation
- Phase 2.5: Ticket & Escalation Management

### **Sprint 4** (Weeks 7-8): Production Ready
- Phase 2.6: Logging, Security & Observability
- Phase 2.7: Frontend (if prioritized)

---

## Feature Flags (Configuration)

```json
{
  "FeatureFlags": {
    "EnableLiveAI": false,
    "EnablePeopleHumSync": false,
    "EnableEscalations": false,
    "EnableRAG": false,
    "EnableApplicationInsights": false
  }
}
```

---

## Testing Strategy

### Unit Tests
- AI intent classification
- PeopleHum caching logic
- Escalation rules
- Validation logic

### Integration Tests
- API + Database + PeopleHum (with mocks)
- Background job execution
- End-to-end chat flow
- Ticket lifecycle

### Test Approach
- Mock external services (OpenAI, PeopleHum)
- Use in-memory or test database (SQLite)
- Implement test fixtures for common scenarios

---

## Deployment Considerations

1. **Containerization**: Docker for API, background jobs, and database
2. **Scaling**: Horizontal pod autoscaling for API instances
3. **Secrets**: Use environment variables or Azure Key Vault
4. **Feature Flags**: Stored in DB or configuration service
5. **Database Migrations**: Run on startup or in CI/CD pipeline
6. **Monitoring**: Application Insights + custom dashboards
7. **Logging**: Centralized Serilog + Application Insights

---

## Next Steps

1. **Confirm Priority**: Prioritize between database-first vs AI-first approach
2. **Database Choice**: Confirm SQL Server vs PostgreSQL vs other
3. **LLM Provider**: Confirm OpenAI vs Azure OpenAI vs other
4. **Frontend Decision**: Confirm if React/Next.js frontend is needed in Phase 2
5. **Team Assignment**: Assign tasks to team members based on expertise
6. **Timeline**: Finalize Sprint schedule and delivery dates
