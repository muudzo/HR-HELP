# HrDesk Phase 2: Architecture Decision Records (ADRs)

## ADR-001: Database Platform Choice

### Context
HrDesk Phase 2 requires persistent storage for chat histories, tickets, and employee cache.

### Decision
**Use SQL Server with Entity Framework Core ORM**

### Rationale
- SQL Server: Widely used in enterprises, excellent ACID compliance, good .NET integration
- EF Core: Industry standard for .NET, excellent migration support, LINQ-to-SQL
- Alternative considered: PostgreSQL (would work equally well, but SQL Server is more standard in enterprise)
- Not considered: NoSQL (relational data structure is beneficial here)

### Consequences
✅ Benefit: Strong data consistency, ACID transactions  
✅ Benefit: Easy migrations and schema versioning  
⚠️ Trade-off: Requires database server (more infrastructure than serverless)  
⚠️ Trade-off: Vertical scaling limits (but acceptable for Phase 2)

### Implementation Status
- [x] EF Core DbContext designed
- [ ] Migrations created
- [ ] Database initialized

---

## ADR-002: AI Model Selection

### Context
Phase 2 requires LLM integration to replace stub AI for HR query classification.

### Decision
**Use OpenAI GPT-3.5-turbo (Phase 2), with ability to upgrade to GPT-4 or Claude**

### Rationale
- GPT-3.5-turbo: Cost-effective (~$0.002/1K tokens), fast, sufficient for intent classification
- Azure OpenAI: Enterprise-grade security and compliance if needed
- Future upgrade path: Easy to swap models by changing config
- Alternative considered: Open-source models (self-hosted) - rejected for time-to-value
- Alternative considered: Claude - good but more expensive, better for Phase 3

### Consequences
✅ Benefit: Production-ready, well-tested models  
✅ Benefit: Immediate value without ML expertise  
✅ Benefit: Scalable API with built-in monitoring  
💰 Cost: ~$0.02-0.05 per user query (manageable)  
⚠️ Trade-off: Dependent on external API (add circuit breaker)

### Implementation Status
- [ ] LlmOrchestrator created
- [ ] OpenAI SDK integrated
- [ ] Structured prompt templates defined
- [ ] Retry/circuit breaker policies added

---

## ADR-003: Background Jobs Scheduler

### Context
Phase 2 needs automated tasks (employee sync, ticket escalation) running on schedule.

### Decision
**Use Hangfire for background job scheduling**

### Rationale
- Hangfire: .NET-native, persistent storage, built-in dashboard, retry logic
- Alternative considered: Quartz.NET (more complex, overkill for current needs)
- Alternative considered: Azure Functions (good, but less control over scheduling)

### Consequences
✅ Benefit: Simple configuration, excellent dashboard for monitoring  
✅ Benefit: Persistent job storage (survives app restarts)  
✅ Benefit: Native .NET exception handling  
⚠️ Trade-off: Requires additional database tables (Hangfire schema)  
⚠️ Trade-off: Single-instance only in basic setup (need distributed version for scale)

### Implementation Status
- [ ] Hangfire NuGet package added
- [ ] SQL Server storage configured
- [ ] Jobs registered
- [ ] Dashboard exposed at `/hangfire`

---

## ADR-004: PeopleHum Integration Strategy

### Context
HrDesk needs employee data from PeopleHum API with minimal latency.

### Decision
**Implement hybrid approach: Real-time API calls with local cache (1-hour TTL)**

### Rationale
- Real-time API: Always current data for critical queries
- Local cache: Reduce API calls (PeopleHum may have rate limits)
- 1-hour TTL: Balance between freshness and performance
- Background sync: Periodic refresh of employee master data
- Circuit breaker: Handle API failures gracefully

### Consequences
✅ Benefit: Always current employee data without rate limiting issues  
✅ Benefit: Graceful degradation if API is down  
✅ Benefit: Reduced latency after cache warm-up  
⚠️ Trade-off: 1-hour stale data possible (acceptable for HR context)  
⚠️ Trade-off: Adds complexity (cache invalidation)

### Implementation Status
- [ ] Real PeopleHumClient implemented
- [ ] HttpClientFactory configured
- [ ] Polly retry/circuit breaker policies
- [ ] SyncPeopleHumJob implemented
- [ ] Cache strategy tested

---

## ADR-005: Feature Flags & Configuration Management

### Context
Phase 2 introduces multiple new features that must be toggled for gradual rollout.

### Decision
**Configuration-based feature flags (appsettings.json), with future database option**

### Rationale
- Configuration files: Simple for Phase 2, no infrastructure overhead
- Future enhancement: Can move to database/FeatureFlagService for runtime changes
- Alternatives: Feature flag SaaS (LaunchDarkly, Split.io) - overkill for Phase 2

### Flags Required
- `EnableLiveAI`: Route to real LLM vs stub
- `EnablePeopleHumSync`: Run background sync job
- `EnableEscalations`: Auto-escalate old tickets
- `EnableRAG`: Augmented responses from knowledge base
- `EnableApplicationInsights`: Send telemetry

### Consequences
✅ Benefit: Phased rollout without redeployment  
✅ Benefit: Rollback capability (flip config, no code change)  
✅ Benefit: Environment-specific configs (dev/staging/prod)  
⚠️ Trade-off: Requires app restart to apply (can be enhanced later)

### Implementation Status
- [ ] Feature flags defined in appsettings.json
- [ ] IFeatureFlagService created
- [ ] Flags checked in critical paths
- [ ] Documentation updated

---

## ADR-006: Logging & Observability Strategy

### Context
Production Phase 2 needs comprehensive logging, correlation tracking, and error monitoring.

### Decision
**Structured logging with Serilog + Application Insights for production telemetry**

### Rationale
- Serilog: Industry-standard .NET logging, structured logging support
- Application Insights: Azure monitoring, built-in dashboards, exception tracking
- Correlation IDs: Track requests across services (important for debugging)
- Development: Console output sufficient; Application Insights for prod

### Consequences
✅ Benefit: Rich debugging information with structured logs  
✅ Benefit: Real-time production monitoring  
✅ Benefit: Exception tracking and alerting  
💰 Cost: Application Insights (~$2-5/month for typical volume)  
⚠️ Trade-off: Some overhead (minimal with Serilog batching)

### Implementation Status
- [ ] Serilog configured in Program.cs
- [ ] Application Insights SDK added
- [ ] Correlation ID middleware created
- [ ] Custom events tracked

---

## ADR-007: Authentication & Authorization

### Context
APIs must be secure and enforce role-based access.

### Decision
**JWT authentication with role-based authorization (Employee/HRAdmin/SystemAdmin)**

### Rationale
- JWT: Stateless, scalable, industry standard
- Roles: Simple and effective for current requirements
- Future: Can integrate with Azure AD or OIDC if needed
- Alternative: Basic auth (rejected - not secure enough)

### Consequences
✅ Benefit: Stateless, scales horizontally  
✅ Benefit: Follows industry standards  
✅ Benefit: Easy to test with token generation  
⚠️ Trade-off: Token revocation requires additional logic  
⚠️ Trade-off: Requires secure key management

### Implementation Status
- [x] JWT Bearer authentication configured
- [ ] Role-based authorization middleware
- [ ] Role checks in controllers
- [ ] Token generation service

---

## ADR-008: Deployment Strategy

### Context
HrDesk Phase 2 must be deployable to multiple environments (dev/staging/prod).

### Decision
**Docker containerization with docker-compose for development, Kubernetes for production**

### Rationale
- Docker: Consistent environment across machines, easy CI/CD integration
- docker-compose: Simplified development setup (API + database)
- Kubernetes: Production-grade orchestration, auto-scaling, self-healing
- Infrastructure-as-Code: All config in version control

### Consequences
✅ Benefit: Identical dev/prod environments  
✅ Benefit: Easy horizontal scaling  
✅ Benefit: Fast deployment and rollback  
⚠️ Trade-off: Requires Docker/Kubernetes knowledge  
⚠️ Trade-off: Initial setup overhead

### Implementation Status
- [ ] Dockerfile created
- [ ] docker-compose.yml created
- [ ] Kubernetes manifests drafted
- [ ] CI/CD pipeline created

---

## ADR-009: Testing Strategy

### Context
Phase 2 adds complexity with database, external APIs, and background jobs.

### Decision
**Three-level testing pyramid: Unit (70%) → Integration (20%) → E2E (10%)**

### Rationale
- Unit tests: Fast, high coverage, CI friendly (70 tests)
- Integration tests: Real database/APIs, key workflows (20 tests)
- E2E tests: Full user scenarios, production-like (10 tests)
- Mock external services: OpenAI, PeopleHum (avoid API costs)

### Consequences
✅ Benefit: Comprehensive coverage with reasonable test time  
✅ Benefit: Fast feedback for developers  
✅ Benefit: Confidence in deployments  
⚠️ Trade-off: Requires mock framework setup  
⚠️ Trade-off: Integration tests slower (few seconds)

### Implementation Status
- [ ] Unit tests for business logic
- [ ] Mocks for OpenAI, PeopleHum
- [ ] Integration tests with in-memory database
- [ ] E2E tests for critical paths

---

## ADR-010: Secret Management

### Context
API keys (OpenAI, PeopleHum, JWT) must be secure and separate from code.

### Decision
**Environment variables for runtime secrets, User Secrets for development**

### Rationale
- Environment variables: Standard practice, works in containers, CI-friendly
- User Secrets: .NET built-in, protects dev keys from git
- Azure Key Vault: For production (future enhancement)
- Never in appsettings: Prevents accidental commits

### Consequences
✅ Benefit: Secrets not in version control  
✅ Benefit: Environment-specific configs  
✅ Benefit: Works with container orchestration  
⚠️ Trade-off: Requires environment setup in CI/CD  
⚠️ Trade-off: Local development needs user secrets setup

### Implementation Status
- [ ] User Secrets configured locally
- [ ] Environment variables documented
- [ ] CI/CD secrets configured
- [ ] Key rotation process defined

---

## ADR-011: Error Handling & Resilience

### Context
External dependencies (OpenAI, PeopleHum) may fail; APIs must handle gracefully.

### Decision
**Polly circuit breaker + retry policies with timeout protection**

### Rationale
- Polly: Standardized resilience patterns in .NET
- Circuit breaker: Prevent cascading failures
- Retry: Handle transient failures (network blips)
- Timeouts: Prevent resource exhaustion
- Fallback: Degrade gracefully vs. hard errors

### Consequences
✅ Benefit: System resilient to transient failures  
✅ Benefit: Prevents cascading failures  
✅ Benefit: Self-healing for temporary issues  
⚠️ Trade-off: Adds latency (retry delays)  
⚠️ Trade-off: Requires policy tuning

### Implementation Status
- [ ] Polly policies defined for each external service
- [ ] Retry backoff configured (exponential)
- [ ] Circuit breaker thresholds set
- [ ] Fallback responses implemented

---

## ADR-012: Monitoring & Alerting

### Context
Production system needs real-time visibility and alerting for issues.

### Decision
**Application Insights for monitoring + custom dashboards + threshold-based alerts**

### Rationale
- Application Insights: Integrated with .NET, built-in dashboards
- Custom dashboards: Track KPIs specific to HrDesk
- Threshold alerts: Notify on errors/latency/failures
- Future: Slack/Email integration for urgent issues

### Consequences
✅ Benefit: Real-time production visibility  
✅ Benefit: Proactive issue detection  
✅ Benefit: Historical trends for capacity planning  
💰 Cost: Application Insights pricing (typically <$10/month for Phase 2 volume)

### Implementation Status
- [ ] Application Insights instrumentation
- [ ] Custom metrics defined
- [ ] Dashboards created
- [ ] Alert rules configured

---

## Architectural Patterns Applied

### 1. Repository Pattern
**Purpose**: Abstract data access layer  
**Implementation**: Generic `IRepository<T>` interface  
**Benefit**: Easy to mock for testing, switch databases later

### 2. Dependency Injection
**Purpose**: Loose coupling, testability  
**Implementation**: Microsoft.Extensions.DependencyInjection  
**Benefit**: Already built into ASP.NET Core

### 3. Middleware Pipeline
**Purpose**: Cross-cutting concerns (logging, auth, error handling)  
**Implementation**: Custom middleware components in Program.cs  
**Benefit**: Centralized, reusable

### 4. Circuit Breaker Pattern
**Purpose**: Resilience to external service failures  
**Implementation**: Polly policies  
**Benefit**: Prevents cascading failures, enables graceful degradation

### 5. Publish-Subscribe (Event-Driven)
**Purpose**: Decouple components, enable future integrations  
**Implementation**: Domain events, event handlers  
**Benefit**: Easy to add new listeners without modifying core logic

### 6. Feature Flags (Toggle Pattern)
**Purpose**: Gradual rollout, A/B testing  
**Implementation**: Configuration-based flags  
**Benefit**: Zero-downtime feature deployment

---

## Cross-Cutting Concerns Checklist

- [x] Authentication (JWT)
- [x] Authorization (roles)
- [ ] Logging (Serilog)
- [ ] Monitoring (Application Insights)
- [ ] Error Handling (Middleware)
- [ ] Validation (FluentValidation)
- [ ] Caching (Redis - future)
- [ ] Rate Limiting (future)
- [ ] CORS (configured)
- [ ] HTTPS (enforced)

---

## Technical Debt & Future Improvements

### Short-term (Phase 2.2)
- [ ] Move feature flags to database for runtime toggle
- [ ] Add distributed caching (Redis)
- [ ] Implement audit log archival
- [ ] Create admin dashboard for job monitoring

### Medium-term (Phase 3)
- [ ] Knowledge base RAG for enriched responses
- [ ] Multi-language support
- [ ] Advanced escalation workflows
- [ ] Customer analytics & reporting

### Long-term (Phase 4+)
- [ ] Machine learning for intent prediction
- [ ] Chatbot handoff to human support
- [ ] Integration with other HR systems (Workday, BambooHR)
- [ ] Mobile app for employees

---

## Compliance & Security Considerations

### Data Protection
- [ ] Employee data encrypted at rest
- [ ] API keys in environment variables only
- [ ] Chat history retention policy (GDPR)
- [ ] Audit logs immutable for compliance

### Security
- [ ] HTTPS only in production
- [ ] Input validation on all endpoints
- [ ] SQL injection prevention (EF Core parameterized)
- [ ] XSS prevention (JSON API)
- [ ] CSRF protection (if frontend added)
- [ ] Rate limiting on auth endpoints

### Monitoring
- [ ] Security event logging
- [ ] Unauthorized access attempts logged
- [ ] Failed authentication tracked
- [ ] Suspicious patterns detected (future)

---

## References & Resources

### Decision Making
- [ADR Format](https://github.com/joelparkerhenderson/architecture-decision-record)
- [.NET Architecture Patterns](https://docs.microsoft.com/en-us/dotnet/architecture/)

### Libraries & Frameworks
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [Polly Resilience](https://github.com/App-vNext/Polly)
- [Hangfire Background Jobs](https://www.hangfire.io/)
- [Serilog Logging](https://serilog.net/)
- [Azure Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)

### Best Practices
- [Microsoft .NET Core Architecture](https://docs.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/)
- [RESTful API Best Practices](https://restfulapi.net/)
- [Clean Code Principles](https://www.oreilly.com/library/view/clean-code-a/9780136083238/)

---

## Review & Approval

| Role | Name | Date | Status |
|------|------|------|--------|
| Architect | - | 2026-02-05 | Draft |
| Tech Lead | - | - | Pending |
| Engineering Manager | - | - | Pending |

---

## Sign-off

**Document**: Phase 2 Architecture Decision Records  
**Version**: 1.0  
**Date**: February 5, 2026  
**Owner**: HrDesk Technical Team  
**Status**: DRAFT (Ready for team review)

---

**Next Step**: Share this document with the team for feedback and approval before implementation begins.
