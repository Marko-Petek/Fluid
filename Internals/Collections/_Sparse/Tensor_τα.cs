using System;
using System.Text;
using System.Linq;
using SCG = System.Collections.Generic;
using TB = Fluid.Internals.Toolbox;

using Fluid.Internals.Numerics;

namespace Fluid.Internals.Collections {
   /// <summary>A tensor with specified rank and specified dimension which holds direct subordinates of type τ.</summary>
   /// <typeparam name="τ">Type of direct subordinates.</typeparam>
   public class Tensor<τ,α> : TensorBase<Tensor<τ,α>>,
      IEquatable<Tensor<τ,α>>                               // So we can check two tensors for equality.
      where τ : IEquatable<τ>, IComparable<τ>, new()
      where α : IArithmetic<τ>, new() {
         /// <summary>Contains methods that perform arithmetic.</summary>
         static α Arith { get; } = new α();
         /// <summary>Hierarchy's dimensional structure. E.g.: {3,2,6,5} specifies structure of a tensor of 4th rank with first rank equal to 5 and last rank to 3.</summary>
         public int[] Structure { get; protected set; }
         /// <summary>Rank specifies the height (level) in the hierarchy on which the tensor sits. It equals the number of levels that exist below it. It tells us how many indices we must specify before we reach the value level.</summary>
         public int Rank { get; protected set; }
         /// <summary>Superior: a tensor directly above in the hierarchy. Null if this is the highest rank tensor.</summary>
         public Tensor<τ,α> Sup { get; protected set; }
         ///// <summary>Dummy tensor used by indexer's getter when fetching a non-existent tensor below. When a setter of dummy tensor attempts to add a subordinate, dummy is copied as a real element to its superior.</summary>
         //protected TensorDum<τ,α> Dummy { get; }
         ///// <summary>Creates a tensor with specified rank, with dimension and initial capacity 1.</summary>
         //public Tensor(int rank) : this(rank, 1, 1) {
         //   Dummy = new TensorDum<τ,α>(this);
         //}
         protected Tensor(int cap) : base(cap) { }

         protected Tensor(int[] structure, int rank, Tensor<τ,α> sup, int cap) : base(cap) {
            Structure = structure ?? null;
            Rank = rank;
            Sup = sup ?? null;
         }
         /// <summary>Creates a top tensor with specified structure and initial capacity. Rank is assigned as the length of structure array.</summary>
         /// <param name="structure">Specifies dimension of each rank.</param>
         /// <param name="cap">Initially assigned memory.</param>
         public Tensor(int[] structure) : this(structure, structure.Length, null, 6) { }
         /// <summary>Creates a non-top tensor with specified superior and initial capacity. Rank is assigned as one less that superior.</summary>
         /// <param name="sup">Tensor directly above in hierarchy.</param>
         /// <param name="cap">Initially assigned memory.</param>
         public Tensor(Tensor<τ,α> sup, int cap) : this(sup.Structure, sup.Rank - 1, sup, cap) { }
         // TODO: Make a copy method that creates a different structure and assigns another superior?
         public Tensor(Tensor<τ,α> src) : base(src.Count) {
            Copy(src, this);
         }
         /// <summary>Creates a tensor as a copy of this one: same Rank, Structure and Superior.</summary>
         public virtual Tensor<τ,α> Copy() {
            var res = new Tensor<τ,α>(Count);
            Copy(this, res);
            return res;
         }
         /// <summary>You have to provide the already instantiated target.</summary>
         /// <param name="src">Copy source.</param>
         /// <param name="tgt">Copy target.</param>
         public static void Copy(Tensor<τ,α> src, Tensor<τ,α> tgt) {
            TB.Assert.True(src.Rank > 1,
               "Tensors's rank has to be at least 2 to be copied.");
            tgt.Structure = src.Structure;
            tgt.Rank = src.Rank;
            tgt.Sup = src.Sup ?? null;
            Recursion(src, tgt);

            void Recursion(Tensor<τ,α> src1, Tensor<τ,α> tgt1) {
               if(src1.Rank > 2) {                                       // Subordinates are tensors.
                  foreach (var kv in src1) {
                     var tnr = new Tensor<τ,α>(kv.Value.Structure, kv.Value.Rank, kv.Value.Sup, kv.Value.Count);
                     Recursion(kv.Value, tnr);
                     tgt1.Add(kv.Key, tnr); } }
               else if(src1.Rank == 2) {                                 // Subordinates are vectors.
                  foreach (var kv in src1)
                     tgt1.Add(kv.Key, new Vector<τ,α>((Vector<τ,α>) kv.Value)); }
               else
                  throw new InvalidOperationException(      //TODO: Fix this?
                     "Tensors's rank has to be at least 2 to be copied via this method.");
            }
         }
         ///// <summary>Factory method that creates a vector as a copy of another.</summary>
         ///// <param name="src">Vector to copy.</param>
         //public static Tensor<τ,α> Create(Tensor<τ,α> src) => new Tensor<τ,α>(src);

         ///// <summary>New indexer definition that hides Dictionary's indexer. Returns dummy tensor for non-existing elements.</summary>
         //public new Tensor<τ,α> this[int i] {                        // We know this is not a vector and its elements are tensors.
         //   get {
         //      if(TryGetValue(i, out Tensor<τ,α> val))               // Outputs null if value not found.
         //         return val; 
         //      else
         //         return new TensorDum<τ,α>(this, i, 6); }              // this as its Superior
         //   set {
         //      if(!value.Equals(null)) {                             // Right hand side non-null.
         //         base[i] = value;                                   // Indexer adds or modifies if entry already exists.
         //         if(this is TensorDum<τ,α> dum) {                   // Try downcasting.
         //            do {
         //               var tnr = new Tensor<τ,α>(dum);                  
         //               dum.Sup.Add(dum.Inx, tnr);                   // Add new tensor to its superior.
         //               dum = dum.Sup as TensorDum<τ,α>; }           // Try downcasting previous dummy's superior.
         //            while (dum != null); } }                        // Cast succeeded, repeat.
         //      else if(!(this is TensorDum<τ,α>))
         //         Remove(i);                                         // Remove value at given index if value set is null and we are not in dummy.
         //   }
         //}// TODO: Rather implement this as a method.
         //public τ this[int i, uint j] {                      // We know this is a Vector and its elements are values. j serves only to identify overload.
         //   get {
         //      var thisVec = (Vector<τ,α>) this;
         //      thisVec.Vals.TryGetValue(i, out τ val);          // Outputs zero if value not found.
         //      return val; }
         //   set {
         //      if(!value.Equals(default(τ))) {
         //         var thisVec = (Vector<τ,α>) this;
         //         thisVec.Vals[i] = value;
         //         if(this is TensorDum<τ,α> dum) {                   // Dummy vector.
         //            var newVec = new Vector<τ,α>(thisVec);
         //            newVec.Sup.Add(dum.Inx, newVec);
         //            if(newVec.Sup is TensorDum<τ,α> dum1) {                   // Try downcasting.
         //            do {
         //               var tnr = new Tensor<τ,α>(dum1);                  
         //               dum1.Sup.Add(dum1.Inx, tnr);                   // Add new tensor to its superior.
         //               dum1 = dum1.Sup as TensorDum<τ,α>; }           // Try downcasting previous dummy's superior.
         //            while (dum1 != null); } }                        // Cast succeeded, repeat.
         //      }
         //      else if(!(this is TensorDum<τ,α>)) {            // Remove value at given index if value set is 0 and we are not in a Dummy.
         //         var thisVec = (Vector<τ,α>) this;
         //         thisVec.Vals.Remove(i); }
         //   }
         //}
         public virtual Tensor<τ,α> this[uint overloadDummy, params int[] inx] { //TODO: Override on vector.
            get {
               Tensor<τ,α> tnr = this;
               for(int i = 0; i < inx.Length; ++i) {
                  if(!tnr.TryGetValue(inx[i], out tnr))
                     return null; }
               return tnr; }                                // No problem with tnr being null. We return above.
            set {
               Tensor<τ,α> tnr = this;
               if(value != null) {
                  int n = inx.Length - 1;
                  for(int i = 0; i < n; ++i) {
                     if(!tnr.TryGetValue(inx[i], out tnr))
                        tnr = new Tensor<τ,α>(Structure, tnr.Rank - 1, tnr, 6);
                        tnr.Sup.Add(inx[i], tnr); }
                  var dict = (TensorBase<Tensor<τ,α>>) tnr;
                  dict[inx[n]] = value; }
               else {
                  for(int i = 0; i < inx.Length; ++i) {
                     if(!tnr.TryGetValue(inx[i], out tnr))
                        return; }
                  tnr.Sup.Remove(inx[inx.Length - 1]); } }
         }
         public virtual Vector<τ,α> this[short overloadDummy, params int[] inx] { //TODO: Override on vector.
            get {
               Tensor<τ,α> tnr = this;
               int n = inx.Length - 1;
               for(int i = 0; i < n; ++i) {
                  if(!tnr.TryGetValue(inx[i], out tnr))
                     return null; }
               if(tnr.TryGetValue(inx[n], out tnr))         // No probelm with null.
                  return (Vector<τ,α>)tnr;                  // Same.
               else
                  return null; }
            set {
               Tensor<τ,α> tnr = this;
               Tensor<τ,α> tnr2;                // Temporary to avoid null problem below.
               if(value != null) {
                  int n = inx.Length - 1;                      // One before last chooses tensor, last chooses vector.
                  for(int i = 0; i < n; ++i) {
                     if(tnr.TryGetValue(inx[i], out tnr2)) {
                        tnr = tnr2; }
                     else {
                        tnr = new Tensor<τ,α>(Structure, tnr.Rank - 1, tnr, 6);
                        tnr.Sup.Add(inx[i], tnr); }}
                  var dict = (TensorBase<Tensor<τ,α>>) tnr;                      // We do not check that it is a vector beforehand. It is assumed that the user used indexer correctly.
                  dict[inx[n]] = value; }
               else {
                  int n = inx.Length;                 // Last chooses vector.
                  for(int i = 0; i < n; ++i) {
                     if(!tnr.TryGetValue(inx[i], out tnr))
                        return; }
                  tnr.Sup.Remove(inx[n - 1]); } }     // Vector.Superior.Remove
         }
         public virtual τ this[params int[] inx] {
            get {
               Tensor<τ,α> tnr = this;
               int n = inx.Length - 2;
               for(int i = 0; i < n; ++i) {
                  if(!tnr.TryGetValue(inx[i], out tnr))
                     return default(τ); }
               if(tnr.TryGetValue(inx[n], out tnr)) {         // No probelm with null.
                  var vec = (Vector<τ,α>)tnr;                  // Same.
                  vec.Vals.TryGetValue(inx[n + 1], out τ val);
                  return val; }
               else
                  return default(τ); }
            set {
               Tensor<τ,α> tnr = this;
               Tensor<τ,α> tnr2;                               // Temporary to avoid null problem below.
               Vector<τ,α> vec;
               if(!value.Equals(default(τ))) {
                  if(inx.Length > 1) {                         // At least a 2nd rank tensor.
                     int n = inx.Length - 2;                      //TODO: Override on vector.
                     for(int i = 0; i < n; ++i) {                    // This loop is entered only for a 3rd rank tensor or above.
                        if(tnr.TryGetValue(inx[i], out tnr2)) {
                           tnr = tnr2; }
                        else {
                           tnr = new Tensor<τ,α>(Structure, tnr.Rank - 1, tnr, 6);
                           tnr.Sup.Add(inx[i], tnr); }}
                     if(tnr.TryGetValue(inx[n], out tnr2)) {                  // Does vector exist?
                        vec = (Vector<τ,α>) tnr2; }
                     else {
                        vec = new Vector<τ,α>()} }    // TODO: Create a detailed constructor on Vector.
                  var dict = (TensorBase<Tensor<τ,α>>) tnr;    // tnr is now vector.
                  dict[inx[n + 1]] = value; }
               else {
                  int n = inx.Length;
                  for(int i = 0; i < n; ++i) {
                     if(!tnr.TryGetValue(inx[i], out tnr))
                        return; }
                  tnr.Sup.Remove(inx[n - 1]); } }
         }
         /// <summary>Append specified vector to caller.</summary>
         /// <param name="appTnr">Vector to append.</param>
         public void MergeWith(Tensor<τ> appTnr) {
            Dim += appTnr.Dim;                                      // Readjust width.
            foreach(var kvPair in appTnr)
               this[kvPair.Key] = kvPair.Value;
         }
         /// <summary>Swap two R1 elements specified by indices.</summary>
         /// <param name="inx1">First element index.</param>
         /// <param name="inx2">Second element index.</param>
         public void Swap(int inx1, int inx2) {
            bool firstExists = TryGetValue(inx1, out τ val1);
            bool secondExists = TryGetValue(inx2, out τ val2);
            if(firstExists) {
               if(secondExists) {
                  base[inx1] = val2;
                  base[inx2] = val1; }
               else {
                  Remove(inx1);                                   // Element at inx1 becomes 0 and is removed.
                  Add(inx2, val1); } }
            else if(secondExists) {
               Add(inx1, val2);
               Remove(inx2); }                                   // Else nothing happens, both are 0.
         }
         /// <summary>Apply element swaps as specified by a swap vector.</summary>
         /// <param name="swapVec">Vector where an element at index i with integer value j instructs a permutation i->j.</param>
         public void ApplySwaps(Tensor<int> swapVec) {
            foreach(var kVPair in swapVec)
               Swap(kVPair.Key, kVPair.Value);   
         }
         /// <summary>Create a string of all non-zero elements in form {{key1, val1}, {key2, val2}, ..., {keyN,valN}}.</summary>
         public override string ToString() {
            StringBuilder sb = new StringBuilder(72);
            sb.Append("{");
            foreach(var elm in this.OrderBy( kvPair => kvPair.Key ))
               sb.Append($"{{{elm.Key.ToString()}, {elm.Value.ToString()}}}, ");
            int length = sb.Length;
            sb.Remove(length - 2, 2);
            sb.Append("}");
            return sb.ToString();
         }
   }
}