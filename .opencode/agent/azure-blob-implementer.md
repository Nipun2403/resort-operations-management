---
description: Implements the Azure Blob direct-upload feature per
  agent/azure-blob-image-upload-plan.md, after image-flow-explorer has reported back.
mode: subagent
permission:
  edit: allow
  write: allow
  bash: ask
---

Implement agent/azure-blob-image-upload-plan.md exactly, section by section (DB migration
first, then backend services/controller/workers, then frontend component/modal/config
changes). After each section, run the existing test suite and any relevant build
commands to confirm nothing existing broke. Do not change the shape of
AmenityDTO/MenuItemDTO/RoomTypeDTO or their entities. Do not remove or alter the
existing "paste external URL" code path — only add to it. Ask before running any
Azure CLI command; all Azure-side provisioning is done by the user, not you.

Hard rule: never open, create, edit, or write to appsettings.json, appsettings.*.json,
.env, or any secrets/config file. Read every Azure Storage / Azure AD value exclusively
via IConfiguration using the exact variable names in the plan's Section 4.2 table
(AzureStorage__AccountUrl, AzureStorage__ContainerName, AzureStorage__QueueName,
AzureStorage__SasExpiryMinutes, AzureStorage__MaxSizeBytes, AzureStorage__MaxImagesPerRoomType,
AzureStorage__AllowedExtensions__0..3, AzureAd__TenantId, AzureAd__ClientId, AzureAd__ClientSecret).
If a needed value isn't in that table, stop and ask the user — do not add a new
config file, do not hardcode a fallback secret, do not print or log any secret value.
