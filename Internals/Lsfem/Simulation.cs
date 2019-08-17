using System;
using System.Collections.Generic;

using Fluid.Internals.Numerics;
using My = Fluid.Internals.Collections.Custom;

namespace Fluid.Internals.Lsfem {
   public class Simulation {
      protected Mesh Mesh { get; }
      /// <summary>Lists of nodes lying inside a block.</summary>
      protected Dictionary<string,My.List<Vec2>> Patches { get; set; }
      /// <summary>Lists of nodes shared across blocks.</summary>
      protected Dictionary<string,My.List<Vec2>> Strips { get; set; }

      public Simulation() {

      }
   }
}