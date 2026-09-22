using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using SolidWorks.Interop.sldworks;
namespace SolidWorksConsole;
internal static class Program
{
 [STAThread] private static int Main()
 {
  SldWorks? app=null;
  try
  {
   app=Interaction.GetObject(string.Empty,"SldWorks.Application") as SldWorks;if(app==null)return 2;
   string template=@"C:\EurovacSWV\Templates\Title Blocks\Part_ANSI_Inch_R01.prtdot";
   if(!File.Exists(template))throw new FileNotFoundException("Part template not found.",template);
   ModelDoc2? newDoc=app.NewDocument(template,0,0.0,0.0) as ModelDoc2;
   if(newDoc==null)throw new InvalidOperationException("SolidWorks NewDocument returned null.");
   Console.WriteLine($"Created new part: {newDoc.GetTitle()}");Marshal.ReleaseComObject(newDoc);return 0;
  }
  catch(Exception ex){Console.Error.WriteLine($"Error: {ex.Message}");return 1;}
  finally{if(app!=null)Marshal.ReleaseComObject(app);}
 }
}
