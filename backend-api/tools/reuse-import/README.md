# Mebieco Reuse Import

Use this tool to bring selected Mebieco files into a reviewable staging area before any source change. It is not a bulk-copy command for the whole repository.

## Safety model

- `reference_only`: report whether source and target match; never copy.
- `stage_only`: copy source into `.reuse-staging/files/` for review only.
- `copy_if_missing`: eligible for apply only when the target file does not exist.
- Protected paths are always blocked: `appsettings*.json`, migrations, `ApplicationDbContext`, Commerce entities/configurations, and Auth/current-user flows.

## Workflow

```powershell
# 1. Inspect availability and hashes. Does not write source files.
powershell -ExecutionPolicy Bypass -File tools\reuse-import\Compare-ReuseImport.ps1 `
  -SourceRoot 'D:\Source_Mebieco\backend-api'

# 2. Copy only stage_only/copy_if_missing entries into an ignored review area.
powershell -ExecutionPolicy Bypass -File tools\reuse-import\Stage-ReuseImport.ps1 `
  -SourceRoot 'D:\Source_Mebieco\backend-api'

# 3. Review staged files and the target diff.
git diff --no-index -- .reuse-staging\files\<target-path> <target-path>

# 4. Change only reviewed manifest entries to copy_if_missing, then apply explicitly.
powershell -ExecutionPolicy Bypass -File tools\reuse-import\Apply-ReuseImport.ps1 -Apply
```

Never copy connection strings, runtime settings, migrations, DbContext, Commerce persistence/domain files, or Auth flows through this tool. Patch those deliberately after source review.

## Manifest

Edit `reuse-manifest.json` when the actual Mebieco source path differs. Keep one entry per file, with a short Commerce rationale. The starter manifest contains current foundation references and optional missing integrations discovered in the live Ecom source.
