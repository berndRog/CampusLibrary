#!/usr/bin/env bash
set -euo pipefail

# Deletes generated build artifacts from a .NET solution/project tree
# so that ZIP archives stay small.
#
# Usage:
#   ./cleanup-dotnet.sh [path]
#   ./cleanup-dotnet.sh [path] --dry-run
#   ./cleanup-dotnet.sh [path] --include-local-db
#
# Default:
# - removes bin/, obj/, TestResults/, coverage output, .vs/, .idea/, .vscode/
# - removes macOS ZIP artefacts: .DS_Store and __MACOSX
# - removes log files: *.log
#
# Optional:
# - --include-local-db also removes local SQLite files (*.db, *.db-shm, *.db-wal)
#
# The script does not delete .git.

ROOT="$(pwd)"
DRY_RUN=false
INCLUDE_LOCAL_DB=false

for arg in "$@"; do
   case "$arg" in
      --dry-run)
         DRY_RUN=true
         ;;
      --include-local-db)
         INCLUDE_LOCAL_DB=true
         ;;
      *)
         ROOT="$arg"
         ;;
   esac
done

if [[ ! -d "$ROOT" ]]; then
   echo "Path does not exist or is not a directory: $ROOT" >&2
   exit 1
fi

ROOT="$(cd "$ROOT" && pwd)"

echo "Cleaning generated artifacts below: $ROOT"
if [[ "$DRY_RUN" == true ]]; then
   echo "Mode: dry-run"
fi

run_delete() {
   local target="$1"
   if [[ "$DRY_RUN" == true ]]; then
      echo "Would delete: $target"
   else
      echo "Deleting: $target"
      rm -rf "$target"
   fi
}

# Directory artifacts.
while IFS= read -r -d '' dir; do
   run_delete "$dir"
done < <(
   find "$ROOT" \
      -path "$ROOT/.git" -prune -o \
      -type d \( \
         -name bin -o \
         -name obj -o \
         -name TestResults -o \
         -name coverage -o \
         -name .vs -o \
         -name .idea -o \
         -name .vscode -o \
         -name __MACOSX \
      \) -print0
)

# File artifacts that are safe to remove.
while IFS= read -r -d '' file; do
   run_delete "$file"
done < <(
   find "$ROOT" \
      -path "$ROOT/.git" -prune -o \
      -type f \( \
         -name '.DS_Store' -o \
         -name '*.trx' -o \
         -name '*.coverage' -o \
         -name '*.coveragexml' -o \
         -name '*.log' \
      \) -print0
)

if [[ "$INCLUDE_LOCAL_DB" == true ]]; then
   while IFS= read -r -d '' file; do
      run_delete "$file"
   done < <(
      find "$ROOT" \
         -path "$ROOT/.git" -prune -o \
         -type f \( \
            -name '*.db' -o \
            -name '*.db-shm' -o \
            -name '*.db-wal' \
         \) -print0
   )
fi

echo "Done."