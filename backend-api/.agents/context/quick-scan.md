# Quick Scan

Use this when an agent needs to locate the right source files quickly without reading broad context.

## First Pass

1. Normalize the clue into one or two search terms:
   - route segment, endpoint name, command/query name, entity name, error code, telemetry code, Redis key prefix, SignalR group, or log phrase.
2. Run the related-file helper:

```powershell
powershell -ExecutionPolicy Bypass -File .agents\scripts\find-related-files.ps1 -Term "<term>"
```

3. If the result is too broad, search by boundary:

```powershell
rg -n "Route|HttpGet|HttpPost|HttpPut|HttpDelete|<route-term>" Presentation/Ecom.API/Controllers
rg -n "<CommandOrQuery>|IRequestHandler|AbstractValidator" Core/Ecom.Application/Features
rg -n "<EntityOrEnum>|<ErrorCode>|<MessageKey>" Core/Ecom.Domain
rg -n "<ServiceOrExternalTerm>|Redis|SignalR|TelemetryHandler|Code <code>" Infrastructure/Ecom.Infrastructure
```

4. Summarize a candidate module before opening many files:

```powershell
powershell -ExecutionPolicy Bypass -File .agents\scripts\summarize-module.ps1 -Path "Core\Ecom.Application\Features\<Feature>"
```

5. Open files in this order:
   - controller or hub entrypoint,
   - request DTO / command / query,
   - validator,
   - handler or application service,
   - domain entity or constants,
   - infrastructure implementation, repository, cache, worker, or external integration.

## Narrowing Rules

- Prefer exact symbols over English/Vietnamese descriptions.
- Search by both public route text and internal feature name when they differ.
- For typo-bearing names, search by class or method symbol before guessing filenames.
- For high-risk areas, load `.agents/context/risk-map.md` and the matching domain skill before editing.
- Do not open large files such as `DependencyInjection.cs`, report generators, IoT managers, `Permissions.cs`, or `ErrorCodes.cs` until a symbol search points there.
- Source code remains the current truth when quick-scan output conflicts with old reports.

## Useful Patterns

```powershell
rg -n "EnableUnitOfWork|SaveChangesAsync|CommitTransactionAsync|Repository<" Core/Ecom.Application/Features/<Feature>
rg -n "Permissions\.|Authorize\(" Presentation/Ecom.API Core/Ecom.Application/Features/<Feature>
rg -n "Hub|Group|SignalR|ScaleSession_|Device_|Pond_" Presentation Infrastructure Core
rg -n "TelemetryHandler|EventHub|MQTT|Code 300|Code 503|Oxy" Infrastructure/Ecom.Infrastructure
rg -n "catch \(Exception|TResult|ApiResponse|MessageKey|ErrorCodes" Core/Ecom.Application/Features/<Feature> Presentation/Ecom.API
```

