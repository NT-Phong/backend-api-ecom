# Prompt Example - New Feature Design

```markdown
Task Type: New Feature Design
Short Description: Add a backend design for exposing the latest live scale snapshot by cycle and session.
Boundary/Module: Scale live-weight API, ScaleRecord session context, ScaleCacheService, TelemetryHub groups.
Additional Details:
- Read .agents/context/task-router.md first.
- Read .agents/skills/scale_optimise/SKILL.md, scale-review.md, IMPLEMENTED_PLANS.md, and PLAN.md.
- Preserve existing Redis keys and SignalR group names unless explicitly approved.
- Do not make StartScaleSessionCommand.ScaleId required.
- No implementation yet; design only.
Expected Output:
- Plan only with API boundary, command/query shape, validation, cache/source lookup, response model, risks, and verification.
- Identify whether public API changes require explicit approval.
- Identify which skill report must be updated if the design is approved.
```
