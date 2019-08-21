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
      /// <summary>Create a block.</summary>
      public Block() { }


      /// <summary>Create a position inside the block that corresponds to an intersection of two (curvilinear) coordinate lines: a "vertical" one going through the bottom edge at width-wise parameter value tW and a "horizontal" one going through the left edge at height-wise parameter value tH</summary>
      /// <param name="tW">Width-wise parameter value.</param>
      /// <param name="tH">Height-wise parameter value.</param>
      public abstract Vec2 CreatePos(double tW, double tH);
      

      
   }
}