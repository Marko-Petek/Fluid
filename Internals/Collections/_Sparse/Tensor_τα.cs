using System;
using System.Text;
using System.Linq;
using SCG = System.Collections.Generic;
using TB = Fluid.Internals.Toolbox;

using Fluid.Internals.Numerics;

namespace Fluid.Internals.Collections {
   /// <summary>A tensor with specified rank and specified dimension which holds direct subordinates of type τ.</summary>
   /// <typeparam name="τ">Type of direct subordinates.</typeparam>
   public class Tensor<τ,α> : TensorBase<Tensor<τ,α>>,
      IEquatable<Tensor<τ,α>>                               // So we can check two vectors for equality.
      where τ : IEquatable<τ>, IComparable<τ>, new()
      where α : IArithmetic<τ>, new() {
         /// <summary>Contains methods that perform arithmetic.</summary>
         static α Arith { get; } = new α();
         /// <summary>Superior: a tensor directly above in the hierarchy. Null if this is the highest rank tensor.</summary>
         public Tensor<τ,α> Sup { get; protected set; }
         /// <summary>Dummy tensor used by indexer's getter when fetching a non-existent tensor below. When a setter of dummy tensor attempts to add a subordinate, dummy is copied as a real element to its superior.</summary>
         protected TensorDum<τ,α> Dummy { get; }
         /// <summary>Creates a tensor with specified rank, with dimension and initial capacity 1.</summary>
         public Tensor(int rank) : this(rank, 1, 1) {
            Dummy = new TensorDum<τ,α>(this);
         }
         /// <summary>Creates a tensor with specified rank, dimension and initial capacity.</summary>
         /// <param name="rank">Rank specifies its height in the hierarchy.</param>
         /// <param name="dim">Dimension: number of direct subordinates.</param>
         /// <param name="cap">Initially assigned memory.</param>
         public Tensor(int rank, int dim, int cap = 6) : base(cap) {
            Rank = rank;
            Dim = dim;
         }
         ///// <summary>Factory method that creates a tensor with specified rank, dimension and initial capacity.</summary>
         ///// <param name="rank">Rank specifies height in the hierarchy.</param>
         ///// <param name="dim">Dimension: number of direct subordinates.</param>
         ///// <param name="cap">Initially assigned memory.</param>
         //public static Tensor<τ,α> Create(int rank, int dim, int cap = 6) => new Tensor<τ,α>(rank, dim, cap);
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
         ///// <summary>Factory method that creates a vector as a copy of another.</summary>
         ///// <param name="src">Vector to copy.</param>
         //public static Tensor<τ,α> Create(Tensor<τ,α> src) => new Tensor<τ,α>(src);

         /// <summary>New indexer definition that hides Dictionary's indexer. Returns 0 for non-existing elements.</summary>
         public new Tensor<τ,α> this[int i] {                           // We know this is not a vector and its elements are tensors.
            get {
               TryGetValue(i, out Tensor<τ,α> val);                                  // Outputs null if value not found.
               return val; }
            set {
               if(!value.Equals(null)) {                             // Value non-null.
                  if(this is TensorDum<τ,α> dum) {                      // Try downcasting to a dummy tensor. // TODO: Dummy propagation, slot dimension info.
                     var newTen = new Tensor<τ,α>(Rank,;                    // Add new vector to its rank 2 tensor owner.
                     newTen.Add(i, value);                                // Add value the setter accepted to new vector.
                     dum.Sup.Add(dum.Index, newTen); }
                  else
                     ((Vector<τ,α>) this).Emt }//base[i] = value;                                   // Indexer adds or modifies if entry already exists.
               
               else if(!(this is DumTensor1<τ>))
                  Remove(i);                                // Remove value at given index if value set is 0 and we are not in DummyVector.

            }
         }
         
         public τ this[uint i] {                      // We know this is a Vector and its elements are values.
            get {
               var thisVec = (Vector<τ,α>) this;
               thisVec.Vals.TryGetValue((int)i, out τ val);                                  // Outputs zero if value not found.
               return val; }
            set {
               if(!value.Equals(default(τ))) {
                  if(this is TensorDum<τ,α> dum) {
                     var newVec = new Vector<τ,α>(Dim);
                     newVec.Vals.Add((int)i, value);
                     dum.Sup.Add(dum.Inx, newVec);
                  }
                  else {
                     var thisVec = (Vector<τ,α>) this;
                     thisVec.Vals[(int)i] = value; } }
               else if(!(this is TensorDum<τ,α>)) {            // Remove value at given index if value set is 0 and we are not in a Dummy.
                  var thisVec = (Vector<τ,α>) this;
                  thisVec.Vals.Remove((int)i); }
            }
         }
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
   }
}