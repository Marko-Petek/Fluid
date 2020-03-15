using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using static System.Char;

using static Fluid.Internals.Tools;                  // For Toolbox.
using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;
using static Fluid.Internals.Algorithms;

namespace Fluid.Internals.IO {

public static class TextReaderExt {
   /// <summary>Attempts to read a data stream fed by a text reader as a hierarchy. Throws an exception if the data is not convertible to a hierarchy.</summary>
   /// <param name="tr">Text reader such as FileReader.</param>
   /// <typeparam name="τ">Type of Hierarchy elements.</typeparam>
   public static Hierarchy<τ>? ReadHierarchy<τ>(this TextReader tr)
   where τ : IEquatable<τ>, new() {
      var nFI = new NumberFormatInfo();
      nFI.NumberDecimalSeparator = ".";
      var types = new Type[] { typeof(string), typeof(IFormatProvider) };
      var parseMethod = typeof(τ).GetMethod("Parse", types);
      if(parseMethod == null)
         throw new NullReferenceException("Parse method not found.");
      var parse = (Func<string,IFormatProvider,τ>) Delegate.CreateDelegate(
         typeof(Func<string,IFormatProvider,τ>), parseMethod);
      
      var wholeStr = tr.ReadToEnd();
      var sb = new StringBuilder(@"(\w+\.?\w*|", 300);
      sb.Append(@"\{                     ");
      sb.Append(@"  (?>                  ");
      sb.Append(@"      [^{}]+           ");
      sb.Append(@"    |                  ");
      sb.Append(@"      \{ (?<DEPTH>)    ");
      sb.Append(@"    |                  ");
      sb.Append(@"      }  (?<-DEPTH>)   ");
      sb.Append(@"  )*                   ");
      sb.Append(@"  (?(DEPTH)(?!))       ");
      sb.Append(@"}                      ");
      sb.Append(@")");                                    // The end of an alternator from first line.
      var matcher = new Regex(sb.ToString(), RegexOptions.IgnorePatternWhitespace,
         new TimeSpan(0,0,1));
      var firstMatch = matcher.Match(wholeStr);
      if(firstMatch.Success) {                                     // Now check elements inside it.
         var topNode = new RankedNode();
         var hier = new Hierarchy<τ>(topNode);
         Recursion(firstMatch.Value, topNode);
         return hier; }
      else
         return null;

      void Recursion(string prevMatched, RankedNode nodeAbove) {                    
         var sb2 = new StringBuilder(prevMatched, 300);
         sb2.Remove(prevMatched.Length - 1, 1).Remove(0,1);     // Strip it of outer braces.
         prevMatched = sb2.ToString();                          // Now it is without outer braces.
         Match newMatch = matcher.Match(prevMatched);           // Match either first value or braces
         while(newMatch.Success) {                                                   // If there was anything inside at all.
            if(newMatch.Value[0] == '{')                                            // If it was a non-value node.
               Recursion(newMatch.Value, new RankedNode(nodeAbove));
            else                                                                    // If it was a value node.
               new ValueNode<τ>(nodeAbove, parse(newMatch.Value, nFI));
            newMatch = newMatch.NextMatch(); }
      }
   }

   /// <summary>Attempts to read a data stream fed by a text reader as an array. Throws an exception if the data is not convertible to an array.</summary>
   /// <param name="tr">Text reader such as FileReader.</param>
   /// <typeparam name="τ">Type of array elements.</typeparam>
   public static Array Read<τ>(this TextReader tr)
   where τ : IEquatable<τ>, new() {
      return ReadHierarchy<τ>(tr) switch {
         Hierarchy<τ> res => res.ConvertToArray() switch {
            Array arr => arr,
            _ => throw new InvalidOperationException("Read data cannot be converted to array.") },
         _ => throw new IOException("Could not read data from file into a Hierarchy.") };
   }

}
}