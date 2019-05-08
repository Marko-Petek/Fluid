using System;
using System.Text;
using System.Linq;
using SCG = System.Collections.Generic;
using TB = Fluid.Internals.Toolbox;

using Fluid.Internals.Numerics;

namespace Fluid.Internals.Collections {
   /// <summary>A tensor with specified rank and specified dimension which holds direct subordinates of type τ.</summary>
   /// <typeparam name="τ">Type of direct subordinates.</typeparam>
   public class Tensor<τ,α> : TensorBase<Tensor<τ,α>> {
      /// <summary>Creates a vector with specified rank, initial capacity of 1 and non-assigned dimension. You must specify dimension manually.</summary>
      public Tensor(int rank) : base(rank, 1) { }
      /// <summary>Creates a tensor with specified rank, dimension and initial capacity.</summary>
      /// <param name="rank">Rank specifies its height in the hierarchy.</param>
      /// <param name="dim">Dimension: number of direct subordinates.</param>
      /// <param name="cap">Initially assigned memory.</param>
      public Tensor(int rank, int dim, int cap = 6) : base(cap) {
         Rank = rank;
         Dim = dim;
      }
      /// <summary>Factory method that creates a tensor with specified rank, dimension and initial capacity.</summary>
      /// <param name="rank">Rank specifies height in the hierarchy.</param>
      /// <param name="dim">Dimension: number of direct subordinates.</param>
      /// <param name="cap">Initially assigned memory.</param>
      public static Tensor<τ,α> Create(int rank, int dim, int cap = 6) => new Tensor<τ,α>(rank, dim, cap);
      /// <summary>Creates a tensor as a copy of another.</summary>
      /// <param name="src">Tensor to copy.</param>
      public Tensor(Tensor<τ,α> source) : this(source.Rank, source.Dim, source.Count) {      // TODO: Make a deep copy.
         TB.Assert.True(source.Rank > 1,
            "Tensors's rank has to be at least 2 to be copied via constructor.");
         Recursion(source, this);

         void Recursion(Tensor<τ,α> src, Tensor<τ,α> tgt) {
            if(src.Rank > 2) {                                       // Subordinates are tensors.
               foreach (var kv in src) {
                  var tnr = new Tensor<τ,α>(kv.Value.Rank, kv.Value.Dim, kv.Value.Count);
                  Recursion(kv.Value, tnr);
                  tgt.Add(kv.Key, tnr); } }
            else if(src.Rank == 2) {                                 // Subordinates are vectors.
               foreach (var kv in src)
                  tgt.Add(kv.Key, new Vector<τ,α>((Vector<τ,α>) kv.Value)); }
            else
               throw new InvalidOperationException(
                  "Tensors's rank has to be at least 2 to be copied via constructor.");
         }
      }
      /// <summary>Factory method that creates a vector as a copy of another.</summary>
      /// <param name="src">Vector to copy.</param>
      public static Tensor<τ,α> Create(Tensor<τ,α> src) => new Tensor<τ,α>(src);

      /// <summary>Creates a vector with specified R1 dimension from an array.</summary>
      /// <param name="arr">Array to copy.</param>
      /// <param name="srtArrInx">Index of array element at which copying begins.</param>
      /// <param name="nArrEmts">How many consecutive array elements to copy.</param>
      /// <param name="firstR1Inx">What index to assign to first element copied to vector.</param>
      /// <param name="dim">Vector's R1 dimension.</param>
      public static Tensor<τ> CreateFromArray(τ[] arr, int srtArrInx, int nArrEmts,
         int firstR1Inx, int dim) {
            var row = Create(dim, arr.Length);
            for(int i = srtArrInx, j = firstR1Inx; i < srtArrInx + nArrEmts; ++i, ++j)
               row[j] = arr[i];
            return row;
      }
      /// <summary>Factory method that creates a vector from an array. Vector's dimension equals number of copied elements.</summary>
      /// <param name="arr">Array to copy.</param>
      /// <param name="srtArrInx">Index of array element at which copying begins.</param>
      /// <param name="nArrEmts">How many consecutive array elements to copy. Also the R1 dimension of tensor.</param>
      /// <param name="firstR1Inx">What index to assign to first element copied to vector.</param>
      public static Tensor<τ> CreateFromArray(τ[] arr, int srtArrInx, int nArrEmts,
         int firstR1Inx) => CreateFromArray(arr, srtArrInx, nArrEmts,
            firstR1Inx, nArrEmts);
      /// <summary>Factory method that creates a vector from an array. Vector's dimension equals number of copied elements. Index of first copied element is set to 0.</summary>
      /// <param name="arr">Array to copy.</param>
      /// <param name="srtArrInx">Index of array element at which copying begins.</param>
      /// <param name="nArrEmts">How many consecutive array elements to copy. Also the R1 dimension of vector.</param>
      public static Tensor<τ> CreateFromArray(τ[] arr, int srtArrInx, int nArrEmts) =>
         CreateFromArray(arr, srtArrInx, nArrEmts, 0);
      /// <summary>Splits a vector into two vectors. Caller (left remainder) is modified, while right remainder is returned as a separate vector re-indexed from 0.</summary>
      /// <param name="inx">Element at this index will end up as part of right remainder.</param>
      public Tensor<τ> SplitAt(int inx) {
         var remTen1 = CreateNew(Dim - inx);
         foreach(var kvPair in this.Where(pair => pair.Key >= inx))
            remTen1.Add(kvPair.Key - inx, kvPair.Value);                         // Add to right remainder.
         foreach(var key in remTen1.Keys)
            Remove(key + inx);                                          // Must not modify collection during enumeration. Therefore entries have to be removed from left remainder afterwards.
         Dim = inx;
         return remTen1;
      }
      /// <summary>Append specified vector to caller.</summary>
      /// <param name="appTnr">Vector to append.</param>
      public void MergeWith(Tensor<τ> appTnr) {
         Dim += appTnr.Dim;                                      // Readjust width.
         foreach(var kvPair in appTnr)
            this[kvPair.Key] = kvPair.Value;
      }
      /// <summary>Swap two R1 elements specified by indices.</summary>
      /// <param name="inx1">First element index.</param>
      /// <param name="inx2">Second element index.</param>
      public void Swap(int inx1, int inx2) {
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
      /// <summary>Apply element swaps as specified by a swap vector.</summary>
      /// <param name="swapVec">Vector where an element at index i with integer value j instructs a permutation i->j.</param>
      public void ApplySwaps(Tensor<int> swapVec) {
         foreach(var kVPair in swapVec)
            Swap(kVPair.Key, kVPair.Value);   
      }
      /// <summary>Create a string of all non-zero elements in form {{key1, val1}, {key2, val2}, ..., {keyN,valN}}.</summary>
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
      /// <summary>New indexer definition that hides Dictionary's indexer. Returns 0 for non-existing elements.</summary>
      public new τ this[int i] {
         get {
            TryGetValue(i, out τ val);                                  // Outputs zero if value not found.
            return val; }
         set {
            if(!value.Equals(default(τ))) {                             // Value different from 0.
               if(this is DumTensor1<τ> dumTen1) {                      // Try downcasting to a dummy vector.
                  var newTen1 = CreateNew(Dim);                    // Add new vector to its rank 2 tensor owner.
                  newTen1.Add(i, value);                                // Add value the setter accepted to new vector.
                  dumTen1.Sup.Add(dumTen1.Index, newTen1); }
               else
                  base[i] = value; }                                  // Indexer adds or modifies if entry already exists.
            else if(!(this is DumTensor1<τ>))
               Remove(i);
         }                                           // Remove value at given index if value set is 0 and we are not in DummyVector.
      }
   }
}