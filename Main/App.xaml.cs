using System;
using System.Reflection;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using static Fluid.Internals.Toolbox;

namespace Fluid {
public class App : Application {
   public override void Initialize() {
      AvaloniaXamlLoader.Load(this);
   }
   
   public override void OnFrameworkInitializationCompleted() {
      if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
         desktop.MainWindow = new MainWindow();
         desktop.Startup += (o, e) =>              // args are either "UI [...]" or "".
            Task.Factory.StartNew( () => {      // Run a console application the old way, parallel to the window.
               try {
                  //#if DEBUG
                  //args = new string[] { "Tests", "Tests/bin/Debug/netcoreapp3.1/Tests.dll" };
                  //#endif

                  // Print build time info independently from Fluid APIs. Enables us to see if the program recompiled even if Fluid.IO.Toolbox is not working.
                  var buildDT = GetBuildDate(Assembly.GetExecutingAssembly());               // Extract build time from assembly.
                  var dti = DateTimeFormatInfo.InvariantInfo;                                // Formatting info with which buildDT will be printed.
                  Console.WriteLine($"\nAssembly version: {buildDT.ToString(dti)}");
                  // Initialize Fluid APIs.
                  R("Toolbox initialized.");
                  if(e.Args != null && e.Args.Length > 1 && e.Args[1] == "Console") {
                     R("Starting parallel Console app.");
                     int res = 0;
                     if(e.Args.Length > 2) {
                        res = e.Args[2] switch {
                           "Tests" => Tests.Entry.Point(e.Args),                             // There's a third argument that specifies entry point: "UI Console Tests".
                           "CavityFlow" => Runnables.CavityFlow.Entry.Point(),
                           "Fiddle" => Runnables.Fiddle.Entry.Point(),
                           "Markdig" => Runnables.Markdig.Entry.Point(),
                           _ => throw new ArgumentException("Misspelled input args.") }; }
                     else {
                        res = Runnables.Fiddle.Entry.Point(); } } }                          // No third argument specifying the entry point: "UI Console" Start "Fiddle".
               // else  ...   No further args means no parallel console app to run.
               catch(Exception exc) {
                  Console.WriteLine($"Exception occured: {exc.Message}");
                  Console.WriteLine($"Stack trace:{exc.StackTrace}");
                  throw exc; }
               finally {
                  R("Exiting parallel console application.");
                  Reporter.WriteLine();
                  FWriter.Flush(); } } ); }

      base.OnFrameworkInitializationCompleted();
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