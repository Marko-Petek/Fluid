#if false
using System;
using System.Linq;
using SCG = System.Collections.Generic;

using Fluid.Internals.Numerics;
using TB = Fluid.Internals.Toolbox;
namespace Fluid.Internals.Collections {
   public class Tensor2<τ,α> : Tensor2<τ>,
      IEquatable<Tensor2<τ,α>>                                                        // So we can compare two SparseMats via Equals method.
      where τ : IEquatable<τ>, IComparable<τ>, new()
      where α : IArithmetic<τ>, new() {
         /// <summary>Contains arithmetic operations.</summary>
         static α Arith { get; } = new α();

         /// <summary>Does not assign Width or Height. User of this constructor must do it manually.</summary>
         protected Tensor2() : base() {
            DumTnr1 = new DumTensor1<τ>(this, Dim1);
         }
         /// <summary>Create a SparseMatrix with given width, height and initial row capacity.</summary><param name="width">Width (length of rows) that matrix would have in its explicit form.</param><param name="height">Height (length of columns) that matrix would have in its explicit form.</param><param name="capacity">Initial row capacity.</param>
         public Tensor2(int width, int height, int capacity = 6) : base(capacity) {
            DumTnr1 = new DumTensor1<τ,α>(this, Dim1);
            Dim1 = width;
            Height = height;
         }
         /// <summary>Create a copy of specified SparseMatrix.</summary><param name="source">Source SparseMatrix to copy.</param>
         public Tensor2(Tensor2<τ,α> source) : this(source.Dim1, source.Height, source.Count) {
            foreach(var matKVPair in source)
               Add(matKVPair.Key, new Tensor1<τ,α>(matKVPair.Value));
         }

         
         /// <summary>Split matrix on left and right part. Return right part. Element at specified index will be part of right part.</summary><param name="colInx">Index of element at which to split. This element will be part of right matrix.</param>
         public Tensor2<τ,α> SplitAtCol(int colInx) {
            int remWidth = Dim1 - colInx;
            var remMat = new Tensor2<τ,α>(remWidth, Height);
            Dim1 = colInx;                                                 // Adjust width of this Matrix.
            foreach(var matKVPair in this) {                                  // Split each SparseRow separately.
               var remRow = matKVPair.Value.SplitAt(colInx);
               remMat.Add(matKVPair.Key, remRow); }
            return remMat;
         }
         /// <summary>Split matrix on upper and lower part. Return lower part. Element at specified index will be part of lower part.</summary><param name="col">Index of element at which to split. This element will be part of lower matrix.</param>
         public Tensor2<τ,α> SplitAtRow(int inx) {
            var remMat = new Tensor2<τ,α>(Dim1, Height - inx); 
            foreach(var matKVPair in this.Where(kvPair => kvPair.Key >= inx))
               remMat.Add(matKVPair.Key - inx, matKVPair.Value);
            foreach(var key in remMat.Keys)
               Remove(key + inx);
            return remMat;
         }
         /// <summary>Swap rows with specified virtual indices. Correctly handles cases with non-existent rows.</summary><param name="inx1">Virtual index of first row to swap.</param><param name="inx2">Virtual index of second row to swap.</param>
         public void SwapRows(int inx1, int inx2) {
            if(TryGetValue(inx1, out Tensor1<τ,α> row1)) {               // Row 1 exists.
               if(TryGetValue(inx2, out Tensor1<τ,α> row2)) {          // Row 2 exists.
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
               matKVPair.Value.Swap(inx1, inx2);              // Swap elms of row.
         }
         public void ApplyColSwaps(SCG.Dictionary<int,int> swapDict) {
            foreach(var rowKVPair in swapDict)
               SwapCols(rowKVPair.Key, rowKVPair.Value);
         }
         public new Tensor1<τ,α> this[int i] {
            get {
               if(TryGetValue(i, out Tensor1<τ,α> result))            // Try to fetch value at index i.
                  return result;
               else {
                  DumTnr1.SparseMat = this;
                  DumTnr1.Index = i;
                  return (Tensor1<τ,α>)DumTnr1; } }
            set {
               if(value.Count != 0)
                  base[i] = value;
               else
                  Remove(i); }
         }
         /// <summary>Creates a SparseMat that is a sum of two operand SparseMats.</summary><param name="lMat">Left operand.</param><param name="rMat">Right operand.</param>
         public static Tensor2<τ,α> operator +
            (Tensor2<τ,α> lMat, Tensor2<τ,α> rMat) {
               TB.Assert.AreEqual(lMat.Dim1, rMat.Dim1);                                         // Check that width and height of operands match.
               TB.Assert.AreEqual(lMat.Height, rMat.Height);
               var res = new Tensor2<τ,α>(rMat);
               foreach(var lMatKVPair in lMat) {
                  res[lMatKVPair.Key] = lMatKVPair.Value + rMat[lMatKVPair.Key]; }
               return res;
         }
         public static Tensor1<τ,α> operator *
            (Tensor2<τ,α> lMat, Tensor1<τ,α> rRow) {
               TB.Assert.AreEqual(lMat.Dim1, rRow.Dim);                                 // Check that matrix and row can be multiplied.                                        
               var resultRow = new Tensor1<τ,α>(lMat.Dim1);           // lMat.Count = # of non-zero rows.
               τ sum;
               foreach(var lMatKVPair in lMat) {                                           // Go through each row in lMat. Rows that do not exist, create no entries in result row.
                  sum = default(τ);
                  foreach(var lRowKVPair in lMatKVPair.Value) {                         // Move over each element in lMatRow.
                     if(rRow.TryGetValue(lRowKVPair.Key, out τ rEmt))                // Check its index then search for an element with matching index in rRow.
                        sum = Arith.Add(sum, Arith.Mul(lRowKVPair.Value, rEmt)); }   // sum += lMatRowKVPair.Value * rRowEmt; ~~> Add all such contributions to emt with same index in rRow.
                  resultRow[lMatKVPair.Key] = sum; }
               return resultRow;
         }
         public static Tensor1<τ,α> operator *
            (Tensor1<τ,α> lRow, Tensor2<τ,α> rMat) {
               TB.Assert.AreEqual(rMat.Dim1, lRow.Dim);                                      // Check that matrix and row can be multiplied.
               var resultRow = new Tensor1<τ,α>(rMat.Height, rMat.Dim1);
               foreach(var rMatKVPair in rMat)
                  if(lRow.TryGetValue(rMatKVPair.Key, out τ lRowVal)) {
                     foreach(var rRowKVPair in rMatKVPair.Value)
                        resultRow[rRowKVPair.Key] = Arith.Add(resultRow[rRowKVPair.Key], Arith.Mul(lRowVal, rRowKVPair.Value)); }
               return resultRow;
         }

         
         
   }
}
#endif