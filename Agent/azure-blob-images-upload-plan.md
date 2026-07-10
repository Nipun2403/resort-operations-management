# Direct-to-Azure-Blob Image Upload — Implementation Plan

Target entities: **Amenity**, **MenuItem**, **RoomType**
Stack: ASP.NET Core (C#) + PostgreSQL (local) backend, Angular admin portal, JWT bearer auth.
Azure resources: `nsdeply00` (storage, `NS-Deply` RG), `images` container, `image-validation-queue`.

---

## 0. Executive Summary & Key Architecture Decisions — ALL CONFIRMED

| Decision                            | Choice                                                                                                                                                         | Why                                                                                                                                                                    |
| ----------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Upload path                         | Client uploads **directly to Blob Storage** using a short-lived SAS issued by the backend. Backend never touches file bytes on the way in.                     | Matches your rule; avoids proxy cost/latency; standard Azure pattern.                                                                                                  |
| SAS type                            | **Account-key Service SAS** (`StorageSharedKeyCredential`)                                                                                                     | Your account lacks Graph API / Entra ID admin privileges for app registration + RBAC; pragmatic fallback. Account key scoped to the storage account, stored only in `appsettings.Local.json` (`.gitignore`). Swap to Entra-only when you deploy to an Azure compute resource with managed identity. |
| Storage layout                      | **CONFIRMED: one container** (`images`), blobs named `{entityType}/{guid}{ext}`                                                                                | Simpler CORS/lifecycle config than 3 containers; entityType prefix keeps things organized/filterable.                                                                  |
| Read access to images               | **CONFIRMED: public read (`blob` level)** — the stored blob URL is a plain HTTPS URL, identical in shape to a pasted Google image link                         | Keeps the frontend **completely unchanged** — `ImageUrl`/`ImageUrls` stay plain strings, no SAS-refresh logic needed on read.                                          |
| DB schema impact on existing tables | **None.** `Amenity.ImageUrl`, `MenuItem.ImageUrl`, `RoomType.ImageUrls` keep their exact current type/shape.                                                   | Satisfies "front end continues to load images just like now" and "no existing feature breaks."                                                                         |
| New table                           | `UploadSession` — tracks every issued upload slot (pending/confirmed/rejected/attached/expired). Ownership key = email (`UploadedByEmail`).                    | Needed for magic-byte validation, orphan cleanup, ownership/rate tracking, and to stop a confirmed-but-unattached blob from being reused by someone else.              |
| Async validation                    | **CONFIRMED: Option A** — ASP.NET Core `BackgroundService` consuming an **Azure Storage Queue**, inside the existing API app (no separate Azure Functions app) | You have no Functions infra yet; a queue + hosted worker inside the existing deployable is simpler ops for a "ground up" build.                                        |
| Backend identity/credential         | **CONFIRMED: Storage account key** (`StorageSharedKeyCredential`), since your account lacks Entra ID admin privileges for app registration + RBAC                | Account key stored only in `appsettings.Local.json` (`.gitignore`). Zero-day swap to `DefaultAzureCredential` + managed identity when/if you deploy to an Azure compute resource — just change the factory to return `new DefaultAzureCredential()`. |
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
  │  - rate-limits per user (email-based key)
  │  - validates extension + size against config limits
  │  - creates UploadSession (Status=Pending)
  │  - signs a Service SAS using StorageSharedKeyCredential, scoped to exactly one blob, write-only, ~15 min expiry
  │  - returns { sessionId, uploadUrl, blobUrl, expiresOn }
  ▲
  │  2. PUT directly to Blob Storage using @azure/storage-blob (block upload, progress, retry)
  ▼
Azure Blob Storage (container: images, public read)
  │
  │  3. POST /api/images/{sessionId}/confirm   (client calls after upload finishes)
  ▼
ASP.NET Core Backend
  │  - enqueues message to Azure Storage Queue "image-validation-queue" (name from config)
  │  - returns 202 Accepted
  │
  ▼
BackgroundService (in-process worker, dequeues async)
  │  - downloads first 512 bytes of the blob
  │  - checks magic bytes vs. declared extension/content-type
  │  - checks actual blob size vs. configured max
  │  - Status → Confirmed (valid) or Rejected (deletes blob, marks row)
  ▲
  │  4. GET /api/images/{sessionId}/status   (client polls adaptively: 1s x10, then 2s x20, cap 60)
  ▼
Admin Portal
  - on Confirmed: sets the form control's value = blobUrl, shows thumbnail, enables Save
  - on Rejected: shows error, lets admin retry
  - on Save (Create/Update entity): backend re-validates that any storage-domain URL
    in the payload maps to a Confirmed UploadSession owned by this admin, then flips
    it to Attached and stamps EntityType/EntityId
```

Orphan cleanup: a second `BackgroundService` (timer-based, runs hourly) deletes blobs/rows. Safe ordering: mark `Expired` in DB first, then delete blob. Separate windows: Pending=1h, Confirmed/Rejected=24h. Also consider Azure Storage lifecycle management rule as a belt-and-suspenders backstop.

---

## 2. Azure Setup — Run These Yourself (CLI)

Your existing resources: storage account `nsdeply00` in resource group `NS-Deply`. Only steps 2 onward need to be run — the storage account already exists.

Data-plane commands use either `--auth-mode login` (container, queue) or auto-query the account key (CORS). Management-plane commands (`az ad *`, `az role assignment *`, `az storage account *`) always use your `az login` credentials — no `--auth-mode` needed.

```bash
# 1a. Check the container's current public access level
az storage container show \
  --account-name nsdeply00 \
  --name images \
  --auth-mode login \
  --query "properties.publicAccess"

# 1b. If it returns "container" or "off", recreate with "blob" level
#     (az storage container set-permission only supports --auth-mode key,
#      so delete+recreate is simpler and the container is empty):
az storage container delete --account-name nsdeply00 --name images --auth-mode login
az storage container create --account-name nsdeply00 --name images --public-access blob --auth-mode login

# 2. CORS — required so the Angular app (browser) can PUT directly to blob storage.
#    NOTE: az storage cors add does NOT support --auth-mode. Pass only --account-name;
#    the CLI auto-queries the account key using your logged-in identity.
#    Using "*" for both headers to avoid shell escaping issues with space-separated lists.
az storage cors add \
  --account-name nsdeply00 \
  --services b \
  --methods GET PUT OPTIONS HEAD \
  --origins "http://localhost:4200" \
  --allowed-headers "*" \
  --exposed-headers "*" \
  --max-age 3600

# 3. Queue for async post-upload validation
az storage queue create \
  --name image-validation-queue \
  --account-name nsdeply00 \
  --auth-mode login

# 4. Get the storage account key for StorageSharedKeyCredential.
#    NOT a security concern: this account key is stored only in appsettings.Local.json
#    (already in .gitignore). The key never leaves your local machine.
#    When/if you later deploy to Azure, switch to managed identity + DefaultAzureCredential.
az storage account keys list --account-name nsdeply00 --resource-group NS-Deply --query "[0].value" -o tsv
#    Copy the output key and paste it into appsettings.Local.json as AzureStorage:AccountKey
#
#    NOTE: Steps 5 (RBAC) and 6 (harden shared key access) are SKIPPED because
#    we're using the account key directly — disabling shared key access would
#    break our auth mechanism. These steps apply only for the Entra-only approach.

# 5. (Optional, recommended) Lifecycle management backstop: auto-delete blobs
#    under images older than 2 days that were never "tiered"/touched —
#    a safety net in case the app-level cleanup worker ever fails. Configure via
#    Portal → Storage Account → Data management → Lifecycle management, rule
#    scoped to prefix "images/", condition "days since creation > 2",
#    action "delete blob". (CLI equivalent uses a JSON policy file — say the
#    word if you want the exact JSON for this.)
```

Create `Backend/HotelManagement.API/appsettings.Local.json` (already in `.gitignore`) with your Azure config values:

```json
{
  "AzureStorage": {
    "AccountUrl": "https://nsdeply00.blob.core.windows.net",
    "ContainerName": "images",
    "QueueName": "image-validation-queue",
    "AccountKey": "xBXIXdA9AYkxlrpdqIfUw/lavRoxcR0xsnkDiB/wy6n6so59Fd2jSy5A6LsCtYjq17rnWu2IlZmm+ASt5q5u1g==",
    "SasExpiryMinutes": 15,
    "MaxSizeBytes": 10485760,
    "MaxImagesPerRoomType": 5,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".webp"]
  }
}
```

A `Program.cs` change is already applied: `builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)`. This gives precedence: `appsettings.json` < `appsettings.Local.json` < environment variables (env vars still win for CI/CD).

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
    public string UploadedByEmail { get; set; } = string.Empty;  // from ICurrentUserService.GetUserEmail()
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ConfirmedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public string? RejectionReason { get; set; }
}
```

Migration: `dotnet ef migrations add AddUploadSession -p HotelManagement.DAL -s HotelManagement.API` (adjust project names to your actual solution layout). No changes needed to `Amenity`, `MenuItem`, or `RoomType` tables or their DTOs — `ImageUrl`/`ImageUrls` will simply contain either a pasted URL or a `https://nsdeply00.blob.core.windows.net/images/...` URL, indistinguishable to the frontend.

---

## 4. Backend Implementation Plan

### 4.1 New packages

```
Azure.Storage.Blobs
Azure.Storage.Queues
Azure.Identity
```

### 4.2 Configuration — **`appsettings.Local.json` (already in `.gitignore`)**

Configuration is read exclusively via `IConfiguration`. The code never references `appsettings.Local.json` directly — it only reads strongly-typed options bound from `IConfiguration` sections.

The implementation agent must:

- Add `AzureStorageOptions` POCO class in `HotelManagement.API` (or BLL) with `IOptions<T>` binding
- Register `services.Configure<AzureStorageOptions>(builder.Configuration.GetSection("AzureStorage"))`
- Read every value through `IOptions<AzureStorageOptions>.Value`
- **Never open, create, or edit `appsettings.json`, `appsettings.*.json`, `.env`, or any secrets/config file for any reason**

**Config section layout (matching the JSON above):**

| Section path                        | .NET type  | Notes                                                                                             |
| ----------------------------------- | ---------- | ------------------------------------------------------------------------------------------------- |
| `AzureStorage:AccountUrl`           | `string`   | `https://nsdeply00.blob.core.windows.net`                                                         |
| `AzureStorage:ContainerName`        | `string`   | `images`                                                                                          |
| `AzureStorage:QueueName`            | `string`   | `image-validation-queue`                                                                            |
| `AzureStorage:AccountKey`           | `string`   | Storage account key from `az storage account keys list` — **never logged, never written to any file by the implementation agent** |
| `AzureStorage:SasExpiryMinutes`     | `int`      | Default `15` if unset                                                                             |
| `AzureStorage:MaxSizeBytes`         | `long`     | Default `10485760` (10 MB) if unset                                                               |
| `AzureStorage:MaxImagesPerRoomType` | `int`      | Default `5` if unset                                                                              |
| `AzureStorage:AllowedExtensions`    | `string[]` | Default `[".jpg", ".jpeg", ".png", ".webp"]` if unset                                             |

**Credential factory**: Create `IAzureCredentialFactory` (interface) + `AzureCredentialFactory` (implementation) in `HotelManagement.API/Utilities/`. Registered as singleton in DI. The factory builds `new StorageSharedKeyCredential(accountName, accountKey)` from `IOptions<AzureStorageOptions>` (parsed from `AccountUrl` for the account name). Swap note: if the backend later moves onto an Azure compute resource with managed identity, swap the factory to return `new DefaultAzureCredential()` — one change in one file, zero changes anywhere else in the codebase.

**Program.cs registration** (already done):

```csharp
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
```

### 4.3 New files

**`HotelManagement.BLL/Interfaces/IImageUploadService.cs`** — contract: `RequestUploadAsync`, `ConfirmUploadAsync`, `GetStatusAsync`, `AttachToEntityAsync(sessionIds, entityType, entityId)`, `ValidateAndAttachUrlsAsync(...)`.

**`HotelManagement.BLL/Services/ImageUploadService.cs`** — implements the above:

- `RequestUploadAsync(entityType, fileName, declaredContentType, declaredSizeBytes, userEmail, entityId?)`
  - validate extension against `AllowedExtensions`
  - validate `declaredSizeBytes` ≤ `MaxSizeBytes` (10 MB, same constant for all three entity types)
  - **if `entityType == RoomType`**: count existing `Attached` + `Confirmed`/`Pending` (not yet expired) sessions for this `entityId` (on Update) — or count how many upload slots the admin has already started filling in the current create/edit session (frontend tracks this too, see 5.2) — and reject with a clear error if it would push the total past `MaxImagesPerRoomType` (5). This mirrors the existing "Add image URL" button behavior, which the frontend should also disable once 5 images (pasted + uploaded combined) are present.
  - build blob name `{entityType}/{Guid.NewGuid()}{ext}`
  - insert `UploadSession` row, `Status = Pending`, `ExpiresAtUtc = now + SasExpiryMinutes` — **entity save + session attachment must happen in a single transaction** (see 4.4)
  - call `BlobServiceClient.GetUserDelegationKeyAsync(start, expiry)`
  - build `BlobSasBuilder` with `BlobSasPermissions.Write | Create`, `Resource = "b"`, scoped to that exact blob name
  - return `{ sessionId, uploadUrl (blob url + sas query string), blobUrl (clean url, no query), expiresOn }`
- `ConfirmUploadAsync(sessionId, userEmail)`
  - verify session belongs to `userEmail` and `Status == Pending`
  - enqueue JSON message `{ sessionId }` to queue name from config
  - return 202-style result
- `GetStatusAsync(sessionId, userEmail)` → returns current `Status` (+ `RejectionReason` if rejected)
- `AttachToEntityAsync` → called by Amenity/MenuItem/RoomType services right after `SaveChangesAsync` gives the new/edited entity its Id; flips matching `Confirmed` sessions referenced in the saved URLs to `Attached` + sets `AttachedEntityId`.

**`HotelManagement.BLL/Workers/ImageValidationWorker.cs`** (`BackgroundService`)

- long-running loop, dequeues from the configured Azure Storage Queue
- for each message: load `UploadSession`, download first 512 bytes via `BlobClient.DownloadStreamingAsync(range: 0-512)`
- magic-byte table:
  | Extension | Magic bytes (hex) |
  |---|---|
  | .jpg/.jpeg | `FF D8 FF` |
  | .png | `89 50 4E 47 0D 0A 1A 0A` |
  | .webp | `52 49 46 46` @ offset 0 **AND** `57 45 42 50` @ offset 8 (both required) |
- compare detected type to the extension in `BlobName`; mismatch → `Rejected`, delete blob, set `RejectionReason`
- also re-check actual blob size (`Properties.ContentLength`) against the same limit table (defense against a client lying about `declaredSizeBytes` at request time) → oversized → `Rejected`, delete blob
- success → `Status = Confirmed`, `ActualSizeBytes` set, `ConfirmedAtUtc = now`
- queue poll: use `QueueClient.ReceiveMessagesAsync` with visibility timeout 30s; when empty, `Task.Delay(2s)` between polls

**`HotelManagement.BLL/Workers/OrphanImageCleanupWorker.cs`** (`BackgroundService`, timer-based, runs hourly)

- Two separate cleanup windows:
  - **Pending sessions** older than 1 hour: mark `Expired` in DB first, then delete blob from storage. Shorter window because an abandoned Pending upload is just wasted storage space.
  - **Confirmed/Rejected sessions** older than 24 hours with `AttachedEntityId == null`: mark `Expired` in DB first, then delete blob. Longer window because Confirmed images might be in the middle of a multi-step edit flow.
- **Safe ordering**: always mark `Expired` in the database first, then delete the blob. If the blob delete fails (transient), the next run retries it. If the DB update fails, no orphan blob was deleted — consistent.
- Use `IDbContextFactory<ApplicationDbContext>` for short-lived context scopes (worker runs outside an HTTP request scope).

**`HotelManagement.API/Controllers/ImagesController.cs`**

```
POST /api/images/upload-sas      [Authorize(Roles="Admin")]  [EnableRateLimiting("image-upload")]
POST /api/images/{sessionId}/confirm  [Authorize(Roles="Admin")]
GET  /api/images/{sessionId}/status   [Authorize(Roles="Admin")]
```

All three read `userEmail` from `ICurrentUserService.GetUserEmail()` (email is the ownership key since `ICurrentUserService` has no user ID method). A session can only be confirmed/checked by the admin who created it.

**`HotelManagement.API/Utilities/IAzureCredentialFactory.cs`** + **`HotelManagement.API/Utilities/AzureCredentialFactory.cs`**: registered as singleton, constructs `StorageSharedKeyCredential` from `IOptions<AzureStorageOptions>`.

### 4.4 Changes to existing services (small, additive)

`AmenityService.CreateAmenityAsync` / `UpdateAmenityAsync`, `MenuItemService.CreateMenuItemAsync` / `UpdateMenuItemAsync`, `RoomTypeService.CreateRoomTypeAsync` / `UpdateRoomTypeAsync`:

- **Transaction wrapping**: entity save + session attachment must be atomic. Use `BeginTransactionAsync` before save, `CommitAsync` after both succeed, `RollbackAsync` on failure. If attachment fails, the entity save must roll back too — no orphan entities with unattached image references.

- After `SaveChangesAsync()` succeeds (inside the transaction), call:

  ```csharp
  await _imageUploadService.AttachToEntityAsync(urls-from-dto, UploadEntityType.X, entity.Id);
  ```

- Inside `AttachToEntityAsync`: for each URL in the incoming list:
  - If the URL's host equals your storage account host → look up matching `UploadSession` by `BlobUrl`
  - Require `Status == Confirmed` (or already `Attached` to this same `entity.Id`, to tolerate re-saving an unchanged image on Update) AND `UploadedByEmail == currentUserEmail`
  - Anything else (Confirmed-but-owned-by-someone-else, Pending, Rejected, or "no matching session at all" for a storage-domain URL) → throw a validation error and **do not** save the entity
  - Plain external URLs (non-storage-domain) skip this check entirely and behave exactly as they do today

- **RoomType-specific**:
  - Handle `List<string>?` — DTO's `ImageUrls` may be null (existing entity has no images). Treat null as empty list for cap checking.
  - Before attaching, re-validate `dto.ImageUrls.Count <= MaxImagesPerRoomType` (5) server-side — this is the authoritative check; the frontend cap (5.3 below) is only a UX convenience and must not be trusted alone. If the incoming list already exceeds 5 (e.g. a stale client), reject with a clear validation error before touching any session or the entity row.

- This keeps `[Required][Url]` on `CreateUpdateAmenityDTO.ImageUrl` etc. completely untouched — a confirmed blob URL is, syntactically, just a URL, so existing validation attributes keep working unchanged.

**New DI registrations in `Program.cs`:**

```csharp
// Image upload infrastructure
builder.Services.AddScoped<IImageUploadService, ImageUploadService>();
builder.Services.AddSingleton<IAzureCredentialFactory, AzureCredentialFactory>();
builder.Services.AddHostedService<ImageValidationWorker>();
builder.Services.AddHostedService<OrphanImageCleanupWorker>();
```

Also add `BlobServiceClient` and `QueueServiceClient` as scoped services (or register them transiently via the factory). These need `TokenCredential` injected via DI — the BLL service receives `TokenCredential` in its constructor (from `IAzureCredentialFactory`), not the factory itself.

### 4.5 Rate limiting

```csharp
// In Program.cs, alongside existing GlobalPolicy:
options.AddPolicy("image-upload", httpContext =>
    RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.User.FindFirst(ClaimTypes.Name)?.Value ?? "anonymous",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 20,
            Window = TimeSpan.FromMinutes(5)
        }));
```

Applied only to `upload-sas` (the expensive/abusable one — SAS issuance); `confirm`/`status` are cheap reads/enqueues and get the existing `GlobalPolicy` (100/10s) since `app.MapControllers().RequireRateLimiting("GlobalPolicy")` applies globally. The `image-upload` policy on the specific endpoint overrides the global default for that one route.

Partition key uses `ClaimTypes.Name` (which maps to the JWT `sub`/email claim — same value as `ICurrentUserService.GetUserEmail()`) rather than `"sub"` claim directly.

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

**Imports**: `MatTabsModule` needed for the tab UI (Paste URL / Upload Image). This is an Angular Material module — add to the component's `imports` array.

- Two-tab UI: **Paste URL** (existing plain `<input>` behavior, unchanged) / **Upload Image** (new)
- Upload tab flow:
  1. `<input type="file" accept=".jpg,.jpeg,.png,.webp">`
  2. client-side pre-check: extension + `file.size` against the same limits as backend (fast UX feedback; **not** authoritative — backend re-checks everything)
  3. `POST /api/images/upload-sas` → get `{ sessionId, uploadUrl, blobUrl }`
  4. `new BlockBlobClient(uploadUrl).uploadData(file, { blockSize: 4*1024*1024, concurrency: 4, onProgress: (p) => ... })` — this is the SDK's chunked block upload, giving you real progress and per-block retry (the practical equivalent of "resumable" within a single page session)
  5. on upload success → `POST /api/images/{sessionId}/confirm`
  6. adaptive polling `GET /api/images/{sessionId}/status`: **1s intervals for first 10 attempts, 2s intervals for attempts 11–30, cap at 60 total** (~90s max before timeout)
  7. `Confirmed` → set the control's value to `blobUrl`, show thumbnail preview, show a "Remove" option (clears value, lets admin pick again)
  8. `Rejected` → show the `rejectionReason`, let admin retry
- While `Pending`/uploading: disable the modal's Save button (expose `isUploading` boolean) so nothing can be submitted mid-upload.

### 5.3 CRUD modal changes (`crud-modal.component`)

Add two new `field.type` cases alongside the existing `select`/`textarea`/`toggle`/`keyValueList`/`imageUrlList`:

- `imageUrlOrUpload` → single `<app-image-upload-or-url [formControl]="getControl(field.key)">` (replaces the plain `<input>` currently used for `ImageUrl` on Amenity/MenuItem forms)

- `imageUrlOrUploadList` → same `formArrayName` structure you already have for `imageUrlList`, but each row renders `<app-image-upload-or-url [formControl]="$any(urlCtrl)">` instead of the plain input — the "Add image URL"/"Remove" buttons and FormArray mechanics stay exactly as they are today.

- **`isSaving` signal wiring**: The modal's Save button must be disabled when either `isSaving` is true (prevents double-submit) or any image is still uploading. Wire the existing `isSaving` signal from `GenericCrudComponent` to the Save button's `[disabled]` binding. Also add a local `isUploading` signal that combines child component upload states.

- **RoomType cap of 5**: disable/hide the "Add image URL" button once `getImageUrlArray('imageUrls').length >= 5` (mirrors the existing `addImageUrl`/`removeImageUrl` methods already in the modal component — just add a length guard). This is a UX convenience only; Section 4.4 has the authoritative server-side check, since a stale/tampered client request must never be able to exceed 5 on save.

**`crud-config.model.ts`**: Extend the `FormFieldDef.type` union type to include `'imageUrlOrUpload'` and `'imageUrlOrUploadList'`.

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

