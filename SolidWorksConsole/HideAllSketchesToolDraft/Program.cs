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
        var configurationNames = (string[])swDoc.GetConfigurationNames();
        var retryNames = new List<string>();
        var processedConfigurations = 0;
        var hiddenSketchFeatures = 0;
        var failures = new List<string>();

        foreach (var configurationName in configurationNames)
        {
            if (!swDoc.ShowConfiguration2(configurationName))
            {
                retryNames.Add(configurationName);
                continue;
            }

            foreach (var item in (object[])swDoc.FeatureManager.GetFeatures(false))
            {
                var feature = item as Feature;
                if (feature == null || feature.GetSpecificFeature2() is not Sketch) continue;
                swDoc.ClearSelection2(true);
                if (feature.Select2(false, 0))
                {
                    swDoc.BlankSketch();
                    hiddenSketchFeatures++;
                }
                else failures.Add($"{configurationName}: unable to select {feature.Name}");
            }
            processedConfigurations++;
        }

        foreach (var configurationName in retryNames)
        {
            if (!swDoc.ShowConfiguration2(configurationName))
            {
                failures.Add($"{configurationName}: failed to activate after retry");
                continue;
            }
            foreach (var item in (object[])swDoc.FeatureManager.GetFeatures(false))
            {
                var feature = item as Feature;
                if (feature == null || feature.GetSpecificFeature2() is not Sketch) continue;
                swDoc.ClearSelection2(true);
                if (feature.Select2(false, 0))
                {
                    swDoc.BlankSketch();
                    hiddenSketchFeatures++;
                }
                else failures.Add($"{configurationName}: unable to select {feature.Name}");
            }
            processedConfigurations++;
        }

        swDoc.ShowConfiguration2(originalConfig);
        swDoc.ClearSelection2(true);
        swDoc.EditRebuild3();
        Console.WriteLine($"Configurations processed: {processedConfigurations}/{configurationNames.Length}");
        Console.WriteLine($"Sketch features hidden: {hiddenSketchFeatures}");
        foreach (var failure in failures) Console.WriteLine(failure);
        if (failures.Count > 0) throw new InvalidOperationException($"{failures.Count} configuration or selection issue(s) occurred.");
    }

    private static SldWorks? ConnectToSolidWorks()
    {
        try { return Interaction.GetObject(string.Empty, "SldWorks.Application") as SldWorks; }
        catch (COMException) { }
        var type = Type.GetTypeFromProgID("SldWorks.Application");
        return type == null ? null : (SldWorks?)Activator.CreateInstance(type);
    }
}
