using Xunit;
using Fluid.Internals;
using Fluid.Internals.Numerics;

namespace Fluid.Tests {
   public class MatsTests {
      [Fact] public void InvertMatrix1() {
         double[][] matrix = new double[3][] {
            new double[3] {6, 0, 0},
            new double[3] {0, 2, 0},
            new double[3] {0, 0, 8} };
         matrix.Invert();
         double epsilon = 0.000001;
         var expectedResult = new double[3][] {
            new double[3] { 1.0/ 6, 0.0, 0.0   },
            new double[3] {    0.0, 0.5, 0.0   },
            new double[3] {    0.0, 0.0, 0.125 } };
         Assert.True(MatOps.Equals(matrix, expectedResult, epsilon));
      }
      [Fact] public void InvertMatrix2() {
         double[][] matrix2 = new double[3][] {
            new double[3] {1, 2, 3},
            new double[3] {4, 2, 2},
            new double[3] {5, 1, 7} };
         matrix2.Invert();
         double epsilon = 0.000001;
         var expectedResult2 = new double[3][] {
            new double[3] {-2.0/7, 11.0/42, 1.0/21},
            new double[3] { 3.0/7, 4.0/21, -5.0/21},
            new double[3] { 1.0/7, -3.0/14, 1.0/7} };
         Assert.True(MatOps.Equals(matrix2, expectedResult2, epsilon));
      }
   }
}