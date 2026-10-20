using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace UpdateDrawingApprovalProperties;

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
            swDoc = swApp?.IActiveDoc2;
            if (swApp == null || swDoc == null)
                throw new InvalidOperationException("No active SolidWorks document.");

            bool apply = Array.Exists(args, a => string.Equals(a, "--apply", StringComparison.OrdinalIgnoreCase));
            ExecuteTask(swDoc, apply);
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
            throw new InvalidOperationException("This macro can only run on an active .slddrw drawing document.");

        var values = new (string Name, string Value)[]
        {
            ("DrawnBy", "B.M."),
            ("CheckedBy", "C.B."),
            ("ApprovedBy", "C.B."),
            ("DrawnDate", DateTime.Now.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture))
        };

        Console.WriteLine($"Active drawing: {swDoc.GetPathName() ?? swDoc.GetTitle()}");
        Console.WriteLine(apply ? "Applying document-level properties:" : "Dry run — no properties will be changed:");
        foreach (var (name, value) in values)
            Console.WriteLine($"  {name} = {value}");

        if (!apply)
        {
            Console.WriteLine("Run again with --apply to write these values.");
            return;
        }

        var manager = swDoc.Extension.CustomPropertyManager[string.Empty];
        foreach (var (name, value) in values)
        {
            int result = manager.Add3(
                name,
                (int)swCustomInfoType_e.swCustomInfoText,
                value,
                (int)swCustomPropertyAddOption_e.swCustomPropertyReplaceValue);
            Console.WriteLine($"  {name}: Add3 result {result}");
        }
        swDoc.GraphicsRedraw2();
        Console.WriteLine("Properties updated. The drawing was not saved.");
    }

    private static SldWorks? ConnectToSolidWorks()
    {
        try { return Interaction.GetObject(string.Empty, "SldWorks.Application") as SldWorks; }
        catch (COMException) { return null; }
    }
}
