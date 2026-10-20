---
name: swpilotcli-cutlist-body-step
description: Process one cut-list item into a one-body configuration in the active SolidWorks multibody part. Safely resumes partial work: creates bodyNN and KeepBody_bodyNN only when missing, preserves all design-intent Delete/Keep Body features, uses staged configuration waits, verifies EditRebuild3, and confirms one retained solid body. Run dry-run first; use apply <index> for one item at a time.
---

# Cut-list body configuration step

Run a non-mutating preflight:

```powershell
dotnet run --project ./sup_tools/swpilotcli-cutlist-body-step/scripts/CutListBodyConfigStep/CutListBodyConfigStep.csproj -- dry-run
```

Process one cut-list item:

```powershell
dotnet run --project ./sup_tools/swpilotcli-cutlist-body-step/scripts/CutListBodyConfigStep/CutListBodyConfigStep.csproj -- apply 1 3 2
```

Arguments are `apply <index> [preSwitchDelaySeconds=3] [postUpdateDelaySeconds=2]`.

Start each apply invocation from `Default`. The tool intentionally leaves the target `bodyNN` configuration active after validating it; reactivate Default before processing the next item. The tool never saves the model.

## Preconditions

- Active document is a part with a current cut list.
- Default is active.
- Every pre-existing Delete/Keep Body feature is unsuppressed in Default and remains part of the design intent.

