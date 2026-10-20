# API Reference

## SolidWorks APIs used

| API | Purpose |
| --- | --- |
| `IModelDoc2::GetConfigurationNames` | Retrieves every configuration name. |
| `IModelDoc2::ShowConfiguration2` | Activates each configuration. |
| `IFeatureManager::GetFeatures(false)` | Retrieves top-level and absorbed features. |
| `IFeature::GetSpecificFeature2` | Identifies sketch features. |
| `IFeature::Select2` | Selects each sketch for processing. |
| `IModelDoc2::BlankSketch` | Hides the selected sketch. |

## Output

The tool writes:

- configurations processed versus total;
- sketch features hidden;
- any configuration activation or sketch-selection failures.

It exits with an error if a configuration cannot be activated after retrying or a sketch cannot be selected.
