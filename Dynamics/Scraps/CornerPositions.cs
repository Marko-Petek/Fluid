#if false
using System;

namespace Fluid.Dynamics.Meshing
{
    /// <summary>Positions of mesh corners.</summary>
    public class CornerPositions
    {
        /// <summary>Position of north-west corner.</summary>
        Position _upperLeft;
        /// <summary>Position of south-west corner.</summary>
        Position _lowerLeft;
        /// <summary>Position of south-east corner.</summary>
        Position _lowerRight;
        /// <summary>Position of north-east corner.</summary>
        Position _upperRight;

        /// <summary>Position of north-west corner.</summary>
        public ref Position _ll => ref _upperLeft;
        /// <summary>Position of north-east corner.</summary>
        public ref Position _ur => ref _upperRight;
        /// <summary>Position of south-west corner.</summary>
        public ref Position _ll => ref _lowerLeft;
        /// <summary>Position of south-east corner.</summary>
        public ref Position _lr => ref _lowerRight;


        /// <summary>Create a data structure that holds positions of mesh corners.</summary><param name="upperLeftX">X coordinate of north-west corner.</param><param name="upperLeftY">Y coordinate of north-west corner.</param><param name="upperRightX">X coordinate of north-east corner.</param><param name="upperRightY">Y coordinate of north-east corner.</param><param name="lowerLeftX">X coordinate south-west corner.</param><param name="lowerLeftY">Y coordinate south-west corner.</param><param name="lowerRightX">X coordinate south-east corner.</param><param name="lowerRightY">Y coordinate south-east corner.</param>
        public CornerPositions(double upperLeftX, double upperLeftY, double lowerLeftX, double lowerLeftY,
                                double lowerRightX, double lowerRightY, double upperRightX, double upperRightY) {

            _upperLeft = new Position(upperLeftX, upperLeftY);
            _lowerLeft = new Position(lowerLeftX, lowerLeftY);
            _lowerRight = new Position(lowerRightX, lowerRightY);
            _upperRight = new Position(upperRightX, upperRightY);
        }
    }
}
#endif