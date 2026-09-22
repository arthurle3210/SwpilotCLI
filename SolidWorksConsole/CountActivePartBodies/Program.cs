using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace CountActivePartBodies;

internal static class Program
{
    [STAThread]
    private static int Main()
    {
        SldWorks? app = null;
        ModelDoc2? doc = null;
        try
        {
            app = Interaction.GetObject(string.Empty, "SldWorks.Application") as SldWorks;
            doc = app?.IActiveDoc2;
            if (app == null || doc == null) throw new InvalidOperationException("No active SolidWorks document.");
            if (doc.GetType() != (int)swDocumentTypes_e.swDocPART) throw new InvalidOperationException("The active document is not a part.");

            var part = (PartDoc)doc;
            int solids = (part.GetBodies2((int)swBodyType_e.swSolidBody, false) as object[] ?? Array.Empty<object>()).Length;
            int sheets = (part.GetBodies2((int)swBodyType_e.swSheetBody, false) as object[] ?? Array.Empty<object>()).Length;
            var cutLists = new List<(string Name, int Quantity)>();
            for (Feature? feature = doc.FirstFeature(); feature != null; feature = feature.GetNextFeature())
            {
                if (feature.GetTypeName2() != "SolidBodyFolder") continue;
                for (Feature? child = feature.GetFirstSubFeature() as Feature; child != null; child = child.GetNextSubFeature() as Feature)
                {
                    if (child.GetTypeName2() != "CutListFolder" || child.GetSpecificFeature2() is not BodyFolder folder) continue;
                    int quantity = folder.GetBodyCount();
                    if (quantity > 0) cutLists.Add((child.Name, quantity));
                }
            }
            Console.WriteLine($"Solid bodies: {solids}");
            Console.WriteLine($"Sheet/surface bodies: {sheets}");
            Console.WriteLine($"Total bodies: {solids + sheets}");
            Console.WriteLine($"Unique BOM cut-list items: {cutLists.Count}");
            Console.WriteLine($"Bodies represented by cut list: {cutLists.Sum(x => x.Quantity)}");
            Console.WriteLine("Configurations: " + string.Join(", ", (string[]?)doc.GetConfigurationNames() ?? Array.Empty<string>()));
            var keepFeatures = new List<string>();
            for (Feature? feature = doc.FirstFeature(); feature != null; feature = feature.GetNextFeature())
                if (feature.Name.Contains("Keep", StringComparison.OrdinalIgnoreCase) || feature.GetTypeName2().Contains("Delete", StringComparison.OrdinalIgnoreCase))
                    keepFeatures.Add($"{feature.Name} [{feature.GetTypeName2()}]");
            Console.WriteLine("Keep/Delete features: " + (keepFeatures.Count == 0 ? "none" : string.Join(", ", keepFeatures)));
            for (Feature? feature = doc.FirstFeature(); feature != null; feature = feature.GetNextFeature())
                if (feature.Name.Contains("Keep", StringComparison.OrdinalIgnoreCase) || feature.GetTypeName2().Contains("Delete", StringComparison.OrdinalIgnoreCase))
                {
                    object value = feature.IsSuppressed2((int)swInConfigurationOpts_e.swThisConfiguration, null);
                    string display = value is bool[] states ? string.Join(",", states.Select(x => x.ToString())) : value?.ToString() ?? "null";
                    Console.WriteLine($"  {feature.Name} suppressed-current={display}");
                }
            int mapped = 0;
            for (int i = 1; i <= 39; i++)
            {
                string name = $"KeepBody_body{i:D2}";
                Feature? feature = null;
                for (Feature? candidate = doc.FirstFeature(); candidate != null; candidate = candidate.GetNextFeature())
                    if (string.Equals(candidate.Name, name, StringComparison.OrdinalIgnoreCase)) { feature = candidate; break; }
                if (feature?.IsSuppressed2((int)swInConfigurationOpts_e.swSpecifyConfiguration, new object[] { $"body{i:D2}" }) is bool[] states && states.Length == 1 && !states[0]) mapped++;
            }
            Console.WriteLine($"Matching KeepBody features unsuppressed in their bodyNN configuration: {mapped}/39");
            foreach (var item in cutLists) Console.WriteLine($"  {item.Name}: qty {item.Quantity}");
            return 0;
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
    }

}
