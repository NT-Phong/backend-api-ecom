# Quick Scan

Use exact Commerce routes/entities before broad scans.

```powershell
powershell -ExecutionPolicy Bypass -File .agents\scripts\find-related-files.ps1 -Term "<term>"
rg -n "HttpGet|HttpPost|HttpPatch|HttpDelete|<route>" Presentation/Ecom.API/Controllers
rg -n "<CommandOrQuery>|IRequestHandler|AbstractValidator" Core/Ecom.Application/Features
rg -n "<Entity>|IAggregateRoot|CommerceDomainException" Core/Ecom.Domain
rg -n "HasOne|HasIndex|HasCheckConstraint|<Entity>" Infrastructure/Ecom.Infrastructure/Persistence
rg -n "Fact|Theory|<Entity>" Tests
```

Open in order: controller, request/validator, handler, aggregate, EF/query implementation, tests. Source overrides guidance.
