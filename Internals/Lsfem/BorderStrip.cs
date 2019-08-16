/*
  ██████╗ ██████╗ ███████╗██████╗  █████╗ ████████╗██╗ ██████╗ ███╗   ██╗
 ██╔═══██╗██╔══██╗██╔════╝██╔══██╗██╔══██╗╚══██╔══╝██║██╔═══██╗████╗  ██║
 ██║   ██║██████╔╝█████╗  ██████╔╝███████║   ██║   ██║██║   ██║██╔██╗ ██║
 ██║   ██║██╔═══╝ ██╔══╝  ██╔══██╗██╔══██║   ██║   ██║██║   ██║██║╚██╗██║
 ╚██████╔╝██║     ███████╗██║  ██║██║  ██║   ██║   ██║╚██████╔╝██║ ╚████║
  ╚═════╝ ╚═╝     ╚══════╝╚═╝  ╚═╝╚═╝  ╚═╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝
                                                                         
We first create all Blocks, so that the blocks create their elements and save them inside the global element list on BlockMesh. Each block creates a mapping from a 2D grid to the global element list, so that we can easily refer to elements from the blocks. When the blocks are creating their own elements, they omit the creation of bordering elements that contain nodes shared by two blocks.

After that, the BorderStrips are created and they create the remaining nodes that border on multiple blocks. The strips also fill the empty spots in the mapping.
*/

using System;
using System.Collections.Generic;

namespace Fluid.Internals.Lsfem {
   public class BorderStrip {
      /// <summary>Block pairs neighbouring on each other.</summary>
      protected List<Block[]> BlockPairs { get; }
      /// <summary>Actions that take their corresponding block pairs as arguments.</summary>
      protected List<Action<Block[]>> CreateNodes { get; }
      public BorderStrip() {

      }
   }
}