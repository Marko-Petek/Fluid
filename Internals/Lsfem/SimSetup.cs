#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Fluid.Internals.Numerics;
using Fluid.Internals.Collections;
using My = Fluid.Internals.Collections.Custom;
using static Fluid.Internals.Toolbox;
using dbl = System.Double;
using DA = Fluid.Internals.Numerics.DblArithmetic;
using Supercluster.KDTree;
namespace Fluid.Internals.Lsfem {

using SymTnr = SymTensor<dbl,DA>;
using Vec = Fluid.Internals.Collections.Vector<dbl,DA>;
using Tnr = Fluid.Internals.Collections.Tensor<dbl, DA>;
using Voids = Voids<dbl,DA>;
using Lst = List<int>;
using VecLst = My.List<Vec2>;
using PE = PseudoElement;

public class SimSetup {
   public dbl Dt { get; }


   public SimSetup(dbl dt) {
      Dt = dt;
   }


   /// <summary>We must call this from the derived type's factory method.</summary>
   /// <param name="sim"></param>
   protected static void Initialize() {                                       R!.R("Started Sim.Initialize.");
      var sim = new Simulation()
      int currGInx = 0;
      (currGInx, sim.Patches) = sim.CreatePatches(currGInx);                  R.R("Created Patches.");
      (currGInx, sim.Joints) = sim.CreateJoints(currGInx);                    R.R("Created Joints.");
      (sim.NPos, sim.Pos) = sim.CreatePositions();                            R.R("Created Positions.");
      var emts = sim.CreateElements();                                        R.R("Created Elements and calculated overlap integrals.");
      var coms = sim.CreateCOMs(emts);                                        R.R("Created a list of Centers of Mass.");
      sim.EmtTree = sim.CreateElementTree(coms, emts);                       R.R("Created the ElementTree.");
      sim.UC = sim.CreateConstraints();                                       R.R("Created constraints.");
      sim.NfPos = sim.CountFreePositions(sim.NPos, sim.NVar);                 R.R("Counted free positions.");
   }


   public Simulation CreateSim() {                                            R!.R("Started Simulation creation.");
      
   }
}

}


#nullable restore