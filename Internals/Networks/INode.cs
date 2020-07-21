using System;
using System.Collections.Generic;

namespace Fluid.Internals.Networks {
/// <summary>A node is a collection of references to other nodes.</summary>
   public interface INode {
      protected Dictionary<INode,List<int>> Bond { get; }       // (which node it is connected to; sequential connection slot number)

      void ConnectC(INode node) {
         if(!Bond.TryGetValue(node, out var list)) {            // If a connection already exists.
            throw new InvalidOperationException("Already connected."); }
         else {

         }
      }
   }
}