using System;
using System.IO;
using static System.Console;
using static Fluid.Dynamics.Internals.AppReporter.SettingsEnum;

namespace Fluid.Dynamics.Internals
{
    /// <summary>Writes program's progression either to a file, to console or both.</summary>
    public static class AppReporter
    {
        [Flags]
        public enum SettingsEnum
        {
            WritesToConsole = 1 << 0,
            WritesToFile    = 1 << 1
        }

        /// <summary>Initialize AppReporter as set to write to console and file.</summary>
        static AppReporter() {
            Settings = new SettingsEnum();
            Settings |= WritesToConsole;
            Settings |= WritesToFile;
            Writer = new StreamWriter(path: new FileInfo("log.txt").FullName, append: true);
        }


        /// <summary>Determines Reporter's output channel: console or file or both.</summary>
        public static SettingsEnum Settings { get; set; }

        static StreamWriter Writer { get; }

        public static void Report(string str) {

            if((Settings & WritesToConsole) == WritesToConsole) {
                WriteLine(str);
            }
            
            if((Settings & WritesToFile) == WritesToFile) {
                
            }
        }
    }

    
}