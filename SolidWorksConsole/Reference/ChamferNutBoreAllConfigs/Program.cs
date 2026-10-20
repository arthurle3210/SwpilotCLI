using System;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
namespace SolidWorksConsole;
internal static class Program
{
 [STAThread] private static int Main()
 {
  SldWorks? app=null; ModelDoc2? doc=null;
  try
  {
   app=Interaction.GetObject(string.Empty,"SldWorks.Application") as SldWorks; if(app==null)return 2;
   doc=app.IActiveDoc2; if(doc==null)return 3;
   const double targetRadiusM=0.01666875/2.0, toleranceM=0.00005, chamferM=0.00079375;
   object[] bodies=(object[])((PartDoc)doc).GetBodies2((int)swBodyType_e.swSolidBody,true);
   doc.ClearSelection2(true); int selected=0;
   foreach(object bo in bodies) if(((Body2)bo).GetEdges() is object[] edges) foreach(object eo in edges)
   {
    Edge e=(Edge)eo; Curve c=(Curve)e.GetCurve();
    if(c==null||!c.IsCircle()||c.CircleParams is not double[] cp||cp.Length<7)continue;
    if(Math.Abs(cp[6]-targetRadiusM)>toleranceM)continue;
    if(((Entity)e).Select4(true,null))selected++;
   }
   if(selected!=2)throw new InvalidOperationException($"Expected 2 bore rim edges; selected {selected}.");
   doc.FeatureChamferType((short)swChamferType_e.swChamferAngleDistance,chamferM,Math.PI/4.0,false,0,0,0,0);
   doc.EditRebuild3(); Feature chamfer=(Feature)doc.FeatureByPositionReverse(0);
   if(chamfer==null||!chamfer.GetTypeName2().Contains("Chamfer",StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Chamfer was not created.");
   if(!chamfer.SetSuppression2((int)swFeatureSuppressionAction_e.swUnSuppressFeature,(int)swInConfigurationOpts_e.swAllConfiguration,null))throw new InvalidOperationException("Could not enable chamfer in all configurations.");
   foreach(string name in new[]{"0.75UNC","0.75UNF"}){doc.ShowConfiguration2(name);doc.EditRebuild3();Console.WriteLine($"{name}: chamfer rebuilt.");}
   doc.ShowConfiguration2("0.75UNC");doc.EditRebuild3();int errors=0,warnings=0;
   if(!doc.Save3((int)swSaveAsOptions_e.swSaveAsOptions_Silent,ref errors,ref warnings))throw new InvalidOperationException($"Save failed ({errors}, {warnings}).");
   Console.WriteLine($"Created {chamfer.Name}: 1/32 in x 45 deg on both bore edges."); return 0;
  }
  catch(Exception ex){Console.Error.WriteLine($"Error: {ex.Message}");return 1;}
  finally{if(doc!=null)Marshal.ReleaseComObject(doc);if(app!=null)Marshal.ReleaseComObject(app);}
 }
}
