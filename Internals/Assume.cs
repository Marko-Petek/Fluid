using System;
using System.Runtime.CompilerServices;
using SCG = System.Collections.Generic;
namespace Fluid.Internals {

/// <summary>A class which simplifies exception coding.</summary>
public static class Assume {
   public static void True(bool cond, Func<string> msg) {               // msg is a method so that we can use Stringer from toolbox.
      if(!cond)
         throw new Exception(msg());
   }
   /// <summary>Makes sure a variable is not equal to specified value.</summary><param name="i">Variable.</param><param name="j">Value the variable should not be equal to.</param><param name="varName">Name of variable.</param><param name="remarks">Remarks appended to default exception report.</param><param name="varKind">Variable kind: local, field or property.</param><param name="callerName">Assigns compiler.</param>
   public static void NotEquals<T>(ref T i, T j, SCG.EqualityComparer<T> comparer,
   string varName = "N/A", string remarks = "", MemberKinds varKind = MemberKinds.Argument,
   [CallerMemberName] string callerName = "") {
      if(!comparer.Equals(i,j)) {
         string msg = varKind switch {
            MemberKinds.Argument => $"Argument {nameof(T)} {varName} inside method call {callerName} should not be {j}. {remarks}",
            MemberKinds.Field => $"Field {nameof(T)} {varName} inside {callerName} should not be {j}. {remarks}",
            MemberKinds.LocalVar => $"Local variable {nameof(T)} {varName} inside {callerName} should not be {j}. {remarks}",
            MemberKinds.Property => $"Property {nameof(T)} {varName} inside {callerName} should not be {j}. {remarks}",
            _ => "A variable had a value it should not have had." };
         throw new Exception(msg); }
   }
   /// <summary>Makes sure a variable is inside specified range.</summary><param name="index">Variable (usually index).</param><param name="lowerBound">Inclusive lower bound.</param><param name="upperBound">Exclusive upper bound.</param><param name="callerName">Assigns compiler.</param>
   public static void IndexInRange(int index, int lowerBound, int upperBound,
   [CallerMemberName] string callerName = "") {
      if(index < lowerBound)
         throw new IndexOutOfRangeException($"Index too small inside method {callerName}");
      else if(index >= upperBound)
         throw new IndexOutOfRangeException($"Index too large inside method {callerName}");
   }
   /// <summary>Throw ArgumentException if two specified fields are not equal.</summary><param name="field1">First field to compare.</param><param name="field2">Second field to compare.</param><param name="comparer">Arbiter of equality.</param><param name="callerName">Omit, filled by compiler.</param><typeparam name="T">Type of compared fields.</typeparam>
   public static void AreEqual<T>(T field1, T field2, SCG.EqualityComparer<T> comparer,
   [CallerMemberName] string callerName = "") {
      if(!comparer.Equals(field1, field2))
         throw new ArgumentException($"Fields used inside {callerName} not equal.");
   }
   /// <summary>Throw ArgumentException if two specified int fields are not equal.</summary><param name="field1">First int field to compare.</param><param name="field2">Second int field to compare.</param><param name="comparer">Arbiter of equality.</param><param name="callerName">Omit, filled by compiler.</param><typeparam name="T">Type of compared fields.</typeparam>
   public static void AreEqual(int field1, int field2,
      [CallerMemberName] string callerName = "") =>
         AreEqual(field1, field2, Comparers.Int, callerName);
   public enum MemberKinds {
      Argument, LocalVar, Field, Property, Method, Constructor
   }
}

}