using System;
using System.Collections.Generic;

namespace Fluid.Internals.Networks {
/// <summary>A network is a dictionary of nodes, keyed by unique ID of type uint.</summary>
   public interface INetwork<τ>
   where τ : notnull {
      /// <summary>Internal dictionary of nodes.</summary>
      protected Dictionary<uint, τ> Nodes { get; }

      protected Dictionary<τ, uint> Keys { get; }

      protected Dictionary<uint, uint[]> CBonds { get; }

      protected Dictionary<uint, uint[]> PBonds { get; }

      /// <summary>Try to acquire existing node.</summary>
      /// <param name="key">Key whose node we are searching for.</param>
      (bool, τ) TryGet(uint key) {
         if(Nodes.TryGetValue(key, out var node))
            return (true, node);
         else
            return (false, default(τ));
      }

      (bool, uint) TryGet(τ node) {
         if(Keys.TryGetValue(node, out uint key))
            return (true, key);
         else
            return (false, 0);
      }

      τ Get(uint key) {
         if(Nodes.TryGetValue(key, out var node))
            return node;
         else
            throw new ArgumentException($"Node at key {key} not found.");
      }

      uint Get(τ node) {
         if(Keys.TryGetValue(node, out uint key))
            return key;
         else
            throw new ArgumentException($"Node {node} not found.");
      }

      /// <summary>Add new node or replace existing node.</summary>
      /// <param name="key">Key at which we are adding or replacing a node.</param>
      /// <param name="node">The node to add or replace.</param>
      void Set(uint key, τ node) {
         Nodes[key] = node;
         Keys[node] = key;
      }

      (bool, τ) TryRemove(uint key) {
         if(Nodes.Remove(key, out τ node)) {
            FreeKeys.Add(key);
            return (true, node); }
         else
            return (false, default);
      }

      (bool, uint) TryRemove(τ node) {
         if(Keys.Remove(node, out uint key)) {
            FreeKeys.Add(key);
            return (true, key); }
         else
            return (false, 0);
      }

      τ Remove(uint key) {
         if(Nodes.Remove(key, out τ node)) {
            FreeKeys.Add(key);
            return node; }
         else
            throw new ArgumentException($"Node at key {key} not found.");
      }

      uint Remove(τ node) {
         if(Keys.Remove(node, out uint key)) {
            FreeKeys.Add(key);
            return key; }
         else
            throw new ArgumentException($"Node {node} not found.");
      }

      protected SortedSet<uint> FreeKeys { get; }  // Must add first key upon construction! Has to always hold at least one key - that is the highest available key.

      protected int FreeKeyIteration { get; set; } // Init to 0!

      /// <summary>Purpose: to avoid having gaps in between keys. If a "gap key" is available it provides that one, otherwise it provides the next higher key.</summary>
      protected uint ProvideKey() {
         FreeKeyIteration++;
         if(FreeKeyIteration % 100 == 0) {      // An unbroken sequence of free keys can accumulate at the end. This is undesirable and we clear it every 100 iterations.
               uint last, beforeLast;
            do {
               last = FreeKeys.Max;
               FreeKeys.Remove(last);
               beforeLast = FreeKeys.Max;
            } while(last == beforeLast + 1);
            FreeKeys.Add(last); }                    // Sequence had a gap when iteration above ended, thus the loop went one iteration too far. We have to revert to a valid state.
         uint key = FreeKeys.Min;                     // Take the smallest key, we will return it at the end of this method.
         FreeKeys.Remove(key);
         if(FreeKeys.Count == 0)                   // If FreeKeys set is now empty, add a new key.
            FreeKeys.Add(key + 1);
         return key;
      }

      /// <summary>Assigns a unique identity to a node and adds it to the network. Returns its identity. If the specified node is already present, an exception is thrown.</summary>
      /// <param name="node">Node to add to network.</param>
      uint Add(τ node) {
         if(!Keys.TryGetValue(node, out uint key)) {           // Specified node not yet added.
            key = ProvideKey();
            Nodes.Add(key, node);
            Keys[node] = key;
            return key; }
         else
            throw new ArgumentException($"A node you tried to add is already in the network at key {key}.");
      }

      uint Set(τ node) {
         if(!Keys.TryGetValue(node, out uint key))           // Specified node not yet added.
            key = ProvideKey();
         Nodes[key] = node;
         Keys[node] = key;
         return key;
      }
   }
}