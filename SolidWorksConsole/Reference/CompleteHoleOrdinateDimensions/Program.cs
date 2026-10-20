using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace SolidWorksConsole;

internal static class Program
{
    private sealed record Hole(Edge Edge, double X, double Z);
    private sealed record OrdinateCandidate(Annotation Annotation, string Name, double Value, double X, double Y);

    [STAThread]
    private static int Main()
    {
        SldWorks? swApp = null;
        ModelDoc2? swDoc = null;
        try
        {
            swApp = ConnectToSolidWorks();
            swDoc = swApp?.IActiveDoc2;
            if (swApp == null || swDoc == null) throw new InvalidOperationException("No active SolidWorks document.");
            ExecuteTask(swDoc);
            Console.WriteLine("Task completed successfully.");
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

    private static void ExecuteTask(ModelDoc2 swDoc)
    {
        if (swDoc.GetType() != (int)swDocumentTypes_e.swDocDRAWING)
            throw new InvalidOperationException("The active document must be a drawing.");

        var drawing = (DrawingDoc)swDoc;
        if (!drawing.ActivateView("Front View"))
            throw new InvalidOperationException("Drawing view 'Front View' was not found.");
        var view = drawing.ActiveDrawingView as SolidWorks.Interop.sldworks.View
            ?? throw new InvalidOperationException("Could not activate 'Front View'.");

        const double expectedRadius = 0.00555625; // 0.21875 in
        const double radiusTolerance = 0.00010;
        var holes = new List<Hole>();
        var components = view.GetVisibleComponents() as object[] ?? Array.Empty<object>();
        foreach (var component in components.OfType<Component2>())
        {
            var entities = view.GetVisibleEntities2(component, (int)swViewEntityType_e.swViewEntityType_Edge)
                           as object[] ?? Array.Empty<object>();
            foreach (var edge in entities.OfType<Edge>())
            {
                var curve = edge.GetCurve() as Curve;
                if (curve == null || !curve.IsCircle()) continue;
                var circle = curve.CircleParams as double[];
                if (circle == null || circle.Length < 7 || Math.Abs(circle[6] - expectedRadius) > radiusTolerance) continue;
                holes.Add(new Hole(edge, circle[0], circle[2]));
            }
        }

        var xHoles = DistinctByCoordinate(holes, h => h.X);
        var zHoles = DistinctByCoordinate(holes, h => h.Z);
        if (xHoles.Count != 14 || zHoles.Count != 10)
            throw new InvalidOperationException($"Expected 14 unique X and 10 unique Z hole locations; found {xHoles.Count} and {zHoles.Count}.");

        var candidates = new List<OrdinateCandidate>();
        for (Annotation? ann = view.GetFirstAnnotation3(); ann != null; ann = ann.GetNext3())
        {
            if (ann.GetType() != (int)swAnnotationType_e.swDisplayDimension) continue;
            var display = ann.GetSpecificAnnotation() as DisplayDimension;
            var dimension = display?.GetDimension2(0) as Dimension;
            if (display == null || dimension == null || display.Type2 != (int)swDimensionType_e.swOrdinateDimension) continue;
            var pos = ann.GetPosition() as double[];
            if (pos == null || pos.Length < 2) continue;
            candidates.Add(new OrdinateCandidate(ann, dimension.FullName ?? "(unnamed)", dimension.SystemValue, pos[0], pos[1]));
        }

        var nonZero = candidates.Where(c => Math.Abs(c.Value) > 0.000001).ToList();
        if (nonZero.Count < 2)
            throw new InvalidOperationException("Could not find the existing non-zero ordinate dimensions.");

        var horizontal = nonZero.OrderBy(c => c.Y).First();
        var vertical = nonZero.OrderBy(c => c.X).First();
        if (ReferenceEquals(horizontal.Annotation, vertical.Annotation))
            throw new InvalidOperationException("Could not distinguish horizontal and vertical ordinate groups.");

        AddToExistingGroup(swDoc, horizontal, xHoles);
        AddToExistingGroup(swDoc, vertical, zHoles);
        swDoc.ClearSelection2(true);
        swDoc.GraphicsRedraw2();

        Console.WriteLine($"Horizontal group: {horizontal.Name}; added {xHoles.Count} X ordinates.");
        Console.WriteLine(string.Join(", ", xHoles.Select(h => $"{h.X / 0.0254:F4} in")));
        Console.WriteLine($"Vertical group: {vertical.Name}; added {zHoles.Count} Z ordinates.");
        Console.WriteLine(string.Join(", ", zHoles.Select(h => $"{h.Z / 0.0254:F4} in")));
    }

    private static List<Hole> DistinctByCoordinate(IEnumerable<Hole> holes, Func<Hole, double> coordinate)
    {
        const double tolerance = 0.00001; // 0.01 mm
        var result = new List<Hole>();
        foreach (var hole in holes.OrderBy(coordinate))
        {
            if (!result.Any(existing => Math.Abs(coordinate(existing) - coordinate(hole)) < tolerance))
                result.Add(hole);
        }
        return result;
    }

    private static void AddToExistingGroup(ModelDoc2 swDoc, OrdinateCandidate existing, IEnumerable<Hole> holes)
    {
        swDoc.ClearSelection2(true);
        if (!existing.Annotation.Select3(false, null))
            throw new InvalidOperationException($"Could not select existing ordinate dimension {existing.Name}.");
        foreach (var hole in holes)
            if (!((Entity)hole.Edge).Select4(true, null))
                throw new InvalidOperationException("Could not select a hole edge for ordinate dimensioning.");
        swDoc.EditOrdinate();
        swDoc.ClearSelection2(true);
    }

    private static SldWorks? ConnectToSolidWorks()
    {
        try { return Interaction.GetObject(string.Empty, "SldWorks.Application") as SldWorks; }
        catch (COMException) { return null; }
    }
}
