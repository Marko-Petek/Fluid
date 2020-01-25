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

using Vec = Fluid.Internals.Collections.Vector<dbl,DA>;
using Tnr = Fluid.Internals.Collections.Tensor<dbl, DA>;
using Lst = List<int>;
using VecLst = My.List<Vec2>;
using PE = PseudoElement;

/// <summary>Most general aspects of LSFEM.</summary>
public abstract class Sim {
   /// <summary>Location of lowest left point.</summary>
   public Vec2 LL { get; }
   /// <summary>Location of upper-most right point.</summary>
   public Vec2 UR { get; }
   /// <summary>An array of all positions.</summary>
   public Vec2[] Pos { get; protected set;}
   /// <summary>A mapping that takes an element (center of mass) into indices of nodes that belong to an element.</summary>
   public KDTree<double,Element> EmtTree { get; set; }
   /// <summary>Number of independent variables at a single position (= number of equations).</summary>
   public int NVar { get; internal set; }
   /// <summary>Number of all positions.</summary>
   public int NPos { get; protected set; }
   /// <summary>Number of positions with free variables. Used to determine solution tensor capacity.</summary>
   public int NfPos { get; protected set; }
   // /// <summary>Primary Dynamics tensor, 4th rank. Includes only free variables.</summary>
   // public Tnr K { get; internal set; }
   // /// <summary>Forcing tensor, 2nd rank.</summary>
   // public Tnr F { get; internal set; }
   /// <summary>Secondary dynamics tensor, 5th rank. Includes both, free and constrained variables.
   /// Node (1), Derivative (2), 1st index of element matrix (3), 2nd index of element matrix (4).</summary>
   public Tnr A { get; internal set; }
   /// <summary>Free variables tensor, 2nd rank. There can be free and constrained variables at a single position.</summary>
   public Tnr UF { get; internal set; }
   /// <summary>Constrained variables tensor, 2nd rank. There can be free and constrained variables at a single position.</summary>
   public Tnr UC { get; internal set; }
   /// <summary>A sequence of bits that indicate constrainedness of each variable: (i,j) => NVar*i + j. Therefore the BitArray holds NPos*NVar bits.</summary>
   protected Constraints Cnstr { get; set; }
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


   /// <summary>Create a Simulation ready to run.</summary>
   /// <param name="patches"></param>
   /// <param name="joints">1D lists of nodes shared across blocks.</param>
   /// <param name="pos">An unordered (listed in sequence) array of positions.</param>
   /// <param name="emtTree">A mapping that takes an element (center of mass) into indices of nodes that belong to an element.</param>
   /// <param name="uC">Constrained variables tensor, 2nd rank.</param>
   /// <param name="nfPos">Number of positions with free variables.</param>
   public Sim(ISimInit si) {                                            R("Started Sim Initialization.");
      Dictionary<string, dbl[][][]> ordPos;
      (LL, UR, ordPos) = si.GetRawOrderedPosData();                     R("Read ordered lists of positions.");
      var (newCurrGInx, patches) = si.CreatePatches(ordPos);            R("Created Patches.");
      var joints = si.CreateJoints(newCurrGInx, ordPos);                R("Created Joints.");
      (NPos, Pos) = CreatePositions(patches, joints);                   R("Created Positions.");
      var emts = si.CreateElements(patches, joints);                    R("Created Elements and calculated overlap integrals.");
      var coms = CreateCOMs(emts);                                      R("Created a list of Centers of Mass.");
      EmtTree = CreateElementTree(coms, emts);                          R("Created the ElementTree.");
      (UC, Cnstr) = si.CreateConstraints(patches, joints, NPos, NVar);  R("Created constraints.");
      NfPos = CountFreePositions(NPos, NVar);                           R("Counted free positions.");
      UF = new Tnr(new List<int> {NfPos,NVar}, NfPos);                  R("Created the free variables tensor as zeroes.");
      (A, Fs) = si.InitializeSecondaries(NPos, NfPos);                  R("Initialized secondary tensors.");
   }
   (int nPos, Vec2[]) CreatePositions(Dictionary<string,PE[][]> patches, Dictionary<string,PE[]> joints) {
      int approxNPos = patches.Sum( patch => patch.Value.Length ) * 5 +
         joints.Sum( joint => joint.Value.Length ) * 3;                    // Estimate for the number of positions so that we can allocate an optimal amount of space for the posList.
      var posList = new My.List<Vec2>(approxNPos);
      int gInx = 0;                                                        R("Adding positions to Mesh.");
      foreach(var patch in patches.Values) {                               // Global indices on PseudoElements are not yet set. We set them now and add positions to Mesh.
         foreach(var pseudoRow in patch) {
            foreach(var pseudoEmt in pseudoRow) {
               for(int i = 0; i < pseudoEmt.PEInxs.Length; ++i) {
                  pseudoEmt.GInxs[i] = gInx;
                  posList.Add(pseudoEmt.Poss[i]);
                  ++gInx; } } } }
      foreach(var joint in joints.Values) {                                // We also set global indices on Joints and add their positions to Mesh.
            foreach(var pseudoEmt in joint) {
               for(int i = 0; i < pseudoEmt.PEInxs.Length; ++i) {
                  pseudoEmt.GInxs[i] = gInx;
                  posList.Add(pseudoEmt.Poss[i]);
                  ++gInx; } } }
      posList.TrimExcessSpace();
      return (posList.Count, posList._E);
   }
   /// <summary>Create the list of COMs after the Elements are created.</summary>
   double[][] CreateCOMs(IList<Element> elements) {
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
   KDTree<double,Element> CreateElementTree(dbl[][] coms, Element[] emts) {               R("Creating KDTree of Elements vs. CoMs positions.");
      var emtTree = new KDTree<double,Element>(2, coms, emts,
         (r1,r2) => Sqrt(Pow(r2[0]-r1[0], 2.0) + Pow(r2[1]-r1[1], 2.0)));
      return emtTree;
   }
   /// <summary>Count positions containing at least one free variable.</summary>
   /// <param name="nPos">Number of positions.</param>
   /// <param name="nVar">Number of variables at a position.</param>
   int CountFreePositions(int nPos, int nVar) {
      int nFreePos = 0;
      int n = nVar*nPos;
      bool freePos;                          // This will signify whether a position is free after the j-loop.
      for(int i = 0; i < nPos; ++i) {
         freePos = false;
         for(int j = 0; j < nVar; ++j) {
            if(!Cnstr[i,j]) {               // If there is a single free position, then the position is free.
               freePos = true;
               break; } }
         if(freePos) {                       // At least one free variable at position.
            ++nFreePos; } }
      return nFreePos;
   }                
   

   /// <summary>Takes updated secondary (dynamics and forcing) tensors and constrained node variables and creates primary tensors which are ready to be passed to the Solver. This process iterates over all Elements (surfaces) and adds their contribution to corresponding nodes (points).</summary>
   /// <param name="emts">A collection of Elements, each element containing overlap integrals of node functions and a mapping from eNodes to gNodes.</param>
   (Tnr K, Tnr F) AssemblePrimaries(IEnumerable<Element> emts) {
      var tnrK = new Tnr(new Lst{NPos,NVar,NPos,NVar}, NfPos);                    // Create a 4th rank result tensor.
      var tnrF = new Tnr(new Lst{NPos,NVar}, NfPos);                            // Create a 2nd rank result tensor.
      Lst allJs = Enumerable.Range(0,NVar).ToList();
      foreach(var emt in emts) {
      for(int γ = 0, c = emt.P[γ];  γ < 12;  ++γ, c = emt.P[γ]) {
         Vec vecF_j = tnrF[Vec.V, c];
         var jfs = allJs.Where( j => !Cnstr[c,j] ).ToArray();                      // Determine which variables (c,j) are free.
         for(int α = 0, a = emt.P[α];  α < 12;  ++α, a = emt.P[α]) {
         for(int p = 0; p < 3; ++p) {
         for(int r = 0; r < 3; ++r) {
            Tnr a_ij = A[Tnr.T, a,p,r];                                            // Precursor for all terms.
            for(int β = 0, b = emt.P[β];  β < 12;  ++β, b = emt.P[β]) {
               Tnr f_si = Fs[Tnr.T, b];
               for(int s = 0; s < 3; ++s) {                                         // Index for A*Fs_j
                  Vec fs_i = f_si[Vec.V, s];
                  Vec afs_j = (Vec) Tnr.Contract(a_ij, fs_i, 1, 1);                 // Prepare for first term of primary forcing.
                  foreach(var j in allJs)
                     vecF_j[j] += emt.T[3*α+r, 3*γ+p, 3*β+s] * afs_j[j];
                  for(int q = 0; q < 3; ++q) {
                     Tnr a_ik = A[Tnr.T, b,q,s];                                   // For second term in primary forcing.
                     Tnr aa_jk = Tnr.Contract(a_ij, a_ik, 1, 1);                    // Precursor for first term in PD and second term in PF.
                     for(int δ = 0, d = emt.P[δ]; δ < 12; ++δ, d = emt.P[δ]) {
                        var grpsK = allJs.GroupBy( j => Cnstr[c,j] ).ToArray();
                        var kfs = grpsK.Where( grp => grp.Key == false ).
                           Single().ToArray();                                      // Determine which variables (c,j) are free and which constrained.
                        var kcs = grpsK.Where( grp => grp.Key == true ).
                           Single().ToArray();
                        dbl coefQ = emt.Q[3*α+r, 3*β+s, 3*γ+p, 3*δ+q];
                        foreach(var jf in jfs) {
                           Vec aa_k = aa_jk[Vec.V,jf];
                           foreach(int k in kfs)
                              tnrK[c,jf,d,k] += coefQ * aa_k[k];
                           foreach(int l in kcs)
                              vecF_j[jf] -= coefQ * aa_k[l] * UC[d,l];
         } } } } } } } } } }
      return (tnrK, tnrF);
   }
   /// <summary>Update secondary tensors (dynamics A and forcing Fs) from the current state of the variable field U. These secondaries will be used in the assembly process of primary tensors. This code is case-dependent (on the system of PDE) and has to be provided by library user. See NavStokesSim.cs for an example.</summary>
   protected abstract (Tnr modA, Tnr modFs) AdvanceSecondaries(Tnr a, Tnr fs);

}
}