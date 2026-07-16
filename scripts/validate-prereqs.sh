#!/bin/bash
set -euo pipefail

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

log_info() { echo -e "${GREEN}[INFO]${NC} $1"; }
log_warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

if [[ ! -f "$SCRIPT_DIR/.env.local" ]]; then
    log_error ".env.local not found in $SCRIPT_DIR"
    exit 1
fi

# Load .env.local safely
while IFS='=' read -r key value; do
    [[ -z "$key" ]] && continue
    [[ "$key" =~ ^[[:space:]]*# ]] && continue

    key=$(echo "$key" | xargs)
    value=$(echo "$value" | xargs)

    # Remove surrounding quotes
    value="${value%\"}"
    value="${value#\"}"
    value="${value%\'}"
    value="${value#\'}"

    export "$key=$value"
done < "$SCRIPT_DIR/.env.local"

checks_passed=0
checks_failed=0

check() {
    local name="$1"
    local cmd="$2"

    echo "DEBUG: Checking '$name'"
    echo "       Command: $cmd"

    if eval "$cmd" >/dev/null 2>&1; then
        log_info "✅ $name"
        ((++checks_passed))
    else
        log_error "❌ $name"
        ((++checks_failed))
    fi
}

log_info "Validating prerequisites..."
echo ""

check "Azure CLI installed" "command -v az"
check "Azure CLI logged in" "az account show"
check "Subscription ID set" "[ -n \"\${AZURE_SUBSCRIPTION_ID:-}\" ]"
check "Subscription accessible" "az account show --subscription \"\$AZURE_SUBSCRIPTION_ID\""
check "Resource Group name set" "[ -n \"\${RESOURCE_GROUP:-}\" ]"
check "Resource Group exists" "az group show -n \"\$RESOURCE_GROUP\""
check "Location set" "[ -n \"\${LOCATION:-}\" ]"
check "Unique suffix set" "[ -n \"\${UNIQUE_SUFFIX:-}\" ]"
check "PostgreSQL password set" "[ -n \"\${POSTGRES_ADMIN_PASSWORD:-}\" ]"
check "Storage account key set" "[ -n \"\${STORAGE_ACCOUNT_KEY:-}\" ]"
check "Groq API key set" "[ -n \"\${GROQ_API_KEY:-}\" ]"
check "Email SMTP password set" "[ -n \"\${EMAIL_SMTP_APP_PASSWORD:-}\" ]"
check "JWT key set or will generate" "[ -n \"\${JWT_KEY:-}\" ] || true"
check "Web PubSub connection string set" "[ -n \"\${WEB_PUBSUB_CONNECTION_STRING:-}\" ]"
check "dotnet installed" "command -v dotnet"
check "dotnet version 10+" "dotnet --version | grep -E '^10\.'"
check "node installed" "command -v node"
check "node version 20+" "node --version | grep -E '^v2[0-9]\.'"
check "npm installed" "command -v npm"
check "dotnet-ef installed" "dotnet tool list -g | grep dotnet-ef"

echo ""
echo "========================================"
echo "Checks Passed : $checks_passed"
echo "Checks Failed : $checks_failed"
echo "========================================"

if [[ $checks_failed -eq 0 ]]; then
    log_info "All $checks_passed checks passed! ✅"
    exit 0
else
    log_error "$checks_failed check(s) failed. Fix the above issues and re-run."
    exit 1
fi