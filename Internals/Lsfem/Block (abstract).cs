using System;
using System.Collections.Generic;
using static System.Math;

using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;
using My = Fluid.Internals.Collections.Custom;

namespace Fluid.Internals.Lsfem {
   using dbl = Double;
   using Tensor = Tensor<double,DblArithmetic>;
   using Vector = Vector<double,DblArithmetic>;
   /// <summary>Represents a method that takes three indices and returns a position by reference.</summary>
   //public delegate ref MeshNode NodeDelegate(int blockRow, int blockCol, int index);

   

   /// <summary>Used by Simulation to generate node positions when forming elements.</summary>
   public abstract class Block {
      /// <summary>Number of rows of elements.</summary>
      public int NRows { get; protected set; }
      /// <summary>Number of columns of elements.</summary>
      public int NCols { get; protected set; }
      

      /// <summary>Create a block.</summary>
      public Block() { }
   }
}