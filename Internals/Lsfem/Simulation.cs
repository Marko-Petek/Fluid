using System;
using System.Collections.Generic;
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
   
   public abstract class Simulation {
      /// <summary>The Simulation.</summary>
      public static Simulation Sim { get; protected set; }
      /// <summary>Number of independent variables at a single position (= number of equations).</summary>
      public int NM { get; internal set; }
      /// <summary>Number of constrained variables at all positions.</summary>
      public int NC { get; protected set; }
      /// <summary>Number of free variables at all positions.</summary>
      public int NF { get; protected set; }
      /// <summary>Primary Dynamics tensor, 4th rank.</summary>
      public Tnr K { get; internal set; }
      /// <summary>Forcing tensor, 2nd rank.</summary>
      public Tnr F { get; internal set; }
      /// <summary>Secondary dynamics tensor, 5th rank.
      /// Node (1), Derivative (2), 1st index of element matrix (3), 2nd index of element matrix (4).</summary>
      public Tnr A { get; internal set; }
      
      /// <summary>Free node values tensor, 2nd rank.</summary>
      public Tnr UF { get; internal set; }
      /// <summary>Constrained node values tensor, 2nd rank.</summary>
      public Tnr UC { get; internal set; }
      public Vec U(Vec dummy, int inx) {
         Vec u = UF[dummy, inx];
         if(u != null)
            return u;
         else return UC[dummy, inx];
      }
      /// <summary>Nodes values tensor, 2nd rank. Returns value at desired index regardless of which tensor it resides in.</summary>
      /// <param name="inxs">A set of indices that extends all the way down to value rank.</param>
      public dbl U(params int[] inxs) {
         dbl u = UF[inxs];
         if(u != 0.0)
            return u;
         else return UC[inxs];
      }
      /// <summary>Secondary F. Third rank participant in the assembly process of F.</summary>
      public Tnr Fs { get; internal set; }
      protected Mesh Mesh { get; }
      /// <summary>2D lists of nodes lying inside a block.</summary>
      protected Dictionary<string,PseudoElement[][]> Patches { get; set; }
      /// <summary>1D lists of nodes shared across blocks.</summary>
      protected Dictionary<string,PseudoElement[]> Joints { get; set; }
      /// <summary></summary>
      protected abstract Dictionary<string,PseudoElement[][]> CreatePatches();
      /// <summary></summary>
      protected abstract Dictionary<string,PseudoElement[]> CreateJoints();
      protected ConjGradsSolver Solver { get; }


      public Simulation() {
         Sim = this;
         Mesh = new Mesh();
      }


      protected void CreatePositions() {                                    R.R("Creating Patches.");
         Patches = CreatePatches();                                     R.R("Creating Joints.");
         Joints = CreateJoints();
         int gInx = 0;                                                  R.R("Adding positions to Mesh.");
         foreach(var patch in Patches.Values) {                         // Global indices on PseudoElements are not yet set. We set them now and add positions to Mesh.
            foreach(var pseudoRow in patch) {
               foreach(var pseudoEmt in pseudoRow) {
                  for(int i = 0; i < pseudoEmt.LInx.Length; ++i) {
                     pseudoEmt.GInx[i] = gInx;
                     Mesh.Pos.Add(pseudoEmt.Pos[i]);
                     ++gInx; } } } }
         foreach(var joint in Joints.Values) {                          // We also set global indices on Joints and add their positions to Mesh.
               foreach(var pseudoEmt in joint) {
                  for(int i = 0; i < pseudoEmt.LInx.Length; ++i) {
                     pseudoEmt.GInx[i] = gInx;
                     Mesh.Pos.Add(pseudoEmt.Pos[i]);
                     ++gInx; } } }
      }
      /// <summary>User must supply custom logic here. The logic here depends on the way the blocks are joined together.</summary>
      protected abstract My.List<Element> CreateElements();
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
      /// <summary>Elements are found in InternalNodeArray.</summary>
      protected KDTree<double,Element> CreateElementTree() {               R.R("Creating a list of Elements.");
         var emts = CreateElements();                                      R.R("Creating a list of Centers of Mass.");
         var coms = CreateCOMs(emts);                                      R.R("Creating KDTree of Elements vs. CoMs positions.");
         emts.TrimExcessSpace();                                           // Trim excess space in array for proper operation of KDTree.
         var emtTree = new KDTree<double,Element>(2, coms, emts._E,
            (r1,r2) => Sqrt(Pow(r2[0]-r1[0], 2.0) + Pow(r2[1]-r1[1], 2.0)));
         return emtTree;
      }
      /// <summary>Create constrained variables at desired positions and also return the number of constrained and free variables.</summary>
      protected abstract (Tnr uc, int nc, int nf) CreateConstrainedVars();
      /// <summary>Creates free values as zeros: an empty tensor.</summary>
      protected virtual Tnr CreateFreeVars() =>
         new Tnr(new List<int> {NF,NM}, NF*NM);
      

      /// <summary>Assemble the dynamics tensor.</summary>
      /// <param name="initDynTnr">Sample Dynamics Tensor with existing entries (e.g.from previous time step).</param>
      /// <param name="emts">Elements list.</param>
      Tnr AssemblePrimaryDynamics(IEnumerable<Element> emts) {
         var tnrK = new Tnr(new Lst{NF,NM,NF,NM}, NF);
         foreach(var emt in emts) {                                                          // Over Elements.
            for(int γ = 0, c = emt.P[γ];  γ < 12;  ++γ, c = emt.P[γ]) {
            for(int δ = 0, d = emt.P[δ];  δ < 12;  ++δ, d = emt.P[δ]) {
            for(int α = 0, a = emt.P[α]; α < 12; ++α, a = emt.P[α]) {                        // Over each e-node. Convert e-index to global index.
            for(int β = 0, b = emt.P[β];  β < 12;  ++β, b = emt.P[β]) {
               for(int p = 0; p < 3; ++p) {
               for(int q = 0; q < 3; ++q) {
               for(int r = 0; r < 3; ++r) {
               for(int s = 0; s < 3; ++s) {
                     Tnr a_ij = A[Tnr.Ex, a,p,r];                                              // Now 2nd rank.
                     Tnr a_ik = A[Tnr.Ex, b,q,s];                                              // 2nd rank.
                     Tnr aa_jk = Tnr.Contract(a_ij, a_ik, 1, 1);                                    // AA now also 2nd rank.
                     foreach(var tnr_aa_k in aa_jk) {                                              // j
                        int j = tnr_aa_k.Key;
                        Vec aa_k = (Vec) tnr_aa_k.Value;
                        foreach(var kv_aa in aa_k) {                                             // k
                           int k = kv_aa.Key;
                           dbl aa = kv_aa.Value;
                           tnrK[c,j,d,k] += emt.Q[3*α+p, 3*β+q, 3*γ+r, 3*δ+s] * aa;     // v is AA[j,k]
         }} }}}} }}}} }
         return tnrK;
      }

      Tnr AssemblePrimaryForcing(IEnumerable<Element> emts) {
         var tnrF = new Tnr(new Lst{NF,NM}, NF);
         foreach(var emt in emts) {
            for(int γ = 0, c = emt.P[γ];  γ < 12;  ++γ, c = emt.P[γ]) {
               Vec fc_j = new Vec(NM, NM);
               for(int α = 0, a = emt.P[α];  α < 12;  ++α, a = emt.P[α]) {
               for(int η = 0, h = emt.P[η];  η < 12;  ++η, h = emt.P[η]) {
                  for(int p = 0; p < 3; ++p) {
                  for(int r = 0; r < 3; ++r) { 
                  for(int s = 0; s < 3; ++s) {
                     var a_ij = A[Tnr.Ex, a,p,r];
                     Vec f_i = Fs[Vec.Ex, h, s];
                     Vec af_j = (Vec) Tnr.Contract(a_ij, f_i, 1, 1);
                     fc_j += emt.T[3*α+p, 3*γ+r, 3*η+s] * af_j;        // First part added.
            
                     for(int β = 0, b = emt.P[β];  β < 12;  ++β, b = emt.P[β]) {       // η is useda as δ now
                        for(int q = 0; q < 3; ++q) {
                           Tnr a_ik = A[Tnr.Ex, b,q,s];
                           Tnr aa_jk = Tnr.Contract(a_ij, a_ik, 1, 1);   // Rank 2 now.
                           Vec u_k = UC[Vec.Ex, h];
                           Vec aau_j = (Vec) Tnr.Contract(aa_jk, u_k, 2, 1);
                           fc_j -= emt.Q[3*α+p, 3*β+q, 3*γ+r, 3*η+s] * aau_j; }} }}} }}   // Second part added.
               tnrF[Vec.Ex, c] += fc_j; }}
         return tnrF;
      }
      /// <summary>Construct new A and Fs </summary>
      /// <param name="nextA">Next step dynamics.</param>
      /// <param name="nextFs">Next step forcing (secondary).</param>
      /// <param name="prevUF">Previous step state.</param>
      protected abstract (Tnr nextA, Tnr nextFs) AdvanceDynamics(Tnr prevUF);

      /// <summary>Find solution value at specified point.</summary><param name="pos">Sought after position.</param><param name="vars">Indices of variables we wish to retrieve.</param>S
      public abstract double[] Solution(in Vec2 pos);
   }
}