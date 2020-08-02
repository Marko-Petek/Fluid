using System;
using System.Collections.Generic;
using System.Linq;

namespace Fluid.Internals.Networks {
/// <summary>A node is a collection of references to other nodes.</summary>
   public interface INode<τ> {
      /// <summary>Is non-existent (set to null) by default.</summary>
      protected Dictionary<INode<τ>,List<int>>? Bond { get; set; }       // (which node it is connected to; sequential connection slot number)

      /// <summary>Add a connection to node:
      ///   C-connect: never throw exception, connections to the same (negative) integer are treated as a group.
      ///   P-connect: throw an exception if connecting to the same non-negative integer.</summary>
      /// <param name="node">The "connectee". Node being connected to.</param>
      /// <param name="slot">A negative (C-connect) or non-negative (P-connect) integer.</param>
      void ConnectA(INode<τ> node, int slot) {
         List<int>? list;
         if(Bond == null) {                                       // Bond dictionary does not exist at all.
            Bond = new Dictionary<INode<τ>, List<int>>(3);           // Make space for 3 entries initially.
            list = new List<int>(1) {slot};
            Bond.Add(node, list); }
         else if(!Bond.TryGetValue(node, out list)) {                             // Bond dictionary entry for the other node (method's argument) does not even exist = no connections yet leading to the node.
            list = new List<int>(1);
            list.Add(slot);
            Bond.Add(node, list); }
         else if(slot < 0 || !list.Contains(slot))
            list.Add(slot);                                                // Bond entry Bond[node] already exists, no need to add.
         else // if(slot >= 0 && list.Contains(slot))      P-connection already exists at specified slot. This is the only case that is absolutely invalid.
            throw new InvalidOperationException("Already connected.");
      }

      /// <summary>Add a connection to node:
      /// C-connect: throw exception when a connection exists at any negative integer.
      /// P-connect: throw an exception if connecting to the same non-negative integer.</summary>
      /// <param name="node1">The "connectee". Node being connected to.</param>
      /// <param name="slot">A negative (C-connect) or non-negative (P-connect) integer.</param>
      void ConnectB(INode<τ> node, int slot) {
         List<int>? list;
         if(Bond == null) {                                       // Bond dictionary does not exist at all.
            Bond = new Dictionary<INode<τ>, List<int>>(3);           // Make space for 3 entries initially.
            list = new List<int>(1) {slot};
            Bond.Add(node, list); }
         else if(!Bond.TryGetValue(node, out list)) {                             // Bond dictionary entry for the other node (method's argument) does not even exist = no connections yet leading to the node.
            list = new List<int>(1);
            list.Add(slot);
            Bond.Add(node, list); }
         else if(!list.Contains(slot))                          // Bond was there, list was there, but that exact slot was unoccupied.
            list.Add(slot);
         else                                                  // Connection already exists at specified slot.
            throw new InvalidOperationException("Already connected.");
      }

      // Disconnect at a specific slot. Treat a non-existing node at the slot as erroneous usage.
      void DisconnectA(INode<τ> node, int slot) {
         List<int>? list;
         if(Bond != null) {
            if(Bond.TryGetValue(node, out list)) {
               if(!list.Remove(slot))                                               // No connection present specified at slot = error.
                  throw new InvalidOperationException("No existing connection."); } }
         else
            throw new InvalidOperationException("No existing connection.");
      }
   }
}