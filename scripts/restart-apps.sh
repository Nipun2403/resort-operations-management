#!/bin/bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# ─── Load Environment ───
if [[ -f "$SCRIPT_DIR/.env.local" ]]; then
    set -a
    source "$SCRIPT_DIR/.env.local"
    set +a
fi

UNIQUE_SUFFIX="${UNIQUE_SUFFIX:-demo1}"
SUFFIX="$UNIQUE_SUFFIX"
RG="${RESOURCE_GROUP}"

echo "Restarting Container Apps..."

az containerapp restart -n "hotel-api-$SUFFIX" -g "$RG"
az containerapp restart -n "hotel-web-$SUFFIX" -g "$RG"

echo "Apps restarted. Waiting 15s for stabilization..."
sleep 15

"$SCRIPT_DIR/verify.sh"