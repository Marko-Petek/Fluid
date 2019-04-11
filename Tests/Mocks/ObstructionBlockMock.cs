using System;
using Fluid.ChannelFlow;
using Fluid.Internals.Meshing;
using Fluid.Internals.Numerics;

namespace Fluid.Tests.Mocks {
   public class ObstructionBlockMock : ObstructionBlock {
      public ObstructionBlockMock() {
         RowCount = 2;
         ColCount = 2;
      }

      protected override int ApplyConstraints() {
         return 0;
      }
      protected override void CreateNodes() { }
      protected override Pos CalcLeftBoundaryPos(double eta) {
         throw new NotImplementedException();
      }
      protected override Pos CalcLowerBoundaryPos(double eta) {
         throw new NotImplementedException();
      }
      protected override Pos CalcRightBoundaryPos(double eta) {
         throw new NotImplementedException();
      }
      protected override Pos CalcUpperBoundaryPos(double eta) {
         throw new NotImplementedException();
      }
      public override double[] Solution(ref Pos pos, params int[] vars) {
         throw new NotImplementedException();
      }
   }
}