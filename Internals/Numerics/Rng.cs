namespace Fluid.Internals.Numerics
{
    public class Rng
    {
        System.Random Random { get; }
        public double Min { get; protected set; }
        public double Max { get; protected set; }

        public Rng(double min = 0.0, double max = 1.0) {
            Random = new System.Random();
            Min = min;
            Max = max;
        }

        public void SetRange(double min, double max) {
            Min = min;
            Max = max;
        }

        public int Int32() => Random.Next((int)Min, (int)Max);


        public double Double() => Random.NextDouble() * (Max - Min) + Min;
    }
}