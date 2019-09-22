using System;
using System.IO;
using System.Collections.Generic;

using Fluid.Internals.Collections;
using Fluid.Internals.Lsfem;
using Fluid.Internals.Numerics;
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
      /// <summary>Boundary velocity.</summary>
      public dbl V { get; }
      

      public CavityFlow(dbl dt, dbl re, dbl boundaryVelocity) : base(dt, re) {
         OrdPos = ReadPos();
         V = boundaryVelocity;
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
      protected override (Tnr uC, int nCPos, int nFPos) CreateConstraints() {
         var uc = new Tnr(new Lst {12, 4}, 12);                                      // There will be 12 boundary positions.
         int nCPos = 12;
         int nFPos = 28;                                                             // 12 + 4*4
         var patch = Patches["Patch"];
         var rightJoint = Joints["RightJoint"];
         var upperJoint = Joints["UpperJoint"];
         var upperRightJoint = Joints["UpperRightJoint"];
         CreateLeftBoundaryVars(patch, uc, 0, (in Vec2 pos) => 0.0);
         CreateLeftBoundaryVars(patch, uc, 1, (in Vec2 pos) => -V);
         CreateLowerBoundaryVars(patch, uc, 0, (in Vec2 pos) => V);
         CreateLowerBoundaryVars(patch, uc, 1, (in Vec2 pos) => 0.0);
         CreateBoundaryVars(rightJoint, uc, 0, (in Vec2 pos) => 0.0);
         CreateBoundaryVars(rightJoint, uc, 1, (in Vec2 pos) => V);
         CreateBoundaryVars(upperJoint, uc, 0, (in Vec2 pos) => -V);
         CreateBoundaryVars(upperJoint, uc, 1, (in Vec2 pos) => 0.0);
         //CreateUpperRightCornerVar(upperRightJoint, uc, 0,)       // TODO: Set corner velocity to 0. Create a method that assigns proper values to the two middle edge points based on the specified function.
         throw new NotImplementedException();
      }
   }
}