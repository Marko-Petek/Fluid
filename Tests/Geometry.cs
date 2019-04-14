using System;
using static System.Console;
using Xunit;

using Fluid.ChannelFlow;
using Fluid.Internals;
using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;
using Fluid.Internals.Meshing;
using Fluid.Tests.Mocks;
using TB = Fluid.Internals.Toolbox;

namespace Fluid.Tests {
   public class GeometryTests {
      static GeometryTests() => TB.TryInitialize();

      [InlineData(-0.5, 0.5)]
      [InlineData(0.5, 0.5)]
      [InlineData(0.0, 0.5)]
      [InlineData(0.0, -0.5)]
      [Theory] public void PointInsideQuadrilateral(double x, double y) {
         var point = new Pos(x,y);
         var vertices = new Pos[4] {                 // Define vertices of  quadrilateral.
            new Pos(-1.0, -1.0),
            new Pos(1.0, -1.0),
            new Pos(2.0, 1.0),
            new Pos(-2.0, 1.0) };                             
         Assert.True(point.IsInsidePolygon(vertices));
      }

      [InlineData(-2.0, 0.0)] [InlineData(2.0, 0.0)] [InlineData(0.0, 1.5)] [InlineData(0.0, -1.5)]
      [Theory] public void PointOutsideQuadrilateral(double x, double y) {
         var point = new Pos(x,y);
         var vertices = new Pos[4] {                 // Define vertices of  quadrilateral.
            new Pos(-1.0, -1.0),
            new Pos(1.0, -1.0),
            new Pos(2.0, 1.0),
            new Pos(-2.0, 1.0) };     
         Assert.True(!point.IsInsidePolygon(vertices));
      }

      [InlineData(1, 0)] [InlineData(1.1, 0.2)] [InlineData(3.9, 0.99)] [InlineData(1.01, 0.99)]
      [InlineData(2.5, 0.5)] [InlineData(3.1, 0.66)] [InlineData(1.00000001, 0.9999999)] [InlineData(2, 0.1)]
      [Theory] public void TestPointInsidePolygon1(double x, double y) {
         var point = new Pos(x,y);
         var vertices = new Pos[4] {
            new Pos(1,0),
            new Pos(4,0),
            new Pos(4,1),
            new Pos(1,1) };
         Assert.True(point.IsInsidePolygon(vertices));
      }
      // TODO: Test IsInsidePolygon method more.
      [InlineData(0.35, 0.0)]
      [Theory] public void TestPointOutsidePolygon1(double x, double y) {
         var point = new Pos(x,y);
         var vertices = new Pos[4] {
            new Pos(1,0),
            new Pos(4,0),
            new Pos(4,1),
            new Pos(1,1) };
         Assert.False(point.IsInsidePolygon(vertices));
      }

      [Fact] public void StructBehavior() {
         TB.TryInitialize();
         Variable var = new Variable(1, true);
         var.Val = 2;
         TB.Reporter.Write($"var = {var.ToString()}");
      }
   }
}