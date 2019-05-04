using System;
using System.Text;
using System.Linq;
using SCG = System.Collections.Generic;

using Fluid.Internals.Numerics;

namespace Fluid.Internals.Collections {
   public class Tensor1<τ,α> : Tensor1<τ>,
      IEquatable<Tensor1<τ,α>>                                         // So that we can equate two SparseRows via the Equals method.
      where τ : IEquatable<τ>, IComparable<τ>, new()
      where α : IArithmetic<τ>, new() {
         /// <summary>Contains arithmetic operations.</summary>
         static α Arith { get; } = new α();

         /// <summary>Does not assigns width. User of this constructor must do it manually.</summary>
         protected Tensor1() : base() {}
         /// <summary>Create a SparseRow with specified width it would have in explicit form and specified initial capacity.</summary><param name="width">Width it would have in explicit form.</param><param name="capacity">Initial capacity.</param>
         public Tensor1(int width, int capacity = 6) : base(capacity) {
            Dim1 = width;
         }
         /// <summary>Creates a SparseRow as a copy of specified SparseRow.</summary><param name="source">Source to copy.</param>
         public Tensor1(Tensor1<τ,α> source) : this(source.Dim1, source.Count) {
            foreach(var pair in source)
               Add(pair.Key, pair.Value);
         }

         /// <summary>Creates an instance of the same (most derived) type as instance on which it is invoked.</summary><param name="width">Width (length of rows) that matrix would have in its explicit form.</param><param name="capacity">Initial row capacity.</param>
         new public static Tensor1<τ,α> CreateSparseRow(int width, int capacity = 6) => new Tensor1<τ,α>(width, capacity);
         /// <summary>Creates a SparseRow as a copy of specified SparseRow.</summary><param name="source">Source to copy.</param>
         public static Tensor1<τ,α> CreateSparseRow(Tensor1<τ,α> source) => new Tensor1<τ,α>(source);
         /// <summary>Create a new SparseRow by copying an array.</summary><param name="arr">Array to copy.</param>
         new public static Tensor1<τ,α> CreateFromArray(τ[] arr, int startCol, int nCols,
            int startInx, int width) {
               var row = CreateSparseRow(width, arr.Length);
               for(int i = startCol, j = startInx; i < startCol + nCols; ++i, ++j)
                  row[j] = arr[i];
               return row;
         }

         new public static Tensor1<τ,α> CreateFromArray(τ[] arr, int startCol, int nCols,
            int startInx) => CreateFromArray(arr, startCol, nCols, startInx, nCols);

         new public static Tensor1<τ,α> CreateFromArray(τ[] arr, int startCol, int nCols) =>
            CreateFromArray(arr, startCol, nCols, 0);

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