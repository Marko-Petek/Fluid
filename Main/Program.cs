using System;
using System.Diagnostics;
using Fluid.Internals;


using Avalonia;
using Avalonia.Logging.Serilog;

namespace Fluid.Main {
  /// <summary>For any program of Fluid, execution starts here. This is a perfect point for initialization of utilities inside Toolbox.</summary>
class Program {

   static void Main(string[] args) =>
      BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
      

   // Avalonia configuration, don't remove; also used by visual designer.
   public static AppBuilder BuildAvaloniaApp()
      => AppBuilder.Configure<App>()
         .UsePlatformDetect()
         .LogToDebug();

}
}