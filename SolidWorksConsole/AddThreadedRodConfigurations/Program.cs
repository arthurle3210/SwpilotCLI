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
        SldWorks? app = null;
        ModelDoc2? doc = null;
        try
        {
            app = Interaction.GetObject(string.Empty, "SldWorks.Application") as SldWorks;
            doc = app?.IActiveDoc2 ?? throw new InvalidOperationException("No active SolidWorks document.");

            string original = doc.IGetActiveConfiguration().Name;
            string[] configs = (string[])doc.GetConfigurationNames();
            Console.WriteLine($"Document: {doc.GetTitle()}");
            Console.WriteLine($"Active configuration: {original}");
            Console.WriteLine("Configurations: " + string.Join(", ", configs));

            PartDoc part = (PartDoc)doc;
            if (args.Length > 0 && args[0].Equals("apply", StringComparison.OrdinalIgnoreCase))
            {
                Dimension length = doc.Parameter("D1@Boss-Extrude1") as Dimension
                    ?? throw new InvalidOperationException("Length dimension D1@Boss-Extrude1 was not found.");
                var requested = new (string Name, double Inches)[]
                {
                    ("5\"", 5.0),
                    ("5.25\"", 5.25),
                    ("5.5\"", 5.5),
                    ("5.75\"", 5.75),
                    ("6\"", 6.0),
                    ("6.25\"", 6.25)
                };

                if (!doc.ShowConfiguration2("4\""))
                    throw new InvalidOperationException("Could not activate the 4-inch baseline configuration.");

                foreach ((string name, double inches) in requested)
                {
                    if (doc.GetConfigurationByName(name) == null && doc.AddConfiguration3(name, $"{inches:g} inch threaded rod", "", 0) == null)
                        throw new InvalidOperationException($"Could not create configuration {name}.");

                    int result = length.SetSystemValue3(inches * 0.0254,
                        (int)swSetValueInConfiguration_e.swSetValue_InSpecificConfigurations, new[] { name });
                    if (result != 0)
                        throw new InvalidOperationException($"Could not set length for {name}; API result {result}.");
                }

                foreach ((string name, double inches) in requested)
                {
                    if (!doc.ShowConfiguration2(name))
                        throw new InvalidOperationException($"Could not activate {name} for verification.");
                    doc.EditRebuild3();
                    double[] values = (double[])length.GetSystemValue3(3, new[] { name });
                    double[] box = (double[])part.GetPartBox(true);
                    Console.WriteLine($"Verified {name}: dimension={values[0] / 0.0254:F3} in, Z extent={(box[5] - box[2]) / 0.0254:F3} in");
                }

                if (!doc.ShowConfiguration2(original))
                    throw new InvalidOperationException($"Could not restore original configuration {original}.");
                if (!double.TryParse(original.TrimEnd('"'), System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out double originalInches))
                    throw new InvalidOperationException($"Could not derive the original rod length from configuration name {original}.");
                int restoreOriginal = length.SetSystemValue3(originalInches * 0.0254,
                    (int)swSetValueInConfiguration_e.swSetValue_InThisConfiguration, null);
                if (restoreOriginal != 0)
                    throw new InvalidOperationException($"Could not restore original configuration {original}; API result {restoreOriginal}.");
                doc.EditRebuild3();
                double[] originalBox = (double[])part.GetPartBox(true);
                Console.WriteLine($"Restored {original}: Z extent={(originalBox[5] - originalBox[2]) / 0.0254:F3} in");
                int errors = 0, warnings = 0;
                if (!doc.Save3((int)swSaveAsOptions_e.swSaveAsOptions_Silent, ref errors, ref warnings))
                    throw new InvalidOperationException($"Save failed; errors={errors}, warnings={warnings}.");
                Console.WriteLine("Saved active document.");
                return 0;
            }

            foreach (string config in configs)
            {
                doc.ShowConfiguration2(config);
                doc.EditRebuild3();
                double[] box = (double[])part.GetPartBox(true);
                Console.WriteLine($"Extent {config}: X={(box[3] - box[0]) * 1000:F3}, Y={(box[4] - box[1]) * 1000:F3}, Z={(box[5] - box[2]) * 1000:F3} mm");
            }

            for (Feature? feature = doc.FirstFeature(); feature != null; feature = feature.GetNextFeature())
            {
                DisplayDimension? display = feature.GetFirstDisplayDimension() as DisplayDimension;
                while (display != null)
                {
                    Dimension? dimension = display.GetDimension2(0) as Dimension;
                    if (dimension != null)
                    {
                        Console.Write($"{dimension.FullName}");
                        foreach (string config in configs)
                        {
                            double[] values = (double[])dimension.GetSystemValue3(3, new[] { config });
                            Console.Write($" | {config}={values[0] * 1000:F6} mm");
                        }
                        Console.WriteLine();
                    }
                    display = feature.GetNextDisplayDimension(display) as DisplayDimension;
                }
            }

            doc.ShowConfiguration2(original);
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
