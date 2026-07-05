# Direct-to-Azure-Blob Image Upload — Implementation Plan

Target entities: **Amenity**, **MenuItem**, **RoomType**
Stack: ASP.NET Core (C#) + PostgreSQL (local) backend, Angular admin portal, JWT bearer auth, no Azure resources yet.

---

## 0. Executive Summary & Key Architecture Decisions — ALL CONFIRMED

| Decision                            | Choice                                                                                                                                                         | Why                                                                                                                                                                    |
| ----------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Upload path                         | Client uploads **directly to Blob Storage** using a short-lived SAS issued by the backend. Backend never touches file bytes on the way in.                     | Matches your rule; avoids proxy cost/latency; standard Azure pattern.                                                                                                  |
| SAS type                            | **User delegation SAS** (Entra ID–signed), not account-key SAS                                                                                                 | Safer (no shared key ever leaves Azure AD-protected identity), revocable via RBAC, auditable.                                                                          |
| Storage layout                      | **CONFIRMED: one container** (`entity-images`), blobs named `{entityType}/{guid}{ext}`                                                                         | Simpler CORS/lifecycle config than 3 containers; entityType prefix keeps things organized/filterable.                                                                  |
| Read access to images               | **CONFIRMED: public read (`blob` level)** — the stored blob URL is a plain HTTPS URL, identical in shape to a pasted Google image link                         | Keeps the frontend **completely unchanged** — `ImageUrl`/`ImageUrls` stay plain strings, no SAS-refresh logic needed on read.                                          |
| DB schema impact on existing tables | **None.** `Amenity.ImageUrl`, `MenuItem.ImageUrl`, `RoomType.ImageUrls` keep their exact current type/shape.                                                   | Satisfies "front end continues to load images just like now" and "no existing feature breaks."                                                                         |
| New table                           | `UploadSession` — tracks every issued upload slot (pending/confirmed/rejected/attached/expired)                                                                | Needed for magic-byte validation, orphan cleanup, ownership/rate tracking, and to stop a confirmed-but-unattached blob from being reused by someone else.              |
| Async validation                    | **CONFIRMED: Option A** — ASP.NET Core `BackgroundService` consuming an **Azure Storage Queue**, inside the existing API app (no separate Azure Functions app) | You have no Functions infra yet; a queue + hosted worker inside the existing deployable is simpler ops for a "ground up" build.                                        |
| Backend identity/credential         | **CONFIRMED: App Registration + client secret** (`ClientSecretCredential`), since the backend runs on your local machine only today, not inside Azure          | Managed identity requires the workload to run on an Azure compute resource; not applicable yet. Swap-over note included for when/if you deploy to Azure.               |
| Max upload size                     | **CONFIRMED: 10 MB, uniform across Amenity, MenuItem, and RoomType**                                                                                           | Simplifies config to a single constant instead of a per-entity map.                                                                                                    |
| Max images per entity               | **CONFIRMED: RoomType up to 5 images; Amenity and MenuItem stay single-image** (already enforced today by `ImageUrl` being a single string)                    | RoomType's `List<string>? ImageUrls` needs an explicit cap added (today it's unbounded).                                                                               |
| UI pattern per image slot           | Toggle: **"Paste URL" / "Upload file"**, admin picks one per image slot; can switch/replace later                                                              | Coexists cleanly with the existing `imageUrlList` FormArray for RoomType and a single control for Amenity/MenuItem, with minimal disruption to the generic CRUD modal. |

All decisions below are final for this build — no more branching options.

---

## 1. Architecture Overview

```
Admin Portal (Angular)
  │
  │  1. POST /api/images/upload-sas  { entityType, fileName, contentType, sizeBytes }
  ▼
ASP.NET Core Backend
  │  - authenticates JWT, checks Admin role
  │  - rate-limits per user
  │  - validates extension + size against config limits
  │  - creates UploadSession (Status=Pending)
  │  - requests User Delegation Key from Entra ID (via DefaultAzureCredential)
  │  - signs a User Delegation SAS scoped to exactly one blob, write-only, ~15 min expiry
  │  - returns { sessionId, uploadUrl, blobUrl, expiresOn }
  ▲
  │  2. PUT directly to Blob Storage using @azure/storage-blob (block upload, progress, retry)
  ▼
Azure Blob Storage (container: entity-images, public read)
  │
  │  3. POST /api/images/{sessionId}/confirm   (client calls after upload finishes)
  ▼
ASP.NET Core Backend
  │  - enqueues message to Azure Storage Queue "image-validation-queue"
  │  - returns 202 Accepted
  │
  ▼
BackgroundService (in-process worker, dequeues async)
  │  - downloads first 512 bytes of the blob
  │  - checks magic bytes vs. declared extension/content-type
  │  - checks actual blob size vs. configured max
  │  - Status → Confirmed (valid) or Rejected (deletes blob, marks row)
  ▲
  │  4. GET /api/images/{sessionId}/status   (client polls every ~1s until Confirmed/Rejected)
  ▼
Admin Portal
  - on Confirmed: sets the form control's value = blobUrl, shows thumbnail, enables Save
  - on Rejected: shows error, lets admin retry
  - on Save (Create/Update entity): backend re-validates that any storage-domain URL
    in the payload maps to a Confirmed UploadSession owned by this admin, then flips
    it to Attached and stamps EntityType/EntityId
```

Orphan cleanup: a second lightweight `TimerBackgroundService` (or Azure Storage lifecycle management rule as a belt-and-suspenders backstop) deletes blobs/rows left in `Pending`/`Confirmed` (never attached) after 24h.

---

## 2. Azure Setup — Run These Yourself (CLI)

You said you want direct control over the Azure side. Here is the exact sequence; the agent will only write code that _consumes_ these resources, never provision them for you.

Replace `<...>` placeholders (region, unique suffix, your app's origin) before running.

```bash
# 0. Login
az login

# 1. Resource group
az group create \
  --name rg-hotelapp-storage \
  --location <region e.g. eastus>

# 2. Storage account (StorageV2, TLS1.2 min, HTTPS only, public blob access allowed
#    at account level so the container-level public-read setting in step 3 works)
az storage account create \
  --name sthotelimages<uniquesuffix> \
  --resource-group rg-hotelapp-storage \
  --location <region> \
  --sku Standard_LRS \
  --kind StorageV2 \
  --min-tls-version TLS1_2 \
  --https-only true \
  --allow-blob-public-access true

# 3. Container (public read at "blob" level = individual blobs are readable via
#    direct URL, but container listing is NOT public)
az storage container create \
  --account-name sthotelimages<uniquesuffix> \
  --name entity-images \
  --public-access blob \
  --auth-mode login

# 4. CORS — required so the Angular app (browser) can PUT directly to blob storage
az storage cors add \
  --account-name sthotelimages<uniquesuffix> \
  --services b \
  --methods GET PUT OPTIONS HEAD \
  --origins "http://localhost:4200" "https://<your-prod-admin-portal-domain>" \
  --allowed-headers "*" \
  --exposed-headers "*" \
  --max-age 3600 \
  --auth-mode login

# 5. Queue for async post-upload validation
az storage queue create \
  --name image-validation-queue \
  --account-name sthotelimages<uniquesuffix> \
  --auth-mode login

# 6. Identity for the backend to call Get-User-Delegation-Key + read/delete blobs.
#    CONFIRMED: backend runs on your local machine only today (not inside Azure),
#    so this uses an APP REGISTRATION + client secret (ClientSecretCredential),
#    not a managed identity. (If you later deploy the backend to Azure App Service
#    / Container Apps, switch to `az webapp identity assign` + a system-assigned
#    managed identity instead, and drop the client secret entirely — the code is
#    written behind one factory method specifically so this swap is a one-line
#    change later, see Section 4.2.)
az ad app create --display-name "hotelapp-image-upload-backend"
#    note the returned "appId" (this is your ClientId)
az ad sp create --id <appId-from-above>
az ad app credential reset --id <appId-from-above> --years 1
#    capture: appId (ClientId), tenantId (from `az account show`), and the
#    generated client secret — put these into YOUR OWN .env file under the
#    exact variable names AzureAd__ClientId, AzureAd__TenantId, and
#    AzureAd__ClientSecret (see Section 4.2's variable table). The agent will
#    never write to, edit, or read your .env file directly — it only reads
#    these values by name through the app's normal configuration pipeline.

# 7. Grant the app registration's service principal the least-privileged role
#    that can (a) generate a user delegation key AND (b) read/delete blobs for
#    validation + cleanup:
az role assignment create \
  --assignee <appId-from-step-6> \
  --role "Storage Blob Data Contributor" \
  --scope /subscriptions/<sub-id>/resourceGroups/rg-hotelapp-storage/providers/Microsoft.Storage/storageAccounts/sthotelimages<uniquesuffix>

# 8. Harden: disable Shared Key auth account-wide so nobody can ever fall back to
#    account-key SAS by mistake — forces Entra-only auth as designed.
az storage account update \
  --name sthotelimages<uniquesuffix> \
  --resource-group rg-hotelapp-storage \
  --allow-shared-key-access false

# 9. (Optional, recommended) Lifecycle management backstop: auto-delete blobs
#    under entity-images older than 2 days that were never "tiered"/touched —
#    a safety net in case the app-level cleanup worker ever fails. Configure via
#    Portal → Storage Account → Data management → Lifecycle management, rule
#    scoped to prefix "entity-images/", condition "days since creation > 2",
#    action "delete blob". (CLI equivalent uses a JSON policy file — say the
#    word if you want the exact JSON for this.)
```

**Fill your own `.env` with:** storage account URL, container name, queue name, tenant ID, client ID, client secret — using the exact variable names in Section 4.2's table. Do not hand these to the agent as plain chat text if you can avoid it; the agent only needs to know the variable _names_ (already fixed above), not their values, since it should never see or need the raw secret to write correct code.

---

## 3. Database Changes

One new table, zero changes to existing entities/DTOs.

```csharp
// HotelManagement.DAL/Entities/UploadSession.cs
public enum UploadStatus { Pending, Confirmed, Rejected, Attached, Expired }
public enum UploadEntityType { Amenity, MenuItem, RoomType }

public class UploadSession
{
    public Guid Id { get; set; }
    public string BlobName { get; set; } = string.Empty;      // "amenity/{guid}.jpg"
    public string BlobUrl { get; set; } = string.Empty;        // full public https url
    public string DeclaredContentType { get; set; } = string.Empty;
    public long DeclaredSizeBytes { get; set; }
    public long? ActualSizeBytes { get; set; }
    public UploadStatus Status { get; set; } = UploadStatus.Pending;
    public UploadEntityType EntityType { get; set; }
    public int? AttachedEntityId { get; set; }                 // null until Save succeeds
    public string UploadedByUserId { get; set; } = string.Empty; // from JWT sub/nameidentifier
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ConfirmedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public string? RejectionReason { get; set; }
}
```

Migration: `dotnet ef migrations add AddUploadSession -p HotelManagement.DAL -s HotelManagement.API` (adjust project names to your actual solution layout). No changes needed to `Amenity`, `MenuItem`, or `RoomType` tables or their DTOs — `ImageUrl`/`ImageUrls` will simply contain either a pasted URL or a `https://sthotelimages....blob.core.windows.net/entity-images/...` URL, indistinguishable to the frontend.

---

## 4. Backend Implementation Plan

### 4.1 New packages

```
Azure.Storage.Blobs
Azure.Storage.Queues
Azure.Identity
Microsoft.AspNetCore.RateLimiting   (built into .NET 7+, just needs registering)
```

### 4.2 Configuration — **environment variables only, never `appsettings.json`**

**Hard rule for the implementation agent:** never open, create, or edit `appsettings.json`, `appsettings.*.json`, `.env`, or any secrets/config file, ever, for any reason. All Azure-related configuration is read exclusively via `IConfiguration`/`Environment.GetEnvironmentVariable`, bound to the exact variable names below. You (the user) own populating these in your own `.env` file / OS environment / launch profile — the agent only ever _reads_ them by name in code.

ASP.NET Core's default configuration pipeline already reads environment variables and maps double-underscore (`__`) to nested config sections automatically, so `AzureStorage__AccountUrl` binds to `config["AzureStorage:AccountUrl"]` with zero extra setup — no code needs to parse a `.env` file itself (if you're loading `.env` via something like `DotNetEnv` in `Program.cs` today, that's fine and out of scope; the agent should not add, remove, or touch that mechanism either — it should only _read_ the resulting `IConfiguration` values).

**Exact variable names to put in your `.env` — fill these in yourself:**

| Variable name                        | Example value                                         | Notes                                                                                                                               |
| ------------------------------------ | ----------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------- |
| `AzureStorage__AccountUrl`           | `https://sthotelimages<suffix>.blob.core.windows.net` | From Section 2, step 2                                                                                                              |
| `AzureStorage__ContainerName`        | `entity-images`                                       | From Section 2, step 3                                                                                                              |
| `AzureStorage__QueueName`            | `image-validation-queue`                              | From Section 2, step 5                                                                                                              |
| `AzureStorage__SasExpiryMinutes`     | `15`                                                  | Optional override; code defaults to 15 if unset                                                                                     |
| `AzureStorage__MaxSizeBytes`         | `10485760`                                            | 10 MB; optional override, code defaults to this if unset                                                                            |
| `AzureStorage__MaxImagesPerRoomType` | `5`                                                   | Optional override; code defaults to 5 if unset                                                                                      |
| `AzureStorage__AllowedExtensions__0` | `.jpg`                                                | Env-var array syntax; repeat with `__1`, `__2`, `__3` for `.jpeg`, `.png`, `.webp` — optional, code has these 4 as default if unset |
| `AzureAd__TenantId`                  | `<tenantId from az account show>`                     | From Section 2, step 6                                                                                                              |
| `AzureAd__ClientId`                  | `<appId from az ad app create>`                       | From Section 2, step 6                                                                                                              |
| `AzureAd__ClientSecret`              | `<secret from az ad app credential reset>`            | From Section 2, step 6 — **never logged, never written to any file by the agent**                                                   |

The agent should read every one of these through `IConfiguration` (e.g. `config["AzureStorage:AccountUrl"]`) or a strongly-typed `IOptions<AzureStorageOptions>`/`IOptions<AzureAdOptions>` bound from those same section names — never hardcode a fallback value for `AzureAd__ClientSecret` specifically, and never write a sample/placeholder value into any settings file "to show the shape" — the variable names above are the entire contract between you and the code.

Credential selection in `Program.cs`: **confirmed** to use `ClientSecretCredential`, built purely from `AzureAd:TenantId` + `AzureAd:ClientId` + `AzureAd:ClientSecret` read via `IConfiguration` — since the backend runs locally only today. Wrap the construction in one small factory method (e.g. `IAzureCredentialFactory.Create()`) so that if the backend later moves onto an Azure compute resource, swapping to `DefaultAzureCredential` + a system-assigned managed identity is a one-line change in that single factory, with zero changes anywhere else in the codebase, and no new file-touching required.

### 4.3 New files

**`HotelManagement.BLL/Interfaces/IImageUploadService.cs`** — contract: `RequestUploadAsync`, `ConfirmUploadAsync`, `GetStatusAsync`, `AttachToEntityAsync(sessionIds, entityType, entityId)`, `ValidateAndAttachUrlsAsync(...)`.

**`HotelManagement.BLL/Services/ImageUploadService.cs`** — implements the above:

- `RequestUploadAsync(entityType, fileName, declaredContentType, declaredSizeBytes, userId, entityId?)`
  - validate extension against `AllowedExtensions`
  - validate `declaredSizeBytes` ≤ `MaxSizeBytes` (10 MB, same constant for all three entity types)
  - **if `entityType == RoomType`**: count existing `Attached` + `Confirmed`/`Pending` (not yet expired) sessions for this `entityId` (on Update) — or count how many upload slots the admin has already started filling in the current create/edit session (frontend tracks this too, see 5.2) — and reject with a clear error if it would push the total past `MaxImagesPerRoomType` (5). This mirrors the existing "Add image URL" button behavior, which the frontend should also disable once 5 images (pasted + uploaded combined) are present.
  - build blob name `{entityType}/{Guid.NewGuid()}{ext}`
  - insert `UploadSession` row, `Status = Pending`, `ExpiresAtUtc = now + SasExpiryMinutes`
  - call `BlobServiceClient.GetUserDelegationKeyAsync(start, expiry)`
  - build `BlobSasBuilder` with `BlobSasPermissions.Write | Create`, `Resource = "b"`, scoped to that exact blob name
  - return `{ sessionId, uploadUrl (blob url + sas query string), blobUrl (clean url, no query), expiresOn }`
- `ConfirmUploadAsync(sessionId, userId)`
  - verify session belongs to `userId` and `Status == Pending`
  - enqueue JSON message `{ sessionId }` to `image-validation-queue`
  - return 202-style result
- `GetStatusAsync(sessionId, userId)` → returns current `Status` (+ `RejectionReason` if rejected)
- `AttachToEntityAsync` → called by Amenity/MenuItem/RoomType services right after `SaveChangesAsync` gives the new/edited entity its Id; flips matching `Confirmed` sessions referenced in the saved URLs to `Attached` + sets `AttachedEntityId`.

**`HotelManagement.BLL/Workers/ImageValidationWorker.cs`** (`BackgroundService`)

- long-running loop, dequeues from `image-validation-queue` (long polling / short delay when empty)
- for each message: load `UploadSession`, download first 512 bytes via `BlobClient.DownloadStreamingAsync(range: 0-512)`
- magic-byte table:
  | Extension | Magic bytes (hex) |
  |---|---|
  | .jpg/.jpeg | `FF D8 FF` |
  | .png | `89 50 4E 47 0D 0A 1A 0A` |
  | .webp | `52 49 46 46 ....` + `57 45 42 50` at offset 8 |
- compare detected type to the extension in `BlobName`; mismatch → `Rejected`, delete blob, set `RejectionReason`
- also re-check actual blob size (`Properties.ContentLength`) against the same limit table (defense against a client lying about `declaredSizeBytes` at request time) → oversized → `Rejected`, delete blob
- success → `Status = Confirmed`, `ActualSizeBytes` set, `ConfirmedAtUtc = now`

**`HotelManagement.BLL/Workers/OrphanImageCleanupWorker.cs`** (`BackgroundService`, timer-based, e.g. hourly)

- finds `UploadSession` rows with `Status in (Pending, Confirmed, Rejected)` and `CreatedAtUtc < now - 24h` and `AttachedEntityId == null`
- deletes the blob (if still present) and marks row `Expired`

**`HotelManagement.API/Controllers/ImagesController.cs`**

```
POST /api/images/upload-sas      [Authorize(Roles="Admin")]  [EnableRateLimiting("image-upload")]
POST /api/images/{sessionId}/confirm  [Authorize(Roles="Admin")]
GET  /api/images/{sessionId}/status   [Authorize(Roles="Admin")]
```

All three read `userId` from `ICurrentUserService` (you already have this pattern in `AmenityService`) so a session can only be confirmed/checked by the admin who created it.

### 4.4 Changes to existing services (small, additive)

`AmenityService.CreateAmenityAsync` / `UpdateAmenityAsync`, `MenuItemService.CreateMenuItemAsync` / `UpdateMenuItemAsync`, `RoomTypeService.CreateRoomTypeAsync` / `UpdateRoomTypeAsync`:

- After the existing `SaveChangesAsync()` call succeeds, add one line: `await _imageUploadService.AttachToEntityAsync(urls-from-dto, UploadEntityType.X, entity.Id);`
- Inside `AttachToEntityAsync`: for each URL in the incoming list, if the URL's host equals your storage account host, look up the matching `UploadSession` by `BlobUrl`; require `Status == Confirmed` (or already `Attached` to this same `entity.Id`, to tolerate re-saving an unchanged image on Update) and `UploadedByUserId == currentUser`; anything else (Confirmed-but-owned-by-someone-else, Pending, Rejected, or "no matching session at all" for a storage-domain URL) → throw a validation error and **do not** save the entity. Plain external URLs (non-storage-domain) skip this check entirely and behave exactly as they do today.
- **RoomType-specific**: before attaching, also re-validate `dto.ImageUrls.Count <= MaxImagesPerRoomType` (5) server-side — this is the authoritative check; the frontend cap (5.2 below) is only a UX convenience and must not be trusted alone. If the incoming list already exceeds 5 (e.g. a stale client), reject with a clear validation error before touching any session or the entity row.
- This keeps `[Required][Url]` on `CreateUpdateAmenityDTO.ImageUrl` etc. completely untouched — a confirmed blob URL is, syntactically, just a URL, so existing validation attributes keep working unchanged.

### 4.5 Rate limiting

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("image-upload", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.FindFirst("sub")?.Value ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(5)
            }));
});
```

Applied only to `upload-sas` (the expensive/abusable one — SAS issuance); `confirm`/`status` are cheap reads/enqueues and can share a looser or no limit.

### 4.6 Error handling / validation summary

- Extension not in allow-list → 400 at `upload-sas` time (fastest possible rejection)
- Declared size over limit → 400 at `upload-sas` time
- Actual magic bytes don't match extension → blob deleted, session `Rejected`, surfaced to client via polling
- Actual size over limit (client lied) → blob deleted, session `Rejected`
- SAS expires before upload completes → client gets 403 from Blob Storage directly → UI shows "upload expired, please retry" and calls `upload-sas` again
- Entity Save references a URL whose session isn't `Confirmed`/owned by the caller → 400, entity not saved, no partial state
- All of the above are additive — no existing exception paths in `AmenityService`/`MenuItemService`/`RoomTypeService` change.

---

## 5. Frontend Implementation Plan (Angular)

### 5.1 New package

```
npm install @azure/storage-blob
```

### 5.2 New shared component: `ImageUploadOrUrlComponent`

Implements `ControlValueAccessor` so it drops into the existing reactive forms exactly like a plain `<input>` does today.

- Two-tab UI: **Paste URL** (existing plain `<input>` behavior, unchanged) / **Upload Image** (new)
- Upload tab flow:
  1. `<input type="file" accept=".jpg,.jpeg,.png,.webp">`
  2. client-side pre-check: extension + `file.size` against the same limits as backend (fast UX feedback; **not** authoritative — backend re-checks everything)
  3. `POST /api/images/upload-sas` → get `{ sessionId, uploadUrl, blobUrl }`
  4. `new BlockBlobClient(uploadUrl).uploadData(file, { blockSize: 4*1024*1024, concurrency: 4, onProgress: (p) => ... })` — this is the SDK's chunked block upload, giving you real progress and per-block retry (the practical equivalent of "resumable" within a single page session)
  5. on upload success → `POST /api/images/{sessionId}/confirm`
  6. poll `GET /api/images/{sessionId}/status` every ~1s (cap ~30 attempts) until `Confirmed`/`Rejected`
  7. `Confirmed` → set the control's value to `blobUrl`, show thumbnail preview, show a "Remove" option (clears value, lets admin pick again)
  8. `Rejected` → show the `rejectionReason`, let admin retry
- While `Pending`/uploading: disable the modal's Save button (bind to a `uploading` signal exposed by this component) so nothing can be submitted mid-upload.

### 5.3 CRUD modal changes (`crud-modal.component`)

Add two new `field.type` cases alongside the existing `select`/`textarea`/`toggle`/`keyValueList`/`imageUrlList`:

- `imageUrlOrUpload` → single `<app-image-upload-or-url [formControl]="getControl(field.key)">` (replaces the plain `<input>` currently used for `ImageUrl` on Amenity/MenuItem forms)
- `imageUrlOrUploadList` → same `formArrayName` structure you already have for `imageUrlList`, but each row renders `<app-image-upload-or-url [formControl]="$any(urlCtrl)">` instead of the plain input — the "Add image URL"/"Remove" buttons and FormArray mechanics stay exactly as they are today.
- **RoomType cap of 5**: disable/hide the "Add image URL" button once `getImageUrlArray('imageUrls').length >= 5` (mirrors the existing `addImageUrl`/`removeImageUrl` methods already in the modal component — just add a length guard). This is a UX convenience only; Section 4.4 has the authoritative server-side check, since a stale/tampered client request must never be able to exceed 5 on save.

### 5.4 Per-entity config changes

- `amenity-management` field config: `ImageUrl` field type `text` → `imageUrlOrUpload`
- `menu-item-management` field config: same change
- `room-type-management` field config: `ImageUrls` field type `imageUrlList` → `imageUrlOrUploadList`

No changes needed to `generic-crud` component itself (the table/card rendering already just reads `imageUrl`/`imageUrls` off the returned DTOs, which are unchanged) — only the modal's field renderer and the three page-level configs change.

### 5.5 Environment config

Add the images API base path to `environment.ts`/`environment.prod.ts` alongside your existing API config (assumed already present given the rest of the app).

---

## 6. Testing & Rollout Checklist

- [ ] Unit tests: `ImageUploadService` (extension/size validation, SAS scoping to single blob, attach-ownership checks)
- [ ] Unit tests: magic-byte detector against real sample files for each allowed extension, plus a deliberately mismatched file (e.g. `.exe` renamed to `.jpg`) to confirm rejection
- [ ] Integration test: full happy path — request SAS → upload via SDK against Azurite (Azure Storage emulator, for local/dev testing without touching real Azure) → confirm → poll to Confirmed → create entity → verify DB row has the blob URL and session is `Attached`
- [ ] Integration test: reject path — upload a text file renamed `.png` → confirm → verify session `Rejected` and blob deleted
- [ ] Integration test: someone tries to attach another user's `Confirmed`-but-not-theirs session → 400
- [ ] Load/abuse test: hammer `upload-sas` past the rate limit → verify 429
- [ ] Regression test: existing "paste external URL" flow for all three entities still works completely unchanged (no storage-domain checks triggered)
- [ ] Manual QA: slow network upload cancel/retry, SAS-expiry-mid-upload retry, RoomType multi-image add/remove mixing pasted + uploaded images in the same list
- [ ] Confirm orphan cleanup worker actually deletes an abandoned pending upload after the configured window

---

