---
name: swpilotcli-custom-properties
description: Read custom properties from the active SolidWorks document at file level and across every configuration. Lists raw and resolved values, with an optional case-insensitive property-name filter. Use to inspect BOM metadata, descriptions, part numbers, materials, revisions, or other custom properties without modifying the model.
---

# SolidWorks Custom Properties

Inspect custom properties in the active SolidWorks document without changing it.

## Usage

```powershell
dotnet run --project ./sup_tools/swpilotcli-custom-properties/scripts/ListCustomProperties/ListCustomProperties.csproj
dotnet run --project ./sup_tools/swpilotcli-custom-properties/scripts/ListCustomProperties/ListCustomProperties.csproj -- Description
```

With no argument, list every property. With one argument, show only exact property-name matches, case-insensitively. The output distinguishes document-level properties from each configuration and includes raw and resolved values.
