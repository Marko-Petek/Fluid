namespace Fluid.Internals.Mesh {
   /// <summary>Contains value and a flag indicating whether a NodeValue is free or constrained.</summary><remarks>This is a value type. When assigning, always create a new one.</remarks>
   public struct Variable {   
      /// <summary>Value at node.</summary>
      public double Val;
      /// <summary>True if node is constrained.</summary>
      public bool Constrained;

      public Variable(double val, bool constrained) {
         Val = val;
         Constrained = constrained;
      }

      public override string ToString() => $"{{{Val.ToString()}, {Constrained.ToString()}}}";
   }
}