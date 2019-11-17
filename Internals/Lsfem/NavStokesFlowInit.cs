#nullable enable
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
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
using V = Voids<dbl,DA>;
using Lst = List<int>;
using VecLst = My.List<Vec2>;
using PE = PseudoElement;
public class NavStokesFlowInit : ISimInit {
   /// <summary>A cartesian array of positions. [i][j][{x,y}]</summary>
   dbl [][][] OrdPos { get; set; }

   public NavStokesFlowInit() {
      OrdPos = ReadPos();              R("Read element corner positions from file.");
   }


   /// <summary>We have to initialize secondary tensors with zeros due to the nature of the optimized algorithm in AdvanceDynamics. It presupposes the tensors being assigned to already exist.</summary>
   public (Tnr initA, Tnr initFs) InitializeSecondaries(int nPos, int nfPos, int nVar) {                                // TODO: Fix Tensor so that this is unnecessary.
      var a_bqsik = new Tnr(new Lst{nPos,3,3,nVar,nVar}, nfPos);            // Initialize a_bqsik. 5th rank tensor. It will be returned.
      for(int b = 0; b < nPos; ++b) {
         var a_qsik = new Tnr(a_bqsik, 3);
         a_bqsik[V.Tnr, b] = a_qsik;
         for(int q = 0; q < 3; ++q) {
            var a_sik = new Tnr(a_qsik, 3);
            a_qsik[V.Tnr, q] = a_sik;
            for(int s = 0; s < 3; ++s) {
               var a_ik = new Tnr(a_sik, 3);
               a_sik[V.Tnr, s] = a_ik; } } }
      var fs_hsi = new Tnr(new Lst{nPos,3,nVar}, nPos);                   // Initialize fs_hsi, 3rd rank tensor.
      for(int h = 0; h < nPos; ++h) {
         var fs_si = new Tnr(fs_hsi, 3);
         fs_hsi[V.Tnr, h] = fs_si;
         for(int s = 0; s < 3; ++s) {
            var fs_i = new Vec(fs_si, 2);
            fs_si[V.Vec, s] = fs_i; } }
      return (a_bqsik, fs_hsi);
   }

   dbl[][][] ReadPos() {
      string dir = @"Seminar/Mathematica";
      string name = @"nodesC";
      string ext = @".txt";
     T.FileReader.SetDirAndFile(dir, name, ext);                      // Read the points to an array.
      var hierarchy =T.FileReader.ReadHierarchy<dbl>();
      var convRes = hierarchy.ConvertToArray();
      return convRes.success switch {
         true => convRes.array switch {
            dbl[][][] pos => pos,
            _ => throw new InvalidCastException("Positions array could not be cast to a rank 3 double array.") },
         _ => throw new FileNotFoundException("Could not locate nodes' position data.", $"{dir}/{name}{ext}") };
   }
}
}
#nullable restore