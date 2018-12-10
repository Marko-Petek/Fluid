using System;
using static System.Math;

using static Fluid.Internals.ArrayOperations;

namespace Fluid.Internals.Numerics
{
    public static class MatrixOperations
    {   
        /// <summary>Invert a square matrix. Matrix M itself is transformed in its own inverse in process.</summary><param name="M">Square matrix to invert.</param>
        public static void Invert(this double[][] M) {

            int pivotCol = 0, pivotRow = 0;
            int n = M.Length;
            double biggestElement = 0.0;
            double pivotInverse = 0.0;
            double dummy = 0.0;
            int[] pivotIndices = new int[n];
            int[] colIndices = new int[n];
            int[] rowIndices = new int[n];

            for(int i = 0; i < n; ++i) {
                biggestElement = 0.0;

                for(int row = 0; row < n; ++row) {

                    if(pivotIndices[row] != 1) {

                        for(int col = 0; col < n; ++col) {

                            if(pivotIndices[col] == 0) {

                                if(Abs(M[row][col]) >= biggestElement) {
                                    biggestElement = Abs(M[row][col]);
                                    pivotRow = row;
                                    pivotCol = col;
                                }
                            }
                        }
                    }
                }
                ++(pivotIndices[pivotCol]);

                if(pivotCol != pivotRow) {

                    for(int col = 0; col < n; ++col) {
                        M.Swap<double>(pivotRow, col,
                                        pivotCol, col);
                    }
                }
                rowIndices[i] = pivotRow;
                colIndices[i] = pivotCol;

                if(M[pivotCol][pivotCol] == 0.0) {
                    throw new ArgumentException("Singular input matrix.");
                }
                pivotInverse = 1.0 / M[pivotCol][pivotCol];
                M[pivotCol][pivotCol] = 1.0;

                for(int col = 0; col < n; ++col) {
                    M[pivotCol][col] *= pivotInverse;
                }
                
                for(int row = 0; row < n; ++row) {

                    if(row != pivotCol) {
                        dummy = M[row][pivotCol];
                        M[row][pivotCol] = 0.0;

                        for(int col = 0; col < n; ++col) {
                            M[row][col] -= M[pivotCol][col] * dummy;
                        }
                    }
                }
            }

            for(int i = n - 1; i > -1; --i) {

                if(rowIndices[i] != colIndices[i]) {

                    for(int row = 0; row < n; ++row) {
                        M.Swap<double>(row, rowIndices[i],
                                        row, colIndices[i]);
                    }
                }
            }
        }

        /// <summary>Dot two matrices.</summary><param name="left">Left operand.</param><param name="right">Right operand.</param>
        public static double[][] Dot(double[][] left, double[][] right) {
            
            if(left.Length == right.Length) {
                double[][] result = new double[left.Length][];

                for(int i = 0; i < left.Length; ++i) {
                    result[i] = new double[left.Length];

                    for(int j = 0; j < left.Length; ++j) {
                            result[i][j] = left[i][j] * right[j][i];
                    }
                }
                return result;
            }
            throw new ArgumentException("The two arrays multiplied have to have the same dimension.");
        }

        /// <summary>Multiply a scalar with a matrix.</summary><param name="left">Scalar.</param><param name="right">Matrix.</param>
        public static double[][] Times(double left, double[][] right) {
            double[][] result = new double[right.Length][];

            for(int i = 0; i < right.Length; ++i) {
                result[i] = new double[right[i].Length];

                for(int j = 0; j < right[i].Length; ++j) {
                    result[i][j] = left * right[i][j];
                }
            }
            return result;
        }

        /// <summary>Transpose a square matrix. Result is created on specified matrix.</summary><param name="matrix">Square matrix to transpose.</param>
        public static void Transpose(ref double[][] matrix) {
            int length = matrix.Length;
            double temp;

            for(int i = 0; i < length; ++i) {

                for(int j = i + 1; j < length; ++j) {

                    temp = matrix[j][i];
                    matrix[j][i] = matrix[i][j];
                    matrix[i][j] = temp;
                }
            }
        }

        /// <summary>Add two square matrices.</summary><param name="matrix1">Left matrix.</param><param name="matrix2">Right matrix.</param>
        public static double[][] Add(double[][] matrix1, double[][] matrix2) {
            int length = matrix1.Length;
            double[][] result = new double[length][];

            for(int i = 0; i < length; ++i) {
                result[i] = new double[length];
                
                for(int j = 0; j < length; ++j) {

                    result[i][j] = matrix1[i][j] + matrix2[i][j];
                }
            }
            return result;
        }

        /// <summary>Take a matrix by ref and add to it another matrix.</summary><param name="thisMatrix">Matrix to be added to.</param><param name="sumand">Matrix that will be added to addee.</param>
        public static void AddTo(ref double[][] thisMatrix, double[][] sumand) {
            int length = thisMatrix.Length;

            for(int i = 0; i < length; ++i) {
                
                for(int j = 0; j < length; ++j) {

                    thisMatrix[i][j] += sumand[i][j];
                }
            }
        }

        /// <summary>Subtract two square matrices.</summary><param name="matrix1">First matrix.</param><param name="matrix2">Second matrix.</param>
        public static double[][] Subtract(double[][] matrix1, double[][] matrix2) {
            int length = matrix1.Length;
            double[][] result = new double[length][];

            for(int i = 0; i < length; ++i) {
                result[i] = new double[length];
                
                for(int j = 0; j < length; ++j) {

                    result[i][j] = matrix1[i][j] - matrix2[i][j];
                }
            }
            return result;
        }

        /// <summary>Coumpute determinant of 2x2 matrix.</summary><param name="M">2x2 matrix.</param>
        public static double Det(this double[][] M) {
            return M[0][0]*M[1][1] - M[0][1]*M[1][0];
        }

        /// <summary>Coumpute trace of 2x2 matrix.</summary><param name="M">2x2 matrix.</param>
        public static double Tr(this double[][] M) {
            return M[0][0] + M[0][1] + M[1][0] + M[1][1];
        }
    }
}