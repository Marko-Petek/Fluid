using System;
using System.Collections.Generic;

namespace Fluid.Internals.Collections
{
    public abstract class Node<T> where T : Node<T>
    {
        protected Dictionary<T,int> _Peers = new Dictionary<T,int>();
        public Dictionary<T,int> Peers => _Peers;
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