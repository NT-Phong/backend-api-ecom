# Backend API Architecture Analysis Template

## Input
Task Type:
`<New Feature Design / Bug Debugging / Refactoring / Architecture Review>`

Short Description:
`<goal to achieve or observed failure>`

Boundary/Module:
`<controller, handler, route, entity, worker, hub, integration>`

Additional Details:
`<logs, snippets, business requirements, assumptions>`

Expected Output:
`<deliverables satisfying the request>`

## Analysis Steps

### 1. Locate API Boundary
- Route:
- Controller/handler:
- Hub/middleware/filter:
- DTO/schema:
- Command/query:
- Tests or verification target:

### 2. Trace Dependency Flow

```text
API boundary
-> application/service
-> domain logic
-> repository/infrastructure
-> database/cache/external service
```

### 3. Check Architecture Boundaries
- Is business logic outside the controller/hub?
- Is domain logic free from infrastructure dependencies?
- Are repositories hidden behind interfaces where the project expects that?
- Are dependencies flowing inward?
- Is transaction behavior aligned with `[EnableUnitOfWork]`?
- Is the code matching this repo's pragmatic Application patterns rather than forcing generic Clean Architecture purity?

### 4. Check API Contract
- Request validation:
- Response shape:
- Error format:
- Backward compatibility risk:

### 5. Check Security
- Authentication:
- Authorization:
- Input validation:
- Sensitive data exposure:
- Logging safety:

### 6. Check Test Coverage
- Existing tests:
- Missing tests:
- Recommended focused test or build:
- If no automated test exists, recommended build:

### 7. Recommendation
- Root issue:
- Smallest safe change:
- Files to modify:
- Verification commands:
- Risks:
- Existing project debt touched:
- Skill report to update, if any:

## Execution Phases

```text
Plan -> Implement Plan -> Report -> Update skill files
```

Keep each phase delta-first. Use source evidence for code behavior and update the relevant skill report when a task confirms reusable domain knowledge.

## Template A - Delta Review For Changed Files

```text
Role: Ecom Backend Codeflow Analyst.
Task: Run delta-only backend review.
Scope: changed files only.
Rules:
1. Do not repeat unchanged conclusions.
2. Reuse issue IDs if they already exist.
3. Report only new findings, changed risk, or invalidated assumptions.
4. Include file:line evidence for every important finding.
Output:
Findings -> Impact -> Evidence -> Adjustment Plan -> Score -> Next actions.
If no changes in risk: "No new findings; previous conclusions remain valid."
```

## Template B - Feature Flow Review

```text
Role: Ecom Backend Codeflow Analyst.
Task: Review feature flow and architecture compliance.
Scope: Controller -> Command/Query -> Handler -> Repository/UnitOfWork -> Infrastructure.
Checks:
- Thin controller rule
- CQRS separation
- Validator completeness
- Transaction boundary with EnableUnitOfWork
- Idempotency/concurrency risks
- Infrastructure boundary
- Logging safety
Output:
Findings -> Impact -> Evidence -> Adjustment Plan -> Score -> Next actions.
```

## Template C - Bug Hunt On One Call Path

```text
Role: Ecom Backend Codeflow Analyst.
Task: Deep analyze one call path: {ENTRYPOINT}.
Rules:
1. Trace runtime path only.
2. Identify root cause and blast radius.
3. Provide the smallest safe fix first.
4. Include trade-offs only when there are multiple viable fixes.
Output:
Root cause -> Evidence -> Fix plan -> Verification -> Risks.
```

## Template D - Release Gate Check

```text
Role: Ecom Backend Codeflow Analyst.
Task: Perform GO/NO-GO backend release gate check.
Checklist:
- Open High issues
- Concurrency/idempotency safety
- Transaction integrity
- Security/config handling
- Test coverage for critical paths
Rule:
Return GO or NO-GO with exact blockers and file:line evidence.
Output:
Findings -> Impact -> Evidence -> Adjustment Plan -> Score -> Next actions.
```

## Template E - Implementation Plan From Approved Findings

```text
Role: Ecom Backend Codeflow Analyst.
Task: Build implementation plan only from approved findings.
Input:
- Source reports: {FILES}
- Approved issue IDs: {LIST}
Rules:
1. No broad re-analysis.
2. Convert issue IDs into tasks by layer:
   - Application
   - Infrastructure
   - Presentation
   - Tests
3. Include done criteria and required verification.
Output:
Adjustment Plan -> Next actions.
```

## Quick One-Liner

```text
Use role Ecom Backend Codeflow Analyst, run delta-first review for changed files only, avoid repeating unchanged conclusions, include file:line evidence, and output Findings -> Impact -> Evidence -> Adjustment Plan -> Score -> Next actions.
```

