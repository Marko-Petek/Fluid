# Block Structured Mesh

All indices are zero based.

**BlockMesh.cs** holds free nodes in a 2nd rank tensor *u*<sub>f</sub>*<sup>δl</sup>*. The first slot index *δ* chooses between *N* node positions, the second slot index *l* chooses between *m* variables. It stores constrained nodes in a 2nd rank tensor *u*<sub>c</sub>*<sup>δl</sup>*

A 1st rank array *x*<sub>f</sub>*<sup>δ</sup>* holding node positions, where *δ* again chooses between *N* nodes.
