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

namespace Apache.Qpid.Proton.Client
{
   /// <summary>
   /// Special sender options that are applied the streaming senders which allow
   /// transmission of large message payloads.
   /// </summary>
   public class StreamSenderOptions : SenderOptions, ICloneable
   {
      /// <summary>
      /// Defines the default pending write buffering size which is used to control how
      /// much outgoing data can be buffered for local writing before the sender has back
      /// pressured applied to avoid out of memory conditions due to overly large pending
      /// batched writes.
      /// </summary>
      public static readonly uint DEFAULT_PENDING_WRITES_BUFFER_SIZE = SessionOptions.DEFAULT_SESSION_OUTGOING_CAPACITY;

      /// <summary>
      /// Defines the default minimum size that the context write buffer will allocate
      /// which drives the interval auto flushing of written data for this context.
      /// </summary>
      public static readonly uint MIN_BUFFER_SIZE_LIMIT = 256;

      /// <summary>
      /// Creates a default stream sender options instance.
      /// </summary>
      public StreamSenderOptions() : base()
      {
      }

      /// <summary>
      /// Create a new stream sender options instance whose settings are copied from the instance provided.
      /// </summary>
      /// <param name="other">The stream sender options instance to copy</param>
      public StreamSenderOptions(StreamSenderOptions other) : this()
      {
         other.CopyInto(this);
      }

      internal StreamSenderOptions CopyInto(StreamSenderOptions other)
      {
         other.WriteBufferSize = WriteBufferSize;
         other.PendingWriteBufferSize = PendingWriteBufferSize;

         return base.CopyInto(other) as StreamSenderOptions;
      }

      /// <summary>
      /// Clone this options instance, changes to the cloned options are not reflected
      /// in this options instance.
      /// </summary>
      /// <returns>A deep copy of this options instance.</returns>
      public override object Clone()
      {
         return CopyInto(new StreamSenderOptions());
      }

      /// <summary>
      /// Configures the overall number of bytes the stream sender will buffer before automatically
      /// flushing the currently buffered bytes.  By default the stream sender implementation chooses
      /// a value for this buffer limit based on the configured frame size limits of the connection.
      /// </summary>
      public uint WriteBufferSize { get; set; }

      /// <summary>
      /// Sets the overall number of bytes the stream sender will allow to be pending for write before
      /// applying back pressure to the stream write caller. By default the stream sender implementation
      /// chooses a value for this pending write limit based on the configured frame size limits of the
      /// connection.  This is an advanced option and should not be used unless the impact of doing so
      /// is understood by the user.
      /// </summary>
      public uint PendingWriteBufferSize { get; set; }

   }
}