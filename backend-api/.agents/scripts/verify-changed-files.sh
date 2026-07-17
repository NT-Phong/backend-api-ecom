#!/usr/bin/env bash
set -euo pipefail

echo "Changed files:"
git status --short

echo
echo "Tracked markdown links to .agents files:"
rg -n "\.agents/" AGENTS.md .agents || true

echo
echo "Potential protected config changes:"
git status --short -- appsettings.json appsettings.Development.json ".env*" || true
