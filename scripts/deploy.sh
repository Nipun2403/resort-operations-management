#!/bin/bash
set -euo pipefail

# ─── Colors ───
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

log_info() { echo -e "${GREEN}[INFO]${NC} $1"; }
log_warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }
log_step() { echo -e "${BLUE}[STEP]${NC} $1"; }

# ─── Load Environment ───
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
if [[ ! -f "$SCRIPT_DIR/.env.local" ]]; then
    log_error ".env.local not found in $SCRIPT_DIR"
    exit 1
fi
set -a
source "$SCRIPT_DIR/.env.local"
set +a

# ─── Defaults ───
RG="${RESOURCE_GROUP}"
LOCATION="${LOCATION:-centralindia}"
UNIQUE_SUFFIX="${UNIQUE_SUFFIX:-demo1}"
PG_ADMIN_PASSWORD="${POSTGRES_ADMIN_PASSWORD}"
STORAGE_KEY="${STORAGE_ACCOUNT_KEY}"
GROQ_KEY="${GROQ_API_KEY}"
EMAIL_SMTP_PASS="${EMAIL_SMTP_APP_PASSWORD}"
JWT_KEY="${JWT_KEY}"
WPS_CONNECTION_STRING="${WEB_PUBSUB_CONNECTION_STRING}"

# ─── Validate Required ───
required_vars=("AZURE_SUBSCRIPTION_ID" "RESOURCE_GROUP" "UNIQUE_SUFFIX" "POSTGRES_ADMIN_PASSWORD" "STORAGE_ACCOUNT_KEY" "GROQ_API_KEY" "EMAIL_SMTP_APP_PASSWORD" "WEB_PUBSUB_CONNECTION_STRING" "JWT_KEY")
for var in "${required_vars[@]}"; do
    if [[ -z "${!var:-}" ]]; then
        log_error "Required variable $var is not set in .env.local"
        exit 1
    fi
done

# ─── Get Local IPv4 (required for PostgreSQL firewall) ───
log_step "Detecting local IPv4 address..."
MY_IP=$(curl -s -4 https://api.ipify.org 2>/dev/null || curl -s -4 ifconfig.me 2>/dev/null || true)
if [[ ! "$MY_IP" =~ ^[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+$ ]]; then
    log_error "Could not detect valid IPv4 address (got: '${MY_IP}'). Check internet connectivity."
    exit 1
fi
log_info "Local IP: $MY_IP"

# ─── Image tag (timestamp ensures new revision on every deploy) ───
IMAGE_TAG="v$(date +%Y%m%d%H%M%S)"

# ─── MAIN DEPLOYMENT ───
log_info "=== Hotel Management Deployment Started ==="
log_info "Resource Group:  $RG"
log_info "Location:        $LOCATION"
log_info "Unique Suffix:   $UNIQUE_SUFFIX"
log_info "Subscription:    $AZURE_SUBSCRIPTION_ID"
log_info "Image Tag:       $IMAGE_TAG"

# ─── 1. Set Subscription ───
log_step "Setting Azure subscription..."
az account set --subscription "$AZURE_SUBSCRIPTION_ID"

# ─── 2. Deploy Infrastructure (Bicep) ───
log_step "Deploying infrastructure (Bicep)..."
log_info "This takes ~5-8 minutes..."

az deployment group create \
  --resource-group "$RG" \
  --template-file "$SCRIPT_DIR/../infra/main.bicep" \
  --parameters \
    uniqueSuffix="$UNIQUE_SUFFIX" \
    pgAdminPassword="$PG_ADMIN_PASSWORD" \
    myPublicIP="$MY_IP" \
    storageAccountKey="$STORAGE_KEY" \
    openRouterKey="$GROQ_KEY" \
    emailSmtpPass="$EMAIL_SMTP_PASS" \
    jwtKey="$JWT_KEY" \
    webPubSubConnectionString="$WPS_CONNECTION_STRING" \
  --output table

# ─── 3. Get ACR Details & Credentials ───
log_step "Getting ACR details and credentials..."
ACR_NAME=$(az acr list -g "$RG" --query "[0].name" -o tsv)
ACR_LOGIN_SERVER="${ACR_NAME}.azurecr.io"

if [[ -z "$ACR_NAME" ]]; then
    log_error "ACR not found after deployment"
    exit 1
fi
log_info "ACR: $ACR_NAME ($ACR_LOGIN_SERVER)"

ACR_PASSWORD=$(az acr credential show -n "$ACR_NAME" | jq -r '.passwords[0].value')
if [[ -z "$ACR_PASSWORD" || "$ACR_PASSWORD" == "null" ]]; then
    log_error "Could not get ACR password"
    exit 1
fi
log_info "ACR credentials retrieved"

# ─── 4. Build & Push Images (Cloud Build) ───
log_step "Building backend image ($IMAGE_TAG)..."
az acr build --registry "$ACR_NAME" \
  --image "hotel-api:$IMAGE_TAG" \
  --file "$SCRIPT_DIR/../Backend/HotelManagement.API/Dockerfile" \
  "$SCRIPT_DIR/.."

log_step "Building frontend image ($IMAGE_TAG)..."
az acr build --registry "$ACR_NAME" \
  --image "hotel-web:$IMAGE_TAG" \
  --file "$SCRIPT_DIR/../Frontend/Dockerfile" \
  "$SCRIPT_DIR/.."

# ─── 5. Update Container Apps with ACR Credentials & Images ───
log_step "Updating backend Container App..."
az containerapp registry set \
  --name "hotel-api-$UNIQUE_SUFFIX" -g "$RG" \
  --server "$ACR_LOGIN_SERVER" \
  --username "$ACR_NAME" \
  --password "$ACR_PASSWORD"
az containerapp update \
  --name "hotel-api-$UNIQUE_SUFFIX" -g "$RG" \
  --image "$ACR_LOGIN_SERVER/hotel-api:$IMAGE_TAG" \
  --set-env-vars "ASPNETCORE_ENVIRONMENT=Development"

log_step "Updating frontend Container App..."
az containerapp registry set \
  --name "hotel-web-$UNIQUE_SUFFIX" -g "$RG" \
  --server "$ACR_LOGIN_SERVER" \
  --username "$ACR_NAME" \
  --password "$ACR_PASSWORD"
az containerapp update \
  --name "hotel-web-$UNIQUE_SUFFIX" -g "$RG" \
  --image "$ACR_LOGIN_SERVER/hotel-web:$IMAGE_TAG"

log_step "Updating Container App Jobs..."
for job in image-validation-job orphan-cleanup-job blob-cleanup-job proposal-cleanup-job idempotency-cleanup-job; do
  az containerapp job update \
    --name "$job" -g "$RG" \
    --image "$ACR_LOGIN_SERVER/hotel-api:$IMAGE_TAG"
done

# ─── 6. Configure Azure Storage CORS (for direct browser uploads via SAS) ───
log_step "Configuring Azure Storage CORS..."
FRONTEND_FQDN=$(az containerapp show -n "hotel-web-$UNIQUE_SUFFIX" -g "$RG" --query "properties.configuration.ingress.fqdn" -o tsv 2>/dev/null || true)
az storage cors clear --services b --account-name nsdeply00 --account-key "$STORAGE_KEY"
az storage cors add \
  --services b \
  --methods DELETE GET HEAD MERGE OPTIONS POST PUT \
  --origins \
    "https://${FRONTEND_FQDN:-hotel-web-$UNIQUE_SUFFIX.azurecontainerapps.io}" \
    "http://localhost:4200" \
    "https://localhost:4200" \
  --allowed-headers "*" \
  --exposed-headers "*" \
  --max-age 3600 \
  --account-name nsdeply00 \
  --account-key "$STORAGE_KEY"
log_info "Storage CORS configured for $FRONTEND_FQDN"

# ─── 8. Add PostgreSQL Firewall Rule for Migrations ───
log_step "Adding PostgreSQL firewall rule for migrations..."
FIREWALL_RULE_NAME="allow-deploy-$UNIQUE_SUFFIX"
az postgres flexible-server firewall-rule create \
  -g "$RG" \
  --server-name "pg-hotel-mgmt-$UNIQUE_SUFFIX" \
  --name "$FIREWALL_RULE_NAME" \
  --start-ip-address "$MY_IP" \
  --end-ip-address "$MY_IP" \
  2>/dev/null || az postgres flexible-server firewall-rule update \
  -g "$RG" \
  --server-name "pg-hotel-mgmt-$UNIQUE_SUFFIX" \
  --name "$FIREWALL_RULE_NAME" \
  --start-ip-address "$MY_IP" \
  --end-ip-address "$MY_IP"
log_info "Firewall rule '$FIREWALL_RULE_NAME' applied for $MY_IP"

# ─── 7. Run EF Core Migrations ───
log_step "Running EF Core migrations..."
PG_FQDN=$(az postgres flexible-server show -g "$RG" -n "pg-hotel-mgmt-$UNIQUE_SUFFIX" --query fullyQualifiedDomainName -o tsv)

if [[ -z "$PG_FQDN" ]]; then
    log_error "Could not get PostgreSQL FQDN"
    exit 1
fi

CONN_STR="Host=$PG_FQDN;Database=HotelManagement;Username=pgadmin;Password=$PG_ADMIN_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"

cd "$SCRIPT_DIR/../Backend/HotelManagement.API"
dotnet ef database update --connection "$CONN_STR"
cd "$SCRIPT_DIR"

# ─── 8. Remove Migration Firewall Rule ───
log_step "Removing migration firewall rule..."
az postgres flexible-server firewall-rule delete \
  -g "$RG" \
  --server-name "pg-hotel-mgmt-$UNIQUE_SUFFIX" \
  --name "$FIREWALL_RULE_NAME" \
  --yes 2>/dev/null || log_warn "Could not remove firewall rule (may not exist)"

# ─── 9. Wait for Container Apps to Stabilize ───
log_step "Waiting 60s for Container Apps to stabilize..."
sleep 60

# ─── 10. Verify Deployment ───
log_step "Verifying deployment..."
"$SCRIPT_DIR/verify.sh"

# ─── 11. Output URLs ───
BACKEND_URL="https://$(az containerapp show -n "hotel-api-$UNIQUE_SUFFIX" -g "$RG" --query "properties.configuration.ingress.fqdn" -o tsv)"
FRONTEND_URL="https://$(az containerapp show -n "hotel-web-$UNIQUE_SUFFIX" -g "$RG" --query "properties.configuration.ingress.fqdn" -o tsv)"

echo ""
log_info "=== DEPLOYMENT COMPLETE ==="
log_info "Backend:  $BACKEND_URL"
log_info "Frontend: $FRONTEND_URL"
log_info "Swagger:  $BACKEND_URL/swagger"
log_info ""
log_info "=== Next Steps ==="
log_info "1. Open: $FRONTEND_URL"
log_info "2. Verify: $SCRIPT_DIR/verify.sh"
log_info "3. Cleanup: $SCRIPT_DIR/cleanup.sh"
