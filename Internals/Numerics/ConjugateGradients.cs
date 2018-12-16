using System;
using Fluid.Internals.Collections;

namespace Fluid.Internals.Numerics
{
    /// <summary>Solves linear systems of form Ax=b.</summary>
    public class ConjugateGradients
    {
        readonly SparseMatDouble _A;
        readonly SparseRowDouble _b;
        //SparseRow<double> _x;

        /// <summary>Create an iterative linear system solver that uses method of conjugate gradients. Systems are of form: A x = b, where solution sought after is x.</summary><param name="A">Stiffness matrix.</param><param name="b">Forcing vector.</param>
        public ConjugateGradients(SparseMatDouble A, SparseRowDouble b) {
            _A = A;
            _b = b;
        }

        /// <summary>Finds and returns solution of system as a SparseRow.</summary><param name="initialGuess">A SparseRow with same width as solution. Provides starting point in phase space.</param><param name="maximumResidual">Value of residual at which iterative solution process stops.</param>
        public SparseRowDouble GetSolution(SparseRowDouble initialGuess, double maximumResidual) {
            var d = new SparseRowDouble[2] {_b - _A * initialGuess, new SparseRowDouble(_b.Width, 10)};
            var r = new SparseRowDouble[2] {d[0], new SparseRowDouble(_b.Width, 10)};
            var x = new SparseRowDouble[2] {new SparseRowDouble(d[0]), new SparseRowDouble(_b.Width, 10)};
            double alfa;
            double beta;
            SparseRowDouble ADotDi;
            double riDotRi;

            int i = 0;
            int j = 1;

            do {
                riDotRi = r[i] * r[i];
                ADotDi = _A * d[i];
                alfa = riDotRi / (d[i] * ADotDi);
                x[j] = x[i] + alfa * d[i];
                r[j] = r[i] - alfa * ADotDi;
                beta = (r[j] * r[j]) / (riDotRi);
                d[j] = r[j] + beta * d[i];

                i = (i + 1) % 2;
                j = (j + 1) % 2;
            }
            while(r[i].NormSqr() > maximumResidual);

            return x[i];
        }
    }
}