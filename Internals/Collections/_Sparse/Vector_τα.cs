using System;
using System.Collections.Generic;
using Fluid.Internals.Numerics;
using TB = Fluid.Internals.Toolbox;

namespace Fluid.Internals.Collections {
   /// <summary>A vector with specified dimension which holds values of type τ. Those can use arithmetic defined inside type α.</summary>
   /// <typeparam name="τ">Type of values.</typeparam>
   /// <typeparam name="α">Type defining arithmetic between values.</typeparam>
   public class Vector<τ,α> : Tensor<τ,α>, IEquatable<Vector<τ,α>>
   where τ : IEquatable<τ>, IComparable<τ>, new()
   where α : IArithmetic<τ>, new() {
      public Dictionary<int,τ> Vals { get; protected set; }          // An extra wrapped Dictionary which holds values.
      protected Vector(int[] structure, Tensor<τ,α> sup, int cap) : base(structure, 1, sup, 0) {
         Vals = new Dictionary<int, τ>(cap);
      }
      /// <summary>Creates a type τ vector with arithmetic α, with specified initial capacity.</summary>
      protected Vector(int cap) : this(null, null, cap) { }
      /// <summary>Creates a type τ vector with arithmetic α, with specified initial capacity.</summary>
      public Vector(int dim, int cap) : this(new int[1] {dim}, null, cap) { }
      public Vector(Vector<τ,α> src) : base(src) {    // TODO: Rewrite after static copy method is done.
         Copy(src, this);
      }
      /// <summary>You have to provide the already instantiated target.</summary>
      /// <param name="src">Copy source.</param>
      /// <param name="tgt">Copy target.</param>
      public static void Copy(Vector<τ,α> src, Vector<τ,α> tgt) {
         tgt.Structure = src.Structure;
         tgt.Rank = 1;
         tgt.Sup = src.Sup ?? null;
         tgt.Vals = new Dictionary<int,τ>(src.Vals);
      }
      /// <summary>Creates a vector with specified dimension from an array.</summary>
      /// <param name="arr">Array to copy.</param>
      /// <param name="srtArrInx">Index of array element at which copying begins.</param>
      /// <param name="nArrEmts">How many consecutive array elements to copy.</param>
      /// <param name="firstR1Inx">What index to assign to first element copied to vector.</param>
      /// <param name="dim">Vector's R1 dimension.</param>
      public static Vector<τ,α> CreateFromArray(τ[] arr, int srtArrInx, int nArrEmts,
         int firstR1Inx, int dim) {
            var tnr = new Vector<τ,α>(dim, arr.Length);
            for(int i = srtArrInx, j = firstR1Inx; i < srtArrInx + nArrEmts; ++i, ++j)
               tnr[j] = arr[i];
            return tnr;
      }
      /// <summary>Factory method that creates a vector from an array. Vector's dimension equals number of copied elements.</summary>
      /// <param name="arr">Array to copy.</param>
      /// <param name="srtArrInx">Index of array element at which copying begins.</param>
      /// <param name="nArrEmts">How many consecutive array elements to copy. Also the R1 dimension of tensor.</param>
      /// <param name="firstR1Inx">What index to assign to first element copied to vector.</param>
      public static Vector<τ,α> CreateFromArray(τ[] arr, int srtArrInx, int nArrEmts,
         int firstR1Inx) => CreateFromArray(arr, srtArrInx, nArrEmts,
            firstR1Inx, nArrEmts);
      /// <summary>Factory method that creates a vector from an array. Vector's dimension equals number of copied elements. Index of first copied element is set to 0.</summary>
      /// <param name="arr">Array to copy.</param>
      /// <param name="srtArrInx">Index of array element at which copying begins.</param>
      /// <param name="nArrEmts">How many consecutive array elements to copy. Also the R1 dimension of vector.</param>
      public static Vector<τ,α> CreateFromArray(τ[] arr, int srtArrInx, int nArrEmts) =>
         CreateFromArray(arr, srtArrInx, nArrEmts, 0);

      /// <summary>New indexer definition that hides Dictionary's indexer. Returns 0 for non-existing elements.</summary>
      public new τ this[int i] {
         get {
            Vals.TryGetValue(i, out τ val);                                  // Outputs zero if value not found.
            return val; }
         set {
            if(!value.Equals(default(τ))) {                             // Value different from 0.
               if(this is VectorDum<τ,α> dumVec) {                      // Try downcasting to a dummy vector.
                  var newVec = new Vector<τ,α>(Dim);                    
                  newVec.Vals.Add(i, value);                                // Add value the setter accepted to new vector.
                  dumVec.Sup.Add(dumVec.Index, newVec); }               // Add new vector to its rank 2 tensor owner.
               else
                  Vals[i] = value; }                                  // Indexer adds or modifies if entry already exists.
            else if(!(this is VectorDum<τ,α>))
               Remove(i);
         }                                           // Remove value at given index if value set is 0 and we are not in DummyVector.
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
      
      /// <summary>Sum two R1 tensors.</summary>
      /// <param name="lTnr">Left operand.</param>
      /// <param name="rTnr">Right operand.</param>
      /// <returns>A new R1 tensor.</returns>
      public static Tensor1<τ,α> operator + (Tensor1<τ,α> lTnr, Tensor1<τ,α> rTnr) {
         var resTnr = new Tensor1<τ,α>(rTnr);    // Copy right operand. Result will appear here.
         foreach(var lTnrKV in lTnr)
            resTnr[lTnrKV.Key] = Arith.Add(lTnrKV.Value, rTnr[lTnrKV.Key]);
         return resTnr;
      }
      public static Tensor1<τ,α> operator - (Tensor1<τ,α> lTnr, Tensor1<τ,α> rTnr) {
         var resRow = new Tensor1<τ,α>(lTnr);    // Copy right operand. Result will appear here. Upcast to dictionary so that Dictionary's indexer is used in loop.
         foreach(var rTnrKV in rTnr)
            resRow[rTnrKV.Key] = Arith.Sub(lTnr[rTnrKV.Key], rTnrKV.Value);
         return resRow;
      }



      /// <summary>Dot (scalar) product.</summary>
      public static τ operator *(Tensor1<τ,α> lTnr, Tensor1<τ,α> rTnr) {
         τ res = default(τ);
         foreach(var lRowKVPair in lTnr)
            if(rTnr.TryGetValue(lRowKVPair.Key, out τ rVal))
               res = Arith.Add(res, Arith.Mul(lRowKVPair.Value, rVal));
         return res;
      }
      public static Tensor1<τ,α> operator *(τ lVal, Tensor1<τ,α> rTnr) {
         if(!lVal.Equals(default(τ))) {                                                // Not zero.
            var res = new Tensor1<τ,α>(rTnr.Dim, rTnr.Count);      // Upcast to dictionary so that Dictionary's indexer is used.
            foreach(var rTnrKV in rTnr)
               res.Add(rTnrKV.Key, Arith.Mul(rTnrKV.Value, lVal));
            return res; }
         else                                                                          // Zero.
            return new Tensor1<τ,α>(rTnr.Dim);                               // Return empty row.
      }
      /// <summary>Calculates square of Euclidean norm of SparseRow.</summary>
      public τ NormSqr() {
         τ result = default(τ);
         foreach(var val in this.Values)
            result = Arith.Add(result, Arith.Mul(val,val));
         return result;
      }

      public bool Equals(Tensor1<τ,α> tnr1) {
         foreach(var tnrKV in this)
            if(!(tnr1.TryGetValue(tnrKV.Key, out τ val) && tnrKV.Value.Equals(val)))        // Fetch did not suceed or values are not equal.
               return false;
         return true;
      }

      public bool Equals(Tensor1<τ,α> tnr1, τ eps) {
         foreach(var tnrKV in this) {
            if(!(tnr1.TryGetValue(tnrKV.Key, out τ val)))                      // Fetch did not suceed.
               return false;
            if(Arith.Abs(Arith.Sub(tnrKV.Value, val)).CompareTo(eps) > 0 ) // Fetch suceeded but values do not agree within tolerance.
               return false; }
         return true;                                                              // All values agree within tolerance.
      }
   }
}
