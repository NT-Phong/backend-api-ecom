# Bad Patterns

Avoid these patterns unless the user explicitly asks and the tradeoff is justified.

## Business Logic In Controller
Do not query DbContext, Redis, or external services directly from controllers for business decisions.

## Broad Cleanup During Bug Fix
Do not split `PondController`, rename typo-bearing files, normalize mojibake, or reformat large handlers while fixing one local bug.

## Contract Rename Without Migration
Do not rename existing public types or files such as `Hanlder`, `Vadilator`, `CreateImportRecept`, or `ExportImporReceip` just because they are misspelled.

## Realtime Contract Drift
Do not change SignalR group names such as `ScaleSession_{sessionId}`, `Scale_{scaleId}`, `Device_{iotDeviceId}`, `Cycle_{cycleId}`, or `Pond_{pondId}` without coordinating client changes.

## Error Hiding
Do not add broad `catch (Exception)` blocks that return success, null, or a generic fallback without logging useful context.

## Sensitive Logging
Do not log refresh tokens, JWTs, FCM tokens, query-string `access_token`, connection strings, IoT auth values, Basic Auth credentials, or raw file payloads.

## Unsafe Validation Workaround
Do not use unsafe casts, blanket disables, weakened validators, or skipped policy checks to bypass failures.
