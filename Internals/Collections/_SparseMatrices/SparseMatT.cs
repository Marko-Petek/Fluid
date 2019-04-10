using System;
using System.Linq;
using SCG = System.Collections.Generic;

using Fluid.Internals.Numerics;
using TB = Fluid.Internals.Toolbox;

namespace Fluid.Internals.Collections {
   public class SparseMat<T,TArith> : SCG.Dictionary<int,SparseRow<T,TArith>>
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
      protected SparseMat() : base() {}
      /// <summary>Create a SparseMatrix with given width, height and initial row capacity.</summary><param name="width">Width (length of rows) that matrix would have in its explicit form.</param><param name="height">Height (length of columns) that matrix would have in its explicit form.</param><param name="capacity">Initial row capacity.</param>
      public SparseMat(int width, int height, int capacity = 6) : base(capacity) {
         Width = width;
         Height = height;
      }
      /// <summary>Create a copy of specified SparseMatrix.</summary><param name="source">Source SparseMatrix to copy.</param>
      public SparseMat(SparseMat<T,TArith> source) : base(source) {
         Width = source.Width;
         Height = source.Height;
      }


      /// <summary>Split matrix on left and right part. Return right part. Element at specified virtual index will be part of right part.</summary><param name="virtJ">Index of element at which to split. This element will be part of right matrix.</param>
      protected SparseMat<T,TArith> SplitAtCol(int virtJ) {
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
      protected SparseMat<T,TArith> SplitAtRow(int virtI) {
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
      /// <summary>Creates a SparseMat that is a sum of two operand SparseMats.</summary><param name="leftMat">Left operand.</param><param name="rightMat">Right operand.</param>
      public static SparseMat<T,TArith> operator + (SparseMat<T,TArith> leftMat, SparseMat<T,TArith> rightMat) {
         TB.Assert.AreEqual(leftMat.Width, rightMat.Width);             // Check that width and height of operands match.
         TB.Assert.AreEqual(leftMat.Height, rightMat.Height);
         var rightRowDict = (SCG.Dictionary<int,SparseRow<T,TArith>>) new SparseMat<T,TArith>(rightMat);                     // Copy right operand. Result will appear here. Upcast to dictionary so that Dictionary's indexer is used.
         //SCG.Dictionary<int,T> leftRowDict;
         SCG.Dictionary<int,T> rightElmDict;
         T rightRowElm;
         foreach(var leftMatKVPair in leftMat) {
            //leftRowDict = (SCG.Dictionary<int,T>) leftKVPair.Value;
            if(rightRowDict.TryGetValue(leftMatKVPair.Key, out SparseRow<T,TArith> rightRow)) {        // Right row counterpart exists.
               rightElmDict = (SCG.Dictionary<int,T>) rightRow;
               foreach(var leftRowElm in leftMatKVPair.Value) {
                  rightRowElm = Arith.Add(rightRow[leftRowElm.Key], leftRowElm.Value);
                  if(!rightRowElm.Equals(default(T)))                               // Not zero.
                     rightElmDict[leftRowElm.Key] = rightRowElm;
                  else                                      // temp == 0 && resultRow[kvPair.Key] != 0
                     rightElmDict.Remove(leftRowElm.Key);
               }
            }
            else                                                          // Right row counterpart does not exist (zeros).
               rightRowDict.Add(leftMatKVPair.Key, new SparseRow<T,TArith>(leftMatKVPair.Value));
         }
         return rightRowDict;
      }

      public static SparseRowDouble operator * (SparseMatDouble mat, SparseRowDouble row) {
         Assert.AreEqual(mat.Width, row.Width);                                  // Check that matrix and row can be multiplied.

         // 1) Go through each row in left matrix. Rows that do not exist, create no entries in result row.
         // 2) Move over each element in row i, check its virtual index, then search for an element with
         //      matching virtual index in right row.
         // 3) Add all such contributions to element with virtual index i in right row.
         // 4) Return result row.

         int matrixRowCount = mat.Count;                                         // Number of occupied (non-zero) rows.
         var resultRow = new SparseRowDouble(mat.Height, matrixRowCount);
         double temp;

         foreach(var rowPair in mat) {
               temp = 0.0;

               foreach(var colPair in rowPair.Value) {
                  if(row.TryGetValue(colPair.Key, out double rowElm))
                     temp += colPair.Value * rowElm;
               }
               resultRow[rowPair.Key] = temp;
         }
         return resultRow;
      }

      public static SparseRowDouble operator * (SparseRowDouble row, SparseMatDouble mat) {
         Assert.AreEqual(mat.Width, row.Width);                                  // Check that matrix and row can be multiplied.
         var resultRow = new SparseRowDouble(mat.Height, mat.Width);

         foreach(var matRow in mat) {

               foreach(var matCol in matRow.Value) {

                  if(row.TryGetValue(matCol.Key, out var leftRowElm))
                     resultRow[matRow.Key] += leftRowElm * matCol.Value;
               }
         }
         return resultRow;
      }
   }
}