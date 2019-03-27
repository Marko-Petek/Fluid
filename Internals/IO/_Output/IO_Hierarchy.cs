using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using static System.Char;

using TB = Fluid.Internals.Toolbox;                  // For Toolbox.
using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;
using static Fluid.Internals.Operations;

namespace Fluid.Internals.IO {
    public static partial class IO {
        public static void Write<T>(this Hierarchy<T> hier, TextWriter tw) {
            if(hier.TopNode != null) {                        // Do nothing if specified node is already top node.
                var topNode = hier.TopNode;
                // if comes here ... if top node is already a value.
                Recursion(topNode);
            }

            void Recursion(RankedNode startNode) {
                int nSubordinates;
                int i = 0;

                if(startNode.Leader != null)
                    nSubordinates = startNode.Peers.Count - 1;              // We need the count to know when we are about to reach the last member.
                else
                    nSubordinates = startNode.Peers.Count;
                tw.Write('{');

                foreach(var node in startNode.Subordinates) {
                     
                    if(node is ValueNode<T> vNode)                                  // This is a value node.
                        tw.Write(((ValueNode<T>)node).Value.ToString());            // Write its value.
                    else                                                            // This is not a value node.
                        Recursion(node);                                            // Re-enter recursion.                            

                    if(++i < nSubordinates)                                     // We have more than one node left in this group.
                        tw.Write(", ");                                             // So add a comma and space.
                }
                tw.Write('}');
        }   }

        public static void WriteLine<T>(this Hierarchy<T> hier, TextWriter tw) {
            Write(hier, tw);
            tw.WriteLine();
        }
    }
}