using System;
using System.Text;
using System.Linq;
using SCG = System.Collections.Generic;

using Fluid.Internals.Numerics;

namespace Fluid.Internals.Collections {
   /// <summary>A rank 1 (R1) tensor with specified dimension of its only rank which holds values of type τ.</summary>
   /// <typeparam name="τ">Type of R1 elements.</typeparam>
   public class Tensor1<τ> : SCG.Dictionary<int,τ> where τ : new() {
      /// <summary>Dimension of R1 which holds values of type τ.</summary>
      public int DimR1 { get; protected set; }
      /// <summary>R2 tensor in which this R1 tensor resides. Can be null.</summary>
      public Tensor2<τ> Tnr2 { get; set; }

      /// <summary>Creates a R1 tensor with initial capacity of 1 and does not assign its R1 dimension. You must do it manually.</summary>
      public Tensor1() : base(1) {}
      /// <summary>Creates a R1 tensor with specified R1 dimension and initial capacity.</summary>
      /// <param name="dimR1">Rank 1 dimension.</param>
      /// <param name="cap">Initially assigned memory.</param>
      public Tensor1(int dimR1, int cap = 6) : base(cap) {
         DimR1 = dimR1;
      }
      /// <summary>Factory method that creates a R1 tensor with specified R1 dimension and initial capacity.</summary>
      /// <param name="dimR1">Rank 1 dimension.</param>
      /// <param name="cap">Initially assigned memory.</param>
      public static Tensor1<τ> Create(int dimR1, int cap = 6) => new Tensor1<τ>(dimR1, cap);
      /// <summary>Virtual factory method that creates a R1 tensor with specified R1 dimension and initial capacity.</summary>
      /// <param name="dimR1">Rank 1 dimension.</param>
      /// <param name="cap">Initially assigned memory.</param>
      public virtual Tensor1<τ> CreateNew(int dimR1, int cap = 6) => new Tensor1<τ>(dimR1, cap);
      /// <summary>Creates a R1 tensor as a copy of another.</summary>
      /// <param name="src">R1 tensor to copy.</param>
      public Tensor1(Tensor1<τ> src) : this(src.DimR1, src.Count) {
         foreach(var pair in src)
            Add(pair.Key, pair.Value);
      }
      /// <summary>Factory method that creates a R1 tensor as a copy of another.</summary>
      /// <param name="src">R1 tensor to copy.</param>
      public static Tensor1<τ> Create(Tensor1<τ> src) => new Tensor1<τ>(src);

      /// <summary>Creates a R1 tensor with specified dimension from an array.</summary>
      /// <param name="arr">Array to copy.</param>
      /// <param name="srtArrInx">Index of array element at which copying begins.</param>
      /// <param name="nArrEmts">How many consecutive array elements to copy.</param>
      /// <param name="firstR1Inx">What index to assign to first element copied to tensor.</param>
      /// <param name="dimR1">Tensor's R1 dimension.</param>
      public static Tensor1<τ> CreateFromArray(τ[] arr, int srtArrInx, int nArrEmts,
         int firstR1Inx, int dimR1) {
            var row = Create(dimR1, arr.Length);
            for(int i = srtArrInx, j = firstR1Inx; i < srtArrInx + nArrEmts; ++i, ++j)
               row[j] = arr[i];
            return row;
      }
      /// <summary>Factory method that creates a R1 tensor from an array. Tensor's R1 dimension equals number of copied elements.</summary>
      /// <param name="arr">Array to copy.</param>
      /// <param name="srtArrInx">Index of array element at which copying begins.</param>
      /// <param name="nArrEmts">How many consecutive array elements to copy. Also the R1 dimension of tensor.</param>
      /// <param name="firstR1Inx">What index to assign to first element copied to tensor.</param>
      public static Tensor1<τ> CreateFromArray(τ[] arr, int srtArrInx, int nArrEmts,
         int firstR1Inx) => CreateFromArray(arr, srtArrInx, nArrEmts,
            firstR1Inx, nArrEmts);
      /// <summary>Factory method that creates a R1 tensor from an array. Tensor's R1 dimension equals number of copied elements. Index of first copied element is set to 0.</summary>
      /// <param name="arr">Array to copy.</param>
      /// <param name="srtArrInx">Index of array element at which copying begins.</param>
      /// <param name="nArrEmts">How many consecutive array elements to copy. Also the R1 dimension of tensor.</param>
      public static Tensor1<τ> CreateFromArray(τ[] arr, int srtArrInx, int nArrEmts) =>
         CreateFromArray(arr, srtArrInx, nArrEmts, 0);
      /// <summary>Splits a R1 tensor into two R1 tensors. Caller (left remainder) is modified, while right remainder is returned as a separate R1 tensor re-indexed from 0.</summary>
      /// <param name="inx">Element at this index will end up as part of right remainder.</param>
      public Tensor1<τ> SplitAt(int inx) {
         var remTen1 = CreateNew(DimR1 - inx);
         foreach(var kvPair in this.Where(pair => pair.Key >= inx))
            remTen1.Add(kvPair.Key - inx, kvPair.Value);                         // Add to right remainder.
         foreach(var key in remTen1.Keys)
            Remove(key + inx);                                          // Must not modify collection during enumeration. Therefore entries have to be removed from left remainder afterwards.
         DimR1 = inx;
         return remTen1;
      }
      /// <summary>Append specified R1 tensor to caller.</summary>
      /// <param name="appTnr">R1 tensor to append.</param>
      public void MergeWith(Tensor1<τ> appTnr) {
         DimR1 += appTnr.DimR1;                                      // Readjust width.
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
      /// <summary>Apply element swaps as specified by a R1 swap tensor.</summary>
      /// <param name="swapTnr">Rank 1 tensor where an element at index i with integer value j instructs a permutation i->j.</param>
      public void ApplySwaps(Tensor1<int> swapTnr) {
         foreach(var kVPair in swapTnr)
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
               if(this is DumTensor1<τ> dumTen1) {                      // Try downcasting to a dummy tensor.
                  var newTen1 = CreateNew(DimR1);                    // Add new rank 1 tensor to its rank 2 owner.
                  newTen1.Add(i, value);                                // Add value the setter accepted to new rank 1 tensor.
                  dumTen1.Tnr2.Add(dumTen1.Index, newTen1); }
               else
                  base[i] = value; }                                  // Indexer adds or modifies if entry already exists.
            else if(!(this is DumTensor1<τ>))
               Remove(i); }                                           // Remove value at given index if value set is 0 and we are not in DummyRow.
      }
   }
}