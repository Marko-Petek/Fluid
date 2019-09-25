using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static System.Math;

using Fluid.Internals.Numerics;
using Fluid.Internals.Collections;
using My = Fluid.Internals.Collections.Custom;
using static Fluid.Internals.Toolbox;
using dbl = System.Double;
using DA = Fluid.Internals.Numerics.DblArithmetic;

using Supercluster.KDTree;

namespace Fluid.Internals.Lsfem {
   using SymTnr = SymTensor<dbl,DA>;
   using Vec = Fluid.Internals.Collections.Vector<dbl,DA>;
   using Tnr = Fluid.Internals.Collections.Tensor<dbl, DA>;
   using Lst = List<int>;
   using VecLst = My.List<Vec2>;
   using PE = PseudoElement;
   
   public abstract class Simulation {
      /// <summary>An unordered (listed in sequence) array of positions.</summary>
      public Vec2[] Pos { get; protected set;}
      /// <summary>Contains node indices that belong to an element. (element index, nodes)</summary>
      public KDTree<double,Element> Elements { get; set; }
      /// <summary>Number of independent variables at a single position (= number of equations).</summary>
      public int NVar { get; internal set; }
      /// <summary>Number of all positions.</summary>
      public int NPos { get; protected set; }
      /// <summary>Number of positions with free variables. Used to determine solution tensor capacity.</summary>
      public int NfPos { get; protected set; }
      /// <summary>Primary Dynamics tensor, 4th rank. Includes only free variables.</summary>
      public Tnr K { get; internal set; }
      /// <summary>Forcing tensor, 2nd rank.</summary>
      public Tnr F { get; internal set; }
      /// <summary>Secondary dynamics tensor, 5th rank. Includes both, free and constrained variables.
      /// Node (1), Derivative (2), 1st index of element matrix (3), 2nd index of element matrix (4).</summary>
      public Tnr A { get; internal set; }
      
      /// <summary>Free variables tensor, 2nd rank. There can be free and constrained variables at a single position.</summary>
      public Tnr UF { get; internal set; }
      /// <summary>Constrained variables tensor, 2nd rank. There can be free and constrained variables at a single position.</summary>
      public Tnr UC { get; internal set; }
      /// <summary>A sequence of bits that indicate constrainedness of each variable: (i,j) => NVar*i + j. Therefore the BitArray holds NPos*NVar bits.</summary>
      protected BitArray C { get; set; }
      /// <summary>Fetches a vector of variables</summary>
      /// <param name="dummy"></param>
      /// <param name="inx"></param>
      public Vec U(Vec dummy, int inx) {
         Vec u = UF[dummy, inx];
         if(u != null)
            return u;
         else return UC[dummy, inx];
      }
      /// <summary>Fetches a variable value at existing (i,j) from wherever the variable exists (free variables UF or constrained variables UC).</summary>
      /// <param name="inxs">A set of indices that extends all the way down to value rank.</param>
      public (dbl val, bool free) U(params int[] inxs) {
         dbl u = UF[inxs];
         if(u != 0.0)
            return (u, true);
         else return (UC[inxs], false);
      }
      /// <summary>Secondary F. Third rank participant in the assembly process of F.</summary>
      public Tnr Fs { get; internal set; }
      /// <summary>2D lists of nodes lying inside a block.</summary>
      protected Dictionary<string,PseudoElement[][]> Patches { get; set; }
      /// <summary>1D lists of nodes shared across blocks.</summary>
      protected Dictionary<string,PseudoElement[]> Joints { get; set; }
      /// <summary>Solver that solves the linear system using Conjugate Gradients.</summary>
      protected ConjGradsSolver Solver { get; }


      public Simulation() {
         Initialize();
      }
      /// <summary>Set the Simulation up.</summary>
      public void Initialize() {                                        R.R("Creating Patches.");
         int currGInx = 0;
         (currGInx, Patches) = CreatePatches(currGInx);                 R.R("Creating Joints.");
         (currGInx, Joints) = CreateJoints(currGInx);                   R.R("Creating Positions.");
         (NPos, Pos) = CreatePositions();                               R.R("Creating Elements and calculating overlap integrals.");
         var emts = CreateElements();                                   R.R("Creating a list of Centers of Mass.");
         var coms = CreateCOMs(emts);                                   R.R("Creating ElementTree.");
         Elements = CreateElementTree(coms, emts);                      R.R("Creating constraints.");
         UC = CreateConstraints();                                      R.R("Counting free positions.");
         NfPos = CountFreePositions(NPos, NVar);
      }
      /// <summary>Constrainednes of a variable (i,j). True = constrained </summary>
      /// <param name="i">Position index.</param>
      /// <param name="j">Variable index.</param>
      public bool Constr(int i, int j) => C[NVar*i + j];
      protected void SetConstr(int i, int j) => C[NVar*i + j] = true;
      /// <summary></summary>
      protected abstract (int newCurrGInx, Dictionary<string,PE[][]>) CreatePatches(int currGinx);
      /// <summary></summary>
      protected abstract (int newCurrGInx, Dictionary<string,PseudoElement[]>) CreateJoints(int currGInx);
      /// <summary>User must supply custom logic here. The logic here depends on the way the blocks are joined together. Do not forget to trim excess space.</summary>
      protected abstract Element[] CreateElements();
      protected (int nPos, Vec2[]) CreatePositions() {
         int approxNPos = Patches.Sum( patch => patch.Value.Length ) * 5 +
            Joints.Sum( joint => joint.Value.Length ) * 3;                    // Estimate for the number of positions so that we can allocate an optimal amount of space for the posList.
         var posList = new My.List<Vec2>(approxNPos);
         int gInx = 0;                                                        R.R("Adding positions to Mesh.");
         foreach(var patch in Patches.Values) {                               // Global indices on PseudoElements are not yet set. We set them now and add positions to Mesh.
            foreach(var pseudoRow in patch) {
               foreach(var pseudoEmt in pseudoRow) {
                  for(int i = 0; i < pseudoEmt.PEInxs.Length; ++i) {
                     pseudoEmt.GInxs[i] = gInx;
                     posList.Add(pseudoEmt.Poss[i]);
                     ++gInx; } } } }
         foreach(var joint in Joints.Values) {                                // We also set global indices on Joints and add their positions to Mesh.
               foreach(var pseudoEmt in joint) {
                  for(int i = 0; i < pseudoEmt.PEInxs.Length; ++i) {
                     pseudoEmt.GInxs[i] = gInx;
                     posList.Add(pseudoEmt.Poss[i]);
                     ++gInx; } } }
         posList.TrimExcessSpace();
         return (posList.Count, posList._E);
      }
      /// <summary>Create the list of COMs after the Elements are created.</summary>
      protected double[][] CreateCOMs(IList<Element> elements) {
         int nEmts = elements.Count;
         var coms = new double[nEmts][];
         dbl xCom, yCom;
         for(int i = 0; i < nEmts; ++i) {       // For each Element.
            xCom = 0.0;
            yCom = 0.0;
            for(int j = 0; j < 4; ++j) {              // Corner nodes.
               xCom += elements[i].Pos(3*j).X;
               yCom += elements[i].Pos(3*j).Y; }
            xCom /= 4;
            yCom /= 4;
            coms[i] = new double[2] { xCom, yCom }; }
         return coms;
      }
      /// <summary>Elements are found in InternalNodeArray. Do not forget to trim excess space in array for proper operation of KDTree.</summary>
      protected KDTree<double,Element> CreateElementTree(dbl[][] coms, Element[] emts) {               R.R("Creating KDTree of Elements vs. CoMs positions.");
         var emtTree = new KDTree<double,Element>(2, coms, emts,
            (r1,r2) => Sqrt(Pow(r2[0]-r1[0], 2.0) + Pow(r2[1]-r1[1], 2.0)));
         return emtTree;
      }
      /// <summary>Count positions containing at least one free variable.</summary>
      /// <param name="nPos">Number of positions.</param>
      /// <param name="nVar">Number of variables at a position.</param>
      int CountFreePositions(int nPos, int nVar) {
         int nFreePos = 0;
         int n = C.Length;
         bool freePos;                          // This will signify whether a position is free after the j-loop.
         for(int i = 0; i < nPos; ++i) {
            freePos = false;
            for(int j = 0; j < nVar; ++j) {
               if(!Constr(i,j)) {               // If there is a single free position, then the position is free.
                  freePos = true;
                  break; } }
            if(freePos) {                       // At least one free variable at position.
               ++nFreePos; } }
         return nFreePos;
      }                
      /// <summary>Take the constraints tensor by reference and create values on the left boundary of a patch. Specify the variable index and the variable field.</summary>
      /// <param name="leftPatch">A left boundary patch (not connected to anything on its left).</param>
      /// <param name="upperJoint">An boundary joint.</param>
      /// <param name="uC">Contraints tensor.</param>
      /// <param name="varInx">Variable index of the variable that will take on the field values.</param>
      /// <param name="f">Field to assign to the variable.</param>
      protected void CreateLeftBoundaryVars(PE[][] leftPatch, PE[] upperJoint, Tnr uC, int varInx, F2Db f) {
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
               SetConstr(g[p], varInx); }
         }
      }
      protected void CreateRightBoundaryVars(PE[] rightJoint, PE[][] upperRightPatch,
      Tnr uC, int varInx, F2Db f) {
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
               SetConstr(g[p], varInx); }
         }
      }
      protected void CreateLowerBoundaryVars(PE[][] lowerPatch, PE[] rightJoint,
         Tnr uC, int varInx, F2Db f) {
         int n = lowerPatch[0].Length;                          // Number of cols.
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
               SetConstr(g[p], varInx); }
         }
      }

      protected void CreateUpperBoundaryVars(PE[] upperJoint, PE[][] upperRightPatch,
      Tnr uC, int varInx, F2Db f) {
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
               SetConstr(g[p], varInx); }
         }
      }

      /// <summary>Create constrained variables at desired positions.</summary>
      protected abstract Tnr CreateConstraints();
      /// <summary>Creates free values as zeros: an empty tensor.</summary>
      protected virtual Tnr CreateFreeVars() =>
         new Tnr(new List<int> {NfPos,NVar}, NfPos);

      /// <summary>Takes updated secondary (dynamics and forcing) tensors and constrained node variables and creates primary tensors which are ready to be passed to the Solver. This process iterates over all Elements (surfaces) and adds their contribution to corresponding nodes (points).</summary>
      /// <param name="emts">A collection of Elements, each element containing overlap integrals of node functions and a mapping from eNodes to gNodes.</param>
      (Tnr K, Tnr F) AssemblePrimaryTensors(IEnumerable<Element> emts) {
         var tnrK = new Tnr(new Lst{NPos,NVar,NPos,NVar}, NfPos);                    // Create a 4th rank result tensor.
         var tnrF = new Tnr(new Lst{NPos,NVar}, NfPos);                            // Create a 2nd rank result tensor.
         Lst allJs = Enumerable.Range(0,NVar).ToList();
         foreach(var emt in emts) {
         for(int γ = 0, c = emt.P[γ];  γ < 12;  ++γ, c = emt.P[γ]) {
            Vec vecF_j = tnrF[Vec.Ex, c];
            var jfs = allJs.Where( j => !Constr(c,j) ).ToArray();                      // Determine which variables (c,j) are free.
            for(int α = 0, a = emt.P[α];  α < 12;  ++α, a = emt.P[α]) {
            for(int p = 0; p < 3; ++p) {
            for(int r = 0; r < 3; ++r) {
               Tnr a_ij = A[Tnr.Ex, a,p,r];                                            // Precursor for all terms.
               for(int β = 0, b = emt.P[β];  β < 12;  ++β, b = emt.P[β]) {
                  Tnr f_si = Fs[Tnr.Ex, b];
                  for(int s = 0; s < 3; ++s) {                                         // Index for A*Fs_j
                     Vec fs_i = f_si[Vec.Ex, s];
                     Vec afs_j = (Vec) Tnr.Contract(a_ij, fs_i, 1, 1);                 // Prepare for first term of primary forcing.
                     foreach(var j in allJs)
                        vecF_j[j] += emt.T[3*α+r, 3*γ+p, 3*β+s] * afs_j[j];
                     for(int q = 0; q < 3; ++q) {
                        Tnr a_ik = A[Tnr.Ex, b,q,s];                                   // For second term in primary forcing.
                        Tnr aa_jk = Tnr.Contract(a_ij, a_ik, 1, 1);                    // Precursor for first term in PD and second term in PF.
                        for(int δ = 0, d = emt.P[δ]; δ < 12; ++δ, d = emt.P[δ]) {
                           var grpsK = allJs.GroupBy( j => Constr(c,j) ).ToArray();
                           var kfs = grpsK.Where( grp => grp.Key == false ).
                              Single().ToArray();                                      // Determine which variables (c,j) are free and which constrained.
                           var kcs = grpsK.Where( grp => grp.Key == true ).
                              Single().ToArray();
                           dbl coefQ = emt.Q[3*α+r, 3*β+s, 3*γ+p, 3*δ+q];
                           foreach(var jf in jfs) {
                              Vec aa_k = aa_jk[Vec.Ex,jf];
                              foreach(int k in kfs)
                                 tnrK[c,jf,d,k] += coefQ * aa_k[k];
                              foreach(int l in kcs)
                                 vecF_j[jf] -= coefQ * aa_k[l] * UC[d,l];
            } } } } } } } } } }
         return (tnrK, tnrF);
      }
      /// <summary>Update secondary tensors (dynamics A and forcing Fs) from the current state of the variable field U. These secondaries will be used in the assembly process of primary tensors. This code is case-dependent (on the system of PDE) and has to be provided by library user. See NavStokesSim.cs for an example.</summary>
      protected abstract void UpdateDynamicsAndForcing();
      // TODO: Set constraints.
   }
}