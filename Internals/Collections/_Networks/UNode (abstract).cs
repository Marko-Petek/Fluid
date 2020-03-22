using System.Collections.Generic;
using System.Linq;
namespace Fluid.Internals.Collections {

/// <summary>UNode is an (U)nordered set that holds references to peers of type τ (that are also UNodes). Unordered implies no metric with regards to its peers (unnumbered spots, aka no indices).</summary>
/// <typeparam name="τ">Type of peers.</typeparam>
public abstract class UNode<τ> : HashSet<τ>  where τ : UNode<τ> {
   /// <summary>Create a node with a given initial capacity.</summary>
   /// <param name="cap">Initial capacity of dictionary.</param>
   internal UNode(int cap) : base(cap) {}

   /// <summary>Creates an unordered node with the internal capacity of the provided "peers" IEnumerable and adds the peers.</summary>
   /// <param name="peers">Peers of new node.</param>
   internal UNode(IEnumerable<τ> peers) : this(peers.Count())  {
      Add(peers);
   }

   /// <summary>Adds a node to its peers. Throws exception if the node is already a peer.</summary>
   /// <param name="peer">New peer to add.</param>
   new public void Add(τ peer) =>
      base.Add(peer);
   /// <summary>Adds a number of nodes to its peers. Throws exception if any of the nodes is already a peer.</summary>
   /// <param name="peers">New peers to add.</param>
   public void Add(IEnumerable<τ> peers) {
      foreach(var peer in peers)
         Add(peer);
   }

}
}