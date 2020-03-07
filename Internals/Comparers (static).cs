using System.Collections.Generic;

namespace Fluid.Internals {
public static class Comparers {
   public static EqualityComparer<int> Int = EqualityComparer<int>.Default;
   public static EqualityComparer<double> Double = EqualityComparer<double>.Default;
}
}