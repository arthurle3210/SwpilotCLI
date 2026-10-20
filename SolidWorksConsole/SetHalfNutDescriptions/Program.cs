using System;
using System.Collections.Generic;
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
   var values=new Dictionary<string,string>{{"0.5UNC","1/2UNC HEX NUT ZINC PLATED"},{"0.5UNF","1/2UNF HEX NUT ZINC PLATED"}};
   foreach(var item in values){CustomPropertyManager manager=doc.Extension.CustomPropertyManager[item.Key];int result=manager.Set2("Description",item.Value);if(result!=0)throw new InvalidOperationException($"Failed to set Description for {item.Key}; result={result}.");Console.WriteLine($"{item.Key}: Description = {item.Value}");}
   int errors=0,warnings=0;if(!doc.Save3((int)swSaveAsOptions_e.swSaveAsOptions_Silent,ref errors,ref warnings))throw new InvalidOperationException($"Save failed: errors={errors}, warnings={warnings}.");return 0;
  }
  catch(Exception ex){Console.Error.WriteLine($"Error: {ex.Message}");return 1;}finally{if(doc!=null)Marshal.ReleaseComObject(doc);if(app!=null)Marshal.ReleaseComObject(app);}
 }
}
