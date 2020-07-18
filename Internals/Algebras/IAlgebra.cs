using System;
using System.Diagnostics.CodeAnalysis;
using dbl = System.Double;
using Fluid.Internals.Numerics;

namespace Fluid.Internals.Algebras {
   /// <summary>Performs arithmetic on operands of type T.</summary><typeparam name="τ">Operand and result type.</typeparam>
   public interface IAlgebra<τ> {
      [return: MaybeNull]
      τ Sum([AllowNull] τ x1, [AllowNull] τ x2);     // With AllowNull we signal that the method promises to be implemented to accept null when generic τ is a reference type: it treats null like a zero.
      
      [return: MaybeNull]
      τ Sub([AllowNull] τ x1, [AllowNull] τ x2);
      
      [return: MaybeNull]
      τ Mul([AllowNull] τ x1, [AllowNull] τ x2);
      
      [return: MaybeNull]
      [return: NotNullIfNotNull("x1")]
      τ Div([AllowNull] τ x1, [AllowNull] τ x2);
      
      [return: MaybeNull]
      [return: NotNullIfNotNull("x")]
      τ Abs([AllowNull] τ x);
      
      [return: MaybeNull]
      [return: NotNullIfNotNull("x")]
      τ Neg([AllowNull] τ x);
      
      [NotNull]                     // Getter never returns null.
      τ Unit { get; }
      
      [MaybeNull]                   // Getter may return null (when we are dealing with a reference type).
      τ Zero { get; }

      bool Equal([AllowNull] τ x1, [AllowNull] τ x2);

      bool IsZero([NotNullWhen(false)] [AllowNull] τ x);          // If the method returns false, x is not null when a ref type.
   
      int Compare([AllowNull] τ x1, [AllowNull] τ x2);
   }

   

   

   

   

   

   
}