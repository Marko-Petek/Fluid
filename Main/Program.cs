using System;
using System.Reflection;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using static Fluid.Internals.Toolbox;

using Avalonia.Logging.Serilog;

namespace Fluid.Main {
  /// <summary>For any program of Fluid, execution starts here. This is a perfect point for initialization of utilities inside Toolbox.</summary>
class Program {

   /// <summary>If you start without arguments it starts a UI application.</summary>
   /// <param name="args"></param>
   static int Main(string[] args) {
      int res;
      if(args != null) {
         res = args[0] switch {
            "UI" => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args),    // If arg[1] is "Console" we start a UI app with a parallel console (as a background task).
            _ => StartConsoleApp(args) }; }                                      // Not UI. Start console app with args provided.
      else {
         res = BuildAvaloniaApp().StartWithClassicDesktopLifetime(null); }       // No args defaults to UI only, no parallel console.
      return res;
   }

      

   // Avalonia configuration, don't remove; also used by visual designer.
   public static AppBuilder BuildAvaloniaApp()
      => AppBuilder.Configure<App>()
         .UsePlatformDetect()
         .LogToDebug();

   public static int StartConsoleApp(string[] args) {
      int res = 0;
      try {
         //#if DEBUG
         //args = new string[] { "Tests", "Tests/bin/Debug/netcoreapp3.1/Tests.dll" };
         //#endif

         // Print build time info independently from Fluid APIs. Enables us to see if the program recompiled even if Fluid.IO.Toolbox is not working.
         var buildDT = GetBuildDate(Assembly.GetExecutingAssembly());         // Extract build time from assembly.
         var dti = DateTimeFormatInfo.InvariantInfo;                          // Formatting info with which buildDT will be printed.
         Console.WriteLine($"\nAssembly version: {buildDT.ToString(dti)}");
         // Initialize Fluid APIs.
         R("Toolbox initialized. Starting pure console application.");
         if(args != null && args.Length > 0) {
            res = args[0] switch {
            "Tests" => Tests.Entry.Point(args),
            "CavityFlow" => Runnables.CavityFlow.Entry.Point(),
            "Fiddle" => Runnables.Fiddle.Entry.Point(),
            "Markdig" => Runnables.Markdig.Entry.Point(),
            _ => throw new ArgumentException("Misspelled input args.") }; }
         else
            res = Runnables.Fiddle.Entry.Point(); }
      catch(Exception exc) {
         Console.WriteLine($"Exception occured: {exc.Message}");
         Console.WriteLine($"Stack trace:{exc.StackTrace}");
         throw exc; }
      finally {
         R("Exiting parallel console application.");
         Reporter.WriteLine();
         FWriter.Flush(); }
      return res;
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