using System;
using System.Reflection;
using Fluid.Internals;

namespace Fluid.Internals.Networks {

public class Hierarchy<τ>
where τ : IEquatable<τ>, new() {
   public RankedNode TopNode { get; protected set; }

   public Hierarchy(RankedNode topNode) {
      TopNode = topNode;
   }

   /// <summary>Make a specified node a top node and reorganize hierarchy around it while keeping all connections untouched.</summary><param name="topNode">Node to make top node.</param>
   public void MakeTopNode(RankedNode topNode) {
      TopNode = topNode;
      if(topNode.Leader != null) {                        // Do nothing if specified node is already top node.
         topNode.Leader = null;
         Recursion(topNode); }

      void Recursion(RankedNode startNode) {
         foreach(var node in startNode.Peers.Keys) {
            if(node != startNode?.Leader) {                 // Must leave its leader alone.
               node.Leader = startNode;                    // Make all of its peers subordinates.

            if(node.Peers.Count > 1)                  // Lowest node has only 1 peer - its leader.
               Recursion(node); } }
      }
   }
   /// <summary>Tries to convert hierarchy to jagged array. Returns null when not able to do so.</summary>
   public Array? ConvertToArray() {
      int valDepth = 0;
      bool valDepthNotYetReached = true;
      // Fail: When value node is reached at depth above value depth.
      // Fail: When non value nodes coexist with value nodes.
      // Fail: When value node is reached at any other depth than any previous value node.
      return Recursion(TopNode, 0);

      Array? Recursion(RankedNode? startNode, int currDepth) {
         if(startNode == null)
            throw new ArgumentNullException("StartNode Cannot be null.");
         else if(startNode.Subordinates.Count != 0) {                   // Explore further.
            ++currDepth;                                            // Set currDepth for foreach loop.
            Array? nextDepthArray = null;
            Array? currDepthArray = null;
            int i = 0;
            foreach(var subNode in startNode.Subordinates) {
               if(subNode is ValueNode<τ> valNode) {              // Value node.
                  if(currDepth != valDepth) {                       // Current depth not value depth.
                     if(valDepthNotYetReached) {                     // We reached value depth for the first time.                                 
                        valDepthNotYetReached = false;
                        valDepth = currDepth; }
                     else
                        return null; }                          // Hierarchy can not be converted.
                  if(currDepthArray == null)                                                        // Create array if it does not yet exist.
                     currDepthArray = Array.CreateInstance(typeof(τ), startNode.Subordinates.Count);
                  currDepthArray.SetValue(valNode.Value, i++); }                                       // Create entry here.
               else {                                          // Non-value node.
                  if(currDepth == valDepth)                   // At value depth.
                     return null;                          // Hierarchy can not be converted.
                  else {                                     // Call recursion again.
                     nextDepthArray = Recursion(subNode, currDepth);
                     if(nextDepthArray == null)         // Did not return a valid sub-array.    
                        return null;                        // Propagate failure.
                     else {                                  // Branch returned a valid sub-array.
                        if(currDepthArray == null)
                           currDepthArray = Array.CreateInstance(nextDepthArray.GetType(),
                              startNode.Subordinates.Count);
                        currDepthArray.SetValue(nextDepthArray, i++); } } } }
            return currDepthArray; }
         else                                               // No subordinates of non-value node.
            return null;                                   // Can not convert.
      }
   }
}
}