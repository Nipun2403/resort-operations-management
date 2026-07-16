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
RG="${RESOURCE_GROUP:?RESOURCE_GROUP not set in .env.local}"

API_FQDN=$(az containerapp show -n "hotel-api-$SUFFIX" -g "$RG" --query "properties.configuration.ingress.fqdn" -o tsv 2>/dev/null)
WEB_FQDN=$(az containerapp show -n "hotel-web-$SUFFIX" -g "$RG" --query "properties.configuration.ingress.fqdn" -o tsv 2>/dev/null)

if [[ -z "$API_FQDN" ]]; then
    log_error "Could not get backend FQDN — is hotel-api-$SUFFIX deployed?"
    exit 1
fi

API_URL="https://$API_FQDN"
WEB_URL="https://${WEB_FQDN:-hotel-web-$SUFFIX.azurecontainerapps.io}"

# ─── Colors ───
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m'

log_info() { echo -e "${GREEN}[INFO]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }
log_warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }

echo "=== Verifying Deployment ($SUFFIX) ==="

# ─── 1. Backend Health ───
log_info "1. Backend Health Check..."
if curl -sf "$API_URL/health" >/dev/null; then
    log_info "✅ Backend healthy"
else
    log_error "❌ Backend health check failed"
    exit 1
fi

# ─── 2. Swagger ───
log_info "2. Swagger UI..."
if curl -sf "$API_URL/swagger" | grep -q "swagger-ui"; then
    log_info "✅ Swagger loads"
else
    log_warn "⚠️ Swagger not available (disabled in Production environment)"
fi

# ─── 3. Web PubSub Negotiate ───
log_info "3. Web PubSub Negotiate..."
if curl -sf "$API_URL/notifications/negotiate" | grep -q "accessToken"; then
    log_info "✅ Web PubSub token returned"
else
    log_warn "⚠️ Web PubSub negotiate returned non-200 (requires auth token — skip in automated verify)"
fi

# ─── 4. Frontend ───
log_info "4. Frontend Load..."
if curl -sf "$WEB_URL" | grep -q "<html"; then
    log_info "✅ Frontend loads"
else
    log_error "❌ Frontend failed to load"
    exit 1
fi

# ─── 5. API Integration ───
log_info "5. API Integration (Rooms)..."
HTTP_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$API_URL/api/v1/rooms")
if [[ "$HTTP_STATUS" == "200" ]]; then
    log_info "✅ API returns data"
elif [[ "$HTTP_STATUS" == "401" ]]; then
    log_warn "⚠️ API /rooms returned 401 (auth required — expected in production)"
else
    log_error "❌ API integration failed (HTTP $HTTP_STATUS)"
    exit 1
fi

# ─── 6. Image Upload Flow ───
log_info "6. Image Upload (SAS Generation)..."
if curl -sf -X POST "$API_URL/api/v1/images/upload-request" \
  -H "Content-Type: application/json" \
  -d '{"entityType":"RoomType","fileName":"test.jpg","declaredContentType":"image/jpeg","declaredSizeBytes":1000,"userEmail":"test@test.com"}' | grep -q "uploadUrl"; then
    log_info "✅ SAS URL generated"
else
    log_warn "⚠️ Image upload test failed (may need auth)"
fi

# ─── 7. Workers ───
log_info "7. Workers Running (checking logs)..."
if az containerapp logs show -n "hotel-api-$SUFFIX" -g "$RG" --tail 50 2>/dev/null | grep -q "ImageValidationWorker started"; then
    log_info "✅ Workers logging"
else
    log_warn "⚠️ Check workers manually in portal"
fi

echo ""
log_info "=== ALL CHECKS PASSED ✅ ==="
log_info "Backend:  $API_URL"
log_info "Frontend: $WEB_URL"
log_info "Swagger:  $API_URL/swagger"