using System;
using System.Threading;
using Xunit;
using Xunit.Runners;

namespace Fluid.Tests {

public class Entry {
   static object consoleLock = new object();                                  // Messages can arrive in parallel, so we want to make sure we get consistent console output.
   static ManualResetEvent finished = new ManualResetEvent(false);            // An event to know when we're done.
   static int result = 0;                                                     // Start out assuming success; we'll set this to 1 if we get a failed test.

   public static int Point(string[] args) {
      if (args.Length == 0 || args.Length > 2) {
         Console.WriteLine("usage: TestRunner <assembly> [typeName]");
         return 2; }
      var testAssembly = args[0];

      Xunit.

      var typeName = args.Length == 2 ? args[1] : null;
      using (var runner = AssemblyRunner.WithoutAppDomain(testAssembly)) {
         runner.OnDiscoveryComplete = OnDiscoveryComplete;
         runner.OnExecutionComplete = OnExecutionComplete;
         runner.OnTestFailed = OnTestFailed;
         runner.OnTestSkipped = OnTestSkipped;
         Console.WriteLine("Discovering...");
         runner.Start(typeName);
         finished.WaitOne();
         finished.Dispose();
         return result; }
   }

   static void OnDiscoveryComplete(DiscoveryCompleteInfo info) {
      lock (consoleLock)
         Console.WriteLine($"Running {info.TestCasesToRun} of {info.TestCasesDiscovered} tests...");
   }
   static void OnExecutionComplete(ExecutionCompleteInfo info) {
      lock (consoleLock)
         Console.WriteLine($"Finished: {info.TotalTests} tests in {Math.Round(info.ExecutionTime, 3)}s ({info.TestsFailed} failed, {info.TestsSkipped} skipped)");
      finished.Set();
   }
   static void OnTestFailed(TestFailedInfo info) {
      lock (consoleLock) {
         Console.ForegroundColor = ConsoleColor.Red;
         Console.WriteLine("[FAIL] {0}: {1}", info.TestDisplayName, info.ExceptionMessage);
         if (info.ExceptionStackTrace != null)
            Console.WriteLine(info.ExceptionStackTrace);
         Console.ResetColor(); }
      result = 1;
   }
   static void OnTestSkipped(TestSkippedInfo info) {
      lock (consoleLock) {
         Console.ForegroundColor = ConsoleColor.Yellow;
         Console.WriteLine("[SKIP] {0}: {1}", info.TestDisplayName, info.SkipReason);
         Console.ResetColor(); }
   }
}

}