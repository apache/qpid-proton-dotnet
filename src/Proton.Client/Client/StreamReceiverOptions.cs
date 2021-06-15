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

namespace Apache.Qpid.Proton.Client
{
   public class StreamReceiverOptions : ReceiverOptions, ICloneable
   {
      /// <summary>
      /// Defines the default read buffering size which is used to control how much incoming
      /// data can be buffered before the remote has back pressured applied to avoid out of
      /// memory conditions.
      /// </summary>
      public static readonly uint DEFAULT_READ_BUFFER_SIZE = SessionOptions.DEFAULT_SESSION_INCOMING_CAPACITY;

      /// <summary>
      /// Creates a default stream receiver options instance.
      /// </summary>
      public StreamReceiverOptions() : base()
      {
      }

      /// <summary>
      /// Create a new stream receiver options instance whose settings are copied from the instance provided.
      /// </summary>
      /// <param name="other">The stream receiver options instance to copy</param>
      public StreamReceiverOptions(StreamReceiverOptions other) : this()
      {
         other.CopyInto(this);
      }

      /// <summary>
      /// Clone this options instance, changes to the cloned options are not reflected
      /// in this options instance.
      /// </summary>
      /// <returns>A deep copy of this options instance.</returns>
      public override object Clone()
      {
         return CopyInto(new StreamReceiverOptions());
      }

      internal StreamReceiverOptions CopyInto(StreamReceiverOptions other)
      {
         other.ReadBufferSize = ReadBufferSize;

         return base.CopyInto(other) as StreamReceiverOptions;
      }

      /// <summary>
      /// Configures the incoming buffer capacity (in bytes) that the stream receiver created
      /// with these options.
      /// </summary>
      /// <remarks>
      /// When the remote peer is sending incoming data for a streamed delivery the  amount that
      /// is stored in memory before back pressure is applied to the remote is controlled by this
      /// option. If the user does not read incoming data as it arrives this limit can prevent out
      /// of memory errors that might otherwise arise as the remote attempts to immediately send
      /// all contents of very large message payloads.
      /// </remarks>
      public uint ReadBufferSize { get; set; }

   }
}