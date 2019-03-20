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

//"preLaunchTask": "build",
            // If you have changed target frameworks, make sure to update the program path.
            "program": "${workspaceFolder}/Tests/bin/Debug/netcoreapp2.2/Tests.dll",
            "args": [],
            "cwd": "${workspaceFolder}/Tests",
            // For more information about the 'console' field, see https://github.com/OmniSharp/omnisharp-vscode/blob/master/debugger-launchjson.md#console-terminal-window
            "console": "integratedTerminal",
            "stopAtEntry": false,
            "internalConsoleOptions": "openOnSessionStart",
            //"justMyCode": false
            "enableStepFiltering": false
#endif