# Azure Blob Storage Integration — Image Upload & Serving

**Date:** 2026-07-04
**Status:** Draft
**Author:** Superpowers (Brainstorming → Design)

---

## Overview

Replace current manual URL-based image storage (admin types image links) with Azure Blob Storage. Admin uploads files via admin portal, backend stores them in Azure Blob Storage, and frontend serves them directly from public blob URLs — maintaining the same `src` binding pattern.

---

## Azure Setup

- Storage account: Standard general-purpose v2, LRS (e.g., `shotelmgmt`)
- Single container: `images`
- Access level: **Blob (anonymous read-only)**
- CORS: allow frontend origin (e.g., `http://localhost:4200`, production URL)
- CDN: optional, can add later if needed
- Connection string stored in `appsettings.json`

---

## Backend Changes

### NuGet Package
- `Azure.Storage.Blobs` (latest stable)

### Configuration (`appsettings.json`)

```json
{
  "AzureStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...",
    "ImageContainerName": "images",
    "MaxFileSizeMB": 5
  }
}
```

### New Service Layer

| Interface | Implementation | Location |
|---|---|---|
| `IAzureBlobStorageService` | `AzureBlobStorageService` | `HotelManagement.BLL/Services/` |

Methods:
- `UploadAsync(Stream fileStream, string fileName, string contentType)` → `string (blobUrl)`
- `DeleteAsync(string blobUrl)` → `void`
- `GetContainerUrl()` → `string`

Blob naming convention: `{entityType}/{guid}-{sanitizedFilename}`
- RoomType: `roomtypes/{guid}-filename.jpg`
- Amenity: `amenities/{guid}-filename.jpg`
- MenuItem: `menuitems/{guid}-filename.jpg`

### DTO Changes

Replace URL strings with file upload fields:

| Current DTO | New Field | Type |
|---|---|---|
| `CreateRoomTypeDTO.ImageUrls` | `ImageFiles` | `List<IFormFile>` |
| `UpdateRoomTypeDTO.ImageUrls` | `ImageFiles` + `ExistingImageUrls` | `List<IFormFile>?` + `List<string>?` |
| `CreateUpdateAmenityDTO.ImageUrl` | `ImageFile` | `IFormFile` |
| `CreateMenuItemDTO.ImageUrl` | `ImageFile` | `IFormFile` |
| `MenuItemDTO.ImageUrl` | `ImageFile` (for create) | `IFormFile` |

Keep `MenuItemDTO` with `ImageUrl: string` for responses.

### Controller Changes

- Switch from `[FromBody]` to `[FromForm]` for create/update actions
- Upload flow per file:
  1. Validate file extension (`.jpg|.jpeg|.png|.gif|.webp`)
  2. Validate file size (≤ `MaxFileSizeMB`)
  3. Call `AzureBlobStorageService.UploadAsync()`
  4. Store returned blob URL in DB
- Update flow: compare `ExistingImageUrls` with DB to detect removals → `DeleteAsync()` removed blobs
- Delete entity flow: load entity, delete all associated blobs, then remove DB record

### Database Schema

**No changes.** URLs are persisted as they are today:
- `RoomType.ImageUrls` → JSON text column
- `Amenity.ImageUrl` → text column
- `MenuItem.ImageUrl` → text column

Existing URLs remain untouched (start-fresh approach — DB starts empty).

---

## Frontend Changes

### Admin Management Components

| Component | Change |
|---|---|
| `room-type-management` | Replace URL text input with `<input type="file" multiple>` + preview thumbnails + reorder/remove buttons |
| `amenities-management` | Replace URL input with single `<input type="file">` + preview |
| `menu-management` | Replace URL input with single `<input type="file">` + preview |
| `crud-modal` (shared) | Add new form field types: `image-upload` (single), `image-upload-list` (multiple) |

Form handling changes:
- New field type `FormFieldDef.type`: `'image-upload' | 'image-upload-list'` alongside existing `'url' | 'imageUrlList'`
- Show local preview via `URL.createObjectURL(file)` before upload
- On save: build `FormData` object, post via `HttpClient`

### Admin API Services

Switch from JSON body to `FormData` for create/update:

```typescript
// Before
create(data: CreateRoomTypeDTO): Observable<RoomType> {
  return this.http.post<RoomType>(url, data);
}

// After
create(formData: FormData): Observable<RoomType> {
  return this.http.post<RoomType>(url, formData);
}
```

### Public / User / Kitchen Components

**Zero changes.** All image display components bind via `<img [src]="imageUrl">`. Blob URLs are consumed identically to manually-entered URLs.

Affected but unchanged:
- `HomeComponent` — `getFirstImage()`
- `RoomCatalogueComponent` — `getFirstImage()`
- `RoomDetailComponent` — `galleryImages = computed(...)`
- `AvailabilityComponent` — `getFirstImage()`
- `ExperiencesComponent` — `getAmenityImage()`
- `MenuComponent`
- `KitchenMenuItemsComponent` — `getItemImage()`
- `MenuGridComponent` — `getImageUrl()`

---

## Data Flow

### Upload Flow
```
Admin picks file(s) in Angular form
  → FormData assembled with file + metadata fields
  → POST /api/v1/room-types (multipart/form-data, JWT auth)
  → Backend controller receives IFormFile[]
  → AzureBlobStorageService.UploadAsync() per file
  → Azure SDK writes blob to images/{entityType}/{guid}-{filename}
  → Blob URL returned
  → Backend saves URL to DB
  → Response returns entity with blob URL(s)
  → Frontend displays via <img [src]="blobUrl">
```

### Display Flow (unchanged)
```
Entity loaded from API → ImageUrls/ImageUrl contains blob URLs
→ Template binds directly: <img [src]="entity.imageUrls[0]">
→ If null/empty: fallback to placeholder image
```

---

## Image Deletion Strategy

| Scenario | Action |
|---|---|
| Image removed from edit form | Frontend sends `ExistingImageUrls` without that URL. Backend detects removal → `DeleteAsync()` on removed blob URL. |
| Entity deleted | Backend loads entity, calls `DeleteAsync()` on all associated blob URLs, then removes DB record. |
| Image replaced (same entity) | Old blob deleted in update flow (see above). New blob uploaded. |
| Orphan blobs | Tolerated. No automatic cleanup to avoid accidental deletion. Manual cleanup via Azure Portal if needed. |

---

## Security & Validation

| Concern | Mitigation |
|---|---|
| File type | Server-side check: only `.jpg/.jpeg/.png/.gif/.webp` allowed |
| File size | Max 5 MB (configurable via `MaxFileSizeMB`) |
| Path traversal | GUID-prefixed blob names prevent overwriting existing blobs |
| Overwrite | GUID-prefixed names ensure uniqueness |
| Unauthorized upload | All upload endpoints have `[Authorize(Roles = "Admin")]` (existing) |
| Container security | Public read only — no anonymous write/delete |

---

## Files to Modify

### Backend
1. `HotelManagement.API/HotelManagement.API.csproj` — add `Azure.Storage.Blobs` package
2. `HotelManagement.API/Program.cs` — register `IAzureBlobStorageService`, bind config
3. `HotelManagement.API/appsettings.json` — add `AzureStorage` section
4. `HotelManagement.BLL/Services/AzureBlobStorageService.cs` — new file
5. `HotelManagement.BLL/Interfaces/IAzureBlobStorageService.cs` — new file
6. `HotelManagement.BLL/DTOs/` — update DTOs (replace URL fields with IFormFile)
7. `HotelManagement.API/Controllers/RoomTypesController.cs` — update create/update actions
8. `HotelManagement.API/Controllers/AmenitiesController.cs` — update create/update actions
9. `HotelManagement.API/Controllers/MenuItemsController.cs` — update create/update actions

### Frontend
10. `Frontend/src/app/shared/components/generic-crud/crud-modal/crud-modal.component.ts` — add file upload field types
11. `Frontend/src/app/features/admin/services/room-type-api.service.ts` — switch to FormData
12. `Frontend/src/app/features/admin/services/amenity-api.service.ts` — switch to FormData
13. `Frontend/src/app/features/admin/services/menu-item-api.service.ts` — switch to FormData
14. `Frontend/src/app/features/admin/pages/management/room-type-management.component.ts` — file input UI
15. `Frontend/src/app/features/admin/pages/management/amenities-management.component.ts` — file input UI
16. `Frontend/src/app/features/admin/pages/management/menu-management.component.ts` — file input UI
17. `Frontend/src/app/shared/models/crud-config.model.ts` — add new field types

---

## Non-Goals (Out of Scope)

- Image resizing / thumbnail generation
- CDN setup
- Migration of existing image URLs (DB starts empty)
- Progressive image loading / blur-up placeholders
- Multi-region blob storage replication
