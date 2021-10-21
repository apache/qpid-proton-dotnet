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
using System.Collections.Generic;

namespace Apache.Qpid.Proton.Client
{
   /// <summary>
   /// A single AMQP delivery tracker instance.
   /// </summary>
   public interface IDelivery
   {
      /// <summary>
      /// Returns the parent receiver instance where this delivery arrived.
      /// </summary>
      IReceiver Receiver { get; }

      /// <summary>
      /// The message format value that was transmitted with this delivery (default is zero).
      /// </summary>
      uint MessageFormat { get; }

      /// <summary>
      /// Decodes the payload of the delivery and returns a new message.
      ///
      /// Calling this message claims the payload of the delivery for the returned Message
      /// and excludes use of the RawInputStream method of the delivery object. Calling the
      /// RawInputStream method after calling this method throws ClientIllegalStateException.
      /// </summary>
      /// <remarks>
      /// If the incoming message carried any delivery annotations they can be accessed
      /// via the Annotations method.  Re-sending the returned message will not also
      /// send the incoming delivery annotations, the sender must include them in the
      /// sender's send call if they are to be forwarded onto the next recipient.
      /// </remarks>
      /// <typeparam name="T">Body type of the message</typeparam>
      /// <returns>the decoded message from the delivery payload</returns>
      IMessage<object> Message();

      /// <summary>
      /// Create and return an read-only Stream that reads the raw payload bytes of the
      /// given delivery. Calling this method claims the payload of the delivery for the
      /// returned Stream and excludes use of the message and annotations API methods of
      /// the delivery object. Closing the returned input stream discards any unread bytes
      /// from the delivery payload.  Calling the message or annotations methods after
      /// calling this method will throw a ClientIllegalStateException.
      /// </summary>
      Stream RawInputStream { get; }

      /// <summary>
      /// Decodes the delivery payload and returns a dictionary containing a copy of any
      /// associated delivery annotations that were transmitted with the message payload.
      ///
      /// Calling this message claims the payload of the delivery for the message and annotations
      /// methods and excludes use of the RawInputStream method of the delivery object. Calling
      /// the RawInputStream method after calling this method throws ClientIllegalStateException.
      /// </summary>
      IDictionary<string, object> Annotations { get; }

      /// <summary>
      /// Accepts and settles this delivery.
      /// </summary>
      /// <returns>This delivery instance</returns>
      IDelivery Accept();

      /// <summary>
      /// Releases and settles this delivery.
      /// </summary>
      /// <returns>This delivery instance</returns>
      IDelivery Release();

      /// <summary>
      /// Rejects the delivery with an ErrorCondition that contains the provided condition
      /// and description information and settles.
      /// </summary>
      /// <param name="condition">The condition that defines this rejection error</param>
      /// <param name="description">A description of the rejection cause.</param>
      /// <returns>This delivery instance</returns>
      IDelivery Reject(string condition, string description);

      /// <summary>
      /// Modifies and settles the delivery applying the failure and routing options.
      /// </summary>
      /// <param name="deliveryFailed">If the delivery failed on this receiver for some reason</param>
      /// <param name="undeliverableHere">If the delivery should not be routed back to this receiver.</param>
      /// <returns>This delivery instance</returns>
      IDelivery Modified(bool deliveryFailed, bool undeliverableHere);

      /// <summary>
      /// Applies the given delivery state to the delivery if not already settled and
      /// optionally settles it.
      /// </summary>
      /// <param name="state">delivery state to apply to this delivery</param>
      /// <param name="settled">optionally settles the delivery</param>
      /// <returns>This delivery instance</returns>
      IDelivery Disposition(IDeliveryState state, bool settled);

      /// <summary>
      /// Settles the delivery with the remote which prevents any further delivery
      /// state updates.
      /// </summary>
      /// <returns>This delivery instance</returns>
      IDelivery Settle();

      /// <summary>
      /// Returns true if this delivery has already been settled.
      /// </summary>
      bool Settled { get; }

      /// <summary>
      /// Returns the currently set delivery state for this delivery or null if none set.
      /// </summary>
      IDeliveryState State { get; }

      /// <summary>
      /// Returns true if this delivery has already been settled by the remote.
      /// </summary>
      bool RemoteSettled { get; }

      /// <summary>
      /// Returns the currently set delivery state for this delivery as set by the remote or null if none set.
      /// </summary>
      IDeliveryState RemoteState { get; }

   }
}