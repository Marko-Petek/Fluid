using System.Threading;
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
         desktop.Startup += (o, e) => 
            Task.Factory.StartNew( () => {
               try {
                  //#if DEBUG
                  //args = new string[] { "Tests", "Tests/bin/Debug/netcoreapp3.1/Tests.dll" };
                  //#endif

                  // Print build time info independently from Fluid APIs. Enables us to see if the program recompiled even if Fluid.IO.Toolbox is not working.
                  var buildDT = GetBuildDate(Assembly.GetExecutingAssembly());         // Extract build time from assembly.
                  var dti = DateTimeFormatInfo.InvariantInfo;                          // Formatting info with which buildDT will be printed.
                  Console.WriteLine($"\nAssembly version: {buildDT.ToString(dti)}");
                  // Initialize Fluid APIs.
                  R("Toolbox initialized.");

                  if(e.Args != null && e.Args.Length > 0) {
                     int res = e.Args[0] switch {
                     "Tests" => Tests.Entry.Point(e.Args),
                     "CavityFlow" => Runnables.CavityFlow.Entry.Point(),
                     "Fiddle" => Runnables.Fiddle.Entry.Point(),
                     _ => throw new ArgumentException("Misspelled input args.") }; }
                  else
                     Runnables.Fiddle.Entry.Point(); }
               catch(Exception exc) {
                  Console.WriteLine($"Exception occured: {exc.Message}");
                  Console.WriteLine($"Stack trace:{exc.StackTrace}");
                  throw exc; }
               finally {
                  R("Exiting application.");
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