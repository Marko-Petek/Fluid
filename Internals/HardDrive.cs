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
        public const string DefaultFileName = "Output.txt";
        public string AppDirPath { get; set; }
        string _Directory;

        public string Directory {
            get => _Directory;
            set {
                if(_Directory != value) {
                    _Directory = value;
                    _FullPath = value + "/" + File;
                    _Writer?.Dispose();
                    _Writer = new StreamWriter(_FullPath);
                }
            }
        }
        string _File;

        public string File {
            get => _File;
            set {
                if(_File != value) {
                    _File = value;
                    _FullPath = Directory + "/" + value;
                    _Writer?.Dispose();
                    _Writer = new StreamWriter(_FullPath);
                }
            }
        }
        string _FullPath;
        
        string FullPath {
            get => _FullPath;
            set {
                if(_FullPath != value) {
                    _FullPath = value;
                    var splitPath = value.Split('/').Skip(1);
                    var dir = splitPath.Take(splitPath.Count() - 1).Select(word => "/" + word);
                    _Directory = String.Concat(dir);
                    var file = splitPath.TakeLast(1);
                    _File = String.Concat(file);
                    _Writer?.Dispose();
                    _Writer = new StreamWriter(FullPath);
                }
            }
        }

        StreamWriter _Writer;
        StreamWriter Writer => _Writer;



        public HardDrive() {
            var assembly = Assembly.GetExecutingAssembly();
            var dllLocation = assembly.Location;
            string[] splitDllLoc = dllLocation.Split('/');

            int i = splitDllLoc.Length - 1;
            while(splitDllLoc[i] != "Fluid") {

                
                if(i-- == 0)                                                // Path did not contain the word "Fluid".
                    throw new DirectoryNotFoundException(
                        @"Could not find directory named Fluid above directory containing dll.");
            }
            var splitPath = splitDllLoc.Skip(1).Take(i).Select(word => "/" + word);
            AppDirPath = String.Concat(splitPath);
            FullPath = AppDirPath + "/" + DefaultFileName;                  // This initializes Writer.
        }

        /// <summary>Writes an IEnumerable to hard drive.</summary><param name="enumerable">Enumerable to write to HD.</param>
        public void Write(IEnumerable enumerable) {
            Writer.Write(enumerable.ToString());                    // We count on the fact that enumerable overrides its ToString method.
        }

        /// <summary>Writes an IEnumerable to hard drive.</summary><param name="enumerable">Enumerable to write to HD.</param>
        public void Write(IEnumerable enumerable, string fullPathOrFileName) {
            FullPath = fullPath;                                                // TODO: Implement a method that determines which one user entered.
            Write(enumerable);
        }

        ~HardDrive() {
            Writer.Dispose();
        }
    }
}