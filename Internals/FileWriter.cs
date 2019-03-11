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
        FileInfo _File;
        public FileInfo File {
            get => _File;
            set {
                if(value != _File) {
                    _File = value;
                    SettingsChanged = true;
                }
            }
        }

        DirectoryInfo _Directory;
        DirectoryInfo Directory => _Directory;

        bool _Append;
        /// <summary>Whether Writer should append to currently set file or overwrite it.</summary>
        public bool Append {
            get => _Append;
            set {
                if(value != _Append) {
                    _Append = value;
                    SettingsChanged = true;
                }
            }
        }

        public const string DefaultFileName = "output.txt";

        StreamWriter _Writer;
        StreamWriter Writer => _Writer;

        /// <summary>Indicates whether a setting that requires reinitialization of StreamWriter has changed.</summary>
        bool SettingsChanged { get; set; }



        /// <summary>Writes data to hard drive.</summary>
        public FileWriter() : this(DefaultFileName, true) {
        }



        /// <summary>Writes data to hard drive.</summary>
        public FileWriter(string path, bool append) {
            var assembly = Assembly.GetExecutingAssembly();
            var dllLocation = assembly.Location;
            _File = new FileInfo(dllLocation);
            _Directory = new DirectoryInfo(File.DirectoryName);
            var parent = Directory.Parent;

            while(true) {                                                           // Search for a directory named Fluid.
                if(parent.Name == "Fluid") {
                    _Directory = parent;
                    break;
                }
                parent = parent.Parent;

                if(parent == null)                                                  // null == when root is reached.
                    throw new DirectoryNotFoundException(
                        @"Could not find directory named Fluid above directory containing dll.");
            }
            SetFile(path, append);
        }

        public void SetFile(string path, bool append = false) {
            string filePath = Path.Combine(Directory.FullName, path);
            _File = new FileInfo(filePath);
            _Append = append;
            InitializeWriter();
        }

        /// <summary>Crate a new StreamWriter with currently specified settings. Properly disposes an existing writer (if any).</summary>
        void InitializeWriter() {
            Writer?.Dispose();                                      // Dispose old writer if it exists.
            _Writer = new StreamWriter(File.FullName, Append);
            SettingsChanged = false;                                // Reset flag.
        }

        /// <summary>Immediatelly rrite any RAM buffered data to hard drive.</summary>
        public void Flush() => Writer.Flush();

        /// <summary>Writes a 1D array to hard drive.</summary><param name="array1d"> 1D array to write to HD.</param>
        public void Write<T>(T[] array1d) {
            if(SettingsChanged)
                InitializeWriter();

            IO.Write(array1d, Writer);
        }

        /// <summary>Writes a 2D array to hard drive.</summary><param name="array1d">2D array to write to HD.</param>
        public void Write<T>(T[][] array2d) {
            if(SettingsChanged)
                InitializeWriter();

            IO.Write(array2d, Writer);
        }

        /// <summary>Writes a 3D array to hard drive.</summary><param name="array3d">3D array to write to HD.</param>
        public void Write<T>(T[][][] array3d) {
            if(SettingsChanged)
                InitializeWriter();

            IO.Write(array3d, Writer);
        }

        /// <summary>Writes a 4D array to hard drive.</summary><param name="array4d">4D array to write to HD.</param>
        public void Write<T>(T[][][][] array4d) {
            if(SettingsChanged)
                InitializeWriter();

            IO.Write(array4d, Writer);
        }

        /// <summary>Writes a 5D array to hard drive.</summary><param name="array6d">5D array to write to HD.</param>
        public void Write<T>(T[][][][][] array5d) {
            if(SettingsChanged)
                InitializeWriter();

            IO.Write(array5d, Writer);
        }

        /// <summary>Writes a 6D array to hard drive.</summary><param name="array6d">6D array to write to HD.</param>
        public void Write<T>(T[][][][][][] array6d) {
            if(SettingsChanged)
                InitializeWriter();

            IO.Write(array6d, Writer);
        }

        /// <summary>Write a 1D array to hard drive and append a NewLine.</summary><param name="array1d"> 1D array to write to HD.</param>
        public void WriteLine<T>(T[] array1d) {
            if(SettingsChanged)
                InitializeWriter();

            IO.WriteLine(array1d, Writer);
        }

        /// <summary>Write a 2D array to hard drive and append a NewLine.</summary><param name="array2d"> 2D array.</param>
        public void WriteLine<T>(T[][] array2d) {
            if(SettingsChanged)
                InitializeWriter();
                
            IO.WriteLine(array2d, Writer);
        }

        /// <summary>Write a 3D array to hard drive and append a NewLine.</summary><param name="array3d"> 3D array.</param>
        public void WriteLine<T>(T[][][] array3d) {
            if(SettingsChanged)
                InitializeWriter();
                
            IO.WriteLine(array3d, Writer);
        }

        /// <summary>Write a 4D array to hard drive and append a NewLine.</summary><param name="array4d"> 4D array.</param>
        public void WriteLine<T>(T[][][][] array4d) {
            if(SettingsChanged)
                InitializeWriter();
                
            IO.WriteLine(array4d, Writer);
        }

        /// <summary>Write a 5D array to hard drive and append a NewLine.</summary><param name="array5d"> 5D array.</param>
        public void WriteLine<T>(T[][][][][] array5d) {
            if(SettingsChanged)
                InitializeWriter();
                
            IO.WriteLine(array5d, Writer);
        }

        /// <summary>Write a 6D array to hard drive and append a NewLine.</summary><param name="array6d"> 6D array.</param>
        public void WriteLine<T>(T[][][][][][] array6d) {
            if(SettingsChanged)
                InitializeWriter();
                
            IO.WriteLine(array6d, Writer);
        }

        public void Write<T>(T val) => Writer.Write(val);
        public void WriteLine<T>(T val) => Writer.WriteLine(val);

        public void Dispose() {
            Writer.Dispose();
            GC.SuppressFinalize(this);
        }

        ~FileWriter() => Dispose();
    }
}