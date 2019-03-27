using System;
using System.Collections.Generic;

namespace Fluid.Internals.Collections
{
    public class Hierarchy<T>
    {
        public RankedNode TopNode { get; protected set; }

        public Hierarchy() : base() {}


        /// <summary>Make a specified node a top node and reorganize hierarchy around it while keeping all connections untouched.</summary><param name="topNode">Node to make top node.</param>
        public void MakeTopNode(RankedNode topNode) {

            if(topNode.Leader != null) {                        // Do nothing if specified node is already top node.
                topNode.Leader = null;
                Recursion(topNode);
            }

            void Recursion(RankedNode startNode) {
                foreach(var node in startNode.Peers.Keys) {
                    node.Leader = startNode;                    // Make all of its peers subordinates.

                    if(node.Peers.Count > 1)                  // Lowest node has only 1 peer - its leader.
                        Recursion(node);
        }   }   }
    }
}