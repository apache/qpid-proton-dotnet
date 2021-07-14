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
   /// Provides a write-only stream accessor of a proton buffer instance. The stream
   /// does not allow reading of the buffer that it wraps.
   /// </summary>
   public sealed class ProtonBufferOutputStream : Stream
   {
      private readonly IProtonBuffer buffer;
      private readonly long initialWriteIndex;

      private bool closed;

      /// <summary>
      /// Create a new stream instance that wraps the given buffer.
      /// </summary>
      /// <param name="buffer"></param>
      public ProtonBufferOutputStream(IProtonBuffer buffer) : base()
      {
         this.buffer = buffer;
         this.initialWriteIndex = buffer.WriteOffset;
      }

      public override bool CanRead => false;

      public override bool CanSeek => false;

      public override bool CanWrite => buffer.Writable;

      public override long Length => buffer.WritableBytes;

      public override long Position
      {
         get => buffer.WriteOffset;
         set => throw new NotImplementedException();
      }

      public override void Flush()
      {
      }

      public override void Close()
      {
         this.closed = true;
         base.Close();
      }

      public override int Read(byte[] buffer, int offset, int count)
      {
         throw new InvalidOperationException("Cannot read from a buffer write stream");
      }

      public override long Seek(long offset, SeekOrigin origin)
      {
         throw new InvalidOperationException("Cannot seek from a buffer write stream");
      }

      public override void SetLength(long value)
      {
         throw new InvalidOperationException("Cannot alter length on a buffer write stream");
      }

      public override void WriteByte(byte value)
      {
         CheckClosed();

         this.buffer.EnsureWritable(sizeof(byte));
         this.buffer.WriteUnsignedByte(value);
      }

      public override void Write(byte[] buffer, int offset, int count)
      {
         CheckClosed();

         this.buffer.EnsureWritable(count);
         this.buffer.WriteBytes(buffer, offset, count);
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