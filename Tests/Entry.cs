using System;
using System.Threading;
using Xunit;
using Xunit.Runners;

using static Fluid.Internals.Toolbox;

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
         R.R("Discovering...");
         Start(null);
         // _Finished.WaitOne();
         // Start(nameof(Fluid.Tests.Geometry));
         // _Finished.WaitOne();
         // Start(nameof(Fluid.Tests.Matrices));
         // _Finished.WaitOne();
         // Start(nameof(Fluid.Tests.Numerics));
         // _Finished.WaitOne();
         // Start(nameof(Fluid.Tests.Tensors));
         _Finished.WaitOne();
         _Finished.Dispose();  
         
         void Start(string type) {
            //_Finished.Reset();
            runner.Start(type, diagnosticMessages: true, null, null, null,       // Runs start here.
               parallel: true, maxParallelThreads: 3, null); }
      return _Result; }
   }

   static void OnDiscoveryComplete(DiscoveryCompleteInfo info) {
      lock (_ConsoleLock)
         R.R($"Running {info.TestCasesToRun} of {info.TestCasesDiscovered} tests...");
   }
   static void OnExecutionComplete(ExecutionCompleteInfo info) {
      lock (_ConsoleLock)
         R.R($"Finished: {info.TotalTests} tests in {Math.Round(info.ExecutionTime, 3)}s ({info.TestsFailed} failed, {info.TestsSkipped} skipped)");
      _Finished.Set();
   }
   static void OnTestFailed(TestFailedInfo info) {
      lock (_ConsoleLock) {
         Console.ForegroundColor = ConsoleColor.Red;
         R.R($"[FAIL] {info.TestDisplayName}: {info.ExceptionMessage}");
         if (info.ExceptionStackTrace != null)
            R.R(info.ExceptionStackTrace);
         Console.ResetColor(); }
      _Result = 1;
   }
   static void OnTestSkipped(TestSkippedInfo info) {
      lock (_ConsoleLock) {
         Console.ForegroundColor = ConsoleColor.Yellow;
         R.R($"[SKIP] {info.TestDisplayName}: {info.SkipReason}");
         Console.ResetColor(); }
   }
}

}