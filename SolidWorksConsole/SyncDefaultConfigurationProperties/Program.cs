using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using SolidWorks.Interop.sldworks;

namespace SyncDefaultConfigurationProperties;

internal static class Program
{
    [STAThread]
    private static int Main()
    {
        SldWorks? app = null;
        ModelDoc2? doc = null;
        try
        {
            app = Interaction.GetObject(string.Empty, "SldWorks.Application") as SldWorks
                ?? throw new InvalidOperationException("SolidWorks is not running.");
            doc = app.IActiveDoc2 ?? throw new InvalidOperationException("No active SolidWorks document.");
            dynamic sourceManager = doc.Extension.CustomPropertyManager["Default"];
            string[] sourceNames = sourceManager.GetNames() as string[] ?? Array.Empty<string>();
            if (sourceNames.Length == 0) throw new InvalidOperationException("Default has no configuration-level custom properties.");
            var source = new Dictionary<string, (int Type, string Raw)>(StringComparer.OrdinalIgnoreCase);
            foreach (string name in sourceNames)
            {
                sourceManager.Get6(name, false, out string raw, out string resolved, out bool wasResolved, out bool linked);
                source[name] = (sourceManager.GetType2(name), raw);
            }
            var failures = new List<string>();
            foreach (string configuration in doc.GetConfigurationNames() as string[] ?? Array.Empty<string>())
            {
                if (string.Equals(configuration, "Default", StringComparison.OrdinalIgnoreCase)) continue;
                dynamic targetManager = doc.Extension.CustomPropertyManager[configuration];
                foreach (string name in targetManager.GetNames() as string[] ?? Array.Empty<string>())
                {
                    if (!source.ContainsKey(name) && (int)targetManager.Delete2(name) != 0) failures.Add($"{configuration}: could not remove extra {name}");
                }
                foreach (var item in source)
                {
                    if ((int)targetManager.Add3(item.Key, item.Value.Type, item.Value.Raw, 1) != 0) failures.Add($"{configuration}: could not set {item.Key}");
                }
                Console.WriteLine($"Synchronized {configuration}: {source.Count} properties");
            }
            if (failures.Count > 0) throw new InvalidOperationException(string.Join("; ", failures));
            Console.WriteLine($"Default source properties: {source.Count}");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine("ERROR: " + ex.Message); return 1; }
        finally { if (doc != null) Marshal.ReleaseComObject(doc); if (app != null) Marshal.ReleaseComObject(app); }
    }
}
