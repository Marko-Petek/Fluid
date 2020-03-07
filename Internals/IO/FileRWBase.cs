using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using static Fluid.Internals.Development.Reporter;
namespace Fluid.Internals.IO {

/// <summary>dot dot dot</summary>
public abstract class FileRWBase {
   protected event EventHandler? SettingsChanged;
   public FileInfo File { get; protected set; }
   public DirectoryInfo AppDirectory { get; protected set; }
   public DirectoryInfo Directory { get; protected set; }
   protected string DirPath { get; set; }
   protected string FileNameNoExt { get; set; }
   protected string FileExt { get; set; }
   

   /// <summary>Writes data to hard drive.</summary>
   public FileRWBase(string dirPath, string fileNameNoExt,
      string fileExt) {
         var assembly = Assembly.GetExecutingAssembly();
         var dllLocation = assembly.Location;
         File = new FileInfo(dllLocation);
         AppDirectory = new DirectoryInfo(File.DirectoryName);
         Directory = AppDirectory;
         DirPath = dirPath;
         FileNameNoExt = fileNameNoExt;
         FileExt = fileExt;
         var parent = AppDirectory.Parent;
         while(true) {                                                           // Search for a directory named Fluid.
            if(parent.Name == "Fluid") {
               AppDirectory = parent;
               break; }
            parent = parent.Parent;
            if(parent == null)                                                  // null == when root is reached.
               throw new DirectoryNotFoundException(
                  @"Could not find directory named Fluid above directory containing dll."); }
         // SettingsChanged += DirChanged;
         // SettingsChanged += FileChanged;
   }
   protected void OnSettingsChanged() {
      if(SettingsChanged != null) {
            SettingsChanged(this, EventArgs.Empty);
            SettingsChanged = null;
            ResetUnmanagedResource(); }
   }
   /// <summary>Reset underlying Stream.</summary>
   protected abstract void ResetUnmanagedResource();
   void DirChanged(object? sender, EventArgs e) =>
      Directory = new DirectoryInfo(Path.Combine(AppDirectory.FullName, DirPath));
   /// <summary>Crate a new StreamWriter with currently specified settings. Properly disposes an existing writer (if any).</summary>
   void FileChanged(object? sender, EventArgs e) =>
      File = new FileInfo(Path.Combine(Directory.FullName, $"{FileNameNoExt}{FileExt}"));
   void AppendChanged(object sender, EventArgs e) { }
   public void SetDir(string dirPath) {
      DirPath = dirPath;
      if(SettingsChanged == null)            // DirPath has to be applied first.
         SettingsChanged += DirChanged;
      else
         SettingsChanged = DirChanged + SettingsChanged;
   }
   public void SetDir() => SetDir(AppDirectory.FullName);
   // dirPath can be relative or absolute
   public void SetFile(string fileNameNoExt, string fileExt) {
      FileNameNoExt = fileNameNoExt;
      FileExt = fileExt;
      SettingsChanged += FileChanged;
   }
   // dirPath can be relative or absolute
   public virtual void SetDirAndFile(string dirPath, string fileNameNoExt, string fileExt) {
      SetDir(dirPath);
      SetFile(fileNameNoExt, fileExt);
   }
}
}