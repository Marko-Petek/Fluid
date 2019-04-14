using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

using Fluid.Internals.Collections;
using static Fluid.Internals.Development.AppReporter;

namespace Fluid.Internals.IO {
   public class FileReader : FileRWBase, IDisposable {
      StreamReader Reader { get; set; }

      public FileReader(string dirPath = DefaultDirPath, string fileNameNoExt = DefaultFileName,
         string fileExt = DefaultExt) : base(dirPath, fileNameNoExt, fileExt) {
            OnSettingsChanged();
      }

      protected override void ResetUnmanagedResource() {
         Reader?.Dispose();                                                  // Dispose old reader if it exists.
         Reader = new StreamReader(File.FullName, Encoding.UTF8);
      }
      public Hierarchy<T> ReadHierarchy<T>() {
         OnSettingsChanged();
         return IO.ReadHierarchy<T>(Reader);
      }
      public Array ReadArray<T>() {
         OnSettingsChanged();
         (var succ, var arr) = IO.ReadHierarchy<T>(Reader).ConvertToArray();
         if(succ)
            return arr;
         else
            throw new IOException("Failure when reading array.");
      }
      public void Dispose() {
         Reader.Dispose();
         GC.SuppressFinalize(this);
      }
      ~FileReader() => Dispose();
   }
}