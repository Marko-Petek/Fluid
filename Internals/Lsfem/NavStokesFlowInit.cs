#nullable enable
using System.Collections.Generic;
using dbl = System.Double;
using DA = Fluid.Internals.Numerics.DblArithmetic;
namespace Fluid.Internals.Lsfem {
using Vec = Fluid.Internals.Collections.Vector<dbl,DA>;
using Tnr = Fluid.Internals.Collections.Tensor<dbl, DA>;
using V = Voids<dbl,DA>;
using Lst = List<int>;
using PE = PseudoElement;

public abstract class NavStokesFlowInit : ISimInit {
   public int NVar { get; }
   /// <summary>Time step.</summary>
   public dbl Dt { get; protected set; }
   /// <summary>Reynolds number.</summary>
   public dbl Re { get; protected set; }


   public NavStokesFlowInit(dbl dt, dbl re) {
      NVar = 4;
      Dt = dt;
      Re = re;
   }


   /// <summary>We have to initialize secondary tensors with zeros due to the nature of the optimized algorithm in AdvanceDynamics. It presupposes the tensors being assigned to already exist.</summary>
   public (Tnr initA, Tnr initFs) InitializeSecondaries(int nPos, int nfPos) {                                // TODO: Fix Tensor so that this is unnecessary.
      var a_bqsik = new Tnr(new Lst{nPos,3,3,NVar,NVar}, nfPos);            // Initialize a_bqsik. 5th rank tensor. It will be returned.
      for(int b = 0; b < nPos; ++b) {
         var a_qsik = new Tnr(a_bqsik, 3);
         a_bqsik[V.Tnr, b] = a_qsik;
         for(int q = 0; q < 3; ++q) {
            var a_sik = new Tnr(a_qsik, 3);
            a_qsik[V.Tnr, q] = a_sik;
            for(int s = 0; s < 3; ++s) {
               var a_ik = new Tnr(a_sik, 3);
               a_sik[V.Tnr, s] = a_ik; } } }
      var fs_hsi = new Tnr(new Lst{nPos,3,NVar}, nPos);                   // Initialize fs_hsi, 3rd rank tensor.
      for(int h = 0; h < nPos; ++h) {
         var fs_si = new Tnr(fs_hsi, 3);
         fs_hsi[V.Tnr, h] = fs_si;
         for(int s = 0; s < 3; ++s) {
            var fs_i = new Vec(fs_si, 2);
            fs_si[V.Vec, s] = fs_i; } }
      return (a_bqsik, fs_hsi);
   }
   /// <summary>A cartesian array of positions. [i][j][{x,y}]</summary>
   public abstract Dictionary<string, dbl[][][]> GetRawOrderedPosData();
   /// <summary>2D lists of nodes lying inside a block. Return the "one after last" node index.</summary>
   /// <param name="ordPos">A cartesian array of positions. [i][j][{x,y}]</param>
   public abstract (int newCurrGInx, Dictionary<string,PE[][]>) CreatePatches(Dictionary<string, dbl[][][]> ordPos);
   /// <summary>1D lists of nodes shared across blocks.</summary>
   /// <param name="currGInx">The "one after last" index returned by CreatePatches method.</param>
   /// <param name="ordPos">A cartesian array of positions. [i][j][{x,y}]</param>
   public abstract Dictionary<string,PE[]> CreateJoints(int currGInx, Dictionary<string, dbl[][][]> ordPos);
   /// <summary>Custom logic here depends on the way the blocks are joined together. Do not forget to trim excess space.</summary>
   public abstract Element[] CreateElements(
   Dictionary<string,PE[][]> patches, Dictionary<string,PE[]> joints);
   /// <summary>Create constrained variables at desired positions.</summary>
   public abstract (Tnr uc, Constraints bits) CreateConstraints(
   Dictionary<string,PE[][]> patches, Dictionary<string,PE[]> joints, int nPos, int nVar);
}
}
#nullable restore