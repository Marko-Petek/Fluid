using System;
using System.IO;
using System.Collections.Generic;

using Fluid.Internals.Collections;
using Fluid.Internals.Lsfem;
using static Fluid.Internals.Toolbox;
using dbl = System.Double;
using DA = Fluid.Internals.Numerics.DblArithmetic;


namespace Fluid.Demos {
   using SymTnr = SymTensor<dbl,DA>;
   using Vec = Fluid.Internals.Collections.Vector<dbl,DA>;
   using Tnr = Fluid.Internals.Collections.Tensor<dbl, DA>;
   using Lst = List<int>;
   using PE = PseudoElement;
   public class CavityFlow : NavStokesSim {
      

      public CavityFlow(dbl dt, dbl re) : base(dt, re) {

      }

      protected override Dictionary<string,PE[][]> CreatePatches() {
         string dir = @"Seminar/Mathematica";
         string name = @"nodesC";
         string ext = @".txt";
         FileReader.SetDirAndFile(dir, name, ext);       // Read the points to an array.
         var hierarchy = FileReader.ReadHierarchy<dbl>();
         (bool success, var array) = hierarchy.ConvertToArray();
         double[][] positions;
         if(success)
            positions = (double[][]) array;
         else
            throw new FileNotFoundException("Could not locate nodes' position data.", $"{dir}/{name}{ext}");
         var dict = new Dictionary<string,PE[][]>(1);
         var pEmts = new PE[3][];
         pEmts[0] = new PE[3] {new PE()};  // TODO: Read positions from Mathematica input files.
      }
      protected override Dictionary<string,PseudoElement[]> CreateJoints() {

      }
      protected override Element[] CreateElements() {

      }
   }
}