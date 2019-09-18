using System;
using System.IO;
using System.Text;
using Xunit.Abstractions;

namespace Fluid.Internals.IO {
   public class TestTextWriter : StreamWriter {
      ITestOutputHelper OutHelper { get; }


      public TestTextWriter(ITestOutputHelper outHelper)
         : base(new TestOutStream(outHelper), Encoding.UTF8, 1024) {
         AutoFlush = true;
         OutHelper = outHelper;
      }


      public override void WriteLine(string str) {
            OutHelper.WriteLine(str);
      }

      class TestOutStream : Stream {
         ITestOutputHelper OutHelper { get; }

         public TestOutStream(ITestOutputHelper outHelper) : base() {
            OutHelper = outHelper;
         }


         public override void Write(byte[] buffer, int offset, int count) {
            OutHelper.WriteLine(Encoding.UTF8.GetString(buffer, offset, count));
         }
         public override bool CanRead { get { return false; } }
         public override bool CanSeek { get { return false; } }
         public override bool CanWrite { get { return true; } }
         public override void Flush() { }
         public override long Length { get { throw new InvalidOperationException(); } }
         public override int Read(byte[] buffer, int offset, int count) { throw new InvalidOperationException(); }
         public override long Seek(long offset, SeekOrigin origin) { throw new InvalidOperationException(); }
         public override void SetLength(long value) { throw new InvalidOperationException(); }
         public override long Position {
            get { throw new InvalidOperationException(); }
            set { throw new InvalidOperationException(); }
         }
      };
   }
}