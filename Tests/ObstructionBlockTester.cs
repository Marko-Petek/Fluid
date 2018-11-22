using System;
using Fluid.ChannelFlow;
using Fluid.Dynamics.Meshing;
using Fluid.Dynamics.Numerics;

namespace Fluid.Tests
{
    public class ObstructionBlockTester : ObstructionBlock
    {
        public ObstructionBlockTester() {
            _rowCount = 2;
            _colCount = 2;
        }

        protected override int ApplyConstraints() {
            return 0;
        }

        protected override void CreateNodes() {

        }

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