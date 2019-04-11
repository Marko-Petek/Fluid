using System;
using System.Linq;
using SCG = System.Collections.Generic;

using Fluid.Internals.Numerics;
using TB = Fluid.Internals.Toolbox;

namespace Fluid.Internals.Collections {
   public class SparseMat<T,TArith> : SCG.Dictionary<int,SparseRow<T,TArith>>,
      IEquatable<SparseMat<T,TArith>>                                                        // So we can compare two SparseMats via Equals method.
      where T : IEquatable<T>, new()
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


         /// <summary>Split matrix on left and right part. Return right part. Element at specified virtual index will be part of right part.</summary><param name="virtJ">Index of element at which to split. This element will be part of right matrix.</param>
         public SparseMat<T,TArith> SplitAtCol(int virtJ) {
            int remWidth = Width - virtJ;
            var removedRightPart = new SparseMat<T,TArith>(remWidth, Height);
            Width = virtJ;                                                 // Adjust width of this Matrix.
            foreach(var rowKVPair in this) {                                  // Split each SparseRow separately.
               var removedCols = rowKVPair.Value.SplitAt(virtJ);             // TODO: Check all split algorithms whether they properly remove rows/cols.
               removedRightPart.Add(rowKVPair.Key, removedCols);
            }
            return removedRightPart;
         }
         /// <summary>Split matrix on upper and lower part. Return lower part. Element at specified virtual index will be part of lower part.</summary><param name="col">Index of element at which to split. This element will be part of lower matrix.</param>
         public SparseMat<T,TArith> SplitAtRow(int virtI) {
            int remWidth = Width;
            int removedHeight = Height - virtI;
            var removedMatrix = new SparseMat<T,TArith>(Width, removedHeight);
            foreach(var row in this.Where(kvPair => kvPair.Key >= virtI)) {
               removedMatrix.Add(row.Key, row.Value);
               Remove(row.Key);
            }
            return removedMatrix;
         }
         /// <summary>Swap rows with specified virtual indices. Correctly handles cases with non-existent rows.</summary><param name="virtI">Virtual index of first row to swap.</param><param name="virtJ">Virtual index of second row to swap.</param>
         public void SwapRows(int virtI, int virtJ) {
            if(TryGetValue(virtI, out var row1)) {                            // Row 1 exists.
               if(TryGetValue(virtJ, out var row2)) {                         // Row 2 exists.
                  base[virtI] = row2;
                  base[virtJ] = row1;
               }
               else {                                                         // Row 2 does not exist.
                  Remove(virtI);
                  base[virtJ] = row1;
               }
            }
            else                                                              // Row 1 does not exist.
               if(TryGetValue(virtJ, out var row2)) {                         // Row 2 exists.
                  base[virtI] = row2;
                  Remove(virtJ);
               }
         }
         /// <summary>Swap columns with specified virtual indices. Correctly handles cases with non-existent rows.</summary><param name="virtI">Virtual index of first column to swap.</param><param name="virtJ">Virtual index of second column to swap.</param>
         public void SwapCols(int virtI, int virtJ) {
            foreach(var row in this)
               row.Value.SwapElms(virtI, virtJ);
         }
         public void ApplyColSwaps(SparseMat<int,IntArithmetic> swapMatrix) {
            foreach(var rowKVPair in swapMatrix)
               foreach(var colKVPair in rowKVPair.Value)
                  SwapCols(rowKVPair.Key, colKVPair.Key);
         }
         public new SparseRow<T,TArith> this[int i] {
            get {
               if(TryGetValue(i, out SparseRow<T,TArith> result))            // Try to fetch value at index i.
                  return result;
               else {
                  DummyRow.SparseMat = this;
                  DummyRow.Index = i;
                  return (SparseRow<T,TArith>)DummyRow;
               }
            }
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
                           rMatRowAsDict.Remove(lMatRowKVPair.Key);
                  }  }
                  else                                                                             // Right row counterpart does not exist (zeros).
                     rMatAsDict.Add(lMatKVPair.Key, new SparseRow<T,TArith>(lMatKVPair.Value));
               }
               return (SparseMat<T,TArith>) rMatAsDict;
         }
         public static SparseRow<T,TArith> operator *
            (SparseMat<T,TArith> lMat, SparseRow<T,TArith> rRow) {
               TB.Assert.AreEqual(lMat.Width, rRow.Width);                                // Check that matrix and row can be multiplied.
               int matrixRowCount = lMat.Count;                                         
               var resultRow = new SparseRow<T,TArith>(lMat.Height, lMat.Count);          // lMat.Count = # of non-zero rows.
               T sum;
               foreach(var lMatKVPair in lMat) {                                          // Go through each row in lMat. Rows that do not exist, create no entries in result row.
                  sum = default(T);
                  foreach(var lMatRowKVPair in lMatKVPair.Value) {                        // Move over each element in lMatRow.
                     if(rRow.TryGetValue(lMatRowKVPair.Key, out T rRowEmt))               // Check its index then search for an element with matching index in rRow.
                        sum = Arith.Add(sum, Arith.Mul(lMatRowKVPair.Value, rRowEmt));    // sum += lMatRowKVPair.Value * rRowEmt; ~~> Add all such contributions to emt with same index in rRow.
                  }
                  resultRow[lMatKVPair.Key] = sum;
               }
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
      }
}