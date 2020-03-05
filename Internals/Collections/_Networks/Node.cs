using System;
using System.Collections.Generic;

namespace Fluid.Internals.Collections {
   public class Node : Node<Node> {
      public Node(IEnumerable<Node> peers) : base(peers) {}
   }
}