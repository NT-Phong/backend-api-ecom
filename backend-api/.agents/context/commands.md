# Commands

## Shell
The shared workspace normally runs on Windows PowerShell.

Use `rg` for searches. Prefer focused commands and avoid broad recursive reads unless a project-wide analysis is explicitly requested.

## Build Verification
Builds are user-run by default. When a build is needed after an implementation phase, the agent must stop and report:
- what changed,
- why build verification is needed,
- the narrowest recommended build scope,
- the exact command for the user to run manually.

Do not run build commands unless the user explicitly asks the agent to run them. Treat user-provided build output as the verification result.

Use `--no-restore` when packages are already restored:

```powershell
dotnet build Core\Ecom.Domain\Ecom.Domain.csproj --no-restore
dotnet build Core\Ecom.Application\Ecom.Application.csproj --no-restore
dotnet build Infrastructure\Ecom.Infrastructure\Ecom.Infrastructure.csproj --no-restore
dotnet build Presentation\Ecom.API\Ecom.API.csproj --no-restore
```

Suggested scope:
- Domain-only changes: build `Core\Ecom.Domain\Ecom.Domain.csproj`.
- Application changes: build `Core\Ecom.Application\Ecom.Application.csproj`.
- Infrastructure changes: build `Infrastructure\Ecom.Infrastructure\Ecom.Infrastructure.csproj`.
- API/controller/hub/middleware changes: build `Presentation\Ecom.API\Ecom.API.csproj`.
- Cross-layer changes: build the lowest changed project first, then `Presentation\Ecom.API`.

## Run Locally
Only run the API when the user wants runtime verification and local configuration is available:

```powershell
dotnet run --project Presentation\Ecom.API\Ecom.API.csproj
```

Do not modify `appsettings*.json` to make local runs work unless explicitly requested.

## EF Core
Treat migrations and model snapshots as high risk. Ask before creating, editing, or removing migrations.

Common discovery commands:

```powershell
rg "DbSet<|IEntityTypeConfiguration|HasOne|HasMany|HasIndex" Infrastructure\Ecom.Infrastructure\Persistence
rg "MigrationBuilder|ModelSnapshot" Infrastructure\Ecom.Infrastructure\Migrations
```

## Guidance Verification

```powershell
rg -n "\.github/skills|\.github\\skills" AGENTS.md .agents
rg -n "\.agents/" AGENTS.md .agents
Test-Path AGENTS.md
```

Use `.agents/scripts/verify-guidance.ps1` for a repeatable guidance sanity check.

