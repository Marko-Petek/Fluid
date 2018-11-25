using System;
using System.Collections;

namespace L03_Percolation
{
    public class Grid
    {
        /// <summary>Array that specifies which elements are conductive.</summary>
        BitArray[] _Rows;

        public Grid(int nRows, int nCols) {
            _Rows = new BitArray[nRows];

            for(int i = 0; i < nRows; ++i) {
                _Rows[i] = new BitArray(nCols);
            }
        }
    }
}