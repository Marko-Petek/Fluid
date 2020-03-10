using System.Text;
namespace Fluid.Internals.Text {

/// <summary>A simplification of StringBuilder with only two methods: Append and Yield.</summary>
public class Stringer {
   StringBuilder _SB;

   public Stringer(int cap) {
      _SB = new StringBuilder(cap);
   }
   /// <summary>Append a string to the StringBuilder.</summary>
   /// <param name="str">String to append.</param>
   public void A(string str) => _SB.Append(str);
   /// <summary>Yield the stored string (meaning: remove it from memory and pass it over).</summary>
   public string Y() {
      var str = _SB.ToString();
      _SB.Clear();
      return str;
   }
   public string Y(string str) {
      _SB.Append(str);
      var outStr = _SB.ToString();
      _SB.Clear();
      return outStr;
   }
}
}