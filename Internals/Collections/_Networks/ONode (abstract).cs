using System.Collections.Generic;
namespace Fluid.Internals.Collections {

/// <summary>Node is actaully a dictionary that holds references to peers of type τ.</summary>
/// <typeparam name="τ">Type of peers.</typeparam>
public abstract class ONode<τ> : Dictionary<int,τ>  where τ : ONode<τ> {

   /// <summary>Create a node with a given initial capacity.</summary>
   /// <param name="cap">Initial capacity of dictionary.</param>
   public ONode(int cap) : base(cap) {}

   public ONode(IEnumerable<τ> peers) {
      Connect(peers);
   }

   public void Connect(τ peer) {
      Peers[peer] = RecentIndex++;
      peer.Peers[(τ)this] = peer.RecentIndex;
   }
   public void Connect(IEnumerable<τ> peers) {
      foreach(var peer in peers)
         Connect(peer);
   }

}
}