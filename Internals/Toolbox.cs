using System;
using System.Text;
using Fluid.Internals.Development;

using static Fluid.Internals.Development.AppReporter;

namespace Fluid.Internals
{
    public static class Toolbox
    {
        public static HardDrive Drive { get; } = new HardDrive();
        public static AppReporter Reporter { get; } = new AppReporter(VerbositySettings.Moderate);

        static Toolbox() {
            Console.OutputEncoding = Encoding.UTF8;
        }
    }
}