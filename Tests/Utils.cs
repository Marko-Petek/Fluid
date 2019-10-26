namespace Fluid.Tests{
   public static class Utils {
      public static void Read(int[] data, ref int pos, ref int read) {
         pos += read;
         read = data[pos];
         ++pos;
      }
   }
}