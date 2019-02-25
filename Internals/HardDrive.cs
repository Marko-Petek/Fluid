using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;

using static Fluid.Internals.Development.AppReporter;

namespace Fluid.Internals
{
    public class HardDrive
    {
        FileInfo _File;
        public FileInfo File => _File;

        DirectoryInfo _Directory;
        public DirectoryInfo Directory => _Directory;

        public const string DefaultFileName = "output.txt";
        // public string AppDirPath { get; set; }
        // string _DirPath;

        // public string DirPath {
        //     get => _DirPath;
        //     set {
        //         if(_DirPath != value) {
        //             _DirPath = value;
        //             _FullPath = value + "/" + FilePath;
        //             _Writer?.Dispose();
        //             _Writer = new StreamWriter(_FullPath);
        //         }
        //     }
        // }
        // string _FilePath;

        // public string FilePath {
        //     get => _FilePath;
        //     set {
        //         if(_FilePath != value) {
        //             _FilePath = value;
        //             _FullPath = DirPath + "/" + value;
        //             _Writer?.Dispose();
        //             _Writer = new StreamWriter(_FullPath);
        //         }
        //     }
        // }
        // string _FullPath;
        
        // string FullPath {
        //     get => _FullPath;
        //     set {
        //         if(_FullPath != value) {
        //             _FullPath = value;
        //             var splitPath = value.Split('/').Skip(1);
        //             var dir = splitPath.Take(splitPath.Count() - 1).Select(word => "/" + word);
        //             _DirPath = String.Concat(dir);
        //             var file = splitPath.TakeLast(1);
        //             _FilePath = String.Concat(file);
        //             _Writer?.Dispose();
        //             _Writer = new StreamWriter(FullPath);
        //         }
        //     }
        // }

        StreamWriter _Writer;
        StreamWriter Writer => _Writer;


        public HardDrive() {
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
            string filePath = Path.Join(Directory.FullName, DefaultFileName);
            _Writer = new StreamWriter(filePath);
        }

        /// <summary>Writes a 1D array to hard drive.</summary><param name="array1d"> 1D array to write to HD.</param>
        public void Write<T>(T[] array1d) {
            IO.Write(array1d, Writer);
            Writer.Flush();
        }

        /// <summary>Writes a 2D array to hard drive.</summary><param name="array1d">2D array to write to HD.</param>
        public void Write<T>(T[][] array2d) {
            IO.Write(array2d, Writer);
            Writer.Flush();
        }

        /// <summary>Writes a 3D array to hard drive.</summary><param name="array3d">3D array to write to HD.</param>
        public void Write<T>(T[][][] array3d) {
            IO.Write(array3d, Writer);
            Writer.Flush();
        }

        /// <summary>Writes a 4D array to hard drive.</summary><param name="array4d">4D array to write to HD.</param>
        public void Write<T>(T[][][][] array4d) {
            IO.Write(array4d, Writer);
            Writer.Flush();
        }

        /// <summary>Writes a 5D array to hard drive.</summary><param name="array6d">5D array to write to HD.</param>
        public void Write<T>(T[][][][][] array5d) {
            IO.Write(array5d, Writer);
            Writer.Flush();
        }

        /// <summary>Writes a 6D array to hard drive.</summary><param name="array6d">6D array to write to HD.</param>
        public void Write<T>(T[][][][][][] array6d) {
            IO.Write(array6d, Writer);
            Writer.Flush();
        }

        /// <summary>Writes a 1D array to hard drive.</summary><param name="array1d">1D array to write to HD.</param>
        public void Write<T>(T[] array1d, string path) {
            
            if(IsName(pathOrName)) {
                FilePath = pathOrName;
            }
            else {
                FullPath = pathOrName;
            }
            Write(array);
        }

        public void Write<T>(T[][] array, string pathOrName) where T : struct {
            
            if(IsName(pathOrName)) {
                FilePath = pathOrName;
            }
            else {
                FullPath = pathOrName;
            }
            Write(array);
        }

        public void Write<T>(T[][][] array, string pathOrName) where T : struct {
            
            if(IsName(pathOrName)) {
                FilePath = pathOrName;
            }
            else {
                FullPath = pathOrName;
            }
            Write(array);
        }

        public void Write<T>(T[][][][] array, string pathOrName) where T : struct {
            
            if(IsName(pathOrName)) {
                FilePath = pathOrName;
            }
            else {
                FullPath = pathOrName;
            }
            Write(array);
        }

        /// <summary>Checks whether a string is a full path or only a file name.</summary><param name="pathOrName">String to check.</param>
        bool IsName(string pathOrName) {
            var stringArray = pathOrName.Split('/');

            if(stringArray.Length > 1)                 // It's a path.
                return false;
            else                                        // It's a name.
                return true;
        }

        ~HardDrive() {
            Writer.Dispose();
        }
    }
}