using System;
using Fluid.Internals.Collections;

namespace Fluid.Internals.Numerics
{
    /// <summary>Solves linear systems of form Ax=b.</summary>
    public class ConjugateGradients
    {
        readonly SparseMatrix<double> _A;
        readonly SparseRow<double> _b;
        //SparseRow<double> _x;

        /// <summary>Create an iterative linear system solver that uses method of conjugate gradients. Systems are of form: A x = b, where solution sought after is x.</summary><param name="A">Stiffness matrix.</param><param name="b">Forcing vector.</param>
        public ConjugateGradients(SparseMatrix<double> A, SparseRow<double> b) {
            _A = A;
            _b = b;
        }

        /// <summary>Finds and returns solution of system as a SparseRow.</summary><param name="initialGuess">A SparseRow with same width as solution. Provides starting point in phase space.</param><param name="maximumResidual">Value of residual at which iterative solution process stops.</param>
        public SparseRow<double> GetSolution(SparseRow<double> initialGuess, double maximumResidual) {
            var d = new SparseRow<double>[2] {_b - _A * initialGuess, new SparseRow<double>(_b.Width, 10)};
            var r = new SparseRow<double>[2] {d[0], new SparseRow<double>(_b.Width, 10)};
            var x = new SparseRow<double>[2] {new SparseRow<double>(d[0]), new SparseRow<double>(_b.Width, 10)};
            double alfa;
            double beta;
            SparseRow<double> ADotDi;
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
            while(r[i].CalcNorm() > maximumResidual);

            return x[i];
        }
    }
}