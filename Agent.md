# HotelManagement — Project Context

Stack: ASP.NET Core (C#) backend, Angular admin portal, PostgreSQL (local dev), JWT bearer auth.

## Current image flow (as of this feature's start)
- Admin CRUD forms let an admin paste an external image URL (Google or any host).
- Backend stores that URL verbatim: `Amenity.ImageUrl` (string?), `MenuItem.ImageUrl` (string?),
  `RoomType.ImageUrls` (List<string>?).
- Frontend renders images by binding the DTO's `imageUrl`/`imageUrls` directly to `<img src>`.
  No proxying, no signed URLs, no transformation.
- Generic CRUD modal (`crud-modal.component`) drives forms from a `FieldDef[]` config per entity;
  RoomType's image list uses field type `imageUrlList` backed by a FormArray named `urls`.

## In-progress feature: direct-to-Azure-Blob upload
See `agent/azure-blob-image-upload-plan.md` for the full design. Key constraints:
- Never proxy file bytes through the backend — client uploads directly to Blob Storage
  using a backend-issued User Delegation SAS scoped to one blob.
- Validate file type by magic bytes post-upload, not by extension alone.
- All uploads and validation are async (Storage Queue + BackgroundService).
- No changes to existing DTO/entity shapes — blob URLs are stored in the same
  ImageUrl/ImageUrls fields as pasted URLs.
- Existing "paste a URL" flow must keep working unmodified.

## Secrets & config — hard rule, no exceptions
NEVER open, create, edit, or write to `appsettings.json`, `appsettings.*.json`,
`.env`, or any other secrets/config file, for any reason, at any point in this
feature's implementation. All Azure Storage / Azure AD configuration is read
exclusively via `IConfiguration` using the exact environment variable names
listed in `agent/azure-blob-image-upload-plan.md` Section 4.2 (e.g.
`AzureStorage__AccountUrl`, `AzureAd__ClientId`, `AzureAd__ClientSecret`, etc.).
The user fills these in their own `.env` outside of any agent session. If a
value you need isn't in that table, stop and ask the user to add it — do not
invent a new config file or hardcode a fallback secret to "make it run."
