# Testing And Verification

## Current Test Layout
No dedicated test project is currently visible in `Ecom.sln`.

Files named `TestDeviceHubConnection` or `TestWarningDeviceHubConnection` are application features, not automated tests.

When a suitable test project is added, prefer focused tests before broad builds.

## Default Verification
Build verification is user-run by default. After implementation, the agent should summarize completed work and provide the exact recommended build command, then wait for the user to run it manually. Do not run builds directly unless the user explicitly asks.

- Docs-only or agent-guidance-only change: verify file existence, `.agents` links, and stale `.github/skills` references.
- Domain-only change: build `Core/Ecom.Domain/Ecom.Domain.csproj`.
- Application-layer change: build `Core/Ecom.Application/Ecom.Application.csproj`.
- Infrastructure change: build `Infrastructure/Ecom.Infrastructure/Ecom.Infrastructure.csproj`.
- API/controller/hub/middleware change: build `Presentation/Ecom.API/Ecom.API.csproj`.
- Cross-layer change: build the narrowest affected project first, then the API project if needed.
- Migrations/schema change: ask first, then verify migration and API build.

## Build Commands
Provide these commands to the user for manual execution unless explicitly asked to run them.

When dependencies are already restored, prefer:

```powershell
dotnet build Core\Ecom.Domain\Ecom.Domain.csproj --no-restore
dotnet build Core\Ecom.Application\Ecom.Application.csproj --no-restore
dotnet build Infrastructure\Ecom.Infrastructure\Ecom.Infrastructure.csproj --no-restore
dotnet build Presentation\Ecom.API\Ecom.API.csproj --no-restore
```

Known approved build prefixes may exist for:
- `dotnet build Infrastructure\Ecom.Infrastructure\Ecom.Infrastructure.csproj --no-restore`
- `dotnet build Presentation\Ecom.API\Ecom.API.csproj --no-restore -p:OutDir=C:\tmp\Ecom-api-build\`

If restore or external network access is required, follow the active tool permission rules before escalating.

## Test Policy
- Behavior-changing code should include or update tests when a test harness exists.
- Test behavior, not implementation details.
- Use existing fixtures and helpers.
- Do not skip or disable tests without explicit approval.
- If tests cannot be run or do not exist, report the verification gap clearly.
- Do not claim test coverage exists unless a real test project or command was run.

