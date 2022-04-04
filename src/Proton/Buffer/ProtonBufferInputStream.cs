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

namespace Apache.Qpid.Proton.Buffer
{
   /// <summary>
   /// Provides a read-only stream view of a proton buffer instance.  The stream
   /// allows seeking within the readable bytes portion of the provided buffer but
   /// does not allow altering the readable length or writing to the buffer.
   /// </summary>
   public sealed class ProtonBufferInputStream : Stream
   {
      private readonly IProtonBuffer buffer;
      private readonly long initialReadIndex;

      private bool closed;

      public ProtonBufferInputStream(IProtonBuffer buffer) : base()
      {
         this.buffer = buffer ?? throw new ArgumentNullException(nameof(buffer), "Wrapped buffer cannot be null");
         this.initialReadIndex = buffer.ReadOffset;
      }

      public long BytesRead => buffer.ReadOffset - initialReadIndex;

      public override bool CanRead => true;

      public override bool CanSeek => true;

      public override bool CanWrite => false;

      public override long Length => buffer.WriteOffset - initialReadIndex;

      public override long Position
      {
         get => buffer.ReadOffset - initialReadIndex;
         set => buffer.ReadOffset = initialReadIndex + value;
      }

      public override void Close()
      {
         this.closed = true;
         base.Close();
      }

      public override int ReadByte()
      {
         CheckClosed();
         if (buffer.IsReadable)
         {
            return buffer.ReadUnsignedByte();
         }

         return -1;
      }

      public override int Read(byte[] destination, int offset, int count)
      {
         CheckClosed();

         if (offset < 0 || count < 0)
         {
            throw new ArgumentOutOfRangeException(nameof(offset), "offset and count must be non-negative");
         }

         int available = buffer.ReadableBytes > Int32.MaxValue ? Int32.MaxValue : (int)buffer.ReadableBytes;
         int toRead = Math.Min(available, count);

         buffer.CopyInto(buffer.ReadOffset, destination, offset, toRead);
         buffer.ReadOffset += toRead;

         return toRead;
      }

      public override long Seek(long offset, SeekOrigin origin)
      {
         CheckClosed();

         long newReadOffset = 0;

         switch (origin)
         {
            case SeekOrigin.Begin:
               newReadOffset = initialReadIndex + offset;
               break;
            case SeekOrigin.Current:
               newReadOffset = buffer.ReadOffset + offset;
               break;
            case SeekOrigin.End:
               newReadOffset = buffer.WriteOffset + offset;
               break;
         }

         if (newReadOffset > buffer.WriteOffset)
         {
            throw new ArgumentOutOfRangeException(nameof(newReadOffset), "Cannot seek beyond readable portion of the wrapped buffer");
         }
         else if ((int) newReadOffset < initialReadIndex)
         {
            throw new ArgumentOutOfRangeException(nameof(newReadOffset), "Cannot seek beyond readable portion of the wrapped buffer");
         }
         else
         {
            buffer.ReadOffset = newReadOffset;
         }

         return buffer.ReadOffset;
      }

      public override void SetLength(long value)
      {
         throw new InvalidDataException("Buffer input stream cannot alter the readable window of the wrapped buffer");
      }

      public override void Write(byte[] buffer, int offset, int count)
      {
         throw new InvalidDataException("Buffer input stream cannot be written to");
      }

      public override void Flush()
      {
         throw new InvalidDataException("Buffer input stream cannot be written to");
      }

      private void CheckClosed()
      {
         if (closed)
         {
            throw new ObjectDisposedException("Stream was previously closed");
         }
      }
   }
}
