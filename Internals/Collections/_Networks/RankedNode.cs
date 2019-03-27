using System;
using SC = System.Collections;
using SCG = System.Collections.Generic;

namespace Fluid.Internals.Collections
{
    public class RankedNode : Node<RankedNode>
    {
        public RankedNode Leader { get; set;}
        public SubordinateCollection Subordinates { get; }

        public RankedNode(SCG.IEnumerable<RankedNode> peers, RankedNode leader) : base(peers) {
            Leader = leader;
            Subordinates = new SubordinateCollection(this);
        }


        public class SubordinateCollection : SCG.IEnumerable<RankedNode> {
            RankedNode Node { get; }

            public SubordinateCollection(RankedNode leader) {
                Node = leader;
            }

            public SCG.IEnumerator<RankedNode> GetEnumerator() {
                foreach(var rankedNode in Node.Peers.Keys)
                    if(rankedNode != Node)
                        yield return rankedNode;
            }

            SC.IEnumerator SC.IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}