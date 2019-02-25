using System;
using System.IO;
using System.Text;
using System.Globalization;
using static System.Char;

using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;
using static Fluid.Internals.Operations;

namespace Fluid.Internals
{
    public static class IO
    {
        public static void Write<T>(this T[] array1d, TextWriter tw) {
            int loop = array1d.Length - 1;
            tw.Write('{');
            for(int i = 0; i < loop; ++i) {
                tw.Write($"{array1d[i].ToString()}, ");
            }
            tw.Write($"{array1d[loop].ToString()}}}");
        }

        public static void Write<T>(this T[][] array2d, TextWriter tw) {
            int loop = array2d.Length - 1;
            tw.Write('{');
            for(int i = 0; i < loop; ++i) {
                Write(array2d[i], tw);
                tw.Write(", ");
            }
            Write(array2d[loop], tw);
            tw.Write('}');
        }

        public static void Write<T>(this T[][][] array3d, TextWriter tw) {
            int loop = array3d.Length - 1;
            tw.Write('{');
            for(int i = 0; i < loop; ++i) {
                Write(array3d[i], tw);
                tw.Write(", ");
            }
            Write(array3d[loop], tw);
            tw.Write('}');
        }

        public static void Write<T>(this T[][][][] array4d, TextWriter tw) {
            int loop = array4d.Length - 1;
            tw.Write('{');
            for(int i = 0; i < loop; ++i) {
                Write(array4d[i], tw);
                tw.Write(", ");
            }
            Write(array4d[loop], tw);
            tw.Write('}');
        }

        public static void Write<T>(this T[][][][][] array5d, TextWriter tw) {
            int loop = array5d.Length - 1;
            tw.Write('{');
            for(int i = 0; i < loop; ++i) {
                Write(array5d[i], tw);
                tw.Write(", ");
            }
            Write(array5d[loop], tw);
            tw.Write('}');
        }

        public static void Write<T>(this T[][][][][][] array6d, TextWriter tw) {
            int loop = array6d.Length - 1;
            tw.Write('{');
            for(int i = 0; i < loop; ++i) {
                Write(array6d[i], tw);
                tw.Write(", ");
            }
            Write(array6d[loop], tw);
            tw.Write('}');
        }

        public static void WriteLine<T>(this T[] array1d, TextWriter tw) {
            Write(array1d, tw);
            tw.WriteLine();
        }

        // public static void ConsoleWrite<T>(this SparseRow<T> sparseRow)
        // where T : struct, IEquatable<T> {
        //     Console.WriteLine(sparseRow.ToString());
        // }

        // public static void ConsoleWrite<T>(this SparseMat<T> sparseMatrix)
        // where T : struct, IEquatable<T> {
        //     Console.WriteLine(sparseMatrix.ToString());
        // }

        // public static void WriteFile<T>(this List<List<T>> table, string fileName) {

        //     int rowCount = table.Count;
        //     int colCount;
        //     FileInfo file = new FileInfo(fileName);

        //     using(StreamWriter sw = new StreamWriter(file.FullName, false)) {

        //         for(int i = 0; i < rowCount; ++i) {
        //             colCount = table[i].Count;

        //             for(int j = 0; j < colCount; ++j) {
        //                     sw.WriteLine(table[i][j].ToString());
        //             }
        //         }
        //     }
        // }

        // public static void WriteFile<T>(this T[][][] table, string fileName) {

        //     int rowCount = table.Length;
        //     int colCount;
        //     FileInfo file = new FileInfo(fileName);

        //     using(StreamWriter sw = new StreamWriter(file.FullName, false)) {

        //         for(int row = 0; row < rowCount; ++row) {
        //             colCount = table[row].Length;

        //             for(int col = 0; col < colCount; ++col) {

        //                 for(int k = 0; k < 5; ++k) {
        //                     sw.WriteLine(table[row][col][k].ToString());
        //                 }
        //             }
        //         }
        //     }
        // }

        /// <summary>Reads Mathematica exported integrals needed to compute stiffness matrix which are produced by a single element of rectangular grid.</summary><param name="relativePath">Path to exported integrals.</param><remarks>[j][k][n][m] = [12 basis functions][12-j basis functions][5 term parts][5 term parts]</remarks>
        public static double[][][][] ReadRectStiffnessIntegrals(string relativePath) {
            var file = new FileInfo(relativePath);
            var results = new double[12][][][];

            for(int j = 0; j < 12; ++j) {
                results[j] = new double[12-j][][];

                for(int k = 0; k < 12-j; ++k) {
                    results[j][k] = new double[5][];

                    for(int n = 0; n < 5; ++n) {
                        results[j][k][n] = new double[5] {0,0,0,0,0};
                    }
                }
            }
            var dimIndex = new int[] {0,0,0,0};     // Indices of levels at which we reside.
            int currLevel = -1;
            char prevRead = 'č', currRead;
            double number;
            var numberSB = new StringBuilder(30);
            var numberFormatInfo = new NumberFormatInfo();
            numberFormatInfo.NumberDecimalSeparator = ".";

            using (var sr = new StreamReader(file.FullName)) {

                while(sr.Peek() > -1) {
                    currRead = (char)sr.Read();
                    
                    if(currRead == '{') {                               // If it's an opening brace.
                        ++currLevel;
                    }
                    else if(currRead == ',') {                          // Delimits dimensions.

                        if(prevRead == '.') {                      // Dumb-ass Mathematica export format where 0., is a posibility.
                            number = 0.0;
                            results[dimIndex[0]][dimIndex[1]][dimIndex[2]][dimIndex[3]] = number;
                            numberSB.Clear();
                        }
                        else if(IsDigit(prevRead)) {                         // CurrRead is a comma and prevRead was a digit. We stay at same level, but write prevRead.
                            number = Double.Parse(numberSB.ToString(), NumberStyles.Float, numberFormatInfo);
                            results[dimIndex[0]][dimIndex[1]][dimIndex[2]][dimIndex[3]] = number;     // Write number.
                            numberSB.Clear();                           // Clear string for next number.
                        }
                        ++dimIndex[currLevel];                           // Advance an index in any case (number or closing bracket).
                    }
                    else if(currRead == '}') {

                        if(IsDigit(prevRead) || prevRead == '.') {                         // We encountered a closing brace after a digit or decimals delimiter. Flush string.
                            number = Double.Parse(numberSB.ToString(), NumberStyles.Float, numberFormatInfo);
                            results[dimIndex[0]][dimIndex[1]][dimIndex[2]][dimIndex[3]] = number;
                            numberSB.Clear();
                        }
                        dimIndex[currLevel] = 0;                         // Leaving a level, reset its index.
                        --currLevel;
                    }
                    else if(IsDigit(currRead) || currRead == '.' || currRead == '-') {                    // If it's a digit or decimals separator,
                        numberSB.Append(currRead);                      // Append currRead in all cases.
                    }
                    else if(currRead == '^') {
                        numberSB.Append('E');
                    }
                    prevRead = currRead;                                // Prepare for next iteration.
                }
            }
            return results;
        }

        /// <summary>Reads Mathematica exported integrals needed to compute forcing vector which are produced by a single element of rectangular grid.</summary><param name="relativePath">Path to exported integrals.</param><remarks>[j][n] = [12 basis functions][5 term parts]</remarks>
        public static double[][] ReadRectForcingIntegrals(string relativePath) {
            var file = new FileInfo(relativePath);
            var results = new double[12][];

            for(int j = 0; j < 12; ++j) {
                results[j] = new double[5] {0,0,0,0,0};
            }
            var dimIndex = new int[] {0,0};     // Indices of levels at which we reside.
            int currLevel = -1;
            char prevRead = 'č', currRead;
            double number;
            var numberSB = new StringBuilder(30);
            var numberFormatInfo = new NumberFormatInfo();
            numberFormatInfo.NumberDecimalSeparator = ".";

            using (var sr = new StreamReader(file.FullName)) {

                while(sr.Peek() > -1) {
                    currRead = (char)sr.Read();
                    
                    if(currRead == '{') {                               // If it's an opening brace.
                        ++currLevel;
                    }
                    else if(currRead == ',') {                          // Delimits dimensions.

                        if(prevRead == '.') {                      // Dumb-ass Mathematica export format where 0., is a posibility.
                            number = 0.0;
                            results[dimIndex[0]][dimIndex[1]] = number;
                            numberSB.Clear();
                        }
                        else if(IsDigit(prevRead)) {                         // CurrRead is a comma and prevRead was a digit. We stay at same level, but write prevRead.
                            number = Double.Parse(numberSB.ToString(), NumberStyles.Float, numberFormatInfo);
                            results[dimIndex[0]][dimIndex[1]] = number;     // Write number.
                            numberSB.Clear();                           // Clear string for next number.
                        }
                        ++dimIndex[currLevel];                           // Advance an index in any case (number or closing bracket).
                    }
                    else if(currRead == '}') {

                        if(IsDigit(prevRead) || prevRead == '.') {                         // We encountered a closing brace after a digit or decimals delimiter. Flush string.
                            number = Double.Parse(numberSB.ToString(), NumberStyles.Float, numberFormatInfo);
                            results[dimIndex[0]][dimIndex[1]] = number;
                            numberSB.Clear();
                        }
                        dimIndex[currLevel] = 0;                         // Leaving a level, reset its index.
                        --currLevel;
                    }
                    else if(IsDigit(currRead) || currRead == '.' || currRead == '-') {                    // If it's a digit or decimals separator,
                        numberSB.Append(currRead);                      // Append currRead in all cases.
                    }
                    else if(currRead == '^') {
                        numberSB.Append('E');
                    }
                    prevRead = currRead;                                // Prepare for next iteration.
                }
            }
            return results;
        }

        /// <summary>Reads Mathematica exported integrals needed to compute stiffness matrix which are produced by elements of the upper right part of NorthBlock.</summary><param name="relativePath">Path to exported integrals.</param><remarks>[row][col][j][k][n][m] = [23 rows][10 cols][12 basis functions][12-j basis functions][5 term parts][5 term parts]</remarks>
        public static double[][][][][][] ReadObstructionStiffnessIntegrals(string relativePath) {

            FileInfo file = new FileInfo(relativePath);
            var results = new double[23][][][][][];

            for(int row = 0; row < 23; ++row) {                                     // Initialize array.
                results[row] = new double[10][][][][];

                for(int col = 0; col < 10; ++col) {
                    results[row][col] = new double[12][][][];

                    for(int j = 0; j < 12; ++j) {
                        results[row][col][j] = new double[12 - j][][];

                        for(int k = 0; k < 12 - j; ++k) {
                            results[row][col][j][k] = new double[5][];

                            for(int n = 0; n < 5; ++n) {
                                results[row][col][j][k][n] = new double[5];
                            }
                        }
                    }
                }
            }
            var dimIndex = new int[] {0,0,0,0,0,0};     // Indices of levels at which we reside.
            int currLevel = -1;
            char prevRead = 'č', currRead;
            double number;
            var numberSB = new StringBuilder(30);
            var numberFormatInfo = new NumberFormatInfo();
            numberFormatInfo.NumberDecimalSeparator = ".";

            using (var sr = new StreamReader(file.FullName)) {

                while(sr.Peek() > -1) {
                    currRead = (char)sr.Read();
                    
                    if(currRead == '{') {                               // If it's an opening brace.
                        ++currLevel;
                    }
                    else if(currRead == ',') {                          // Delimits dimensions.

                        if(prevRead == '.') {                      // Dumb-ass Mathematica export format where 0., is a posibility.
                            number = 0.0;
                            results[dimIndex[0]][dimIndex[1]][dimIndex[2]][dimIndex[3]][dimIndex[4]][dimIndex[5]] = number;
                            numberSB.Clear();
                        }
                        else if(IsDigit(prevRead)) {                         // CurrRead is a comma and prevRead was a digit. We stay at same level, but write prevRead.
                            number = Double.Parse(numberSB.ToString(), NumberStyles.Float, numberFormatInfo);
                            results[dimIndex[0]][dimIndex[1]][dimIndex[2]][dimIndex[3]][dimIndex[4]][dimIndex[5]] = number;     // Write number.
                            numberSB.Clear();                           // Clear string for next number.
                        }
                        ++dimIndex[currLevel];                           // Advance an index in any case (number or closing bracket).
                    }
                    else if(currRead == '}') {

                        if(IsDigit(prevRead)) {                         // We encountered a closing brace after a digit. Flush string.
                            number = Double.Parse(numberSB.ToString(), NumberStyles.Float, numberFormatInfo);
                            results[dimIndex[0]][dimIndex[1]][dimIndex[2]][dimIndex[3]][dimIndex[4]][dimIndex[5]] = number;
                            numberSB.Clear();
                        }
                        dimIndex[currLevel] = 0;                         // Leaving a level, reset its index.
                        --currLevel;
                    }
                    else if(IsDigit(currRead) || currRead == '.' || currRead == '-') {                    // If it's a digit or decimals separator,
                        numberSB.Append(currRead);                      // Append currRead in all cases.
                    }
                    else if(currRead == '^') {
                        numberSB.Append('E');
                    }
                    prevRead = currRead;                                // Prepare for next iteration.
                }
            }
            return results;
        }

        /// <summary>Reads Mathematica exported integrals needed to compute forcing vector which are produced by elements of the upper right part of NorthBlock.</summary><param name="relativePath">Path to exported integrals.</param><remarks>[row][col][j][n] = [23 rows][10 cols][12 basis functions][5 term parts]</remarks>
        public static double[][][][] ReadObstructionForcingIntegrals(string relativePath) {

            FileInfo file = new FileInfo(relativePath);
            var results = new double[23][][][];

            for(int row = 0; row < 23; ++row) {                                     // Initialize array.
                results[row] = new double[10][][];

                for(int col = 0; col < 10; ++col) {
                    results[row][col] = new double[12][];

                    for(int j = 0; j < 12; ++j) {
                        results[row][col][j] = new double[5];
                    }
                }
            }
            var dimIndex = new int[] {0,0,0,0};     // Indices of levels at which we reside.
            int currLevel = -1;
            char prevRead = 'č', currRead;
            double number;
            var numberSB = new StringBuilder(30);
            var numberFormatInfo = new NumberFormatInfo();
            numberFormatInfo.NumberDecimalSeparator = ".";

            using (var sr = new StreamReader(file.FullName)) {

                while(sr.Peek() > -1) {
                    currRead = (char)sr.Read();
                    
                    if(currRead == '{') {                               // If it's an opening brace.
                        ++currLevel;
                    }
                    else if(currRead == ',') {                          // Delimits dimensions.

                        if(prevRead == '.') {                      // Dumb-ass Mathematica export format where 0., is a posibility.
                            number = 0.0;
                            results[dimIndex[0]][dimIndex[1]][dimIndex[2]][dimIndex[3]] = number;
                            numberSB.Clear();
                        }
                        else if(IsDigit(prevRead)) {                         // CurrRead is a comma and prevRead was a digit. We stay at same level, but write prevRead.
                            number = Double.Parse(numberSB.ToString(), NumberStyles.Float, numberFormatInfo);
                            results[dimIndex[0]][dimIndex[1]][dimIndex[2]][dimIndex[3]] = number;     // Write number.
                            numberSB.Clear();                           // Clear string for next number.
                        }
                        ++dimIndex[currLevel];                           // Advance an index in any case (number or closing bracket).
                    }
                    else if(currRead == '}') {

                        if(IsDigit(prevRead)) {                         // We encountered a closing brace after a digit. Flush string.
                            number = Double.Parse(numberSB.ToString(), NumberStyles.Float, numberFormatInfo);
                            results[dimIndex[0]][dimIndex[1]][dimIndex[2]][dimIndex[3]] = number;
                            numberSB.Clear();
                        }
                        dimIndex[currLevel] = 0;                         // Leaving a level, reset its index.
                        --currLevel;
                    }
                    else if(IsDigit(currRead) || currRead == '.' || currRead == '-') {                    // If it's a digit or decimals separator,
                        numberSB.Append(currRead);                      // Append currRead in all cases.
                    }
                    else if(currRead == '^') {
                        numberSB.Append('E');
                    }
                    prevRead = currRead;                                // Prepare for next iteration.
                }
            }
            return results;
        }

        // public static void Print<T>(this T[][] array) {

        //     int rowCount = array.Length;
        //     int colCount;

        //     for(int i = 0; i < rowCount - 1; ++i) {
        //         colCount = array[i].Length;
        //         Console.Write("{");

        //         for(int j = 0; j < colCount - 1; ++j) {
        //             Console.Write($"{array[i][j].ToString()}, ");
        //         }
        //         Console.WriteLine($"{array[i][colCount - 1].ToString()}}},");
        //     }
        //     colCount = array[rowCount - 1].Length;
        //         Console.Write("{");

        //         for(int j = 0; j < colCount - 1; ++j) {
        //             Console.Write($"{array[rowCount - 1][j].ToString()}, ");
        //         }
        //         Console.WriteLine($"{array[rowCount - 1][colCount - 1].ToString()}}}");
        //         Console.Write("}");
        // }
    }
}