# Internals.IO

Methods that convert arrays to strings found in: TextWriterExt. They are written as extension methods of TextWriter.

Console.cs uses a TextWriter named "TW" to write either to System.Console.Out or a DebugTextWriter.
