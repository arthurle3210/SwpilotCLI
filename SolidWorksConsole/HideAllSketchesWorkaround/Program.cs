using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace HideAllSketchesWorkaround;

internal static class Program
{
    [STAThread]
    private static int Main()
    {
        SldWorks? app = null;
        ModelDoc2? doc = null;
        try
        {
            app = ConnectToSolidWorks();
            doc = app?.IActiveDoc2;
            if (app == null || doc == null) throw new InvalidOperationException("No active SolidWorks document.");
            if (doc.GetType() is not ((int)swDocumentTypes_e.swDocPART) and not ((int)swDocumentTypes_e.swDocASSEMBLY))
                throw new InvalidOperationException("The active document must be a part or assembly.");

            var original = ((Configuration)doc.GetActiveConfiguration()).Name;
            var names = doc.GetConfigurationNames() as string[] ?? Array.Empty<string>();
            int processed = 0, hidden = 0;
            var failures = new List<string>();

            foreach (var name in names)
            {
                var active = ((Configuration)doc.GetActiveConfiguration()).Name;
                if (!string.Equals(active, name, StringComparison.Ordinal))
                {
                    doc.ShowConfiguration2(name);
                    active = ((Configuration)doc.GetActiveConfiguration()).Name;
                }
                if (!string.Equals(active, name, StringComparison.Ordinal))
                {
                    failures.Add($"{name}: could not activate.");
                    continue;
                }

                foreach (var item in doc.FeatureManager.GetFeatures(false) as object[] ?? Array.Empty<object>())
                {
                    if (item is not Feature feature || feature.GetSpecificFeature2() is not Sketch) continue;
                    doc.ClearSelection2(true);
                    if (!feature.Select2(false, 0))
                    {
                        failures.Add($"{name}: could not select {feature.Name}.");
                        continue;
                    }
                    doc.BlankSketch();
                    hidden++;
                }
                doc.ClearSelection2(true);
                processed++;
            }

            if (!string.Equals(((Configuration)doc.GetActiveConfiguration()).Name, original, StringComparison.Ordinal))
                doc.ShowConfiguration2(original);
            doc.ClearSelection2(true);
            doc.EditRebuild3();
            Console.WriteLine($"Configurations processed: {processed}/{names.Length}");
            Console.WriteLine($"Sketch features hidden: {hidden}");
            foreach (var failure in failures) Console.WriteLine(failure);
            if (failures.Count > 0) throw new InvalidOperationException($"{failures.Count} configuration or selection issue(s) occurred.");
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
        return 0;
    }

    private static SldWorks? ConnectToSolidWorks()
    {
        try { return Interaction.GetObject(string.Empty, "SldWorks.Application") as SldWorks; }
        catch (COMException) { return null; }
    }
}
