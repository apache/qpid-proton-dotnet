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

using System.IO;

namespace Apache.Qpid.Proton.Client
{
   /// <summary>
   /// A specialized streaming message type used to operate on a streamed message which
   /// allows message data to be written in one or more transfer frames to the remote
   /// allowing for very large message transmission with limited memory overhead.
   /// </summary>
   public interface IStreamSenderMessage : IAdvancedMessage<Stream>
   {
      /// <summary>
      /// Returns the stream tracker that is associated with this outgoing stream message.
      /// </summary>
      IStreamTracker Tracker { get; }

      /// <summary>
      /// Returns the stream sender instance that owns this outgoing stream message.
      /// </summary>
      IStreamSender Sender { get; }

      /// <summary>
      /// Marks the currently streaming message as being complete. Marking a message as
      /// complete finalizes the streaming send operation and causes a final transfer
      /// frame to be sent to the remote indicating that the ongoing streaming delivery
      /// is done and no more message data will arrive.
      /// </summary>
      /// <returns>This outgoing stream message instance</returns>
      IStreamSenderMessage Complete();

      /// <summary>
      /// Returns if the outgoing stream message has been completed.
      /// </summary>
       bool Completed { get; }

      /// <summary>
      /// Marks the currently streaming message as being aborted. Once aborted no further
      /// writes regardless of whether any writes have yet been performed or not.
      /// </summary>
      /// <returns>This outgoing stream message instance</returns>
      IStreamSenderMessage Abort();

      /// <summary>
      /// Returns if the outgoing stream message has been aborted.
      /// </summary>
      bool Aborted { get; }

      /// <summary>
      /// Creates an write only stream instance configured with the given options which
      /// will write the bytes as the payload of one or more AMQP data sections based on
      /// the provided configuration..
      /// </summary>
      /// <param name="options">options to apply to the created stream</param>
      /// <returns>A write only stream instance used to write the message body</returns>
      Stream body(OutputStreamOptions options);

      /// <summary>
      ///
      /// </summary>
      /// <returns></returns>
      Stream RawOutputStream();

   }
}