using System.Collections.Generic;
namespace Fluid.Internals.Collections {

/// <summary>A base for concrete implementation of the same name.</summary>
/// <typeparam name="τ">Type of peers whose references are stored in the dictionary.</typeparam>
public abstract class Node<τ> where τ : Node<τ> {
   /// <summary>
   /// 
   /// </summary>
   /// <typeparam name="τ"></typeparam>
   public Dictionary<τ,int> Peers { get; protected set; } = new Dictionary<τ,int>();
   int RecentIndex { get; set; } = 0;

   public Node() {}
   public Node(τ peer) => Connect(peer);
   public Node(IEnumerable<τ> peers) {
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