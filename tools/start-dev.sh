#!/usr/bin/env bash
set -euo pipefail

# Starts each service in its own Terminal window, in dependency order.
# Usage:  ./tools/start-dev.sh [http|https]

PROFILE="${1:-https}"
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
CAMPUS_STATE="/tmp/campus-dev-state"
mkdir -p "$CAMPUS_STATE"

# Ports per profile
if [[ "$PROFILE" == "https" ]]; then
  PORT_API=8010; PORT_CLIENT=6040
else
  PORT_API=8012; PORT_CLIENT=5040
fi

open_terminal() {
  local label="$1" cmd="$2"
  local script="$CAMPUS_STATE/${label}.sh"
  printf '#!/usr/bin/env bash\necho "=== %s ==="\n%s\nexec bash\n' \
    "$label" "$cmd" > "$script"
  chmod +x "$script"

  open -a Terminal "$script"
  sleep 0.8
  local wid
  wid=$(osascript \
    -e 'tell application "Terminal"' \
    -e '  return id of window 1' \
    -e 'end tell')
  echo "$wid" > "$CAMPUS_STATE/${label}.wid"
}

wait_for_port() {
  local port="$1" label="$2"
  echo "Waiting for $label on port $port ..."
  until nc -z localhost "$port" 2>/dev/null; do
    sleep 2
  done
  echo "$label is ready."
}

echo "==> Opening window: CampusLibraryApi ..."
open_terminal "CampusLibraryApi" \
  "dotnet run --project '$ROOT/CampusLibraryApi/CampusLibraryApi.csproj' --launch-profile $PROFILE"

wait_for_port $PORT_API "CampusLibraryApi"

echo "==> Opening window: CampusLibraryClient ..."
open_terminal "CampusLibraryClient" \
  "dotnet run --project '$ROOT/CampusLibraryClient/CampusLibraryClient.csproj' --launch-profile $PROFILE"

echo ""
echo "Both services started in separate Terminal windows."
