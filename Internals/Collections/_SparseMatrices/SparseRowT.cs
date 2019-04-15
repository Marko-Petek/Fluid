using System;
using System.Text;
using System.Linq;
using SCG = System.Collections.Generic;

using Fluid.Internals.Numerics;

namespace Fluid.Internals.Collections {
   public class SparseRow<T,TArith> : SCG.Dictionary<int,T>,
      IEquatable<SparseRow<T,TArith>>                                         // So that we can equate two SparseRows via the Equals method.
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
         public static SparseRow<T,TArith> CreateSparseRow(int width, int capacity = 6) => new SparseRow<T,TArith>(width, capacity);
         /// <summary>Creates a SparseRow as a copy of specified SparseRow.</summary><param name="source">Source to copy.</param>
         public static SparseRow<T,TArith> CreateSparseRow(SparseRow<T,TArith> source) => new SparseRow<T,TArith>(source);
         /// <summary>Create a new SparseRow by copying an array.</summary><param name="arr">Array to copy.</param>
         public static SparseRow<T,TArith> CreateFromArray(T[] arr, int startCol, int nCols) {
            var row = CreateSparseRow(arr.Length, arr.Length);
            for(int i = startCol; i < startCol + nCols; ++i)
               row.Add(i, arr[i]);
            return row;
         } 
         /// <summary>Splits SparseRow in two SparseRows. This SparseRow is modified (left remainder), while chopped-off part (right remainder) is put into specified second argument.</summary><param name="inx">Index at which to split. Element at this index will be chopped off and end up as part of returned SparseRow.</param><param name="removedCols">Right remainder will be put in here.</param>
         public SparseRow<T,TArith> SplitAt(int inx) {
            var removedCols = new SparseRow<T,TArith>(Width - inx);
            var remKeys = new List<int>(10);                                   // Must not modify collection during enumeration. Therefore create a list of keys to be removed afterwards.
            foreach(var kvPair in this.Where(pair => pair.Key >= inx)) {
               removedCols.Add(kvPair.Key, kvPair.Value);                         // Add to right remainder.
               remKeys.Add(kvPair.Key); }                                              // Add key to be removed afterwards from left remainder.
            foreach(int key in remKeys)
               Remove(key);
            return removedCols;
         }
         /// <summary>Append specified SparseRow to this one.</summary><param name="rightCols">SparseRow to append.</param>
         public void MergeWith(SparseRow<T,TArith> rightCols) {//TODO: Test MergeWith method.
            Width += rightCols.Width;                                      // Readjust width.
            foreach(var kvPair in rightCols)
               this[kvPair.Key] = kvPair.Value;
         }
         /// <summary>Swap two elements specified by indices.</summary><param name="inx1">Index of first element.</param><param name="inx2">Index of second element.</param><remarks>Useful for swapping columns.</remarks>
         public void SwapElms(int inx1, int inx2) {
            //var asDict = (Dictionary<)
            bool firstExists = TryGetValue(inx1, out T val1);
            bool secondExists = TryGetValue(inx2, out T val2);
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
         /// <summary>Apply element swaps as specified by a given swap matrix.</summary><param name="swapMatrix">SparseMatrix where non-zero element at [i][j] signifies a permutation i --> j.</param>
         public void ApplySwaps(SparseMat<int,IntArithmetic> swapMatrix) {
            foreach(var rowKVPair in swapMatrix)
               foreach(var colKVPair in rowKVPair.Value)
                  SwapElms(rowKVPair.Key, colKVPair.Key);
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
               if(!value.Equals(default(T))) {                           // Value different from 0.
                  if(this is DummyRow<T,TArith> dummyRow) {             // Try downcasting to DummyRow.
                     var newRow = new SparseRow<T,TArith>(Width);               // Add new row to its owner and add value to it.
                     newRow.Add(i, value);
                     dummyRow.SparseMat.Add(dummyRow.Index, newRow); }
                  else
                     base[i] = value; }                                  // Indexers adds or modifies if entry already exists.
               else if(!(this is DummyRow<T,TArith>))
                  Remove(i); }                                           // Remove value at given index if value set is 0 and we are not in DummyRow.
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

         public bool Equals(SparseRow<T,TArith> other) {
            foreach(var rowKVPair in this)
               if(!(other.TryGetValue(rowKVPair.Key, out T val) && rowKVPair.Value.Equals(val)))        // Fetch did not suceed or values are not equal.
                  return false;
            return true;
         }
   }
}