using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using SolidWorks.Interop.sldworks;

namespace DeleteFileLevelCustomProperties;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        SldWorks? app = null;
        ModelDoc2? doc = null;
        try
        {
            string mode = args.Length == 0 ? "dry-run" : args[0].Trim().ToLowerInvariant();
            if (args.Length > 1 || mode is not ("dry-run" or "apply")) throw new InvalidOperationException("Usage: [dry-run|apply]");
            app = Interaction.GetObject(string.Empty, "SldWorks.Application") as SldWorks ?? throw new InvalidOperationException("SolidWorks is not running.");
            doc = app.IActiveDoc2 ?? throw new InvalidOperationException("No active SolidWorks document.");
            dynamic manager = doc.Extension.CustomPropertyManager[string.Empty];
            string[] names = manager.GetNames() as string[] ?? Array.Empty<string>();
            Console.WriteLine($"Document-level custom properties: {names.Length}");
            foreach (string name in names) Console.WriteLine($"  {name}");
            if (mode == "dry-run")
            {
                Console.WriteLine("DRY-RUN: no properties changed. Use apply to delete these document-level properties.");
                return 0;
            }
            var failures = new List<string>();
            foreach (string name in names)
            {
                int result = manager.Delete2(name);
                if (result != 0) failures.Add($"{name} (result {result})");
            }
            string[] remaining = manager.GetNames() as string[] ?? Array.Empty<string>();
            if (failures.Count > 0 || remaining.Length > 0)
                throw new InvalidOperationException($"Deletion incomplete. Failed: {string.Join(", ", failures)}. Remaining: {string.Join(", ", remaining)}");
            Console.WriteLine($"Deleted: {names.Length}");
            Console.WriteLine("Configuration-level custom properties were not accessed. Changes are not saved.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine("ERROR: " + ex.Message); return 1; }
        finally { if (doc != null) Marshal.ReleaseComObject(doc); if (app != null) Marshal.ReleaseComObject(app); }
    }
}
