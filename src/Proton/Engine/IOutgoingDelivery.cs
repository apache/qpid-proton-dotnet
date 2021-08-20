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
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Engine
{
   /// <summary>
   /// Represents an outgoing delivery that is sent from a local sender to
   /// a remote receiver on an open link.
   /// </summary>
   public interface IOutgoingDelivery
   {
      /// <summary>
      /// Returns the sender instance that owns this outgoing delivery.
      /// </summary>
      ISender Sender { get; }

      /// <summary>
      /// Access the attachments instance that allows callers to attach state data
      /// to an outgoing delivery instance.
      /// </summary>
      IAttachments Attachments { get; }

      /// <summary>
      /// Allows the endpoint to have some user defined resource linked to it
      /// which can be used to store application state data or other associated
      /// object instances with this outgoing delivery.
      /// </summary>
      object LinkedResource { get; set; }

      /// <summary>
      /// Sets the message-format for this Delivery, representing the 32bit value using an
      /// unsigned integer value. The message format can only be  set prior to the first
      /// Transfer of delivery payload having been written. If one of the delivery write
      /// methods is called prior to the message format being set then it defaults to the
      /// AMQP default format of zero. If called after having written delivery payload bytes
      /// the set access method throws an exception.
      /// <para/>
      /// The default value is 0 as per the message format defined in the core AMQP 1.0 specification.
      /// </summary>
      uint MessageFormat { get; set; }

      /// <summary>
      /// Access the delivery tag to assign to this outgoing delivery which if setting a
      /// value must be done prior to calling one of the delivery bytes write methods.
      /// </summary>
      IDeliveryTag DeliveryTag { get; set; }

      /// <summary>
      /// Access the delivery tag to assign to this outgoing delivery as a byte array which
      /// if setting a value must be done prior to calling one of the delivery bytes write
      /// methods.
      /// </summary>
      byte[] DeliveryTagBytes { get; set; }

      /// <summary>
      /// Returns if the delivery is in a partial state meaning that the sender has not
      /// completed the write of payload via one of the byte write methods.
      /// </summary>
      bool IsPartial { get; }

      /// <summary>
      /// Write the given bytes as the payload of this delivery, no additional writes can
      /// occur on this delivery if the write succeeds in sending all of the given bytes.
      /// <para/>
      /// When called the provided buffer is treated as containing the entirety of the
      /// transfer payload and the Transfer(s) that result from this call will result in
      /// a final Transfer frame whose more flag is set to false which tells the remote that
      /// no additional data will be sent for this delivery. The sender will output as much
      /// of the buffer as possible within the constraints of both the link credit and the
      /// current capacity of the parent session.
      /// <para/>
      /// The caller must check that all bytes were written and if not they should await
      /// updates from the sender that indicate that the it has become sendable once again
      /// or the caller should check via polling that the sender has become sendable
      /// periodically until it becomes true once again.
      /// </summary>
      /// <param name="buffer">The payload to send for this delivery.</param>
      /// <returns>This outgoing delivery instance.</returns>
      IOutgoingDelivery WriteBytes(IProtonBuffer buffer);

      /// <summary>
      /// Write the given bytes as a portion of the payload of this delivery, additional
      /// bytes can be streamed until the stream complete flag is set to true on a call
      /// to either the stream bytes method that accepts a done flag or a call to the
      /// write bytes API where all bytes are consumed from the passed in buffer.
      /// <para/>
      /// The sender will output as much of the buffer as possible within the constraints
      /// of both the link credit and the current capacity of the parent session. The caller
      /// must check that all bytes were written and if not they should await updates from
      /// sender state update event or poll the sender to see when it becomes writeable once
      /// more and again attempt to write the remaining bytes.
      /// </summary>
      /// <param name="buffer">The payload to send for this delivery.</param>
      /// <returns>This outgoing delivery instance.</returns>
      IOutgoingDelivery StreamBytes(IProtonBuffer buffer);

      /// <summary>
      /// Write the given bytes as a portion of the payload of this delivery, additional
      /// bytes can be streamed until the stream complete flag is set to true on a call
      /// to either this method method or a call to the write bytes API where all bytes
      /// are consumed from the passed in buffer.
      /// <para/>
      /// The sender will output as much of the buffer as possible within the constraints
      /// of both the link credit and the current capacity of the parent session. The caller
      /// must check that all bytes were written and if not they should await updates from
      /// sender state update event or poll the sender to see when it becomes writeable once
      /// more and again attempt to write the remaining bytes.
      /// </summary>
      /// <param name="buffer">The payload to send for this delivery.</param>
      /// <param name="complete">Should the delivery be completed.</param>
      /// <returns>This outgoing delivery instance.</returns>
      IOutgoingDelivery StreamBytes(IProtonBuffer buffer, bool complete);

      /// <summary>
      /// Checks if the delivery was previously aborted.
      /// </summary>
      bool IsAborted { get; }

      /// <summary>
      /// Aborts the outgoing delivery if it has not already been marked as complete.
      /// </summary>
      /// <returns>This outgoing delivery instance.</returns>
      IOutgoingDelivery Abort();

      /// <summary>
      /// Access the delivery state assigned by the local end of this delivery.
      /// </summary>
      IDeliveryState State { get; }

      /// <summary>
      /// Access the delivery state assigned by the remote end of this delivery.
      /// </summary>
      IDeliveryState RemoteState { get; }

      /// <summary>
      /// Checks if the delivery has been settled locally.
      /// </summary>
      bool IsSettled { get; }

      /// <summary>
      /// Checks if the delivery has been settled by the remote.
      /// </summary>
      bool IsRemotelySettled { get; }

      /// <summary>
      /// Update the delivery with the given disposition if not locally settled.
      /// </summary>
      /// <param name="state">The delivery state to apply to this delivery</param>
      /// <returns>This outgoing delivery instance</returns>
      IOutgoingDelivery Disposition(IDeliveryState state);

      /// <summary>
      /// Update the delivery with the given disposition if not locally settled
      /// and optionally settles the delivery if not already settled.
      /// <para/>
      /// Applies the given delivery state and local settlement value to this delivery
      /// writing a new disposition frame if the remote has not already settled the
      /// delivery. Once locally settled no additional updates to the local delivery
      /// state can be applied and if attempted an exception will be thrown to indicate
      /// this is not possible.
      /// </summary>
      /// <param name="state">The delivery state to apply to this delivery</param>
      /// <param name="settled">Should the delivery be settled</param>
      /// <returns>This outgoing delivery instance</returns>
      /// <exception cref="InvalidOperationException">If already locally settled"</exception>
      IOutgoingDelivery Disposition(IDeliveryState state, bool settled);

      /// <summary>
      /// Settles this delivery locally, transmitting a disposition frame to the remote
      /// if the remote has not already settled the delivery. Once locally settled the
      /// delivery will not accept any additional updates to the delivery state via one
      /// of the disposition methods.
      /// </summary>
      /// <returns>This outgoing delivery instance</returns>
      IOutgoingDelivery Settle();

      /// <summary>
      /// Returns the total number of transfer frames that have arrived for this
      /// delivery so far.
      /// </summary>
      uint TransferCount { get; }

      #region Outgoing delivery event points

      /// <summary>
      /// Handler for updates to the remote state of outgoing deliveries that have previously
      /// been sent.
      /// <para/>
      /// Remote state updates for an previously sent delivery can happen when the remote
      /// settles a complete delivery or otherwise modifies the delivery outcome and the user
      /// needs to act on those changes such as a spontaneous update to the delivery state.
      /// </summary>
      /// <param name="handler">A delegate that will handle this event</param>
      /// <returns>This outgoing delivery instance</returns>
      IOutgoingDelivery DeliveryStateUpdatedHandler(Action<IOutgoingDelivery> handler);

      #endregion
   }
}