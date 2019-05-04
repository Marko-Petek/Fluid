using System;
using System.Text;
using System.Linq;
using SCG = System.Collections.Generic;

using Fluid.Internals.Numerics;

namespace Fluid.Internals.Collections {
   public class Tensor1<τ> : SCG.Dictionary<int,τ> where τ : new() {
      /// <summary>Width of row if written out explicitly.</summary>
      public int Width { get; protected set; }
      /// <summary>Matrix in which this SparseRow resides.</summary>
      public SparseMat<τ> SparseMat { get; set; }

      /// <summary>Does not assigns width. User of this constructor must do it manually.</summary>
      protected Tensor1() : base() {}
      /// <summary>Create a SparseRow with specified width it would have in explicit form and specified initial capacity.</summary><param name="width">Width it would have in explicit form.</param><param name="capacity">Initial capacity.</param>
      public Tensor1(int width, int capacity = 6) : base(capacity) {
         Width = width;
      }
      /// <summary>Creates a SparseRow as a copy of specified SparseRow.</summary><param name="source">Source to copy.</param>
      public Tensor1(Tensor1<τ> source) : this(source.Width, source.Count) {
         foreach(var pair in source)
            Add(pair.Key, pair.Value);
      }

      /// <summary>Creates an instance of the same (most derived) type as instance on which it is invoked.</summary><param name="width">Width (length of rows) that matrix would have in its explicit form.</param><param name="capacity">Initial row capacity.</param>
      public static Tensor1<τ> CreateSparseRow(int width, int capacity = 6) => new Tensor1<τ>(width, capacity);
      /// <summary>Creates a SparseRow as a copy of specified SparseRow.</summary><param name="source">Source to copy.</param>
      public static Tensor1<τ> CreateSparseRow(Tensor1<τ> source) => new Tensor1<τ>(source);
      /// <summary>Create a new SparseRow by copying an array.</summary><param name="arr">Array to copy.</param>
      public static Tensor1<τ> CreateFromArray(τ[] arr, int startCol, int nCols,
         int startInx, int width) {
            var row = CreateSparseRow(width, arr.Length);
            for(int i = startCol, j = startInx; i < startCol + nCols; ++i, ++j)
               row[j] = arr[i];
            return row;
      }

      public static Tensor1<τ> CreateFromArray(τ[] arr, int startCol, int nCols,
         int startInx) => CreateFromArray(arr, startCol, nCols, startInx, nCols);

      public static Tensor1<τ> CreateFromArray(τ[] arr, int startCol, int nCols) =>
         CreateFromArray(arr, startCol, nCols, 0);
      /// <summary>Splits SparseRow in two. This SparseRow is modified (left remainder), while chopped-off part (right remainder) is returned as a separate (re-indexed) SparseRow.</summary><param name="inx">Index at which to split. Element at this index will end up as part of right remainder.</param>
      public Tensor1<τ> SplitAt(int inx) {
         var remRow = new Tensor1<τ>(Width - inx);                                  
         foreach(var kvPair in this.Where(pair => pair.Key >= inx))
            remRow.Add(kvPair.Key - inx, kvPair.Value);                         // Add to right remainder.
         foreach(var key in remRow.Keys)
            Remove(key + inx);                                          // Must not modify collection during enumeration. Therefore entries have to be removed from left remainder afterwards.
         Width = inx;
         return remRow;
      }
      /// <summary>Append specified SparseRow to this one.</summary><param name="rightCols">SparseRow to append.</param>
      public void MergeWith(Tensor1<τ> rightCols) {
         Width += rightCols.Width;                                      // Readjust width.
         foreach(var kvPair in rightCols)
            this[kvPair.Key] = kvPair.Value;
      }
      /// <summary>Swap two elements specified by indices.</summary><param name="inx1">Index of first element.</param><param name="inx2">Index of second element.</param><remarks>Useful for swapping columns.</remarks>
      public void SwapElms(int inx1, int inx2) {
         //var asDict = (Dictionary<)
         bool firstExists = TryGetValue(inx1, out τ val1);
         bool secondExists = TryGetValue(inx2, out τ val2);
         if(firstExists) {
            if(secondExists) {
               base[inx1] = val2;
               base[inx2] = val1; }
            else {
               Remove(inx1);                                   // Element at inx1 becomes 0 and is removed.
               Add(inx2, val1); } }
         else if(secondExists) {
            Add(inx1, val2);
            Remove(inx2); }                                   // Else nothing happens, both are 0.
      }
      /// <summary>Apply element swaps as specified by a given swap matrix.</summary><param name="swapMatrix">SparseMatrix where non-zero element at [i][j] signifies a permutation i --> j.</param>
      public void ApplySwaps(SCG.Dictionary<int,int> swapDict) {
         foreach(var kVPair in swapDict)
            SwapElms(kVPair.Key, kVPair.Value);   
      }
      /// <summary>Create a string of form {{key1, val1}, {key2, val2}, ..., {keyN,valN}}..</summary>
      public override string ToString() {
         StringBuilder sb = new StringBuilder(72);
         sb.Append("{");
         foreach(var elm in this.OrderBy( kvPair => kvPair.Key ))
            sb.Append($"{{{elm.Key.ToString()}, {elm.Value.ToString()}}}, ");
         int length = sb.Length;
         sb.Remove(length - 2, 2);
         sb.Append("}");
         return sb.ToString();
      }
      /// <summary>New indexer definition (hides Dictionary's indexer). Returns 0 for non-existing elements.</summary>
      public new τ this[int i] {
         get {
            TryGetValue(i, out τ val);                               // Outputs zero if value not found.
            return val; }
         set {
            if(!value.Equals(default(τ))) {                           // Value different from 0.
               if(this is DumTensor1<τ> dummyRow) {             // Try downcasting to DummyRow.
                  var newRow = new Tensor1<τ>(Width);               // Add new row to its owner and add value to it.
                  newRow.Add(i, value);
                  dummyRow.SparseMat.Add(dummyRow.Index, newRow); }
               else
                  base[i] = value; }                                  // Indexers adds or modifies if entry already exists.
            else if(!(this is DumTensor1<τ>))
               Remove(i); }                                           // Remove value at given index if value set is 0 and we are not in DummyRow.
      }
   }
}