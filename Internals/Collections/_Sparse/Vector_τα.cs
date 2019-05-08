using System;
using System.Collections.Generic;

namespace Fluid.Internals.Collections {
   public class Vector<τ,α> : Tensor<τ,α> {
      protected Dictionary<int,τ> Emts { get; }

      public Vector(int cap = 6) : base(1) { }
      public Vector(int dim, int cap = 6) : base(1, dim, cap) { }

      public Vector(Vector<τ,α> src) : this(src.Dim, src.Emts.Count) {
         Emts = new Dictionary<int, τ>(src.Emts);
      }

      /// <summary>New indexer definition that hides Dictionary's indexer. Returns 0 for non-existing elements.</summary>
      public new τ this[int i] {
         get {
            Emts.TryGetValue(i, out τ val);                                  // Outputs zero if value not found.
            return val; }
         set {
            if(!value.Equals(default(τ))) {                             // Value different from 0.
               if(this is DumVector<τ,α> dumVec) {                      // Try downcasting to a dummy vector.
                  var newVec = new Vector<τ,α>(Dim);                    
                  newVec.Emts.Add(i, value);                                // Add value the setter accepted to new vector.
                  dumVec.Sup.Add(dumVec.Index, newVec); }               // Add new vector to its rank 2 tensor owner.
               else
                  Emts[i] = value; }                                  // Indexer adds or modifies if entry already exists.
            else if(!(this is DumVector<τ,α>))
               Remove(i);
         }                                           // Remove value at given index if value set is 0 and we are not in DummyVector.
      }
   }
}
