using System;

using Fluid.Internals.Numerics;

namespace Fluid.Internals.Lsfem {
   /// <summary>Contains up to 5 unique nodes from the Mesh that are contained in no other PseudoElement.</summary>
   public class PseudoElement {
      /// <summary>Local indices of nodes from 0 to 4.</summary>
      public int[] LInx { get; set; }
      /// <summary>Global indices of nodes on Mesh.</summary>
      public int[] GInx { get; set; }
      /// <summary>Positions of nodes.</summary>
      public Vec2[] Pos { get; }
   }
}