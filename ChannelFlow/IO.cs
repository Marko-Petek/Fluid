using System;
using System.IO;
using System.Globalization;
using Fluid.Internals.Meshing;
using Fluid.Internals.Numerics;
using static System.Math;

namespace Fluid.ChannelFlow
{
    public static class IO
    {
        /// <summary>Write values of velocity at uniformly distributed sampling points. We can specify sampling density.</summary><param name="mesh">ChannelMesh which contains solution.</param><param name="samplingDensity">Linear sampling density. Number of segments per unit square side.</param>
        public static void WriteVelocityField(this ChannelMesh mesh, int samplingDensity) {
            // {{{x1, y1}, {u1, v1}}, {{x2, y2}, {u2, v2}}, ...}
            string fileName = "ChannelFlow/Results/velocityField.txt";
            var file = new FileInfo(fileName);
            double width = 4 * mesh.GetWidth();                             // Width here = length of channel.
            double height = mesh.GetWidth();                                // Height here = width of channel.
            double segmentLength =  height / (20.0 * samplingDensity);        // Determine segment length.
            double[] v;
            Pos pos = new Pos();                                            // Current position.
            
            using(var sw = new StreamWriter(file.FullName)) {
                sw.WriteLine("{");
                v = mesh.Solution(ref pos, 0, 1);
                if(!Double.IsNaN(v[0]))
                    sw.Write($"{{{pos.ToString()}, {{{v[0].ToString()},{v[1].ToString()}}}}}");

                while(pos._y < height) {
                    pos._x = 0.0;

                    while(pos._x < width) {
                        pos._x += segmentLength;
                        v = mesh.Solution(ref pos, 0, 1);

                        if(!Double.IsNaN(v[0]))                                                               // Write only values inside domain.
                            sw.Write($", {{{pos.ToString()}, {{{v[0].ToString()},{v[1].ToString()}}}}}");
                    }
                    pos._y += segmentLength;
                }
                sw.Write("}");
            }
        }

        public static void WriteVelocityMagnitudeField(this ChannelMesh mesh) {
            /*
                {
                    {sqrt(u_11^2 + v_11^2), sqrt(u_12^2 + v_12^2), ..., sqrt(u_1n^2 + v_1n^2)},
                    {sqrt(u_21^2 + v_21^2), sqrt(u_22^2 + v_22^2), ..., sqrt(u_2n^2 + v_2n^2)},
                    ...
                    {sqrt(u_nn^2 + v_nn^2), sqrt(u_nn^2 + v_nn^2), ..., sqrt(u_nn^2 + v_nn^2)}
                }
            */
            string fileName = "Results/velocityMagnitudeField.txt";
            var file = new FileInfo(fileName);
            int nodeCount = mesh.Nodes.Length;
            
            using(var sw = new StreamWriter(file.FullName)) {
                sw.Write("{");                                              // Outer-most opening braces.
                for(int i = 0; i < nodeCount - 1; ++i) {

                }
                sw.Write("}");                                              // Outer-most closing braces.

                string Magnitude(int node) {
                    ref var nodeRef = ref mesh.Node(node);
                    double magnitude = Sqrt(Pow(nodeRef.Var(0)._value, 2) + Pow(nodeRef.Var(1)._value, 2));
                    return magnitude.ToString(CultureInfo.InvariantCulture);
                }
            }


            
        }

        public static void WritePressureField(this ChannelMesh channelMesh) {
            /*
                {
                    {p_11, p_12, ..., p_1n},
                    {p_21, p_22, ..., p_2n},
                    ...
                    {p_n1, p_n2, ..., p_nn}
                }
            */
        }
    }
}