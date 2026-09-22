using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

internal static class Program
{
    [STAThread]
    private static int Main()
    {
        SldWorks? swApp = null;
        ModelDoc2? doc = null;
        try
        {
            swApp = Interaction.GetObject(string.Empty, "SldWorks.Application") as SldWorks
                ?? throw new InvalidOperationException("SolidWorks is not running.");
            doc = swApp.IActiveDoc2 ?? throw new InvalidOperationException("No active document.");
            if (doc.GetType() != (int)swDocumentTypes_e.swDocPART)
                throw new InvalidOperationException("The active document is not a part.");

            var part = (IPartDoc)doc;
            object raw = part.GetPartBox(false);
            var box = (double[])raw;
            if (box.Length < 6) throw new InvalidOperationException("Unexpected bounding-box result.");

            double x = Math.Abs(box[3] - box[0]);
            double y = Math.Abs(box[4] - box[1]);
            double z = Math.Abs(box[5] - box[2]);
            Console.WriteLine($"X={x:G17}, Y={y:G17}, Z={z:G17}");
            Console.WriteLine($"Length (X): {x:G17}");
            Console.WriteLine($"Width (Y): {y:G17}");
            Console.WriteLine($"Thickness (Z): {z:G17}");
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
            if (swApp != null) Marshal.ReleaseComObject(swApp);
        }
    }
}
