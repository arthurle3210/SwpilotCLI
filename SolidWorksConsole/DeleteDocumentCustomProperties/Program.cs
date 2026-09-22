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
  SldWorks? app=null;ModelDoc2? doc=null;
  try
  {
   app=Interaction.GetObject(string.Empty,"SldWorks.Application") as SldWorks;if(app==null)return 2;doc=app.IActiveDoc2;if(doc==null)return 3;
   CustomPropertyManager manager=doc.Extension.CustomPropertyManager[string.Empty];string[] names=manager.GetNames() as string[]??Array.Empty<string>();Console.WriteLine($"Document-level properties found: {names.Length}");
   foreach(string name in names){int result=manager.Delete2(name);if(result!=0)throw new InvalidOperationException($"Failed to delete '{name}'; result={result}.");Console.WriteLine($"Deleted: {name}");}
   int errors=0,warnings=0;if(!doc.Save3((int)swSaveAsOptions_e.swSaveAsOptions_Silent,ref errors,ref warnings))throw new InvalidOperationException($"Save failed: errors={errors}, warnings={warnings}.");return 0;
  }
  catch(Exception ex){Console.Error.WriteLine($"Error: {ex.Message}");return 1;}finally{if(doc!=null)Marshal.ReleaseComObject(doc);if(app!=null)Marshal.ReleaseComObject(app);}
 }
}
