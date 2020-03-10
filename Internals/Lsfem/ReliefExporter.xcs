using System.Linq;
using dbl = System.Double;
using Fluid.Internals.Numerics;
using Supercluster.KDTree;
using static System.Linq.Enumerable;
using static Fluid.Internals.Lsfem.SimManager;
using static Fluid.Internals.Toolbox;

namespace Fluid.Internals.Lsfem {
/// <summary>Takes a rectangle, the resolution specification on it and returns plot data suitable for Mathematica's ReliefPlot.</summary>
public class ReliefExporter {
   /// <summary>Lower left.</summary>
   Vec2 LL { get; }
   /// <summary>Upper right.</summary>
   Vec2 UR { get; }
   /// <summary>Number of pixels width-wise.</summary>
   int NW;
   /// <summary>Number of pixels height-wise.</summary>
   int NH;
   /// <summary>PixelWidth.</summary>
   dbl PW { get; }
   /// <summary>PixelHeight.</summary>
   dbl PH { get; }
   
   //Vec2[] Pos => SM.Sim.Pos;
   KDTree<dbl,Element> EmtTree => SM.Sim.EmtTree;


   public ReliefExporter(Vec2 ll, Vec2 ur, dbl pw) {
      LL = ll;
      UR = ur;
      dbl wdth = ur.X - ll.X;
      dbl hght = ur.Y - ll.Y;
      NW = (int) (wdth / pw);                     // Width multiplier.
      NH = (int) (hght / pw);                     // Heigth multiplier.
      PW = wdth / NW;                                 // Adjusted pixel width.
      PH = hght / NH;                                 // Adjusted pixel height;
   }

   /// <summary>Create the data ready to be plotted in Mathematica's ReliefPlot.</summary>
   dbl[][][] CreateReliefData() {                                               // [variable index][y index][val at x] // TODO: Test ExportReliefData.
      var posTable = Range(0, NH + 1).Select( i =>                                     // Create an Enumerable of positions.
         Range(0, NW + 1).Select( j => new dbl[2] {LL.X + j*PW, LL.Y + i*PH}));        // This does not take bulk memory.
      var valArrays = Range(0, SM.Sim.NVar).Select( i =>
         Range(0, NH + 1).Select( j => new dbl[NW + 1] ).ToArray()).ToArray();         // Make #NVar value arrays.
      for(int i = 0; i < NH + 1; ++i) {                                                   // Enumerate the positions.   var col in posTable
         for(int j = 0; j < NW + 1; ++j) {                                                                      // var pos in col
            var pos = new dbl[] {LL.X + j*PW, LL.Y + i*PH};
            var emtPairs = EmtTree.NearestNeighbors( pos, 4 );
            foreach(var emtPair in emtPairs) {                                         // emtPair = (CoM, Emt)
               var emt = emtPair.Item2;
               var vals = emt.ValueAt(pos.ToVec2());                                   // Attempt to construct the value through base functions.
               if(vals != null) {                                                      // Location inside element. Values constructed.
                  for(int k = 0; k < SM.Sim.NVar; ++k)
                     valArrays[k][i][j] = vals[k]; } } } }
      return valArrays;
   }
   /// <summary>Export each variable into a separate file.</summary>
   /// <param name="relDirPath">Directory path relative to the Fluid folder.</param>
   /// <param name="fileNameNoExt">File name without the extension. Files will be numbered automatically.</param>
   public void ExportReliefData(string relDirPath, string fileNameNoExt) {
      var reliefData = CreateReliefData();
      for(int i = 0; i < SM.Sim.NVar; ++i) {                                        R("Starting to write Relief data to file.");
         T.FileWriter.SetDirAndFile(relDirPath, $"{fileNameNoExt}{i}", ".txt");     R($"Finished writing file {i+1}/{SM.Sim.NVar}");
         T.FileWriter.Write(reliefData[i]); }
   }
}
}