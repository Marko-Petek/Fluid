using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using static System.Char;

using static Fluid.Internals.Tools;                  // For Toolbox.
using Fluid.Internals.Collections;
using System.Collections.Generic;
using Fluid.Internals.Numerics;
using static Fluid.Internals.Algorithms;

namespace Fluid.Internals.IO {

public static class TextWriterExt {
   static CultureInfo _en_US = new CultureInfo("en-US");
   /// <summary>Write a 1D array with specified TextWriter.</summary>
   /// <param name="tw">TextWriter which writes to an underlying stream.</param>
   /// <param name="array1d">1D array.</param>
   public static void Write<T>(this TextWriter tw, T[] array1d) {
      int loop = array1d.Length - 1;
      tw.Write('{');
      for(int i = 0; i < loop; ++i) {
         tw.Write(
            String.Format(
            _en_US,
            "{0:G17}, ",
            array1d[i] ) ); }
      tw.Write(
         String.Format(
         _en_US, 
         "{0:G17}{1}",
         array1d[loop],
         "}" ) );
   }
   /// <summary>Write a 2D array with specified TextWriter.</summary>
   /// <param name="tw">TextWriter which writes to an underlying stream.</param>
   /// <param name="array1d">2D array.</param>
   public static void Write<T>(this TextWriter tw, T[][] array2d) {
      int loop = array2d.Length - 1;
      tw.Write('{');
      for(int i = 0; i < loop; ++i) {
         tw.Write<T>(array2d[i]);
         tw.Write(", "); }
      tw.Write<T>(array2d[loop]);
      tw.Write('}');
   }
   /// <summary>Write a 3D array with specified TextWriter.</summary>
   /// <param name="tw">TextWriter which writes to an underlying stream.</param>
   /// <param name="array1d">3D array.</param>
   public static void Write<T>(this TextWriter tw, T[][][] array3d) {
      int loop = array3d.Length - 1;
      tw.Write('{');
      for(int i = 0; i < loop; ++i) {
         tw.Write<T>(array3d[i]);
         tw.Write(", "); }
      tw.Write<T>(array3d[loop]);
      tw.Write('}');
   }
   /// <summary>Write a 4D array with specified TextWriter.</summary>
   /// <param name="tw">TextWriter which writes to an underlying stream.</param>
   /// <param name="array1d">4D array.</param>
   public static void Write<T>(this TextWriter tw, T[][][][] array4d) {
      int loop = array4d.Length - 1;
      tw.Write('{');
      for(int i = 0; i < loop; ++i) {
         tw.Write<T>(array4d[i]);
         tw.Write(", "); }
      tw.Write<T>(array4d[loop]);
      tw.Write('}');
   }
   /// <summary>Write a 5D array with specified TextWriter.</summary>
   /// <param name="array1d">5D array.</param>
   /// <param name="tw">TextWriter which writes to an underlying stream.</param>
   public static void Write<T>(this TextWriter tw, T[][][][][] array5d) {
      int loop = array5d.Length - 1;
      tw.Write('{');
      for(int i = 0; i < loop; ++i) {
         tw.Write<T>(array5d[i]);
         tw.Write(", "); }
      tw.Write<T>(array5d[loop]);
      tw.Write('}');
   }
   /// <summary>Write a 6D array with specified TextWriter.</summary>
   /// <param name="tw">TextWriter which writes to an underlying stream.</param>
   /// <param name="array1d">6D array.</param>
   public static void Write<T>(this TextWriter tw, T[][][][][][] array6d) {
      int loop = array6d.Length - 1;
      tw.Write('{');
      for(int i = 0; i < loop; ++i) {
         tw.Write<T>(array6d[i]);
         tw.Write(", "); }
      tw.Write<T>(array6d[loop]);
      tw.Write('}');
   }
   /// <summary>Write a 1D array with specified TextWriter and appends NewLine at end.</summary>
   /// <param name="tw">TextWriter which writes to an underlying stream.</param>
   /// <param name="array1d">1D array.</param>
   public static void WriteLine<T>(this TextWriter tw, T[] array1d) {
      tw.Write<T>(array1d);
      tw.WriteLine();
   }
   /// <summary>Write a 2D array with specified TextWriter and appends NewLine at end.</summary>
   /// <param name="tw">TextWriter which writes to an underlying stream.</param>
   /// <param name="array2d">2D array.</param>
   public static void WriteLine<T>(this TextWriter tw, T[][] array2d) {
      tw.Write<T>(array2d);
      tw.WriteLine();
   }
   /// <summary>Write a 3D array with specified TextWriter and appends NewLine at end.</summary>
   /// <param name="tw">TextWriter which writes to an underlying stream.</param>
   /// <param name="array3d">3D array.</param>
   public static void WriteLine<T>(this TextWriter tw, T[][][] array3d) {
      tw.Write<T>(array3d);
      tw.WriteLine();
   }
   /// <summary>Write a 4D array with specified TextWriter and appends NewLine at end.</summary>
   /// <param name="tw">TextWriter which writes to an underlying stream.</param>
   /// <param name="array4d">4D array.</param>
   public static void WriteLine<T>(this TextWriter tw, T[][][][] array4d) {
      tw.Write<T>(array4d);
      tw.WriteLine();
   }
   /// <summary>Write a 5D array with specified TextWriter and appends NewLine at end.</summary>
   /// <param name="tw">TextWriter which writes to an underlying stream.</param>
   /// <param name="array5d">5D array.</param>
   public static void WriteLine<T>(this TextWriter tw, T[][][][][] array5d) {
      tw.Write<T>(array5d);
      tw.WriteLine();
   }
   /// <summary>Write a 6D array with specified TextWriter and appends NewLine at end.</summary><param name="array6d">6D array.</param><param name="tw">TextWriter which writes to an underlying stream.</param>
   public static void WriteLine<T>(this TextWriter tw, T[][][][][][] array6d) {
      tw.Write<T>(array6d);
      tw.WriteLine();
   }

   public static void Write<T>(this TextWriter tw, IEnumerable<T> ienum) {
      tw.Write(ienum.ToString());
   }

   public static void WriteLine<T>(this TextWriter tw, IEnumerable<T> ienum) {
      tw.Write<T>(ienum);
      tw.WriteLine();
   }

   public static void Write<τ>(this TextWriter tw, Hierarchy<τ> hier)  where τ : IEquatable<τ>, new() {
      if(hier.TopNode is ValueNode<τ> valueNode)           // top node is already a value.
         tw.Write(valueNode.Value.ToString());
      else
         Recursion(hier.TopNode);

      void Recursion(RankedNode startNode) {
         int i = 0;
         tw.Write('{');
         foreach(var node in startNode.Subordinates) {
            if(node is ValueNode<τ> vNode)                                  // This is a value node.
               tw.Write(((ValueNode<τ>)node).Value.ToString());            // Write its value.
            else                                                            // This is not a value node.
               Recursion(node);                                            // Re-enter recursion.                            
            if(++i < startNode.Subordinates.Count)                                     // We have more than one node left in this group.
               tw.Write(", "); }                                            // So add a comma and space.
         tw.Write('}'); }
   }
   
   public static void WriteLine<τ>(this TextWriter tw, Hierarchy<τ> hier)  where τ : IEquatable<τ>, new() {
      tw.Write<τ>(hier);
      tw.WriteLine();
   }
   
   
}
}