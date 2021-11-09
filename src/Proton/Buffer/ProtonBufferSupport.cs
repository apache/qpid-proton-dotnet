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
using Apache.Qpid.Proton.Utilities;

namespace Apache.Qpid.Proton.Buffer
{
   public static class ProtonBufferSupport
   {
      /// <summary>
      /// Throws an exception if the user supplied length value if negative.
      /// </summary>
      /// <param name="offset">User supplied value be validated</param>
      /// <exception cref="ArgumentOutOfRangeException">If the value is negative</exception>
      public static void CheckLength(long length)
      {
         if (length < 0)
         {
            throw new ArgumentOutOfRangeException(
               string.Format("The length value cannot be negative: {0}", length));
         }
      }

      /// <summary>
      /// Throws an exception if the user supplied offset value if negative.
      /// </summary>
      /// <param name="offset">User supplied value be validated</param>
      /// <exception cref="ArgumentOutOfRangeException">If the value is negative</exception>
      public static void CheckOffset(long offset)
      {
         if (offset < 0)
         {
            throw new ArgumentOutOfRangeException(
               string.Format("The offset value cannot be negative: {0}", offset));
         }
      }

      /// <summary>
      /// Checks two proton buffers for byte level equality, each buffer must have
      /// the same number of readable bytes otherwise they are considered not equal
      /// </summary>
      /// <param name="bufferA">The first buffer to compare to the second</param>
      /// <param name="bufferB">The second buffer to compare to the first</param>
      /// <returns>true if the buffer contents are equal</returns>
      public static bool Equals(IProtonBuffer bufferA, IProtonBuffer bufferB)
      {
         if ((bufferA == null && bufferB != null) || (bufferB == null && bufferA != null))
         {
            return false;
         }

         if (bufferA == bufferB)
         {
            return true;
         }

         long aLen = bufferA.ReadableBytes;
         if (aLen != bufferB.ReadableBytes)
         {
            return false;
         }

         return Equals(bufferA, bufferA.ReadOffset, bufferB, bufferB.ReadOffset, aLen);
      }

      /// <summary>
      /// Compares the bytes within a region of two separate buffers for equality and returns
      /// true if the bytes therein are equal.  The start index does not need to be within the
      /// current range of readable bytes but is constrained to the limit of the current write
      /// offset.
      /// </summary>
      /// <param name="a">The first buffer</param>
      /// <param name="aStartIndex">The offset into the first buffer to start</param>
      /// <param name="b">The second buffer</param>
      /// <param name="bStartIndex">The offset into the second buffer to start</param>
      /// <param name="length">The number of bytes in the given buffers to compare</param>
      /// <returns>True if the bytes in the given subsection of the buffers are equal</returns>
      public static bool Equals(IProtonBuffer a, long aStartIndex, IProtonBuffer b, long bStartIndex, long length)
      {
         Statics.RequireNonNull(a, "a");
         Statics.RequireNonNull(b, "b");
         // All indexes and lengths must be non-negative
         Statics.CheckPositiveOrZero(aStartIndex, "aStartIndex");
         Statics.CheckPositiveOrZero(bStartIndex, "bStartIndex");
         Statics.CheckPositiveOrZero(length, "length");

         if (a.WriteOffset - length < aStartIndex || b.WriteOffset - length < bStartIndex)
         {
            return false;
         }

         long longCount = length >> 3;
         long byteCount = length & 7;

         for (long i = longCount; i > 0; i--)
         {
            if (a.GetLong(aStartIndex) != b.GetLong(bStartIndex))
            {
               return false;
            }
            aStartIndex += 8;
            bStartIndex += 8;
         }

         for (long i = byteCount; i > 0; i--)
         {
            if (a.GetByte(aStartIndex) != b.GetByte(bStartIndex))
            {
               return false;
            }
            aStartIndex++;
            bStartIndex++;
         }

         return true;
      }

      /// <summary>
      /// Computes a hash code using the bytes contained within the given
      /// proton buffer instance.
      /// </summary>
      /// <param name="buffer">The buffer whose bytes are to be read</param>
      /// <returns>The hash code computed from the buffer contents</returns>
      public static int GetHashCode(IProtonBuffer buffer)
      {
         long aLen = buffer.ReadableBytes;
         long intCount = aLen >> 2;
         long byteCount = aLen & 3;

         int hashCode = 0;
         long arrayIndex = buffer.ReadOffset;
         for (long i = intCount; i > 0; i--)
         {
            hashCode = 31 * hashCode + buffer.GetInt(arrayIndex);
            arrayIndex += 4;
         }

         for (long i = byteCount; i > 0; i--)
         {
            hashCode = 31 * hashCode + buffer.GetByte(arrayIndex++);
         }

         if (hashCode == 0)
         {
            hashCode = 1;
         }

         return hashCode;
      }
   }
}