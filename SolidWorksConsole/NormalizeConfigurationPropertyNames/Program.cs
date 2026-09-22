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
        SldWorks? app = null;
        ModelDoc2? doc = null;
        try
        {
            app = Interaction.GetObject(string.Empty, "SldWorks.Application") as SldWorks;
            doc = app?.IActiveDoc2 ?? throw new InvalidOperationException("No active SolidWorks document.");

            string[] configurations = (string[])doc.GetConfigurationNames();
            if (!configurations.Any(name => string.Equals(name, "8\"", StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Source configuration '8\"' was not found.");

            dynamic source = doc.Extension.CustomPropertyManager["8\""];
            string[] sourceNames = source.GetNames() as string[] ?? Array.Empty<string>();
            if (sourceNames.Length == 0)
                throw new InvalidOperationException("Source configuration '8\"' has no custom properties.");

            var sourceTypes = sourceNames.ToDictionary(
                name => name,
                name =>
                {
                    int type = (int)source.GetType2(name);
                    return type == (int)swCustomInfoType_e.swCustomInfoUnknown
                        ? (int)swCustomInfoType_e.swCustomInfoText
                        : type;
                },
                StringComparer.OrdinalIgnoreCase);
            int added = 0;

            foreach (string configuration in configurations)
            {
                if (string.Equals(configuration, "8\"", StringComparison.OrdinalIgnoreCase)) continue;
                dynamic target = doc.Extension.CustomPropertyManager[configuration];
                string[] targetNames = target.GetNames() as string[] ?? Array.Empty<string>();
                var existing = new HashSet<string>(targetNames, StringComparer.OrdinalIgnoreCase);

                foreach ((string name, int type) in sourceTypes)
                {
                    if (existing.Contains(name)) continue;
                    int result = (int)target.Add2(name, type, string.Empty);
                    if (result != 1 && result != -1)
                        throw new InvalidOperationException($"Could not add property '{name}' to '{configuration}'; API result {result}.");
                    existing.Add(name);
                    added++;
                    Console.WriteLine($"Added '{name}' to {configuration} (blank value).");
                }

                if (!sourceTypes.Keys.All(existing.Contains))
                    throw new InvalidOperationException($"Verification failed while normalizing '{configuration}'.");
            }

            doc.EditRebuild3();
            int errors = 0, warnings = 0;
            if (!doc.Save3((int)swSaveAsOptions_e.swSaveAsOptions_Silent, ref errors, ref warnings))
                throw new InvalidOperationException($"Save failed; errors={errors}, warnings={warnings}.");
            Console.WriteLine($"Verified {sourceNames.Length} property names across {configurations.Length} configurations; added {added} missing entries.");
            Console.WriteLine("Saved active document.");
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
