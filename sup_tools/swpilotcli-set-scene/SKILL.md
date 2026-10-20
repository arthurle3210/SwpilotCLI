---
name: swpilotcli-set-scene
description: Set a SolidWorks scene on the active document using a scene name from the installed scene library or a .p2s file path. Supports current or all configurations, optional verification, and optional saving. Defaults to current configuration, verification enabled, and no save. Use as an atomic building block when changing model backgrounds or render scenes; caller controls whether changes are saved.
---

# swpilotcli-set-scene

Set the scene for the active SolidWorks document.

## Usage

```bash
dotnet run --project ./sup_tools/swpilotcli-set-scene/scripts/SetScene/SetScene.csproj -- <scene-name-or-path> [current|all] [verify|noverify] [nosave|save]
```

## Parameters

| Position | Parameter | Values | Default |
|---|---|---|---|
| 1 | Scene | Installed scene name, filename, or full `.p2s` path | Required |
| 2 | Configuration scope | `current` or `all` | `current` |
| 3 | Verification | `verify` or `noverify` | `verify` |
| 4 | Save behavior | `nosave` or `save` | `nosave` |

Scene-name lookup is case-insensitive and searches the installed SolidWorks scene library recursively. The `.p2s` extension is optional when using a scene name.

## Examples

```bash
# Apply to the current configuration, verify, and leave unsaved
dotnet run --project ./sup_tools/swpilotcli-set-scene/scripts/SetScene/SetScene.csproj -- "dark grey background"

# Apply to every configuration, verify each assignment, and save
dotnet run --project ./sup_tools/swpilotcli-set-scene/scripts/SetScene/SetScene.csproj -- "dark grey background" all verify save

# Apply a specific scene file without verification or saving
dotnet run --project ./sup_tools/swpilotcli-set-scene/scripts/SetScene/SetScene.csproj -- "C:\Scenes\custom.p2s" current noverify nosave
```

## Notes

- Operates on the active part, assembly, or drawing.
- Restores the initially active configuration after `all` scope.
- `nosave` intentionally leaves persistence to the caller for tool composition.
- SolidWorks RealView graphics must be available for the scene to display fully.

