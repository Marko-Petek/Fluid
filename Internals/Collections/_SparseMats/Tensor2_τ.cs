using System;
using System.Linq;
using SCG = System.Collections.Generic;

using Fluid.Internals.Numerics;
using TB = Fluid.Internals.Toolbox;
// TODO: Write tests for constructors.
namespace Fluid.Internals.Collections {
   /// <summary>Rank 2 tensor holding rank 1 tensors sparsly. It does not possess arithmetic operations.</summary><typeparam name="τ">Type of values inside rank 1 tensor.</typeparam>
   public class Tensor2<τ> : SCG.Dictionary<int,Tensor1<τ>> where τ : new() {
         /// <summary>Size of rank 2 slot (holding rank 1 tensors).</summary>
         public int Dim2 { get; protected set; }
         /// <summary>Size of rank 1 slot (holding values of type τ).</summary>
         public int Dim1 { get; protected set; }
         /// <summary>Dummy Tensor1 used by indexer when fetching a Tensor1 that does not actually exist from slot 2. If, after a call of slot 2 indexer, a setter of slot 1 decides to add an element, dummy Tensor1 added as a real element to slot 2.</summary>
         internal DumTensor1<τ> DumTensor1 { get; }

         /// <summary>Does not assign Dim2 or Dim1. User of this constructor must do it manually.</summary>
         protected Tensor2() : base() {
            DumTensor1 = new DumTensor1<τ>(this, Dim2);
         }
         /// <summary>Create a rank 2 tensor with given width, height and initial row capacity.</summary><param name="dim2">Width (length of rows) that matrix would have in its explicit form.</param><param name="dim1">Height (length of columns) that matrix would have in its explicit form.</param><param name="capacity">Initial row capacity.</param>
         public Tensor2(int dim1, int dim2, int capacity = 6) : base(capacity) {
            DumTensor1 = new DumTensor1<τ>(this, Dim2);
            Dim2 = dim2;
            Dim1 = dim1;
         }
         /// <summary>Factory method.</summary>
         /// <param name="width"></param>
         /// <param name="height"></param>
         /// <param name="capacity"></param>
         /// <typeparam name="τ"></typeparam>
         /// <returns></returns>
         public virtual Tensor2<τ> Create(int width, int height, int capacity = 6) =>
            new Tensor2<τ>(width, height, capacity);
         /// <summary>Create a copy of specified SparseMatrix.</summary><param name="source">Source SparseMatrix to copy.</param>
         public Tensor2(Tensor2<τ> source) : this(source.Dim2, source.Dim1, source.Count) {
            foreach(var matKVPair in source)
               Add(matKVPair.Key, new Tensor1<τ>(matKVPair.Value));
         }

         public static Tensor2<τ> CreateFromArray(τ[][] arr) {
            int nRows = arr.Length;
            int nCols = arr[0].Length;
            var tensor2 = new Tensor2<τ>(nCols, nRows, nCols*nRows);
            for(int i = 0; i < nRows; ++i)
               for(int j = 0; j < nCols; ++j)
                  tensor2[i][j] = arr[i][j];
            return tensor2;
         }
         public static Tensor2<τ> CreateFromArray(τ[] arr, int allRows, int startRow,
            int nRows, int startCol, int nCols, int width, int height, int startRowInx = 0, int startColInx = 0) {
               int allCols = arr.Length/allRows;
               var sparseMat = new Tensor2<τ>(width, height, nCols*nRows);
               for(int i = startRow, k = startRowInx; i < startRow + nRows; ++i, ++k)
                  for(int j = startCol, l = startColInx; j < startCol + nCols; ++j, ++l)
                     sparseMat[k][l] = arr[i*allCols + j];
               return sparseMat;
         }

         public static Tensor2<τ> CreateFromArray(τ[] arr, int allRows, int startRow,
            int nRows, int startCol, int nCols) =>
               CreateFromArray(arr, allRows, startRow, nRows, startCol, nCols, nCols, nRows);

         public static Tensor2<τ> CreateFromSpan(Span<τ> slice, int nRows) {
            int nCols = slice.Length / nRows;
            var sparseMat = new Tensor2<τ>(nCols, nRows, nCols*nRows);
            for(int i = 0; i < nRows; ++i)
               for(int j = 0; j < nCols; ++j)
                  sparseMat[i][j] = slice[i*nCols + j];
            return sparseMat;
         }
         /// <summary>Split matrix on left and right part. Return right part. Element at specified index will be part of right part.</summary><param name="colInx">Index of element at which to split. This element will be part of right matrix.</param>
         public Tensor2<τ> SplitAtCol(int colInx) {
            int remWidth = Dim2 - colInx;
            var remMat = new Tensor2<τ,α>(remWidth, Dim1);               // We nevertheless need a factory method here.
            Dim2 = colInx;                                                 // Adjust width of this Matrix.
            foreach(var matKVPair in this) {                                  // Split each SparseRow separately.
               var remRow = matKVPair.Value.SplitAt(colInx);
               remMat.Add(matKVPair.Key, remRow); }
            return remMat;
         }
         /// <summary>Split matrix on upper and lower part. Return lower part. Element at specified index will be part of lower part.</summary><param name="col">Index of element at which to split. This element will be part of lower matrix.</param>
         public Tensor2<τ,α> SplitAtRow(int inx) {
            var remMat = new Tensor2<τ,α>(Dim2, Dim1 - inx); 
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
                  DumTensor1.SparseMat = this;
                  DumTensor1.Index = i;
                  return (SparseRow<τ,α>)DumTensor1; } }
            set {
               if(value.Count != 0)
                  base[i] = value;
               else
                  Remove(i); }
         }
         /// <summary>Creates a SparseMat that is a sum of two operand SparseMats.</summary><param name="lMat">Left operand.</param><param name="rMat">Right operand.</param>
         public static Tensor2<τ,α> operator +
            (Tensor2<τ,α> lMat, Tensor2<τ,α> rMat) {
               TB.Assert.AreEqual(lMat.Width, rMat.Width);                                         // Check that width and height of operands match.
               TB.Assert.AreEqual(lMat.Height, rMat.Height);
               var res = new Tensor2<τ,α>(rMat);
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
            (Tensor2<τ,α> lMat, SparseRow<τ,α> rRow) {
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
            (SparseRow<τ,α> lRow, Tensor2<τ,α> rMat) {
               TB.Assert.AreEqual(rMat.Width, lRow.Width);                                      // Check that matrix and row can be multiplied.
               var resultRow = new SparseRow<τ,α>(rMat.Height, rMat.Width);
               foreach(var rMatKVPair in rMat)
                  if(lRow.TryGetValue(rMatKVPair.Key, out τ lRowVal)) {
                     foreach(var rRowKVPair in rMatKVPair.Value)
                        resultRow[rRowKVPair.Key] = Arith.Add(resultRow[rRowKVPair.Key], Arith.Mul(lRowVal, rRowKVPair.Value)); }
               return resultRow;
         }

         
         /// <summary>Compare two SparseMats.</summary><param name="other">The other SparseMat to compare with.</param>
         public bool Equals(Tensor2<τ,α> other) {
            foreach(var matKVPair in this)
               if(!(other.TryGetValue(matKVPair.Key, out SparseRow<τ,α> val) && matKVPair.Value.Equals(val)))        // Fetch did not suceed or values are not equal.
                  return false;
            return true;
         }

         public bool Equals(Tensor2<τ,α> other, τ eps) {
            foreach(var matKVPair in this) {
               if(!(other.TryGetValue(matKVPair.Key, out SparseRow<τ,α> otherRow)))  // Fetch did not suceed.
                  return false;
               if(!matKVPair.Value.Equals(otherRow, eps))                             // Fetch suceeded and values do not agree within tolerance.
                  return false; }
            return true;                                                              // All values agree within tolerance.
         }
   }
}