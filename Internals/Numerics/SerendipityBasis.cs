using System;
using System.Collections.Generic;
using static System.Math;
using dbl = System.Double;
using dA = Fluid.Internals.Numerics.DblArithmetic;
using F2DA = Fluid.Internals.Numerics.Func2DArithmetic;
using Fluid.Internals.Collections;

//using Fluid.Internals.Meshing;

namespace Fluid.Internals.Numerics {
   using FTnr = Tensor<Func2D,F2DA>;
   using FVec = Vector<Func2D,F2DA>;
   public static class SerendipityBasis {
      /// <summary>12 Serendipity basis functions [0][i from 0 to 11] and their partials over ξ [1][i from 0 to 11] and η[2][i from 0 to 11]. All defined on reference square</summary>
      public static FTnr ϕ { get; } = new FTnr(new List<int>(2) {12,3}, 12) {
         [FVec.VoidVector, 0] = new FVec {
            [0] = new Func2D( (ξ,η) => ((-1 + η)*(1 - ξ)*(-2 + η + η*η + ξ + ξ*ξ))/8.0 ),
            [1] = new Func2D( (ξ,η) => -((-1 + η)*(-3 + η + η*η + 3*ξ*ξ))/8.0 ),
            [2] = new Func2D( (ξ,η) => -((-1 + ξ)*(-3 + 3*η*η + ξ + ξ*ξ))/8.0 ) },
         [FVec.VoidVector, 1] = new FVec {
            [0] = new Func2D( (ξ,η) => -((-1 + η)*Pow(-1 + ξ,2)*(1 + ξ))/8.0 ),
            [1] = new Func2D( (ξ,η) => -((-1 + η)*(-1 - 2*ξ + 3*ξ*ξ))/8.0 ),
            [2] = new Func2D( (ξ,η) => -(Pow(-1 + ξ,2)*(1 + ξ))/8.0 ) },
         [FVec.VoidVector, 2] = new FVec {
            [0] = new Func2D( (ξ,η) => ((-1 + η)*(-1 + ξ)*Pow(1 + ξ,2))/8.0 ),
            [1] = new Func2D( (ξ,η) => ((-1 + η)*(1 + ξ)*(-1 + 3*ξ))/8.0 ),
            [2] = new Func2D( (ξ,η) => ((-1 + ξ)*Pow(1 + ξ,2))/8.0 ) },
         [FVec.VoidVector, 3] = new FVec {
            [0] = new Func2D( (ξ,η) => ((-1 + η)*(1 + ξ)*(-2 + η + η*η - ξ + ξ*ξ))/8.0 ),
            [1] = new Func2D( (ξ,η) => ((-1 + η)*(-3 + η + η*η + 3*ξ*ξ))/8.0 ),
            [2] = new Func2D( (ξ,η) => ((1 + ξ)*(-3 + 3*η*η - ξ + ξ*ξ))/8.0 ) },
         [FVec.VoidVector, 4] = new FVec {
            [0] = new Func2D( (ξ,η) => (Pow(-1 + η,2)*(1 + η)*(1 + ξ))/8.0),
            [1] = new Func2D( (ξ,η) => (Pow(-1 + η,2)*(1 + η))/8.0 ),
            [2] = new Func2D( (ξ,η) => ((-1 + η)*(1 + 3*η)*(1 + ξ))/8.0 ) },
         [FVec.VoidVector, 5] = new FVec {
            [0] = new Func2D( (ξ,η) => ((-1 + η)*Pow(1 + η,2)*(-1 - ξ))/8.0 ),
            [1] = new Func2D( (ξ,η) => -((-1 + η)*Pow(1 + η,2))/8.0 ),
            [2] = new Func2D( (ξ,η) => -((-1 + 2*η + 3*η*η)*(1 + ξ))/8.0 ) },
         [FVec.VoidVector, 6] = new FVec {
            [0] = new Func2D( (ξ,η) => ((1 + η)*(-1 - ξ)*(-2 - η + η*η - ξ + ξ*ξ))/8.0 ),
            [1] = new Func2D( (ξ,η) => -((1 + η)*(-3 - η + η*η + 3*ξ*ξ))/8.0 ),
            [2] = new Func2D( (ξ,η) => -((1 + ξ)*(-3 + 3*η*η - ξ + ξ*ξ))/8.0 ) },
         [FVec.VoidVector, 7] = new FVec {
            [0] = new Func2D( (ξ,η) => ((1 + η)*(1 - ξ)*Pow(1 + ξ,2))/8.0 ),
            [1] = new Func2D( (ξ,η) => -((1 + η)*(-1 + 2*ξ + 3*ξ*ξ))/8.0 ),
            [2] = new Func2D( (ξ,η) => -((-1 + ξ)*Pow(1 + ξ,2))/8.0 ) },
         [FVec.VoidVector, 8] = new FVec {
            [0] = new Func2D( (ξ,η) => ((1 + η)*Pow(-1 + ξ,2)*(1 + ξ))/8.0 ),
            [1] = new Func2D( (ξ,η) => ((1 + η)*(-1 + ξ)*(1 + 3*ξ))/8.0 ),
            [2] = new Func2D( (ξ,η) => (Pow(-1 + ξ,2)*(1 + ξ))/8.0 ) },
         [FVec.VoidVector, 9] = new FVec {
            [0] = new Func2D( (ξ,η) => ((1 + η)*(-1 + ξ)*(-2 - η + η*η + ξ + ξ*ξ))/8.0 ),
            [1] = new Func2D( (ξ,η) => ((1 + η)*(-3 - η + η*η + 3*ξ*ξ))/8.0 ),
            [2] = new Func2D( (ξ,η) => ((-1 + ξ)*(-3 + 3*η*η + ξ + ξ*ξ))/8.0 ) },
         [FVec.VoidVector, 10] = new FVec {
            [0] = new Func2D( (ξ,η) => ((-1 + η)*Pow(1 + η,2)*(-1 + ξ))/8.0 ),
            [1] = new Func2D( (ξ,η) => ((-1 + η)*Pow(1 + η,2))/8.0 ),
            [2] = new Func2D( (ξ,η) => ((1 + η)*(-1 + 3*η)*(-1 + ξ))/8.0 ) },
         [FVec.VoidVector, 11] = new FVec {
            [0] = new Func2D( (ξ,η) => (Pow(-1 + η,2)*(1 + η)*(1 - ξ))/8.0 ),
            [1] = new Func2D( (ξ,η) => -(Pow(-1 + η,2)*(1 + η))/8.0 ),
            [2] = new Func2D( (ξ,η) => -((-1 - 2*η + 3*η*η)*(-1 + ξ))/8.0 ) } };


      // /// <summary>12 Serendipity basis functions [0][i from 0 to 11] and their partials over ξ [1][i from 0 to 11] and η[2][i from 0 to 11]. All defined on reference square</summary>
      // public static Func2D[][] ϕ { get; } = new Func2D[12][] {
      //    new Func2D[3] {                                                      // E-node 0 basis funcs.
      //       new Func2D( (ξ,η) => ((-1 + η)*(1 - ξ)*(-2 + η + η*η + ξ + ξ*ξ))/8.0 ),
      //       new Func2D( (ξ,η) => -((-1 + η)*(-3 + η + η*η + 3*ξ*ξ))/8.0 ),
      //       new Func2D( (ξ,η) => -((-1 + ξ)*(-3 + 3*η*η + ξ + ξ*ξ))/8.0 )
      //    },
      //    new Func2D[3] {                                                       // E-node 1 basis funcs.
      //       new Func2D( (ξ,η) => -((-1 + η)*Pow(-1 + ξ,2)*(1 + ξ))/8.0 ),
      //       new Func2D( (ξ,η) => -((-1 + η)*(-1 - 2*ξ + 3*ξ*ξ))/8.0 ),
      //       new Func2D( (ξ,η) => -(Pow(-1 + ξ,2)*(1 + ξ))/8.0 )
      //    },
      //    new Func2D[3] {                                                       // E-node 2 basis funcs.
      //       new Func2D( (ξ,η) => ((-1 + η)*(-1 + ξ)*Pow(1 + ξ,2))/8.0 ),
      //       new Func2D( (ξ,η) => ((-1 + η)*(1 + ξ)*(-1 + 3*ξ))/8.0 ),
      //       new Func2D( (ξ,η) => ((-1 + ξ)*Pow(1 + ξ,2))/8.0 )
      //    },
      //    new Func2D[3] {                                                      // E-node 3 basis funcs.
      //       new Func2D( (ξ,η) => ((-1 + η)*(1 + ξ)*(-2 + η + η*η - ξ + ξ*ξ))/8.0 ),
      //       new Func2D( (ξ,η) => ((-1 + η)*(-3 + η + η*η + 3*ξ*ξ))/8.0 ),
      //       new Func2D( (ξ,η) => ((1 + ξ)*(-3 + 3*η*η - ξ + ξ*ξ))/8.0 )
      //    },
      //    new Func2D[3] {                                                      // E-node 4 basis funcs.
      //       new Func2D( (ξ,η) => (Pow(-1 + η,2)*(1 + η)*(1 + ξ))/8.0),
      //       new Func2D( (ξ,η) => (Pow(-1 + η,2)*(1 + η))/8.0 ),
      //       new Func2D( (ξ,η) => ((-1 + η)*(1 + 3*η)*(1 + ξ))/8.0 )
      //    },
      //    new Func2D[3] {                                                      // E-node 5 basis funcs.
      //       new Func2D( (ξ,η) => ((-1 + η)*Pow(1 + η,2)*(-1 - ξ))/8.0 ),
      //       new Func2D( (ξ,η) => -((-1 + η)*Pow(1 + η,2))/8.0 ),
      //       new Func2D( (ξ,η) => -((-1 + 2*η + 3*η*η)*(1 + ξ))/8.0 )
      //    },
      //    new Func2D[3] {                                                      // E-node 6 basis funcs.
      //       new Func2D( (ξ,η) => ((1 + η)*(-1 - ξ)*(-2 - η + η*η - ξ + ξ*ξ))/8.0 ),
      //       new Func2D( (ξ,η) => -((1 + η)*(-3 - η + η*η + 3*ξ*ξ))/8.0 ),
      //       new Func2D( (ξ,η) => -((1 + ξ)*(-3 + 3*η*η - ξ + ξ*ξ))/8.0 )
      //    },
      //    new Func2D[3] {                                                      // E-node 7 basis funcs.
      //       new Func2D( (ξ,η) => ((1 + η)*(1 - ξ)*Pow(1 + ξ,2))/8.0 ),
      //       new Func2D( (ξ,η) => -((1 + η)*(-1 + 2*ξ + 3*ξ*ξ))/8.0 ),
      //       new Func2D( (ξ,η) => -((-1 + ξ)*Pow(1 + ξ,2))/8.0 )
      //    },
      //    new Func2D[3] {                                                      // E-node 8 basis funcs.
      //       new Func2D( (ξ,η) => ((1 + η)*Pow(-1 + ξ,2)*(1 + ξ))/8.0 ),
      //       new Func2D( (ξ,η) => ((1 + η)*(-1 + ξ)*(1 + 3*ξ))/8.0 ),
      //       new Func2D( (ξ,η) => (Pow(-1 + ξ,2)*(1 + ξ))/8.0 )
      //    },
      //    new Func2D[3] {                                                      // E-node 9 basis funcs.
      //       new Func2D( (ξ,η) => ((1 + η)*(-1 + ξ)*(-2 - η + η*η + ξ + ξ*ξ))/8.0 ),
      //       new Func2D( (ξ,η) => ((1 + η)*(-3 - η + η*η + 3*ξ*ξ))/8.0 ),
      //       new Func2D( (ξ,η) => ((-1 + ξ)*(-3 + 3*η*η + ξ + ξ*ξ))/8.0 )
      //    },
      //    new Func2D[3] {                                                      // E-node 10 basis funcs.
      //       new Func2D( (ξ,η) => ((-1 + η)*Pow(1 + η,2)*(-1 + ξ))/8.0 ),
      //       new Func2D( (ξ,η) => ((-1 + η)*Pow(1 + η,2))/8.0 ),
      //       new Func2D( (ξ,η) => ((1 + η)*(-1 + 3*η)*(-1 + ξ))/8.0 )
      //    },
      //    new Func2D[3] {                                                      // E-node 11 basis funcs.
      //       new Func2D( (ξ,η) => (Pow(-1 + η,2)*(1 + η)*(1 - ξ))/8.0 ),
      //       new Func2D( (ξ,η) => -(Pow(-1 + η,2)*(1 + η))/8.0 ),
      //       new Func2D( (ξ,η) => -((-1 - 2*η + 3*η*η)*(-1 + ξ))/8.0 )
      //    }
      // };
   }
}