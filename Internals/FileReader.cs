using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using static Fluid.Internals.Development.AppReporter;

namespace Fluid.Internals
{
    public class FileReader : FileRWBase, IDisposable
    {
        StreamReader _Reader;
        StreamReader Reader => _Reader;

        event EventHandler SettingsChanged;

        protected void OnSettingsChanged() {
            if(SettingsChanged != null) {
                SettingsChanged(this, EventArgs.Empty);
                SettingsChanged = null;                                 // Remove all subscribers.
                Writer?.Dispose();                                      // Dispose old writer if it exists.
                _Writer = new StreamWriter(File.FullName, Append);
            }
        }


        public void Dispose() {
            Writer.Dispose();
            GC.SuppressFinalize(this);
        }

        ~FileWriter() => Dispose();
    }
}