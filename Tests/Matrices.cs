using Xunit;
using System;
using System.Threading;
using Fluid.Internals.Algebras;
using Fluid.Internals.Networks;
using Fluid.Internals.Numerics;
using static Fluid.Internals.Tools;
namespace Fluid.Tests {
using dbl = Double;

public class Matrices {

   [Fact] public void MatrixInvert1() {
      double[][] matrix = new double[3][] {
         new dbl[3] {6, 0, 0},
         new dbl[3] {0, 2, 0},
         new dbl[3] {0, 0, 8} };
      matrix.Invert<dbl,DblA>();
      double epsilon = 0.000001;
      var expectedResult = new dbl[3][] {
         new dbl[3] { 1.0/ 6, 0.0, 0.0   },
         new dbl[3] {    0.0, 0.5, 0.0   },
         new dbl[3] {    0.0, 0.0, 0.125 } };
      Assert.True(matrix.Equals<dbl,DblA>(expectedResult, epsilon));
   }

   [Fact] public void MatrixInvert2() {
      double[][] matrix2 = new double[3][] {
         new dbl[3] {1, 2, 3},
         new dbl[3] {4, 2, 2},
         new dbl[3] {5, 1, 7} };
      matrix2.Invert<dbl,DblA>();
      double epsilon = 0.000001;
      var expectedResult2 = new dbl[3][] {
         new dbl[3] {-2.0/7, 11.0/42, 1.0/21},
         new dbl[3] { 3.0/7, 4.0/21, -5.0/21},
         new dbl[3] { 1.0/7, -3.0/14, 1.0/7} };
      Assert.True(matrix2.Equals<dbl,DblA>(expectedResult2, epsilon));
   }
}
}