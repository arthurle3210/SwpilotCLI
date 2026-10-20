using System;
using System.IO;
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
        SldWorks? swApp = null;
        ModelDoc2? swDoc = null;
        try
        {
            swApp = Interaction.GetObject(string.Empty, "SldWorks.Application") as SldWorks;
            if (swApp == null) return 2;
            swDoc = swApp.IActiveDoc2;
            if (swDoc == null) return 3;

            string currentPath = swDoc.GetPathName();
            string? folder = Path.GetDirectoryName(currentPath);
            if (string.IsNullOrWhiteSpace(folder)) throw new InvalidOperationException("Could not determine the document folder.");

            string newPath = Path.Combine(folder, "0.75 hex nut.SLDPRT");
            int result = swDoc.SaveAs3(newPath, (int)swSaveAsVersion_e.swSaveAsCurrentVersion,
                (int)swSaveAsOptions_e.swSaveAsOptions_Silent);
            if (result != 0) throw new InvalidOperationException($"SaveAs3 failed with status {result}.");

            Console.WriteLine($"Renamed active part to: {newPath}");
            return 0;
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
}
