using System;
using System.Collections.Generic;

namespace Fluid.Internals.Networks.Old {
   public abstract class Node<T> where T : Node<T> {
      public Dictionary<T,int> Peers { get; protected set; } = new Dictionary<T,int>();
      int RecentIndex { get; set; } = 0;

      public Node() {}
      public Node(T peer) => Connect(peer);
      public Node(IEnumerable<T> peers) {
         Connect(peers);
      }

      public void Connect(T peer) {
         Peers[peer] = RecentIndex++;
         peer.Peers[(T)this] = peer.RecentIndex;
      }
      public void Connect(IEnumerable<T> peers) {
         foreach(var peer in peers)
            Connect(peer);
      }
   }
}