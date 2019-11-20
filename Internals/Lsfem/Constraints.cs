using System.Collections;

namespace Fluid.Internals.Lsfem {
public class Constraints {
   BitArray Bits { get; }
   int NVar { get; }


   public Constraints(int nPos, int nVar) {
      Bits = new BitArray(nPos*nVar);
      NVar = nVar;
   }


   public bool this[int i, int j] {
      get => Bits[NVar*i + j];
      set => Bits[NVar*i + j] = value;
   }
}
}