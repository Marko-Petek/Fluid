using System;
using static System.Math;

namespace Fluid.Internals
{
   public static class ArrayOperations
   {
      /// <summary>Extends the capacity of an array if it does not satisfy the specified capacity.</summary>
      public static void EnsureArrayCapacity<T>(ref T[] array, int capacity) {
         if (array.Length < capacity) {
            var biggerArray = new T[2 * array.Length];
            Array.Copy(array, biggerArray, array.Length);
            array = biggerArray;
         }
      }
      
      /// <summary>
      /// Swaps two elements in a 2D array.
      /// </summary>
      /// <param name="array"></param>
      /// <param name="index11"></param>
      /// <param name="index12"></param>
      /// <param name="index21"></param>
      /// <param name="index22"></param>
      /// <typeparam name="T"></typeparam>
      public static void Swap<T>(this T[][] array, int index11, int index12, int index21, int index22) {
         T temp = array[index21][index22];
         array[index21][index22] = array[index11][index12];
         array[index11][index12] = temp;
      }

      public static bool Equals(this double[] M, double[] other, double epsilon) {
         if(M.Length == other.Length) {
            for(int i = 0; i < M.Length; ++i) {
               if(Abs(M[i] - other[i]) > epsilon)
                  return false;
            }
            return true;
         }
         return false;
      }

      public static bool Equals(this double[][] M, double[][] other, double epsilon) {
         if(M.Length == other.Length) {
            for(int i = 0; i < M.Length; ++i) {
               if(!Equals(M[i], other[i], epsilon))
                  return false;
            }
            return true;
         }
         return false;
      }

      public static bool Equals(this double[][][] M, double[][][] other, double epsilon) {
         if(M.Length == other.Length) {
            for(int i = 0; i < M.Length; ++i) {
               if(!Equals(M[i], other[i], epsilon))
                  return false;
            }
            return true;
         }
         return false;
      }
   }
}
