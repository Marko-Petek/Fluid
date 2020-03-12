using System;
using CavityFlow = Fluid.Runnables.CavityFlow;
using Fluid.Internals;
using static Fluid.Internals.Toolbox;
using System.Globalization;
using System.Reflection;

namespace Fluid.Main
{
  /// <summary>For any program of Fluid, execution starts here. This is a perfect point for initialization of utilities inside Toolbox.</summary>
class Program {

   static void Main(string[] args) {            // args[0] has to be the name of the project without the extension.
      try {
         //#if DEBUG
         //args = new string[] { "Tests", "Tests/bin/Debug/netcoreapp3.1/Tests.dll" };
         //#endif
         // Print build time info independently from Fluid APIs. Enables us to see if the program recompiled even if Fluid.IO.Toolbox is not working.
         var buildDT = GetBuildDate(Assembly.GetExecutingAssembly());         // Extract build time from assembly.
         var dti = DateTimeFormatInfo.InvariantInfo;                          // Formatting info with which buildDT will be printed.
         Console.WriteLine($"Version: {buildDT.ToString(dti)}");
         // Initialize Fluid APIs.  nm
         R("Toolbox initialized.");
         Console.WriteLine("HERE");
         int result = args[0] switch {
            "Tests" => Tests.Entry.Point(args),
            "CavityFlow" => CavityFlow.Entry.Point(),
            _ => throw new ArgumentException("Task/Launch arguments incorrect.") }; }
      catch(Exception exc) {
         Console.WriteLine($"Exception occured: {exc.Message}");
         Console.WriteLine($"Stack trace:{exc.StackTrace}");
         throw exc; }
      finally {
         //R("Exiting application.");
         FileWriter.Flush(); }
   }

   private static DateTime GetBuildDate(Assembly assembly) {
      const string BuildVersionMetadataPrefix = "+build";
      var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
      if (attribute?.InformationalVersion != null) {
         var value = attribute.InformationalVersion;
         var index = value.IndexOf(BuildVersionMetadataPrefix);
         if (index > 0) {
            value = value.Substring(index + BuildVersionMetadataPrefix.Length);
            if (DateTime.TryParseExact(value, "yyyyMMddHHmmss",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
               return result; } }
      return default;
   }

}
}