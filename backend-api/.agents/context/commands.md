# Commands

## Shell

Use Windows PowerShell and prefer `rg` for targeted source discovery.

## Build and Test

For authorized source changes, run the narrowest safe build/test automatically when the environment permits it. Report commands that cannot run and the remaining risk.

```powershell
dotnet build Core\Ecom.Domain\Ecom.Domain.csproj --no-restore
dotnet build Core\Ecom.Application\Ecom.Application.csproj --no-restore
dotnet build Infrastructure\Ecom.Infrastructure\Ecom.Infrastructure.csproj --no-restore
dotnet build Presentation\Ecom.API\Ecom.API.csproj --no-restore
dotnet test Tests\Ecom.Domain.Tests\Ecom.Domain.Tests.csproj --no-restore
```

Use `--no-restore` only after dependencies are available. Start at the lowest changed project and expand verification in proportion to risk.

## Approval-Gated Commands

Request explicit approval before generating/applying migrations, modifying runtime configuration or dependencies, starting external services, changing production data, deploying, pushing, or merging.

Do not run `dotnet ef database update` as an implementation convenience. Generate and review migration SQL before any approved application.

## Agent-System Verification

```powershell
powershell -ExecutionPolicy Bypass -File .agents\scripts\verify-agent-system.ps1
```
