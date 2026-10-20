using System;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

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
        var drawing = (DrawingDoc)swDoc;
        const string targetViewName = "Drawing View2";
        if (!drawing.ActivateView(targetViewName))
            throw new InvalidOperationException($"Could not activate {targetViewName}.");

        var targetView = (SolidWorks.Interop.sldworks.View)drawing.ActiveDrawingView;
        var inserted = targetView.AutoInsertCenterMarks2(
            (int)swAutoInsertCenterMarkTypes_e.swAutoInsertCenterMarkType_Hole,
            (int)swCenterMarkConnectionLine_e.swCenterMark_ShowNoConnectLines,
            false, false, true, 0.0, 0.0, false, false, 0.0);

        if (!inserted)
            throw new InvalidOperationException($"SolidWorks could not insert center marks in {targetViewName}.");

        swDoc.ClearSelection2(true);
        swDoc.EditRebuild3();
        Console.WriteLine($"Inserted center marks for holes in {targetViewName} only.");
    }

    private static SldWorks? ConnectToSolidWorks()
    {
        try { return Interaction.GetObject(string.Empty, "SldWorks.Application") as SldWorks; }
        catch (COMException) { }
        var type = Type.GetTypeFromProgID("SldWorks.Application");
        return type == null ? null : (SldWorks?)Activator.CreateInstance(type);
    }
}
