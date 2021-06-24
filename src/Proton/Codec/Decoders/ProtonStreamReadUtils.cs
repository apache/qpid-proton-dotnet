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

namespace Apache.Qpid.Proton.Codec.Decoders
{
   public static class ProtonStreamReadUtils
   {
      private static readonly byte[] EmptyArray = new byte[0];

      /// <summary>
      /// Reads the given number of bytes from the provided Stream into an array and return
      /// that to the caller.  If the requested number of bytes cannot be read from the
      /// stream an DecodeException is thrown to indicate an underflow.
      /// </summary>
      /// <param name="stream">The stream where the bytes should be read from</param>
      /// <param name="length">the number of bytes to read from the stream</param>
      /// <returns>An array of bytes read from the given stream</returns>
      /// <exception cref="DecodeException">If the value cannot be read from the stream</exception>
      public static byte[] ReadBytes(Stream stream, int length)
      {
         try
         {
            if (length == 0)
            {
               return EmptyArray;
            }
            else
            {
               byte[] payload = new byte[length];

               if (stream.Read(payload) < length)
               {
                  throw new DecodeException(string.Format(
                      "Failed to read requested number of bytes %d: instead only %d bytes were read.", length, payload.Length));
               }

               return payload;
            }
         }
         catch (IOException ex)
         {
            throw new DecodeException("Caught IO error reading from provided stream", ex);
         }
      }

      /// <summary>
      /// Reads a single byte from the given Stream and thrown a DecodeException if the
      /// Stream indicates an EOF condition was encountered.
      /// </summary>
      /// <param name="stream">The stream where the byte should be read from</param>
      /// <returns>An byte value read from the given stream</returns>
      /// <exception cref="DecodeException">If the value cannot be read from the stream</exception>
      public static EncodingCodes ReadEncodingCode(Stream stream)
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
               throw new DecodeEOFException("Cannot read more type information from stream that has reached its end.");
            }
         }
         catch (IOException ex)
         {
            throw new DecodeException("Caught IO error reading from provided stream", ex);
         }
      }

      /// <summary>
      /// Reads a single byte from the given Stream and thrown a DecodeException if the
      /// Stream indicates an EOF condition was encountered.
      /// </summary>
      /// <param name="stream">The stream where the byte should be read from</param>
      /// <returns>An byte value read from the given stream</returns>
      /// <exception cref="DecodeException">If the value cannot be read from the stream</exception>
      public static sbyte ReadByte(Stream stream)
      {
         try
         {
            int result = stream.ReadByte();
            if (result >= 0)
            {
               return (sbyte)result;
            }
            else
            {
               throw new DecodeEOFException("Cannot read more type information from stream that has reached its end.");
            }
         }
         catch (IOException ex)
         {
            throw new DecodeException("Caught IO error reading from provided stream", ex);
         }
      }

      /// <summary>
      /// Reads a single byte from the given Stream and thrown a DecodeException if the
      /// Stream indicates an EOF condition was encountered.
      /// </summary>
      /// <param name="stream">The stream where the byte should be read from</param>
      /// <returns>An byte value read from the given stream</returns>
      /// <exception cref="DecodeException">If the value cannot be read from the stream</exception>
      public static byte ReadUnsignedByte(Stream stream)
      {
         try
         {
            int result = stream.ReadByte();
            if (result >= 0)
            {
               return (byte)result;
            }
            else
            {
               throw new DecodeEOFException("Cannot read more type information from stream that has reached its end.");
            }
         }
         catch (IOException ex)
         {
            throw new DecodeException("Caught IO error reading from provided stream", ex);
         }
      }

      /// <summary>
      /// Reads a short value from the given Stream and thrown a DecodeException if the Stream
      /// indicates an EOF condition was encountered.
      /// </summary>
      /// <param name="stream">The stream where the value should be read from</param>
      /// <returns>An value read from the given stream</returns>
      /// <exception cref="DecodeException">If the value cannot be read from the stream</exception>
      public static short ReadShort(Stream stream)
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
      public static ushort ReadUnsignedShort(Stream stream)
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
      public static int ReadInt(Stream stream)
      {
         return (int)((ReadUnsignedByte(stream) & 0xFF) << 24 |
                      (ReadUnsignedByte(stream) & 0xFF) << 16 |
                      (ReadUnsignedByte(stream) & 0xFF) << 8 |
                      (ReadUnsignedByte(stream) & 0xFF) << 0);
      }

      /// <summary>
      /// Reads a unsigned int value from the given Stream and thrown a DecodeException if the
      /// Stream indicates an EOF condition was encountered.
      /// </summary>
      /// <param name="stream">The stream where the value should be read from</param>
      /// <returns>An value read from the given stream</returns>
      /// <exception cref="DecodeException">If the value cannot be read from the stream</exception>
      public static uint ReadUnsignedInt(Stream stream)
      {
         return (uint)((ReadUnsignedByte(stream) & 0xFF) << 24 |
                       (ReadUnsignedByte(stream) & 0xFF) << 16 |
                       (ReadUnsignedByte(stream) & 0xFF) << 8 |
                       (ReadUnsignedByte(stream) & 0xFF) << 0);
      }

      /// <summary>
      /// Reads a long value from the given Stream and thrown a DecodeException if the Stream
      /// indicates an EOF condition was encountered.
      /// </summary>
      /// <param name="stream">The stream where the value should be read from</param>
      /// <returns>An value read from the given stream</returns>
      /// <exception cref="DecodeException">If the value cannot be read from the stream</exception>
      public static long ReadLong(Stream stream)
      {
         return (long)((ReadUnsignedByte(stream) & 0xFF) << 56 |
                       (ReadUnsignedByte(stream) & 0xFF) << 48 |
                       (ReadUnsignedByte(stream) & 0xFF) << 40 |
                       (ReadUnsignedByte(stream) & 0xFF) << 32 |
                       (ReadUnsignedByte(stream) & 0xFF) << 24 |
                       (ReadUnsignedByte(stream) & 0xFF) << 16 |
                       (ReadUnsignedByte(stream) & 0xFF) << 8 |
                       (ReadUnsignedByte(stream) & 0xFF) << 0);
      }

      /// <summary>
      /// Reads a unsigned long value from the given Stream and thrown a DecodeException if the
      /// Stream indicates an EOF condition was encountered.
      /// </summary>
      /// <param name="stream">The stream where the value should be read from</param>
      /// <returns>An value read from the given stream</returns>
      /// <exception cref="DecodeException">If the value cannot be read from the stream</exception>
      public static ulong ReadUnsignedLong(Stream stream)
      {
         return (ulong)((ReadUnsignedByte(stream) & 0xFF) << 56 |
                        (ReadUnsignedByte(stream) & 0xFF) << 48 |
                        (ReadUnsignedByte(stream) & 0xFF) << 40 |
                        (ReadUnsignedByte(stream) & 0xFF) << 32 |
                        (ReadUnsignedByte(stream) & 0xFF) << 24 |
                        (ReadUnsignedByte(stream) & 0xFF) << 16 |
                        (ReadUnsignedByte(stream) & 0xFF) << 8 |
                        (ReadUnsignedByte(stream) & 0xFF) << 0);
      }

      /// <summary>
      /// Reads a 32bit float value from the given Stream and thrown a DecodeException if the
      /// Stream indicates an EOF condition was encountered.
      /// </summary>
      /// <param name="stream">The stream where the value should be read from</param>
      /// <returns>An value read from the given stream</returns>
      /// <exception cref="DecodeException">If the value cannot be read from the stream</exception>
      public static float ReadFloat(Stream stream)
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
      public static double ReadDouble(Stream stream)
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
      public static Stream skipBytes(Stream stream, long amount)
      {
         try
         {
            stream.Seek(amount, SeekOrigin.Current);
         }
         catch (IOException ex)
         {
            throw new DecodeException(
                string.Format("Error while attempting to skip %d bytes in the given InputStream", amount), ex);
         }

         return stream;
      }
   }
}
