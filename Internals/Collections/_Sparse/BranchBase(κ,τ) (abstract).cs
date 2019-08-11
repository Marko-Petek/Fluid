using System.Collections.Generic;

namespace Fluid.Internals.Collections {
   public abstract class BranchBase<κ,τ> : List<KeyValuePair<κ,τ>> where τ : BranchBase<κ,τ> {
      public BranchBase() : base(7) { }
   }
}