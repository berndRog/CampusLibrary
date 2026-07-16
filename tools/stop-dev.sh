#!/usr/bin/env bash
set -euo pipefail

# Stops all CampusLibrary dev services and closes their Terminal windows.
# Usage: ./tools/stop-dev.sh [http|https]   (default: https)

PROFILE="${1:-https}"
LABELS=("IdentityAccessServer" "CampusLibraryApi" "CampusLibraryClient")
CAMPUS_STATE="/tmp/campus-dev-state"

if [[ "$PROFILE" == "https" ]]; then
  PORTS=(7010 8010 6040)
else
  PORTS=(7011 8012 5040)
fi

# 1. Kill processes by port
any_killed=false
for i in "${!PORTS[@]}"; do
  port="${PORTS[$i]}"
  label="${LABELS[$i]}"
  pids=$(lsof -ti TCP:"$port" -sTCP:LISTEN 2>/dev/null || true)
  if [[ -n "$pids" ]]; then
    echo "Stopping $label (port $port, PID $pids) ..."
    kill -TERM $pids 2>/dev/null || kill -KILL $pids 2>/dev/null || true
    any_killed=true
  else
    echo "$label (port $port) – not running."
  fi
done

# 2. Close Terminal windows by saved window ID
echo ""
for label in "${LABELS[@]}"; do
  wid_file="$CAMPUS_STATE/${label}.wid"
  if [[ -f "$wid_file" ]]; then
    wid=$(cat "$wid_file")
    echo "Closing Terminal window for $label ..."
    osascript \
      -e 'tell application "Terminal"' \
      -e "  close (every window whose id is $wid)" \
      -e 'end tell' 2>/dev/null || true
    rm -f "$wid_file"
  fi
done

$any_killed && echo "" && echo "Done. All services stopped and windows closed." \
            || echo "No services were running."
