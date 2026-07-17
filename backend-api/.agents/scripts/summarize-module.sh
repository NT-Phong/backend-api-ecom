#!/usr/bin/env bash
set -euo pipefail

module="${1:-}"

if [ -z "$module" ]; then
  echo "Usage: .agents/scripts/summarize-module.sh <path>"
  exit 1
fi

echo "Files under $module:"
rg --files "$module" | sed 's#\\#/#g' | sort

echo
echo "Key symbols:"
rg -n "class |interface |record |enum |IRequest|IRequestHandler|AbstractValidator|EnableUnitOfWork" "$module" || true
