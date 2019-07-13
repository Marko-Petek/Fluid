using System;
using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;

namespace Fluid.Internals.Mesh {
   /// <summary>Custom boundary trans-finite interpolated (TFI) submesh block. Boundaries appear as virtual functions of a single arc lenght parameter.</summary>
   public abstract class TfiBlock : Block {
      /// <summary>Positions of block's corners.</summary>
      protected Tetragon _quadrilateral;

      /// <summary>For testing purposes only.</summary>
      public TfiBlock() {
      }

      /// <summary>Create a custom boundary trans-finite interpolated (TFI) submesh block.</summary><param name="mainMesh">Owner of this block.</param><param name="ulx">North-west corner X.</param><param name="uly">North-west corner Y.</param><param name="llx">South-west corner X.</param><param name="lly">South-west corner Y.</param><param name="lrx">South-east corner X.</param><param name="lry">South-east corner Y.</param><param name="urx">North-east corner X.</param><param name="ury">North-east corner Y.</param>
      public TfiBlock(BlockMesh mainMesh,
         double llx, double lly,
         double lrx, double lry,
         double urx, double ury,
         double ulx, double uly)
         : base(mainMesh) {
            _quadrilateral = new Tetragon(llx, lly,
                  lrx, lry, urx, ury, ulx, uly);
      }

      /// <summary>Returns real-world position of upper boundary node located at a given arc length coordinate.</summary><param name="ksi">Arc length coordinate from 0 to 1.</param>
      protected abstract Pos CalcUpperBoundaryPos(double ksi);
      /// <summary>Returns real-world position of boundary node located at a given arc length coordinate.</summary><param name="ksi">Arc length coordinate from 0 to 1.</param>
      protected abstract Pos CalcLowerBoundaryPos(double ksi);
      /// <summary>Returns real-world position of boundary node located at a given arc length coordinate.</summary><param name="eta">Arc length coordinate from 0 to 1.</param>
      protected abstract Pos CalcLeftBoundaryPos(double eta);
      /// <summary>Returns real-world position of boundary node located at a given arc length coordinate.</summary><param name="eta">Arc length coordinate from 0 to 1.</param>
      protected abstract Pos CalcRightBoundaryPos(double eta);
      /// <summary>Take a point in unit square and return a TFI transformed point in real-world.</summary><param name="ksi">Horizontal coordinate on unit square representation of SubMesh.</param> <param name="eta">Vertical coordinate on unit square representation of SubMesh.</param>
      protected Pos CreatePosition(double ksi, double eta) {
         var upperBoundaryPos = CalcUpperBoundaryPos(ksi);
         var lowerBoundaryPos = CalcLowerBoundaryPos(ksi);
         var leftBoundaryPos = CalcLeftBoundaryPos(eta);
         var rightBoundaryPos = CalcRightBoundaryPos(eta);
         var x = (1 - ksi)*leftBoundaryPos.X  +  ksi*rightBoundaryPos.X  + 
                  (1 - eta)*lowerBoundaryPos.X  +  eta*upperBoundaryPos.X  -  
                  (1 - ksi)*(1 - eta)*_quadrilateral._LL.X - (1 - ksi)*eta*_quadrilateral._UL.X -
                  (1 - eta)*ksi*_quadrilateral._LR.X - ksi*eta*_quadrilateral._UR.X;
         var y = (1 - ksi)*leftBoundaryPos.Y  +  ksi*rightBoundaryPos.Y  + 
                  (1 - eta)*lowerBoundaryPos.Y  +  eta*upperBoundaryPos.Y  -  
                  (1 - ksi)*(1 - eta)*_quadrilateral._LL.Y - (1 - ksi)*eta*_quadrilateral._UL.Y -
                  (1 - eta)*ksi*_quadrilateral._LR.Y - ksi*eta*_quadrilateral._UR.Y;
         return new Pos(x, y);
      }
      protected MeshNode CreateNode(double ksi, double eta) {
         var upperBoundaryPos = CalcUpperBoundaryPos(ksi);
         var lowerBoundaryPos = CalcLowerBoundaryPos(ksi);
         var leftBoundaryPos = CalcLeftBoundaryPos(eta);
         var rightBoundaryPos = CalcRightBoundaryPos(eta);
         var x = (1 - ksi)*leftBoundaryPos.X  +  ksi*rightBoundaryPos.X  + 
                  (1 - eta)*lowerBoundaryPos.X  +  eta*upperBoundaryPos.X  -  
                  (1 - ksi)*(1 - eta)*_quadrilateral._LL.X - (1 - ksi)*eta*_quadrilateral._UL.X -
                  (1 - eta)*ksi*_quadrilateral._LR.X - ksi*eta*_quadrilateral._UR.X;
         var y = (1 - ksi)*leftBoundaryPos.Y  +  ksi*rightBoundaryPos.Y  + 
                  (1 - eta)*lowerBoundaryPos.Y  +  eta*upperBoundaryPos.Y  -  
                  (1 - ksi)*(1 - eta)*_quadrilateral._LL.Y - (1 - ksi)*eta*_quadrilateral._UL.Y -
                  (1 - eta)*ksi*_quadrilateral._LR.Y - ksi*eta*_quadrilateral._UR.Y;
         return new MeshNode(x, y, MainMesh.M);
      }
   }
}