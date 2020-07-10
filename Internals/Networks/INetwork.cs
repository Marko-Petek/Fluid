using System;
using System.Collections.Generic;

namespace Fluid.Internals.Networks {
/// <summary>A network is a dictionary of nodes, keyed by integer ID.</summary>
public interface INetwork {
   /// <summary>Internal dictionary of nodes.</summary>
   Dictionary<int,INode> Nodes { get; }
   /// <summary>Indexer used to get and set non-null nodes. Getter: knowledge of existence of node at particular index assumed, null case throws exception. Setter: setting of null warned by linter.</summary>
   INode this[int inx] {
      get {
         if(Nodes.TryGetValue(inx, out var node))
            return node;
         else throw new ArgumentException($"Node at index {inx} does not exist.", nameof(inx)); }
      set {
         Nodes[inx] = value; }
   }
   /// <summary>Attempt to get a node at specified index. Returns null if a node is not there.</summary>
   /// <param name="inx">Node index.</param>
   INode? TryGet(int inx) {
      if(Nodes.TryGetValue(inx, out var node))
         return node;
      else return null;
   }
   /// <summary>Set node at specified index. A null set removes an existing node. A node that was either replaced or removed is returned. If no node was replaced or removed, null is returned.</summary>
   /// <param name="inx">Node index.</param>
   /// <param name="node">Node to set.</param>
   INode? TrySet(int inx, INode node) {
      INode? oldNode = TryGet(inx);
      Nodes[inx] = node;
      return oldNode;
   }
   /// <summary>Set node at specified index. A null set removes an existing node.</summary>
   /// <param name="inx">Node index.</param>
   /// <param name="node">Node to set.</param>
   void Set(int inx, INode node) =>
      Nodes[inx] = node;
   void Add
}
}