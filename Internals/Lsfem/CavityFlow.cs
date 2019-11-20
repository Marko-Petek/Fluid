using System;
using System.IO;
using System.Collections.Generic;
using static System.Math;

using Fluid.Internals.Collections;
using Fluid.Internals.Lsfem;
using Fluid.Internals.Numerics;
using static Fluid.Internals.Toolbox;
using dbl = System.Double;
using DA = Fluid.Internals.Numerics.DblArithmetic;


namespace Fluid.Internals.Lsfem {
using SymTnr = SymTensor<dbl,DA>;
using Vec = Fluid.Internals.Collections.Vector<dbl,DA>;
using Tnr = Fluid.Internals.Collections.Tensor<dbl, DA>;
using Lst = List<int>;
using PE = PseudoElement;
using Emt = Element;

/// <summary>Driven cavity flow problem is a very simple problem with which methods are initially tested.</summary>
public class CavityFlow : NavStokesFlow {
   /// <summary>Boundary velocity.</summary>
   public dbl V { get; }
   
   
   CavityFlow(CavityFlowInit ci) : base(simInit, navStokesFlowInit) {
      
      V = v;
   }

   

   
}
}