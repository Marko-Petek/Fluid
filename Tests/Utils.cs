namespace Fluid.Tests{
public static class Utils {
   /// <summary>Returns position after read.</summary>
   /// <param name="data"></param>
   /// <param name="pos"></param>
   /// <param name="read"></param>
   public static (int pos, int read) Read(int[] data, int pos = 0, int read = 0) {
      pos += read;
      read = data[pos];
      return (pos + 1, read);
   }
}
}