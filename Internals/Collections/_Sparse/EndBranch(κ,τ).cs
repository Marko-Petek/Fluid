using System.Collections.Generic;

namespace Fluid.Internals.Collections {
   public class EndBranch<κ,τ> : Branch<κ,τ> {
      List<KeyValuePair<κ,τ>> Leaves { get; }
      public EndBranch() : base() {
         Leaves = new List<KeyValuePair<κ,τ>>(7);
      }
   }
}