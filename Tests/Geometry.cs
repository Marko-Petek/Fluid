using System;
using System.Threading;
using static System.Console;
using Xunit;
using Xunit.Abstractions;

using Fluid.Internals;
using Fluid.Internals.Networks;
using Fluid.Internals.Numerics;
using Fluid.Internals.Lsfem;
//using Fluid.Tests.Mocks;
using static Fluid.Internals.Tools;

namespace Fluid.Tests {
   public class Geometry {

      [InlineData(-0.5, 0.5)]
      [InlineData(0.5, 0.5)]
      [InlineData(0.0, 0.5)]
      [InlineData(0.0, -0.5)]
      [Theory] public void PointInsideQuadrilateral(double x, double y) {
         var point = new Vec2(x,y);
         var vertices = new Vec2[4] {                 // Define vertices of  quadrilateral.
            new Vec2(-1.0, -1.0),
            new Vec2(1.0, -1.0),
            new Vec2(2.0, 1.0),
            new Vec2(-2.0, 1.0) };                             
         Assert.True(point.IsInsidePolygon(vertices));
      }

      [InlineData(-2.0, 0.0)] [InlineData(2.0, 0.0)] [InlineData(0.0, 1.5)] [InlineData(0.0, -1.5)]
      [Theory] public void PointOutsideQuadrilateral(double x, double y) {
         var point = new Vec2(x,y);
         var vertices = new Vec2[4] {                 // Define vertices of  quadrilateral.
            new Vec2(-1.0, -1.0),
            new Vec2(1.0, -1.0),
            new Vec2(2.0, 1.0),
            new Vec2(-2.0, 1.0) };     
         Assert.True(!point.IsInsidePolygon(vertices));
      }

      [InlineData(1, 0)] [InlineData(1.1, 0.2)] [InlineData(3.9, 0.99)] [InlineData(1.01, 0.99)]
      [InlineData(2.5, 0.5)] [InlineData(3.1, 0.66)] [InlineData(1.00000001, 0.9999999)] [InlineData(2, 0.1)]
      [Theory] public void TestPointInsidePolygon1(double x, double y) {
         var point = new Vec2(x,y);
         var vertices = new Vec2[4] {
            new Vec2(1,0),
            new Vec2(4,0),
            new Vec2(4,1),
            new Vec2(1,1) };
         Assert.True(point.IsInsidePolygon(vertices));
      }
      // TODO: Test IsInsidePolygon method more.
      [InlineData(0.35, 0.0)]
      [Theory] public void TestPointOutsidePolygon1(double x, double y) {
         var point = new Vec2(x,y);
         var vertices = new Vec2[4] {
            new Vec2(1,0),
            new Vec2(4,0),
            new Vec2(4,1),
            new Vec2(1,1) };
         Assert.False(point.IsInsidePolygon(vertices));
      }

      

      // [Fact] public void StructBehavior() {              // Behaves well.
      //    Variable var = new Variable(1, true);
      //    var.Val = 2;
      //    T.Reporter.Write($"var = {var.ToString()}");
      // }

      // [Fact] public void StructInArrayBehavior() {       // Behaves well.
      //    var vars  = new Variable[1] {
      //      new Variable(1, true)
      //    };
      //    vars[0].Val = 3.0;
      //    Assert.True(vars[0].Val == 3.0);
      // }
   }
}