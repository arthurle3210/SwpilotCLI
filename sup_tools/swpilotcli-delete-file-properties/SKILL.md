---
name: swpilotcli-delete-file-properties
description: Delete every document-level (file-level) custom property from the active SolidWorks document while leaving every configuration-level custom property untouched. Defaults to a read-only dry run; use apply to make changes. Does not save the document.
---

# Delete file-level custom properties

## Prerequisites

- A SolidWorks part, assembly, or drawing is active.

## What this skill does

- Uses the document-level custom-property manager only (`CustomPropertyManager[""]`).
- Lists the file-level properties in `dry-run` mode.
- In `apply` mode, deletes each file-level property and verifies that none remain.
- Never opens, reads, changes, or deletes configuration-specific custom-property managers.
- Does not save the document.

## Quick start

```powershell
# Preview only
dotnet run --project ./sup_tools/swpilotcli-delete-file-properties/scripts/DeleteFileLevelCustomProperties/DeleteFileLevelCustomProperties.csproj -- dry-run

# Delete every file-level property
dotnet run --project ./sup_tools/swpilotcli-delete-file-properties/scripts/DeleteFileLevelCustomProperties/DeleteFileLevelCustomProperties.csproj -- apply
```
