using System;
using System.Linq;
using SCG = System.Collections.Generic;

using Fluid.Internals.Numerics;
using TB = Fluid.Internals.Toolbox;
// TODO: Write tests for constructors.
namespace Fluid.Internals.Collections {
   public class SparseMat<τ,α> : SCG.Dictionary<int,SparseRow<τ,α>>,
      IEquatable<SparseMat<τ,α>>                                                        // So we can compare two SparseMats via Equals method.
      where τ : IEquatable<τ>, IComparable<τ>, new()
      where α : IArithmetic<τ>, new() {
         /// <summary>Contains arithmetic operations.</summary>
         static α Arith { get; } = new α();
         /// <summary>Width (length of rows) that matrix would have in its explicit form.</summary>
         public int Width { get; protected set; }
         /// <summary>Height (length of columns) that matrix would have in its explicit form.</summary>
         public int Height { get; protected set; }
         /// <summary>Used by indexer when fetching a row that does not actually exist. If a setter of that row then decides to add an element, row is copied to matrix.</summary>
         internal DummyRow<τ,α> DummyRow { get; }

         /// <summary>Does not assign Width or Height. User of this constructor must do it manually.</summary>
         protected SparseMat() : base() {
            DummyRow = new DummyRow<τ,α>(this, Width);
         }
         /// <summary>Create a SparseMatrix with given width, height and initial row capacity.</summary><param name="width">Width (length of rows) that matrix would have in its explicit form.</param><param name="height">Height (length of columns) that matrix would have in its explicit form.</param><param name="capacity">Initial row capacity.</param>
         public SparseMat(int width, int height, int capacity = 6) : base(capacity) {
            DummyRow = new DummyRow<τ,α>(this, Width);
            Width = width;
            Height = height;
         }
         /// <summary>Create a copy of specified SparseMatrix.</summary><param name="source">Source SparseMatrix to copy.</param>
         public SparseMat(SparseMat<τ,α> source) : this(source.Width, source.Height, source.Count) {
            foreach(var matKVPair in source)
               Add(matKVPair.Key, new SparseRow<τ,α>(matKVPair.Value));
         }

         public static SparseMat<τ,α> CreateFromArray(τ[][] arr) {
            int nRows = arr.Length;
            int nCols = arr[0].Length;
            var sparseMat = new SparseMat<τ,α>(nCols, nRows, nCols*nRows);
            for(int i = 0; i < nRows; ++i)
               for(int j = 0; j < nCols; ++j)
                  sparseMat[i][j] = arr[i][j];
            return sparseMat;
         }
         public static SparseMat<τ,α> CreateFromArray(τ[] arr, int allRows, int startRow,
            int nRows, int startCol, int nCols, int width, int height, int startRowInx = 0, int startColInx = 0) {
               int allCols = arr.Length/allRows;
               var sparseMat = new SparseMat<τ,α>(width, height, nCols*nRows);
               for(int i = startRow, k = startRowInx; i < startRow + nRows; ++i, ++k)
                  for(int j = startCol, l = startColInx; j < startCol + nCols; ++j, ++l)
                     sparseMat[k][l] = arr[i*allCols + j];
               return sparseMat;
         }

         public static SparseMat<τ,α> CreateFromArray(τ[] arr, int allRows, int startRow,
            int nRows, int startCol, int nCols) =>
               CreateFromArray(arr, allRows, startRow, nRows, startCol, nCols, nCols, nRows);

         public static SparseMat<τ,α> CreateFromSpan(Span<τ> slice, int nRows) {
            int nCols = slice.Length / nRows;
            var sparseMat = new SparseMat<τ,α>(nCols, nRows, nCols*nRows);
            for(int i = 0; i < nRows; ++i)
               for(int j = 0; j < nCols; ++j)
                  sparseMat[i][j] = slice[i*nCols + j];
            return sparseMat;
         }
         /// <summary>Split matrix on left and right part. Return right part. Element at specified index will be part of right part.</summary><param name="colInx">Index of element at which to split. This element will be part of right matrix.</param>
         public SparseMat<τ,α> SplitAtCol(int colInx) {
            int remWidth = Width - colInx;
            var remMat = new SparseMat<τ,α>(remWidth, Height);
            Width = colInx;                                                 // Adjust width of this Matrix.
            foreach(var matKVPair in this) {                                  // Split each SparseRow separately.
               var remRow = matKVPair.Value.SplitAt(colInx);
               remMat.Add(matKVPair.Key, remRow); }
            return remMat;
         }
         /// <summary>Split matrix on upper and lower part. Return lower part. Element at specified index will be part of lower part.</summary><param name="col">Index of element at which to split. This element will be part of lower matrix.</param>
         public SparseMat<τ,α> SplitAtRow(int inx) {
            var remMat = new SparseMat<τ,α>(Width, Height - inx); 
            foreach(var matKVPair in this.Where(kvPair => kvPair.Key >= inx))
               remMat.Add(matKVPair.Key - inx, matKVPair.Value);
            foreach(var key in remMat.Keys)
               Remove(key + inx);
            return remMat;
         }
         /// <summary>Swap rows with specified virtual indices. Correctly handles cases with non-existent rows.</summary><param name="inx1">Virtual index of first row to swap.</param><param name="inx2">Virtual index of second row to swap.</param>
         public void SwapRows(int inx1, int inx2) {
            if(TryGetValue(inx1, out SparseRow<τ,α> row1)) {               // Row 1 exists.
               if(TryGetValue(inx2, out SparseRow<τ,α> row2)) {          // Row 2 exists.
                  base[inx1] = row2;
                  base[inx2] = row1; }
               else {                                                         // Row 2 does not exist.
                  Remove(inx1);
                  base[inx2] = row1; } }
            else                                                              // Row 1 does not exist.
               if(TryGetValue(inx2, out var row2)) {                          // Row 2 exists.
                  Remove(inx2);
                  base[inx1] = row2; }
         }
         /// <summary>Swap columns with specified virtual indices. Correctly handles cases with non-existent rows.</summary><param name="inx1">Virtual index of first column to swap.</param><param name="inx2">Virtual index of second column to swap.</param>
         public void SwapCols(int inx1, int inx2) {
            foreach(var matKVPair in this)
               matKVPair.Value.SwapElms(inx1, inx2);              // Swap elms of row.
         }
         public void ApplyColSwaps(SCG.Dictionary<int,int> swapDict) {
            foreach(var rowKVPair in swapDict)
               SwapCols(rowKVPair.Key, rowKVPair.Value);
         }
         public new SparseRow<τ,α> this[int i] {
            get {
               if(TryGetValue(i, out SparseRow<τ,α> result))            // Try to fetch value at index i.
                  return result;
               else {
                  DummyRow.SparseMat = this;
                  DummyRow.Index = i;
                  return (SparseRow<τ,α>)DummyRow; } }
            set {
               if(value.Count != 0)
                  base[i] = value;
               else
                  Remove(i); }
         }
         /// <summary>Creates a SparseMat that is a sum of two operand SparseMats.</summary><param name="lMat">Left operand.</param><param name="rMat">Right operand.</param>
         public static SparseMat<τ,α> operator +
            (SparseMat<τ,α> lMat, SparseMat<τ,α> rMat) {
               TB.Assert.AreEqual(lMat.Width, rMat.Width);                                         // Check that width and height of operands match.
               TB.Assert.AreEqual(lMat.Height, rMat.Height);
               var res = new SparseMat<τ,α>(rMat);
               foreach(var lMatKVPair in lMat) {
                  res[lMatKVPair.Key] = lMatKVPair.Value + rMat[lMatKVPair.Key]; }
                  // if(rMat.TryGetValue(lMatKVPair.Key, out SparseRow<T,TArith> rRow)) {    // Right row counterpart exists.
                  //    var resRow = lMatKVPair.Value + rRow;
                  //    if(resRow.Count != 0)
                  //       res.Add(lMatKVPair.Key, resRow); }
                  // else                                                                             // Right row counterpart does not exist (zeros).
                  //    res.Add(lMatKVPair.Key, new SparseRow<T,TArith>(lMatKVPair.Value)); }
               return res;
         }
         public static SparseRow<τ,α> operator *
            (SparseMat<τ,α> lMat, SparseRow<τ,α> rRow) {
               TB.Assert.AreEqual(lMat.Width, rRow.Width);                                 // Check that matrix and row can be multiplied.                                        
               var resultRow = new SparseRow<τ,α>(lMat.Width);           // lMat.Count = # of non-zero rows.
               τ sum;
               foreach(var lMatKVPair in lMat) {                                           // Go through each row in lMat. Rows that do not exist, create no entries in result row.
                  sum = default(τ);
                  foreach(var lRowKVPair in lMatKVPair.Value) {                         // Move over each element in lMatRow.
                     if(rRow.TryGetValue(lRowKVPair.Key, out τ rEmt))                // Check its index then search for an element with matching index in rRow.
                        sum = Arith.Add(sum, Arith.Mul(lRowKVPair.Value, rEmt)); }   // sum += lMatRowKVPair.Value * rRowEmt; ~~> Add all such contributions to emt with same index in rRow.
                  resultRow[lMatKVPair.Key] = sum; }
               return resultRow;
         }
         public static SparseRow<τ,α> operator *
            (SparseRow<τ,α> lRow, SparseMat<τ,α> rMat) {
               TB.Assert.AreEqual(rMat.Width, lRow.Width);                                      // Check that matrix and row can be multiplied.
               var resultRow = new SparseRow<τ,α>(rMat.Height, rMat.Width);
               foreach(var rMatKVPair in rMat)
                  if(lRow.TryGetValue(rMatKVPair.Key, out τ lRowVal)) {
                     foreach(var rRowKVPair in rMatKVPair.Value)
                        resultRow[rRowKVPair.Key] = Arith.Add(resultRow[rRowKVPair.Key], Arith.Mul(lRowVal, rRowKVPair.Value)); }
               return resultRow;
         }

         
         /// <summary>Compare two SparseMats.</summary><param name="other">The other SparseMat to compare with.</param>
         public bool Equals(SparseMat<τ,α> other) {
            foreach(var matKVPair in this)
               if(!(other.TryGetValue(matKVPair.Key, out SparseRow<τ,α> val) && matKVPair.Value.Equals(val)))        // Fetch did not suceed or values are not equal.
                  return false;
            return true;
         }

         public bool Equals(SparseMat<τ,α> other, τ eps) {
            foreach(var matKVPair in this) {
               if(!(other.TryGetValue(matKVPair.Key, out SparseRow<τ,α> otherRow)))  // Fetch did not suceed.
                  return false;
               if(!matKVPair.Value.Equals(otherRow, eps))                             // Fetch suceeded and values do not agree within tolerance.
                  return false; }
            return true;                                                              // All values agree within tolerance.
         }
   }
}