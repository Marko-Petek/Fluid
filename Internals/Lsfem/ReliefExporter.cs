using System.Linq;
using dbl = System.Double;
using Fluid.Internals.Numerics;
using Supercluster.KDTree;
using static System.Linq.Enumerable;

namespace Fluid.Internals.Lsfem {
/// <summary>Takes a rectangle, the resolution specification on it and returns plot data suitable for Mathematica's ReliefPlot.</summary>
public class ReliefExporter<σ>
where σ : Sim {
   /// <summary>Lower left.</summary>
   Vec2 LL { get; }
   /// <summary>Upper right.</summary>
   Vec2 UR { get; }
   int NW;
   int NH;
   /// <summary>PixelWidth.</summary>
   dbl PW { get; }
   /// <summary>PixelHeight.</summary>
   dbl PH { get; }
   Vec2[] Pos => SimManager<σ>.I.Sim.Pos;
   KDTree<dbl,Element> EmtTree => SimManager<σ>.I.Sim.EmtTree;


   public ReliefExporter(Vec2 ll, Vec2 ur, dbl pw) {
      LL = ll;
      UR = ur;
      dbl wdth = ur.X - ll.X;
      dbl hght = ur.Y - ll.Y;
      int NW = (int) (wdth / pw);                     // Width multiplier.
      int NH = (int) (hght / pw);                     // Heigth multiplier.
      PW = wdth / NW;                                 // Adjusted pixel width.
      PH = hght / NH;                                 // Adjusted pixel height;
   }


   public dbl[][] ExportReliefData() {
      var posTable = Range(0, NW).Select( i =>                                           // Create an Enumerable of positions.
         Range(0, NH).Select( j => new dbl[2] {LL.X + i*PW, LL.Y + j*PH}));
      foreach(var col in posTable) {
         foreach(var pos in col) {
            var emtPairs = EmtTree.NearestNeighbors(pos, 4);
            foreach(var emtPair in emtPairs) {
               var emt = emtPair.Item2;
            }
         }
         
      }
   }
}
}