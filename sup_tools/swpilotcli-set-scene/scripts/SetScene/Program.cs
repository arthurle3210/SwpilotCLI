using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace SolidWorksConsole;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        SldWorks? swApp = null;
        ModelDoc2? swDoc = null;
        string? originalConfiguration = null;

        try
        {
            if (args.Length is < 1 or > 4)
                throw new ArgumentException("Usage: SetScene <scene-name-or-path> [current|all] [verify|noverify] [nosave|save]");

            string scenePath = ResolveScenePath(args[0]);
            string scope = args.Length > 1 ? args[1].Trim().ToLowerInvariant() : "current";
            string verification = args.Length > 2 ? args[2].Trim().ToLowerInvariant() : "verify";
            string saveBehavior = args.Length > 3 ? args[3].Trim().ToLowerInvariant() : "nosave";

            if (scope is not ("current" or "all"))
                throw new ArgumentException("Configuration scope must be 'current' or 'all'.");
            if (verification is not ("verify" or "noverify"))
                throw new ArgumentException("Verification must be 'verify' or 'noverify'.");
            if (saveBehavior is not ("nosave" or "save"))
                throw new ArgumentException("Save behavior must be 'nosave' or 'save'.");

            swApp = Interaction.GetObject(string.Empty, "SldWorks.Application") as SldWorks;
            swDoc = swApp?.IActiveDoc2 ?? throw new InvalidOperationException("No active SolidWorks document.");
            originalConfiguration = swDoc.IGetActiveConfiguration().Name;

            string[] targets = scope == "all"
                ? (string[])swDoc.GetConfigurationNames()
                : new[] { originalConfiguration };

            ModelDocExtension extension = swDoc.Extension;
            foreach (string configurationName in targets)
            {
                ActivateConfiguration(swDoc, configurationName);
                if (!extension.InsertScene(scenePath))
                    throw new InvalidOperationException($"Could not apply scene to configuration '{configurationName}'.");
                swDoc.GraphicsRedraw2();

                if (verification == "verify")
                    VerifyScene(swDoc, configurationName, scenePath);

                Console.WriteLine($"{configurationName}: {Path.GetFileNameWithoutExtension(scenePath)}" +
                    (verification == "verify" ? " (verified)" : string.Empty));
            }

            ActivateConfiguration(swDoc, originalConfiguration);
            swDoc.GraphicsRedraw2();

            if (saveBehavior == "save")
            {
                int errors = 0;
                int warnings = 0;
                if (!swDoc.Save3((int)swSaveAsOptions_e.swSaveAsOptions_Silent, ref errors, ref warnings))
                    throw new InvalidOperationException($"Save failed; errors={errors}, warnings={warnings}.");
                Console.WriteLine("Saved active document.");
            }
            else
            {
                Console.WriteLine("Changes left unsaved.");
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
        finally
        {
            if (swDoc != null && originalConfiguration != null)
            {
                try { ActivateConfiguration(swDoc, originalConfiguration); }
                catch { }
            }
            if (swDoc != null) Marshal.ReleaseComObject(swDoc);
            if (swApp != null) Marshal.ReleaseComObject(swApp);
        }
    }

    private static void ActivateConfiguration(ModelDoc2 document, string configurationName)
    {
        string activeName = document.IGetActiveConfiguration().Name;
        if (!string.Equals(activeName, configurationName, StringComparison.OrdinalIgnoreCase) &&
            !document.ShowConfiguration2(configurationName))
            throw new InvalidOperationException($"Could not activate configuration '{configurationName}'.");
    }

    private static void VerifyScene(ModelDoc2 document, string configurationName, string expectedPath)
    {
        Configuration configuration = document.GetConfigurationByName(configurationName)
            ?? throw new InvalidOperationException($"Configuration '{configurationName}' was not found during verification.");
        SWScene scene = configuration.GetScene()
            ?? throw new InvalidOperationException($"Configuration '{configurationName}' has no scene after application.");
        string actualPath = string.Empty;
        scene.GetP2SFileName(ref actualPath);

        if (!PathsEqual(actualPath, expectedPath))
            throw new InvalidOperationException(
                $"Scene verification failed for '{configurationName}'. Expected '{expectedPath}', found '{actualPath}'.");
    }

    private static string ResolveScenePath(string sceneInput)
    {
        string input = sceneInput.Trim().Trim('"');
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Scene name or path cannot be empty.");

        if (File.Exists(input)) return Path.GetFullPath(input);

        string sceneRoot = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles),
            "SOLIDWORKS Corp", "SOLIDWORKS", "data", "graphics", "Scenes");
        if (!Directory.Exists(sceneRoot))
            throw new DirectoryNotFoundException($"SolidWorks scene library was not found: {sceneRoot}");

        string requestedName = Path.GetFileNameWithoutExtension(input);
        string[] matches = Directory.EnumerateFiles(sceneRoot, "*.p2s", SearchOption.AllDirectories)
            .Where(path => string.Equals(
                Path.GetFileNameWithoutExtension(path), requestedName, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        return matches.Length switch
        {
            1 => Path.GetFullPath(matches[0]),
            0 => throw new FileNotFoundException($"Scene '{sceneInput}' was not found in {sceneRoot}."),
            _ => throw new InvalidOperationException(
                $"Scene name '{sceneInput}' is ambiguous. Use a full path. Matches: {string.Join("; ", matches)}")
        };
    }

    private static bool PathsEqual(string first, string second)
    {
        if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(second)) return false;
        return string.Equals(Path.GetFullPath(first), Path.GetFullPath(second), StringComparison.OrdinalIgnoreCase);
    }
}
