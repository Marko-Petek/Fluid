using System;
using static System.Math;

namespace Fluid.Internals.Numerics {
    public static class MatOps {   
      /// <summary>Invert a square matrix. Matrix M itself is transformed in its own inverse in process.</summary><param name="mat">Square matrix to invert.</param>
      public static void Invert(this double[][] mat) {
         int pivotCol = 0, pivotRow = 0;
         int n = mat.Length;
         double biggestElement = 0.0;
         double pivotInverse = 0.0;
         double dummy = 0.0;
         int[] pivotIndices = new int[n];
         int[] colIndices = new int[n];
         int[] rowIndices = new int[n];
         for(int i = 0; i < n; ++i) {
            biggestElement = 0.0;
            for(int row = 0; row < n; ++row)
               if(pivotIndices[row] != 1)
                  for(int col = 0; col < n; ++col)
                     if(pivotIndices[col] == 0)
                        if(Abs(mat[row][col]) >= biggestElement) {
                           biggestElement = Abs(mat[row][col]);
                           pivotRow = row;
                           pivotCol = col; }
            ++(pivotIndices[pivotCol]);
            if(pivotCol != pivotRow)
               for(int col = 0; col < n; ++col)
                  mat.Swap<double>(pivotRow, col, pivotCol, col);
            rowIndices[i] = pivotRow;
            colIndices[i] = pivotCol;
            if(mat[pivotCol][pivotCol] == 0.0)
               throw new ArgumentException("Singular input matrix.");
            pivotInverse = 1.0 / mat[pivotCol][pivotCol];
            mat[pivotCol][pivotCol] = 1.0;
            for(int col = 0; col < n; ++col)
               mat[pivotCol][col] *= pivotInverse;
            for(int row = 0; row < n; ++row)
               if(row != pivotCol) {
                  dummy = mat[row][pivotCol];
                  mat[row][pivotCol] = 0.0;
                  for(int col = 0; col < n; ++col)
                     mat[row][col] -= mat[pivotCol][col] * dummy; }}
         for(int i = n - 1; i > -1; --i)
            if(rowIndices[i] != colIndices[i])
               for(int row = 0; row < n; ++row)
                  mat.Swap<double>(row, rowIndices[i], row, colIndices[i]);
      }
      /// <summary>Dot two matrices.</summary><param name="mat1">Left operand.</param><param name="mat2">Right operand.</param>
      public static double[][] Dot(this double[][] mat1, double[][] mat2) {
         if(mat1.Length == mat2.Length) {
            double[][] result = new double[mat1.Length][];
            for(int i = 0; i < mat1.Length; ++i) {
               result[i] = new double[mat1.Length];
               for(int j = 0; j < mat1.Length; ++j)
                  result[i][j] = mat1[i][j] * mat2[j][i]; }
            return result; }
         throw new ArgumentException("The two arrays multiplied have to have the same dimension.");
      }
      /// <summary>Multiply a scalar with a matrix.</summary><param name="dbl">Scalar.</param><param name="mat">Matrix.</param>
      public static double[][] Mul(this double dbl, double[][] mat) {
         double[][] result = new double[mat.Length][];
         for(int i = 0; i < mat.Length; ++i) {
            result[i] = new double[mat[i].Length];
            for(int j = 0; j < mat[i].Length; ++j)
               result[i][j] = dbl * mat[i][j]; }
         return result;
      }
      /// <summary>Transpose a square matrix. Result is created on specified matrix.</summary><param name="mat">Square matrix to transpose.</param>
      public static void Transpose(this double[][] mat) {
         int length = mat.Length;
         double temp;
         for(int i = 0; i < length; ++i)
            for(int j = i + 1; j < length; ++j) {
               temp = mat[j][i];
               mat[j][i] = mat[i][j];
               mat[i][j] = temp; }
      }
      /// <summary>Add two square matrices.</summary><param name="mat1">Left matrix.</param><param name="mat2">Right matrix.</param>
      public static double[][] Add(this double[][] mat1, double[][] mat2) {
         int lgh = mat1.Length;
         double[][] result = new double[lgh][];
         for(int i = 0; i < lgh; ++i) {
            result[i] = new double[lgh];
            for(int j = 0; j < lgh; ++j)
               result[i][j] = mat1[i][j] + mat2[i][j]; }
         return result;
      }
      /// <summary>Take a matrix and add to it another matrix.</summary><param name="mat1">Matrix to be added to.</param><param name="mat2">Matrix that will be added to addee.</param>
      public static void AddTo(this double[][] mat1, double[][] mat2) {
         int lgh = mat1.Length;
         for(int i = 0; i < lgh; ++i)
            for(int j = 0; j < lgh; ++j)
               mat1[i][j] += mat2[i][j];
      }
      /// <summary>Subtract two square matrices.</summary><param name="mat1">First matrix.</param><param name="mat2">Second matrix.</param>
      public static double[][] Sub(this double[][] mat1, double[][] mat2) {
         int lgh = mat1.Length;
         double[][] result = new double[lgh][];
         for(int i = 0; i < lgh; ++i) {
            result[i] = new double[lgh];
            for(int j = 0; j < lgh; ++j)
               result[i][j] = mat1[i][j] - mat2[i][j]; }
         return result;
      }
      /// <summary>Coumpute determinant of 2x2 matrix.</summary><param name="mat">2x2 matrix.</param>
      public static double Det(this double[][] mat) =>
         mat[0][0]*mat[1][1] - mat[0][1]*mat[1][0];
      /// <summary>Coumpute trace of 2x2 matrix.</summary><param name="mat">2x2 matrix.</param>
      public static double Tr(this double[][] mat) =>
         mat[0][0] + mat[0][1] + mat[1][0] + mat[1][1];
      /// <summary>Extends the capacity of an array if it does not satisfy the specified capacity.</summary>
      public static void EnsureCapacity<T>(this T[] mat, int capacity) {
         if (mat.Length < capacity) {
            var biggerArray = new T[2 * mat.Length];
            Array.Copy(mat, biggerArray, mat.Length);
            mat = biggerArray; }
      }
      /// <summary>Swaps two elements in a 2D matrix.</summary><param name="mat">Matrix to operate on.</param><param name="row1">Row index of first element.</param><param name="col1">Column index of first element.</param><param name="row2">Row index of second element.</param><param name="col2">Column index of second element.</param>
      public static void Swap<T>(this T[][] mat, int row1, int col1, int row2, int col2) {
         T temp = mat[row2][col2];
         mat[row2][col2] = mat[row1][col1];//TODO: Write SwapRows and SwapCols.
         mat[row1][col1] = temp;
      }
      public static T[][] CreateFromArray<T>(T[] row, int allRows, int startRow, int nRows,
         int startCol, int nCols) {
            int allCols = row.Length/allRows;
            T[][] mat = new T[nRows][];
            for(int i = 0; i < nRows; ++i) {
               mat[i] = new T[nCols];
               for(int j = 0; j < nCols; ++j)
                  mat[i][j] = row[allCols*(startRow + i) + startCol + j]; }
            return mat;
      }
      public static bool Equals(this double[] mat1, double[] mat2, double epsilon) {
         if(mat1.Length == mat2.Length) {
            for(int i = 0; i < mat1.Length; ++i)
               if(Abs(mat1[i] - mat2[i]) > epsilon)
                  return false;
            return true; }
         return false;
      }
      public static bool Equals(this double[][] mat1, double[][] mat2, double epsilon) {
         if(mat1.Length == mat2.Length) {
            for(int i = 0; i < mat1.Length; ++i)
               if(!Equals(mat1[i], mat2[i], epsilon))
                  return false;
            return true; }
         return false;
      }
      public static bool Equals(this double[][][] mat1, double[][][] mat2, double epsilon) {
         if(mat1.Length == mat2.Length) {
            for(int i = 0; i < mat1.Length; ++i)
               if(!Equals(mat1[i], mat2[i], epsilon))
                  return false;
            return true; }
         return false;
      }
   }
}