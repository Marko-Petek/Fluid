﻿using System;
using System.Diagnostics.CodeAnalysis;
using static System.Math;
using Fluid.Internals.Networks;
using dbl = System.Double;
using Fluid.Internals.Algebras;

namespace Fluid.Internals.Numerics {
   public static class MatOps {
      /// <summary>Invert a square matrix. Matrix M itself is transformed in its own inverse in process.</summary>
      /// <param name="mat">Square matrix to invert.</param>
      /// <remarks><see cref="TestRefs.MatrixInvert"/></remarks>
      public static void Invert<τ,α>(this τ[][] mat)        // τ can be null. We are dealing with arrays.
      where α : IAlgebra<τ>, new()
      where τ : IEquatable<τ>, IComparable<τ> {
         α alg = new α();
         int pivotCol = 0, pivotRow = 0;
         int n = mat.Length;
         τ biggestElement, pivotInverse, dummy;
         int[] pivotIndices = new int[n];
         int[] colIndices = new int[n];
         int[] rowIndices = new int[n];
         for(int i = 0; i < n; ++i) {
            biggestElement = alg.Zero;
            for(int row = 0; row < n; ++row)
               if(pivotIndices[row] != 1)
                  for(int col = 0; col < n; ++col)
                     if(pivotIndices[col] == 0)
                        if(alg.Abs(mat[row][col]).CompareTo(biggestElement) >= 0) {
                           biggestElement = alg.Abs(mat[row][col]);
                           pivotRow = row;
                           pivotCol = col; }
            ++(pivotIndices[pivotCol]);
            if(pivotCol != pivotRow)
               for(int col = 0; col < n; ++col)
                  Swap(mat, pivotRow, col, pivotCol, col);
            rowIndices[i] = pivotRow;
            colIndices[i] = pivotCol;
            if(mat[pivotCol][pivotCol].Equals(default))
               throw new ArgumentException("Singular input matrix.");
            pivotInverse = alg.Div(alg.Unit, mat[pivotCol][pivotCol]);
            mat[pivotCol][pivotCol] = alg.Unit;
            for(int col = 0; col < n; ++col) {
               mat[pivotCol][col] = alg.Mul(mat[pivotCol][col], pivotInverse)!; }         // Result could be null, but such behavior is ok here. Ref types have null as zero representation. I'm using the bang for now.
            for(int row = 0; row < n; ++row)
               if(row != pivotCol) {
                  dummy = mat[row][pivotCol];
                  mat[row][pivotCol] = alg.Zero!;                             // Result could be null, but such behavior is ok here. Ref types have null as zero representation. I'm using the bang for now.
                  for(int col = 0; col < n; ++col)
                     mat[row][col] = alg.Sub(mat[row][col], alg.Mul(mat[pivotCol][col], dummy))!; }}     // Result could be null, but such behavior is ok here. Ref types have null as zero representation. I'm using the bang for now.
         for(int i = n - 1; i > -1; --i)
            if(rowIndices[i] != colIndices[i])
               for(int row = 0; row < n; ++row)
                  Swap(mat, row, rowIndices[i], row, colIndices[i]);
      }

      /// <summary>Dot two matrices.</summary><param name="mat1">Left operand.</param><param name="mat2">Right operand.</param>
      public static τ[][] Dot<τ,α>(this τ[][] mat1, τ[][] mat2)
      where α : IAlgebra<τ>, new()
      where τ : IEquatable<τ>, IComparable<τ> {
         α alg = new α();
         if(mat1.Length == mat2.Length) {
            τ[][] result = new τ[mat1.Length][];
            for(int i = 0; i < mat1.Length; ++i) {
               result[i] = new τ[mat1.Length];
               for(int j = 0; j < mat1.Length; ++j)
                  result[i][j] = alg.Mul(mat1[i][j], mat2[j][i])!; }    // Result could be null, but such behavior is ok here. Ref types have null as zero representation. I'm using the bang for now.
            return result; }
         throw new ArgumentException("The two arrays multiplied have to have the same dimension.");
      }

      /// <summary>Multiply a scalar with a matrix.</summary><param name="dbl">Scalar.</param><param name="mat">Matrix.</param>
      public static τ[][] Mul<τ,α>(this τ dbl, τ[][] mat)
      where α : IAlgebra<τ>, new()
      where τ : IEquatable<τ>, IComparable<τ> {
         α alg = new α();
         τ[][] result = new τ[mat.Length][];
         for(int i = 0; i < mat.Length; ++i) {
            result[i] = new τ[mat[i].Length];
            for(int j = 0; j < mat[i].Length; ++j)
               result[i][j] = alg.Mul(dbl, mat[i][j])!; }            // Result could be null, but such behavior is ok here. Ref types have null as zero representation. I'm using the bang for now.
         return result;
      }

      /// <summary>Transpose a square matrix. Result is created on specified matrix.</summary><param name="mat">Square matrix to transpose.</param>
      public static void Transpose<τ>(this τ[][] mat) {
         int length = mat.Length;
         τ temp;
         for(int i = 0; i < length; ++i)
            for(int j = i + 1; j < length; ++j) {
               temp = mat[j][i];
               mat[j][i] = mat[i][j];
               mat[i][j] = temp; }
      }

      /// <summary>Add two square matrices.</summary><param name="mat1">Left matrix.</param><param name="mat2">Right matrix.</param>
      public static τ[][] Sum<τ,α>(this τ[][] mat1, τ[][] mat2)
      where α : IAlgebra<τ>, new()
      where τ : IEquatable<τ>, IComparable<τ> {
         α alg = new α();
         int lgh = mat1.Length;
         τ[][] result = new τ[lgh][];
         for(int i = 0; i < lgh; ++i) {
            result[i] = new τ[lgh];
            for(int j = 0; j < lgh; ++j)
               result[i][j] = alg.Sum(mat1[i][j], mat2[i][j])!; }        // Result could be null, but such behavior is ok here. Ref types have null as zero representation. I'm using the bang for now.
         return result;
      }

      public static dbl[][] Sum(this dbl[][] mat1, dbl[][] mat2) =>
         Sum<dbl,DblA>(mat1, mat2);
      /// <summary>Take a matrix and add to it another matrix.</summary><param name="mat1">Matrix to be added to.</param><param name="mat2">Matrix that will be added to addee.</param>
      public static void SumInto<τ,α>(this τ[][] mat1, τ[][] mat2)
      where α : IAlgebra<τ>, new()
      where τ : IEquatable<τ>, IComparable<τ> {
         α alg = new α();
         int lgh = mat1.Length;
         for(int i = 0; i < lgh; ++i)
            for(int j = 0; j < lgh; ++j)
               mat1[i][j] = alg.Sum(mat1[i][j], mat2[i][j])!;           // Result could be null, but such behavior is ok here. Ref types have null as zero representation. I'm using the bang for now.
      }

      /// <summary>Subtract two square matrices.</summary><param name="mat1">First matrix.</param><param name="mat2">Second matrix.</param>
      public static τ[][] Sub<τ,α>(this τ[][] mat1, τ[][] mat2)
      where α : IAlgebra<τ>, new()
      where τ : IEquatable<τ>, IComparable<τ> {
         α alg = new α();
         int lgh = mat1.Length;
         τ[][] result = new τ[lgh][];
         for(int i = 0; i < lgh; ++i) {
            result[i] = new τ[lgh];
            for(int j = 0; j < lgh; ++j)
               result[i][j] = alg.Sub(mat1[i][j], mat2[i][j])!; }       // Result could be null, but such behavior is ok here. Ref types have null as zero representation. I'm using the bang for now.
         return result;
      }

      /// <summary>Subtract two square matrices.</summary><param name="mat1">First matrix.</param><param name="mat2">Second matrix.</param>
      public static dbl[][] Sub(this dbl[][] mat1, dbl[][] mat2) =>
         Sub<dbl,DblA>(mat1, mat2);

      /// <summary>Coumpute determinant of 2x2 matrix.</summary><param name="mat">2x2 matrix.</param>
      [return: MaybeNull]
      public static τ Det<τ,α>(this τ[][] mat)
      where α : IAlgebra<τ>, new()
      where τ : IEquatable<τ>, IComparable<τ> {
         α alg = new α();
         return alg.Sub(alg.Mul(mat[0][0], mat[1][1]), alg.Mul(mat[0][1], mat[1][0]));
      }

      public static dbl Det(dbl[][] mat) =>
         Det<dbl,DblA>(mat);

      /// <summary>Coumpute trace of 2x2 matrix.</summary><param name="mat">2x2 matrix.</param>
      [return: MaybeNull]
      public static τ Tr<τ,α>(this τ[][] mat)
      where α : IAlgebra<τ>, new()
      where τ : IEquatable<τ>, IComparable<τ> {
         α alg = new α();
         return alg.Sum(mat[0][0], mat[1][1]);
      }

      /// <summary>Coumpute trace of 2x2 matrix.</summary><param name="mat">2x2 matrix.</param>
      public static double Tr(dbl[][] mat) =>
         Tr<dbl,DblA>(mat);

      /// <summary>Extends the capacity of an array if it does not satisfy the specified capacity.</summary>
      public static τ[] EnsureCapacity<τ>(this τ[] mat, int capacity) {
         if(mat.Length < capacity) {
            var biggerArray = new τ[2 * mat.Length];
            Array.Copy(mat, biggerArray, mat.Length);
            return biggerArray; }
         else return mat;
      }

      /// <summary>Swaps two elements in a 2D matrix.</summary><param name="mat">Matrix to operate on.</param><param name="row1">Row index of first element.</param><param name="col1">Column index of first element.</param><param name="row2">Row index of second element.</param><param name="col2">Column index of second element.</param>
      public static void Swap<τ>(this τ[][] mat, int row1, int col1, int row2, int col2) {
         τ temp = mat[row2][col2];
         mat[row2][col2] = mat[row1][col1];
         mat[row1][col1] = temp;
      }

      public static void SwapRows<τ>(this τ[][] mat, int row1, int row2) {
         τ[] temp = mat[row1];
         mat[row1] = mat[row2];
         mat[row2] = temp;
      }

      public static void SwapCols<τ>(this τ[][] mat, int col1, int col2) {
         for(int i = 0; i < mat.Length; ++i)
            Swap(mat, i, col1, i, col2);
      }

      public static τ[][] CreateFromArray<τ>(τ[] row, int allRows, int startRow, int nRows,
         int startCol, int nCols) {
            int allCols = row.Length/allRows;
            τ[][] mat = new τ[nRows][];
            for(int i = 0; i < nRows; ++i) {
               mat[i] = new τ[nCols];
               for(int j = 0; j < nCols; ++j)
                  mat[i][j] = row[allCols*(startRow + i) + startCol + j]; }
            return mat;
      }

      public static bool Equals<τ,α>(this τ[] mat1, τ[] mat2, [AllowNull] τ epsilon)
      where α : IAlgebra<τ>, new()
      where τ : IEquatable<τ>, IComparable<τ> {
         α alg = new α();
         if(mat1.Length == mat2.Length) {
            for(int i = 0; i < mat1.Length; ++i) {
               var dev = alg.Abs(alg.Sub(mat1[i], mat2[i]));
               if(!alg.IsZero(dev) && dev.CompareTo(epsilon) > 0)
                  return false; }
            return true; }
         return false;
      }

      public static bool Equals<τ,α>(this τ[] mat1, τ[] mat2)
      where α : IAlgebra<τ>, new()
      where τ : IEquatable<τ>, IComparable<τ> {
         α alg = new α();
         return Equals<τ,α>(mat1, mat2, alg.Zero); }
      public static bool Equals<τ,α>(this τ[][] mat1, τ[][] mat2, [AllowNull] τ eps)
      where α : IAlgebra<τ>, new()
      where τ : IEquatable<τ>, IComparable<τ> {
         if(mat1.Length == mat2.Length) {
            for(int i = 0; i < mat1.Length; ++i)
               if(!Equals<τ,α>(mat1[i], mat2[i], eps))
                  return false;
            return true; }
         return false;
      }

      public static bool Equals<τ,α>(this τ[][] mat1, τ[][] mat2)
      where α : IAlgebra<τ>, new()
      where τ : IEquatable<τ>, IComparable<τ> {
         α alg = new α();
         return Equals<τ,α>(mat1, mat2, alg.Zero);
      }

      public static bool Equals<τ,α>(this τ[][][] mat1, τ[][][] mat2, [AllowNull] τ eps)
      where α : IAlgebra<τ>, new()
      where τ : IEquatable<τ>, IComparable<τ> {
         if(mat1.Length == mat2.Length) {
            for(int i = 0; i < mat1.Length; ++i)
               if(!Equals<τ,α>(mat1[i], mat2[i], eps))
                  return false;
            return true; }
         return false;
      }
      public static bool Equals<τ,α>(this τ[][][] mat1, τ[][][] mat2)
      where α : IAlgebra<τ>, new()
      where τ : IEquatable<τ>, IComparable<τ> {
         α alg = new α();
         return Equals<τ,α>(mat1, mat2, alg.Zero); }
   }
}