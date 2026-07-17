# Prompt Example - Refactoring

```markdown
Task Type: Refactoring
Short Description: Reduce duplication in warehouse notification recipient resolution without changing behavior.
Boundary/Module: NotificationRecipientResolver and warehouse notification call sites.
Additional Details:
- Read .agents/context/task-router.md first.
- Read .agents/rules/code-quality.md and .agents/context/conventions.md.
- Read .agents/skills/FCM-notification/report-bug.md if notification recipient behavior is touched.
- Preserve public API, notification payloads, role matrix, and existing TResult/ErrorCodes behavior.
- Do not add dependencies or broad abstractions.
Expected Output:
- Plan -> Implement Plan -> Report -> Update skill files.
- Provide a small refactor plan before editing.
- Implement only if the refactor has clear duplication removal and no behavior change.
- Update the relevant skill report only if durable notification behavior or risk changes.
- Recommend focused build verification.
```
