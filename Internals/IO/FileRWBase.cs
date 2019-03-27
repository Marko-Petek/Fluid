using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

using static Fluid.Internals.Development.AppReporter;

namespace Fluid.Internals.IO
{
    /// <summary>dot dot dot</summary>
    public abstract class FileRWBase
    {
        protected event EventHandler SettingsChanged;

        protected void OnSettingsChanged() {
            if(SettingsChanged != null) {
                SettingsChanged(this, EventArgs.Empty);
                SettingsChanged = null;
                ResetUnmanagedResource();
            }
        }

        /// <summary>Reset underlying Stream.</summary>
        protected abstract void ResetUnmanagedResource();

        FileInfo _File;
        public FileInfo File  => _File;

        DirectoryInfo _AppDirectory;
        protected DirectoryInfo AppDirectory => _AppDirectory;

        DirectoryInfo _Directory;
        protected DirectoryInfo Directory => _Directory;

        string _DirPath;
        protected string DirPath => _DirPath;

        string _FileNameNoExt;
        protected string FileNameNoExt => _FileNameNoExt;

        string _FileExt;
        protected string FileExt => _FileExt;

        public const string DefaultDirPath = "";
        public const string DefaultFileName = "output";
        public const string DefaultExt = ".txt";


        /// <summary>Writes data to hard drive.</summary>
        public FileRWBase(string dirPath = DefaultDirPath, string fileNameNoExt = DefaultFileName, string fileExt = DefaultExt) {
            var assembly = Assembly.GetExecutingAssembly();
            var dllLocation = assembly.Location;
            _File = new FileInfo(dllLocation);
            _AppDirectory = new DirectoryInfo(File.DirectoryName);
            _DirPath = DefaultDirPath;
            _FileNameNoExt = fileNameNoExt;
            _FileExt = fileExt;
            var parent = AppDirectory.Parent;

            while(true) {                                                           // Search for a directory named Fluid.
                if(parent.Name == "Fluid") {
                    _AppDirectory = parent;
                    break;
                }
                parent = parent.Parent;

                if(parent == null)                                                  // null == when root is reached.
                    throw new DirectoryNotFoundException(
                        @"Could not find directory named Fluid above directory containing dll.");
            }
            SettingsChanged += DirChanged;
            SettingsChanged += FileChanged;
        }

        void DirChanged(object sender, EventArgs e) {
            _Directory = new DirectoryInfo(Path.Combine(AppDirectory.FullName, DirPath));
        }

        /// <summary>Crate a new StreamWriter with currently specified settings. Properly disposes an existing writer (if any).</summary>
        void FileChanged(object sender, EventArgs e) {
            string filePath = Path.Combine(Directory.FullName, $"{FileNameNoExt}{FileExt}");
            _File = new FileInfo(filePath);
        }

        void AppendChanged(object sender, EventArgs e) {
        }

        public void SetDir(string dirPath) {
            _DirPath = dirPath;

            if(SettingsChanged == null)            // DirPath has to be applied first.
                SettingsChanged += DirChanged;
            else
                SettingsChanged = DirChanged + SettingsChanged;
        }

        public void SetDir() => SetDir(AppDirectory.FullName);

        // dirPath can be relative or absolute
        public void SetFile(string fileNameNoExt, string fileExt) {
            _FileNameNoExt = fileNameNoExt;
            _FileExt = fileExt;
            SettingsChanged += FileChanged;
        }

        // public void SetFile(string fileNameNoExt) => SetFile(fileNameNoExt, DefaultExt);

        // dirPath can be relative or absolute
        public virtual void SetDirAndFile(string dirPath, string fileNameNoExt, string fileExt) {
            SetDir(dirPath);
            SetFile(fileNameNoExt, fileExt);
        }
    }
}