using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using static Fluid.Internals.Development.Reporter;
namespace Fluid.Internals.IO {

/// <summary>Contains methods which write nicely formatted values to hard drive. You have to call Flush() manually if you want to immediatelly 
/// see results written on HD (empty RAM buffer to HD).</summary>
public partial class FileWriter : FileRWBase, IDisposable {
   public const string DefaultDirPath = "";
   public const string DefaultFileName = "output";
   public const string DefaultExt = ".txt";
   StreamWriter Writer { get; set; }
   /// <summary>Whether Writer should append to currently set file or overwrite it.</summary>
   public bool Append { get; protected set; }
   public const bool DefaultAppend = false;

   #nullable disable // It cannot detect Streamwriter being initialized.
   /// <summary>Writes data to hard drive.</summary>
   public FileWriter(string dirPath = DefaultDirPath, string fileNameNoExt = DefaultFileName,
      string fileExt = DefaultExt, bool append = DefaultAppend) :
      base(dirPath, fileNameNoExt, fileExt) {
         Append = append;
         OnSettingsChanged();    // StreamWriter is initialized in here via ResetUnmanagedResource() override implemented below.
   }
   #nullable restore

   protected override void ResetUnmanagedResource() {
      Writer?.Dispose();                                      // Dispose old writer if it exists.
      Writer = new StreamWriter(File.FullName, Append);
   }
   void AppendChanged(object? sender, EventArgs e) { }
   // dirPath can be relative or absolute
   public void SetFile(string fileNameNoExt, string fileExt, bool append) {
      base.SetFile(fileNameNoExt, fileExt);
      Append = append;
      SettingsChanged += AppendChanged;
   }
   public void SetFile(string fileNameNoExt, bool append) =>
      SetFile(fileNameNoExt, FileExt, append);
   public void SetFile(string fileNameNoExt) => SetFile(fileNameNoExt, Append);
   // dirPath can be relative or absolute
   public void SetDirAndFile(string dirPath, string fileNameNoExt, string fileExt,
      bool append = false) {
         SetDir(dirPath);
         SetFile(fileNameNoExt, fileExt, append);
   }
   /// <summary>Immediatelly write any RAM buffered data to hard drive.</summary>
   public void Flush() => Writer.Flush();
   /// <summary>Writes a 1D array to hard drive.</summary><param name="array1d"> 1D array to write to HD.</param>
   public void Write<T>(T[] array1d) {
      OnSettingsChanged();
      Writer.Write<T>(array1d);
   }
   /// <summary>Writes a 1D array to hard drive.</summary><param name="array1d"> 1D array to write to HD.</param>
   public void Write<T>(T[] array1d, string fileNameNoExt) {
      SetFile(fileNameNoExt);
      Write(array1d);
   }
   /// <summary>Writes a 2D array to hard drive.</summary><param name="array1d">2D array to write to HD.</param>
   public void Write<T>(T[][] array2d) {
      OnSettingsChanged();
      Writer.Write<T>(array2d);
   }
   /// <summary>Writes a 2D array to hard drive.</summary><param name="array1d">2D array to write to HD.</param>
   public void Write<T>(T[][] array2d, string fileNameNoExt) {
      SetFile(fileNameNoExt);
      Write(array2d);
   }
   /// <summary>Writes a 3D array to hard drive.</summary><param name="array3d">3D array to write to HD.</param>
   public void Write<T>(T[][][] array3d) {
      OnSettingsChanged();
      Writer.Write<T>(array3d);
   }
   /// <summary>Writes a 3D array to hard drive.</summary><param name="array3d">3D array to write to HD.</param>
   public void Write<T>(T[][][] array3d, string fileNameNoExt) {
      SetFile(fileNameNoExt);
      Write(array3d);
   }
   /// <summary>Writes a 4D array to hard drive.</summary><param name="array4d">4D array to write to HD.</param>
   public void Write<T>(T[][][][] array4d) {
      OnSettingsChanged();
      Writer.Write<T>(array4d);
   }
   /// <summary>Writes a 4D array to hard drive.</summary><param name="array4d">4D array to write to HD.</param>
   public void Write<T>(T[][][][] array4d, string fileNameNoExt) {
      SetFile(fileNameNoExt);
      Write(array4d);
   }
   /// <summary>Writes a 5D array to hard drive.</summary><param name="array6d">5D array to write to HD.</param>
   public void Write<T>(T[][][][][] array5d) {
      OnSettingsChanged();
      Writer.Write<T>(array5d);
   }
   /// <summary>Writes a 5D array to hard drive.</summary><param name="array6d">5D array to write to HD.</param>
   public void Write<T>(T[][][][][] array5d, string fileNameNoExt) {
      SetFile(fileNameNoExt);
      Write(array5d);
   }
   /// <summary>Writes a 6D array to hard drive.</summary><param name="array6d">6D array to write to HD.</param>
   public void Write<T>(T[][][][][][] array6d) {
      OnSettingsChanged();
      Writer.Write<T>(array6d);
   }
   /// <summary>Writes a 6D array to hard drive.</summary><param name="array6d">6D array to write to HD.</param>
   public void Write<T>(T[][][][][][] array6d, string fileNameNoExt) {
      SetFile(fileNameNoExt);
      Write(array6d);
   }
   /// <summary>Write a 1D array to hard drive and append a NewLine.</summary><param name="array1d"> 1D array to write to HD.</param>
   public void WriteLine<T>(T[] array1d) {
      OnSettingsChanged();
      Writer.WriteLine<T>(array1d);
   }
   /// <summary>Write a 1D array to hard drive and append a NewLine.</summary><param name="array1d"> 1D array to write to HD.</param>
   public void WriteLine<T>(T[] array1d, string fileNameNoExt) {
      SetFile(fileNameNoExt);
      WriteLine(array1d);
   }
   /// <summary>Write a 2D array to hard drive and append a NewLine.</summary><param name="array2d"> 2D array.</param>
   public void WriteLine<T>(T[][] array2d) {
      OnSettingsChanged();
      Writer.WriteLine<T>(array2d);
   }
   /// <summary>Write a 2D array to hard drive and append a NewLine.</summary><param name="array2d"> 2D array.</param>
   public void WriteLine<T>(T[][] array2d, string fileNameNoExt) {
      SetFile(fileNameNoExt);
      WriteLine(array2d);
   }
   /// <summary>Write a 3D array to hard drive and append a NewLine.</summary><param name="array3d"> 3D array.</param>
   public void WriteLine<T>(T[][][] array3d) {
      OnSettingsChanged();
      Writer.WriteLine<T>(array3d);
   }
   /// <summary>Write a 3D array to hard drive and append a NewLine.</summary><param name="array3d"> 3D array.</param>
   public void WriteLine<T>(T[][][] array3d, string fileNameNoExt) {
      SetFile(fileNameNoExt);
      WriteLine(array3d);
   }
   /// <summary>Write a 4D array to hard drive and append a NewLine.</summary><param name="array4d"> 4D array.</param>
   public void WriteLine<T>(T[][][][] array4d) {
      OnSettingsChanged();
      Writer.WriteLine<T>(array4d);
   }
   /// <summary>Write a 4D array to hard drive and append a NewLine.</summary><param name="array4d"> 4D array.</param>
   public void WriteLine<T>(T[][][][] array4d, string fileNameNoExt) {
      SetFile(fileNameNoExt);
      WriteLine(array4d);
   }
   /// <summary>Write a 5D array to hard drive and append a NewLine.</summary><param name="array5d"> 5D array.</param>
   public void WriteLine<T>(T[][][][][] array5d) {
      OnSettingsChanged();
      Writer.WriteLine<T>(array5d);
   }
   /// <summary>Write a 5D array to hard drive and append a NewLine.</summary><param name="array5d"> 5D array.</param>
   public void WriteLine<T>(T[][][][][] array5d, string fileNameNoExt) {
      SetFile(fileNameNoExt);
      WriteLine(array5d);
   }
   /// <summary>Write a 6D array to hard drive and append a NewLine.</summary><param name="array6d"> 6D array.</param>
   public void WriteLine<T>(T[][][][][][] array6d) {
      OnSettingsChanged();
      Writer.WriteLine<T>(array6d);
   }
   /// <summary>Write a 6D array to hard drive and append a NewLine.</summary><param name="array6d"> 6D array.</param>
   public void WriteLine<T>(T[][][][][][] array6d, string fileNameNoExt) {
      SetFile(fileNameNoExt);
      WriteLine(array6d);
   }
   public void Write<T>(T val) {
      OnSettingsChanged();
      Writer.Write(val);
   }
   public void Write<T>(T val, string fileNameNoExt) {
      SetFile(fileNameNoExt);
      Writer.Write(val);
   }
   public void WriteLine<T>(T val) {
      OnSettingsChanged();
      Writer.WriteLine(val);
   }
   public void WriteLine<T>(T val, string fileNameNoExt) {
      SetFile(fileNameNoExt);
      Writer.WriteLine(val);
   }
   public void Dispose() {
      GC.SuppressFinalize(this);
      Writer?.Dispose();
   }
   ~FileWriter() => Dispose();
}
}