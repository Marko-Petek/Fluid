using System;
using static System.Math;

using Fluid.Internals.Collections;
using Fluid.Internals.Numerics;
using My = Fluid.Internals.Collections.Custom;

namespace Fluid.Internals.Lsfem {
   using dbl = Double;
   using Tensor = Tensor<double,DblArithmetic>;
   using Vector = Vector<double,DblArithmetic>;
   /// <summary>Represents a method that takes three indices and returns a position by reference.</summary>
   //public delegate ref MeshNode NodeDelegate(int blockRow, int blockCol, int index);

   // TODO: Implement abstract J(stdInx,p,q) and J(cmtInx,p,q) methods that return Jacobians for each element.

   /// <summary>A structured submesh that contains a map which maps two indices (row and column index) into a unique element index.</summary>
   public abstract class Block {
      /// <summary>Number of rows of elements.</summary>
      public int NRows { get; protected set; }
      /// <summary>Number of columns of elements.</summary>
      public int NCols { get; protected set; }
      /// <summary>Elements residing on the Block.</summary>
      protected My.List<Element> Elements { get; set; }
      

      /// <summary>Create a block.</summary>
      public Block() { }

      
      /// <summary>Creates all of the block's elements and fills them with non-shared nodes.</summary>
      protected abstract void CreateElements();


      /// <summary>Takes 3 corner positions and creates an element and 5 of its lower left nodes.</summary>
      /// <param name="stdRow">Element's row inside mesh block.</param>
      /// <param name="stdCol">Element's col inside mesh block.</param>
      Element CreateNonSharedElement(in Vec2 p0, in Vec2 p3, in Vec2 p9) {
         var quadElm = new Element(NodeStd(stdRow, stdCol, 0), NodeStd(stdRow, stdCol, 1),
            NodeStd(stdRow, stdCol, 2), NodeStd(stdRow, stdCol, 3), NodeStd(stdRow, stdCol, 4),
            NodeStd(stdRow, stdCol, 5), NodeStd(stdRow, stdCol, 6), NodeStd(stdRow, stdCol, 7),
            NodeStd(stdRow, stdCol, 8), NodeStd(stdRow, stdCol, 9), NodeStd(stdRow, stdCol, 10),
            NodeStd(stdRow, stdCol, 11) );
         return quadElm;
      }
      /// <summary>Transfer fully crafted elements to BlockMesh. Called by BlockMesh.</summary>
      /// <param name="elements">Elements list on BlockMesh.</param>
      internal void TransferElements(My.List<Element> elements) {

      }
   }
}