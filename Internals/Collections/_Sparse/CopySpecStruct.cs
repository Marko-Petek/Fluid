using System;

namespace Fluid.Internals.Collections {
/// <summary>Structure that tells Tensor's Copy method how to copy a tensor.</summary>
public readonly struct CopySpecStruct {
   /// <summary>Specify which types of fields to copy: value, non-value or both.</summary>
   public readonly WhichFields FieldsSpec;
   /// <summary>Specify which non-value fields to copy (and conversely, which to omit). Effective only if General.Meta flag is active.</summary>
   public readonly WhichNonValueFields NonValueFieldsSpec;
   /// <summary>Structure CopySpec specifies whether  to genuinely copy the Structure int[] array or only copy the reference.  Contains flags: TrueCopy (creates a new Structure array on the heap), RefCopy (copies only a reference to the Structure array on source).</summary>
   /// <remarks>Effective only if MetaFields.Structure flag is active.</remarks>
   public readonly HowToCopyStructure StructureSpec;
   /// <summary>Lowest rank at which copying of values stops.</summary>
   /// <remarks>Effective only if General.Vals flag is active. EndRank is an inclusive lower bound.</remarks>
   public readonly int EndRank;
   public readonly int ExtraCapacity;
   
   public CopySpecStruct(
      WhichFields whichFields = WhichFields.ValuesAndNonValueFields,
      WhichNonValueFields whichNonValueFields = WhichNonValueFields.All,
      HowToCopyStructure howToCopyStructure = HowToCopyStructure.CreateNewStructure,
      int endRank = 0,
      int extraCapacity = 0)
   {
      FieldsSpec = whichFields;
      NonValueFieldsSpec = whichNonValueFields;
      StructureSpec = howToCopyStructure;
      EndRank = endRank;
      ExtraCapacity = extraCapacity;
   }
}

/// <summary>Specify which types of fields to copy: value, non-value or both.</summary>
   [Flags] public enum WhichFields {
      /// <summary>(1) Copy only the value field (direct subtensors).</summary>
      OnlyValues  = 1,                                               // 1
      /// <summary>(2) Copy only the non-value fields (Structure, Rank, Superior).</summary>
      OnlyNonValueFields  = 1 << 1,                                  // 2
      /// <summary>(3) - Copy both the value field (direct subtensors) and non-value fields (Structure, Rank, Superior).</summary>
      ValuesAndNonValueFields = OnlyNonValueFields | OnlyValues      // 3
   }
   /// <summary>Specify which non-value fields to copy (and conversely, which to omit).</summary>
   [Flags] public enum WhichNonValueFields {
      /// <summary>0 - Copy no non-value fields.</summary>
      None               = 0,                               // 0
      /// <summary>(1) Copy only the Struture field, leaving Rank and Superior uninitialized.</summary>
      Structure          = 1,                               // 1
      /// <summary>(2) Copy only the Rank field, leaving Structure and Superior uninitialized..</summary>
      Rank               = 1 << 1,                          // 2
      /// <summary>(3) Copy only the Superior field, levaing Rank and Structure uninitialized.</summary>
      Superior           = 1 << 2,                          // 3
      /// <summary>(4) Copy all non-value fields: Structure, Rank and Superior.</summary>
      All                = Structure | Rank | Superior,     // 4
      /// <summary>(5) Copy the Rank and Structure fields, but leave Superior uninitialized.</summary>
      AllExceptSuperior  = All & ~Superior                  // 5
   }
   /// <summary>Specifies whether to make.</summary>
   [Flags] public enum HowToCopyStructure {
      /// <summary>(0) Do not copy structure.</summary>
      DoNotCopy = 0,
      /// <summary>(1) Copy only the reference to the existing structure on the original tensor.</summary>
      ReferToOriginalStructure  = 1,            // 1
      /// <summary>(2) Create a fresh copy of the structure on the original tensor and assign its reference to the field.</summary>
      CreateNewStructure = 1 << 1,              // 2
   }
}