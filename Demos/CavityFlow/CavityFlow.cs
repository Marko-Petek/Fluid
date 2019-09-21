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
   using Emt = Element;
   public class CavityFlow : NavStokesSim {
      /// <summary>A cartesian array of positions. [i][j][{x,y}]</summary>
      dbl [][][] OrdPos { get; set; }

      public dbl BoundaryVelocity { get; }
      

      public CavityFlow(dbl dt, dbl re, dbl boundaryVelocity) : base(dt, re) {
         OrdPos = ReadPos();
         BoundaryVelocity = boundaryVelocity;
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

         (int,PE) CreatePE(int i, int j) => PE.CreatePatchElement(currGInx, OrdPos[i][j], OrdPos[i][j+1], OrdPos[i+1][j]);
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
         (currGInx, finalPEmt[0]) = PE.CreateCustom(currGInx, (2, OrdPos[3][3]));
         joints.Add("UpperRightJoint", finalPEmt);
         return (currGInx, joints);

         (int,PE) CreateVertPE(int i, int j) => PE.CreateJointElement(currGInx, OrdPos[i][j], OrdPos[i+1][j]);
         (int,PE) CreateHorzPE(int i, int j) => PE.CreateJointElement(currGInx, OrdPos[i][j], OrdPos[i][j+1]);
      }
      protected override Element[] CreateElements() {
         var patch = Patches["Patch"];
         var rJoint = Joints["RightJoint"];
         var uJoint = Joints["UpperJoint"];
         var uRJoint = Joints["UpperRightJoint"];
         var emts = new Element[9];
         for(int i = 0, n = 0; i < 2; ++i)
            for(int j = 0; j < 2; ++j, ++n)
               emts[n] = Emt.CreatePatchElement(patch, i, j);
         for(int j = 4; j < 6; ++j)
            emts[j] = Emt.CreateUpperJointElement(patch, uJoint, j);
         for(int i = 6; i < 8; ++i)
            emts[i] = Emt.CreateRightJointElement(patch, rJoint, i);
         emts[8] = Emt.CreateUpperRightJointElement(patch, rJoint, uJoint, uRJoint);
         return emts;
      }
      protected override (Tnr uc, int nCPos, int nFPos) CreateConstrVars() {
         var emt00 = Elements.NearestNeighbors(new dbl[] {-2,-2}, 1)[0].Item2;
         var emt10 = Elements.NearestNeighbors(new dbl[] {-2, 0}, 1)[0].Item2;
         var emt20 = Elements.NearestNeighbors(new dbl[] {-2, 2}, 1)[0].Item2;
         var emt21 = Elements.NearestNeighbors(new dbl[] {0, 2}, 1)[0].Item2;
         var emt22 = Elements.NearestNeighbors(new dbl[] {2, 2}, 1)[0].Item2;
         var emt12 = Elements.NearestNeighbors(new dbl[] {2, 0}, 1)[0].Item2;
         var emt02 = Elements.NearestNeighbors(new dbl[] {2, -2}, 1)[0].Item2;
         var emt01 = Elements.NearestNeighbors(new dbl[] {0, -2}, 1)[0].Item2;
         emt00.Vals(0)[0] = BoundaryVelocity;                                          // Element 00.
         emt00.Vals(0)[1] = 0.0;
         SetConstr(emt00.P[0], 0);
         SetConstr(emt00.P[0], 1);
      }
   }
}