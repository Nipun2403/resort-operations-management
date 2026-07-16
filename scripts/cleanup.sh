#!/bin/bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# ─── Load Environment ───
if [[ -f "$SCRIPT_DIR/.env.local" ]]; then
    set -a
    source "$SCRIPT_DIR/.env.local"
    set +a
fi

RG="${RESOURCE_GROUP}"

echo "=== FULL CLEANUP ==="
echo "Resource Group: $RG"
echo "This will DELETE ALL RESOURCES in the resource group."
echo ""
read -p "Type 'yes' to confirm: " confirm
if [[ "$confirm" != "yes" ]]; then
    echo "Cancelled."
    exit 1
fi

echo "Deleting resource group..."
az group delete --name "$RG" --yes --no-wait

echo ""
echo "=== Cleanup initiated ==="
echo "Deletion runs in background. Check status with:"
echo "  az group exists --name $RG"