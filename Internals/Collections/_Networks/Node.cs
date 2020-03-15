using System;
using System.Collections.Generic;

namespace Fluid.Internals.Collections {
   /// <summary>A type with an I</summary>
   public class Node : Node<Node> {
      public Node(IEnumerable<Node> peers) : base(peers) {}
   }
}