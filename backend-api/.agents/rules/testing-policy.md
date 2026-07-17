---
trigger: manual
---

# Testing Policy

## Default Expectation
Behavior-changing code should include or update tests when a suitable test project or test harness exists.

This repository currently has no dedicated test project in `Ecom.sln`. When tests are not available, run the narrowest relevant build and document the gap.

Do not treat feature commands named `Test*Connection` as automated tests.

## Test Selection
Prefer the smallest test that proves the change:
1. Unit test for pure business logic.
2. Integration test for API or database behavior.
3. Component/UI test for rendering or interaction.
4. End-to-end test only when the user flow crosses major boundaries.

## Verification Strategy
- Run focused tests first.
- Run typecheck, analyzer, or build checks when types or style may be affected.
- Run full solution build only for broad changes or release-sensitive tasks.
- For docs-only changes, verify links and file paths instead of building source.
- For API changes, prefer `Presentation\Ecom.API\Ecom.API.csproj` build after the narrower affected project build.
- For agent-guidance changes, run `.agents/scripts/verify-guidance.ps1` or equivalent `rg` checks.

## Test Quality
- Test behavior, not implementation details.
- Avoid brittle snapshot tests unless the project already uses them.
- Use existing fixtures and helpers.
- Do not skip or disable failing tests without explicit approval.

