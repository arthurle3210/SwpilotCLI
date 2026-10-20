# SwpilotCLI Tools Registry Sync Script
# Scans main_tools/ and sup_tools/ directories and generates TOOLS.md

param(
    [string]$ToolsDir = (Split-Path -Parent $PSScriptRoot)
)

$mainToolsPath = Join-Path $ToolsDir "main_tools"
$supToolsPath = Join-Path $ToolsDir "sup_tools"
$toolsOutputPath = Join-Path $ToolsDir "TOOLS.md"

Write-Host "Scanning tools in: $ToolsDir"

# ─────────────────────────────────────────────────────────────────────────
# Helper: Read SKILL.md and extract YAML frontmatter
# ─────────────────────────────────────────────────────────────────────────

function Get-SkillMetadata {
    param([string]$SkillMdPath)

    if (-not (Test-Path $SkillMdPath)) {
        return $null
    }

    $content = Get-Content $SkillMdPath -Raw
    $match = $content -match '(?s)^---\s*\n(.*?)\n---'

    if (-not $match) {
        return $null
    }

    $frontmatter = $matches[1]
    $metadata = @{}

    foreach ($line in $frontmatter -split "`n") {
        $line = $line.Trim()
        if ($line -match '^(\w+):\s*(.+)$') {
            $key = $matches[1]
            $value = $matches[2].Trim('"')
            $metadata[$key] = $value
        }
    }

    return $metadata
}

function Get-ToolInfo {
    param([string]$ToolPath, [string]$ToolType)

    $skillMdPath = Join-Path $ToolPath "SKILL.md"
    $metadata = Get-SkillMetadata $skillMdPath

    if (-not $metadata -or -not $metadata.name) {
        return $null
    }

    $apiRefPath = Join-Path $ToolPath "API_REFERENCE.md"
    $readmePath = Join-Path $ToolPath "README.md"

    $hasApiRef = Test-Path $apiRefPath
    $hasReadme = Test-Path $readmePath

    # Find the script project path (assuming standard structure)
    $scriptPath = Join-Path $ToolPath "scripts"
    $toolFolderName = (Get-Item $ToolPath).Name
    $projSubPath = Join-Path $scriptPath $toolFolderName
    $projFile = Join-Path $projSubPath "$toolFolderName.csproj"

    $toolPrefix = if ($ToolType -eq "main") { "main_tools" } else { "sup_tools" }

    $relativeProjPath = if (Test-Path $projFile) {
        "./$toolPrefix/$toolFolderName/scripts/$toolFolderName/$toolFolderName.csproj"
    } else {
        $null
    }

    return @{
        Name           = $metadata.name
        Description    = $metadata.description
        Path           = "./$toolPrefix/" + (Get-Item $ToolPath).Name
        ToolType       = $ToolType
        SkillMdPath    = $skillMdPath
        HasApiRef      = $hasApiRef
        HasReadme      = $hasReadme
        ProjectPath    = $relativeProjPath
    }
}

# ─────────────────────────────────────────────────────────────────────────
# Scan tools
# ─────────────────────────────────────────────────────────────────────────

$mainTools = @()
$supTools = @()
$toolCount = 0

# Scan main_tools
if (Test-Path $mainToolsPath) {
    Write-Host "Scanning main_tools..."
    $dirs = Get-ChildItem $mainToolsPath -Directory -ErrorAction SilentlyContinue

    foreach ($dir in $dirs) {
        $info = Get-ToolInfo $dir.FullName "main"
        if ($info) {
            $mainTools += $info
            $toolCount++
            Write-Host "  OK $($info.Name)"
        }
    }
}

# Scan sup_tools
if (Test-Path $supToolsPath) {
    Write-Host "Scanning sup_tools..."
    $dirs = Get-ChildItem $supToolsPath -Directory -ErrorAction SilentlyContinue

    foreach ($dir in $dirs) {
        $info = Get-ToolInfo $dir.FullName "sup"
        if ($info) {
            $supTools += $info
            $toolCount++
            Write-Host "  OK $($info.Name)"
        }
    }
}

Write-Host "Found $toolCount tools (main: $($mainTools.Count), sup: $($supTools.Count))"

# ─────────────────────────────────────────────────────────────────────────
# Generate TOOLS.md
# ─────────────────────────────────────────────────────────────────────────

$md = @"
# SwpilotCLI Tools Registry

This document indexes all available tools in the SwpilotCLI ecosystem, including built-in (main) tools and user-defined (sup) tools.

"@

# Main Tools
$md += "`n## Main Tools`n`n"
$md += "Built-in tools maintained as part of SwpilotCLI. These are always available and ready to use without additional setup.`n`n"

foreach ($tool in $mainTools) {
    $md += "### $($tool.Name)`n`n"
    $md += "**Description**: $($tool.Description)`n`n"
    $md += "**Path**: ``$($tool.Path)/```n`n"
    $md += "**Skill Definition**: ``$($tool.Path)/SKILL.md```n`n"

    if ($tool.HasApiRef) {
        $md += "**API Reference**: ``$($tool.Path)/API_REFERENCE.md```n`n"
    }

    if ($tool.ProjectPath) {
        $md += "**Quick run**:`n```bash`ndotnet run --project $($tool.ProjectPath)`n```n`n"
    }

    if ($tool.HasReadme) {
        $md += "**Learn more**: See ``$($tool.Path)/README.md``` for development and migration info.`n`n"
    }

    $md += "---`n`n"
}

# Sup Tools
$md += "`n## Sup Tools`n`n"
$md += "User-defined tools that extend SwpilotCLI functionality. Place your custom tools here in subdirectories following the same structure as Main Tools.`n`n"

if ($supTools.Count -eq 0) {
    $md += "No sup tools installed yet.`n`n"
} else {
    foreach ($tool in $supTools) {
        $md += "### $($tool.Name)`n`n"
        $md += "**Description**: $($tool.Description)`n`n"
        $md += "**Path**: ``$($tool.Path)/```n`n"
    }
}

$md += "`n### Template`n`n"
$md += @"
To create a new sup tool:

1. Create directory: `./sup_tools/your-tool-name/`
2. Add required files:
   `````
   your-tool-name/
   ├── SKILL.md              (required, with YAML frontmatter)
   ├── API_REFERENCE.md      (recommended)
   ├── README.md             (recommended)
   └── scripts/
       └── YourTool/         (your project directory)
           └── YourTool.csproj
   `````
3. Follow the same format as Main Tools for consistency
4. Run `sync-tools.ps1` to update this registry

---

## Migration to Agent Skills

All tools in this registry are designed to be directly compatible with Claude Agent Skills.

### Step 1: Prepare your tools

Ensure your tools follow the official structure:
`````
tool-name/
├── SKILL.md
├── API_REFERENCE.md (optional)
├── README.md (optional)
└── scripts/
    └── YourProject/
        └── YourProject.csproj
`````

### Step 2: Copy to ~/.claude/skills/

`````bash
# Copy main tool
cp -r ./main_tools/swpilotcli-active-doc-info ~/.claude/skills/

# Copy sup tool
cp -r ./sup_tools/your-tool-name ~/.claude/skills/
`````

### Step 3: Update SKILL.md if needed

If migrating from SwpilotCLI to standalone Agent Skills:
- Update `dotnet run` paths (may need adjustment in different environment)
- Update prerequisites if needed
- No changes to YAML frontmatter required

---

## Tool Development Guidelines

### Naming Convention

- Use lowercase with hyphens: `tool-name`
- For SwpilotCLI tools: prefix with `swpilotcli-`: `swpilotcli-feature-name`
- Keep names under 30 characters for clarity

### SKILL.md Structure

`````yaml
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
`````

### Code Implementation (C#)

- Use .NET 8+ for compatibility
- Console project structure with `Program.cs`
- Return structured output (JSON preferred)
- Exit codes: 0 = success, 1+ = error
- Keep execution time under 5 seconds

### Testing

Test locally before adding to registry:
`````bash
dotnet run --project ./scripts/YourTool/YourTool.csproj
`````

---

## FAQ

**Q: How do I add a tool to the registry?**
A: Create the directory structure under `sup_tools/`, follow the format, then run `sync-tools.ps1`.

**Q: Can I modify Main Tools?**
A: Main Tools are maintained by the SwpilotCLI team. For modifications, fork and create a custom Sup Tool.

**Q: Do tools require .NET 8?**
A: Currently yes, for consistency and C# compatibility. Python/Bash wrappers can use other runtimes if needed.

**Q: How are tools discovered by Claude?**
A: Claude reads this TOOLS.md file and can access any tool directory via bash. Specific tools are triggered based on user requests.

---

## Last Updated

- **Generated**: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
- **Tools**: $($mainTools.Count) Main + $($supTools.Count) Sup tools
"@

# Write output
Set-Content -Path $toolsOutputPath -Value $md -Encoding UTF8
Write-Host "`nGenerated: $toolsOutputPath"
