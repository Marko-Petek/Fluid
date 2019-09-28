using System;
using System.Threading;
using Xunit;
using Xunit.Runners;

namespace Fluid.Tests {

public class Entry {
   static object _ConsoleLock = new object();                                  // Messages can arrive in parallel, so we want to make sure we get consistent console output.
   static ManualResetEvent _Finished = new ManualResetEvent(false);            // An event to know when we're done.
   static int _Result = 0;                                                     // Start out assuming success; we'll set this to 1 if we get a failed test.

   public static int Point(string[] args) {
      var testAssembly = args[0];
      //var typeName = args.Length == 2 ? args[1] : null;
      using (var runner = AssemblyRunner.WithoutAppDomain(testAssembly)) {
         runner.OnDiscoveryComplete = OnDiscoveryComplete;
         runner.OnExecutionComplete = OnExecutionComplete;
         runner.OnTestFailed = OnTestFailed;
         runner.OnTestSkipped = OnTestSkipped;
         Console.WriteLine("Discovering...");
         Start(nameof(InputOutput));
         _Finished.WaitOne();
         Start(nameof(Geometry));
         _Finished.WaitOne();
         Start(nameof(Matrices));
         _Finished.WaitOne();
         Start(nameof(Numerics));
         _Finished.WaitOne(); 
         Start(nameof(Tensors));
         _Finished.WaitOne();
         _Finished.Dispose();  
         
         void Start(string type) {
            runner.Start("InputOutput", diagnosticMessages: true, null, null, null,       // Runs start here.
               parallel: true, maxParallelThreads: 3, null); }
      return _Result; }
   }

   static void RunType(Type type, int nThreads) {

   }

   static void OnDiscoveryComplete(DiscoveryCompleteInfo info) {
      lock (_ConsoleLock)
         Console.WriteLine($"Running {info.TestCasesToRun} of {info.TestCasesDiscovered} tests...");
   }
   static void OnExecutionComplete(ExecutionCompleteInfo info) {
      lock (_ConsoleLock)
         Console.WriteLine($"Finished: {info.TotalTests} tests in {Math.Round(info.ExecutionTime, 3)}s ({info.TestsFailed} failed, {info.TestsSkipped} skipped)");
      _Finished.Set();
   }
   static void OnTestFailed(TestFailedInfo info) {
      lock (_ConsoleLock) {
         Console.ForegroundColor = ConsoleColor.Red;
         Console.WriteLine("[FAIL] {0}: {1}", info.TestDisplayName, info.ExceptionMessage);
         if (info.ExceptionStackTrace != null)
            Console.WriteLine(info.ExceptionStackTrace);
         Console.ResetColor(); }
      _Result = 1;
   }
   static void OnTestSkipped(TestSkippedInfo info) {
      lock (_ConsoleLock) {
         Console.ForegroundColor = ConsoleColor.Yellow;
         Console.WriteLine("[SKIP] {0}: {1}", info.TestDisplayName, info.SkipReason);
         Console.ResetColor(); }
   }
}

}