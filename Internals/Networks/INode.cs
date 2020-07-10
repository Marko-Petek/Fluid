using System;
using System.Collections.Generic;
using Fluid.Internals.Algebras;

namespace Fluid.Internals.Networks {

   /// <summary>Can form groups with other Nodes.</summary>
   public interface INode {
      /// <summary>A network to which the node belongs.</summary>
      INetwork Network { get; }
      int Id { get; }
      INode[] Bond { get; }
   }
}