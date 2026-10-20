using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace SolidWorksConsole;

internal static class Program
{
    private static Feature? FindFeature(ModelDoc2 document, string name)
    {
        for (Feature? feature = document.FirstFeature(); feature != null; feature = feature.GetNextFeature())
            if (string.Equals(feature.Name, name, StringComparison.OrdinalIgnoreCase)) return feature;
        return null;
    }

    [STAThread]
    private static int Main()
    {
        SldWorks? app = null;
        ModelDoc2? doc = null;
        try
        {
            app = Interaction.GetObject(string.Empty, "SldWorks.Application") as SldWorks;
            doc = app?.IActiveDoc2 ?? throw new InvalidOperationException("No active SolidWorks document.");
            if (doc.GetType() != (int)swDocumentTypes_e.swDocPART)
                throw new InvalidOperationException("The active document is not a part.");

            var targets = new (string Name, string Type)[]
            {
                ("Helix/Spiral1", "Helix"),
                ("Plane1", "RefPlane")
            };

            foreach ((string name, string expectedType) in targets)
            {
                Feature feature = FindFeature(doc, name)
                    ?? throw new InvalidOperationException($"Feature '{name}' was not found.");
                string actualType = feature.GetTypeName2();
                if (!string.Equals(actualType, expectedType, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"Feature '{name}' has type '{actualType}', expected '{expectedType}'.");

                doc.ClearSelection2(true);
                if (!feature.Select2(false, 0))
                    throw new InvalidOperationException($"Could not select feature '{name}'.");
                if (!doc.Extension.DeleteSelection2(0))
                    throw new InvalidOperationException($"Could not delete feature '{name}'.");
                Console.WriteLine($"Deleted {name}.");
            }

            doc.ClearSelection2(true);
            doc.EditRebuild3();
            doc.GraphicsRedraw2();

            foreach ((string name, _) in targets)
                if (FindFeature(doc, name) != null)
                    throw new InvalidOperationException($"Verification failed: '{name}' is still present.");

            int errors = 0, warnings = 0;
            if (!doc.Save3((int)swSaveAsOptions_e.swSaveAsOptions_Silent, ref errors, ref warnings))
                throw new InvalidOperationException($"Save failed; errors={errors}, warnings={warnings}.");
            Console.WriteLine("Verified both features absent and saved the active document.");
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
