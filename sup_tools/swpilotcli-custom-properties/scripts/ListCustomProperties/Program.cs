using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using SolidWorks.Interop.sldworks;

namespace ListCustomProperties;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        SldWorks? swApp = null;
        ModelDoc2? swDoc = null;
        try
        {
            swApp = Interaction.GetObject(string.Empty, "SldWorks.Application") as SldWorks;
            if (swApp == null) return Fail("Unable to attach to SolidWorks.", 2);
            swDoc = swApp.IActiveDoc2;
            if (swDoc == null) return Fail("No active SolidWorks document.", 3);
            if (args.Length > 1) return Fail("Usage: ListCustomProperties [property_name]", 4);

            string? filter = args.Length == 1 ? args[0] : null;
            var scopes = new List<string> { string.Empty };
            if (swDoc.GetConfigurationNames() is string[] configurations)
                scopes.AddRange(configurations);

            int matches = 0;
            foreach (string scope in scopes)
            {
                dynamic manager = swDoc.Extension.CustomPropertyManager[scope];
                string[] names = manager.GetNames() as string[] ?? Array.Empty<string>();
                var selected = names.Where(name => filter == null || string.Equals(name, filter, StringComparison.OrdinalIgnoreCase)).ToArray();
                if (selected.Length == 0) continue;

                Console.WriteLine($"Scope: {(scope.Length == 0 ? "Document" : scope)}");
                foreach (string name in selected)
                {
                    string raw = string.Empty, resolved = string.Empty;
                    bool wasResolved = false, linkToProperty = false;
                    manager.Get6(name, false, out raw, out resolved, out wasResolved, out linkToProperty);
                    Console.WriteLine($"  {name} = {raw} | resolved = {resolved}");
                    matches++;
                }
            }

            Console.WriteLine($"Total matching properties: {matches}");
            return 0;
        }
        catch (Exception ex) { return Fail(ex.Message, 1); }
        finally
        {
            if (swDoc != null) Marshal.ReleaseComObject(swDoc);
            if (swApp != null) Marshal.ReleaseComObject(swApp);
        }
    }

    private static int Fail(string message, int code)
    {
        Console.Error.WriteLine($"ERROR: {message}");
        return code;
    }
}
