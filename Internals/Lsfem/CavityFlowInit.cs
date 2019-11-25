#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using Fluid.Internals.Numerics;
using Fluid.Internals.Collections;
using My = Fluid.Internals.Collections.Custom;
using static Fluid.Internals.Toolbox;
using dbl = System.Double;
using DA = Fluid.Internals.Numerics.DblArithmetic;
using System.IO;
using static System.Math;

namespace Fluid.Internals.Lsfem {
using static Fluid.Internals.Lsfem.SimManager;
using Vec = Fluid.Internals.Collections.Vector<dbl,DA>;
using Tnr = Fluid.Internals.Collections.Tensor<dbl, DA>;
using V = Voids<dbl,DA>;
using Lst = List<int>;
using VecLst = My.List<Vec2>;
using PE = PseudoElement;
public class CavityFlowInit : NavStokesFlowInit {
   /// <summary>Boundary velocity.</summary>
   dbl V { get; }


   /// <summary>A means of initializing a CavityFlow.</summary>
   /// <param name="dt">Time step.</param>
   /// <param name="re">Reynolds number.</param>
   /// <param name="v">Inlet velocity.</param>
   public CavityFlowInit(dbl dt, dbl re, dbl v) : base(dt, re) {
      V = v;
   }


   public override Dictionary<string, dbl[][][]> GetRawOrderedPosData() {
      string dir = @"Seminar/Mathematica";
      string name = @"nodesC";
      string ext = @".txt";
     T.FileReader.SetDirAndFile(dir, name, ext);                      // Read the points to an array.
      var hierarchy =T.FileReader.ReadHierarchy<dbl>();
      var convRes = hierarchy.ConvertToArray();
      var res = convRes.success switch {
         true => convRes.array switch {
            dbl[][][] pos => pos,
            _ => throw new InvalidCastException("Positions array could not be cast to a rank 3 double array.") },
         _ => throw new FileNotFoundException("Could not locate nodes' position data.", $"{dir}/{name}{ext}") };
      return new Dictionary<string, dbl[][][]> { {"Patch", res} };
   }
   public override (int newCurrGInx, Dictionary<string,PE[][]>) CreatePatches(
   Dictionary<string, dbl[][][]> ordPos) {
      var rawPat = ordPos["Patch"];
      int currGInx = 0;
      var pEmts = new PE[3][];                                                               // Main patch.
      for(int i = 0; i < 3; ++i) {                                                           // Create the 9 PseudoElements.
         pEmts[i] = new PE[3];
         for(int j = 0; j < 3; ++j)
            (currGInx, pEmts[i][j]) = PE.CreatePatchElement(
               currGInx, rawPat[i][j], rawPat[i][j+1], rawPat[i+1][j]); }
      var pEmts2 = new PE[1][];                                                              // Upper right patch containing a single element with a single position.
      pEmts2[0] = new PE[1];
      (currGInx, pEmts2[0][0]) = PE.CreateCustom(currGInx, (2, rawPat[3][3]));
      var patches = new Dictionary<string,PE[][]>(1) {
         {"Patch", pEmts},
         {"UpperRightPatch", pEmts2} };
      return (currGInx, patches);
   }
   public override Dictionary<string,PseudoElement[]> CreateJoints(
   int currGInx, Dictionary<string, dbl[][][]> ordPos) {
      var rawPat = ordPos["Patch"];
      var rPEmts = new PE[3];
      for(int i = 0; i < 3; ++i)
         (currGInx, rPEmts[i]) = CreateVertPE(i,3);
      var joints = new Dictionary<string,PE[]> { {"RightJoint", rPEmts} };
      var uPEmts = new PE[3];
      for(int j = 0; j < 3; ++j)
         (currGInx, uPEmts[j]) = CreateHorzPE(3,j);
      joints.Add("UpperJoint", uPEmts);
      return joints;

      (int,PE) CreateVertPE(int i, int j) => PE.CreateJointElement(currGInx, rawPat[i][j], rawPat[i+1][j]);
      (int,PE) CreateHorzPE(int i, int j) => PE.CreateJointElement(currGInx, rawPat[i][j], rawPat[i][j+1]);
   }
   public override Element[] CreateElements(Dictionary<string,PE[][]> patches, Dictionary<string,PE[]> joints) {
      var patch = patches["Patch"];
      var rJoint = joints["RightJoint"];
      var uJoint = joints["UpperJoint"];
      var uRJoint = joints["UpperRightJoint"];
      var emts = new Element[9];
      for(int i = 0, n = 0; i < 2; ++i)
         for(int j = 0; j < 2; ++j, ++n)
            emts[n] = Element.CreatePatchElement(patch, i, j);
      for(int j = 4; j < 6; ++j)
         emts[j] = Element.CreateUpperJointElement(patch, uJoint, j);
      for(int i = 6; i < 8; ++i)
         emts[i] = Element.CreateRightJointElement(patch, rJoint, i);
      emts[8] = Element.CreateUpperRightJointElement(patch, rJoint, uJoint, uRJoint);
      return emts;
   }
   public override (Tnr uc, Constraints bits) CreateConstraints(
   Dictionary<string,PE[][]> patches, Dictionary<string,PE[]> joints, int nPos, int nVar) {
      var bits = new Constraints(nPos, nVar);
      var uc = new Tnr(new Lst {12, 4}, 12);                                      // There will be 12 boundary positions.
      var patch = patches["Patch"];
      var rightJoint = joints["RightJoint"];
      var upperJoint = joints["UpperJoint"];
      var upperRightPatch = patches["UpperRightPatch"];                          // TODO: Screw Joints, only use Patches. This should simpify boundary creating methods.
      F2Db fLow = (in Vec2 p) => V - (V/9)*Pow(p.X, 2);                          // Parabola that goes through p0 and p3 and has appex in the middle.
      F2Db fHigh = (in Vec2 p) => -V + (V/9)*Pow(p.X, 2);
      F2Db fRight = (in Vec2 p) => V - (V/9)*Pow(p.Y, 2);
      F2Db fLeft = (in Vec2 p) => -V + (V/9)*Pow(p.Y, 2);
      F2Db zero = (in Vec2 p) => 0.0;
      CreateLowerBoundaryVars(patch, rightJoint, uc, 0, fLow, bits);                   // Constrain X.
      CreateLowerBoundaryVars(patch, rightJoint, uc, 1, zero, bits);                   // Constrain Y.
      CreateRightBoundaryVars(rightJoint, upperRightPatch, uc, 0, zero, bits);         // etc.
      CreateRightBoundaryVars(rightJoint, upperRightPatch, uc, 0, fRight, bits);
      CreateUpperBoundaryVars(upperJoint, upperRightPatch, uc, 0, fHigh, bits);
      CreateUpperBoundaryVars(upperJoint, upperRightPatch, uc, 1, zero, bits);
      CreateLeftBoundaryVars(patch, upperJoint, uc, 0, zero, bits);
      CreateLeftBoundaryVars(patch, upperJoint, uc, 1, fLeft, bits);
      return (uc, bits);
   }
   /// <summary>Take the constraints tensor by reference and create values on the left boundary of a patch. Specify the variable index and the variable field.</summary>
   /// <param name="leftPatch">A left boundary patch (not connected to anything on its left).</param>
   /// <param name="upperJoint">An boundary joint.</param>
   /// <param name="uC">Contraints tensor.</param>
   /// <param name="varInx">Variable index of the variable that will take on the field values.</param>
   /// <param name="f">Field to assign to the variable.</param>
   protected void CreateLeftBoundaryVars(PE[][] leftPatch, PE[] upperJoint, Tnr uC,
   int varInx, F2Db f, Constraints bits) {
      int m = leftPatch.Length;                                                     // Number of rows.
      int iL = m - 1;                                                               // Last index.
      PE pEmtLow, pEmtHigh;
      dbl h10, h11;
      int[] cap = new int[] {4};
      int[] lowBound = new int[] {9};
      int[] g = (int[]) Array.CreateInstance(typeof(int), cap, lowBound);          // Create an array of global indices, starting at index 9.
      dbl[] u = (dbl[]) Array.CreateInstance(typeof(dbl), cap, lowBound);
      for(int i = 0; i < iL; ++i) {                                                  // Over all PE rows, except last.
         pEmtLow = leftPatch[i][0];
         pEmtHigh = leftPatch[i+1][0];
         SetNodeVals(2, 0, 1, 2,
            in pEmtHigh._Poss[2], in pEmtLow._Poss[0],
            in pEmtLow._Poss[1], in pEmtLow._Poss[2]);
         Apply(10, 12); }
      pEmtLow = leftPatch[iL][0];                                                    // And now for the last row.
      pEmtHigh = upperJoint[0];
      SetNodeVals(0, 0, 1, 2,
         in pEmtHigh._Poss[0], in pEmtLow._Poss[0],
         in pEmtLow._Poss[1], in pEmtLow._Poss[2]);
      Apply(9, 12);

      void SetNodeVals(int i1, int i2, int i3, int i4,
      in Vec2 p9, in Vec2 p10, in Vec2 p11, in Vec2 p12) {
         g[9] = pEmtHigh.GInxs[i1];
         g[10] = pEmtLow.GInxs[i2];
         g[11] = pEmtLow.GInxs[i3];
         g[12] = pEmtLow.GInxs[i4];
         h10 = f(in p10);
         h11 = f(in p11);
         u[9] = f(in p9);
         u[12] = f(in p12);
         u[10] = 0.25*(18*h10 - 9*h11 + 2*u[12] - 11*u[9]);
         u[11] = 0.25*(-9*h10 + 18*h11 - 11*u[12] + 2*u[9]);
      }
      void Apply(int startNode, int endNode) {
         for(int p = startNode; p <= endNode; ++p) {                                              // Over first three points in a PE.
            uC[g[p], varInx] = u[p];
            bits[g[p], varInx] = true; }
      }
   }
   protected void CreateRightBoundaryVars(PE[] rightJoint, PE[][] upperRightPatch,
   Tnr uC, int varInx, F2Db f, Constraints bits) {
      int m = rightJoint.Length;                                                     // Number of rows.
      int iL = m - 1;                                                               // Last index.
      PE pEmtLow, pEmtHigh;
      dbl h4, h5;
      int[] cap = new int[] {4};
      int[] lowBound = new int[] {3};
      int[] g = (int[]) Array.CreateInstance(typeof(int), cap, lowBound);          // Create an array of global indices, starting at index 9.
      dbl[] u = (dbl[]) Array.CreateInstance(typeof(dbl), cap, lowBound);
      for(int i = 0; i < iL; ++i) {                                                  // Over all PE rows, except last.
         pEmtLow = rightJoint[i];
         pEmtHigh = rightJoint[i+1];

         SetNodeVals(0, 1, 2, 0,
            in pEmtLow._Poss[0], in pEmtLow._Poss[1],
            in pEmtLow._Poss[2], in pEmtHigh._Poss[0]);
         Apply(3, 5); }
      pEmtLow = rightJoint[iL];                                                    // And now for the last row.
      pEmtHigh = upperRightPatch[0][0];               // TODO: Create upper right patch instead of the Frankenstein joint.
      SetNodeVals(0, 1, 2, 2,
         in pEmtLow._Poss[0], in pEmtLow._Poss[1],
         in pEmtLow._Poss[2], in pEmtHigh._Poss[2]);
      Apply(3, 6);

      void SetNodeVals(int i1, int i2, int i3, int i4,
      in Vec2 p3, in Vec2 p4, in Vec2 p5, in Vec2 p6) {
         g[3] = pEmtHigh.GInxs[i1];
         g[4] = pEmtLow.GInxs[i2];
         g[5] = pEmtLow.GInxs[i3];
         g[6] = pEmtLow.GInxs[i4];
         h4 = f(in p4);
         h5 = f(in p5);
         u[3] = f(in p3);
         u[6] = f(in p6);
         u[4] = 0.25*(18*h4 - 9*h5 - 11*u[3] + 2*u[6]);
         u[5] = 0.25*(-9*h4 + 18*h5 + 2*u[3] - 11*u[6]);
      }
      void Apply(int startNode, int endNode) {
         for(int p = startNode; p <= endNode; ++p) {                                              // Over first three points in a PE.
            uC[g[p], varInx] = u[p];
            bits[g[p], varInx] = true; }
      }
   }
   protected void CreateLowerBoundaryVars(PE[][] lowerPatch, PE[] rightJoint,
      Tnr uC, int varInx, F2Db f, Constraints bits) {
      int n = lowerPatch[0].Length;                                           // Number of cols.
      int jL = n - 1;
      PE pEmtLeft, pEmtRight;
      dbl h1, h2;
      int[] g = new int[4];
      dbl[] u = new dbl[4];
      for(int j = 0; j < jL; ++j) {
         pEmtLeft = lowerPatch[0][j];
         pEmtRight = lowerPatch[0][j+1];
         SetNodeVals(2,3,4,2,
            in pEmtLeft._Poss[2], in pEmtLeft._Poss[3],
            in pEmtLeft._Poss[4], in pEmtRight._Poss[2]);
         Apply(0,2); }
      pEmtLeft = lowerPatch[0][jL];
      pEmtRight = rightJoint[0];
      SetNodeVals(2,3,4,0,
            in pEmtLeft._Poss[2], in pEmtLeft._Poss[3],
            in pEmtLeft._Poss[4], in pEmtRight._Poss[0]);
      Apply(0,3);

      void SetNodeVals(int i1, int i2, int i3, int i4,
      in Vec2 p0, in Vec2 p1, in Vec2 p2, in Vec2 p3) {
         g[0] = pEmtLeft.GInxs[i1];
         g[1] = pEmtLeft.GInxs[i2];
         g[2] = pEmtLeft.GInxs[i3];
         g[3] = pEmtRight.GInxs[i4];
         h1 = f(in p1);
         h2 = f(in p2);
         u[0] = f(in p0);
         u[3] = f(in p3);
         u[1] = 0.25*(18*h1 - 9*h2 - 11*u[0] + 2*u[3]);
         u[2] = 0.25*(-9*h1 + 18*h2 + 2*u[0] - 11*u[3]);
      }
      void Apply(int startNode, int endNode) {
         for(int p = startNode; p <= endNode; ++p) {                                              // Over first three points in a PE.
            uC[g[p], varInx] = u[p];
            bits[g[p], varInx] = true; }
      }
   }

   protected void CreateUpperBoundaryVars(PE[] upperJoint, PE[][] upperRightPatch,
   Tnr uC, int varInx, F2Db f, Constraints bits) {
      int n = upperJoint.Length;
      int jL = n - 1;
      PE pEmtLeft, pEmtRight;
      dbl h7, h8;
      int[] cap = new int[] {4};
      int[] lowBound = new int[] {6};
      int[] g = (int[]) Array.CreateInstance(typeof(int), cap, lowBound);          // Create an array of global indices, starting at index 9.
      dbl[] u = (dbl[]) Array.CreateInstance(typeof(dbl), cap, lowBound);
      for(int j = 0; j < jL; ++j) {
         pEmtLeft = upperJoint[j];
         pEmtRight = upperJoint[j+1];
         SetNodeVals(0,2,1,0,
            in pEmtRight._Poss[0], in pEmtLeft._Poss[2],
            in pEmtLeft._Poss[1], in pEmtLeft._Poss[0]);
         Apply(7,9); }
      pEmtLeft = upperJoint[jL];
      pEmtRight = upperRightPatch[0][0];
      SetNodeVals(2,2,1,0,
            in pEmtRight._Poss[2], in pEmtLeft._Poss[2],
            in pEmtLeft._Poss[1], in pEmtLeft._Poss[0]);
      Apply(6,9);
      

      void SetNodeVals(int i1, int i2, int i3, int i4,
      in Vec2 p6, in Vec2 p7, in Vec2 p8, in Vec2 p9) {
         g[6] = pEmtRight.GInxs[i1];
         g[7] = pEmtLeft.GInxs[i2];
         g[8] = pEmtLeft.GInxs[i3];
         g[9] = pEmtLeft.GInxs[i4];
         h7 = f(in p7);
         h8 = f(in p8);
         u[6] = f(in p6);
         u[9] = f(in p9);
         u[7] = 0.25*(18*h7 - 9*h8 + 2*u[9] - 11*u[6]);
         u[8] = 0.25*(-9*h7 + 18*h8 - 11*u[9] + 2*u[6]);
      }
      void Apply(int startNode, int endNode) {
         for(int p = startNode; p <= endNode; ++p) {                                              // Over first three points in a PE.
            uC[g[p], varInx] = u[p];
            bits[g[p], varInx] = true; }
      }
   }
}
}
#nullable restore