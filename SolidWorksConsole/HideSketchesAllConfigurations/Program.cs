using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using SolidWorks.Interop.sldworks;

namespace SolidWorksConsole;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        SldWorks? swApp = null;
        ModelDoc2? swDoc = null;
        try
        {
            swApp = ConnectToSolidWorks();
            if (swApp == null) return 2;
            swApp.Visible = true;
            swDoc = swApp.IActiveDoc2;
            if (swDoc == null) return 3;
            ExecuteTask(swDoc, swApp);
            Console.WriteLine("Task completed successfully.");
            return 0;
        }
        catch (COMException ex)
        {
            Console.Error.WriteLine($"COM error: 0x{ex.ErrorCode:X8} - {ex.Message}");
            return ex.ErrorCode;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
        finally
        {
            if (swDoc != null) Marshal.ReleaseComObject(swDoc);
            if (swApp != null) Marshal.ReleaseComObject(swApp);
        }
    }

    private static void ExecuteTask(ModelDoc2 swDoc, SldWorks swApp)
    {
        var originalConfig = ((Configuration)swDoc.GetActiveConfiguration()).Name;
        var targets = new[] { "Sketch120", "Sketch123", "Sketch124", "Sketch125", "Sketch126", "Sketch127" };
        var configurations = (string[])swDoc.GetConfigurationNames();
        var results = new List<string>();

        foreach (var configuration in configurations)
        {
            if (!swDoc.ShowConfiguration2(configuration))
            {
                results.Add($"{configuration}: failed to activate");
                continue;
            }

            foreach (var sketchName in targets)
            {
                swDoc.ClearSelection2(true);
                bool selected = swDoc.Extension.SelectByID2(sketchName, "SKETCH", 0, 0, 0, false, 0, null, 0);
                if (!selected)
                {
                    results.Add($"{configuration}: {sketchName} not found");
                    continue;
                }
                swDoc.BlankSketch();
                swDoc.ClearSelection2(true);
                results.Add($"{configuration}: {sketchName} hidden");
            }
        }

        swDoc.ShowConfiguration2(originalConfig);
        swDoc.ClearSelection2(true);
        swDoc.EditRebuild3();
        Console.WriteLine($"Processed {configurations.Length} configuration(s).");
        foreach (var result in results) Console.WriteLine(result);
    }

    private static SldWorks? ConnectToSolidWorks()
    {
        try { return Interaction.GetObject(string.Empty, "SldWorks.Application") as SldWorks; }
        catch (COMException) { }
        var type = Type.GetTypeFromProgID("SldWorks.Application");
        return type == null ? null : (SldWorks?)Activator.CreateInstance(type);
    }
}
