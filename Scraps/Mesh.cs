        #if false
        
        /// <summary>Gets position of a node with a given index inside global node list.</summary><param name="globalIndex">Index of node inside global node list.</param>
        public ref Position GetNodePosition(int globalIndex) => ref _nodePositions.GetReference((globalIndex / 8) + (globalIndex % 8));

        <param name="ksi">Arc length coordinate from 0 to 1.</param><remarks>This block is rotated for 180 deg, so upper border is actually on south border of obstruction.</remarks>
        
        _lowerArcLength = 0.25 * channelMesh.GetObstructionCircumference();
                    _upperArcLength = _channelMesh.GetWidth();
                    _sideArcLength = 0.5*Sqrt(2) * _channelMesh.GetWidth() - _channelMesh.GetObstructionRadius();
        
        #endif