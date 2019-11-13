#nullable enable
namespace Fluid.Internals.Numerics {

public class RandomNumberGen {
   System.Random Random { get; }
   public double Min { get; protected set; }
   public double Max { get; protected set; }
   public int Int => Random.Next((int)Min, (int)Max);
   public double Dbl => Random.NextDouble() * (Max - Min) + Min;

   public RandomNumberGen(double min = 0.0, double max = 1.0) {
      Random = new System.Random();
      Min = min;
      Max = max;
   }

   public void SetRange(double min, double max) {
      Min = min;
      Max = max;
   }
}
}
#nullable restore