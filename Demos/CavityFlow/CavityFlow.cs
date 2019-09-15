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
      dbl [][][] Pos { get; }
      

      public CavityFlow(dbl dt, dbl re) : base(dt, re) {
         Pos = ReadPos();
      }

      dbl[][][] ReadPos() {
         string dir = @"Seminar/Mathematica";
         string name = @"nodesC";
         string ext = @".txt";
         FileReader.SetDirAndFile(dir, name, ext);       // Read the points to an array.
         var hierarchy = FileReader.ReadHierarchy<dbl>();
         (bool success, var array) = hierarchy.ConvertToArray();
         if(success) {
            if(array is dbl[][][] pos)
               return pos;
            else
               throw new InvalidCastException("Positions array could not be cast to a rank 3 double array."); }
         else
            throw new FileNotFoundException("Could not locate nodes' position data.",
               $"{dir}/{name}{ext}");
      }

      protected override (int newCurrGInx, Dictionary<string,PE[][]>) CreatePatches(int currGInx) {
         var pEmts = new PE[3][];
         for(int i = 0; i < 3; ++i) {                                                           // Create the 9 PseudoElements.
            pEmts[i] = new PE[3];
            for(int j = 0; j < 3; ++j)
               (currGInx, pEmts[i][j]) = CreatePE(i,j); }
         var patches = new Dictionary<string,PE[][]>(1) { {"Patch", pEmts} };
         return (currGInx, patches);

         (int,PE) CreatePE(int i, int j) => PE.CreatePatchElement(currGInx, Pos[i][j], Pos[i][j+1], Pos[i+1][j]);
      }
      protected override (int newCurrGInx, Dictionary<string,PseudoElement[]>) CreateJoints(int currGInx) {
         var rPEmts = new PE[3];
         for(int i = 0; i < 3; ++i)
            (currGInx, rPEmts[i]) = CreateVertPE(i,3);
         var joints = new Dictionary<string,PE[]> { {"RightJoint", rPEmts} };
         var uPEmts = new PE[3];
         for(int j = 0; j < 3; ++j)
            (currGInx, uPEmts[j]) = CreateHorzPE(3,j);
         joints.Add("UpperJoint", uPEmts);
         var finalPEmt = new PE[1];
         (currGInx, finalPEmt[0]) = PE.CreateCustom(currGInx, (2, Pos[3][3]));
         joints.Add("UpperRightJoint", finalPEmt);
         return (currGInx, joints);

         (int,PE) CreateVertPE(int i, int j) => PE.CreateJointElement(currGInx, Pos[i][j], Pos[i+1][j]);
         (int,PE) CreateHorzPE(int i, int j) => PE.CreateJointElement(currGInx, Pos[i][j], Pos[i][j+1]);
      }
      protected override Element[] CreateElements() {
         var patch = Patches["Patch"];
         var rJoint = Joints["RightJoint"];
         var uJoint = Joints["UpperJoint"];
         var uRJoint = Joints["UpperRightJoint"];
         var emts = new Element[9];
         var pe00 = patch[0][0];
         var pe01 = patch[0][1];
         var pe10 = patch[1][0];
         var pe11 = patch[1][1];
         emts[0] = new Element(pe00[2], pe00[3], pe00[4], pe01[2], pe01[1], pe01[0], pe11[2], pe10[4], pe10[3], pe10[2], pe00[0], pe00[1]);
      }
   }
}