#nullable enable
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using SCG = System.Collections.Generic;
using Fluid.Internals.Collections;
using static Fluid.Internals.Development.Reporter;
namespace Fluid.Internals.IO {

/// <summary>Contains methods which write nicely formatted values to hard drive. You have to call Flush() manually if you want to immediatelly 
/// see results written on HD (empty RAM buffer to HD).</summary>
public partial class FileWriter : FileRWBase, IDisposable {
   public void Write<T>(SCG.IEnumerable<T> ienum) {
      OnSettingsChanged();
      IO.Write(ienum, Writer);
   }
   public void WriteLine<T>(SCG.IEnumerable<T> ienum) {
      OnSettingsChanged();
      IO.WriteLine(ienum, Writer);
   }
}
}
#nullable restore