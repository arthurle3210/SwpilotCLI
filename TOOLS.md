# SwpilotCLI Tools Registry

This document indexes all available tools in the SwpilotCLI ecosystem, including built-in (main) tools and user-defined (sup) tools.

## Main Tools

Built-in tools maintained as part of SwpilotCLI. These are always available and ready to use without additional setup.

### swpilotcli-active-doc-info

**Description**: Get information about the currently open SolidWorks document (part, assembly, or drawing). Use when you need to check document properties, file path, document type, or current state without modifying it.

**Path**: `./main_tools/swpilotcli-active-doc-info/`

**Skill Definition**: `./main_tools/swpilotcli-active-doc-info/SKILL.md`

**API Reference**: `./main_tools/swpilotcli-active-doc-info/API_REFERENCE.md`

**Learn more**: See `./main_tools/swpilotcli-active-doc-info/README.md` for development and migration info.

---

### swpilotcli-eye-screenshot

**Description**: YOUR EYES. Use this tool whenever you need to see the screen â€” do NOT say you cannot see or take screenshots. Run this first, then use the Read tool on ./temp/screenshot/screenshot.png to view the image. Use when: user asks what is on screen, you want to verify code execution results, or you need visual context before deciding next steps.

**Path**: `./main_tools/swpilotcli-eye-screenshot/`

**Skill Definition**: `./main_tools/swpilotcli-eye-screenshot/SKILL.md`

**API Reference**: `./main_tools/swpilotcli-eye-screenshot/API_REFERENCE.md`

**Learn more**: See `./main_tools/swpilotcli-eye-screenshot/README.md` for development and migration info.

---


## Sup Tools

User-defined tools that extend SwpilotCLI functionality. Place your custom tools here in subdirectories following the same structure as Main Tools.

### swpilotcli-ActionGroup-replace-notes

**Description**: [ActionGroup] Batch replace text inside Note annotations across all SolidWorks drawings (.slddrw) in a specified folder (root only)." For each drawing, opens it silently, finds all Notes containing the search text, replaces every occurrence, saves a copy with a custom suffix to a specified output folder, then closes the original without saving. Reports which files were changed and how many notes were replaced per file. Use when you need to find-and-replace note text across multiple drawings in one shot.

**Path**: `./sup_tools/swpilotcli-ActionGroup-replace-notes/`

### swpilotcli-batch-xt-to-step

**Description**: Batch convert Parasolid `.x_t` and `.xt` files in a specified folder to STEP `.step` files in the same folder. Scans the folder root only, creates same-name `.step` outputs, suppresses template-selection prompts by forcing default part templates, and closes each temporary import document after export. Use when you need one-shot XT to STEP conversion for a local folder.

**Path**: `./sup_tools/swpilotcli-batch-xt-to-step/`

### swpilotcli-check-dangling-dims

**Description**: Scan all dimensions in the active SolidWorks drawing and report which ones are dangling (unattached). Lists every dimension with its type, value, and name, marking dangling ones with [!]. Outputs a summary count at the end. Use before running a repair tool, for QC checks before release, or whenever you need to know if a drawing has dangling annotations.

**Path**: `./sup_tools/swpilotcli-check-dangling-dims/`

### swpilotcli-clean-dangling

**Description**: Delete only dangling dimensions, balloons, centerlines, and center marks from the active SolidWorks drawing. Reports candidate and deletion counts by annotation type and does not save. Use as an atomic drawing-cleanup step after reviewing a drawing or before release.

**Path**: `./sup_tools/swpilotcli-clean-dangling/`

### swpilotcli-close-doc

**Description**: Close SolidWorks document(s). Supports three modes via argument: no argument or "nosave" closes the active document without saving; "save" silently saves then closes the active document (if file has never been saved, forces close without saving); "all" closes every open document without saving. Use when the user wants to close the current file, close and save, or close everything.

**Path**: `./sup_tools/swpilotcli-close-doc/`

### swpilotcli-custom-properties

**Description**: Read custom properties from the active SolidWorks document at file level and across every configuration. Lists raw and resolved values, with an optional case-insensitive property-name filter. Use to inspect BOM metadata, descriptions, part numbers, materials, revisions, or other custom properties without modifying the model.

**Path**: `./sup_tools/swpilotcli-custom-properties/`

### swpilotcli-cutlist-body-step

**Description**: Process one cut-list item into a one-body configuration in the active SolidWorks multibody part. Safely resumes partial work: creates bodyNN and KeepBody_bodyNN only when missing, preserves all design-intent Delete/Keep Body features, uses staged configuration waits, verifies EditRebuild3, and confirms one retained solid body. Run dry-run first; use apply <index> for one item at a time.

**Path**: `./sup_tools/swpilotcli-cutlist-body-step/`

### swpilotcli-delete-file-properties

**Description**: Delete every document-level (file-level) custom property from the active SolidWorks document while leaving every configuration-level custom property untouched. Defaults to a read-only dry run; use apply to make changes. Does not save the document.

**Path**: `./sup_tools/swpilotcli-delete-file-properties/`

### swpilotcli-delete-orphan-sketches

**Description**: Delete all orphaned sketches in the active SolidWorks part â€” sketch features not absorbed by any extrusion, cut, or other parent feature. Iterates the feature tree, removes every top-level `ProfileFeature`, then rebuilds. Use for cleanup after discarded geometry experiments, or to remove leftover construction sketches before saving.

**Path**: `./sup_tools/swpilotcli-delete-orphan-sketches/`

### swpilotcli-diagnose-ic-rigidity

**Description**: Diagnose whether candidate sketch points belong to the same rigid body in the active SolidWorks sketch. Compares point movement and pairwise distances between two nearby dimension states. Use before computing IC when you need to verify that your tracked points are valid on the same body.

**Path**: `./sup_tools/swpilotcli-diagnose-ic-rigidity/`

### swpilotcli-export-assembly-parts-to-xt

**Description**: Batch export all unique parts (.sldprt) in the active SolidWorks assembly to Parasolid (.x_t) format. Outputs all files to an "xt/" subfolder next to the assembly. Use when you need to convert an entire assembly's parts to XT in one operation.

**Path**: `./sup_tools/swpilotcli-export-assembly-parts-to-xt/`

### swpilotcli-export-part-to-xt

**Description**: Export a single SolidWorks part (.sldprt) to Parasolid (.x_t) format. Uses the active document if no path is given. Accepts optional part path and output directory arguments. Use for single-part export or as the per-item step in LLM-orchestrated batch workflows.

**Path**: `./sup_tools/swpilotcli-export-part-to-xt/`

### swpilotcli-feature-chamfer-circular-hole

**Description**: Apply an angle-distance chamfer to circular hole edge(s) in the active SolidWorks part. Targets full circular edges by diameter and can optionally limit selection to the nearest matching edge to a supplied world coordinate. Use for chamfering drilled/cut hole openings such as 0.5 C x 45 deg center-hole chamfers.

**Path**: `./sup_tools/swpilotcli-feature-chamfer-circular-hole/`

### swpilotcli-feature-extrude-boss

**Description**: Extrude the active sketch outward to create a Boss-Extrude solid feature. Must be called while a sketch is active in editing mode. Use after sketch-on-face + sketch-draw to build up solid geometry.

**Path**: `./sup_tools/swpilotcli-feature-extrude-boss/`

### swpilotcli-feature-extrude-cut

**Description**: Cut-extrude the active sketch through the solid to remove material. Supports a fixed depth in mm or through-all. Must be called while a sketch is active in editing mode. Use after sketch-on-face + sketch-draw to create holes, pockets, or slots.

**Path**: `./sup_tools/swpilotcli-feature-extrude-cut/`

### swpilotcli-feature-hole-wizard-straight-tap

**Description**: Create a HoleWizard ISO Straight Tap feature on a specified face at one or more positions. Selects the face programmatically by world coordinates â€” no manual pre-selection required. Supports any metric size (M3â€“M12), blind depth in mm, or through-all. Use when the user asks to add tapped holes, M-size threaded holes, or straight taps to a face.

**Path**: `./sup_tools/swpilotcli-feature-hole-wizard-straight-tap/`

### swpilotcli-hide-all-sketches

**Description**: Hide every sketch feature in every configuration of the active SolidWorks part or assembly. Retries configurations that do not activate on the first attempt, restores the original configuration, and reports totals and failures. Use as an atomic building block for cleanup workflows. Does not save the document.

**Path**: `./sup_tools/swpilotcli-hide-all-sketches/`

### swpilotcli-inspect-drawing

**Description**: Full attribute diagnostic tool for the active SolidWorks drawing. Dumps every annotation's complete properties: for dimensions â€” name, value, type, text parts (prefix/suffix/callout), tolerance type and values, precision, arrow style, layer, color; for notes â€” text content, leader attach point, arrow count and style, angle, layer, color. Also lists all visible geometry vertices with both model and sheet coordinates. Use when debugging dangling repairs, auditing annotation attributes before migration, or understanding drawing geometry for automation development.

**Path**: `./sup_tools/swpilotcli-inspect-drawing/`

### swpilotcli-inspect-sketch-setup

**Description**: Inspect the active SolidWorks sketch setup. Reports the active document, selected objects, features, sketch points, and sketch segments for the current sketch. Use when validating manually drawn IC construction geometry or debugging sketch topology before automation.

**Path**: `./sup_tools/swpilotcli-inspect-sketch-setup/`

### swpilotcli-list-assembly-parts

**Description**: List all unique part files (.sldprt) referenced by the active SolidWorks assembly. Outputs a JSON array of full file paths. Use this to preview parts before batch processing, or to get part paths for further automation.

**Path**: `./sup_tools/swpilotcli-list-assembly-parts/`

### swpilotcli-list-faces

**Description**: List every face of every solid body in the active SolidWorks part, printing each face's surface normal and bounding-box centroid in millimetres. Use as a pre-selection scan before `sketch-on-face` or `feature-hole-wizard-straight-tap`, or when you need to identify a target face by coordinates without manually probing the model.

**Path**: `./sup_tools/swpilotcli-list-faces/`

### swpilotcli-list-features

**Description**: List features in the active SolidWorks document's Feature Tree, printing each feature's type and name. Walks from `FirstFeature()` via `GetNextFeature()`. Accepts an optional integer argument to limit output count (default: all features). Use to locate named reference planes, inspect feature order, or find features by name before operations like `move-rollback-bar`, `sketch-on-face`, or deletion.

**Path**: `./sup_tools/swpilotcli-list-features/`

### swpilotcli-move-rollback-bar

**Description**: Move the SolidWorks Rollback Bar to a specified feature position. Use this to suppress features by moving the rollback bar before or after any feature in the Feature Tree.

**Path**: `./sup_tools/swpilotcli-move-rollback-bar/`

### swpilotcli-new-part

**Description**: Create a new SolidWorks Part document using the user's configured default Part template. Reads `swDefaultTemplatePart` from user preferences and calls `NewDocument`. Use when the user asks to create a new part, start modeling from scratch, or needs a fresh `.sldprt` to host a subsequent sketch/feature workflow.

**Path**: `./sup_tools/swpilotcli-new-part/`

### swpilotcli-open-doc

**Description**: Open a SolidWorks document (.sldprt, .sldasm, .slddrw) by file path and activate it. Requires a file path argument. Use when the user wants to open a specific file in SolidWorks.

**Path**: `./sup_tools/swpilotcli-open-doc/`

### swpilotcli-plot-ic-bisectors-at-68

**Description**: Draw the two perpendicular bisectors and IC intersection for the validated 68 to 68.01 mm example. Reads precise source geometry from è‰åœ–1 and draws the bisectors into è‰åœ–5. Use when explaining how the 68 mm IC is constructed geometrically.

**Path**: `./sup_tools/swpilotcli-plot-ic-bisectors-at-68/`

### swpilotcli-plot-ic-points

**Description**: Plot the current validated IC coordinate list into the active SolidWorks sketch as fixed sketch points. Use when you already have the approved IC coordinate sequence for this mechanism and want to visualize the path directly in the sketch.

**Path**: `./sup_tools/swpilotcli-plot-ic-points/`

### swpilotcli-plot-p0-p1-on-sketch

**Description**: Plot the validated P0, P1, midpoint, and short P0-P1 chord used in the 68 to 68.01 mm IC explanation. Draws into è‰åœ–5. Use when you need to show exactly which two positions define the perpendicular bisector construction.

**Path**: `./sup_tools/swpilotcli-plot-p0-p1-on-sketch/`

### swpilotcli-read-drawing

**Description**: Engineering drawing / hand-sketch interpretation protocol. MUST be executed before any feature modeling whenever the user provides an engineering drawing or hand-drawn sketch image. Enforces multi-view cross-reference and isometric priority to prevent geometric misinterpretation.

**Path**: `./sup_tools/swpilotcli-read-drawing/`

### swpilotcli-replace-note-text

**Description**: Find and replace text inside all Note annotations in the active SolidWorks drawing. Requires two arguments: the text to find and the replacement text. Scans every sheet and view, replaces all matching notes, and reports how many were changed. Use as an atomic building block in LLM-orchestrated batch workflows (e.g. open-doc â†’ replace-note-text â†’ save-drawing-as-copy â†’ close-doc). Does NOT save the file â€” caller is responsible for saving or discarding.

**Path**: `./sup_tools/swpilotcli-replace-note-text/`

### swpilotcli-save-drawing-as

**Description**: Save the active SolidWorks drawing to a different format. Exports to the same directory as the drawing file with the same filename and a new extension. Supported formats: pdf, png, jpg, tif, dwg, dxf, ai, psd, edrw, html, slddrw. Default format is pdf if no argument is given. Optionally accepts a second argument to override the output file path. Use when you need to export or convert the current drawing to another format.

**Path**: `./sup_tools/swpilotcli-save-drawing-as/`

### swpilotcli-save-drawing-as-copy

**Description**: Save a SolidWorks drawing (.slddrw) as a copy to a new file path, without changing the original document's path or closing it. Requires two arguments: source drawing path and output path. Use when you need to duplicate a drawing under a new name or location while keeping the original open and unmodified. Useful as a building block in LLM-orchestrated batch rename/copy workflows.

**Path**: `./sup_tools/swpilotcli-save-drawing-as-copy/`

### swpilotcli-set-dimension

**Description**: Set the value of a driving dimension in the active SolidWorks document by its full name. Requires two arguments, the dimension full name (e.g. `D1@Sketch1` or `D1@Boss-Extrude1`) and the new value in millimetres. Calls Parameter().SetSystemValue3 then rebuilds. Use as an atomic building block whenever you need to change a sketch or feature dimension, regardless of what that dimension drives (hole radius, extrude depth, rectangle length, etc.).

**Path**: `./sup_tools/swpilotcli-set-dimension/`

### swpilotcli-set-scene

**Description**: Set a SolidWorks scene on the active document using a scene name from the installed scene library or a .p2s file path. Supports current or all configurations, optional verification, and optional saving. Defaults to current configuration, verification enabled, and no save. Use as an atomic building block when changing model backgrounds or render scenes; caller controls whether changes are saved.

**Path**: `./sup_tools/swpilotcli-set-scene/`

### swpilotcli-set-view

**Description**: Set view orientation of the active SolidWorks document. Supported views: front, back, left, right, top, bottom, iso (default), dimetric, trimetric. Calls ShowNamedView2 + ViewZoomtofit2.

**Path**: `./sup_tools/swpilotcli-set-view/`

### swpilotcli-sketch-draw

**Description**: Draw sketch geometry in the active SolidWorks sketch from a JSON SketchPlan. Supports line, circle, arc, ellipse. Auto-infers H/V on lines and COINCIDENT on shared endpoints. Applies anchor, explicit relations, and dimensions. Does NOT exit the sketch. Call swpilotcli-sketch-exit only when explicitly asked to commit or exit the sketch. Use when the user asks to draw a sketch, reproduce a shape, or add geometry to an open sketch.

**Path**: `./sup_tools/swpilotcli-sketch-draw/`

### swpilotcli-sketch-exit

**Description**: Exit the active SolidWorks sketch and report its constraint status (Fully Defined / Under-Constrained / Over-Constrained). Call this after swpilotcli-sketch-draw to commit geometry and check if the sketch is fully constrained.

**Path**: `./sup_tools/swpilotcli-sketch-exit/`

### swpilotcli-sketch-on-face

**Description**: Open a new sketch on a specified face or plane. Accepts either a named plane (e.g. å‰åŸºæº–é¢) or world face coordinates in mm (e.g. 25,10,-20). Enters sketch editing mode and stays there â€” follow up with sketch-draw to draw geometry, then extrude-boss or extrude-cut.

**Path**: `./sup_tools/swpilotcli-sketch-on-face/`

### swpilotcli-sweep-ic-by-dimension

**Description**: Sweep the validated driving dimension in the active SolidWorks sketch and print the computed IC coordinates for each step. Uses the current Yeti 6-bar point tracking method validated in this session. Use when you need the IC path as coordinates before plotting points.

**Path**: `./sup_tools/swpilotcli-sweep-ic-by-dimension/`

### swpilotcli-undo-one-step

**Description**: Execute one SolidWorks undo action on the active document (equivalent to pressing Ctrl+Z once). Use when you need to roll back the most recent operation quickly.

**Path**: `./sup_tools/swpilotcli-undo-one-step/`


### Template

To create a new sup tool:

1. Create directory: ./sup_tools/your-tool-name/
2. Add required files:
   ``
   your-tool-name/
   â”œâ”€â”€ SKILL.md              (required, with YAML frontmatter)
   â”œâ”€â”€ API_REFERENCE.md      (recommended)
   â”œâ”€â”€ README.md             (recommended)
   â””â”€â”€ scripts/
       â””â”€â”€ YourTool/         (your project directory)
           â””â”€â”€ YourTool.csproj
   ``
3. Follow the same format as Main Tools for consistency
4. Run sync-tools.ps1 to update this registry

---

## Migration to Agent Skills

All tools in this registry are designed to be directly compatible with Claude Agent Skills.

### Step 1: Prepare your tools

Ensure your tools follow the official structure:
``
tool-name/
â”œâ”€â”€ SKILL.md
â”œâ”€â”€ API_REFERENCE.md (optional)
â”œâ”€â”€ README.md (optional)
â””â”€â”€ scripts/
    â””â”€â”€ YourProject/
        â””â”€â”€ YourProject.csproj
``

### Step 2: Copy to ~/.claude/skills/

``ash
# Copy main tool
cp -r ./main_tools/swpilotcli-active-doc-info ~/.claude/skills/

# Copy sup tool
cp -r ./sup_tools/your-tool-name ~/.claude/skills/
``

### Step 3: Update SKILL.md if needed

If migrating from SwpilotCLI to standalone Agent Skills:
- Update dotnet run paths (may need adjustment in different environment)
- Update prerequisites if needed
- No changes to YAML frontmatter required

---

## Tool Development Guidelines

### Naming Convention

- Use lowercase with hyphens: 	ool-name
- For SwpilotCLI tools: prefix with swpilotcli-: swpilotcli-feature-name
- Keep names under 30 characters for clarity

### SKILL.md Structure

``yaml
---
name: tool-name
description: What it does. When to use it. (max 1024 chars)
---

# Title

## Prerequisites

## What this Skill does

## When to use

## Quick start

## Examples

## Reference

## Notes
``

### Code Implementation (C#)

- Use .NET 8+ for compatibility
- Console project structure with Program.cs
- Return structured output (JSON preferred)
- Exit codes: 0 = success, 1+ = error
- Keep execution time under 5 seconds

### Testing

Test locally before adding to registry:
``ash
dotnet run --project ./scripts/YourTool/YourTool.csproj
``

---

## FAQ

**Q: How do I add a tool to the registry?**
A: Create the directory structure under sup_tools/, follow the format, then run sync-tools.ps1.

**Q: Can I modify Main Tools?**
A: Main Tools are maintained by the SwpilotCLI team. For modifications, fork and create a custom Sup Tool.

**Q: Do tools require .NET 8?**
A: Currently yes, for consistency and C# compatibility. Python/Bash wrappers can use other runtimes if needed.

**Q: How are tools discovered by Claude?**
A: Claude reads this TOOLS.md file and can access any tool directory via bash. Specific tools are triggered based on user requests.

---

## Last Updated

- **Generated**: 2026-09-22 13:48:35
- **Tools**: 2 Main + 40 Sup tools
