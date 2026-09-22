using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace SolidWorksConsole;

internal static class Program
{
    private const string ScenePath = @"C:\Program Files\SOLIDWORKS Corp\SOLIDWORKS\data\graphics\Scenes\01 basic scenes\dark grey background.p2s";

    [STAThread]
    private static int Main()
    {
        SldWorks? app = null;
        ModelDoc2? doc = null;
        try
        {
            app = Interaction.GetObject(string.Empty, "SldWorks.Application") as SldWorks;
            doc = app?.IActiveDoc2 ?? throw new InvalidOperationException("No active SolidWorks document.");
            if (!File.Exists(ScenePath)) throw new FileNotFoundException("Scene file was not found.", ScenePath);

            string original = doc.IGetActiveConfiguration().Name;
            string[] names = (string[])doc.GetConfigurationNames();
            ModelDocExtension extension = doc.Extension;

            foreach (string name in names)
            {
                Console.WriteLine($"Applying to {name}...");
                string activeName = doc.IGetActiveConfiguration().Name;
                if (!string.Equals(activeName, name, StringComparison.OrdinalIgnoreCase) && !doc.ShowConfiguration2(name))
                    throw new InvalidOperationException($"Could not activate configuration {name}.");
                Console.WriteLine($"  Active: {doc.IGetActiveConfiguration().Name}");
                if (!extension.InsertScene(ScenePath))
                    throw new InvalidOperationException($"Could not apply the scene to configuration {name}.");
                Console.WriteLine("  Inserted scene");
                doc.GraphicsRedraw2();
                Console.WriteLine("  Redrawn");
            }

            foreach (string name in names)
            {
                Console.WriteLine($"Verifying {name}...");
                Configuration config = doc.GetConfigurationByName(name);
                SWScene scene = config.GetScene();
                string appliedPath = string.Empty;
                scene.GetP2SFileName(ref appliedPath);
                bool matches = string.Equals(Path.GetFullPath(appliedPath), Path.GetFullPath(ScenePath), StringComparison.OrdinalIgnoreCase);
                Console.WriteLine($"{name}: {Path.GetFileNameWithoutExtension(appliedPath)} ({(matches ? "verified" : "mismatch")})");
                if (!matches) throw new InvalidOperationException($"Scene verification failed for {name}: {appliedPath}");
            }

            if (!string.Equals(doc.IGetActiveConfiguration().Name, original, StringComparison.OrdinalIgnoreCase) && !doc.ShowConfiguration2(original))
                throw new InvalidOperationException($"Could not restore original configuration {original}.");
            doc.GraphicsRedraw2();
            int errors = 0, warnings = 0;
            if (!doc.Save3((int)swSaveAsOptions_e.swSaveAsOptions_Silent, ref errors, ref warnings))
                throw new InvalidOperationException($"Save failed; errors={errors}, warnings={warnings}.");
            Console.WriteLine("Saved active document.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
        finally
        {
            if (doc != null) Marshal.ReleaseComObject(doc);
            if (app != null) Marshal.ReleaseComObject(app);
        }
    }
}
