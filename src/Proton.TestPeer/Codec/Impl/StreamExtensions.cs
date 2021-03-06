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
using System.IO;

namespace Apache.Qpid.Proton.Test.Driver.Codec.Impl
{
   public static class StreamExtensions
   {
      private static readonly byte[] EmptyArray = Array.Empty<byte>();

      public static bool IsReadable(this Stream stream)
      {
         return stream.CanRead && stream.Position < stream.Length;
      }

      public static bool IsWritable(this Stream stream)
      {
         return stream.CanWrite;
      }

      public static long ReadIndex(this Stream stream)
      {
         return stream.Position;
      }

      public static long ReadIndex(this Stream stream, long index)
      {
         return stream.Position = index;
      }

      public static long ReadableBytes(this Stream stream)
      {
         return stream.CanRead ? stream.Length - stream.Position : 0L;
      }

      /// <summary>
      /// Reads a single byte from the given Stream and thrown a DecodeException if the
      /// Stream indicates an EOF condition was encountered.
      /// </summary>
      /// <param name="stream">The stream where the byte should be read from</param>
      /// <returns>An byte value read from the given stream</returns>
      /// <exception cref="IOException">If the value cannot be read from the stream</exception>
      public static EncodingCodes ReadEncodingCode(this Stream stream)
      {
         try
         {
            int result = stream.ReadByte();
            if (result >= 0)
            {
               return (EncodingCodes)result;
            }
            else
            {
               throw new EndOfStreamException("Cannot read more type information from stream that has reached its end.");
            }
         }
         catch (IOException ex)
         {
            throw new IOException("Caught IO error reading from provided stream", ex);
         }
      }

      /// <summary>
      /// Reads the given number of bytes from the provided Stream into an array and return
      /// that to the caller.  If the requested number of bytes cannot be read from the
      /// stream an DecodeException is thrown to indicate an underflow.
      /// </summary>
      /// <param name="stream">The stream where the bytes should be read from</param>
      /// <param name="length">the number of bytes to read from the stream</param>
      /// <returns>An array of bytes read from the given stream</returns>
      /// <exception cref="IOException">If the value cannot be read from the stream</exception>
      public static byte[] ReadBytes(this Stream stream, int length)
      {
         if (length == 0)
         {
            return EmptyArray;
         }
         else if (stream.Length < length)
         {
            throw new IOException(string.Format(
                  "Failed to read requested number of bytes {0}: instead only {1} bytes are ready.", length, stream.Length));
         }
         else
         {
            byte[] payload = new byte[length];

            if (stream.Read(payload) < length)
            {
               throw new IOException(string.Format(
                     "Failed to read requested number of bytes {0}: instead only {1} bytes were read.", length, payload.Length));
            }

            return payload;
         }
      }

      /// <summary>
      /// Reads a single byte from the given Stream and thrown a DecodeException if the
      /// Stream indicates an EOF condition was encountered.
      /// </summary>
      /// <param name="stream">The stream where the byte should be read from</param>
      /// <returns>An byte value read from the given stream</returns>
      /// <exception cref="IOException">If the value cannot be read from the stream</exception>
      public static sbyte ReadSignedByte(this Stream stream)
      {
         int result = stream.ReadByte();
         if (result >= 0)
         {
            return (sbyte)result;
         }
         else
         {
            throw new EndOfStreamException("Cannot read more type information from stream that has reached its end.");
         }
      }

      /// <summary>
      /// Reads a single byte from the given Stream and thrown a DecodeException if the
      /// Stream indicates an EOF condition was encountered.
      /// </summary>
      /// <param name="stream">The stream where the byte should be read from</param>
      /// <returns>An byte value read from the given stream</returns>
      /// <exception cref="IOException">If the value cannot be read from the stream</exception>
      public static byte ReadUnsignedByte(this Stream stream)
      {
         int result = stream.ReadByte();
         if (result >= 0)
         {
            return (byte)result;
         }
         else
         {
            throw new EndOfStreamException("Cannot read more type information from stream that has reached its end.");
         }
      }

      /// <summary>
      /// Reads a short value from the given Stream and thrown a DecodeException if the Stream
      /// indicates an EOF condition was encountered.
      /// </summary>
      /// <param name="stream">The stream where the value should be read from</param>
      /// <returns>An value read from the given stream</returns>
      /// <exception cref="DecodeException">If the value cannot be read from the stream</exception>
      public static short ReadShort(this Stream stream)
      {
         return (short)((ReadUnsignedByte(stream) & 0xFF) << 8 |
                        (ReadUnsignedByte(stream) & 0xFF) << 0);
      }

      /// <summary>
      /// Reads an unsigned short value from the given Stream and thrown a DecodeException if the
      /// Stream indicates an EOF condition was encountered.
      /// </summary>
      /// <param name="stream">The stream where the value should be read from</param>
      /// <returns>An value read from the given stream</returns>
      /// <exception cref="DecodeException">If the value cannot be read from the stream</exception>
      public static ushort ReadUnsignedShort(this Stream stream)
      {
         return (ushort)((ReadUnsignedByte(stream) & 0xFF) << 8 |
                         (ReadUnsignedByte(stream) & 0xFF) << 0);
      }

      /// <summary>
      /// Reads a int value from the given Stream and thrown a DecodeException if the Stream
      /// indicates an EOF condition was encountered.
      /// </summary>
      /// <param name="stream">The stream where the value should be read from</param>
      /// <returns>An value read from the given stream</returns>
      /// <exception cref="DecodeException">If the value cannot be read from the stream</exception>
      public static int ReadInt(this Stream stream)
      {
         return (ReadUnsignedByte(stream) & 0xFF) << 24 |
                (ReadUnsignedByte(stream) & 0xFF) << 16 |
                (ReadUnsignedByte(stream) & 0xFF) << 8 |
                (ReadUnsignedByte(stream) & 0xFF) << 0;
      }

      /// <summary>
      /// Reads a unsigned int value from the given Stream and thrown a DecodeException if the
      /// Stream indicates an EOF condition was encountered.
      /// </summary>
      /// <param name="stream">The stream where the value should be read from</param>
      /// <returns>An value read from the given stream</returns>
      /// <exception cref="DecodeException">If the value cannot be read from the stream</exception>
      public static uint ReadUnsignedInt(this Stream stream)
      {
         return (uint)(ReadUnsignedByte(stream) & 0xFF) << 24 |
                (uint)(ReadUnsignedByte(stream) & 0xFF) << 16 |
                (uint)(ReadUnsignedByte(stream) & 0xFF) << 8 |
                (uint)(ReadUnsignedByte(stream) & 0xFF) << 0;
      }

      /// <summary>
      /// Reads a long value from the given Stream and thrown a DecodeException if the Stream
      /// indicates an EOF condition was encountered.
      /// </summary>
      /// <param name="stream">The stream where the value should be read from</param>
      /// <returns>An value read from the given stream</returns>
      /// <exception cref="DecodeException">If the value cannot be read from the stream</exception>
      public static long ReadLong(this Stream stream)
      {
         return (long)(ReadUnsignedByte(stream) & 0xFF) << 56 |
                (long)(ReadUnsignedByte(stream) & 0xFF) << 48 |
                (long)(ReadUnsignedByte(stream) & 0xFF) << 40 |
                (long)(ReadUnsignedByte(stream) & 0xFF) << 32 |
                (long)(ReadUnsignedByte(stream) & 0xFF) << 24 |
                (long)(ReadUnsignedByte(stream) & 0xFF) << 16 |
                (long)(ReadUnsignedByte(stream) & 0xFF) << 8 |
                (long)(ReadUnsignedByte(stream) & 0xFF) << 0;
      }

      /// <summary>
      /// Reads a unsigned long value from the given Stream and thrown a DecodeException if the
      /// Stream indicates an EOF condition was encountered.
      /// </summary>
      /// <param name="stream">The stream where the value should be read from</param>
      /// <returns>An value read from the given stream</returns>
      /// <exception cref="DecodeException">If the value cannot be read from the stream</exception>
      public static ulong ReadUnsignedLong(this Stream stream)
      {
         return (ulong)(ReadUnsignedByte(stream) & 0xFF) << 56 |
                (ulong)(ReadUnsignedByte(stream) & 0xFF) << 48 |
                (ulong)(ReadUnsignedByte(stream) & 0xFF) << 40 |
                (ulong)(ReadUnsignedByte(stream) & 0xFF) << 32 |
                (ulong)(ReadUnsignedByte(stream) & 0xFF) << 24 |
                (ulong)(ReadUnsignedByte(stream) & 0xFF) << 16 |
                (ulong)(ReadUnsignedByte(stream) & 0xFF) << 8 |
                (ulong)(ReadUnsignedByte(stream) & 0xFF) << 0;
      }

      /// <summary>
      /// Reads a 32bit float value from the given Stream and thrown a DecodeException if the
      /// Stream indicates an EOF condition was encountered.
      /// </summary>
      /// <param name="stream">The stream where the value should be read from</param>
      /// <returns>An value read from the given stream</returns>
      /// <exception cref="DecodeException">If the value cannot be read from the stream</exception>
      public static float ReadFloat(this Stream stream)
      {
         return BitConverter.Int32BitsToSingle(ReadInt(stream));
      }

      /// <summary>
      /// Reads a 64bit double value from the given Stream and thrown a DecodeException if the
      /// Stream indicates an EOF condition was encountered.
      /// </summary>
      /// <param name="stream">The stream where the value should be read from</param>
      /// <returns>An value read from the given stream</returns>
      /// <exception cref="DecodeException">If the value cannot be read from the stream</exception>
      public static double ReadDouble(this Stream stream)
      {
         return BitConverter.Int64BitsToDouble(ReadLong(stream));
      }

      /// <summary>
      /// Attempts to skip the given number of bytes from the provided Stream instance and
      /// throws a DecodeException if an error occurs during the skip.
      /// </summary>
      /// <param name="stream">The stream where the value should be read from</param>
      /// <param name="amount">The number of bytes to advance the current stream position by</param>
      /// <returns>The stream that was provided to this method.</returns>
      public static Stream SkipBytes(this Stream stream, long amount)
      {
         long position = stream.Position;
         long newPosition = stream.Seek(amount, SeekOrigin.Current);

         if (newPosition - position != amount)
         {
            throw new EndOfStreamException("Stream was not able to skip the requested amount of bytes: " + amount);
         }

         return stream;
      }

      public static Stream WriteShort(this Stream stream, short value)
      {
         return stream.WriteUnsignedShort((ushort)value);
      }

      public static Stream WriteUnsignedShort(this Stream stream, ushort value)
      {
         if (!stream.IsWritable())
         {
            throw new IOException("Cannot write to stream");
         }

         stream.WriteByte((byte)(value >> 8));
         stream.WriteByte((byte)(value >> 0));

         return stream;
      }

      public static Stream WriteInt(this Stream stream, int value)
      {
         return stream.WriteUnsignedInt((uint)value);
      }

      public static Stream WriteUnsignedInt(this Stream stream, uint value)
      {
         if (!stream.IsWritable())
         {
            throw new IOException("Cannot write to stream");
         }

         stream.WriteByte((byte)(value >> 24));
         stream.WriteByte((byte)(value >> 16));
         stream.WriteByte((byte)(value >> 8));
         stream.WriteByte((byte)(value >> 0));

         return stream;
      }

      public static Stream WriteLong(this Stream stream, long value)
      {
         return stream.WriteUnsignedLong((ulong)value);
      }

      public static Stream WriteUnsignedLong(this Stream stream, ulong value)
      {
         if (!stream.IsWritable())
         {
            throw new IOException("Cannot write to stream");
         }

         stream.WriteByte((byte)(value >> 56));
         stream.WriteByte((byte)(value >> 48));
         stream.WriteByte((byte)(value >> 40));
         stream.WriteByte((byte)(value >> 32));
         stream.WriteByte((byte)(value >> 24));
         stream.WriteByte((byte)(value >> 16));
         stream.WriteByte((byte)(value >> 8));
         stream.WriteByte((byte)(value >> 0));

         return stream;
      }

      public static Stream WriteFloat(this Stream stream, float value)
      {
         return stream.WriteUnsignedInt((uint)BitConverter.SingleToInt32Bits(value));
      }

      public static Stream WriteDouble(this Stream stream, double value)
      {
         return stream.WriteUnsignedLong((ulong)BitConverter.DoubleToInt64Bits(value));
      }
   }
}