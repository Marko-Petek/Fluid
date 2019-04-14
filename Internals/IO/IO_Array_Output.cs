using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using static System.Char;

using TB = Fluid.Internals.Toolbox;                  // For Toolbox.
using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;
using static Fluid.Internals.Ops;

namespace Fluid.Internals.IO {
   public static partial class IO {
      static CultureInfo _en_US = new CultureInfo("en-US");
      /// <summary>Write a 1D array with specified TextWriter.</summary><param name="array1d">1D array.</param><param name="tw">TextWriter which writes to an underlying stream.</param>
      public static void Write<T>(T[] array1d, TextWriter tw) {
         int loop = array1d.Length - 1;
         tw.Write('{');
         for(int i = 0; i < loop; ++i)
            //tw.Write($"{array1d[i].ToString("G17")}, ");
            tw.Write(
               String.Format(
                  _en_US,
                  "{0:G17}, ",
                  array1d[i] ) );
         //tw.Write($"{array1d[loop].ToString():G17}}}");
         tw.Write(
            String.Format(
               _en_US, 
               "{0:G17}{1}",
               array1d[loop],
               "}" ) );
      }
      /// <summary>Write a 2D array with specified TextWriter.</summary><param name="array1d">2D array.</param><param name="tw">TextWriter which writes to an underlying stream.</param>
      public static void Write<T>(T[][] array2d, TextWriter tw) {
         int loop = array2d.Length - 1;
         tw.Write('{');
         for(int i = 0; i < loop; ++i) {
            Write(array2d[i], tw);
            tw.Write(", "); }
         Write(array2d[loop], tw);
         tw.Write('}');
      }
      /// <summary>Write a 3D array with specified TextWriter.</summary><param name="array1d">3D array.</param><param name="tw">TextWriter which writes to an underlying stream.</param>
      public static void Write<T>(T[][][] array3d, TextWriter tw) {
         int loop = array3d.Length - 1;
         tw.Write('{');
         for(int i = 0; i < loop; ++i) {
            Write(array3d[i], tw);
            tw.Write(", "); }
         Write(array3d[loop], tw);
         tw.Write('}');
      }
      /// <summary>Write a 4D array with specified TextWriter.</summary><param name="array1d">4D array.</param><param name="tw">TextWriter which writes to an underlying stream.</param>
      public static void Write<T>(T[][][][] array4d, TextWriter tw) {
         int loop = array4d.Length - 1;
         tw.Write('{');
         for(int i = 0; i < loop; ++i) {
            Write(array4d[i], tw);
            tw.Write(", "); }
         Write(array4d[loop], tw);
         tw.Write('}');
      }
      /// <summary>Write a 5D array with specified TextWriter.</summary><param name="array1d">5D array.</param><param name="tw">TextWriter which writes to an underlying stream.</param>
      public static void Write<T>(T[][][][][] array5d, TextWriter tw) {
         int loop = array5d.Length - 1;
         tw.Write('{');
         for(int i = 0; i < loop; ++i) {
            Write(array5d[i], tw);
            tw.Write(", "); }
         Write(array5d[loop], tw);
         tw.Write('}');
      }
      /// <summary>Write a 6D array with specified TextWriter.</summary><param name="array1d">6D array.</param><param name="tw">TextWriter which writes to an underlying stream.</param>
      public static void Write<T>(T[][][][][][] array6d, TextWriter tw) {
         int loop = array6d.Length - 1;
         tw.Write('{');
         for(int i = 0; i < loop; ++i) {
            Write(array6d[i], tw);
            tw.Write(", "); }
         Write(array6d[loop], tw);
         tw.Write('}');
      }
      /// <summary>Write a 1D array with specified TextWriter and appends NewLine at end.</summary><param name="array1d">1D array.</param><param name="tw">TextWriter which writes to an underlying stream.</param>
      public static void WriteLine<T>(T[] array1d, TextWriter tw) {
         Write(array1d, tw);
         tw.WriteLine();
      }
      /// <summary>Write a 2D array with specified TextWriter and appends NewLine at end.</summary><param name="array2d">2D array.</param><param name="tw">TextWriter which writes to an underlying stream.</param>
      public static void WriteLine<T>(T[][] array2d, TextWriter tw) {
         Write(array2d, tw);
         tw.WriteLine();
      }
      /// <summary>Write a 3D array with specified TextWriter and appends NewLine at end.</summary><param name="array3d">3D array.</param><param name="tw">TextWriter which writes to an underlying stream.</param>
      public static void WriteLine<T>(T[][][] array3d, TextWriter tw) {
         Write(array3d, tw);
         tw.WriteLine();
      }
      /// <summary>Write a 4D array with specified TextWriter and appends NewLine at end.</summary><param name="array4d">4D array.</param><param name="tw">TextWriter which writes to an underlying stream.</param>
      public static void WriteLine<T>(T[][][][] array4d, TextWriter tw) {
         Write(array4d, tw);
         tw.WriteLine();
      }
      /// <summary>Write a 5D array with specified TextWriter and appends NewLine at end.</summary><param name="array5d">5D array.</param><param name="tw">TextWriter which writes to an underlying stream.</param>
      public static void WriteLine<T>(T[][][][][] array5d, TextWriter tw) {
         Write(array5d, tw);
         tw.WriteLine();
      }
      /// <summary>Write a 6D array with specified TextWriter and appends NewLine at end.</summary><param name="array6d">6D array.</param><param name="tw">TextWriter which writes to an underlying stream.</param>
      public static void WriteLine<T>(T[][][][][][] array6d, TextWriter tw) {
         Write(array6d, tw);
         tw.WriteLine();
      }
   }
}