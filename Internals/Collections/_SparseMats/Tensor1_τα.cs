using System;
using System.Text;
using System.Linq;
using SCG = System.Collections.Generic;

using Fluid.Internals.Numerics;

namespace Fluid.Internals.Collections {
   /// <summary>A rank 1 tensor with specified dimension of its only slot for values of type τ. Type τ can use arithmetic defined inside an interface of type α.</summary><typeparam name="τ">Type of values the tensor holds.</typeparam><typeparam name="α">Type of interface that defines arithmetic between values of type τ.</typeparam>
   public class Tensor1<τ,α> : Tensor1<τ>,
      IEquatable<Tensor1<τ,α>>                                         // So that we can equate two rank 1 tensors via the Equals method.
      where τ : IEquatable<τ>, IComparable<τ>, new()
      where α : IArithmetic<τ>, new() {
         /// <summary>Contains arithmetic operations.</summary>
         static α Arith { get; } = new α();
         /// <summary>Does not assign Dim1. User of this constructor must do it manually.</summary>
         protected Tensor1() : base() {}
         /// <summary>Create a rank 1 tensor with arithmetic α, specified dimension of its only slot for values of type τ and specified initial capacity.</summary><param name="width">Width it would have in explicit form.</param><param name="capacity">Initial capacity.</param>
         public Tensor1(int dim1, int capacity = 6) : base(dim1, capacity) { }
         /// <summary>Factory method that creates a rank 1 tensor with specified dimension and initial capacity.</summary><param name="dim1">Number of slots available for values of type τ.</param><param name="capacity">Actual initially assigned memory.</param>
         new public static Tensor1<τ,α> Create(int dim1, int capacity = 6) => new Tensor1<τ,α>(dim1, capacity);
         /// <summary>Create a rank 1 tensor as a copy of another.</summary><param name="source">Rank 1 tensor to copy.</param>
         public Tensor1(Tensor1<τ,α> source) : this(source.Dim1, source.Count) {
            foreach(var pair in source)
               Add(pair.Key, pair.Value);
         }
         /// <summary>Factory method that creates a rank 1 tensor as a copy of another rank 1 tensor.</summary><param name="source">Rank 1 tensor to copy.</param>
         public static Tensor1<τ,α> Create(Tensor1<τ,α> source) => new Tensor1<τ,α>(source);
         /// <summary>Create a new rank 1 tensor by copying values from an array. Manually specify its dimension.</summary><param name="arr">Source array to copy.</param><param name="startArrEmtInx">Index of array element at which copying begins.</param><param name="nArrEmts">How many consecutive array elements to copy.</param><param name="startTenEmtInx">What index to assign to first element copied to tensor.</param><param name="tenDim1">Dimension of new rank 1 tensor.</param>
         new public static Tensor1<τ,α> CreateFromArray(τ[] arr, int startArrEmtInx, int nArrEmts,
            int startTenEmtInx, int tenDim1) {
               var ten1 = Create(tenDim1, arr.Length);
               for(int i = startArrEmtInx, j = startTenEmtInx; i < startArrEmtInx + nArrEmts; ++i, ++j)
                  ten1[j] = arr[i];
               return ten1;
         }
         /// <summary>Create a new rank 1 tensor by copying values from an array. Dimension of created tensor is same as number of copied elements.</summary><param name="arr">Source array to copy.</param><param name="startArrEmtInx">Index of array element at which copying begins.</param><param name="nArrEmts">How many consecutive array elements to copy. Also the dimension of new tensor.</param><param name="startTenEmtInx">What index to assign to first element copied to tensor.</param>
         new public static Tensor1<τ,α> CreateFromArray(τ[] arr, int startArrEmtInx, int nArrEmts,
            int startTenEmtInx) => CreateFromArray(arr, startArrEmtInx, nArrEmts, startTenEmtInx, nArrEmts);
         /// <summary>Create a new rank 1 tensor by copying values from an array. Dimension of created tensor is deduced from number of copied elements. Index of first copied element is set to 0.</summary><param name="arr">Source array to copy.</param><param name="startArrEmtInx">Index of array element at which copying begins.</param><param name="nArrEmts">How many consecutive array elements to copy. Also the dimension of new tensor.</param>
         new public static Tensor1<τ,α> CreateFromArray(τ[] arr, int startArrEmtInx, int nArrEmts) =>
            CreateFromArray(arr, startArrEmtInx, nArrEmts, 0);
         /// <summary>Splits a rank 1 tensor into two rank 1 tensors. Subject is modified (left remainder), while chopped-off part (right remainder) is returned as a separate (re-indexed from 0) rank 1 tensor.</summary><param name="inx">Index at which to split. Element at this index will end up as part of right remainder.</param>
         new public Tensor1<τ,α> SplitAt(int inx) => (Tensor1<τ,α>)base.SplitAt(inx);

         // TODO: Continue refactoring Tensor1<τ,α>.
         /// <summary>New indexer definition (hides Dictionary's indexer). Returns 0 for non-existing elements.</summary>
         public new τ this[int i] {
            get {
               TryGetValue(i, out τ val);                               // Outputs zero if value not found.
               return val; }
            set {
               if(!value.Equals(default(τ))) {                           // Value different from 0.
                  if(this is DumTensor1<τ,α> dummyRow) {             // Try downcasting to DummyRow.
                     var newRow = new Tensor1<τ,α>(Dim1);               // Add new row to its owner and add value to it.
                     newRow.Add(i, value);
                     dummyRow.Tensor2.Add(dummyRow.Index, newRow); }
                  else
                     base[i] = value; }                                  // Indexers adds or modifies if entry already exists.
               else if(!(this is DumTensor1<τ,α>))
                  Remove(i); }                                           // Remove value at given index if value set is 0 and we are not in DummyRow.
         }
         
         public static Tensor1<τ,α> operator +
            (Tensor1<τ,α> lRow, Tensor1<τ,α> rRow) {
               var resRow = new Tensor1<τ,α>(rRow);    // Copy right operand. Result will appear here. Upcast to dictionary so that Dictionary's indexer is used in loop.
               foreach(var lRowKVPair in lRow)
                  resRow[lRowKVPair.Key] = Arith.Add(lRowKVPair.Value, rRow[lRowKVPair.Key]);
               return resRow;
         }
         public static Tensor1<τ,α> operator -
            (Tensor1<τ,α> lRow, Tensor1<τ,α> rRow) {
               var resRow = new Tensor1<τ,α>(lRow);    // Copy right operand. Result will appear here. Upcast to dictionary so that Dictionary's indexer is used in loop.
               foreach(var rRowKVPair in rRow)
                  resRow[rRowKVPair.Key] = Arith.Sub(lRow[rRowKVPair.Key], rRowKVPair.Value);
               return resRow;
         }
         /// <summary>Dot (scalar) product.</summary>
         public static τ operator *(Tensor1<τ,α> lRow, Tensor1<τ,α> rRow) {
            τ res = default(τ);
            foreach(var lRowKVPair in lRow)
               if(rRow.TryGetValue(lRowKVPair.Key, out τ rVal))
                  res = Arith.Add(res, Arith.Mul(lRowKVPair.Value, rVal));
            return res;
         }
         public static Tensor1<τ,α> operator *(τ leftNum, Tensor1<τ,α> rRow) {
            if(!leftNum.Equals(default(τ))) {                                                // Not zero.
               var result = new Tensor1<τ,α>(rRow.Dim1, rRow.Count);      // Upcast to dictionary so that Dictionary's indexer is used.
               foreach(var rRowKVPair in rRow)
                  result.Add(rRowKVPair.Key, Arith.Mul(rRowKVPair.Value, leftNum));
               return result; }
            else                                                                          // Zero.
               return new Tensor1<τ,α>(rRow.Dim1);                               // Return empty row.
         }
         /// <summary>Calculates square of Euclidean norm of SparseRow.</summary>
         public τ NormSqr() {
            τ result = default(τ);
            foreach(var val in this.Values)
               result = Arith.Add(result, Arith.Mul(val,val));
            return result;
         }

         public bool Equals(Tensor1<τ,α> other) {
            foreach(var rowKVPair in this)
               if(!(other.TryGetValue(rowKVPair.Key, out τ val) && rowKVPair.Value.Equals(val)))        // Fetch did not suceed or values are not equal.
                  return false;
            return true;
         }

         public bool Equals(Tensor1<τ,α> other, τ eps) {
            foreach(var rowKVPair in this) {
               if(!(other.TryGetValue(rowKVPair.Key, out τ val)))                      // Fetch did not suceed.
                  return false;
               if(Arith.Abs(Arith.Sub(rowKVPair.Value, val)).CompareTo(eps) > 0 ) // Fetch suceeded but values do not agree within tolerance.
                  return false; }
            return true;                                                              // All values agree within tolerance.
         }
   }
}