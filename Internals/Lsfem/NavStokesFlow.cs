using System;
using System.Collections.Generic;
using dbl = System.Double;
using DA = Fluid.Internals.Numerics.DblArithmetic;
using static Fluid.Internals.Toolbox;

namespace Fluid.Internals.Lsfem {
   using Vec = Fluid.Internals.Collections.Vector<dbl,DA>;
   using Tnr = Fluid.Internals.Collections.Tensor<dbl, DA>;
   using Lst = List<int>;
   public abstract class NavStokesFlow : Simulation {

      /// <summary>Time step.</summary>
      public dbl Dt { get; protected set; }
      /// <summary>Reynolds number.</summary>
      public dbl Re { get; protected set; }


      protected NavStokesFlow() : base() { }
      protected static void Initialize(NavStokesFlow flow, dbl dt, dbl re) {
         flow.Dt = dt;
         flow.Re = re;                                        R.R("Initializing secondary tensors.");
         Simulation.Initialize(flow);
         (flow.A, flow.Fs) = flow.InitializeSecondaries();
      }


      /// <summary>We have to initialize secondary tensors with zeros due to the nature of the optimized algorithm in AdvanceDynamics. It presupposes the tensors being assigned to already exist.</summary>
      private (Tnr initA, Tnr initFs) InitializeSecondaries() {                                // TODO: Fix Tensor so that this is unnecessary.
         var a_bqsik = new Tnr(new Lst{NPos,3,3,NVar,NVar}, NfPos);            // Initialize a_bqsik. 5th rank tensor. It will be returned.
         for(int b = 0; b < NPos; ++b) {
            var a_qsik = new Tnr(a_bqsik, 3);
            a_bqsik[Tnr.VoidTnr, b] = a_qsik;
            for(int q = 0; q < 3; ++q) {
               var a_sik = new Tnr(a_qsik, 3);
               a_qsik[Tnr.VoidTnr, q] = a_sik;
               for(int s = 0; s < 3; ++s) {
                  var a_ik = new Tnr(a_sik, 3);
                  a_sik[Tnr.VoidTnr, s] = a_ik; } } }
         var fs_hsi = new Tnr(new Lst{NPos,3,NVar}, NPos);                   // Initialize fs_hsi, 3rd rank tensor.
         for(int h = 0; h < NPos; ++h) {
            var fs_si = new Tnr(fs_hsi, 3);
            fs_hsi[Tnr.VoidTnr, h] = fs_si;
            for(int s = 0; s < 3; ++s) {
               var fs_i = new Vec(fs_si, 2);
               fs_si[Vec.VoidVector, s] = fs_i; } }
         return (a_bqsik, fs_hsi);
      }
      /// <summary>Construct new A and Fs for the next time step. Assuming no external volume forces.</summary>
      protected override void UpdateDynamicsAndForcing() {
         var positions = Pos;
         dbl dt = Dt;
         dbl re = Re;
         for(int b = 0; b < NPos; ++b) {                                   // For each position.
            Vec u_j = U(Vec.VoidVector, b);                                        // Pick a values vector at position a.
            dbl u = u_j[0],
                v = u_j[1],
                p = u_j[2],
                ω = u_j[3],
                invDt = 1/dt;
            Tnr a_qsik = A[Tnr.VoidTnr, b];
            Tnr a_0sik = a_qsik[Tnr.VoidTnr, 0];                                // A0
            Tnr a_00ik = a_0sik[Tnr.VoidTnr, 0];                                   // A00
            a_00ik[1,0] = invDt;  a_00ik[2,1] = invDt;  a_00ik[3,3] = 1;
            Tnr a_01ik = a_0sik[Tnr.VoidTnr, 1];                                   // A01
            a_01ik[1,0] = 0.5*u;  a_01ik[2,0] = 0.5*v;
            Tnr a_02ik = a_0sik[Tnr.VoidTnr, 2];                                   // A02
            a_02ik[1,1] = 0.5*u;
            a_02ik[2,1] = 0.5*v;
            Tnr a_1sik = a_qsik[Tnr.VoidTnr, 1];                                // A1
            Tnr a_10ik = a_1sik[Tnr.VoidTnr, 0];                                   // A10
            a_10ik[0,0] = 1;  a_10ik[1,0] = 0.5*u;  a_10ik[2,1] = 0.5*u;
            a_10ik[2,3] = -0.5*re; a_10ik[3,1] = -1;
            Tnr a_2sik = a_qsik[Tnr.VoidTnr, 2];                                // A2
            Tnr a_20ik = a_2sik[Tnr.VoidTnr, 0];                                   // A20
            a_20ik[0,1] = 1;  a_20ik[1,0] = v;  a_20ik[1,3] = 0.5*re;
            a_20ik[2,1] = v;  a_20ik[2,2] = 1;  a_20ik[3,0] = 1;
            Tnr fs_si = Fs[Tnr.VoidTnr, b];
            Vec fs_0i = fs_si[Vec.VoidVector, 0];
            fs_0i[1] = u/dt;
            fs_0i[2] = v/dt;
            Vec fs_1i = fs_si[Vec.VoidVector, 1];
            fs_1i[1] = -0.5*p;
            fs_1i[2] = -0.5*ω/re;
            Vec fs_2i = fs_si[Vec.VoidVector, 2];
            fs_2i[1] = -0.5*ω/re;
            fs_2i[2] = -0.5*p;
         }
      }
   }
}