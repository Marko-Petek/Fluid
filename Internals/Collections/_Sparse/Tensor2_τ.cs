using System;
using System.Linq;
using SCG = System.Collections.Generic;

using Fluid.Internals.Numerics;
using TB = Fluid.Internals.Toolbox;
// TODO: Write tests for constructors.
namespace Fluid.Internals.Collections {
   /// <summary>Rank 2 tensor holding rank 1 tensors sparsly. Does not posess arithmetic operations.</summary>
   /// <typeparam name="τ">Type of values inside rank 1 tensor.</typeparam>
   public class Tensor2<τ> : SCG.Dictionary<int,Tensor1<τ>> where τ : new() {
      /// <summary>Size of rank 2 slot (holding rank 1 tensors).</summary>
      public int Dim2 { get; protected set; }
      /// <summary>Size of rank 1 slot (holding values of type τ).</summary>
      public int Dim1 { get; protected set; }
      /// <summary>Dummy Tensor1 used by indexer when fetching a Tensor1 that does not actually exist from slot 2. If, after a call of slot 2 indexer, a setter of slot 1 decides to add an element, dummy Tensor1 added as a real element to slot 2.</summary>
      protected DumTensor1<τ> DumTnr1 { get; }

      /// <summary>Does not assign Dim2 or Dim1. User of this constructor must do it manually.</summary>
      public Tensor2() : base() {
         DumTnr1 = new DumTensor1<τ>(this);
      }
      /// <summary>Create a rank 2 tensor with given width, height and initial row capacity.</summary>
      /// <param name="dim2">Size of rank 2 slot (holding rank 1 tensors).</param>
      /// <param name="dim1">Size of rank 1 slot (holding values of type τ).</param>
      /// <param name="capacity">Initial memory capacity.</param>
      public Tensor2(int dim2, int dim1, int capacity = 6) : base(capacity) {
         DumTnr1 = new DumTensor1<τ>(this);
         Dim2 = dim2;
         Dim1 = dim1;
      }
      /// <summary>Factory method that creates a rank 2 tensor with given dimensions and initial capacity.</summary>
      /// <param name="dim2">Size of rank 2 slot (holding rank 1 tensors).</param>
      /// <param name="dim1">Size of rank 1 slot (holding values of type τ).</param>
      /// <param name="capacity">Initial memory capacity.</param>
      public virtual Tensor2<τ> Create(int dim2, int dim1, int capacity = 6) =>
         new Tensor2<τ>(dim2, dim1, capacity);
      /// <summary>Create a rank 2 tensor as a copy of another.</summary>
      /// <param name="src">Source tensor to copy.</param>
      public Tensor2(Tensor2<τ> src) : this(src.Dim2, src.Dim1, src.Count) {
         foreach(var matKVPair in src)
            Add(matKVPair.Key, new Tensor1<τ>(matKVPair.Value));
      }
      /// <summary>Factory method that creates a rank 2 tensor from a rank 2 array.</summary>
      /// <param name="arr">Rank 2 source array.</param>
      public static Tensor2<τ> CreateFromArray(τ[][] arr) {
         int dim2 = arr.Length;
         int dim1 = arr[0].Length;
         var tensor2 = new Tensor2<τ>(dim2, dim1, dim1*dim2);
         for(int i = 0; i < dim2; ++i)
            for(int j = 0; j < dim1; ++j)
                tensor2[i][j] = arr[i][j];
         return tensor2;
      }
      /// <summary>Factory method that creates a R2 tensor from a R1 array. You must specify hierarchical ordering in array and method arranges elements into ranks according to those instructions. You must specify dimensions of both ranks of new tensor and the index of element added first to each rank.</summary>
      /// <param name="arr">Rank 1 source array.</param>
      /// <param name="nRowsInArr">Number of elements in rank 2 slot of array (number of rows). This is needed since all slots are flattened into one in the source array.</param>
      /// <param name="srtRowInx">The row of input array at which the copying begins.</param>
      /// <param name="nRows">Number of rows of input array to copy.</param>
      /// <param name="srtColInx">The col of input array at which the copying begins in each row.</param>
      /// <param name="nCols">Number of columns of input array to copy.</param>
      /// <param name="dimR2">Dimension of rank 2 slot of new tensor.</param>
      /// <param name="dimR1">Dimension of rank 1 slot of new tensor.</param>
      /// <param name="firstR2Inx">Index to assign the first element added to slot 2.</param>
      /// <param name="firstR1Inx">Index to assign the first element added to slot 1.</param>
      public static Tensor2<τ> CreateFromArray(τ[] arr, int nRowsInArr, int srtRowInx,
         int nRows, int srtColInx, int nCols, int dimR2, int dimR1, int firstR2Inx = 0,
         int firstR1Inx = 0) {
            int allCols = arr.Length/nRowsInArr;
            var tnr2 = new Tensor2<τ>(dimR2, dimR1, nCols*nRows);
            for(int i = srtRowInx, k = firstR2Inx; i < srtRowInx + nRows; ++i, ++k)
               for(int j = srtColInx, l = firstR1Inx; j < srtColInx + nCols; ++j, ++l)
                  tnr2[k][l] = arr[i*allCols + j];
            return tnr2;
      }
      /// <summary>Factory method that creates a R2 tensor from a R1 array. You must specify hierarchical ordering in array and method arranges elements into ranks according to those instructions. Dimensions of tensor's both ranks are set the same. They equal the number of copied columns. First element added to each rank has its index set to 0.</summary>
      /// <param name="arr">Rank 1 source array.</param>
      /// <param name="nRowsInArr">Dimension of rank 2 slot of array (number of rows).</param>
      /// <param name="srtRowInx">Rank 2 index (row index) of array at which the copying begins.</param>
      /// <param name="nRows">Number of rank 2 elements (rows) of array to copy.</param>
      /// <param name="srtColInx">Rank 1 index (col index) in array at which the copying begins in each rank 2 element (row).</param>
      /// <param name="nCols">Number of rank 1 elements (cols) of array to copy. Dimensions of tensor's both ranks will equal this.</param>
      public static Tensor2<τ> CreateFromArray(τ[] arr, int nRowsInArr, int srtRowInx,
      int nRows, int srtColInx, int nCols) =>
         CreateFromArray(arr, nRowsInArr, srtRowInx, nRows, srtColInx, nCols, nCols, nRows);
      /// <summary></summary>
      /// <param name="slc">Rank 1 source slice.</param>
      /// <param name="nRowsInArr"></param>
      /// <returns></returns>
      public static Tensor2<τ> CreateFromSpan(Span<τ> slc, int nRowsInArr) {
      int nColsInArr = slc.Length / nRowsInArr;
      var sparseMat = new Tensor2<τ>(nRowsInArr, nColsInArr, nColsInArr*nRowsInArr);
      for(int i = 0; i < nRowsInArr; ++i)
         for(int j = 0; j < nColsInArr; ++j)
               sparseMat[i][j] = slc[i*nColsInArr + j];
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
         matKVPair.Value.Swap(inx1, inx2);              // Swap elms of row.
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
               DumTnr1.SparseMat = this;
               DumTnr1.Index = i;
               return (SparseRow<τ,α>)DumTnr1; } }
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
         TB.Assert.AreEqual(lMat.Dim1, rRow.Width);                                 // Check that matrix and row can be multiplied.                                        
         var resultRow = new SparseRow<τ,α>(lMat.Dim1);           // lMat.Count = # of non-zero rows.
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
         TB.Assert.AreEqual(rMat.Dim1, lRow.Width);                                      // Check that matrix and row can be multiplied.
         var resultRow = new SparseRow<τ,α>(rMat.Height, rMat.Dim1);
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