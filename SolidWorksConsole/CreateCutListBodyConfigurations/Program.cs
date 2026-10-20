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
    [STAThread]
    private static int Main(string[] args)
    {
        SldWorks? swApp = null;
        ModelDoc2? swDoc = null;
        try
        {
            swApp = Interaction.GetObject(string.Empty, "SldWorks.Application") as SldWorks
                ?? throw new InvalidOperationException("Unable to attach to SolidWorks.");
            swDoc = swApp.IActiveDoc2 ?? throw new InvalidOperationException("No active SolidWorks document.");
            ExecuteTask(swDoc, args);
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

    private static void ExecuteTask(ModelDoc2 swDoc, string[] args)
    {
        if (swDoc.GetType() != (int)swDocumentTypes_e.swDocPART)
            throw new InvalidOperationException("The active document must be a part.");

        if (args.Length == 2 && string.Equals(args[0], "create", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(args[1], out int createIndex) && createIndex is >= 22 and <= 39)
        {
            if (!EnsureConfiguration(swDoc, "Default") || !swDoc.EditRebuild3())
                throw new InvalidOperationException("Default did not rebuild successfully before feature creation.");
            var representatives = GetCutListRepresentatives(swDoc);
            if (representatives.Count != 39) throw new InvalidOperationException($"Expected 39 cut-list items, found {representatives.Count}.");
            string config = $"body{createIndex:D2}";
            string featureName = $"KeepBody_body{createIndex:D2}";
            if (FindFeature(swDoc, featureName) != null)
            {
                Console.WriteLine($"{featureName} already exists; no duplicate created.");
                return;
            }
            swDoc.ClearSelection2(true);
            if (!representatives[createIndex - 1].Body.Select2(false, null))
                throw new InvalidOperationException($"Could not select the representative body for {representatives[createIndex - 1].Name}.");
            Feature? feature = swDoc.FeatureManager.InsertDeleteBody2(true) ?? throw new InvalidOperationException($"Could not create {featureName}.");
            feature.Name = featureName;
            if (!feature.SetSuppression2((int)swFeatureSuppressionAction_e.swSuppressFeature,
                    (int)swInConfigurationOpts_e.swAllConfiguration, null))
                throw new InvalidOperationException($"Could not suppress {featureName} in all configurations.");
            swDoc.ClearSelection2(true);
            swDoc.ShowConfiguration2(config);
            System.Threading.Thread.Sleep(3000);
            if (!swDoc.EditRebuild3()) throw new InvalidOperationException($"{config} did not rebuild successfully after switch.");
            if (!feature.SetSuppression2((int)swFeatureSuppressionAction_e.swUnSuppressFeature,
                    (int)swInConfigurationOpts_e.swThisConfiguration, null))
                throw new InvalidOperationException($"Could not unsuppress {featureName} in {config}.");
            if (!swDoc.EditRebuild3()) throw new InvalidOperationException($"{config} did not rebuild successfully after feature update.");
            System.Threading.Thread.Sleep(2000);
            Console.WriteLine($"Created {featureName}; {config} rebuilt successfully before and after unsuppression.");
            return;
        }

        if (args.Length == 2 && string.Equals(args[0], "named", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(args[1], out int namedIndex) && namedIndex is >= 24 and <= 39)
        {
            if (!string.Equals(swDoc.IGetActiveConfiguration().Name, "Default", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Activate Default before creating the next feature.");
            if (!swDoc.EditRebuild3()) throw new InvalidOperationException("Default did not rebuild successfully.");
            string config = $"body{namedIndex:D2}";
            string featureName = $"KeepBody_body{namedIndex:D2}";
            Feature? feature = FindFeature(swDoc, featureName);
            if (feature != null)
            {
                if (!feature.SetSuppression2((int)swFeatureSuppressionAction_e.swSuppressFeature, (int)swInConfigurationOpts_e.swAllConfiguration, null))
                    throw new InvalidOperationException($"Could not suppress {featureName} in all configurations.");
                System.Threading.Thread.Sleep(3000);
                if (!feature.SetSuppression2((int)swFeatureSuppressionAction_e.swUnSuppressFeature, (int)swInConfigurationOpts_e.swSpecifyConfiguration, new object[] { config }))
                    throw new InvalidOperationException($"Could not map {featureName} to {config}.");
                if (!swDoc.EditRebuild3()) throw new InvalidOperationException("Default rebuild did not complete.");
                System.Threading.Thread.Sleep(2000);
                Console.WriteLine($"Mapped existing {featureName} only to {config}; Default rebuild completed.");
                return;
            }
            var representatives = GetCutListRepresentatives(swDoc);
            if (representatives.Count != 39) throw new InvalidOperationException($"Expected 39 cut-list items, found {representatives.Count}.");
            swDoc.ClearSelection2(true);
            if (!representatives[namedIndex - 1].Body.Select2(false, null)) throw new InvalidOperationException($"Could not select representative for {featureName}.");
            feature = swDoc.FeatureManager.InsertDeleteBody2(true) ?? throw new InvalidOperationException($"Could not create {featureName}.");
            feature.Name = featureName;
            if (!feature.SetSuppression2((int)swFeatureSuppressionAction_e.swSuppressFeature, (int)swInConfigurationOpts_e.swAllConfiguration, null))
                throw new InvalidOperationException($"Could not suppress {featureName} in all configurations.");
            System.Threading.Thread.Sleep(3000);
            if (!feature.SetSuppression2((int)swFeatureSuppressionAction_e.swUnSuppressFeature, (int)swInConfigurationOpts_e.swSpecifyConfiguration, new object[] { config }))
                throw new InvalidOperationException($"Could not map {featureName} to {config}.");
            if (!swDoc.EditRebuild3()) throw new InvalidOperationException("Default rebuild did not complete.");
            System.Threading.Thread.Sleep(2000);
            Console.WriteLine($"Created {featureName}; mapped only to {config}; Default rebuild completed.");
            return;
        }

        if (args.Length == 1 && string.Equals(args[0], "one", StringComparison.OrdinalIgnoreCase))
        {
            swDoc.ShowConfiguration2("body01");
            swDoc.EditRebuild3();
            Feature? one = FindFeature(swDoc, "KeepBody_body01") ?? throw new InvalidOperationException("KeepBody_body01 was not found.");
            if (!one.SetSuppression2((int)swFeatureSuppressionAction_e.swUnSuppressFeature,
                    (int)swInConfigurationOpts_e.swThisConfiguration, null))
                throw new InvalidOperationException("Could not unsuppress KeepBody_body01 in body01.");
            swDoc.EditRebuild3();
            Console.WriteLine("Activated body01 and unsuppressed KeepBody_body01.");
            return;
        }

        if (args.Length == 1 && string.Equals(args[0], "nextfive", StringComparison.OrdinalIgnoreCase))
        {
            for (int i = 2; i <= 6; i++)
            {
                string config = $"body{i:D2}";
                string featureName = $"KeepBody_body{i:D2}";
                swDoc.ShowConfiguration2(config);
                swDoc.EditRebuild3();
                Feature? feature = FindFeature(swDoc, featureName) ?? throw new InvalidOperationException($"{featureName} was not found.");
                if (!feature.SetSuppression2((int)swFeatureSuppressionAction_e.swUnSuppressFeature,
                        (int)swInConfigurationOpts_e.swThisConfiguration, null))
                    throw new InvalidOperationException($"Could not unsuppress {featureName} in {config}.");
                swDoc.EditRebuild3();
                Console.WriteLine($"{config}: {featureName} unsuppressed; waiting 3 seconds.");
                System.Threading.Thread.Sleep(3000);
            }
            Console.WriteLine("Completed body02 through body06.");
            return;
        }

        if (args.Length == 2 && string.Equals(args[0], "single", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(args[1], out int index) && index is >= 1 and <= 39)
        {
            string config = $"body{index:D2}";
            string featureName = $"KeepBody_body{index:D2}";
            swDoc.ShowConfiguration2(config);
            swDoc.EditRebuild3();
            System.Threading.Thread.Sleep(3000);
            Feature? feature = FindFeature(swDoc, featureName) ?? throw new InvalidOperationException($"{featureName} was not found.");
            if (!feature.SetSuppression2((int)swFeatureSuppressionAction_e.swUnSuppressFeature,
                    (int)swInConfigurationOpts_e.swThisConfiguration, null))
                throw new InvalidOperationException($"Could not unsuppress {featureName} in {config}.");
            swDoc.EditRebuild3();
            System.Threading.Thread.Sleep(2000);
            Console.WriteLine($"Activated {config}; waited 3 seconds; {featureName} is unsuppressed; waited 2 seconds.");
            return;
        }

        if (!EnsureConfiguration(swDoc, "Default"))
            throw new InvalidOperationException($"Could not activate Default from '{swDoc.IGetActiveConfiguration().Name}'.");

        Feature? priorDeleteKeep = FindFeature(swDoc, "Body-Delete/Keep 1");
        if (priorDeleteKeep != null && !priorDeleteKeep.SetSuppression2((int)swFeatureSuppressionAction_e.swSuppressFeature,
                (int)swInConfigurationOpts_e.swAllConfiguration, null))
            throw new InvalidOperationException("Could not suppress the pre-existing Body-Delete/Keep 1 feature.");
        swDoc.EditRebuild3();

        int batchStart = 1, batchEnd = 39;
        bool timedBatch = args.Length == 3 && string.Equals(args[0], "timed", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(args[1], out batchStart) && int.TryParse(args[2], out batchEnd)
            && batchStart >= 1 && batchEnd <= 39 && batchStart <= batchEnd;
        int firstRepair = timedBatch ? batchStart : 1;
        int lastRepair = timedBatch ? batchEnd : 39;
        if ((args.Length == 1 && string.Equals(args[0], "repair", StringComparison.OrdinalIgnoreCase)) || timedBatch)
        {
        for (int i = firstRepair; i <= lastRepair; i++)
        {
            Feature? existingKeep = FindFeature(swDoc, $"KeepBody_body{i:D2}");
            if (existingKeep == null) continue;
            if (!existingKeep.SetSuppression2((int)swFeatureSuppressionAction_e.swSuppressFeature,
                    (int)swInConfigurationOpts_e.swAllConfiguration, null))
                throw new InvalidOperationException($"Could not suppress {existingKeep.Name} in all configurations.");
            if (!existingKeep.SetSuppression2((int)swFeatureSuppressionAction_e.swUnSuppressFeature,
                    (int)swInConfigurationOpts_e.swSpecifyConfiguration, new object[] { $"body{i:D2}" }))
                throw new InvalidOperationException($"Could not enable {existingKeep.Name} in body{i:D2}.");
            swDoc.EditRebuild3();
            if (timedBatch) System.Threading.Thread.Sleep(5000);
        }
        }
        swDoc.EditRebuild3();
        if ((args.Length == 1 && string.Equals(args[0], "repair", StringComparison.OrdinalIgnoreCase)) || timedBatch)
        {
            Console.WriteLine($"Mapped KeepBody_body{firstRepair:D2} through KeepBody_body{lastRepair:D2} to matching configurations.");
            return;
        }

        var part = (PartDoc)swDoc;
        var cutLists = GetCutListRepresentatives(swDoc);
        if (cutLists.Count != 39)
            throw new InvalidOperationException($"Expected 39 non-empty cut-list items, found {cutLists.Count}; no changes made.");
        if ((part.GetBodies2((int)swBodyType_e.swSolidBody, false) as object[] ?? Array.Empty<object>()).Length != 84)
            throw new InvalidOperationException("Default does not contain the expected 84 solid bodies; no changes made.");

        var existing = ((string[]?)swDoc.GetConfigurationNames() ?? Array.Empty<string>()).ToHashSet(StringComparer.OrdinalIgnoreCase);

        for (int i = 1; i <= 39; i++)
        {
            string config = $"body{i:D2}";
            if (!existing.Contains(config) && swDoc.AddConfiguration3(config, $"Representative body from {cutLists[i - 1].Name}", "", 0) == null)
                throw new InvalidOperationException($"Could not create {config}.");
        }
        if (!EnsureConfiguration(swDoc, "Default"))
            throw new InvalidOperationException("Could not return to Default.");

        if (args.Length == 1 && int.TryParse(args[0], out int requestedIndex))
        {
            if (requestedIndex is < 1 or > 39) throw new InvalidOperationException("Feature index must be from 1 to 39.");
            CreateKeepFeature(swDoc, cutLists[requestedIndex - 1], requestedIndex);
            EnsureConfiguration(swDoc, "Default");
            return;
        }

        if (args.Length == 1 && string.Equals(args[0], "configs", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("All body01-body39 configurations are present. No features were created.");
            return;
        }

        for (int i = 1; i <= 39; i++)
            CreateKeepFeature(swDoc, cutLists[i - 1], i);

        swDoc.EditRebuild3();
        for (int i = 1; i <= 39; i++)
        {
            string config = $"body{i:D2}";
            if (!EnsureConfiguration(swDoc, config))
                throw new InvalidOperationException($"Could not activate {config} for verification.");
            swDoc.EditRebuild3();
            int count = (part.GetBodies2((int)swBodyType_e.swSolidBody, false) as object[] ?? Array.Empty<object>()).Length;
            if (count != 1)
                throw new InvalidOperationException($"Verification failed for {config}: expected 1 solid body, found {count}.");
            Console.WriteLine($"Verified {config}: 1 solid body");
        }
        if (!EnsureConfiguration(swDoc, "Default"))
            throw new InvalidOperationException("Could not restore Default.");
        swDoc.EditRebuild3();
        int defaultCount = (part.GetBodies2((int)swBodyType_e.swSolidBody, false) as object[] ?? Array.Empty<object>()).Length;
        if (defaultCount != 84)
            throw new InvalidOperationException($"Default verification failed: expected 84 bodies, found {defaultCount}.");
        Console.WriteLine("Verified Default: 84 solid bodies. Changes are not saved.");
    }

    private static void CreateKeepFeature(ModelDoc2 swDoc, (string Name, Body2 Body) item, int index)
    {
        string config = $"body{index:D2}";
        if (FindFeature(swDoc, $"KeepBody_{config}") != null)
        {
            Console.WriteLine($"Keep Bodies feature already exists for {config}.");
            return;
        }
        swDoc.ClearSelection2(true);
        if (!item.Body.Select2(false, null))
            throw new InvalidOperationException($"Could not select the representative body for {item.Name}.");
        Feature? keep = swDoc.FeatureManager.InsertDeleteBody2(true);
        if (keep == null)
            throw new InvalidOperationException($"Could not create Keep Bodies feature for {item.Name}.");
        keep.Name = $"KeepBody_{config}";
        if (!keep.SetSuppression2((int)swFeatureSuppressionAction_e.swSuppressFeature, (int)swInConfigurationOpts_e.swAllConfiguration, null))
            throw new InvalidOperationException($"Could not suppress {keep.Name} in all configurations.");
        if (!keep.SetSuppression2((int)swFeatureSuppressionAction_e.swUnSuppressFeature,
                (int)swInConfigurationOpts_e.swSpecifyConfiguration, new object[] { config }))
            throw new InvalidOperationException($"Could not enable {keep.Name} in {config}.");
        swDoc.ClearSelection2(true);
        Console.WriteLine($"Created {config}: {item.Name}");
    }

    private static bool EnsureConfiguration(ModelDoc2 doc, string name)
    {
        if (string.Equals(doc.IGetActiveConfiguration().Name, name, StringComparison.OrdinalIgnoreCase)) return true;
        doc.ShowConfiguration2(name);
        System.Threading.Thread.Sleep(3000);
        doc.EditRebuild3();
        return string.Equals(doc.IGetActiveConfiguration().Name, name, StringComparison.OrdinalIgnoreCase);
    }

    private static Feature? FindFeature(ModelDoc2 doc, string name)
    {
        for (Feature? feature = doc.FirstFeature(); feature != null; feature = feature.GetNextFeature())
            if (string.Equals(feature.Name, name, StringComparison.OrdinalIgnoreCase)) return feature;
        return null;
    }

    private static List<(string Name, Body2 Body)> GetCutListRepresentatives(ModelDoc2 doc)
    {
        var items = new List<(string Name, Body2 Body)>();
        for (Feature? top = doc.FirstFeature(); top != null; top = top.GetNextFeature())
        {
            if (top.GetTypeName2() != "SolidBodyFolder") continue;
            for (Feature? child = top.GetFirstSubFeature() as Feature; child != null; child = child.GetNextSubFeature() as Feature)
            {
                if (child.GetTypeName2() != "CutListFolder" || child.GetSpecificFeature2() is not BodyFolder folder || folder.GetBodyCount() < 1) continue;
                if (folder.GetBodies() is not object[] bodies || bodies.Length == 0 || bodies[0] is not Body2 body)
                    throw new InvalidOperationException($"Could not get a body from cut-list item {child.Name}.");
                items.Add((child.Name, body));
            }
        }
        return items;
    }
}
