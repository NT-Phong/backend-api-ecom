#!/usr/bin/env bash
set -euo pipefail

term="${1:-}"

if [ -z "$term" ]; then
  echo "Usage: .agents/scripts/find-related-files.sh <feature-or-symbol>"
  exit 1
fi

rg -n --hidden \
  --glob '!bin/**' \
  --glob '!obj/**' \
  --glob '!.git/**' \
  --glob '!.vs/**' \
  "$term" \
  Core Infrastructure Presentation .agents
