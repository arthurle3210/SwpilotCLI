using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace DeleteDanglingAnnotations;

internal static class Program
{
    private sealed record Candidate(Annotation Annotation, string Category, string ViewName);

    [STAThread]
    private static int Main(string[] args)
    {
        SldWorks? swApp = null;
        ModelDoc2? swDoc = null;
        try
        {
            swApp = ConnectToSolidWorks();
            swDoc = swApp?.IActiveDoc2;
            if (swApp == null || swDoc == null) throw new InvalidOperationException("No active SolidWorks document.");
            ExecuteTask(swDoc, Array.Exists(args, a => string.Equals(a, "--apply", StringComparison.OrdinalIgnoreCase)));
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

    private static void ExecuteTask(ModelDoc2 swDoc, bool apply)
    {
        if (swDoc.GetType() != (int)swDocumentTypes_e.swDocDRAWING)
            throw new InvalidOperationException("This tool requires an active .slddrw drawing document.");

        var candidates = CollectCandidates((DrawingDoc)swDoc);
        var categories = new[] { "Dimensions", "Balloons", "Centerlines", "Centermarks" };

        Console.WriteLine($"Active drawing: {swDoc.GetPathName() ?? swDoc.GetTitle()}");
        Console.WriteLine(apply ? "Deleting dangling annotations:" : "Dry run — no annotations will be deleted:");
        foreach (var category in categories)
            Console.WriteLine($"  {category}: {candidates.FindAll(c => c.Category == category).Count}");

        if (!apply)
        {
            Console.WriteLine("Run with --apply to delete these dangling annotations.");
            return;
        }

        var deleted = CreateCounters(categories);
        var failed = CreateCounters(categories);
        foreach (var candidate in candidates)
        {
            swDoc.ClearSelection2(true);
            bool selected = candidate.Annotation.Select3(false, null);
            bool removed = selected && swDoc.Extension.DeleteSelection2(0);
            if (removed) deleted[candidate.Category]++;
            else
            {
                failed[candidate.Category]++;
                Console.WriteLine($"  Failed: {candidate.Category} in view '{candidate.ViewName}'.");
            }
        }

        swDoc.ClearSelection2(true);
        swDoc.GraphicsRedraw2();
        Console.WriteLine("Deletion results:");
        foreach (var category in categories)
            Console.WriteLine($"  {category}: deleted {deleted[category]}, failed {failed[category]}");
        Console.WriteLine("The drawing was not saved.");
    }

    private static List<Candidate> CollectCandidates(DrawingDoc drawing)
    {
        var result = new List<Candidate>();
        var sheetViews = drawing.GetViews() as object[] ?? Array.Empty<object>();
        foreach (var sheet in sheetViews)
        {
            var views = sheet as object[] ?? Array.Empty<object>();
            foreach (var item in views)
            {
                if (item is not SolidWorks.Interop.sldworks.View view) continue;
                for (Annotation? annotation = view.GetFirstAnnotation3(); annotation != null; annotation = annotation.GetNext3())
                {
                    if (!annotation.IsDangling()) continue;
                    string? category = GetSupportedCategory(annotation);
                    if (category != null) result.Add(new Candidate(annotation, category, view.GetName2()));
                }
            }
        }
        return result;
    }

    private static string? GetSupportedCategory(Annotation annotation)
    {
        return annotation.GetType() switch
        {
            (int)swAnnotationType_e.swDisplayDimension => "Dimensions",
            (int)swAnnotationType_e.swCenterLine => "Centerlines",
            (int)swAnnotationType_e.swCenterMarkSym => "Centermarks",
            (int)swAnnotationType_e.swNote when IsBalloon(annotation) => "Balloons",
            _ => null
        };
    }

    private static bool IsBalloon(Annotation annotation)
    {
        try
        {
            var note = annotation.GetSpecificAnnotation() as Note;
            return note != null && note.GetBalloonStyle() != (int)swBalloonStyle_e.swBS_None;
        }
        catch (COMException) { return false; }
    }

    private static Dictionary<string, int> CreateCounters(IEnumerable<string> categories)
    {
        var counters = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var category in categories) counters[category] = 0;
        return counters;
    }

    private static SldWorks? ConnectToSolidWorks()
    {
        try { return Interaction.GetObject(string.Empty, "SldWorks.Application") as SldWorks; }
        catch (COMException) { return null; }
    }
}
