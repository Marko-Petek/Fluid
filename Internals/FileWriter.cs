using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using static Fluid.Internals.Development.AppReporter;

namespace Fluid.Internals
{
    /// <summary>Contains methods which write nicely formatted values to hard drive. You have to call Flush() manually if you want to immediatelly 
    /// see results written on HD (empty RAM buffer to HD).</summary>
    public class FileWriter : IDisposable
    {
        PotentialChanges SettingsChanges = PotentialChanges.None;

        event EventHandler SettingsChanged;

        protected virtual void OnSettingsChanged() {
            if(SettingsChanged != null) {
                SettingsChanged(this, EventArgs.Empty);
                SettingsChanged = null;                                 // Remove all subscribers.
                Writer?.Dispose();                                      // Dispose old writer if it exists.
                _Writer = new StreamWriter(File.FullName, Append);
            }
        }

        FileInfo _File;
        public FileInfo File  => _File;

        DirectoryInfo _AppDirectory;
        DirectoryInfo AppDirectory => _AppDirectory;

        DirectoryInfo _Directory;
        DirectoryInfo Directory => _Directory;

        string _DirPath;
        string DirPath => _DirPath;

        string _FileNameNoExt;
        string FileNameNoExt => _FileNameNoExt;

        string _FileExt;
        string FileExt => _FileExt;

        bool _Append;
        /// <summary>Whether Writer should append to currently set file or overwrite it.</summary>
        public bool Append => _Append;

        public const string DefaultDirPath = "";
        public const string DefaultFileName = "output";
        public const string DefaultExt = ".txt";
        public const bool DefaultAppend = false;

        StreamWriter _Writer;
        StreamWriter Writer => _Writer;


        /// <summary>Writes data to hard drive.</summary>
        public FileWriter(string dirPath = DefaultDirPath, string fileNameNoExt = DefaultFileName, string ext = DefaultExt, bool append = DefaultAppend) {
            var assembly = Assembly.GetExecutingAssembly();
            var dllLocation = assembly.Location;
            _File = new FileInfo(dllLocation);
            _AppDirectory = new DirectoryInfo(File.DirectoryName);
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
            OnSettingsChanged();
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

            if(SettingsChanged == null) {            // DirPath has to be applied first.
                SettingsChanged += DirChanged;
            }
            else {
                SettingsChanged = DirChanged + SettingsChanged;
            }
        }

        public void SetDir() => SetDir(AppDirectory.FullName);

        // dirPath can be relative or absolute
        public void SetFile(string fileNameNoExt, string fileExt, bool append = DefaultAppend) {
            _FileNameNoExt = fileNameNoExt;
            _FileExt = fileExt;
            SettingsChanged += FileChanged;
            _Append = append;
            SettingsChanged += AppendChanged;
        }

        public void SetFile(string fileNameNoExt, bool append) => SetFile(fileNameNoExt, FileExt, append);

        public void SetFile(string fileNameNoExt) => SetFile(fileNameNoExt, Append);

        // dirPath can be relative or absolute
        public void SetDirAndFile(string dirPath, string fileNameNoExt, string fileExt, bool append = false) {
            SetDir(dirPath);
            SetFile(fileNameNoExt, fileExt, append);
        }

        /// <summary>Immediatelly rrite any RAM buffered data to hard drive.</summary>
        public void Flush() => Writer.Flush();

        /// <summary>Writes a 1D array to hard drive.</summary><param name="array1d"> 1D array to write to HD.</param>
        public void Write<T>(T[] array1d) {
            OnSettingsChanged();
            IO.Write(array1d, Writer);
        }

        /// <summary>Writes a 1D array to hard drive.</summary><param name="array1d"> 1D array to write to HD.</param>
        public void Write<T>(T[] array1d, string fileNameNoExt) {
            SetFile(fileNameNoExt);
            Write(array1d);
        }

        /// <summary>Writes a 2D array to hard drive.</summary><param name="array1d">2D array to write to HD.</param>
        public void Write<T>(T[][] array2d) {
            OnSettingsChanged();
            IO.Write(array2d, Writer);
        }

        /// <summary>Writes a 2D array to hard drive.</summary><param name="array1d">2D array to write to HD.</param>
        public void Write<T>(T[][] array2d, string fileNameNoExt) {
            SetFile(fileNameNoExt);
            Write(array2d);
        }

        /// <summary>Writes a 3D array to hard drive.</summary><param name="array3d">3D array to write to HD.</param>
        public void Write<T>(T[][][] array3d) {
            OnSettingsChanged();
            IO.Write(array3d, Writer);
        }

        /// <summary>Writes a 3D array to hard drive.</summary><param name="array3d">3D array to write to HD.</param>
        public void Write<T>(T[][][] array3d, string fileNameNoExt) {
            SetFile(fileNameNoExt);
            Write(array3d);
        }

        /// <summary>Writes a 4D array to hard drive.</summary><param name="array4d">4D array to write to HD.</param>
        public void Write<T>(T[][][][] array4d) {
            OnSettingsChanged();
            IO.Write(array4d, Writer);
        }

        /// <summary>Writes a 4D array to hard drive.</summary><param name="array4d">4D array to write to HD.</param>
        public void Write<T>(T[][][][] array4d, string fileNameNoExt) {
            SetFile(fileNameNoExt);
            Write(array4d);
        }

        /// <summary>Writes a 5D array to hard drive.</summary><param name="array6d">5D array to write to HD.</param>
        public void Write<T>(T[][][][][] array5d) {
            OnSettingsChanged();
            IO.Write(array5d, Writer);
        }

        /// <summary>Writes a 5D array to hard drive.</summary><param name="array6d">5D array to write to HD.</param>
        public void Write<T>(T[][][][][] array5d, string fileNameNoExt) {
            SetFile(fileNameNoExt);
            Write(array5d);
        }

        /// <summary>Writes a 6D array to hard drive.</summary><param name="array6d">6D array to write to HD.</param>
        public void Write<T>(T[][][][][][] array6d) {
            OnSettingsChanged();
            IO.Write(array6d, Writer);
        }

        /// <summary>Writes a 6D array to hard drive.</summary><param name="array6d">6D array to write to HD.</param>
        public void Write<T>(T[][][][][][] array6d, string fileNameNoExt) {
            SetFile(fileNameNoExt);
            Write(array6d);
        }

        /// <summary>Write a 1D array to hard drive and append a NewLine.</summary><param name="array1d"> 1D array to write to HD.</param>
        public void WriteLine<T>(T[] array1d) {
            OnSettingsChanged();
            IO.WriteLine(array1d, Writer);
        }

        /// <summary>Write a 1D array to hard drive and append a NewLine.</summary><param name="array1d"> 1D array to write to HD.</param>
        public void WriteLine<T>(T[] array1d, string fileNameNoExt) {
            SetFile(fileNameNoExt);
            WriteLine(array1d);
        }

        /// <summary>Write a 2D array to hard drive and append a NewLine.</summary><param name="array2d"> 2D array.</param>
        public void WriteLine<T>(T[][] array2d) {
            OnSettingsChanged();
            IO.WriteLine(array2d, Writer);
        }

        /// <summary>Write a 2D array to hard drive and append a NewLine.</summary><param name="array2d"> 2D array.</param>
        public void WriteLine<T>(T[][] array2d, string fileNameNoExt) {
            SetFile(fileNameNoExt);
            WriteLine(array2d);
        }

        /// <summary>Write a 3D array to hard drive and append a NewLine.</summary><param name="array3d"> 3D array.</param>
        public void WriteLine<T>(T[][][] array3d) {
            OnSettingsChanged();
            IO.WriteLine(array3d, Writer);
        }

        /// <summary>Write a 3D array to hard drive and append a NewLine.</summary><param name="array3d"> 3D array.</param>
        public void WriteLine<T>(T[][][] array3d, string fileNameNoExt) {
            SetFile(fileNameNoExt);
            WriteLine(array3d);
        }

        /// <summary>Write a 4D array to hard drive and append a NewLine.</summary><param name="array4d"> 4D array.</param>
        public void WriteLine<T>(T[][][][] array4d) {
            OnSettingsChanged();
            IO.WriteLine(array4d, Writer);
        }

        /// <summary>Write a 4D array to hard drive and append a NewLine.</summary><param name="array4d"> 4D array.</param>
        public void WriteLine<T>(T[][][][] array4d, string fileNameNoExt) {
            SetFile(fileNameNoExt);
            WriteLine(array4d);
        }

        /// <summary>Write a 5D array to hard drive and append a NewLine.</summary><param name="array5d"> 5D array.</param>
        public void WriteLine<T>(T[][][][][] array5d) {
            OnSettingsChanged();
            IO.WriteLine(array5d, Writer);
        }

        /// <summary>Write a 5D array to hard drive and append a NewLine.</summary><param name="array5d"> 5D array.</param>
        public void WriteLine<T>(T[][][][][] array5d, string fileNameNoExt) {
            SetFile(fileNameNoExt);
            WriteLine(array5d);
        }

        /// <summary>Write a 6D array to hard drive and append a NewLine.</summary><param name="array6d"> 6D array.</param>
        public void WriteLine<T>(T[][][][][][] array6d) {
            OnSettingsChanged();
            IO.WriteLine(array6d, Writer);
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
            Writer.Dispose();
            GC.SuppressFinalize(this);
        }

        ~FileWriter() => Dispose();

        [Flags] enum PotentialChanges {
            None =      0,
            DirPath =   1 << 0,
            FileName =  1 << 1,
            FileExt =   1 << 2,
            Append =    1 << 3
        }
    }
}