using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using static Fluid.Internals.Tools;
namespace Fluid.Internals.IO2 {

/// <summary>Contains methods which write out nicely formatted values to console.</summary>
public class χConsole {

   public χConsole() {
      System.Console.OutputEncoding = Encoding.UTF8;
      TW = System.Console.IsOutputRedirected ? new DebugTextWriter() : System.Console.Out;
      WriteLine($"OutputRedirected: {System.Console.IsOutputRedirected}");
      AddReportIfDefined();
      WriteLine($"Define constants: {DefineConstants.ToString()}");                    // This has to be relayed without relying on Reporter.
   }

   [Conditional("REPORT")] static void AddReportIfDefined() {
      DefineConstants.Append(" REPORT ");
   }

   /// <summary>Write a 1D array to console.</summary><param name="array1d">1D array.</param>
   public void Write<T>(T[] array1d) => Statics.Write(array1d, TW);
   /// <summary>Write a 2D array to console.</summary><param name="array2d">2D array.</param>
   public void Write<T>(T[][] array2d) => Statics.Write(array2d, TW);
   /// <summary>Write a 3D array to console.</summary><param name="array3d">3D array.</param>
   public void Write<T>(T[][][] array3d) => Statics.Write(array3d, TW);
   /// <summary>Write a 4D array to console.</summary><param name="array4d">4D array.</param>
   public void Write<T>(T[][][][] array4d) => Statics.Write(array4d, TW);
   /// <summary>Write a 5D array to console.</summary><param name="array5d">5D array.</param>
   public void Write<T>(T[][][][][] array5d) => Statics.Write(array5d, TW);
   /// <summary>Write a 6D array to console.</summary><param name="array6d">6D array.</param>
   public void Write<T>(T[][][][][][] array6d) => Statics.Write(array6d, TW);
   /// <summary>Write a 1D array to console and append a NewLine.</summary><param name="array1d">1D array.</param>
   public void WriteLine<T>(T[] array1d) => Statics.WriteLine(array1d, TW);
   /// <summary>Write a 12D array to console and append a NewLine.</summary><param name="array2d">2D array.</param>
   public void WriteLine<T>(T[][] array2d) => Statics.WriteLine(array2d, TW);
   /// <summary>Write a 3D array to console and append a NewLine.</summary><param name="array3d">3D array.</param>
   public void WriteLine<T>(T[][][] array3d) => Statics.WriteLine(array3d, TW);
   /// <summary>Write a 4D array to console and append a NewLine.</summary><param name="array4d">4D array.</param>
   public void WriteLine<T>(T[][][][] array4d) => Statics.WriteLine(array4d, TW);
   /// <summary>Write a 5D array to console and append a NewLine.</summary><param name="array5d">5D array.</param>
   public void WriteLine<T>(T[][][][][] array5d) => Statics.WriteLine(array5d, TW);
   /// <summary>Write a 6D array to console and append a NewLine.</summary><param name="array6d">6D array.</param>
   public void WriteLine<T>(T[][][][][][] array6d) => Statics.WriteLine(array6d, TW);
   public void Write<T>(T input) => TW.Write(input?.ToString());
   public void WriteLine<T>(T input) => TW.WriteLine(input?.ToString());
}
}