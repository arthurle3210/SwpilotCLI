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

    private static void ExecuteTask(ModelDoc2 swDoc, SldWorks swApp)
    {
        var drawing = (DrawingDoc)swDoc;
        if (!drawing.ActivateView("Front View")) throw new InvalidOperationException("Could not activate Front View.");
        var view = (SolidWorks.Interop.sldworks.View)drawing.ActiveDrawingView;
        var components = (object[]?)view.GetVisibleComponents() ?? Array.Empty<object>();
        Console.WriteLine($"Visible components: {components.Length}");
        foreach (var item in components)
        {
            var component = item as Component2;
            if (component == null) continue;
            var entities = (object[]?)view.GetVisibleEntities2(component, (int)swViewEntityType_e.swViewEntityType_Edge) ?? Array.Empty<object>();
            foreach (var entity in entities)
            {
                var edge = entity as Edge;
                if (edge == null) continue;
                var curve = edge.GetCurve() as Curve;
                if (curve == null || !curve.IsCircle()) continue;
                var circle = (double[])curve.CircleParams;
                Console.WriteLine($"Circle: center=({circle[0] * 1000:F3}, {circle[1] * 1000:F3}, {circle[2] * 1000:F3}) mm, radius={circle[6] * 1000:F3} mm");
            }
        }
    }

    private static SldWorks? ConnectToSolidWorks()
    {
        try { return Interaction.GetObject(string.Empty, "SldWorks.Application") as SldWorks; }
        catch (COMException) { }
        var type = Type.GetTypeFromProgID("SldWorks.Application");
        return type == null ? null : (SldWorks?)Activator.CreateInstance(type);
    }
}
