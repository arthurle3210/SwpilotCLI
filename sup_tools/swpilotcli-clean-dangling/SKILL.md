---
name: swpilotcli-clean-dangling
description: Delete only dangling dimensions, balloons, centerlines, and center marks from the active SolidWorks drawing. Reports candidate and deletion counts by annotation type and does not save. Use as an atomic drawing-cleanup step after reviewing a drawing or before release.
---

# Clean Dangling Drawing Annotations

Deletes only annotations that SolidWorks reports as dangling from the active `.slddrw` drawing.

## What this tool removes

- Display dimensions
- Balloons
- Centerlines
- Center marks

All other annotation types are ignored. Valid annotations are never selected for deletion.

## Usage

Preview candidates without changing the drawing:

```powershell
dotnet run --project ./sup_tools/swpilotcli-clean-dangling/scripts/CleanDangling/CleanDangling.csproj -- --dry-run
```

Delete the candidates:

```powershell
dotnet run --project ./sup_tools/swpilotcli-clean-dangling/scripts/CleanDangling/CleanDangling.csproj -- --apply
```

## Notes

- Requires an active SolidWorks drawing (`.slddrw`).
- Does not save the drawing; the caller controls whether to save or discard the changes.
- A balloon is recognized as a Note with a balloon style other than `None`.
