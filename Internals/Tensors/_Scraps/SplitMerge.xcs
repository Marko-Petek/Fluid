// #if false
// /// <summary>Splits a vector into two vectors. Caller (left remainder) is modified, while right remainder is returned as a separate vector re-indexed from 0.</summary>
// /// <param name="inx">Element at this index will end up as part of right remainder.</param>
// public Tensor<τ> SplitAt(int inx) {
//    var remTen1 = CreateNew(Dim - inx);
//    foreach(var kvPair in this.Where(pair => pair.Key >= inx))
//       remTen1.Add(kvPair.Key - inx, kvPair.Value);                         // Add to right remainder.
//    foreach(var key in remTen1.Keys)
//       Remove(key + inx);                                          // Must not modify collection during enumeration. Therefore entries have to be removed from left remainder afterwards.
//    Dim = inx;
//    return remTen1;
// }
// #endif