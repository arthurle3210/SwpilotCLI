---
name: swpilotcli-hide-all-sketches
description: Hide every sketch feature in every configuration of the active SolidWorks part or assembly. Retries configurations that do not activate on the first attempt, restores the original configuration, and reports totals and failures. Use as an atomic building block for cleanup workflows. Does not save the document.
---

# Hide All Sketches

## Prerequisites

- SolidWorks is running with an active part or assembly document.

## What this Skill does

- Traverses every configuration in the active document.
- Finds top-level and absorbed sketch features.
- Hides each sketch using SolidWorks' `BlankSketch` operation.
- Retries configurations that fail to activate initially.
- Restores the original active configuration and reports the result.

## When to use

Use as a building block when all sketches should be hidden throughout a configurable model before review, export, or presentation.

## Quick start

```powershell
dotnet run --project ./sup_tools/swpilotcli-hide-all-sketches/scripts/HideAllSketches/HideAllSketches.csproj
```

## Notes

- The active document is modified but not saved; the caller decides whether to save or discard the change.
- On large, heavily configured models, processing can take longer than a typical single-configuration operation.
