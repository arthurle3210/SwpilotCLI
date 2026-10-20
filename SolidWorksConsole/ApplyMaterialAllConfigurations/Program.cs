using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace ApplyMaterialAllConfigurations;

internal static class Program
{
    [STAThread]
    private static int Main()
    {
        SldWorks? app = null;
        ModelDoc2? doc = null;
        try
        {
            app = Interaction.GetObject(string.Empty, "SldWorks.Application") as SldWorks ?? throw new InvalidOperationException("SolidWorks is not running.");
            doc = app.IActiveDoc2 ?? throw new InvalidOperationException("No active SolidWorks document.");
            if (doc.GetType() != (int)swDocumentTypes_e.swDocPART) throw new InvalidOperationException("Active document must be a part.");
            var part = (PartDoc)doc;
            string sourceMaterial = part.GetMaterialPropertyName2("Default", out string sourceDatabase);
            if (!string.Equals(sourceMaterial, "Plain Carbon Steel", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Default material is '{sourceMaterial}', not 'Plain Carbon Steel'.");
            string[] configurations = doc.GetConfigurationNames() as string[] ?? Array.Empty<string>();
            var failures = new List<string>();
            foreach (string configuration in configurations)
            {
                part.SetMaterialPropertyName2(configuration, sourceDatabase, sourceMaterial);
                string actualMaterial = part.GetMaterialPropertyName2(configuration, out string actualDatabase);
                if (!string.Equals(actualMaterial, sourceMaterial, StringComparison.OrdinalIgnoreCase) || !string.Equals(actualDatabase, sourceDatabase, StringComparison.OrdinalIgnoreCase)) failures.Add(configuration);
                else Console.WriteLine($"{configuration}: {actualMaterial} (verified)");
            }
            if (failures.Count > 0) throw new InvalidOperationException($"Material verification failed: {string.Join(", ", failures)}");
            Console.WriteLine($"Applied {sourceMaterial} to {configurations.Length} configuration(s). Changes are not saved.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine("ERROR: " + ex.Message); return 1; }
        finally { if (doc != null) Marshal.ReleaseComObject(doc); if (app != null) Marshal.ReleaseComObject(app); }
    }
}
