using System;
using System.IO;
using System.Collections.Generic;
using static System.Math;

using Fluid.Internals.Collections;
using Fluid.Internals.Lsfem;
using Fluid.Internals.Numerics;
using static Fluid.Internals.Toolbox;
using dbl = System.Double;
using DA = Fluid.Internals.Numerics.DblArithmetic;


namespace Fluid.Runnables.CavityFlow {
using SymTnr = SymTensor<dbl,DA>;
using Vec = Fluid.Internals.Collections.Vector<dbl,DA>;
using Tnr = Fluid.Internals.Collections.Tensor<dbl, DA>;
using Lst = List<int>;
using PE = PseudoElement;
using Emt = Element;

/// <summary>Driven cavity flow problem is a very simple problem with which methods are initially tested.</summary>
public class CavityFlowSim : NavStokesFlow {
   /// <summary>A cartesian array of positions. [i][j][{x,y}]</summary>
   dbl [][][] OrdPos { get; set; }
   /// <summary>Boundary velocity.</summary>
   public dbl V { get; }
   

   CavityFlowSim(dbl dt, dbl re, dbl boundaryVelocity) : base() {   R.R("Reading element corner positions.");
      OrdPos = ReadPos();
      V = boundaryVelocity;
   }
   public static CavityFlowSim Create(dbl dt, dbl re, dbl boundaryVelocity) {
      var cavFlow = new CavityFlowSim(dt, re, boundaryVelocity);
      NavStokesFlow.Initialize(cavFlow);
      return cavFlow;
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
      var pEmts = new PE[3][];                                                               // Main patch.
      for(int i = 0; i < 3; ++i) {                                                           // Create the 9 PseudoElements.
         pEmts[i] = new PE[3];
         for(int j = 0; j < 3; ++j)
            (currGInx, pEmts[i][j]) = PE.CreatePatchElement(
               currGInx, OrdPos[i][j], OrdPos[i][j+1], OrdPos[i+1][j]); }
      var pEmts2 = new PE[1][];                                                              // Upper right patch containing a single element with a single position.
      pEmts2[0] = new PE[1];
      (currGInx, pEmts2[0][0]) = PE.CreateCustom(currGInx, (2, OrdPos[3][3]));
      var patches = new Dictionary<string,PE[][]>(1) {
         {"Patch", pEmts},
         {"UpperRightPatch", pEmts2} };
      return (currGInx, patches);
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
   protected override Tnr CreateConstraints() {
      var uc = new Tnr(new Lst {12, 4}, 12);                                      // There will be 12 boundary positions.
      var patch = Patches["Patch"];
      var rightJoint = Joints["RightJoint"];
      var upperJoint = Joints["UpperJoint"];
      var upperRightPatch = Patches["UpperRightPatch"];                          // TODO: Screw Joints, only use Patches. This should simpify boundary creating methods.
      F2Db fLow = (in Vec2 p) => V - (V/9)*Pow(p.X, 2);                          // Parabola that goes through p0 and p3 and has appex in the middle.
      F2Db fHigh = (in Vec2 p) => -V + (V/9)*Pow(p.X, 2);
      F2Db fRight = (in Vec2 p) => V - (V/9)*Pow(p.Y, 2);
      F2Db fLeft = (in Vec2 p) => -V + (V/9)*Pow(p.Y, 2);
      F2Db zero = (in Vec2 p) => 0.0;
      CreateLowerBoundaryVars(patch, rightJoint, uc, 0, fLow);                   // Constrain X.
      CreateLowerBoundaryVars(patch, rightJoint, uc, 1, zero);                   // Constrain Y.
      CreateRightBoundaryVars(rightJoint, upperRightPatch, uc, 0, zero);         // etc.
      CreateRightBoundaryVars(rightJoint, upperRightPatch, uc, 0, fRight);
      CreateUpperBoundaryVars(upperJoint, upperRightPatch, uc, 0, fHigh);
      CreateUpperBoundaryVars(upperJoint, upperRightPatch, uc, 1, zero);
      CreateLeftBoundaryVars(patch, upperJoint, uc, 0, zero);
      CreateLeftBoundaryVars(patch, upperJoint, uc, 1, fLeft);
      return uc;
   }
}
}