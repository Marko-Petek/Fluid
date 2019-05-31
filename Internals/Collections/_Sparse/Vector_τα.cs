using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Fluid.Internals.Numerics;
using TB = Fluid.Internals.Toolbox;

namespace Fluid.Internals.Collections {
   /// <summary>A vector with specified dimension which holds values of type τ. Those can use arithmetic defined inside type α.</summary>
   /// <typeparam name="τ">Type of values.</typeparam>
   /// <typeparam name="α">Type defining arithmetic between values.</typeparam>
   public class Vector<τ,α> : Tensor<τ,α>, IEquatable<Vector<τ,α>>
   where τ : IEquatable<τ>, IComparable<τ>, new()
   where α : IArithmetic<τ>, new() {
      public Dictionary<int,τ> Vals { get; internal set; }          // An extra wrapped Dictionary which holds values.
      internal Vector() : base(0) {
         Vals = new Dictionary<int, τ>();
         Rank = 1; }
      internal Vector(int[] structure, Tensor<τ,α> sup, int cap) : base(structure, 1, sup, 0) {
         Vals = new Dictionary<int, τ>(cap);
      }

      internal Vector(Tensor<τ,α> sup, int cap) : this(sup.Structure, sup, cap) { }
      /// <summary>Creates a type τ vector with arithmetic α, with specified initial capacity.</summary>
      public Vector(int cap) : this(null, null, cap) { }
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
         else
            tgt.Vals = new Dictionary<int,τ>();
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
      public static Vector<τ,α> CreateFromSpan(Span<τ> slc) {
            var vec = new Vector<τ,α>(slc.Length, slc.Length);
            vec.Structure = new int[] { slc.Length };
            for(int i = 0; i < slc.Length; ++i)
               vec.Vals.Add(i, slc[i]);
            return vec;
      }
      /// <summary>Adds value without checking if it is equal to zero.</summary>
      /// <param name="key">Index.</param>
      /// <param name="val">Value.</param>
      internal void Add(int key, τ val) =>
         Vals.Add(key, val);

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

      public override Tensor<τ,α> TnrProduct(Tensor<τ,α> tnr2) {
         int newRank = Rank + tnr2.Rank;
         var newStructure = Structure.Concat(tnr2.Structure).ToArray();
         // We must substitute this vector with a tensor whose elements are multiples of tnr2.
         var res = new Tensor<τ,α>(newStructure, newRank, null, Vals.Count);
         foreach(var int_val in Vals)
            res.Add(int_val.Key, TensorExtensions<τ,α>.ScalMul1(int_val.Value, tnr2, res)); // int_val.Value*tnr2);
         return res;
      }

      public void Add(Vector<τ,α> vec2) {
         foreach(var int_val2 in vec2.Vals) {
            if(Vals.TryGetValue(int_val2.Key, out τ val1)) {                  // Value exists in Vec1.
               τ sum = O<τ,α>.A.Add(val1, int_val2.Value);
               if(sum != default)                                             // Sum is not zero.
                  Vals[int_val2.Key] = O<τ,α>.A.Add(val1, int_val2.Value);
               else
                  Vals.Remove(int_val2.Key); }
            else
               Vals.Add(int_val2.Key, int_val2.Value);
         }
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

      public Tensor<τ,α> ContractPart2(Tensor<τ,α> tnr2, int truInx2, int[] struc3, int conDim) {
         if(tnr2.Rank > 2) {                                                  // Result is tensor.
            Tensor<τ,α> elimTnr2, sumand, sum;
            sum = new Tensor<τ,α>(struc3);                                    // Set sum to a zero tensor.
            for(int i = 0; i < conDim; ++i) {
               elimTnr2 = tnr2.ElimRank(truInx2, i);
               if(Vals.TryGetValue(i, out var val) && elimTnr2 != null) {
                  sumand = val*elimTnr2;
                  sum.Add(sumand); } }
            if(sum.Count != 0)
               return sum;
            else
               return null; }
         else if(tnr2.Rank == 2) {
            Vector<τ,α> elimVec2, sumand, sum;
            sum = new Vector<τ,α>(struc3, null, 4);
            for(int i = 0; i < conDim; ++i) {
               elimVec2 = (Vector<τ,α>) tnr2.ElimRank(truInx2, i);
               if(Vals.TryGetValue(i, out var val) && elimVec2 != null) {
                  sumand = val*elimVec2;
                  sum.Add(sumand); } }
            if(sum.Vals.Count != 0)
               return sum;
            else
               return null; }
         else {                                                               // Result is scalar.
            throw new ArgumentException("Explicitly cast tnr2 to vector before using contract."); }
      }

      public Tensor<τ,α> Contract(Tensor<τ,α> tnr2, int natInx2) {
         (int[] struc3, _, int truInx2, int conDim) = ContractPart1(tnr2, 1, natInx2);
         return ContractPart2(tnr2, truInx2, struc3, conDim);
      }

      public τ Contract(Vector<τ,α> vec2) {
         τ res = default;
         foreach(var int_val1 in Vals) {
            if(vec2.Vals.TryGetValue(int_val1.Key, out var val2))
               res = O<τ,α>.A.Add(res, O<τ,α>.A.Mul(int_val1.Value, val2)); }
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

      public override string ToString() {
         var sb = new StringBuilder(2*Count);
         sb.Append("{");
         foreach(var emt in Vals) {
            sb.Append($"{emt.ToString()}, ");
         }
         sb.Remove(sb.Length - 2, 2);
         sb.Append("}");
         return sb.ToString();
      }
   }
}