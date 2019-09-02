﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using dbl = System.Double;

using Fluid.Internals.Numerics;
using TB = Fluid.Internals.Toolbox;
using Fluid.TestRef;

namespace Fluid.Internals.Collections {
   /// <summary>A vector with specified dimension which holds values of type τ. Those can use arithmetic defined inside type α.</summary>
   /// <typeparam name="τ">Type of values.</typeparam>
   /// <typeparam name="α">Type defining arithmetic between values.</typeparam>
   public class Vector<τ,α> : Tensor<τ,α>, IEquatable<Vector<τ,α>>
   where τ : IEquatable<τ>, new()
   where α : IArithmetic<τ>, new() {
      new public static Vector<τ,α> Ex { get; } = new Vector<τ,α>();
      public new int Count => CountInternal;
      protected override int CountInternal => Vals.Count;
      public Dictionary<int,τ> Vals { get; internal set; }          // An extra wrapped Dictionary which holds values.
      internal Vector() : base(0) {
         Vals = new Dictionary<int, τ>();
         Rank = 1; }
      internal Vector(List<int> structure, Tensor<τ,α> sup, int cap) : base(structure, 1, sup, 0) {
         Vals = new Dictionary<int, τ>(cap);
      }

      internal Vector(Tensor<τ,α> sup, int cap) : this(sup.Structure, sup, cap) { }
      /// <summary>Creates a type τ vector with arithmetic α, with specified initial capacity.</summary>
      public Vector(int cap) : this(null, null, cap) { }
      /// <summary>Creates a type τ vector with arithmetic α, with specified initial capacity.</summary>
      public Vector(int dim, int cap) : this(new List<int> {dim}, null, cap) { }
      /// <summary>Creates a vector as a deep copy of another. You can optionally specify which meta-fields to copy. Default is AllExceptSup.</summary>
      /// <param name="src"></param>
      public Vector(Vector<τ,α> src, in CopySpecStruct cs) : base(src.Count + cs.ExtraCapacity) {
         Copy(src, this, cs);
      }
      public Vector(Vector<τ,α> src) : this(src, CopySpecs.S342_00) { }
      /// <summary>Creates a deep copy of a vector. You have to provide the already instantiated target.</summary>
      /// <param name="src">Copy source.</param>
      /// <param name="tgt">Copy target.</param>
      public static void Copy(Vector<τ,α> src, Vector<τ,α> tgt, in CopySpecStruct cs) {
         CopyMetaFields(src, tgt, cs.NonValueFieldsSpec, cs.StructureSpec);               // Structure created here.
         if((cs.FieldsSpec & WhichFields.OnlyValues) == WhichFields.OnlyValues)
            tgt.Vals = new Dictionary<int,τ>(src.Vals);
         else
            tgt.Vals = new Dictionary<int,τ>();
      }
      /// <summary>Creates a new vector from an array slice.</summary>
      /// <param name="slc">Array slice.</param>
      public static Vector<τ,α> FromFlatSpec(Span<τ> slc , List<int> structure, Tensor<τ,α> sup) {
         var vec = new Vector<τ,α>(structure, sup, slc.Length);
         for(int i = 0; i < slc.Length; ++i) {
            if(!slc[i].Equals(default(τ)))
               vec.Vals.Add(i, slc[i]); }
         return vec;
      }
      public static Vector<τ,α> FromFlatSpec(Span<τ> slc) =>
         FromFlatSpec(slc, new List<int>(1) { slc.Length }, null);

      public static Vector<τ,α> CreateEmpty(int cap, List<int> structure = null, Tensor<τ,α> sup = null) {
         var vec = new Vector<τ,α>(structure, sup, cap);
         return vec;
      }

      /// <summary>Adds entry to internal dictionary without checking if it is equal to zero.</summary>
      /// <param name="key">Index.</param>
      /// <param name="val">Value.</param>
      internal void Add(int key, τ val) =>
         Vals.Add(key, val);

      /// <remarks> <see cref="TestRefs.VectorIndexer"/> </remarks>
      new public τ this[int i] {
         get {
            Vals.TryGetValue(i, out τ val);
            return val; }
         set {
            if(!value.Equals(default)) {
               Vals[i] = value; }
            else
               Vals.Remove(i); }
      }
      /// <remarks> <see cref="TestRefs.TensorProduct"/> </remarks>
      public override Tensor<τ,α> TnrProduct(Tensor<τ,α> tnr2) {          // TODO: Test TnrProduct with operands which are not top tensors.    
         if(tnr2.Rank > 1) {
            int newRank = Rank + tnr2.Rank;
            var struct1 = CopySubstructure();
            var struct2 = tnr2.CopySubstructure();
            var newStructure = struct1.Concat(struct2).ToList();
            // We must substitute this vector with a tensor whose elements are multiples of tnr2.
            var res = new Tensor<τ,α>(newStructure, newRank, null, Vals.Count);
            foreach(var int_subVal in Vals) {
               int subKey = int_subVal.Key;
               var subVal = int_subVal.Value;
               res.Add(subKey, subVal*tnr2); }
            return res; }
         else {
            var vec2 = (Vector<τ,α>) tnr2;
            return TnrProduct(vec2); }
      }
      /// <remarks> <see cref="TestRefs.VectorTnrProductVector"/> </remarks>
      public Tensor<τ,α> TnrProduct(Vector<τ,α> vec2) {
         int dim1 = Structure.Last();
         int dim2 = vec2.Structure.Last();
         var newStructure = new List<int> {dim1, dim2};
         var res = new Tensor<τ,α>(newStructure, rank: 2, sup: null, Vals.Count);
         foreach(var int_subVal1 in Vals) {
            int subKey = int_subVal1.Key;
            var subVal1 = int_subVal1.Value;
            var newVec = subVal1*vec2;
            res.AddOnlyNonEmpty(subKey, newVec); }
         return res;
      }
      /// <summary>Sums vec2 to vec1. Modifies vec1, does not destroy vec2.</summary>
      /// <param name="vec2">Sumand 2. Is not destroyed.</param>
      /// <remarks><see cref="TestRefs.VectorSum"/></remarks>
      public void Sum(Vector<τ,α> vec2) {
         foreach(var int_subVal2 in vec2.Vals) {
            int subKey = int_subVal2.Key;
            var subVal2 = int_subVal2.Value;
            if(Vals.TryGetValue(subKey, out τ subVal1)) {                  // Value exists in Vec1.
               τ sum = O<τ,α>.A.Sum(subVal1, subVal2);
               if(!sum.Equals(default))                                             // Sum is not zero.
                  Vals[subKey] = sum;
               else
                  Vals.Remove(subKey); }
            else
               Vals.Add(subKey, subVal2); }
      }
      /// <remarks><see cref="TestRefs.VectorSub"/></remarks>
      public void Sub(Vector<τ,α> vec2) {
         foreach(var int_subVal2 in vec2.Vals) {
            int subKey = int_subVal2.Key;
            var subVal2 = int_subVal2.Value;
            if(Vals.TryGetValue(subKey, out τ subVal1)) {                  // Value exists in Vec1.
               τ dif = O<τ,α>.A.Sub(subVal1, subVal2);
               if(!dif.Equals(default))                                             // Sum is not zero.
                  Vals[subKey] = dif;
               else
                  Vals.Remove(subKey); }
            else
               Vals.Add(subKey, O<τ,α>.A.Neg(subVal2)); }
      }
      /// <summary>Multiplies caller with a scalar.</summary>
      /// <param name="scal">Scalar.</param>
      /// <remarks> <see cref="TestRefs.VectorMul"/> </remarks>
      public override void Mul(τ scal) {
         var keys = Vals.Keys.ToArray();                       // To avoid "collection was modified during enumeration".
         foreach(var subKey in keys)
            Vals[subKey] = O<τ,α>.A.Mul(scal, Vals[subKey]);
      }
      /// <summary>Sum two vectors. Does not check substructure match.</summary>
      /// <param name="vec1">Left operand.</param>
      /// <param name="vec2">Right operand.</param>
      /// <remarks><see cref="TestRefs.Op_VectorAddition"/></remarks>
      public static Vector<τ,α> operator + (Vector<τ,α> vec1, Vector<τ,α> vec2) {
         //var newStruc = vec2.CopySubstructure();
         var res = new Vector<τ,α>(vec2, in CopySpecs.S322_04);
         res.AssignStructFromSubStruct(vec2);
         foreach(var int_val1 in vec1.Vals) {
            int key = int_val1.Key;
            var val1 = int_val1.Value;
            if(vec2.Vals.TryGetValue(key, out var val2)) {
               res[key] = O<τ,α>.A.Sum(val1, val2); }
            else {
               res.Add(key, val1); } }
         return res;
      }
      /// <summary>Subtract two vectors. Does not check substructure match.</summary>
      /// <param name="vec1">Left operand.</param>
      /// <param name="vec2">Right operand.</param>
      /// <remarks><see cref="TestRefs.Op_VectorSubtraction"/></remarks>
      public static Vector<τ,α> operator - (Vector<τ,α> vec1, Vector<τ,α> vec2) {
         //var newStruc = vec1.CopySubstructure();
         var res = new Vector<τ,α>(vec1, in CopySpecs.S322_04);
         res.AssignStructFromSubStruct(vec1);
         foreach(var int_val2 in vec2.Vals) {
            int key = int_val2.Key;
            var val2 = int_val2.Value;
            if(vec1.Vals.TryGetValue(key, out var val1)) {
               res[key] = O<τ,α>.A.Sub(val1, val2); }
            else {
               res.Add(key, O<τ,α>.A.Neg(val2)); } }
         return res;
      }
      /// <summary>Modifies this vector by negating each element.</summary>
      public override void Negate() {
         var keys = Vals.Keys.ToArray();                    // We have to do this (access via indexer), because we can't change collection during enumeration.
         for(int i = 0; i < keys.Length; ++i)
            Vals[keys[i]] = O<τ,α>.A.Neg(Vals[keys[i]]);
      }
      /// <summary>Negate operator. Creates a new vector with its own substructure.</summary>
      /// <param name="vec">Vector to negate.</param>
      public static Vector<τ,α> operator - (Vector<τ,α> vec) {
         var newStruc = vec.CopySubstructure();
         var res = new Vector<τ,α>(newStruc, null, vec.Count);
         foreach(var int_val in vec.Vals)
            res.Vals.Add(int_val.Key, O<τ,α>.A.Neg(int_val.Value));
         return res;
      }
      /// <remarks> <see cref="TestRefs.Op_ScalarVectorMultiplication"/> </remarks>
      public static Vector<τ,α> operator * (τ scal, Vector<τ,α> vec) {
         var newStruc = vec.CopySubstructure();
         var res = new Vector<τ,α>(newStruc, null, vec.Count);
         foreach(var int_val in vec.Vals)
            res.Vals.Add(int_val.Key, O<τ,α>.A.Mul(scal, vec[int_val.Key]));
         return res;
      }

      public static Tensor<τ,α> ContractPart2(Vector<τ,α> vec1, Tensor<τ,α> tnr2, int truInx2, List<int> struc3, int conDim) {
         if(tnr2.Rank > 2) {                                                  // Result is tensor.
            Tensor<τ,α> elimTnr2, sumand, sum;
            sum = new Tensor<τ,α>(struc3);                                    // Set sum to a zero tensor.
            for(int i = 0; i < conDim; ++i) {
               elimTnr2 = tnr2.ReduceRank(truInx2, i);
               if(vec1.Vals.TryGetValue(i, out var val) && elimTnr2 != null) {
                  sumand = val*elimTnr2;
                  sum.Sum(sumand); } }
            if(sum.Count != 0)
               return sum;
            else
               return null; }
         else if(tnr2.Rank == 2) {
            Vector<τ,α> elimVec2, sumand, sum;
            sum = new Vector<τ,α>(struc3, null, 4);
            for(int i = 0; i < conDim; ++i) {
               elimVec2 = (Vector<τ,α>) tnr2.ReduceRank(truInx2, i);
               if(vec1.Vals.TryGetValue(i, out var val) && elimVec2 != null) {
                  sumand = val*elimVec2;
                  sum.Sum(sumand); } }
            if(sum.Vals.Count != 0)
               return sum;
            else
               return null; }
         else {                                                               // Result is scalar.
            throw new ArgumentException("Explicitly cast tnr2 to vector before using contract."); }
      }

      public static Tensor<τ,α> Contract(Vector<τ,α> vec1, Tensor<τ,α> tnr2, int natInx2) {
         (List<int> struc3, _, int truInx2, int conDim) = Tensor<τ,α>.ContractPart1(vec1, tnr2, 1, natInx2);
         return ContractPart2(vec1, tnr2, truInx2, struc3, conDim);
      }
      public static τ Contract(Vector<τ,α> vec1, Vector<τ,α> vec2) {
         τ res = default;
         foreach(var int_val1 in vec1.Vals) {
            if(vec2.Vals.TryGetValue(int_val1.Key, out var val2))
               res = O<τ,α>.A.Sum(res, O<τ,α>.A.Mul(int_val1.Value, val2)); }
         return res;
      }
      /// <summary>Dot (scalar) product.</summary>
      /// <remarks> <see cref="TestRefs.Op_VectorDotVector"/> </remarks>
      public static τ operator *(Vector<τ,α> vec1, Vector<τ,α> vec2) =>
         Contract(vec1, vec2);

      /// <summary>Calculates square of Euclidean norm of SparseRow.</summary>
      public τ NormSqr() {
         τ res = default;
         foreach(var kv in Vals)
            res = O<τ,α>.A.Sum(res, O<τ,α>.A.Mul(kv.Value, kv.Value));
         return res;
      }

      public bool Equals(Vector<τ,α> vec2) {
         if(!Vals.Keys.OrderBy(key => key).SequenceEqual(vec2.Vals.Keys.OrderBy(key => key)))    // Keys have to match.
            return false;
         foreach(var int_val in Vals) {
            τ val2 = vec2[int_val.Key];
            if(!int_val.Value.Equals(val2))        // Fetch did not suceed or values are not equal.
               return false; }
         return true;
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