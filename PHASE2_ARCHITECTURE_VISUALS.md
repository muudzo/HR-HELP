# HrDesk Phase 2: Architecture Visuals & Diagrams

## 1. Layered Architecture Overview

```
┌──────────────────────────────────────────────────────────────────┐
│                         CLIENT LAYER                             │
│  ┌─────────────────────┬─────────────────────┬─────────────────┐ │
│  │  Web Frontend       │  Mobile App         │  Admin Dashboard │ │
│  │  (React/Next.js)    │  (Optional)         │  (Optional)      │ │
│  └────────────┬────────┴────────────┬────────┴────────────┬─────┘ │
└───────────────┼──────────────────────┼──────────────────────┼───────┘
                │                      │                      │
                └──────────────────────┼──────────────────────┘
                                       │
┌──────────────────────────────────────┼──────────────────────────────┐
│                     SECURITY LAYER                                 │
│  ┌─────────────────────────────────┼────────────────────────────┐  │
│  │         JWT Authentication      │   Middleware Pipeline      │  │
│  │  ├─ Token Validation           │   ├─ Correlation ID        │  │
│  │  ├─ Role-based Authorization   │   ├─ Input Validation      │  │
│  │  └─ Audience Check             │   └─ Error Handling        │  │
│  └─────────────────────────────────┼────────────────────────────┘  │
└──────────────────────────────────────┼───────────────────────────────┘
                                       │
┌──────────────────────────────────────┼───────────────────────────────┐
│                      API LAYER (ASP.NET Core)                      │
│  ┌──────────────────┬────────────────┬────────────────────────────┐ │
│  │ ChatController   │ AdminController│ HealthController          │ │
│  │ ├─ POST /send    │ ├─ GET/POST    │ ├─ GET /health           │ │
│  │ ├─ GET /history  │ │   /tickets   │ └─ GET /ready            │ │
│  │ └─ GET /status   │ └─ GET/POST    │                          │ │
│  │                  │   /escalations │                          │ │
│  └────────┬─────────┴────────┬───────┴────────────┬──────────────┘ │
│           │                  │                    │                │
│  ┌────────▼──────────────────▼────────────────────▼──────────────┐ │
│  │              Core Interfaces & Models                         │ │
│  │  ├─ IAiOrchestrator    ├─ IPeopleHumClient                   │ │
│  │  ├─ IAdminService      ├─ IAuditLogger                       │ │
│  │  ├─ IChatService       └─ Domain Models                      │ │
│  │  └─ IRepository                                              │ │
│  └────────┬──────────────────┬───────────────────┬───────────────┘ │
└───────────┼──────────────────┼───────────────────┼─────────────────┘
            │                  │                   │
┌───────────▼──────────────────▼───────────────────▼─────────────────┐
│                    BUSINESS LOGIC LAYER                            │
│  ┌────────────────┐  ┌─────────────────┐  ┌─────────────────────┐ │
│  │ AI Layer       │  │ PeopleHum Layer │  │ Admin Service Layer │ │
│  │                │  │                 │  │                     │ │
│  │ LlmOrchestrator│  │ PeopleHumClient │  │ AdminService        │ │
│  │ ├─ Intent      │  │ ├─ Cache logic  │  │ ├─ Ticket CRUD      │ │
│  │ │  classification │ ├─ HTTP client  │  │ ├─ Escalation logic │ │
│  │ ├─ Prompt eng │  │ └─ Retry policy │  │ └─ Status tracking  │ │
│  │ └─ Fallback   │  │                 │  │                     │ │
│  └────────────────┘  └─────────────────┘  └─────────────────────┘ │
│                                                                    │
│  ┌──────────────────────────┐      ┌──────────────────────────┐   │
│  │ Audit & Logging Layer    │      │ Background Jobs Layer    │   │
│  │                          │      │                          │   │
│  │ AuditLogger              │      │ SyncPeopleHumJob         │   │
│  │ AuditLoggingMiddleware   │      │ TicketEscalationJob      │   │
│  │ ApplicationInsightsLogger│      │ (Hangfire powered)       │   │
│  └──────────────────────────┘      └──────────────────────────┘   │
└────────────────┬──────────────────────────┬──────────────────────────┘
                 │                          │
┌────────────────▼──────────────────────────▼──────────────────────────┐
│                   INFRASTRUCTURE LAYER                              │
│  ┌──────────────────────┐   ┌────────────────────┐                  │
│  │ Data Access Layer    │   │ External Services  │                  │
│  │                      │   │                    │                  │
│  │ HrDeskDbContext      │   │ ┌────────────────┐ │                  │
│  │ (EF Core)            │   │ │ OpenAI API     │ │                  │
│  │ ├─ DbSet<Chat...>    │   │ │ GPT-3.5-turbo  │ │                  │
│  │ ├─ DbSet<Ticket>     │   │ │ (Intent class) │ │                  │
│  │ ├─ DbSet<Escalation> │   │ └────────────────┘ │                  │
│  │ ├─ DbSet<Employee>   │   │ ┌────────────────┐ │                  │
│  │ └─ DbSet<AuditLog>   │   │ │ PeopleHum API  │ │                  │
│  │                      │   │ │ (Employee data)│ │                  │
│  │                      │   │ └────────────────┘ │                  │
│  │                      │   │ ┌────────────────┐ │                  │
│  │                      │   │ │ Application    │ │                  │
│  │                      │   │ │ Insights       │ │                  │
│  │                      │   │ └────────────────┘ │                  │
│  └──────────────────────┘   └────────────────────┘                  │
└────────────────┬────────────────────────────────────────────────────┘
                 │
┌────────────────▼────────────────────────────────────────────────────┐
│                      DATA PERSISTENCE LAYER                         │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │  SQL Server Database                                           │ │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────┐ │ │
│  │  │ EmployeeCache│  │ChatHistories │  │   Tickets            │ │ │
│  │  │              │  │              │  │                      │ │ │
│  │  │ id           │  │ id           │  │ id                   │ │ │
│  │  │ name         │  │ employee_id  │  │ subject              │ │ │
│  │  │ email        │  │ message      │  │ description          │ │ │
│  │  │ department   │  │ response     │  │ status               │ │ │
│  │  │ cached_at    │  │ intent       │  │ priority             │ │ │
│  │  │              │  │ correlation_ │  │ assigned_to          │ │ │
│  │  │              │  │ id           │  │ created_at           │ │ │
│  │  │              │  │ created_at   │  │ updated_at           │ │ │
│  │  │              │  │ responded_at │  │ resolved_at          │ │ │
│  │  └──────────────┘  └──────────────┘  └──────────────────────┘ │ │
│  │  ┌──────────────┐  ┌──────────────────────────────────────────┐ │ │
│  │  │ Escalations  │  │ AuditLogs                                │ │ │
│  │  │              │  │                                          │ │ │
│  │  │ id           │  │ id                                       │ │ │
│  │  │ ticket_id    │  │ user_id                                  │ │ │
│  │  │ reason       │  │ action                                   │ │ │
│  │  │ escalated_by │  │ resource                                 │ │ │
│  │  │ escalated_to │  │ details                                  │ │ │
│  │  │ status       │  │ correlation_id                           │ │ │
│  │  │ escalated_at │  │ created_at                               │ │ │
│  │  │              │  │                                          │ │ │
│  │  └──────────────┘  └──────────────────────────────────────────┘ │ │
│  │                                                                  │ │
│  │  Indexes: employee_id, status, correlation_id, created_at      │ │
│  │  Relationships: 1:N from EmployeeCache → ChatHistories         │ │
│  │                 1:N from Tickets → Escalations                 │ │
│  └────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 2. Request Flow Diagram

```
┌─────────────┐
│   Employee  │
│  Browser    │
└──────┬──────┘
       │ 1. POST /api/chat/send
       │    with JWT token
       │
       ▼
┌─────────────────────────────────────┐
│  Nginx / API Gateway (HTTPS)        │
│  ├─ SSL Termination                 │
│  └─ Route to ASP.NET Core           │
└──────┬──────────────────────────────┘
       │
       ▼
┌─────────────────────────────────────┐
│  Authentication Middleware          │
│  ├─ Validate JWT token              │
│  ├─ Extract claims                  │
│  ├─ Check expiration                │
│  └─ Return 401 if invalid           │
└──────┬──────────────────────────────┘
       │
       ▼ (2. Request)
┌─────────────────────────────────────┐
│  ChatController.SendMessage()       │
│  ├─ Deserialize ChatRequest         │
│  ├─ Extract UserContext             │
│  └─ Call IAiOrchestrator            │
└──────┬──────────────────────────────┘
       │
       ├──────────────────────────────────────┐
       │                                      │
       ▼ (3a)                                 ▼ (3b)
┌──────────────────┐          ┌──────────────────────┐
│ LlmOrchestrator  │          │ PeopleHumClient      │
│                  │          │                      │
│ 1. Check feature │          │ 1. Check cache       │
│    flag          │          │ 2. If cache miss:    │
│ 2. Build prompt  │◄─────────┤    Fetch from API    │
│ 3. Call OpenAI   │          │ 3. Store in DB       │
│ 4. Parse JSON    │          │ 4. Return employee   │
│ 5. Return        │          │                      │
│    AiResponse    │          └──────────────────────┘
└────────┬─────────┘                  △
         │                            │
         └────────────────────────────┘
                                       
         (3c) Call OpenAI API
         ┌────────────────────────┐
         │   OpenAI (External)    │
         │                        │
         │ POST /chat/completions │
         │                        │
         │ {                      │
         │   model: gpt-3.5-turbo │
         │   messages: [...]      │
         │   temperature: 0.7     │
         │   max_tokens: 500      │
         │ }                      │
         └────────────────────────┘
         
         Response:
         {
           "intent": "Leave",
           "confidence": 0.95,
           "response": "...",
           "needs_escalation": false
         }

       ▼ (4. Response returned)
┌─────────────────────────────────────┐
│  IChatService.SaveChatAsync()       │
│  ├─ Create ChatHistory entity       │
│  ├─ Save to DbContext               │
│  ├─ Commit to SQL Server            │
│  └─ Return persisted entity         │
└──────┬──────────────────────────────┘
       │
       ▼ (5. Log audit event)
┌─────────────────────────────────────┐
│  IAuditLogger.LogAsync()            │
│  ├─ Create audit log entry          │
│  ├─ Set correlation ID              │
│  ├─ Save to DB                      │
│  └─ Send to Application Insights    │
└──────┬──────────────────────────────┘
       │
       ▼ (6. Prepare response)
┌─────────────────────────────────────┐
│  ChatController.SendMessage()       │
│  ├─ Map ChatHistory to DTO          │
│  ├─ Add correlation ID              │
│  ├─ Set status 200 OK               │
│  └─ Return JSON response            │
└──────┬──────────────────────────────┘
       │
       ▼ (7. HTTP 200)
┌─────────────────────────────────────┐
│  Employee Browser                   │
│                                     │
│  Response:                          │
│  {                                  │
│    "id": "chat123",                 │
│    "response": "Your leave...",     │
│    "intent": "Leave",               │
│    "confidence": 0.95,              │
│    "escalated": false,              │
│    "correlationId": "abc-123",      │
│    "respondedAt": "2026-02-05..."   │
│  }                                  │
└─────────────────────────────────────┘
```

---

## 3. Database Entity Relationship Diagram

```
                    ┌──────────────────┐
                    │  EmployeeCache   │
                    ├──────────────────┤
                    │ id (PK) [STRING] │
                    │ name             │
                    │ email            │
                    │ department       │
                    │ cached_at        │
                    └────────┬─────────┘
                             │
                      1 : N  │
                             │
                    ┌────────▼──────────┐
                    │  ChatHistory      │
                    ├───────────────────┤
                    │ id (PK) [GUID]    │
                    │ employee_id (FK)  │◄──────┐
                    │ ticket_id (FK)    │       │ 1:N
                    │ message           │       │
                    │ response          │       │
                    │ intent            │       │
                    │ confidence        │       │
                    │ needs_escalation  │       │
                    │ correlation_id    │       │
                    │ created_at        │       │
                    │ responded_at      │       │
                    └────────┬──────────┘       │
                             │                  │
                             │ N:1              │
                             │                  │
                    ┌────────▼──────────────────┘
                    │  Ticket                 │
                    ├─────────────────────────┤
                    │ id (PK) [GUID]          │
                    │ subject                 │
                    │ description             │
                    │ status                  │ (Open, InProgress, Resolved, Escalated, Closed)
                    │ priority                │ (Low, Medium, High, Critical)
                    │ assigned_to             │
                    │ created_at              │
                    │ updated_at              │
                    │ resolved_at             │
                    └────────┬────────────────┘
                             │
                      1 : N  │
                             │
                    ┌────────▼──────────┐
                    │  Escalation       │
                    ├───────────────────┤
                    │ id (PK) [GUID]    │
                    │ ticket_id (FK)    │
                    │ reason            │
                    │ escalated_by      │
                    │ escalated_to      │
                    │ status            │
                    │ escalated_at      │
                    └───────────────────┘


┌──────────────────────────────────────────────────────────┐
│  AuditLog (Separate - All Events)                       │
├──────────────────────────────────────────────────────────┤
│ id (PK) [GUID]                                           │
│ user_id                                                  │
│ action (CHAT_MESSAGE_SENT, TICKET_CREATED, etc.)        │
│ resource (ChatMessage, Ticket, etc.)                     │
│ details (JSON)                                           │
│ correlation_id                                           │
│ created_at                                               │
│                                                          │
│ Indexes:                                                 │
│   - correlation_id (for request tracing)                 │
│   - user_id (for user activity)                          │
│   - action (for filtering by operation type)             │
│   - created_at (for time-range queries)                  │
└──────────────────────────────────────────────────────────┘

Key Indexes:
├─ EmployeeCache.id (PRIMARY)
├─ ChatHistory.employee_id (FOREIGN)
├─ ChatHistory.correlation_id (PERFORMANCE)
├─ ChatHistory.created_at (PERFORMANCE)
├─ Ticket.status (PERFORMANCE)
├─ Ticket.assigned_to (PERFORMANCE)
├─ Escalation.ticket_id (FOREIGN)
└─ AuditLog.correlation_id (PERFORMANCE)
```

---

## 4. Background Job Execution Flow

```
┌─────────────────────────────────────────────────────────┐
│         Hangfire Background Job Scheduler               │
│         (Running in application process)                │
└────────────────┬────────────────────────────────────────┘
                 │
    ┌────────────┴────────────┬────────────────┐
    │                         │                │
    ▼                         ▼                ▼
[HOURLY]              [EVERY 30 MINUTES]  [ON DEMAND]
    │                         │                │
    │                         │                │
    ▼                         ▼                ▼
SyncPeopleHumJob      TicketEscalationJob  ManualJob
    │                         │
    ├─ Get all employees      ├─ Find tickets open > 24h
    │  from PeopleHum         ├─ Mark as "Escalated"
    ├─ For each employee:     ├─ Create escalation record
    │  ├─ Check if exists     ├─ Send notification
    │  │  in cache            ├─ Update audit log
    │  ├─ If exists:          └─ Return job result
    │  │  Update row
    │  └─ If not:
    │     Insert row
    ├─ Update cached_at
    ├─ Commit to DB
    ├─ Log job success
    └─ Return result

┌─────────────────────────────────────────────────────────┐
│         Hangfire Storage (SQL Server)                   │
│                                                         │
│ HangfireServer    - Server health                       │
│ HangfireCounter   - Job counters                        │
│ HangfireHash      - Job parameters                      │
│ HangfireJob       - Job records                         │
│ HangfireList      - Job queues                          │
│ HangfireSet       - Job sets                            │
│ HangfireState     - Job state history                   │
│ HangfireConnection- Connection strings                  │
│ HangfireDistLock  - Distributed locks                   │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│    Monitoring Dashboard: /hangfire                      │
│                                                         │
│  ┌───────────────────────────────────────────────────┐  │
│  │ Overview                    Details               │  │
│  │ ├─ Servers: 1 online        ├─ Job queues        │  │
│  │ ├─ Queues: 1 (default)      ├─ Failed jobs       │  │
│  │ ├─ Scheduled: 2             ├─ Recurring jobs    │  │
│  │ ├─ Succeeded: 234           └─ Job history       │  │
│  │ ├─ Failed: 0                                      │  │
│  │ └─ Recurring: 2                                   │  │
│  │                                                   │  │
│  │  [SyncPeopleHum] - Last run: 15m ago              │  │
│  │  ├─ Status: Success ✓                            │  │
│  │  └─ Next run: in 45 minutes                       │  │
│  │                                                   │  │
│  │  [TicketEscalation] - Last run: 5m ago            │  │
│  │  ├─ Status: Success ✓                            │  │
│  │  └─ Next run: in 25 minutes                       │  │
│  └───────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘

Error Handling:
┌─────────────────────────────────────────────────────────┐
│ Retry Policy (Polly)                                    │
│                                                         │
│ On failure:                                             │
│ 1. Wait 5 seconds, retry (attempt 1)                    │
│ 2. Wait 10 seconds, retry (attempt 2)                   │
│ 3. Wait 20 seconds, retry (attempt 3)                   │
│ 4. Move to failed jobs queue                            │
│ 5. Log to Application Insights                          │
│ 6. Send alert notification                              │
└─────────────────────────────────────────────────────────┘
```

---

## 5. Authentication & Authorization Flow

```
┌──────────────────────────────────┐
│  Client (Employee/Admin)         │
│                                  │
│  1. Credentials or API Key       │
│     (handled by auth system)     │
└───────────────┬──────────────────┘
                │
                ▼
    ┌───────────────────────┐
    │   Auth Service        │
    │   (generates JWT)     │
    │                       │
    │  Payload:             │
    │  {                    │
    │    sub: employee_id   │
    │    role: "Employee"   │
    │    email: "user@co"   │
    │    iat: 1234567890    │
    │    exp: 1234571490    │
    │    aud: "HrDesk-Users"│
    │    iss: "HrDesk"      │
    │  }                    │
    └─────────┬─────────────┘
              │
              ▼
    ┌─────────────────────────┐
    │ Return JWT to Client    │
    │                         │
    │ Authorization: Bearer   │
    │ eyJhbGciOiJIUzI1NiIs...│
    └─────────┬───────────────┘
              │
              ▼
    ┌──────────────────────────────────────┐
    │  All API Requests Include JWT Token  │
    │                                      │
    │  GET /api/chat/history               │
    │  Headers:                            │
    │    Authorization: Bearer eyJhbG...   │
    │    Content-Type: application/json    │
    └─────────┬──────────────────────────┘
              │
              ▼
┌─────────────────────────────────────────┐
│  Authentication Middleware              │
│                                         │
│  ├─ Extract token from header           │
│  ├─ Validate signature (JWT secret key) │
│  ├─ Check expiration (exp claim)        │
│  ├─ Validate issuer (iss claim)         │
│  ├─ Validate audience (aud claim)       │
│  │                                      │
│  ├─ If valid: Add ClaimsPrincipal       │
│  │           to HttpContext.User        │
│  │                                      │
│  └─ If invalid: Return 401 Unauthorized │
└────────────┬──────────────────────────┘
             │
             ├─ Valid? ─→ Continue
             │
             └─ Invalid? ─→ HTTP 401
                           {
                             "message": "Unauthorized"
                           }

    ┌─────────────────────────────────────┐
    │  Authorization Middleware           │
    │                                     │
    │  Check [Authorize(Roles="...")]     │
    │                                     │
    │  GET /api/chat/history              │
    │  [Authorize(Roles="Employee")]      │
    │  ├─ If role="Employee": ✓ Allow    │
    │  └─ If role="Admin": ✗ Forbidden   │
    │                                     │
    │  GET /admin/tickets                 │
    │  [Authorize(Roles="HRAdmin")]       │
    │  ├─ If role="HRAdmin": ✓ Allow     │
    │  ├─ If role="Employee": HTTP 403   │
    │  └─ If role="SystemAdmin": ✓ Allow│
    └────────────┬──────────────────────┘
                 │
                 ├─ Authorized? ─→ Continue
                 │
                 └─ Denied? ─→ HTTP 403
                              {
                                "message": "Forbidden"
                              }

    ┌──────────────────────────────────────┐
    │  Controller Action Execution         │
    │                                      │
    │  User.FindFirst("sub") returns ID    │
    │  User.FindFirst(ClaimTypes.Role)     │
    │  User.FindFirst("email")             │
    │                                      │
    │  → Process request                   │
    │  → Return response 200 OK            │
    └──────────────────────────────────────┘

Roles Matrix:
┌───────────────┬──────────┬──────────┬────────────┐
│ Endpoint      │ Employee │ HRAdmin  │ SysAdmin   │
├───────────────┼──────────┼──────────┼────────────┤
│ POST /chat    │ ✓        │ ✓        │ ✓          │
│ GET /history  │ ✓ (own)  │ ✓ (all)  │ ✓          │
│ GET /tickets  │ ✗        │ ✓        │ ✓          │
│ POST /tickets │ ✗        │ ✓        │ ✓          │
│ GET /hangfire │ ✗        │ ✗        │ ✓          │
└───────────────┴──────────┴──────────┴────────────┘
```

---

## 6. Caching Strategy

```
┌──────────────────────────────────┐
│ Request: Get Employee Info       │
│ employee_id = "EMP123"           │
└────────────┬─────────────────────┘
             │
             ▼
    ┌────────────────────┐
    │ Check L1 Cache     │ (In-memory, 5 min TTL)
    │ (Application Mem)  │
    │                    │
    │ HIT ──→ Return     │
    │ MISS ──→ Continue  │
    └────────┬───────────┘
             │
             ▼
    ┌──────────────────────────┐
    │ Check L2 Cache           │ (Database, 1 hour TTL)
    │ SELECT * FROM            │
    │   EmployeeCaches         │
    │ WHERE id = 'EMP123'      │
    │   AND cached_at >        │
    │   NOW() - 1 HOUR         │
    │                          │
    │ HIT ──→ Return + L1 Set  │
    │ MISS ──→ Continue        │
    └────────┬─────────────────┘
             │
             ▼
    ┌────────────────────────────────┐
    │ Call External API              │
    │ GET /api/v1/employees/EMP123   │
    │                                │
    │ Response:                      │
    │ {                              │
    │   "id": "EMP123",              │
    │   "name": "John Doe",          │
    │   "email": "john@co.com",      │
    │   "department": "HR"           │
    │ }                              │
    └────────┬───────────────────────┘
             │
             ▼
    ┌────────────────────────────────────┐
    │ Update/Insert Cache Entry          │
    │ INSERT INTO EmployeeCaches OR      │
    │ UPDATE EmployeeCaches              │
    │ SET name='John', cached_at=NOW()   │
    │ WHERE id='EMP123'                  │
    │                                    │
    │ Commit to database                 │
    └────────┬────────────────────────────┘
             │
             ▼
    ┌──────────────────────┐
    │ Set L1 Cache         │
    │ (Application Memory) │
    │ TTL: 5 minutes       │
    └────────┬─────────────┘
             │
             ▼
    ┌────────────────────────────────┐
    │ Return to Caller               │
    │ {                              │
    │   "id": "EMP123",              │
    │   "name": "John Doe",          │
    │   "email": "john@co.com",      │
    │   "department": "HR",          │
    │   "source": "api"              │
    │ }                              │
    └────────────────────────────────┘

Cache Invalidation Strategy:
┌──────────────────────────────────────────┐
│ Event                  Action             │
├──────────────────────────────────────────┤
│ 1. Employee Updated    Clear L1 + L2     │
│    via PeopleHum       Refresh from API  │
│                                          │
│ 2. L2 Cache Expires    Automatic (1hr)   │
│    (1 hour TTL)        Refresh on access │
│                                          │
│ 3. L1 Cache Expires    Automatic (5min)  │
│    (5 min TTL)         Fall back to L2   │
│                                          │
│ 4. SyncJob Runs        Refresh all cache │
│    (hourly)            Update DB entries │
│                                          │
│ 5. Manual Refresh      Clear cache       │
│    (admin action)      Fetch fresh data  │
└──────────────────────────────────────────┘

Performance Impact:
┌────────────────────────────────────────────┐
│ Scenario                    Latency         │
├────────────────────────────────────────────┤
│ L1 Hit (memory)             <5ms           │
│ L2 Hit (database)           <50ms          │
│ Cache Miss (API call)       <2000ms (2s)   │
│                                            │
│ With 90% L1 Hit Rate:                      │
│   Avg Latency ≈ 0.9*5ms + 0.1*2000ms      │
│                ≈ 204ms                     │
│                                            │
│ Without Cache (100% API):                  │
│   Avg Latency ≈ 2000ms (2s)                │
│                                            │
│ Improvement: 10x faster ✓                  │
└────────────────────────────────────────────┘
```

---

## 7. Observability & Monitoring

```
┌────────────────────────────────────────────────────────┐
│              Application Insights                      │
│                                                        │
│  ┌──────────────────────────────────────────────────┐  │
│  │ Request Tracking                                 │  │
│  │                                                  │  │
│  │ Request ID: abc-123-def-456                      │  │
│  │ ├─ Timestamp: 2026-02-05T10:30:00Z              │  │
│  │ ├─ Method: POST /api/chat/send                   │  │
│  │ ├─ User: employee_id                            │  │
│  │ ├─ Status: 200                                  │  │
│  │ ├─ Duration: 245ms                              │  │
│  │ └─ Dependencies:                                │  │
│  │    ├─ Database: 45ms                            │  │
│  │    ├─ OpenAI API: 180ms                         │  │
│  │    └─ PeopleHum Cache: <1ms (hit)              │  │
│  └──────────────────────────────────────────────────┘  │
│                                                        │
│  ┌──────────────────────────────────────────────────┐  │
│  │ Performance Metrics                              │  │
│  │                                                  │  │
│  │ API Response Time (p95): 450ms                   │  │
│  │ Database Query (p95): 85ms                       │  │
│  │ Error Rate: 0.05%                                │  │
│  │ Availability: 99.8%                              │  │
│  │ Throughput: 150 requests/sec                     │  │
│  │ Custom Events: 2,345 today                       │  │
│  └──────────────────────────────────────────────────┘  │
│                                                        │
│  ┌──────────────────────────────────────────────────┐  │
│  │ Exceptions & Errors                              │  │
│  │                                                  │  │
│  │ OpenAI API Timeout (5 occurrences)               │  │
│  │ ├─ Stack Trace: [...]                           │  │
│  │ ├─ First Occurred: 2h ago                        │  │
│  │ └─ Last Occurred: 5m ago                         │  │
│  │                                                  │  │
│  │ Database Connection Pool Exhausted (1)           │  │
│  │ └─ Investigation: Scale up pool size             │  │
│  └──────────────────────────────────────────────────┘  │
│                                                        │
│  ┌──────────────────────────────────────────────────┐  │
│  │ Custom Events                                    │  │
│  │                                                  │  │
│  │ CHAT_MESSAGE_SENT: 1,234 events                 │  │
│  │ TICKET_CREATED: 45 events                        │  │
│  │ AI_QUERY_ESCALATED: 12 events                    │  │
│  │ JOB_SUCCESS (Sync): 24 events                    │  │
│  │ JOB_SUCCESS (Escalation): 48 events              │  │
│  └──────────────────────────────────────────────────┘  │
│                                                        │
│  ┌──────────────────────────────────────────────────┐  │
│  │ Alerts & Dashboards                              │  │
│  │                                                  │  │
│  │ ALERT: Error Rate > 1% [TRIGGERED]               │  │
│  │   └─ Sent: Slack, Email, PagerDuty              │  │
│  │                                                  │  │
│  │ ALERT: API Latency p95 > 1000ms [OK]             │  │
│  │   └─ Status: Normal (p95 = 450ms)                │  │
│  │                                                  │  │
│  │ Custom Dashboard: "HrDesk Health"                │  │
│  │ ├─ Live Request Rate                            │  │
│  │ ├─ Response Time Distribution                    │  │
│  │ ├─ Error Timeline                                │  │
│  │ ├─ Top Failed Operations                         │  │
│  │ └─ Job Success Rate                              │  │
│  └──────────────────────────────────────────────────┘  │
└────────────────────────────────────────────────────────┘

Logging Flow:
┌──────────────────────────────┐
│  Application Code            │
│                              │
│  Log.Information(             │
│    "Chat message sent: " +    │
│    "{MessageId}, Intent: {Intent}", │
│    messageId, intent)         │
└────────────┬─────────────────┘
             │
             ▼
┌──────────────────────────────────────┐
│  Serilog Structured Logger           │
│                                      │
│  {                                   │
│    "MessageId": "msg123",            │
│    "Intent": "Leave",                │
│    "Timestamp": "2026-02-05T10:30",  │
│    "Level": "Information",           │
│    "CorrelationId": "abc-123"        │
│  }                                   │
└────────┬───────────────────────────┘
         │
         ├──→ Console Output (dev)
         │
         ├──→ File (optional)
         │
         └──→ Application Insights
              (Application Insights Sink)
              
              │
              ▼
         ┌──────────────────────────┐
         │ Application Insights     │
         │ (Analytics & Dashboard)  │
         └──────────────────────────┘
```

---

## 8. Deployment Pipeline

```
┌─────────────────────────────────────────────────────────┐
│  Developer Commit                                       │
│  git push origin feature/phase2-database                │
└────────────┬────────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────────┐
│  GitHub Actions CI/CD Pipeline                          │
│                                                         │
│  1. Trigger: Push to main/develop                       │
│     └─ Run on: ubuntu-latest                           │
└────────────┬────────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────────┐
│  Build Stage                                            │
│                                                         │
│  ├─ Checkout code                                       │
│  ├─ Setup .NET 8.0                                      │
│  ├─ Restore NuGet packages                              │
│  ├─ Build solution                                      │
│  ├─ Run unit tests                                      │
│  │  └─ Coverage: >80%                                   │
│  ├─ SONAR code analysis                                 │
│  └─ Generate code coverage report                       │
└────────────┬────────────────────────────────────────────┘
             │
             ├─ Failed? → Notify developer ✗
             │
             ▼ Success ✓
┌─────────────────────────────────────────────────────────┐
│  Security & Quality Checks                              │
│                                                         │
│  ├─ Dependency scanning (Snyk)                          │
│  ├─ SAST (static analysis)                              │
│  ├─ Secrets detection                                   │
│  └─ Container image scan                                │
└────────────┬────────────────────────────────────────────┘
             │
             ├─ Issues found? → Fail build ✗
             │
             ▼ Passed ✓
┌─────────────────────────────────────────────────────────┐
│  Docker Build & Push                                    │
│                                                         │
│  ├─ Build Docker image                                  │
│  │  Dockerfile:                                         │
│  │  ├─ FROM mcr.microsoft.com/dotnet/aspnet:8.0        │
│  │  ├─ WORKDIR /app                                     │
│  │  ├─ COPY bin/Release /app                            │
│  │  └─ ENTRYPOINT ["dotnet", "HrDesk.Api.dll"]          │
│  ├─ Tag: hrdesk:v1.0.0-20260205-1                       │
│  ├─ Push to container registry                          │
│  │  (Docker Hub / Azure Container Registry)             │
│  └─ Generate SBOM (Software Bill of Materials)          │
└────────────┬────────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────────┐
│  Deploy to Staging                                      │
│                                                         │
│  ├─ Pull image: hrdesk:v1.0.0-20260205-1               │
│  ├─ Kubernetes deployment:                              │
│  │  ├─ Create pods (replicas: 2)                        │
│  │  ├─ Run migrations (init container)                  │
│  │  ├─ Health check: /health                            │
│  │  └─ Readiness probe: /ready                          │
│  ├─ DNS: staging.hrdesk.local                           │
│  ├─ TLS: self-signed certificate                        │
│  └─ Smoke tests:                                        │
│     ├─ GET /health → 200                                │
│     ├─ POST /api/chat/send → 200                        │
│     └─ GET /admin/tickets → 403 (no auth)              │
└────────────┬────────────────────────────────────────────┘
             │
             ├─ Failed? → Rollback & notify ✗
             │
             ▼ Passed ✓
┌─────────────────────────────────────────────────────────┐
│  Integration & Performance Tests (Staging)              │
│                                                         │
│  ├─ Database tests                                      │
│  │  └─ Save/retrieve chat, verify DB integrity          │
│  ├─ API integration tests                               │
│  │  └─ Full request flow with real deps                 │
│  ├─ Load test                                           │
│  │  └─ 100 concurrent users, 10,000 requests            │
│  ├─ Performance baseline                                │
│  │  └─ Response time p95 < 500ms                        │
│  └─ Regression tests                                    │
│     └─ All previous features working                    │
└────────────┬────────────────────────────────────────────┘
             │
             ├─ Failed? → Reject & notify ✗
             │
             ▼ Passed ✓
┌─────────────────────────────────────────────────────────┐
│  Approval Gate (Manual or Automatic)                    │
│                                                         │
│  ├─ Tech lead review: Approve/Reject                    │
│  ├─ Product owner: Confirm features                     │
│  └─ Security team: Review compliance                    │
└────────────┬────────────────────────────────────────────┘
             │
             ├─ Rejected? → Notify & stop ✗
             │
             ▼ Approved ✓
┌─────────────────────────────────────────────────────────┐
│  Deploy to Production                                   │
│                                                         │
│  ├─ Backup database                                     │
│  ├─ Kubernetes rolling update:                          │
│  │  ├─ Max surge: 25%                                   │
│  │  ├─ Max unavailable: 0%                              │
│  │  └─ Termination grace period: 30s                    │
│  ├─ Run database migrations                             │
│  ├─ Health checks pass                                  │
│  ├─ Traffic ramp: 10% → 50% → 100%                      │
│  ├─ Monitor error rate                                  │
│  └─ If error rate > 1%: Automatic rollback              │
└────────────┬────────────────────────────────────────────┘
             │
             ├─ Deployment failed? → Rollback ✗
             │
             ▼ Success ✓
┌─────────────────────────────────────────────────────────┐
│  Post-Deployment                                        │
│                                                         │
│  ├─ Smoke tests (prod)                                  │
│  ├─ Update feature flags                                │
│  ├─ Notify stakeholders                                 │
│  ├─ Create deployment record                            │
│  ├─ Monitor metrics for 1 hour                          │
│  └─ Keep rollback plan ready                            │
└─────────────────────────────────────────────────────────┘

Success Signals:
✓ Build completed: 3m 45s
✓ All tests passed: 1,234/1,234
✓ Security checks passed: 0 issues
✓ Docker image pushed: v1.0.0-20260205-1
✓ Staging deployment: OK
✓ Integration tests: PASSED
✓ Load test: 100 req/s sustained
✓ Production deployment: COMPLETE
✓ Monitoring: All green
```

---

## Summary

These diagrams provide a visual reference for understanding HrDesk Phase 2's architecture at different levels:

1. **Layered Architecture** - Overall system structure and components
2. **Request Flow** - How a chat request moves through the system
3. **Database Design** - Entity relationships and schema
4. **Background Jobs** - Hangfire scheduler and job execution
5. **Authentication** - JWT and role-based authorization
6. **Caching** - Multi-level cache strategy for performance
7. **Observability** - Monitoring, logging, and alerting
8. **Deployment** - CI/CD pipeline from code to production

Use these diagrams for:
- ✅ Team onboarding and knowledge transfer
- ✅ Architecture reviews with stakeholders
- ✅ Documentation in wikis or confluence
- ✅ Design discussions and decisions
- ✅ Testing and QA planning
- ✅ Operations and incident response
