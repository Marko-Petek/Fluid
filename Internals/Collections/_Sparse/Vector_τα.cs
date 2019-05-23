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
      internal Vector(int[] structure, Tensor<τ,α> sup, int cap) : base(structure, 1, sup, 0) {
         Vals = new Dictionary<int, τ>(cap);
      }
      /// <summary>Creates a type τ vector with arithmetic α, with specified initial capacity.</summary>
      protected Vector(int cap) : this(null, null, cap) { }
      /// <summary>Creates a type τ vector with arithmetic α, with specified initial capacity.</summary>
      public Vector(int dim, int cap) : this(new int[1] {dim}, null, cap) { }
      /// <summary>Creates a vector as a deep copy of another. You can optionally specify which meta-fields to copy. Default is AllExceptSup.</summary>
      /// <param name="src"></param>
      public Vector(Vector<τ,α> src, in CopySpecStruct cs) : base(src.Count + cs.ExtraCapacity) {
         Copy(src, this, cs);
      }
      public Vector(Vector<τ,α> src) : this(src, CopySpecs.Default) { }
      /// <summary>Creates a deep copy of a vector. You have to provide the already instantiated target.</summary>
      /// <param name="src">Copy source.</param>
      /// <param name="tgt">Copy target.</param>
      public static void Copy(Vector<τ,α> src, Vector<τ,α> tgt, in CopySpecStruct cs) {
         CopyMetaFields(src, tgt, cs.MetaFields, cs.Structure);
         if((cs.General & GeneralSpecs.Vals) == GeneralSpecs.Vals)
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
      public static Vector<τ,α> CreateFromSpan(Span<τ> slc) {           // TODO: Test Vector.CreateFromSpan method.
            var vec = new Vector<τ,α>(slc.Length, slc.Length);
            for(int i = 0, j = 0; i < slc.Length; ++i, ++j)
               vec[j] = slc[i];
            return vec;
      }
      new public τ this[int i] {
         get {
            Vals.TryGetValue(i, out τ val);
            return val; }
         set {
            if(value != default) {
               Vals[i] = value; }
            else
               Vals.Remove(i); }
      }
      
      #if false   // TODO: Implement Split on Vector.
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
      #endif
      
      /// <summary>Sum two R1 tensors.</summary>
      /// <param name="lVec">Left operand.</param>
      /// <param name="rVec">Right operand.</param>
      /// <returns>A new R1 tensor.</returns>
      public static Vector<τ,α> operator + (Vector<τ,α> lVec, Vector<τ,α> rVec) {
         var res = new Vector<τ,α>(lVec.Structure, lVec.Sup, lVec.Count + 4);
         foreach(var kv in lVec.Vals)
            res[kv.Key] = O<τ,α>.A.Add(kv.Value, rVec[kv.Key]);
         return res;
      }
      public static Vector<τ,α> operator - (Vector<τ,α> lVec, Vector<τ,α> rVec) {
         var res = new Vector<τ,α>(lVec.Structure, lVec.Sup, lVec.Count + 4);                     // Copy right operand. Result will appear here.
         foreach(var kv in rVec.Vals)
            res[kv.Key] = O<τ,α>.A.Sub(lVec[kv.Key], kv.Value);
         return res;
      }
      /// <summary>Negate operator.</summary>
      /// <param name="vec">Vector to negate.</param>
      public static Vector<τ,α> operator - (Vector<τ,α> vec) {
         var res = new Vector<τ,α>(vec.Structure, vec.Sup, vec.Count);                     // Copy right operand. Result will appear here.
         foreach(var kv in vec.Vals)
            res[kv.Key] = O<τ,α>.A.Neg(vec[kv.Key]);
         return res;
      }
      public static Vector<τ,α> operator * (τ scal, Vector<τ,α> vec) {
         var res = new Vector<τ,α>(vec, CopySpecs.ScalarMultiply);
         foreach(var kv in vec.Vals)
            res[kv.Key] = O<τ,α>.A.Mul(scal, vec[kv.Key]);
         return res;
      }

      #if false   // TODO: Implement Contract on Vector.
      /// <summary>Dot (scalar) product.</summary>
      public static τ operator *(Tensor1<τ,α> lTnr, Tensor1<τ,α> rTnr) {
         τ res = default;
         foreach(var lRowKVPair in lTnr)
            if(rTnr.TryGetValue(lRowKVPair.Key, out τ rVal))
               res = Arith.Add(res, Arith.Mul(lRowKVPair.Value, rVal));
         return res;
      }
      public static Tensor1<τ,α> operator *(τ lVal, Tensor1<τ,α> rTnr) {
         if(!lVal.Equals(default)) {                                                // Not zero.
            var res = new Tensor1<τ,α>(rTnr.Dim, rTnr.Count);      // Upcast to dictionary so that Dictionary's indexer is used.
            foreach(var rTnrKV in rTnr)
               res.Add(rTnrKV.Key, Arith.Mul(rTnrKV.Value, lVal));
            return res; }
         else                                                                          // Zero.
            return new Tensor1<τ,α>(rTnr.Dim);                               // Return empty row.
      }
      #endif
      /// <summary>Calculates square of Euclidean norm of SparseRow.</summary>
      public τ NormSqr() {
         τ res = default;
         foreach(var kv in Vals)
            res = O<τ,α>.A.Add(res, O<τ,α>.A.Mul(kv.Value, kv.Value));
         return res;
      }

      public bool Equals(Vector<τ,α> vec) {
         foreach(var kv in Vals)
            if(!(vec.Vals.TryGetValue(kv.Key, out τ val) && kv.Value.Equals(val)))        // Fetch did not suceed or values are not equal.
               return false;
         return true;
      }

      public bool Equals(Vector<τ,α> vec, τ eps) {
         foreach(var kv in Vals) {
            if(!(vec.Vals.TryGetValue(kv.Key, out τ val)))                      // Fetch did not suceed.
               return false;
            if(O<τ,α>.A.Abs(O<τ,α>.A.Sub(kv.Value, val)).CompareTo(eps) > 0 ) // Fetch suceeded but values do not agree within tolerance.
               return false; }
         return true;                                                              // All values agree within tolerance.
      }
   }
}