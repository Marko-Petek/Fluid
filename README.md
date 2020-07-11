# Fluid Dynamics using LSFEM

A textbook simulation of fluid flow around a cylinder in 2D based on LSFEM not (the usual) GFEM. The project started as an assignment for "Applications in Physics" but turned out to be more work than expected. I'm currently writing the theory behind simulation in LaTex (Slovene) and working towards a first functioning version of the program.

View or edit by installing .NET Core 2.2 and Visual Studio Code (VSCode), installing the C# VSCode extension and opening the workspace file "Fluid.code-workspace". I've made a custom theme for VSCode, so it looks nicer. Theory is in Fluid/Seminar/preamble.pdf.

I ran out of time to implement a proximity tree so I'm using a NuGet package: Supercluster.KDTree (MIT license).