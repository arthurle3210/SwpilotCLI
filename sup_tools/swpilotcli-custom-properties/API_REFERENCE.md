# API reference

- `IModelDocExtension.CustomPropertyManager[configurationName]` accesses document-level or configuration-specific properties. An empty name selects document-level properties.
- `IModelDoc2.GetConfigurationNames()` enumerates all configurations.
- `ICustomPropertyManager.GetNames()` enumerates property names.
- `ICustomPropertyManager.Get6()` reads raw and resolved values without modifying the document.
