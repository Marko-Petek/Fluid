using System;
using System.Collections.Generic;
using Fluid.Internals.Algebras;

namespace Fluid.Internals.Networks {

   /// <summary>Can form groups with other Nodes.</summary>
   public interface INode {
      /// <summary>Connections: viewed either as a shape grouping nodes, or as a node grouping shapes.</summary>
      INetwork Network { get; }

   }
}