using Xunit;
using Fluid.Tests.Mocks;
using TB = Fluid.Internals.Toolbox;
using System.Threading;

namespace Fluid.Tests
{
   public partial class Thread1 {
      static Thread1() {
         TB.EntryPointSetup("Starting Thread1 tests.");
      }
      /// <summary>Test transformation from standard block indices to compact block indices.</summary>
      [Fact] public void CmtToStdIndexTransform() {               // Uses TB.FileReader inside ObstructionBlock.
         var block = new ObstructionBlockMock();
         Assert.True( block.CmtInxFromStdInx(0,0,0) == (0,0,2) );
         Assert.True( block.CmtInxFromStdInx(0,0,1) == (0,0,3) );
         Assert.True( block.CmtInxFromStdInx(0,0,2) == (0,0,4) );
         Assert.True( block.CmtInxFromStdInx(0,0,3) == (0,1,2) );
         Assert.True( block.CmtInxFromStdInx(0,0,4) == (0,1,1) );
         Assert.True( block.CmtInxFromStdInx(0,0,5) == (0,1,0) );
         Assert.True( block.CmtInxFromStdInx(0,0,6) == (1,1,2) );
         Assert.True( block.CmtInxFromStdInx(0,0,7) == (1,0,4) );
         Assert.True( block.CmtInxFromStdInx(0,0,8) == (1,0,3) );
         Assert.True( block.CmtInxFromStdInx(0,0,9) == (1,0,2) );
         Assert.True( block.CmtInxFromStdInx(0,0,10) == (0,0,0) );
         Assert.True( block.CmtInxFromStdInx(0,0,11) == (0,0,1) );
      }
   }
}