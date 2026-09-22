# Cut-list body configuration step

This is a resumable atomic operation for creating one `bodyNN` configuration and its matching `KeepBody_bodyNN` feature from a cut-list item. It is deliberately one item per invocation because SolidWorks can stall under long batches of configuration-specific rebuilds.

See `SKILL.md` for usage and `SolidWorksConsole/Reference/CutListBodyConfigurationWorkflow.md` for the validated workflow rationale.
