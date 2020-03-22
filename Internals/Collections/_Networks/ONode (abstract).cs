using System.Collections.Generic;
namespace Fluid.Internals.Collections {

/// <summary>ONode is an (O)rdered set that holds references to peers of type τ (that are also ONodes). Ordered implies the usual integer metric with regards to its peers (numbered spots, aka indices).</summary>
/// <typeparam name="τ">Type of peers.</typeparam>
public abstract class ONode<τ> : Dictionary<int,τ>  where τ : ONode<τ> {

   /// <summary>Create a node with a given initial capacity.</summary>
   /// <param name="cap">Initial capacity of dictionary.</param>
   public ONode(int cap) : base(cap) {}
   
   /// <summary>Adds a node to its peers at the specified index. Throws exception if the node is already a peer.</summary>
   /// <param name="i">Index.</param>
   /// <param name="peer">New peer.</param>
   new public void Add(int i, τ peer) =>
      base.Add(i, peer);

}
}