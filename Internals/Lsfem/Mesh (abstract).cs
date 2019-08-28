using System;
using System.Collections.Generic;

using Fluid.Internals.Collections;
using My = Fluid.Internals.Collections.Custom;
using Fluid.Internals.Numerics;

using Supercluster.KDTree;

namespace Fluid.Internals.Lsfem {
   using dbl = Double;
   using dA = DblArithmetic;
   using Vec = Vector<double,DblArithmetic>;
   using Tnr = Tensor<double, DblArithmetic>;
   /// <summary>A mesh made of structured blocks which consist of quadrilateral elements.</summary>
   public class Mesh {
      public static Mesh Msh { get; protected set; }
      /// <summary>A list of positions.   (global index) => (x,y)</summary>
      public My.List<Vec2> Pos { get; internal set; }
      /// <summary>Contains node indices that belong to an element. (element index, nodes)</summary>
      protected KDTree<double,Element> ElementTree { get; }
      

      /// <summary>Create a block-structured mesh.</summary>
      public Mesh() {
         Msh = this;
      }
   }
}