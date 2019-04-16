using System;
using System.Linq;
using SCG = System.Collections.Generic;

using Fluid.Internals.Numerics;
using TB = Fluid.Internals.Toolbox;

namespace Fluid.Internals.Collections {
   public class SparseMat<T,TArith> : SCG.Dictionary<int,SparseRow<T,TArith>>,
      IEquatable<SparseMat<T,TArith>>                                                        // So we can compare two SparseMats via Equals method.
      where T : IEquatable<T>, IComparable<T>, new()
      where TArith : IArithmetic<T>, new() {
         /// <summary>Contains arithmetic operations.</summary>
         static TArith Arith { get; } = new TArith();
         /// <summary>Width (length of rows) that matrix would have in its explicit form.</summary>
         public int Width { get; protected set; }
         /// <summary>Height (length of columns) that matrix would have in its explicit form.</summary>
         public int Height { get; protected set; }
         /// <summary>Used by indexer when fetching a row that does not actually exist. If a setter of that row then decides to add an element, row is copied to matrix.</summary>
         internal DummyRow<T,TArith> DummyRow { get; }

         /// <summary>Does not assign Width or Height. User of this constructor must do it manually.</summary>
         protected SparseMat() : base() {
            DummyRow = new DummyRow<T,TArith>(this, Width);
         }
         /// <summary>Create a SparseMatrix with given width, height and initial row capacity.</summary><param name="width">Width (length of rows) that matrix would have in its explicit form.</param><param name="height">Height (length of columns) that matrix would have in its explicit form.</param><param name="capacity">Initial row capacity.</param>
         public SparseMat(int width, int height, int capacity = 6) : base(capacity) {
            DummyRow = new DummyRow<T,TArith>(this, Width);
            Width = width;
            Height = height;
         }
         /// <summary>Create a copy of specified SparseMatrix.</summary><param name="source">Source SparseMatrix to copy.</param>
         public SparseMat(SparseMat<T,TArith> source) : base(source) {
            DummyRow = new DummyRow<T,TArith>(this, Width);
            Width = source.Width;
            Height = source.Height;
         }

         public static SparseMat<T,TArith> CreateFromArray(T[][] arr) {
            int nRows = arr.Length;
            int nCols = arr[0].Length;
            var sparseMat = new SparseMat<T,TArith>(nCols, nRows, nCols*nRows);
            for(int i = 0; i < nRows; ++i)
               for(int j = 0; j < nCols; ++j)
                  sparseMat[i][j] = arr[i][j];
            return sparseMat;
         }
         public static SparseMat<T,TArith> CreateFromArray(T[] arr, int allRows, int startRow, int nRows, int startCol, int nCols, int width, int height) {
            int allCols = arr.Length/allRows;
            var sparseMat = new SparseMat<T,TArith>(width, height, nCols*nRows);
            for(int i = startRow; i < startRow + nRows; ++i)
               for(int j = startCol; j < startCol + nCols; ++j)
                  sparseMat[i][j] = arr[i*allCols + j];
            return sparseMat;
         }

         public static SparseMat<T,TArith> CreateFromSpan(Span<T> slice, int nRows) {
            int nCols = slice.Length / nRows;
            var sparseMat = new SparseMat<T,TArith>(nCols, nRows, nCols*nRows);
            for(int i = 0; i < nRows; ++i)
               for(int j = 0; j < nCols; ++j)
                  sparseMat[i][j] = slice[i*nRows + j];
            return sparseMat;
         }
         /// <summary>Split matrix on left and right part. Return right part. Element at specified virtual index will be part of right part.</summary><param name="colInx">Index of element at which to split. This element will be part of right matrix.</param>
         public SparseMat<T,TArith> SplitAtCol(int colInx) {
            int remWidth = Width - colInx;
            var removedRightPart = new SparseMat<T,TArith>(remWidth, Height);
            Width = colInx;                                                 // Adjust width of this Matrix.
            foreach(var rowKVPair in this) {                                  // Split each SparseRow separately.
               var removedCols = rowKVPair.Value.SplitAt(colInx);
               removedRightPart.Add(rowKVPair.Key, removedCols); }
            return removedRightPart;
         }
         /// <summary>Split matrix on upper and lower part. Return lower part. Element at specified virtual index will be part of lower part.</summary><param name="col">Index of element at which to split. This element will be part of lower matrix.</param>
         public SparseMat<T,TArith> SplitAtRow(int rowInx) {
            int remWidth = Width;
            int removedHeight = Height - rowInx;
            var removedMatrix = new SparseMat<T,TArith>(Width, removedHeight);
            var remKeys = new List<int>(10); 
            foreach(var row in this.Where(kvPair => kvPair.Key >= rowInx)) {
               removedMatrix.Add(row.Key, row.Value);
               remKeys.Add(row.Key); }
            foreach(var key in remKeys)
               Remove(key);
            return removedMatrix;
         }
         /// <summary>Swap rows with specified virtual indices. Correctly handles cases with non-existent rows.</summary><param name="inx1">Virtual index of first row to swap.</param><param name="inx2">Virtual index of second row to swap.</param>
         public void SwapRows(int inx1, int inx2) {
            if(TryGetValue(inx1, out SparseRow<T,TArith> row1)) {               // Row 1 exists.
               if(TryGetValue(inx2, out SparseRow<T,TArith> row2)) {          // Row 2 exists.
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
         public new SparseRow<T,TArith> this[int i] {
            get {
               if(TryGetValue(i, out SparseRow<T,TArith> result))            // Try to fetch value at index i.
                  return result;
               else {
                  DummyRow.SparseMat = this;
                  DummyRow.Index = i;
                  return (SparseRow<T,TArith>)DummyRow; } }
         }
         /// <summary>Creates a SparseMat that is a sum of two operand SparseMats.</summary><param name="lMat">Left operand.</param><param name="rMat">Right operand.</param>
         public static SparseMat<T,TArith> operator +
            (SparseMat<T,TArith> lMat, SparseMat<T,TArith> rMat) {
               TB.Assert.AreEqual(lMat.Width, rMat.Width);                                         // Check that width and height of operands match.
               TB.Assert.AreEqual(lMat.Height, rMat.Height);
               var rMatAsDict = (SCG.Dictionary<int,SparseRow<T,TArith>>)                          // Copy right operand. Result will appear here.
                  new SparseMat<T,TArith>(rMat);                                                   // Upcast to dictionary so that Dictionary's indexer is used.
               SCG.Dictionary<int,T> rMatRowAsDict;
               T rMatRowEmt;
               foreach(var lMatKVPair in lMat) {
                  if(rMatAsDict.TryGetValue(lMatKVPair.Key, out SparseRow<T,TArith> rMatRow)) {    // Right row counterpart exists.
                     rMatRowAsDict = (SCG.Dictionary<int,T>) rMatRow;
                     foreach(var lMatRowKVPair in lMatKVPair.Value) {
                        rMatRowEmt = Arith.Add(rMatRow[lMatRowKVPair.Key], lMatRowKVPair.Value);
                        if(!rMatRowEmt.Equals(default(T)))                                         // Not zero.
                           rMatRowAsDict[lMatRowKVPair.Key] = rMatRowEmt;
                        else
                           rMatRowAsDict.Remove(lMatRowKVPair.Key); } }
                  else                                                                             // Right row counterpart does not exist (zeros).
                     rMatAsDict.Add(lMatKVPair.Key, new SparseRow<T,TArith>(lMatKVPair.Value)); }
               return (SparseMat<T,TArith>) rMatAsDict;
         }
         public static SparseRow<T,TArith> operator *
            (SparseMat<T,TArith> lMat, SparseRow<T,TArith> rRow) {
               TB.Assert.AreEqual(lMat.Width, rRow.Width);                                 // Check that matrix and row can be multiplied.
               int matrixRowCount = lMat.Count;                                         
               var resultRow = new SparseRow<T,TArith>(lMat.Height, lMat.Count);           // lMat.Count = # of non-zero rows.
               T sum;
               foreach(var lMatKVPair in lMat) {                                           // Go through each row in lMat. Rows that do not exist, create no entries in result row.
                  sum = default(T);
                  foreach(var lMatRowKVPair in lMatKVPair.Value) {                         // Move over each element in lMatRow.
                     if(rRow.TryGetValue(lMatRowKVPair.Key, out T rRowEmt))                // Check its index then search for an element with matching index in rRow.
                        sum = Arith.Add(sum, Arith.Mul(lMatRowKVPair.Value, rRowEmt)); }   // sum += lMatRowKVPair.Value * rRowEmt; ~~> Add all such contributions to emt with same index in rRow.
                  resultRow[lMatKVPair.Key] = sum; }
               return resultRow;
         }
         public static SparseRow<T,TArith> operator *
            (SparseRow<T,TArith> lRow, SparseMat<T,TArith> rMat) {
               TB.Assert.AreEqual(rMat.Width, lRow.Width);                                      // Check that matrix and row can be multiplied.
               var resultRow = new SparseRow<T,TArith>(rMat.Height, rMat.Width);
               foreach(var rMatKVPair in rMat)
                  foreach(var rMatRowEmt in rMatKVPair.Value)
                     if(lRow.TryGetValue(rMatRowEmt.Key, out T lRowEmt))
                        resultRow[rMatKVPair.Key] = Arith.Add(
                           resultRow[rMatKVPair.Key], Arith.Mul(lRowEmt, rMatRowEmt.Value));    // resultRow[rMatKVPair.Key] += lRowEmt * rMatRowEmt.Value;
               return resultRow;
         }
         /// <summary>Compare two SparseMats.</summary><param name="other">The other SparseMat to compare with.</param>
         public bool Equals(SparseMat<T,TArith> other) {
            foreach(var matKVPair in this)
               if(!(other.TryGetValue(matKVPair.Key, out SparseRow<T,TArith> val) && matKVPair.Value.Equals(val)))        // Fetch did not suceed or values are not equal.
                  return false;
            return true;
         }

         public bool Equals(SparseMat<T,TArith> other, T eps) {
            foreach(var matKVPair in this) {
               if(!(other.TryGetValue(matKVPair.Key, out SparseRow<T,TArith> otherRow)))  // Fetch did not suceed.
                  return false;
               if(!matKVPair.Value.Equals(otherRow, eps))                             // Fetch suceeded and values do not agree within tolerance.
                  return false; }
            return true;                                                              // All values agree within tolerance.
         }
   }
}