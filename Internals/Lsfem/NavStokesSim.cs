using System;
using System.Collections.Generic;
using dbl = System.Double;
using DA = Fluid.Internals.Numerics.DblArithmetic;

namespace Fluid.Internals.Lsfem {
   using Vec = Fluid.Internals.Collections.Vector<dbl,DA>;
   using Tnr = Fluid.Internals.Collections.Tensor<dbl, DA>;
   using Lst = List<int>;
   public abstract class NavStokesSim : Simulation {
      /// <summary>Construct new A and Fs for the next time step.</summary>
      /// <param name="nextA">Next step dynamics.</param>
      /// <param name="nextFs">Next step forcing (secondary).</param>
      /// <param name="prevUF_dk">Previous step state.</param>
      protected override (Tnr nextA, Tnr nextFs, dbl dt) AdvanceDynamics(Tnr prevUF_dk, Tnr prevFs_hsi) {
         var nextA = new Tnr(new Lst{NfPos,Nm,NfPos,Nm}, NfPos);     // Initialize new A. 
         var nextFs = new Tnr(new Lst{NPos})
         foreach(var tnr_uf_k in prevUF_dk) {
            int d = tnr_uf_k.Key;                  // Position index.
            Vec uf_k = (Vec) tnr_uf_k.Value;       // Vector of variables at that position.

            dbl u = uf_k[0],
                v = uf_k[1],
                p = uf_k[2],
                ω = uf_k[3],
         }
      }
   }
}