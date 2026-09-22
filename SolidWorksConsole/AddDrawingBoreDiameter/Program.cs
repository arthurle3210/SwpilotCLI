using System;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using SolidWorks.Interop.sldworks;
using SolidWorks.Interop.swconst;
using SwView = SolidWorks.Interop.sldworks.View;
namespace SolidWorksConsole;
internal static class Program
{
 [STAThread] private static int Main()
 {
  SldWorks? app=null;ModelDoc2? doc=null;
  try
  {
   app=Interaction.GetObject(string.Empty,"SldWorks.Application") as SldWorks;if(app==null)return 2;doc=app.IActiveDoc2;if(doc==null)return 3;
   DrawingDoc drawing=(DrawingDoc)doc;SwView? bestView=null;Edge? bestEdge=null;double bestRadius=double.MaxValue;
   for(SwView? view=drawing.GetFirstView() as SwView;view!=null;view=view.GetNextView() as SwView)
   {
    if(view.GetVisibleComponents() is not object[] components)continue;
    foreach(object co in components){Component2 component=(Component2)co;if(view.GetVisibleEntities2(component,(int)swViewEntityType_e.swViewEntityType_Edge) is not object[] entities)continue;foreach(object eo in entities){if(eo is not Edge edge)continue;Curve curve=(Curve)edge.GetCurve();if(curve==null||!curve.IsCircle()||curve.CircleParams is not double[] cp||cp.Length<7)continue;if(cp[6]<bestRadius){bestRadius=cp[6];bestEdge=edge;bestView=view;}}}
   }
   if(bestEdge==null||bestView==null)throw new InvalidOperationException("No visible circular bore edge was found.");doc.ClearSelection2(true);if(!((Entity)bestEdge).Select4(false,null))throw new InvalidOperationException("Could not select the bore edge.");
   double[] p=(double[])bestView.Position;DisplayDimension dim=(DisplayDimension)doc.AddDiameterDimension2(p[0]+0.035,p[1]+0.015,0);if(dim==null)throw new InvalidOperationException("Diameter dimension was not created.");
   doc.EditRebuild3();doc.GraphicsRedraw2();Console.WriteLine($"Added bore diameter dimension to '{bestView.Name}': {bestRadius*2*1000:F6} mm.");return 0;
  }
  catch(Exception ex){Console.Error.WriteLine($"Error: {ex.Message}");return 1;}finally{if(doc!=null)Marshal.ReleaseComObject(doc);if(app!=null)Marshal.ReleaseComObject(app);}
 }
}
