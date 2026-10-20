using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using SolidWorks.Interop.sldworks;

namespace DeleteDocumentCustomProperties;

internal static class Program
{
    [STAThread]
    private static int Main()
    {
        SldWorks? app = null;
        ModelDoc2? doc = null;
        try
        {
            app = Interaction.GetObject(string.Empty, "SldWorks.Application") as SldWorks
                ?? throw new InvalidOperationException("SolidWorks is not running.");
            doc = app.IActiveDoc2 ?? throw new InvalidOperationException("No active SolidWorks document.");
            dynamic manager = doc.Extension.CustomPropertyManager[string.Empty];
            string[] names = manager.GetNames() as string[] ?? Array.Empty<string>();
            var deleted = new List<string>();
            var failed = new List<string>();
            foreach (string name in names)
            {
                if ((int)manager.Delete(name) == 0) deleted.Add(name); else failed.Add(name);
            }
            Console.WriteLine($"Document-level properties found: {names.Length}");
            Console.WriteLine($"Deleted: {deleted.Count}");
            foreach (string name in deleted) Console.WriteLine($"  deleted: {name}");
            if (failed.Count > 0) throw new InvalidOperationException($"Could not delete: {string.Join(", ", failed)}");
            Console.WriteLine("Configuration-level custom properties were not accessed.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine("ERROR: " + ex.Message); return 1; }
        finally { if (doc != null) Marshal.ReleaseComObject(doc); if (app != null) Marshal.ReleaseComObject(app); }
    }
}
