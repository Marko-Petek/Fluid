using System;
using System.Text;
using System.Linq;
using SCG = System.Collections.Generic;

using Fluid.Internals.Numerics;

namespace Fluid.Internals.Collections {
   /// <summary>A rank 1 (R1) tensor with specified dimension of its only rank which holds values of type τ. Type τ can use arithmetic defined inside type α.</summary>
   /// <typeparam name="τ">Type of R1 elements.</typeparam>
   /// <typeparam name="α">Type that defines arithmetic between values of type τ.</typeparam>
   public class Tensor1<τ,α> : Tensor1<τ>,
      IEquatable<Tensor1<τ,α>>                                         // So that we can equate two rank 1 tensors via the Equals method.
      where τ : IEquatable<τ>, IComparable<τ>, new()
      where α : IArithmetic<τ>, new() {
         /// <summary>Contains arithmetic methods.</summary>
         static α Arith { get; } = new α();
         /// <summary>Creates a R1 tensor with arithmetic α, with initial capacity of 1 and does not assign its R1 dimension. You must do it manually.</summary>
         protected Tensor1() : base() {}
         /// <summary>Creates a R1 tensor with arithmetic α, with specified R1 dimension and initial capacity.</summary>
         /// <param name="dimR1">Rank 1 dimension.</param>
         /// <param name="cap">Initially assigned memory.</param>
         public Tensor1(int dimR1, int cap = 6) : base(dimR1, cap) { }
         /// <summary>Factory method that creates a R1 tensor with arithmetic α, with specified R1 dimension and initial capacity.</summary>
         /// <param name="dimR1">Rank 1 dimension.</param>
         /// <param name="cap">Initially assigned memory.</param>
         new public static Tensor1<τ,α> Create(int dimR1, int cap = 6) => new Tensor1<τ,α>(dimR1, cap);
         /// <summary>Creates a R1 tensor with arithmetic α as a copy of another.</summary>
         /// <param name="src">R1 tensor to copy.</param>
         public Tensor1(Tensor1<τ,α> src) : this(src.DimR1, src.Count) {
            foreach(var pair in src)
               Add(pair.Key, pair.Value);
         }
      /// <summary>Factory method that creates a R1 tensor with arithmetic α as a copy of another.</summary>
      /// <param name="src">R1 tensor to copy.</param>
      public static Tensor1<τ,α> Create(Tensor1<τ,α> src) => new Tensor1<τ,α>(src);
      /// <summary>Creates a R1 tensor with arithmetic α, with specified dimension from an array.</summary>
      /// <param name="arr">Array to copy.</param>
      /// <param name="srtArrInx">Index of array element at which copying begins.</param>
      /// <param name="nArrEmts">How many consecutive array elements to copy.</param>
      /// <param name="firstR1Inx">What index to assign to first element copied to tensor.</param>
      /// <param name="dimR1">Tensor's R1 dimension.</param>
      new public static Tensor1<τ,α> CreateFromArray(τ[] arr, int srtArrInx, int nArrEmts,      // srt = start
         int firstR1Inx, int dimR1) {
            var tnr1 = Create(dimR1, arr.Length);
            for(int i = srtArrInx, j = firstR1Inx; i < srtArrInx + nArrEmts; ++i, ++j)
               tnr1[j] = arr[i];
            return tnr1;
      }
      /// <summary>Factory method that creates a R1 tensor with arithmetic α from an array. Tensor's R1 dimension equals number of copied elements.</summary>
      /// <param name="arr">Array to copy.</param>
      /// <param name="srtArrInx">Index of array element at which copying begins.</param>
      /// <param name="nArrEmts">How many consecutive array elements to copy. Also the R1 dimension of tensor.</param>
      /// <param name="firstR1Inx">What index to assign to first element copied to tensor.</param>
      new public static Tensor1<τ,α> CreateFromArray(τ[] arr, int srtArrInx, int nArrEmts,
            int firstR1Inx) => CreateFromArray(arr, srtArrInx, nArrEmts, firstR1Inx, nArrEmts);
      /// <summary>Factory method that creates a R1 tensor with arithmetic α from an array. Tensor's R1 dimension equals number of copied elements. Index of first copied element is set to 0.</summary>
      /// <param name="arr">Array to copy.</param>
      /// <param name="srtArrInx">Index of array element at which copying begins.</param>
      /// <param name="nArrEmts">How many consecutive array elements to copy. Also the R1 dimension of tensor.</param>
      new public static Tensor1<τ,α> CreateFromArray(τ[] arr, int srtArrInx, int nArrEmts) =>
            CreateFromArray(arr, srtArrInx, nArrEmts, 0);
      /// <summary>Splits a R1 tensor into two R1 tensors. Caller (left remainder) is modified, while right remainder is returned as a separate R1 tensor re-indexed from 0.</summary>
      /// <param name="inx">Element at this index will end up as part of right remainder.</param>
      new public Tensor1<τ,α> SplitAt(int inx) => (Tensor1<τ,α>)base.SplitAt(inx);
      
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
               var res = new Tensor1<τ,α>(rTnr.DimR1, rTnr.Count);      // Upcast to dictionary so that Dictionary's indexer is used.
               foreach(var rTnrKV in rTnr)
                  res.Add(rTnrKV.Key, Arith.Mul(rTnrKV.Value, lVal));
               return res; }
            else                                                                          // Zero.
               return new Tensor1<τ,α>(rTnr.DimR1);                               // Return empty row.
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