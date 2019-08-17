using System;
using System.Collections.Generic;

using Fluid.Internals.Numerics;
using My = Fluid.Internals.Collections.Custom;

namespace Fluid.Internals.Lsfem {
   public abstract class Simulation {
      protected Mesh Mesh { get; }
      /// <summary>2D lists of nodes lying inside a block.</summary>
      protected Dictionary<string,My.List<My.List<Vec2>>> Patches { get; set; }
      /// <summary>1D lists of nodes shared across blocks.</summary>
      protected Dictionary<string,My.List<Vec2>> Strips { get; set; }
      /// <summary>Centers of mass of elements. Created right after elements are created.</summary>
      protected double[][] COMs { get; set; }
      /// <summary>Use Blocks to create positions for Patches and Strips in orderly fashion - so that each node is created only once. All nodes must be created and put on Patches and Strips first. Only then can we traverse Patches and Strips to create Elements: no new nodes should be created when creating elements. At the end create the COMs array.</summary>
      protected abstract void CreateNodesThenElements();
      /// <summary>Create nodes on Mesh based on patches and strips and apply boundary conditions at the same time. Lastly, create the ElementTree on Mesh by providing the COMs array and the internal array of Elements list on Mesh.</summary>
      protected abstract void TransferNodesToMesh();

      public Simulation() {

      }
   }
}