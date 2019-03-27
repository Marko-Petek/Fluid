using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using Fluid.Internals.Collections;
using static Fluid.Internals.Development.AppReporter;

namespace Fluid.Internals.IO
{
    /// <summary>Contains methods which write nicely formatted values to hard drive. You have to call Flush() manually if you want to immediatelly 
    /// see results written on HD (empty RAM buffer to HD).</summary>
    public partial class FileWriter : FileRWBase, IDisposable {
        public void Write<T>(Hierarchy<T> hier) {
            OnSettingsChanged();
            IO.Write(hier, Writer);
        }

        public void WriteLine<T>(Hierarchy<T> hier) {
            OnSettingsChanged();
            IO.WriteLine(hier, Writer);
        }
    }
}