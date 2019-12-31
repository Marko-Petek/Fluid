namespace Fluid.Internals.Collections {
   public static class CopySpecs {
      /// <summary>Copies everything as is. Non-sensical. Copies superior, but the structure is created anew - ensuring errors. Structure should always refer to top-most tensor's structure.</summary>
      public static readonly CopySpecStruct S342_00 = new CopySpecStruct(        // FIXME: Is this needed?
         WhichFields.Both,
         WhichNonValueFields.All,
         HowToCopyStructure.CreateNewStructure,
         endRank: 0,
         extraCapacity: 0);
      /// <summary>Copies everything as is, adds 4 extra spaces to internal dictionary. Non-sensical. Copies superior, but the structure is created anew - ensuring errors. Structure should always refer to top-most tensor's structure.</summary>
      public static readonly CopySpecStruct S342_04 = new CopySpecStruct(
         WhichFields.Both,
         WhichNonValueFields.All,
         HowToCopyStructure.CreateNewStructure,
         endRank: 0,
         extraCapacity: 4);
      /// <summary>Copy values and rank, leave Structure ans Superior unassigned.</summary>
      public static readonly CopySpecStruct S320_00 = new CopySpecStruct(
         WhichFields.Both,
         WhichNonValueFields.Rank,
         HowToCopyStructure.DoNotCopy,
         endRank: 0,
         extraCapacity: 0);
      /// <summary>Copy values (adds 4 extra capacity) and rank, leave Structure and Superior unassigned.</summary>
      public static readonly CopySpecStruct S320_04 = new CopySpecStruct(
         WhichFields.Both,
         WhichNonValueFields.Rank,
         HowToCopyStructure.DoNotCopy,
         endRank: 0,
         extraCapacity: 4);
      ///<summary>Copy values, rank, and structure (brand new created), but not superior.</summary>
      public static readonly CopySpecStruct S352_00 = new CopySpecStruct(
         WhichFields.Both,
         WhichNonValueFields.AllExceptSuperior,
         HowToCopyStructure.CreateNewStructure,
         endRank: 0,
         extraCapacity: 0
      );
   }
}