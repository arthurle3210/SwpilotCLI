using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;

namespace CutListBodyConfigStep;
internal static class Program
{
 [STAThread] static int Main(string[] args)
 {
  SldWorks? app=null; ModelDoc2? doc=null;
  try { app=Interaction.GetObject("", "SldWorks.Application") as SldWorks ?? throw new InvalidOperationException("SolidWorks is not running."); doc=app.IActiveDoc2 ?? throw new InvalidOperationException("No active document."); Run(doc,args); return 0; }
  catch(Exception e){Console.Error.WriteLine("ERROR: "+e.Message);return 1;}
  finally {if(doc!=null)Marshal.ReleaseComObject(doc);if(app!=null)Marshal.ReleaseComObject(app);}
 }
 static void Run(ModelDoc2 doc,string[] args)
 {
  if(doc.GetType()!=(int)swDocumentTypes_e.swDocPART)throw new InvalidOperationException("Active document must be a part.");
  EnsureDefault(doc);
  var items=CutLists(doc); if(items.Count==0)throw new InvalidOperationException("No non-empty cut-list items under SolidBodyFolder.");
  int solids=((PartDoc)doc).GetBodies2((int)swBodyType_e.swSolidBody,false) is object[] b?b.Length:0;
  var roots=Features(doc).Where(f=>f.GetTypeName2()=="DeleteBody"&&!f.Name.StartsWith("KeepBody_",StringComparison.OrdinalIgnoreCase)).ToList();
  foreach(var root in roots) if(Suppressed(root))throw new InvalidOperationException($"Existing Delete/Keep Body feature '{root.Name}' is suppressed in Default; it must remain unsuppressed.");
  if(args.Length==0||args[0].Equals("dry-run",StringComparison.OrdinalIgnoreCase)){Console.WriteLine($"DRY-RUN: solids={solids}; cutListItems={items.Count}; existingDeleteKeep={roots.Count}; next={Next(doc,items.Count)}");return;}
  if(args.Length<2||!args[0].Equals("apply",StringComparison.OrdinalIgnoreCase)||!int.TryParse(args[1],out int n)||n<1||n>items.Count)throw new InvalidOperationException("Usage: [dry-run] | apply <index> [preDelaySeconds=3] [postDelaySeconds=2].");
  int pre=args.Length>2&&int.TryParse(args[2],out int p)?p:3, post=args.Length>3&&int.TryParse(args[3],out int q)?q:2; if(pre<0||post<0)throw new InvalidOperationException("Delays cannot be negative.");
  string cfg=$"body{n:D2}", name=$"KeepBody_body{n:D2}";
  if(doc.GetConfigurationByName(cfg)==null&&doc.AddConfiguration3(cfg,$"Representative cut-list item: {items[n-1].Name}","",0)==null)throw new InvalidOperationException($"Could not create {cfg}.");
  Feature? keep=Features(doc).FirstOrDefault(f=>string.Equals(f.Name,name,StringComparison.OrdinalIgnoreCase));
  if(keep==null){doc.ClearSelection2(true);if(!items[n-1].Body.Select2(false,null))throw new InvalidOperationException($"Could not select {items[n-1].Name}.");keep=doc.FeatureManager.InsertDeleteBody2(true)??throw new InvalidOperationException("Could not create Keep Bodies feature.");keep.Name=name;doc.ClearSelection2(true);}
  if(!keep.SetSuppression2((int)swFeatureSuppressionAction_e.swSuppressFeature,(int)swInConfigurationOpts_e.swAllConfiguration,null))throw new InvalidOperationException($"Could not suppress {name} in all configurations.");
  doc.ShowConfiguration2(cfg);Thread.Sleep(pre*1000);if(!string.Equals(doc.IGetActiveConfiguration().Name,cfg,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException($"SolidWorks did not activate {cfg}.");
  if(!keep.SetSuppression2((int)swFeatureSuppressionAction_e.swUnSuppressFeature,(int)swInConfigurationOpts_e.swThisConfiguration,null))throw new InvalidOperationException($"Could not unsuppress {name} in {cfg}.");
  if(!doc.EditRebuild3())throw new InvalidOperationException($"Rebuild did not complete in {cfg}.");Thread.Sleep(post*1000);
  int count=((PartDoc)doc).GetBodies2((int)swBodyType_e.swSolidBody,false) is object[] vb?vb.Length:0;if(count!=1)throw new InvalidOperationException($"Verification failed for {cfg}: expected 1 solid body, found {count}.");
  Console.WriteLine($"OK: {cfg}; feature={name}; retainedBodies=1; rebuild=true; saved=false");
 }
 static List<(string Name,Body2 Body,int Quantity)> CutLists(ModelDoc2 d){var r=new List<(string,Body2,int)>();foreach(var top in Features(d).Where(f=>f.GetTypeName2()=="SolidBodyFolder"))for(Feature? c=top.GetFirstSubFeature() as Feature;c!=null;c=c.GetNextSubFeature() as Feature)if(c.GetTypeName2()=="CutListFolder"&&c.GetSpecificFeature2() is BodyFolder bf&&bf.GetBodyCount()>0&&bf.GetBodies() is object[] a&&a.Length>0&&a[0] is Body2 b)r.Add((c.Name,b,bf.GetBodyCount()));return r;}
 static void EnsureDefault(ModelDoc2 d){if(IsFullDefault(d))return;d.ShowConfiguration2("Default");for(int i=0;i<5;i++){Thread.Sleep(3000);d.EditRebuild3();if(IsFullDefault(d))return;}throw new InvalidOperationException("SolidWorks did not restore Default with its full cut-list geometry.");}
 static bool IsFullDefault(ModelDoc2 d){var items=CutLists(d);int solids=((PartDoc)d).GetBodies2((int)swBodyType_e.swSolidBody,false) is object[] b?b.Length:0;return string.Equals(d.IGetActiveConfiguration().Name,"Default",StringComparison.OrdinalIgnoreCase)&&items.Count>1&&items.Sum(x=>x.Quantity)>0&&solids==items.Sum(x=>x.Quantity);}
 static List<Feature> Features(ModelDoc2 d){var r=new List<Feature>();for(Feature? f=d.FirstFeature();f!=null;f=f.GetNextFeature())r.Add(f);return r;}
 static bool Suppressed(Feature f)=>f.IsSuppressed2((int)swInConfigurationOpts_e.swThisConfiguration,null) is bool[] a&&a.Length>0&&a[0];
 static string Next(ModelDoc2 d,int n){var names=Features(d).Select(f=>f.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);for(int i=1;i<=n;i++)if(!names.Contains($"KeepBody_body{i:D2}"))return $"body{i:D2}";return "none";}
}
