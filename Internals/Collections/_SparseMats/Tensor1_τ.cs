using System;
using System.Text;
using System.Linq;
using SCG = System.Collections.Generic;

using Fluid.Internals.Numerics;

namespace Fluid.Internals.Collections {
   public class Tensor1<τ> : SCG.Dictionary<int,τ> where τ : new() {
      /// <summary>Number of slots for values of type τ inside this rank 1 tensor.</summary>
      public int Dim1 { get; protected set; }
      /// <summary>Rank 2 tensor in which this rank 1 tensor resides. Can be null.</summary>
      public Tensor2<τ> Tensor2 { get; set; }

      /// <summary>Does not assign Dim1. User of this constructor must do it manually.</summary>
      protected Tensor1() : base() {}
      /// <summary>Create a rank 1 tensor with specified number of slots for values of type τ and specified initial capacity.</summary><param name="dim1">Width it would have in explicit form.</param><param name="capacity">Actual initially assigned memory.</param>
      public Tensor1(int dim1, int capacity = 6) : base(capacity) {
         Dim1 = dim1;
      }
      /// <summary>Factory method that creates a rank 1 tensor with specified dimension and initial capacity.</summary><param name="dim1">Number of slots available for values of type τ.</param><param name="capacity">Actual initially assigned memory.</param>
      public static Tensor1<τ> Create(int dim1, int capacity = 6) => new Tensor1<τ>(dim1, capacity);
      /// <summary>Overridable factory method that creates a rank 1 tensor with specified dimension and initial capacity.</summary><param name="dim1">Number of slots available for values of type τ.</param><param name="capacity">Actual initially assigned memory.</param>
      public virtual Tensor1<τ> CreateNew(int dim1, int capacity = 6) => new Tensor1<τ>(dim1, capacity);
      /// <summary>Creates a rank 1 tensor as a copy of another.</summary><param name="source">Rank 1 tensor to copy.</param>
      public Tensor1(Tensor1<τ> source) : this(source.Dim1, source.Count) {
         foreach(var pair in source)
            Add(pair.Key, pair.Value);
      }
      /// <summary>Factory method that creates a rank 1 tensor as a copy of another rank 1 tensor.</summary><param name="source">Rank 1 tensor to copy.</param>
      public static Tensor1<τ> Create(Tensor1<τ> source) => new Tensor1<τ>(source);
      
      /// <summary>Create a new rank 1 tensor by copying values from an array. Manually specify its dimension.</summary><param name="arr">Source array to copy.</param><param name="startArrEmtInx">Index of array element at which copying begins.</param><param name="nArrEmts">How many consecutive array elements to copy.</param><param name="startTenEmtInx">What index to assign to first element copied to tensor.</param><param name="tenDim1">Dimension of new rank 1 tensor.</param>
      public static Tensor1<τ> CreateFromArray(τ[] arr, int startArrEmtInx, int nArrEmts,
         int startTenEmtInx, int tenDim1) {
            var row = Create(tenDim1, arr.Length);
            for(int i = startArrEmtInx, j = startTenEmtInx; i < startArrEmtInx + nArrEmts; ++i, ++j)
               row[j] = arr[i];
            return row;
      }
      /// <summary>Create a new rank 1 tensor by copying values from an array. Dimension of created tensor is deduced from number of copied elements.</summary><param name="arr">Source array to copy.</param><param name="startArrEmtInx">Index of array element at which copying begins.</param><param name="nArrEmts">How many consecutive array elements to copy. Also the dimension of new tensor.</param><param name="startTenEmtInx">What index to assign to first element copied to tensor.</param>
      public static Tensor1<τ> CreateFromArray(τ[] arr, int startArrEmtInx, int nArrEmts,
         int startTenEmtInx) => CreateFromArray(arr, startArrEmtInx, nArrEmts, startTenEmtInx, nArrEmts);
      /// <summary>Create a new rank 1 tensor by copying values from an array. Dimension of created tensor is deduced from number of copied elements. Index of first copied element is set to 0.</summary><param name="arr">Source array to copy.</param><param name="startArrEmtInx">Index of array element at which copying begins.</param><param name="nArrEmts">How many consecutive array elements to copy. Also the dimension of new tensor.</param>
      public static Tensor1<τ> CreateFromArray(τ[] arr, int startArrEmtInx, int nArrEmts) =>
         CreateFromArray(arr, startArrEmtInx, nArrEmts, 0);
      /// <summary>Splits rank 1 tensor in two rank 1 tensors. Subject is modified (left remainder), while chopped-off part (right remainder) is returned as a separate (re-indexed from 0) rank 1 tensor.</summary><param name="inx">Index at which to split. Element at this index will end up as part of right remainder.</param>
      public Tensor1<τ> SplitAt(int inx) {
         var remTen1 = CreateNew(Dim1 - inx);
         foreach(var kvPair in this.Where(pair => pair.Key >= inx))
            remTen1.Add(kvPair.Key - inx, kvPair.Value);                         // Add to right remainder.
         foreach(var key in remTen1.Keys)
            Remove(key + inx);                                          // Must not modify collection during enumeration. Therefore entries have to be removed from left remainder afterwards.
         Dim1 = inx;
         return remTen1;
      }
      /// <summary>Append specified rank 1 tensor to this one.</summary><param name="rightPart">Rank 1 tensor to append.</param>
      public void MergeWith(Tensor1<τ> rightPart) {
         Dim1 += rightPart.Dim1;                                      // Readjust width.
         foreach(var kvPair in rightPart)
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
                  var newRow = new Tensor1<τ>(Dim1);               // Add new row to its owner and add value to it.
                  newRow.Add(i, value);
                  dummyRow.SparseMat.Add(dummyRow.Index, newRow); }
               else
                  base[i] = value; }                                  // Indexers adds or modifies if entry already exists.
            else if(!(this is DumTensor1<τ>))
               Remove(i); }                                           // Remove value at given index if value set is 0 and we are not in DummyRow.
      }
   }
}