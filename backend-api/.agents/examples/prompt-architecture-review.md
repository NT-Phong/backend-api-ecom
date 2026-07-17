# Prompt Example - Architecture Review

```markdown
Task Type: Architecture Review
Short Description: Review current Oxy alert flow for architecture compliance and unresolved operational risks.
Boundary/Module: Code 300 telemetry, PondAlert handlers, OxyAlertQueueService, OxyAlertSchedulerService, warning hardware services.
Additional Details:
- Read .agents/context/task-router.md first.
- Read .agents/skills/pond_error/report-bug.md and flow-worker.md before source.
- Run delta-first review: do not repeat old conclusions unless source changed or assumptions are invalidated.
- Include file:line evidence for important findings.
- Do not implement code.
Expected Output:
- Findings -> Impact -> Evidence -> Adjustment Plan -> Score -> Next actions.
- Identify any report assumptions that source invalidates.
- Update no files unless explicitly approved after review.
```
