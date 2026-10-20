using System;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
namespace SolidWorksConsole;
internal static class Program
{
 private static Feature? FindFeature(ModelDoc2 doc,string name){for(Feature? f=doc.FirstFeature();f!=null;f=f.GetNextFeature())if(f.Name==name)return f;return null;}
 [STAThread] private static int Main()
 {
  SldWorks? app=null;ModelDoc2? doc=null;
  try
  {
   app=Interaction.GetObject(string.Empty,"SldWorks.Application") as SldWorks;if(app==null)return 2;doc=app.IActiveDoc2;if(doc==null)return 3;
   Feature sf=FindFeature(doc,"Sketch2")??throw new InvalidOperationException("Sketch2 not found.");
   Dimension? bore=doc.Parameter("D1@Sketch2") as Dimension;
   if(bore==null)
   {
    doc.ClearSelection2(true);if(!sf.Select2(false,0))throw new InvalidOperationException("Could not select Sketch2.");doc.EditSketch();
    Sketch sk=(Sketch)sf.GetSpecificFeature2();object[] segs=(object[])sk.GetSketchSegments();if(segs.Length!=1)throw new InvalidOperationException($"Expected one circle; found {segs.Length} segments.");
    if(!((SketchSegment)segs[0]).Select4(false,null))throw new InvalidOperationException("Could not select bore circle.");
    DisplayDimension dd=(DisplayDimension)doc.AddDiameterDimension2(0.012,0,0);if(dd==null)throw new InvalidOperationException("Could not add bore diameter dimension.");
    bore=(Dimension)dd.GetDimension2(0);doc.EditRebuild3();
   }
   string original=doc.IGetActiveConfiguration().Name;
   if(doc.GetConfigurationByName("0.5UNC")==null&&doc.AddConfiguration3("0.5UNC","1/2-13 UNC, 27/64 tap-drill bore","",0)==null)throw new InvalidOperationException("Could not create 0.5UNC.");
   doc.ShowConfiguration2(original);
   if(doc.GetConfigurationByName("0.5UNF")==null&&doc.AddConfiguration3("0.5UNF","1/2-20 UNF, 29/64 tap-drill bore","",0)==null)throw new InvalidOperationException("Could not create 0.5UNF.");
   int unc=bore.SetSystemValue3(0.010715625,(int)swSetValueInConfiguration_e.swSetValue_InSpecificConfigurations,"0.5UNC");
   int unf=bore.SetSystemValue3(0.011509375,(int)swSetValueInConfiguration_e.swSetValue_InSpecificConfigurations,"0.5UNF");
   if(unc!=0||unf!=0)throw new InvalidOperationException($"Bore update failed: UNC={unc}, UNF={unf}.");
   doc.ShowConfiguration2("0.5UNC");if(original!="0.5UNC"&&original!="0.5UNF"&&!doc.DeleteConfiguration2(original))throw new InvalidOperationException($"Could not delete {original}.");
   Feature ch=FindFeature(doc,"Chamfer1")??throw new InvalidOperationException("Chamfer1 not found.");ch.SetSuppression2((int)swFeatureSuppressionAction_e.swUnSuppressFeature,(int)swInConfigurationOpts_e.swAllConfiguration,null);
   foreach(string name in new[]{"0.5UNC","0.5UNF"}){doc.ShowConfiguration2(name);doc.EditRebuild3();double[] v=(double[])bore.GetSystemValue3((int)swInConfigurationOpts_e.swSpecifyConfiguration,name);Console.WriteLine($"{name}: bore diameter={v[0]*1000:F6} mm");}
   doc.ShowConfiguration2("0.5UNC");doc.EditRebuild3();int errors=0,warnings=0;if(!doc.Save3((int)swSaveAsOptions_e.swSaveAsOptions_Silent,ref errors,ref warnings))throw new InvalidOperationException($"Save failed: {errors}, {warnings}.");return 0;
  }
  catch(Exception ex){Console.Error.WriteLine($"Error: {ex.Message}");return 1;}finally{if(doc!=null)Marshal.ReleaseComObject(doc);if(app!=null)Marshal.ReleaseComObject(app);}
 }
}
