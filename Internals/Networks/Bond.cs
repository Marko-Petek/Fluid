using System;
using System.Collections.Generic;

namespace Fluid.Internals.Networks {

   public readonly struct Bond {
      /// <summary>Bond type is either a Combination type or a Permutation type.</summary>
      public readonly BondType Type;
      public readonly uint[] Cons;

      public Bond(BondType type, params uint[] cons) {
         Type = type;
         Cons = cons;
      }
   }

   /// <summary>Specifies how order of connections inside a bond should be treated.</summary>
   public enum BondType {
      /// <summary>Combination type: order of connections does not matter.</summary>
      C,
      /// <summary>Permutation type: order of connections matters.</summary>
      P
   }
}
