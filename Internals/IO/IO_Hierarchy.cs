using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Reflection;
using static System.Char;

using TB = Fluid.Internals.Toolbox;                  // For Toolbox.
using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;
using static Fluid.Internals.Ops;

namespace Fluid.Internals.IO {
   public static partial class IO {
      public static void Write<T>(Hierarchy<T> hier, TextWriter tw) {
         if(hier.TopNode is ValueNode<T> valueNode)           // top node is already a value.
            tw.Write(valueNode.Value.ToString());
         else
            Recursion(hier.TopNode);

         void Recursion(RankedNode startNode) {
            int i = 0;
            tw.Write('{');
            foreach(var node in startNode.Subordinates) {
               if(node is ValueNode<T> vNode)                                  // This is a value node.
                  tw.Write(((ValueNode<T>)node).Value.ToString());            // Write its value.
               else                                                            // This is not a value node.
                  Recursion(node);                                            // Re-enter recursion.                            
               if(++i < startNode.Subordinates.Count)                                     // We have more than one node left in this group.
                  tw.Write(", "); }                                            // So add a comma and space.
            tw.Write('}');
         }
      }
      public static void WriteLine<T>(this Hierarchy<T> hier, TextWriter tw) {
         Write(hier, tw);
         tw.WriteLine();
      }
      public static Hierarchy<T> ReadHierarchy<T>(TextReader tr) {
         var nFI = new NumberFormatInfo();
         nFI.NumberDecimalSeparator = ".";
         var types = new Type[] { typeof(string), typeof(IFormatProvider) };
         var parseMethod = typeof(T).GetMethod("Parse", types);
         var parse = (Func<string,IFormatProvider,T>) Delegate.CreateDelegate(
            typeof(Func<string,IFormatProvider,T>), parseMethod);
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
            var hier = new Hierarchy<T>(topNode);
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
                  new ValueNode<T>(nodeAbove, parse(newMatch.Value, nFI));
               newMatch = newMatch.NextMatch(); }
         }
      }
   }
}