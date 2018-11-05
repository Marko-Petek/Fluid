#if false
using System;
using FluidDynamics.Internals;
using static System.Console;
using static FluidDynamics.Numerics.Matrix;
using static FluidDynamics.Internals.IOHelper;

namespace FluidDynamics
{
    public static class Tests
    {
        #region Mesh and integral tests

        public static void WriteBlockPositions() {
            var channelMesh = new ChannelMesh();
            channelMesh.GetNorthBlock().GetPositions().WriteFile("Results/northBlock.txt");
            channelMesh.GetSouthBlock().GetPositions().WriteFile("Results/southBlock.txt");
            channelMesh.GetWestBlock().GetPositions().WriteFile("Results/westBlock.txt");
            channelMesh.GetEastBlock().GetPositions().WriteFile("Results/eastBlock.txt");
            channelMesh.GetRightBlock().GetPositions().WriteFile("Results/rectBlock.txt");
        }

        /// <summary>Write out positions of nodes for upper right part of ObstructionBlock.</summary>
        public static void WriteElementsToIntegrate() {
            var channelMesh = new ChannelMesh();
            channelMesh.GetNorthBlock().WriteLeftHalfOnly("Results/integrateThis.txt");
        }

        
        #endregion
    
        
    }
}
#endif