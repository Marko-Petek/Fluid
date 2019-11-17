#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Reflection;
using static System.Char;

using static Fluid.Internals.Toolbox;                  // For Toolbox.
using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;
using static Fluid.Internals.Algorithms;

namespace Fluid.Internals.IO {
public static partial class IO {
   /// <remarks><see cref="TestRefs.HierarchyOutput"/></remarks>
   public static void Write<τ>(Hierarchy<τ> hier, TextWriter tw)
   where τ : IEquatable<τ>, new() {
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
         tw.Write('}');
      }
   }
   public static void WriteLine<τ>(this Hierarchy<τ> hier, TextWriter tw)
   where τ : IEquatable<τ>, new() {
      Write(hier, tw);
      tw.WriteLine();
   }
   
   /// <remarks><see cref="TestRefs.HierarchyInput"/></remarks>
   public static Hierarchy<τ> ReadHierarchy<τ>(TextReader tr)
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
         return Voids<τ>.Hier;

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
}
}
#nullable restore