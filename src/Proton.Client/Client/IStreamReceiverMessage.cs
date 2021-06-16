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
   /// A specialized message type that represents a streamed delivery possibly
   /// spanning many incoming AMQP transfer frames from the remote peer. It is
   /// possible for various calls in this message type to block while awaiting
   /// the receipt of sufficient bytes to provide the result.
   /// </summary>
   public interface IStreamReceiverMessage : IAdvancedMessage<Stream>
   {
      /// <summary>
      /// Returns the stream delivery that is linked to this message.
      /// </summary>
      IStreamDelivery Delivery { get; }

      /// <summary>
      /// Returns the stream receiver instance that owns this incoming stream message.
      /// </summary>
      IStreamReceiver Receiver { get; }

      /// <summary>
      /// Check if the streamed delivery that was assigned to this message has been
      /// marked as aborted by the remote.
      /// </summary>
      bool Aborted { get; }

      /// <summary>
      /// Check if the streamed delivery that was assigned to this message has been
      /// marked as complete by the remote.
      /// </summary>
      bool Completed { get; }

   }
}