using System;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using SolidWorks.Interop.sldworks;
namespace SolidWorksConsole;
internal static class Program
{
 [STAThread] private static int Main()
 {
  SldWorks? app=null;ModelDoc2? doc=null;
  try
  {
   app=Interaction.GetObject(string.Empty,"SldWorks.Application") as SldWorks;if(app==null)return 2;doc=app.IActiveDoc2;if(doc==null)return 3;
   foreach(string scope in new[]{"","0.5UNC","0.5UNF"})
   {
    dynamic manager=doc.Extension.CustomPropertyManager[scope];object namesObject=manager.GetNames();string[] names=namesObject as string[]??Array.Empty<string>();
    Console.WriteLine($"Scope: {(scope==""?"Document":scope)}");if(names.Length==0)Console.WriteLine("  (no properties)");
    foreach(string name in names){string raw="",resolved="";bool wasResolved=false,link=false;manager.Get6(name,false,out raw,out resolved,out wasResolved,out link);Console.WriteLine($"  {name} = {raw} | resolved = {resolved}");}
   }
   return 0;
  }
  catch(Exception ex){Console.Error.WriteLine($"Error: {ex.Message}");return 1;}finally{if(doc!=null)Marshal.ReleaseComObject(doc);if(app!=null)Marshal.ReleaseComObject(app);}
 }
}
