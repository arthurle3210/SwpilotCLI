# One-Body Configurations from a Multibody Cut List

## Objective

Given an active multibody SolidWorks part that has a valid weldment/cut-list structure:

- preserve `Default` as the full-model configuration;
- create one `bodyNN` configuration per non-empty cut-list item;
- retain one representative body in each `bodyNN` configuration;
- use a **Body-Delete/Keep** feature in **Keep Bodies** mode for each retained body;
- keep the root-body removal feature active in all configurations.

The validated example had 84 solid bodies represented by 39 cut-list items, so the intended output was `Default` plus `body01` through `body39`.

## Preconditions

1. The active document is a `.sldprt`.
2. The cut list has been updated and contains the intended BOM items.
3. Every pre-existing **Delete/Keep Body** feature in the design tree is part of the model's design intent and must be **unsuppressed in every configuration**. A descriptive name such as `delete_root_solid` is useful, but the workflow must not assume there is only one such feature or alter any of them.
4. `Default` is active before collecting representative bodies.
5. Target configuration names `body01`...`bodyNN` do not collide with unrelated configuration names.

## Cut-List Discovery

Use the `SolidBodyFolder` and its direct `CutListFolder` children. For every non-empty cut-list folder:

- `IBodyFolder.GetBodyCount()` is the BOM quantity;
- `IBodyFolder.GetBodies()` returns the actual `IBody2` objects;
- `GetBodies()[0]` is the representative body for that cut-list item;
- only cut-list folders beneath `SolidBodyFolder` are considered, avoiding duplicate tree references.

The unique-item count is the count of non-empty cut-list folders, not the raw number of solid bodies.

## Feature and Configuration Mapping

For cut-list item `N`:

1. Create configuration `bodyNN`, if absent.
2. While Default contains the complete cut list, select its representative `IBody2`.
3. Create a Body-Delete/Keep feature using `IFeatureManager.InsertDeleteBody2(true)`.
   `true` selects **Keep Bodies** mode.
4. Name the feature `KeepBody_bodyNN`.
5. Suppress the feature in all configurations.
6. Activate `bodyNN`.
7. Wait for the configuration change to settle.
8. Unsuppress `KeepBody_bodyNN` in the active configuration only.
9. Rebuild and wait again before proceeding to the next configuration.

The desired final state is:

| Configuration | Matching KeepBody feature |
| --- | --- |
| `Default` | Every `KeepBody_bodyNN` suppressed |
| `body01` | Only `KeepBody_body01` unsuppressed |
| `body02` | Only `KeepBody_body02` unsuppressed |
| `bodyNN` | Only `KeepBody_bodyNN` unsuppressed |

## Reliable Timing Sequence

The stable sequence tested in this session was:

```text
activate bodyNN
wait 3 seconds
unsuppress KeepBody_bodyNN in this active configuration
rebuild
wait 2 seconds
```

Applying many configuration-specific suppressions in a single long-running external COM process caused the SolidWorks client to stall. Running one configuration update per helper invocation was consistently reliable.

The future tool should make delays configurable, with a conservative default of **3 seconds before** the suppression update and **2 seconds after** it. The caller can reduce them after validating performance on a representative part.

## Rebuild Confirmation

Two useful SolidWorks API calls are available:

- `IModelDoc2.EditRebuild3()` returns `true` when all features needing a rebuild in the active configuration were rebuilt.
- `IModelDoc2.ForceRebuild3(false)` returns `true` when all features in the active configuration rebuilt, but is significantly heavier and was more likely to stall the external helper in this workflow.

Use `EditRebuild3()` as the normal completion signal and treat a `false` return as a failed row. Retain the short waits because configuration activation can settle asynchronously around the COM calls.

## Failed Approaches and Lessons

1. **One monolithic run**: SolidWorks often stopped responding after several configuration changes. Use resumable, per-row work instead.
2. **Activating an already-active configuration**: `ShowConfiguration2` can return `false` even when the requested configuration is already active. Check the active configuration name rather than treating that return value alone as failure.
3. **Named-configuration suppression arrays**: direct `swSpecifyConfiguration` calls were unreliable for this document when used in bulk. The validated approach is: activate the target configuration, then use `swThisConfiguration`.
4. **Returning automatically to Default**: after a one-body configuration, SolidWorks sometimes exposed only one cut-list item through COM even when a configuration switch had been requested. Collect new representative bodies only from a confirmed Default state.
5. **Existing root-body feature**: suppressing it revealed an unwanted root solid and changed the count from 84/39 to 85/40. It must stay unsuppressed in all configurations.
6. **No duplicate features**: before creating `KeepBody_bodyNN`, check by name and resume if it already exists.

## Verification Requirements

At minimum, verify:

1. `Default` contains the original total body count and unique cut-list count.
2. All requested `bodyNN` configurations exist.
3. All requested `KeepBody_bodyNN` features exist.
4. In Default, every KeepBody feature is suppressed and `delete_root_solid` is unsuppressed.
5. In each `bodyNN`, the corresponding feature is unsuppressed and the configuration has exactly one retained representative body.

When a verification or configuration update stalls, do not continue blindly. Stop the helper, inspect the current configuration and feature state, then resume from the next unverified item.

## Atomic Tool Design

This should be a single-purpose reusable tool: create one-body configurations from the active part cut list. The tool should:

- support a dry-run/default diagnostic mode;
- preflight the active document, cut-list count, body count, configuration names, and root-body feature state;
- use resumable behavior so already-created features/configurations are detected and validated rather than duplicated;
- expose `preSwitchDelaySeconds` and `postUpdateDelaySeconds` (defaults 3 and 2);
- use `EditRebuild3()` success/failure as a row-level validation;
- report every configuration created, resumed, verified, skipped, or failed;
- never save automatically; saving remains the caller's decision.

The operation is mid-difficulty because it combines cut-list topology, configuration-specific suppression, COM timing, feature creation, and recovery from partial completion.
