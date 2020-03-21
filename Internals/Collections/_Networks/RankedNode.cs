using System;
using SC = System.Collections;
using SCG = System.Collections.Generic;

namespace Fluid.Internals.Collections {
   public class RankedNode : ONode<RankedNode> {
      public RankedNode? Leader { get; set;}
      public SubordinateCollection Subordinates { get; }

      public RankedNode() : base() {
         Subordinates = new SubordinateCollection(this);
      }
      public RankedNode(RankedNode leader) : this() {
         Leader = leader;
         Connect(leader);
      }
      public RankedNode(SCG.IEnumerable<RankedNode> peers, RankedNode leader) : this() {
         Leader = leader;
         Connect(peers);
      }

      public class SubordinateCollection : SCG.IEnumerable<RankedNode> {
         RankedNode Node { get; }
         public int Count => (Node.Leader != null) ? (Node.Peers.Count - 1) : Node.Peers.Count;

         public SubordinateCollection(RankedNode node) {
               Node = node;
         }

         public SCG.IEnumerator<RankedNode> GetEnumerator() {
            foreach(var rankedNode in Node.Peers.Keys)
               if(rankedNode != Node?.Leader)
                  yield return rankedNode;
         }
         SC.IEnumerator SC.IEnumerable.GetEnumerator() => GetEnumerator();
      }
   }
}