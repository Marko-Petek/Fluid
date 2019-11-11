using System;
using Fluid.ChannelFlow;
using Fluid.Internals.Lsfem;
using Fluid.Internals.Numerics;

namespace Fluid.Tests.Mocks {
   public class ObstructionBlockMock : ObstructionBlock {
      public ObstructionBlockMock() {
         NRows = 2;
         NCols = 2;
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
      public override double[] Solution(in Pos pos, params int[] vars) {
         throw new NotImplementedException();
      }
   }
}