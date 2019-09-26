using Xunit;
using Xunit.Abstractions;

using Fluid.Internals.IO;
using TB = Fluid.Internals.Toolbox;

namespace Fluid.Tests {
   public abstract class CustomTest {
      protected ITestOutputHelper OutHelper { get; }
      public CustomTest(ITestOutputHelper outHelper) {
         OutHelper = outHelper;
         Console.TTW = new TestTextWriter(outHelper);
         TB.Writer.TW = Console.TTW;
      }
   }
}