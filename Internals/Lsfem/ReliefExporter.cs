using dbl = System.Double;
using Fluid.Internals.Numerics;
using Supercluster.KDTree;
using SM = Fluid.Internals.Lsfem.SimManager;

namespace Fluid.Internals.Lsfem {
   /// <summary>Takes a rectangle, the resolution specification on it and returns plot data suitable for Mathematica's ReliefPlot.</summary>
   public class ReliefExporter {
      /// <summary>Rectangle width.</summary>
      dbl W { get; }
      /// <summary>Rectangle height.</summary>
      dbl H { get; }
      /// <summary>PixelWidth.</summary>
      dbl PW { get; }
      Vec2[] Pos => SimManager.I


      public ReliefExporter(dbl w, dbl h, dbl pw) {
         W = w;
         H = h;
         PW = pw;
      }



   }
}