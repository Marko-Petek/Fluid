using System;
using System.Text;
using Fluid.Internals.Development;
using Fluid.Internals.Numerics;

using static Fluid.Internals.Development.AppReporter;

namespace Fluid.Internals
{
    public static class Toolbox
    {
        public static FileReader FileReader { get; } = new FileReader();
        public static FileWriter FileWriter { get; } = new FileWriter();
        public static Console Console { get; } = new Console();
        public static AppReporter Reporter { get; } = new AppReporter(VerbositySettings.Moderate);
        public static Rng Rng { get; } = new Rng();

        static Toolbox() {
            
        }
    }
}