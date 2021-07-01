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

namespace Apache.Qpid.Proton.Buffer
{
   /// <summary>
   /// An buffer allocator instance that creates heap based buffer objects
   /// </summary>
   public static class ProtonByteUtils
   {
      /// <summary>
      /// Converts the given value into a byte array.
      /// </summary>
      /// <param name="value">the value to convert to a byte array</param>
      /// <returns>the new byte array containing the given value</returns>
      public static byte[] ToByteArray(sbyte value)
      {
         return WriteByte(value, new byte[sizeof(byte)], 0);
      }

      /// <summary>
      /// Converts the given value into a byte array.
      /// </summary>
      /// <param name="value">the value to convert to a byte array</param>
      /// <returns>the new byte array containing the given value</returns>
      public static byte[] ToByteArray(byte value)
      {
         return WriteUnsignedByte(value, new byte[sizeof(byte)], 0);
      }

      /// <summary>
      /// Converts the given value into a byte array.
      /// </summary>
      /// <param name="value">the value to convert to a byte array</param>
      /// <returns>the new byte array containing the given value</returns>
      public static byte[] ToByteArray(short value)
      {
         return WriteShort(value, new byte[sizeof(short)], 0);
      }

      /// <summary>
      /// Converts the given value into a byte array.
      /// </summary>
      /// <param name="value">the value to convert to a byte array</param>
      /// <returns>the new byte array containing the given value</returns>
      public static byte[] ToByteArray(ushort value)
      {
         return WriteUnsignedShort(value, new byte[sizeof(short)], 0);
      }

      /// <summary>
      /// Converts the given value into a byte array.
      /// </summary>
      /// <param name="value">the value to convert to a byte array</param>
      /// <returns>the new byte array containing the given value</returns>
      public static byte[] ToByteArray(int value)
      {
         return WriteInt(value, new byte[sizeof(int)], 0);
      }

      /// <summary>
      /// Converts the given value into a byte array.
      /// </summary>
      /// <param name="value">the value to convert to a byte array</param>
      /// <returns>the new byte array containing the given value</returns>
      public static byte[] ToByteArray(uint value)
      {
         return WriteUnsignedInt(value, new byte[sizeof(int)], 0);
      }

      /// <summary>
      /// Converts the given value into a byte array.
      /// </summary>
      /// <param name="value">the value to convert to a byte array</param>
      /// <returns>the new byte array containing the given value</returns>
      public static byte[] ToByteArray(long value)
      {
         return WriteLong(value, new byte[sizeof(long)], 0);
      }

      /// <summary>
      /// Converts the given value into a byte array.
      /// </summary>
      /// <param name="value">the value to convert to a byte array</param>
      /// <returns>the new byte array containing the given value</returns>
      public static byte[] ToByteArray(ulong value)
      {
         return WriteUnsignedLong(value, new byte[sizeof(long)], 0);
      }

      /// <summary>
      /// Writes the given value into the provided byte array at the target offset.
      /// </summary>
      /// <param name="value">The value to write into the array</param>
      /// <param name="destination">The destination where the value should be written</param>
      /// <param name="offset">the offset into the destination to start writing</param>
      /// <returns></returns>
      public static byte[] WriteByte(sbyte value, byte[] destination, int offset)
      {
         return WriteUnsignedByte((byte)value, destination, offset);
      }

      /// <summary>
      /// Writes the given value into the provided byte array at the target offset.
      /// </summary>
      /// <param name="value">The value to write into the array</param>
      /// <param name="destination">The destination where the value should be written</param>
      /// <param name="offset">the offset into the destination to start writing</param>
      /// <returns></returns>
      public static byte[] WriteUnsignedByte(byte value, byte[] destination, int offset)
      {
         destination[offset] = value;

         return destination;
      }

      /// <summary>
      /// Writes the given value into the provided byte array at the target offset.
      /// </summary>
      /// <param name="value">The value to write into the array</param>
      /// <param name="destination">The destination where the value should be written</param>
      /// <param name="offset">the offset into the destination to start writing</param>
      /// <returns>The byte array passed where the value was written</returns>
      public static byte[] WriteShort(short value, byte[] destination, int offset)
      {
         return WriteUnsignedShort((ushort)value, destination, offset);
      }

      /// <summary>
      /// Writes the given value into the provided byte array at the target offset.
      /// </summary>
      /// <param name="value">The value to write into the array</param>
      /// <param name="destination">The destination where the value should be written</param>
      /// <param name="offset">the offset into the destination to start writing</param>
      /// <returns>The byte array passed where the value was written</returns>
      public static byte[] WriteUnsignedShort(ushort value, byte[] destination, int offset)
      {
         destination[offset++] = (byte)(value >> 8);
         destination[offset++] = (byte)(value >> 0);

         return destination;
      }

      /// <summary>
      /// Writes the given value into the provided byte array at the target offset.
      /// </summary>
      /// <param name="value">The value to write into the array</param>
      /// <param name="destination">The destination where the value should be written</param>
      /// <param name="offset">the offset into the destination to start writing</param>
      /// <returns>The byte array passed where the value was written</returns>
      public static byte[] WriteInt(int value, byte[] destination, int offset)
      {
         return WriteUnsignedInt((uint)value, destination, offset);
      }

      /// <summary>
      /// Writes the given value into the provided byte array at the target offset.
      /// </summary>
      /// <param name="value">The value to write into the array</param>
      /// <param name="destination">The destination where the value should be written</param>
      /// <param name="offset">the offset into the destination to start writing</param>
      /// <returns>The byte array passed where the value was written</returns>
      public static byte[] WriteUnsignedInt(uint value, byte[] destination, int offset)
      {
         destination[offset++] = (byte)(value >> 24);
         destination[offset++] = (byte)(value >> 16);
         destination[offset++] = (byte)(value >> 8);
         destination[offset++] = (byte)(value >> 0);

         return destination;
      }

      /// <summary>
      /// Writes the given value into the provided byte array at the target offset.
      /// </summary>
      /// <param name="value">The value to write into the array</param>
      /// <param name="destination">The destination where the value should be written</param>
      /// <param name="offset">the offset into the destination to start writing</param>
      /// <returns>The byte array passed where the value was written</returns>
      public static byte[] WriteLong(long value, byte[] destination, int offset)
      {
         return WriteUnsignedLong((ulong)value, destination, offset);
      }

      /// <summary>
      /// Writes the given value into the provided byte array at the target offset.
      /// </summary>
      /// <param name="value">The value to write into the array</param>
      /// <param name="destination">The destination where the value should be written</param>
      /// <param name="offset">the offset into the destination to start writing</param>
      /// <returns>The byte array passed where the value was written</returns>
      public static byte[] WriteUnsignedLong(ulong value, byte[] destination, int offset)
      {
         destination[offset++] = (byte)(value >> 56);
         destination[offset++] = (byte)(value >> 48);
         destination[offset++] = (byte)(value >> 40);
         destination[offset++] = (byte)(value >> 32);
         destination[offset++] = (byte)(value >> 24);
         destination[offset++] = (byte)(value >> 16);
         destination[offset++] = (byte)(value >> 8);
         destination[offset++] = (byte)(value >> 0);

         return destination;
      }

      /// <summary>
      /// Reads the value from the given array and returns it
      /// </summary>
      /// <param name="array">The array where the value should be read</param>
      /// <param name="offset">The offset into the array where the value is read from</param>
      /// <returns>The value read from the given array</returns>
      public static sbyte ReadByte(byte[] array, int offset)
      {
         return (sbyte)array[offset];
      }

      /// <summary>
      /// Reads the value from the given array and returns it
      /// </summary>
      /// <param name="array">The array where the value should be read</param>
      /// <param name="offset">The offset into the array where the value is read from</param>
      /// <returns>The value read from the given array</returns>
      public static byte ReadUnsignedByte(byte[] array, int offset)
      {
         return array[offset];
      }

      /// <summary>
      /// Reads the value from the given array and returns it
      /// </summary>
      /// <param name="array">The array where the value should be read</param>
      /// <param name="offset">The offset into the array where the value is read from</param>
      /// <returns>The value read from the given array</returns>
      public static short ReadShort(byte[] array, int offset)
      {
         return (short)((array[offset++] & 0xFF) << 8 |
                         (array[offset++] & 0xFF) << 0);
      }

      /// <summary>
      /// Reads the value from the given array and returns it
      /// </summary>
      /// <param name="array">The array where the value should be read</param>
      /// <param name="offset">The offset into the array where the value is read from</param>
      /// <returns>The value read from the given array</returns>
      public static ushort ReadUnsignedShort(byte[] array, int offset)
      {
         return (ushort)((array[offset++] & 0xFF) << 8 |
                          (array[offset++] & 0xFF) << 0);
      }

      /// <summary>
      /// Reads the value from the given array and returns it
      /// </summary>
      /// <param name="array">The array where the value should be read</param>
      /// <param name="offset">The offset into the array where the value is read from</param>
      /// <returns>The value read from the given array</returns>
      public static int ReadInt(byte[] array, int offset)
      {
         return (array[offset++] & 0xFF) << 24 |
                (array[offset++] & 0xFF) << 16 |
                (array[offset++] & 0xFF) << 8 |
                (array[offset++] & 0xFF) << 0;
      }

      /// <summary>
      /// Reads the value from the given array and returns it
      /// </summary>
      /// <param name="array">The array where the value should be read</param>
      /// <param name="offset">The offset into the array where the value is read from</param>
      /// <returns>The value read from the given array</returns>
      public static uint ReadUnsignedInt(byte[] array, int offset)
      {
         return (uint)((array[offset++] & 0xFF) << 24 |
                       (array[offset++] & 0xFF) << 16 |
                       (array[offset++] & 0xFF) << 8 |
                       (array[offset++] & 0xFF) << 0);
      }

      /// <summary>
      /// Reads the value from the given array and returns it
      /// </summary>
      /// <param name="array">The array where the value should be read</param>
      /// <param name="offset">The offset into the array where the value is read from</param>
      /// <returns>The value read from the given array</returns>
      public static long ReadLong(byte[] array, int offset)
      {
         return (long)(array[offset++] & 0xFF) << 56 |
                (long)(array[offset++] & 0xFF) << 48 |
                (long)(array[offset++] & 0xFF) << 40 |
                (long)(array[offset++] & 0xFF) << 32 |
                (long)(array[offset++] & 0xFF) << 24 |
                (long)(array[offset++] & 0xFF) << 16 |
                (long)(array[offset++] & 0xFF) << 8 |
                (long)(array[offset++] & 0xFF) << 0;
      }

      /// <summary>
      /// Reads the value from the given array and returns it
      /// </summary>
      /// <param name="array">The array where the value should be read</param>
      /// <param name="offset">The offset into the array where the value is read from</param>
      /// <returns>The value read from the given array</returns>
      public static ulong ReadUnsignedLong(byte[] array, int offset)
      {
         return (ulong)((long)(array[offset++] & 0xFF) << 56 |
                        (long)(array[offset++] & 0xFF) << 48 |
                        (long)(array[offset++] & 0xFF) << 40 |
                        (long)(array[offset++] & 0xFF) << 32 |
                        (long)(array[offset++] & 0xFF) << 24 |
                        (long)(array[offset++] & 0xFF) << 16 |
                        (long)(array[offset++] & 0xFF) << 8 |
                        (long)(array[offset++] & 0xFF) << 0);
      }
   }
}