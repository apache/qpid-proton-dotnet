/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;

namespace Apache.Qpid.Proton.Utilities
{
   /// <summary>
   /// Static utility methods for validation and or interactions with
   /// various opaque object vales.
   /// </summary>
   public static class Statics
   {
      /// <summary>
      /// Creates and returns a copy of the input array with either a truncated
      /// view if the length value passed is less than the input array length or
      /// padded with default values for entries beyond the original array length.
      /// </summary>
      /// <typeparam name="T">The type of array this method is copying</typeparam>
      /// <param name="original">The original array to copy</param>
      /// <param name="newLength">The length of the new array to return</param>
      /// <returns>The copy or the original array with the specified length</returns>
      public static T[] CopyOf<T>(T[] original, int newLength)
      {
         T[] theCopy = new T[newLength];
         Array.ConstrainedCopy(original, 0, theCopy, 0, Math.Min(original.Length, theCopy.Length));
         return theCopy;
      }

      /// <summary>
      /// Copy values from the given array that fall within the given range to a new
      /// array that is sized to hold the copied range.
      /// </summary>
      /// <typeparam name="T">The type of the array being copied</typeparam>
      /// <param name="original">The original array where the elements are copied from</param>
      /// <param name="from">The starting index in the array to begin the copy</param>
      /// <param name="to">The ending index in the array for the copy</param>
      /// <returns>A new array that contains a copy of the specified subregion of the original array</returns>
      /// <exception cref="ArgumentOutOfRangeException">If the from index is greater than the to index</exception>
      public static T[] CopyOfRange<T>(T[] original, int from, int to)
      {
         int newLength = to - from;
         if (newLength < 0)
         {
            throw new ArgumentOutOfRangeException("Value of " + from + " > " + to);
         }

         T[] copy = newLength == 0 ? Array.Empty<T>() : new T[newLength];
         Array.ConstrainedCopy(original, from, copy, 0, Math.Min(original.Length - from, newLength));
         return copy;
      }

      /// <summary>
      /// Checks if the given value is greater than zero and throws an exception if not.
      /// </summary>
      /// <param name="value">The value to check</param>
      /// <param name="name">The name to use in the exception message for the given value</param>
      /// <returns>The value provided if it passes the check</returns>
      /// <exception cref="ArgumentOutOfRangeException">if the value fails the check</exception>
      public static int CheckPositive(int value, string name)
      {
         if (value <= 0)
         {
            throw new ArgumentOutOfRangeException(name + " : " + value + " (expected: > 0)");
         }
         return value;
      }

      /// <summary>
      /// Checks if the given value is greater than zero and throws an exception if not.
      /// </summary>
      /// <param name="value">The value to check</param>
      /// <param name="name">The name to use in the exception message for the given value</param>
      /// <returns>The value provided if it passes the check</returns>
      /// <exception cref="ArgumentOutOfRangeException">if the value fails the check</exception>
      public static long CheckPositive(long value, string name)
      {
         if (value <= 0L)
         {
            throw new ArgumentOutOfRangeException(name + " : " + value + " (expected: > 0)");
         }
         return value;
      }

      /// <summary>
      /// Checks if the given value is less than zero and throws an exception if so.
      /// </summary>
      /// <param name="value">The value to check</param>
      /// <param name="name">The name to use in the exception message for the given value</param>
      /// <returns>The value provided if it passes the check</returns>
      /// <exception cref="ArgumentOutOfRangeException">if the value fails the check</exception>
      public static int CheckPositiveOrZero(int value, string name)
      {
         if (value < 0)
         {
            throw new ArgumentOutOfRangeException(name + " : " + value + " (expected: >= 0)");
         }
         return value;
      }

      /// <summary>
      /// Checks if the given value is less than zero and throws an exception if so.
      /// </summary>
      /// <param name="value">The value to check</param>
      /// <param name="name">The name to use in the exception message for the given value</param>
      /// <returns>The value provided if it passes the check</returns>
      /// <exception cref="ArgumentOutOfRangeException">if the value fails the check</exception>
      public static long CheckPositiveOrZero(long value, string name)
      {
         if (value < 0L)
         {
            throw new ArgumentOutOfRangeException(name + " : " + value + " (expected: >= 0)");
         }
         return value;
      }

      /// <summary>
      /// Checks if the range specified by the provided index + the size of the
      /// range provided is within the provided length.  The method considers the
      /// index as being within the range specified and considers zero as a value
      /// that lies within the range of valid lengths.
      /// </summary>
      /// <param name="index">The index where the operation will begin</param>
      /// <param name="size">The size of the range that defines the operation</param>
      /// <param name="length">The available length of the target of the operation</param>
      /// <returns>The given index if the operation is deemed valid.</returns>
      public static int CheckFromIndexSize(int index, int size, int length)
      {
         if ((length | index | size) < 0 || size > (length - index))
         {
            throw new ArgumentOutOfRangeException(string.Format(
               "The given range specified by index {0} + size {1} is outside the specified region length {2}",
               index, size, length));
         }
         return index;
      }

      /// <summary>
      /// Checks if the range specified by the provided index + the size of the
      /// range provided is within the provided length.  The method considers the
      /// index as being within the range specified and considers zero as a value
      /// that lies within the range of valid lengths.
      /// </summary>
      /// <param name="index">The index where the operation will begin</param>
      /// <param name="size">The size of the range that defines the operation</param>
      /// <param name="length">The available length of the target of the operation</param>
      /// <returns>The given index if the operation is deemed valid.</returns>
      public static long CheckFromIndexSize(long index, long size, long length)
      {
         if ((length | index | size) < 0 || size > (length - index))
         {
            throw new ArgumentOutOfRangeException(string.Format(
               "The given range specified by index {0} + size {1} is outside the specified region length {2}",
               index, size, length));
         }
         return index;
      }

      /// <summary>
      /// Used to check for a method argument being null when the precondition
      /// states that it should not be, the method provides for the user to supply
      /// a message to use when throwing an Argument null exception if the value
      /// that was passed is indeed null.
      /// </summary>
      /// <param name="value">The value to check for null</param>
      /// <param name="errorMessage">The message to supply when throwing an error</param>
      /// <exception cref="ArgumentNullException">If the given value is null</exception>
      public static void RequireNonNull(object value, string errorMessage)
      {
         if (value == null)
         {
            throw new ArgumentNullException(errorMessage ?? "Value provided cannot be null");
         }
      }

      /// <summary>
      /// Add the two numbers and return the result if there was no overflow, otherwise
      /// throw an exception.
      /// </summary>
      /// <param name="x">The first value to add</param>
      /// <param name="y">The second value to add</param>
      /// <returns>The resulting summed value if no overflow occurred</returns>
      /// <exception cref="ArithmeticException">If the addition resulted in an overflow</exception>
      internal static int AddExact(int x, int y)
      {
         int r = x + y;
         // HD 2-12 Overflow if both arguments have the opposite sign of the result
         if (((x ^ r) & (y ^ r)) < 0)
         {
            throw new ArithmeticException("integer overflow");
         }
         return r;
      }

      /// <summary>
      /// Add the two numbers and return the result if there was no overflow, otherwise
      /// throw an exception.
      /// </summary>
      /// <param name="x">The first value to add</param>
      /// <param name="y">The second value to add</param>
      /// <returns>The resulting summed value if no overflow occurred</returns>
      /// <exception cref="ArithmeticException">If the addition resulted in an overflow</exception>
      internal static long AddExact(long x, long y)
      {
         long r = x + y;
         // HD 2-12 Overflow if both arguments have the opposite sign of the result
         if (((x ^ r) & (y ^ r)) < 0)
         {
            throw new ArithmeticException("integer overflow");
         }
         return r;
      }

      /// <summary>
      /// Used to check that the given sequence within an array of elements is equal
      /// using the default equality comparison mechanism.
      /// </summary>
      /// <param name="a"></param>
      /// <param name="aFromIndex"></param>
      /// <param name="aToIndex"></param>
      /// <param name="b"></param>
      /// <param name="bFromIndex"></param>
      /// <param name="bToIndex"></param>
      /// <returns></returns>
      public static bool SequenceEquals<T>(T[] a, int aFromIndex, int aToIndex,
                                           T[] b, int bFromIndex, int bToIndex)
      {
         RangeCheck(a.Length, aFromIndex, aToIndex);
         RangeCheck(b.Length, bFromIndex, bToIndex);

         int aLength = aToIndex - aFromIndex;
         int bLength = bToIndex - bFromIndex;
         if (aLength != bLength)
         {
            return false;
         }

         EqualityComparer<T> comparer = EqualityComparer<T>.Default;
         for (int i = 0; i < aLength; i++)
         {
            if (!comparer.Equals(a[aFromIndex + i], b[bFromIndex + i]))
            {
               return false;
            }
         }

         return true;
      }

      /// <summary>
      /// Validate that the range given by a from index and a to index are within the bounds
      /// of the length value provided.
      /// </summary>
      /// <param name="arrayLength"></param>
      /// <param name="fromIndex"></param>
      /// <param name="toIndex"></param>
      /// <exception cref="ArgumentOutOfRangeException"></exception>
      internal static void RangeCheck(int arrayLength, int fromIndex, int toIndex)
      {
         if (fromIndex > toIndex)
         {
            throw new ArgumentOutOfRangeException(
                    "fromIndex(" + fromIndex + ") > toIndex(" + toIndex + ")");
         }
         if (fromIndex < 0)
         {
            throw new ArgumentOutOfRangeException("Given from index is negative: " + fromIndex);
         }
         if (toIndex > arrayLength)
         {
            throw new ArgumentOutOfRangeException("Given to index is greater than the length: " + fromIndex);
         }
      }
   }
}