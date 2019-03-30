using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

using Fluid.Internals.Collections;
using static Fluid.Internals.Development.AppReporter;

namespace Fluid.Internals.IO
{
   public class FileReader : FileRWBase, IDisposable
   {
      StreamReader _Reader;
      StreamReader Reader => _Reader;


      protected override void ResetUnmanagedResource() {
         Reader?.Dispose();                                                  // Dispose old reader if it exists.
         _Reader = new StreamReader(File.FullName, Encoding.UTF8);
      }


      public FileReader(string dirPath = DefaultDirPath, string fileNameNoExt = DefaultFileName, string fileExt = DefaultExt) :
      base(dirPath, fileNameNoExt, fileExt) {
         OnSettingsChanged();
      }


      // public double[] ReadDoubleArray1d() {
      //    OnSettingsChanged();
      //    return IO.ReadDoubleArray1d(Reader);
      // }

      public Hierarchy<T> ReadHierarchy<T>() {
         OnSettingsChanged();
         return IO.ReadHierarchy<T>(Reader);
      }

      // public Hierarchy<int> ReadHierarchyInt() {
      //    OnSettingsChanged();
      //    return IO.ReadHierarchyInt(Reader);
      // }


      public void Dispose() {
         Reader.Dispose();
         GC.SuppressFinalize(this);
      }

      ~FileReader() => Dispose();
   }
}