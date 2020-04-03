using System;
using SCG = System.Collections.Generic;
using static Fluid.Internals.Numerics.MatOps;
namespace Fluid.Internals.Networks.Custom {

public abstract class ManagedList<T> : List<T> 
where T : struct {
   /// <summary>Create ManagedList with default initial capacity of internal array.</summary>
   public ManagedList() : base() { }
   /// <summary>Create ManagedList with specified initial capacity of internal array.</summary><param name="capacity">Initial capacity of internal array.</param>
   public ManagedList(int capacity) : base(capacity) { }
   /// <summary>Create a copy of ManagedList.</summary><param name="sourceList">Source ManagedList to copy.</param>
   public ManagedList(ManagedList<T> sourceList) : base(sourceList) {
   }

   /// <summary>Assign any properties of the element that has just entered an internal array of the List at the specified index via Add or Insert.</summary>
   protected abstract void AfterEmtEntry(int index);
   /// <summary>Assign any properties of the element that is about to leave an internal array of the List at the specified index via Remove, RemoveAt or Clear.</summary>
   protected abstract void BeforeEmtExit(int index);
   public override T this[int index] {
      get {
         if (index > -1) {
            if (index < Count)
               return _E[index];
            else
               throw new IndexOutOfRangeException("Index too large."); }
         else
            throw new IndexOutOfRangeException("Negative index."); }
      set {
         if (index > -1) {
            if (index < Count) {
               if (!Comparer.Equals(_E[index], default))     // If there is a pre-existing element at the index in question.
                  BeforeEmtExit(index);
               _E[index] = value;
               if (!Comparer.Equals(value, default))                 // If the newly assigned value is not null.
                  AfterEmtEntry(index); }
            else
               throw new IndexOutOfRangeException("Index too big."); }
         else
            throw new IndexOutOfRangeException("Negative index."); }
   }
   /// <summary>Adds an element to the end of the list.</summary>
   public override void Add(T emt) {
      _E = _E.EnsureCapacity(Count + 1);
      _E[Count] = emt;
      Count++;
      if (!Comparer.Equals(emt, default))                 // If the newly assigned value is not null.
         AfterEmtEntry(Count - 1);
   }
   public override void AddRange(SCG.IList<T> elements) {
      _E = _E.EnsureCapacity(Count + elements.Count);
      for (int i = 0; i < elements.Count; i++) {
         _E[Count + i] = elements[i];
         if (!Comparer.Equals(elements[i], default))                 // If the newly assigned value is not null.
            AfterEmtEntry(Count + i); }
      Count += elements.Count;
   }
   /// <summary>Insert an element at a desired index.</summary>
   public override void Insert(int inx, T emt) {
      if (inx < 0) throw new IndexOutOfRangeException("Negative index.");
      else if (inx <= Count) {
         _E = _E.EnsureCapacity(Count + 1);
         for (int i = Count; i > inx; i--)                // Shift all consequent members by one. Makes room for a member.
            _E[i] = _E[i - 1];
         _E[inx] = emt;
         Count++;
         if (!Comparer.Equals(emt, default))                 // If the newly assigned value is not null.
            AfterEmtEntry(inx); }
      else
         throw new IndexOutOfRangeException("Index too large.");
   }
   /// <summary>Searches for and removes the specified element from this Subsequence. Returns true if successfully removed.</summary>
   public override bool Remove(T emt) {
      for (int i = 0; i < Count; i++)
         if (Comparer.Equals(_E[i], emt)) {                   // If the element has been found.
            BeforeEmtExit(i);
            for (int j = i; j < Count - 1; j++)
               _E[j] = _E[j + 1];          // Shift elements, writing over the removed element.
            _E[Count - 1] = default;         // Reset the last Instruction.
            Count--;                                // Adjust the Count.
            return true; }
      return false;
   }
   /// <summary>Remove an element at the specified index.</summary>
   public override void RemoveAt(int inx) {
      if (inx < 0)
         throw new IndexOutOfRangeException("Negative index.");
      else if (inx < Count) {
         if (!Comparer.Equals(_E[inx], default))     // If there is a pre-existing element at the index in question.
            BeforeEmtExit(inx);
         for (int i = inx; i < Count - 1; i++)
            _E[i] = _E[i + 1];
         _E[Count - 1] = default;
         Count--; }
      else
         throw new IndexOutOfRangeException("Index too large.");
   }
   /// <summary>Clears the internal array without changing its capacity.</summary>
   public override void Clear() {
      for (int i = 0; i < Count; i++) {
         if (!Comparer.Equals(_E[i], default))     // If there is a pre-existing element at the index in question.
            BeforeEmtExit(i);
         _E[i] = default; }
      Count = 0;
   }
}
}