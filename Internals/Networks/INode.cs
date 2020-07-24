using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluid.Internals.Networks {
/// <summary>A node is a collection of references to other nodes.</summary>
   public interface INode {
      /// <summary>Is non-existent (set to null) by default.</summary>
      protected Dictionary<INode,List<int>>? Bond { get; set; }       // (which node it is connected to; sequential connection slot number)

      /// <summary>Add a connection to node regardless if already connected.
      ///   C-connect: never throw exception, connection to the same (negative) integer are treated as a group.
      ///   P-connect: throw an exception if connecting to the same non-negative integer.</summary>
      /// <param name="node">The "connectee". Node being connected to.</param>
      /// <param name="slot">A negative (C-connect) or non-negative (P-connect) integer.</param>
      void ConnectA(INode node, int slot) {
         if(Bond == null) {                                       // Bond dictionary does not exist at all.
            Bond = new Dictionary<INode, List<int>>(3);           // Make space for 3 entries initially.
            var list = new List<int>(1) {slot};
            Bond.Add(node, list);
         }

         if(!Bond.TryGetValue(node, out list)) {                             // Bond dictionary entry for the other node (method's argument) does not even exist = no connections yet leading to the node.
            list = new List<int>(1);
            list.Add(slot); }
         else if(slot >= 0 && list.Contains(slot))                           // P-connection already exists at specified slot. This is the only case that is absolutely invalid.
            throw new InvalidOperationException("Already connected.");
         else
            list.Add(slot);
         
         Bond.
      }

      /// <summary>Add a connection to node only if not yet connected. Treat any existing connection as erroneous method usage.
      /// C-connect: throw an exception when a connection exists at any negative integer.
      /// P-connect: throw an exception when a connection exists at any non-negative integer.</summary>
      /// <param name="node1">The "connectee". Node being connected to.</param>
      /// <param name="slot">A negative (C-connect) or non-negative (P-connect) integer.</param>
      void Connect2(INode node, int slot) {
         List<int>? list;
         if(!Bond.TryGetValue(node, out list)) {
            list = new List<int>(1);
            list.Add(slot); }
         else
            throw new InvalidOperationException("Already connected.");
      }

      // Disconnect at a specific slot. Treat a non-existing node at the slot as erroneous usage.
      void Disconnect1(INode node1, int slot) {
         List<int>? list;

      }
   }
}