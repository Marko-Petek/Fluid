using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluid.Internals.Networks {

/// <summary>Action node. Delegate calls propagate along this network.</summary>
public interface IActionNode : INode {

   
   protected Dictionary<INode,List<int>>? ActionBond { get; set; }

}
}