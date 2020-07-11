using System;
using System.Collections.Generic;

namespace Fluid.Internals.Networks {
   /// <summary>A network is a dictionary of nodes, keyed by unique ID of type τ. Type τ must be a reference type.</summary>
   /// <typeparam name="τ">Key type used in internal dictionary.</typeparam>
   public interface INetworkR<τ> : INetwork<τ>
   where τ : class {
   /// <summary>Internal dictionary of nodes.</summary>
   Dictionary<τ,INode> Nodes { get; }
   /// <summary>Indexer used to get and set non-null nodes. Getter: knowledge of existence of node at particular index assumed, null case throws exception. Setter: setting of null warned by linter.</summary>
   INode this[τ key] {
      get {
         if(Nodes.TryGetValue(key, out var node))
            return node;
         else throw new ArgumentException($"Node at key {key} does not exist.", nameof(key)); }
      set {
         Nodes[key] = value; }
   }
   /// <summary>Attempt to get a node at specified key. Returns null if a node is not there.</summary>
   /// <param name="key">Node key.</param>
   INode? TryGet(τ key) {
      if(Nodes.TryGetValue(key, out var node))
         return node;
      else return null;
   }
   /// <summary>Set node at specified key. A node that was either replaced or removed is returned. If no node was replaced or removed, null is returned. A null set value removes an existing node. </summary>
   /// <param name="key">Node key.</param>
   /// <param name="node">Node to set.</param>
   INode? TrySet(τ key, INode? node) {
      INode? oldNode = TryGet(key);
      if(node != null)
         Nodes[key] = node;
      else if(oldNode != null)
         Nodes.Remove(key);
      return oldNode;
   }
   /// <summary>Assigns a unique identity to a node. Returns its identity.</summary>
   /// <param name="node">Node to add to network.</param>
   int Add(INode node);
   /// <summary>Removes a node at specified key. Returns the removed node, or null if there was no node at the key.</summary>
   /// <param name="key">Node key.</param>
   INode? Remove(τ key);
}
}