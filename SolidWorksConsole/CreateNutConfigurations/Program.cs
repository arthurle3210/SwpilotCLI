using System;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace SolidWorksConsole;
internal static class Program
{
    [STAThread]
    private static int Main()
    {
        SldWorks? swApp = null; ModelDoc2? swDoc = null;
        try
        {
            swApp = Interaction.GetObject(string.Empty, "SldWorks.Application") as SldWorks;
            if (swApp == null) return 2;
            swDoc = swApp.IActiveDoc2; if (swDoc == null) return 3;

            Configuration original = swDoc.IGetActiveConfiguration();
            string originalName = original.Name;
            if (swDoc.GetConfigurationByName("0.75UNC") == null && swDoc.AddConfiguration3("0.75UNC", "3/4-10 UNC, 21/32 tap-drill bore", "", 0) == null)
                throw new InvalidOperationException("Failed to create 0.75UNC.");
            swDoc.ShowConfiguration2(originalName);
            if (swDoc.GetConfigurationByName("0.75UNF") == null && swDoc.AddConfiguration3("0.75UNF", "3/4-16 UNF, 11/16 tap-drill bore", "", 0) == null)
                throw new InvalidOperationException("Failed to create 0.75UNF.");

            Dimension bore = swDoc.Parameter("D1@Sketch2") as Dimension ?? throw new InvalidOperationException("Could not find D1@Sketch2.");
            int unc = bore.SetSystemValue3(0.01666875, (int)swSetValueInConfiguration_e.swSetValue_InSpecificConfigurations, "0.75UNC");
            int unf = bore.SetSystemValue3(0.01746250, (int)swSetValueInConfiguration_e.swSetValue_InSpecificConfigurations, "0.75UNF");
            swDoc.EditRebuild3();
            if (unc != 0 || unf != 0) throw new InvalidOperationException($"Dimension update failed (UNC={unc}, UNF={unf}).");

            if (swDoc.IGetActiveConfiguration().Name != "0.75UNC" && !swDoc.ShowConfiguration2("0.75UNC"))
                throw new InvalidOperationException("Failed to activate 0.75UNC.");
            swDoc.EditRebuild3();
            if (originalName != "0.75UNC" && originalName != "0.75UNF" && !swDoc.DeleteConfiguration2(originalName))
                throw new InvalidOperationException($"Failed to delete '{originalName}'.");

            int errors = 0, warnings = 0;
            if (!swDoc.Save3((int)swSaveAsOptions_e.swSaveAsOptions_Silent, ref errors, ref warnings))
                throw new InvalidOperationException($"Save failed (errors={errors}, warnings={warnings}).");
            foreach (string name in new[] { "0.75UNC", "0.75UNF" })
            {
                if (swDoc.IGetActiveConfiguration().Name != name && !swDoc.ShowConfiguration2(name))
                    throw new InvalidOperationException($"Could not verify {name}.");
                swDoc.EditRebuild3();
                object raw = bore.GetSystemValue3((int)swInConfigurationOpts_e.swSpecifyConfiguration, new string[] { name });
                double[] values = (double[])raw;
                Console.WriteLine($"{name}: bore diameter = {values[0] * 1000.0:F5} mm");
            }
            swDoc.ShowConfiguration2("0.75UNC");
            swDoc.EditRebuild3();
            errors = 0; warnings = 0;
            if (!swDoc.Save3((int)swSaveAsOptions_e.swSaveAsOptions_Silent, ref errors, ref warnings))
                throw new InvalidOperationException($"Final save failed (errors={errors}, warnings={warnings}).");
            Console.WriteLine("Created exactly two configurations: 0.75UNC and 0.75UNF. Active: 0.75UNC.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine($"Error: {ex.Message}"); return 1; }
        finally { if (swDoc != null) Marshal.ReleaseComObject(swDoc); if (swApp != null) Marshal.ReleaseComObject(swApp); }
    }
}
