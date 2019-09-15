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
   using PE = PseudoElement;
   
   public abstract class Simulation {
      /// <summary>Number of independent variables at a single position (= number of equations).</summary>
      public int NVar { get; internal set; }
      /// <summary>Number of all positions.</summary>
      public int NPos { get; protected set; }
      /// <summary>Number of positions with constrained variables.</summary>
      public int NcPos { get; protected set; }
      /// <summary>Number of positions with free variables.</summary>
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
      protected Mesh Mesh { get; private set; }
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
         (currGInx, Patches) = CreatePatches();                                     R.R("Creating Joints.");
         Joints = CreateJoints();
         Mesh = new Mesh();                                             R.R("Creating Positions.");
         Mesh.Pos = CreatePositions();
         NPos = Mesh.Pos.Count;                                         R.R("Creating Elements and calculating overlap integrals.");
         var emts = CreateElements();                                   R.R("Creating a list of Centers of Mass.");
         var coms = CreateCOMs(emts);
         Mesh.Elements = CreateElementTree(coms, emts);
      }
      /// <summary>Constrainednes of a variable (i,j). True = constrained </summary>
      /// <param name="i">Position index.</param>
      /// <param name="j">Variable index.</param>
      public bool Constr(int i, int j) => C[NVar*i + j];
      /// <summary></summary>
      protected abstract (int newCurrGInx, Dictionary<string,PE[][]>) CreatePatches(int currGinx);
      /// <summary></summary>
      protected abstract (int newCurrGInx, Dictionary<string,PseudoElement[]>) CreateJoints(int currGInx);
      /// <summary>User must supply custom logic here. The logic here depends on the way the blocks are joined together. Do not forget to trim excess space.</summary>
      protected abstract Element[] CreateElements();
      protected My.List<Vec2> CreatePositions() {
         int approxNPos = Patches.Sum( patch => patch.Value.Length ) * 5 +
            Joints.Sum( joint => joint.Value.Length ) * 3;                    // Estimate for the number of positions so that we can allocate an optimal amount of space for the posList.
         var posList = new My.List<Vec2>(approxNPos);
         int gInx = 0;                                                        R.R("Adding positions to Mesh.");
         foreach(var patch in Patches.Values) {                               // Global indices on PseudoElements are not yet set. We set them now and add positions to Mesh.
            foreach(var pseudoRow in patch) {
               foreach(var pseudoEmt in pseudoRow) {
                  for(int i = 0; i < pseudoEmt.PEInx.Length; ++i) {
                     pseudoEmt.GInx[i] = gInx;
                     posList.Add(pseudoEmt.Pos[i]);
                     ++gInx; } } } }
         foreach(var joint in Joints.Values) {                                // We also set global indices on Joints and add their positions to Mesh.
               foreach(var pseudoEmt in joint) {
                  for(int i = 0; i < pseudoEmt.PEInx.Length; ++i) {
                     pseudoEmt.GInx[i] = gInx;
                     posList.Add(pseudoEmt.Pos[i]);
                     ++gInx; } } }
         posList.TrimExcessSpace();
         return posList;
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
      /// <summary>Create constrained variables at desired positions and also return the number of constrained and free variables.</summary>
      protected abstract (Tnr uc, int ncPos, int nfPos) CreateConstrVars();
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

      /// <summary>Find solution value at specified point.</summary><param name="pos">Sought after position.</param><param name="vars">Indices of variables we wish to retrieve.</param>S
      public abstract double[] Solution(in Vec2 pos);
   }
}