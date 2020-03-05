using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

using Fluid.Internals.Collections;
using static Fluid.Internals.Development.Reporter;
namespace Fluid.Internals.IO {
   
public class FileReader : FileRWBase, IDisposable {
   public const string DefaultDirPath = "";
   public const string DefaultFileName = "input";
   public const string DefaultExt = ".txt";
   StreamReader Reader { get; set; }

   public FileReader(string dirPath = DefaultDirPath, string fileNameNoExt = DefaultFileName,
   string fileExt = DefaultExt) : base(dirPath, fileNameNoExt, fileExt) {
      Reader = new StreamReader(File.FullName, Encoding.UTF8);
   }

   protected override void ResetUnmanagedResource() {
      Reader?.Dispose();                                                  // Dispose old reader if it exists.
      Reader = new StreamReader(File.FullName, Encoding.UTF8);
   }
   public Hierarchy<τ> ReadHierarchy<τ>()
   where τ : IEquatable<τ>, new() {
      OnSettingsChanged();
      return IO.ReadHierarchy<τ>(Reader) switch {
         Hierarchy<τ> hier => hier,
         _ => throw new Exception("Could not read hierarchy from file.") };
   }
   public Array ReadArray<τ>()
   where τ : IEquatable<τ>, new() {
      Array? arr = ReadHierarchy<τ>().ConvertToArray();
      if(arr != null)
         return arr;
      else
         throw new IOException("Failure converting hierarchy to array.");
   }
   public void Dispose() {
      Reader.Dispose();
      GC.SuppressFinalize(this);
   }
   ~FileReader() => Dispose();
}
}