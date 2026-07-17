# Prompt Example - Bug Debugging

```markdown
Task Type: Bug Debugging
Short Description: FCM notification row is created on dev, but push delivery does not show FCM_EVENT_RECEIVED or FCM_SEND_RESULT logs.
Boundary/Module: ImportReceipt create flow, NotificationService, BulkNotificationEventHandler, FcmNotificationEventHandler, FirebaseFcmService.
Additional Details:
- Read .agents/context/task-router.md first.
- Read .agents/skills/FCM-notification/report-bug.md and review.md before source.
- Use source code as current truth if reports disagree.
- Do not change public API, appsettings*.json, secrets, dependencies, or notification payload contracts.
- Expected logs currently seen:
  ```text
  BulkNotification created | Count=...
  ```
- Missing logs:
  ```text
  FCM_EVENT_RECEIVED
  FCM_TOKEN_RESOLVED
  FCM_SEND_RESULT
  ```
Expected Output:
- Plan -> Implement Plan -> Report -> Update skill files.
- Identify root cause with file:line evidence.
- Implement the smallest safe fix if source evidence is conclusive.
- Update .agents/skills/FCM-notification/report-bug.md with the delta.
- Recommend the narrowest verification command; do not run builds unless asked.
```
