using System;
using System.IO;
using System.Text;
using System.Diagnostics;

using static Fluid.Internals.Toolbox;

namespace Fluid.Internals.IO {
   /// <summary>Contains methods which write out nicely formatted values to console.</summary>
   public class Console {
      public int BufferWidth => System.Console.IsOutputRedirected ? 125 : System.Console.BufferWidth;
      /// <summary>TextWriter belonging to System.Console.</summary>
      public TextWriter TW { get; set; }
      public static StringBuilder DefineConstants { get; } = new StringBuilder(100);

      public Console() {
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
      public void Write<T>(T[] array1d) => IO.Write(array1d, TW);
      /// <summary>Write a 2D array to console.</summary><param name="array2d">2D array.</param>
      public void Write<T>(T[][] array2d) => IO.Write(array2d, TW);
      /// <summary>Write a 3D array to console.</summary><param name="array3d">3D array.</param>
      public void Write<T>(T[][][] array3d) => IO.Write(array3d, TW);
      /// <summary>Write a 4D array to console.</summary><param name="array4d">4D array.</param>
      public void Write<T>(T[][][][] array4d) => IO.Write(array4d, TW);
      /// <summary>Write a 5D array to console.</summary><param name="array5d">5D array.</param>
      public void Write<T>(T[][][][][] array5d) => IO.Write(array5d, TW);
      /// <summary>Write a 6D array to console.</summary><param name="array6d">6D array.</param>
      public void Write<T>(T[][][][][][] array6d) => IO.Write(array6d, TW);
      /// <summary>Write a 1D array to console and append a NewLine.</summary><param name="array1d">1D array.</param>
      public void WriteLine<T>(T[] array1d) => IO.WriteLine(array1d, TW);
      /// <summary>Write a 12D array to console and append a NewLine.</summary><param name="array2d">2D array.</param>
      public void WriteLine<T>(T[][] array2d) => IO.WriteLine(array2d, TW);
      /// <summary>Write a 3D array to console and append a NewLine.</summary><param name="array3d">3D array.</param>
      public void WriteLine<T>(T[][][] array3d) => IO.WriteLine(array3d, TW);
      /// <summary>Write a 4D array to console and append a NewLine.</summary><param name="array4d">4D array.</param>
      public void WriteLine<T>(T[][][][] array4d) => IO.WriteLine(array4d, TW);
      /// <summary>Write a 5D array to console and append a NewLine.</summary><param name="array5d">5D array.</param>
      public void WriteLine<T>(T[][][][][] array5d) => IO.WriteLine(array5d, TW);
      /// <summary>Write a 6D array to console and append a NewLine.</summary><param name="array6d">6D array.</param>
      public void WriteLine<T>(T[][][][][][] array6d) => IO.WriteLine(array6d, TW);
      public void Write<T>(T input) => TW.Write(input.ToString());
      public void WriteLine<T>(T input) => TW.WriteLine(input.ToString());
   }
}