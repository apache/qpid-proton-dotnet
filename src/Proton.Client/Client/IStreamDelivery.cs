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

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Apache.Qpid.Proton.Client
{
   /// <summary>
   /// A specialized delivery type that is returned from the stream receiver
   /// which can be used to read incoming large messages that are streamed via
   /// multiple incoming AMQP transfer frames.
   /// </summary>
   public interface IStreamDelivery
   {
      /// <summary>
      /// Returns the parent streaming receiver instance where this delivery arrived.
      /// </summary>
      IStreamReceiver Receiver { get; }

      /// <summary>
      /// The message format value that was transmitted with this delivery (default is zero).
      /// </summary>
      uint MessageFormat { get; }

      /// <summary>
      /// Returns a stream receiver message type that will perform a decode of message
      /// payload as portions of the streamed message arrive. The message API is inherently
      /// a blocking API as the decoder will need to wait in some cases to decode a full
      /// section the incoming message when it is requested.
      /// </summary>
      /// <remarks>
      /// If the incoming message carried any delivery annotations they can be accessed
      /// via the Annotations method.  Re-sending the returned message will not also
      /// send the incoming delivery annotations, the sender must include them in the
      /// sender's send call if they are to be forwarded onto the next recipient.
      /// </remarks>
      /// <typeparam name="T">Body type of the message</typeparam>
      /// <returns>the decoded message from the delivery payload</returns>
      IStreamReceiverMessage Message();

      /// <summary>
      /// Decodes the delivery payload and returns a dictionary containing a copy of any
      /// associated delivery annotations that were transmitted with the message payload.
      ///
      /// Calling this message claims the payload of the delivery for the message and annotations
      /// methods and excludes use of the RawInputStream method of the delivery object. Calling
      /// the RawInputStream method after calling this method throws ClientIllegalStateException.
      /// </summary>
      IReadOnlyDictionary<string, object> Annotations { get; }

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
      /// Accepts and settles this delivery.
      /// </summary>
      /// <returns>This delivery instance</returns>
      IStreamDelivery Accept();

      /// <summary>
      /// Accepts and settles this delivery asynchronously ensuring that the call does not
      /// block on any IO or other client operations.
      /// </summary>
      /// <returns>A Task that returns this delivery instance</returns>
      Task<IStreamDelivery> AcceptAsync();

      /// <summary>
      /// Releases and settles this delivery.
      /// </summary>
      /// <returns>This delivery instance</returns>
      IStreamDelivery Release();

      /// <summary>
      /// Releases and settles this delivery asynchronously ensuring that the call does not
      /// block on any IO or other client operations.
      /// </summary>
      /// <returns>A Task that returns this delivery instance</returns>
      Task<IStreamDelivery> ReleaseAsync();

      /// <summary>
      /// Rejects the delivery with an ErrorCondition that contains the provided condition
      /// and description information and settles.
      /// </summary>
      /// <param name="condition">The condition that defines this rejection error</param>
      /// <param name="description">A description of the rejection cause.</param>
      /// <returns>This delivery instance</returns>
      IStreamDelivery Reject(string condition, string description);

      /// <summary>
      /// Asynchronously rejects the delivery with an ErrorCondition that contains the provided
      /// condition and description information and settles.
      /// </summary>
      /// <param name="condition">The condition that defines this rejection error</param>
      /// <param name="description">A description of the rejection cause.</param>
      /// <returns>A Task that returns this delivery instance</returns>
      Task<IStreamDelivery> RejectAsync(string condition, string description);

      /// <summary>
      /// Modifies and settles the delivery applying the failure and routing options.
      /// </summary>
      /// <param name="deliveryFailed">If the delivery failed on this receiver for some reason</param>
      /// <param name="undeliverableHere">If the delivery should not be routed back to this receiver.</param>
      /// <returns>This delivery instance</returns>
      IStreamDelivery Modified(bool deliveryFailed, bool undeliverableHere);

      /// <summary>
      /// Modifies and settles the delivery asynchronously applying the failure and routing
      /// options without any blocking due to IO or other client internal operations.
      /// </summary>
      /// <param name="deliveryFailed">If the delivery failed on this receiver for some reason</param>
      /// <param name="undeliverableHere">If the delivery should not be routed back to this receiver.</param>
      /// <returns>A Task that returns this delivery instance</returns>
      Task<IStreamDelivery> ModifiedAsync(bool deliveryFailed, bool undeliverableHere);

      /// <summary>
      /// Applies the given delivery state to the delivery if not already settled and
      /// optionally settles it.
      /// </summary>
      /// <param name="state">delivery state to apply to this delivery</param>
      /// <param name="settled">optionally settles the delivery</param>
      /// <returns>This delivery instance</returns>
      IStreamDelivery Disposition(IDeliveryState state, bool settled);

      /// <summary>
      /// Applies the given delivery state to the delivery if not already settled and
      /// optionally settles it performing all IO and client work asynchronously
      /// ensuring that any calls to this method do not block.
      /// </summary>
      /// <param name="state">delivery state to apply to this delivery</param>
      /// <param name="settled">optionally settles the delivery</param>
      /// <returns>A Task that returns this delivery instance</returns>
      Task<IStreamDelivery> DispositionAsync(IDeliveryState state, bool settled);

      /// <summary>
      /// Settles the delivery with the remote which prevents any further delivery
      /// state updates.
      /// </summary>
      /// <returns>This delivery instance</returns>
      IStreamDelivery Settle();

      /// <summary>
      /// Settles the delivery with the remote which prevents any further delivery
      /// state updates asynchronously.
      /// </summary>
      /// <returns>A Task that returns this delivery instance</returns>
      Task<IStreamDelivery> SettleAsync();

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

      /// <summary>
      /// Returns true if the remote has aborted this incoming streaming delivery and
      /// no more bytes are available for read from the remote.
      /// </summary>
      bool Aborted { get; }

      /// <summary>
      /// Returns true if the remote has completed the send of all portions of the streaming
      /// delivery payload and there are no more incoming bytes expected or allowed for this
      /// delivery.
      /// </summary>
      bool Completed { get; }

   }
}