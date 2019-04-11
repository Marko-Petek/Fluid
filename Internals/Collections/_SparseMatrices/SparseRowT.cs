using System;
using System.Text;
using System.Linq;
using SCG = System.Collections.Generic;

using Fluid.Internals.Numerics;

namespace Fluid.Internals.Collections {
   public class SparseRow<T,TArith> : SCG.Dictionary<int,T>
      where T : IEquatable<T>, new()
      where TArith : IArithmetic<T>, new() {
         /// <summary>Contains arithmetic operations.</summary>
         static TArith Arith { get; } = new TArith();
         /// <summary>Width of row if written out explicitly.</summary>
         public int Width { get; protected set; }
         /// <summary>Matrix in which this SparseRow resides.</summary>
         public SparseMat<T,TArith> SparseMat { get; set; }

         /// <summary>Does not assigns width. User of this constructor must do it manually.</summary>
         protected SparseRow() : base() {}
         /// <summary>Create a SparseRow with specified width it would have in explicit form and specified initial capacity.</summary><param name="width">Width it would have in explicit form.</param><param name="capacity">Initial capacity.</param>
         public SparseRow(int width, int capacity = 6) : base(capacity) {
            Width = width;
         }
         /// <summary>Creates a SparseRow as a copy of specified SparseRow.</summary><param name="source">Source to copy.</param>
         public SparseRow(SparseRow<T,TArith> source) : base(source.Count) {
            foreach(var pair in source)
               Add(pair.Key, pair.Value);
         }

         /// <summary>Creates an instance of the same (most derived) type as instance on which it is invoked.</summary><param name="width">Width (length of rows) that matrix would have in its explicit form.</param><param name="capacity">Initial row capacity.</param>
         public virtual SparseRow<T,TArith> CreateSparseRow(int width, int capacity = 6) => new SparseRow<T,TArith>(width, capacity);
         /// <summary>Creates a SparseRow as a copy of specified SparseRow.</summary><param name="source">Source to copy.</param>
         public SparseRow<T,TArith> CreateSparseRow(SparseRow<T,TArith> source) => new SparseRow<T,TArith>(source);
         /// <summary>Create a new SparseRow by copying an array.</summary><param name="source">Array to copy.</param>
         public SparseRow<T,TArith> CreateFromArray(T[] source) {
            var row = CreateSparseRow(source.Length, source.Length);
            for(int i = 0; i < source.Length; ++i)
               Add(i, source[i]);
            return row;
         } 
         /// <summary>Splits SparseRow in two SparseRows. This SparseRow is modified (left remainder), while chopped-off part (right remainder) is put into specified second argument.</summary><param name="virtIndex">Index at which to split. Element at this index will be chopped off and end up as part of returned SparseRow.</param><param name="removedCols">Right remainder will be put in here.</param>
         public SparseRow<T,TArith> SplitAt(int virtIndex) {
            var removedCols = new SparseRow<T,TArith>(Width - virtIndex);
            foreach(var kvPair in this.Where(kvPair => kvPair.Key >= virtIndex)) {
               removedCols.Add(kvPair.Key, kvPair.Value);                         // Add to right remainder.
               Remove(kvPair.Key);                                             // Remove from left remainder.
            }
            return removedCols;
         }
         /// <summary>Append specified SparseRow to this one.</summary><param name="rightCols">SparseRow to append.</param>
         public void MergeWith(SparseRow<T,TArith> rightCols) {
            Width += rightCols.Width;                                      // Readjust width.
            foreach(var kvPair in rightCols)
               this[kvPair.Key] = kvPair.Value;
         }
         /// <summary>Swap two elements specified by virtual indices.</summary><param name="virtIndex1">Virtual index of first element.</param><param name="virtIndex2">Virtual index of second element.</param><remarks>Useful for swapping columns.</remarks>
         public void SwapElms(int virtIndex1, int virtIndex2) {
            bool firstExists = TryGetValue(virtIndex1, out T val1);
            bool secondExists = TryGetValue(virtIndex2, out T val2);
            if(firstExists)
               if(secondExists) {
                  this[virtIndex1] = val2;
                  this[virtIndex2] = val1; }
               else {
                  Remove(virtIndex1);                                   // Element at virtIndex1 becomes 0 and is removed.
                  Add(virtIndex2, val1); }
            else if(secondExists) {
               Add(virtIndex1, val2);
               Remove(virtIndex2); }                                   // Else nothing happens, both are 0.
         }
         /// <summary>Apply element swaps as specified by a given swap matrix.</summary><param name="swapMatrix">SparseMatrix where non-zero element at [i][j] signifies a permutation i --> j.</param>
         public void ApplySwaps(SparseMat<int,IntArithmetic> swapMatrix) {
            foreach(var rowKVPair in swapMatrix)
               foreach(var colPair in rowKVPair.Value)
                  SwapElms(rowKVPair.Key, colPair.Key);
         }
         // TODO: Move this to IO class perhaps?
         /// <summary>Create a string of form {{key1, val1}, {key2, val2}, ..., {keyN,valN}}..</summary>
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
         /// <summary>New indexer definition (hides Dictionary's indexer). Returns 0 for non-existing elements.</summary>
         public new T this[int i] {
            get {
               TryGetValue(i, out T val);                               // Outputs zero if value not found.
               return val; }
            set {
               if(!value.Equals(default(T)))                            // Value different from 0.
                  if(this is DummyRow<T,TArith> dummyRow) {             // Try downcasting to DummyRow.
                     var newRow = CreateSparseRow(Width);               // Add new row to its owner and add value to it.
                     newRow.Add(i, value);
                     dummyRow.SparseMat.Add(dummyRow.Index, newRow); }
                  else
                     base[i] = value;                                   // Indexers adds or modifies if entry already exists.
               else
                  Remove(i); }                                           // Remove if value set is 0.
         }
         public static SparseRow<T,TArith> operator +
            (SparseRow<T,TArith> left, SparseRow<T,TArith> right) {
               var resultRow = (SCG.Dictionary<int,T>) new SparseRow<T,TArith>(right);    // Copy right operand. Result will appear here. Upcast to dictionary so that Dictionary's indexer is used in loop.
               T temp;
               foreach(var kvPair in left) {
                  resultRow.TryGetValue(kvPair.Key, out T val);                  // Then add to existing value.
                  temp = Arith.Add(kvPair.Value, val);
                  if(!temp.Equals(default(T)))                                   // Not zero.
                     resultRow[kvPair.Key] = temp;      // Add to result. Upcast to dictionary so that Dictionary's indexer is used.
                  else                                                           // Zero.
                     resultRow.Remove(kvPair.Key); }
               return (SparseRow<T,TArith>) resultRow;
         }
         public static SparseRow<T,TArith> operator -
            (SparseRow<T,TArith> left, SparseRow<T,TArith> right) {
               var resultRow = (SCG.Dictionary<int,T>) new SparseRow<T,TArith>(left);     // Copy right operand. Result will appear here. Upcast to dictionary so that Dictionary's indexer is used in loop.
               T temp;
               foreach(var kvPair in right) {
                  resultRow.TryGetValue(kvPair.Key, out T val);               // val is 0, if key does not exist
                  temp = Arith.Sub(val, kvPair.Value);
                  if(!temp.Equals(default(T)))
                     resultRow[kvPair.Key] = temp;
                  else
                     resultRow.Remove(kvPair.Key); }
               return (SparseRow<T,TArith>) resultRow;
         }
         /// <summary>Dot (scalar) product.</summary>
         public static T operator *(SparseRow<T,TArith> left, SparseRow<T,TArith> right) {
            T result = default(T);
            foreach(var kvPair in left)
               if(right.TryGetValue(kvPair.Key, out T val))
                  result = Arith.Add(result, Arith.Mul(kvPair.Value, val));
            return result;
         }
         public static SparseRow<T,TArith> operator *(T left, SparseRow<T,TArith> right) {
            if(!left.Equals(default(T))) {                                                // Not zero.
               var result = (SCG.Dictionary<int,T>) new SparseRow<T,TArith>(right);      // Upcast to dictionary so that Dictionary's indexer is used.
               foreach(var key in result.Keys)
                  result[key] = Arith.Mul(result[key], left); //result[key] *= left;
               return (SparseRow<T,TArith>)result; }
            else                                                                          // Zero.
               return new SparseRow<T,TArith>(right.Width);                               // Return empty row.
         }
         /// <summary>Calculates square of Euclidean norm of SparseRow.</summary>
         public T NormSqr() {
            T result = default(T);
            foreach(var val in this.Values)
               result = Arith.Add(result, Arith.Mul(val,val));
            return result;
         }
   }
}