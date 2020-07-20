using System;
using System.Collections.Generic;

namespace Fluid.Internals.Networks {
/// <summary>A node is a collection of references to other nodes.</summary>
   public interface INode {
      
      protected Dictionary<INode,int[]> Bonds { get; }       // (which node it is connected to; sequential connection slot number)

      // C parts of Bonds have negative values.
      // P parts have positive values.
   }
}