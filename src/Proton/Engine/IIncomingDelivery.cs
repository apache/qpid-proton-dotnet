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
   /// Represents an incoming delivery that is received by a local receiver
   /// from a remote sender on an open link.
   /// </summary>
   public interface IIncomingDelivery
   {
      /// <summary>
      /// Returns a reference to the parent receiver that read this delivery.
      /// </summary>
      IReceiver Receiver { get; }

      /// <summary>
      /// Returns the number of bytes that are currently available for reading
      /// from this incoming delivery. Note that this value will change as bytes
      /// are received, and is in general not equal to the total length of a
      /// delivery until the point where the is partial check returns false and
      /// no content has yet been received by the application.
      /// </summary>
      long Available { get; }

      /// <summary>
      /// Marks all available bytes as being claimed by the caller meaning that available
      /// byte count value can be returned to the session which can expand the session
      /// incoming window to allow more bytes to be sent from the remote peer.
      /// <para/>
      /// This method is useful in the case where the session has been configured with a
      /// small incoming capacity and the receiver needs to expand the session window in
      /// order to read the entire contents of a delivery whose payload exceeds the configured
      /// session capacity.  The incoming delivery implementation will track the amount of
      /// claimed bytes and ensure that it never releases back more bytes to the session
      /// than has actually been received as a whole which allows this method to be called
      /// with each incoming transfer frame of a large split framed delivery.
      /// </summary>
      /// <returns>This incoming delivery instance</returns>
      IIncomingDelivery ClaimAvailableBytes();

      /// <summary>
      /// Returns the current read buffer without copying it effectively consuming all
      /// currently available bytes from this delivery. If no data is available then this
      /// method returns null.
      /// </summary>
      /// <returns>The currently available delivery bytes without copying them.</returns>
      IProtonBuffer ReadAll();

      /// <summary>
      /// Reads bytes from this delivery and writes them into the destination buffer
      /// reducing the available bytes by the value of the number of bytes written to
      /// the target. The number of bytes written will be the equal to the writable
      /// bytes of the target buffer. The writable bytes of the target buffer will be
      /// decremented by the number of bytes written into it.
      /// </summary>
      /// <param name="buffer">The buffer to write into</param>
      /// <returns>This incoming delivery instance</returns>
      IIncomingDelivery ReadBytes(IProtonBuffer buffer);

      /// <summary>
      /// Reads bytes from this delivery and writes them into the destination array
      /// starting at the given offset and continuing for the specified length reducing
      /// the available bytes by the value of the number of bytes written to the target.
      /// </summary>
      /// <param name="target">The byte array where the bytes are written</param>
      /// <param name="offset">The offset into the array to start writing at</param>
      /// <param name="length">The number of bytes to write into the array</param>
      /// <returns>This incoming delivery instance</returns>
      IIncomingDelivery ReadBytes(byte[] target, int offset, int length);

      /// <summary>
      /// Configures a default delivery state to be used if a received delivery is
      /// settled/freed without any disposition state having been previously applied.
      /// </summary>
      IDeliveryState DefaultDeliveryState { get; set; }

      /// <summary>
      /// Access the attachments instance that allows callers to attach state data
      /// to an incoming delivery instance.
      /// </summary>
      IAttachments Attachments { get; }

      /// <summary>
      /// Allows the endpoint to have some user defined resource linked to it
      /// which can be used to store application state data or other associated
      /// object instances with this incoming delivery.
      /// </summary>
      object LinkedResource { get; set; }

      /// <summary>
      /// Access the delivery tag that the remote sender assigned to this incoming delivery.
      /// </summary>
      IDeliveryTag DeliveryTag { get; }

      /// <summary>
      /// Access the delivery state assigned by the local end of this delivery.
      /// </summary>
      IDeliveryState State { get; }

      /// <summary>
      /// Access the delivery state assigned by the remote end of this delivery.
      /// </summary>
      IDeliveryState RemoteState { get; }

      /// <summary>
      /// Access the message-format for this Delivery, representing the 32bit value using an
      /// unsigned int.
      /// <para/>
      /// The default value is 0 as per the message format defined in the core AMQP 1.0 specification.
      /// </summary>
      uint MessageFormat { get; }

      /// <summary>
      /// Checks if the delivery is partial or has been completed by the remote.
      /// </summary>
      bool IsPartial { get; }

      /// <summary>
      /// Checks if the delivery has been aborted by the remote sender.
      /// </summary>
      bool IsAborted { get; }

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
      /// <returns>This incoming delivery instance</returns>
      IIncomingDelivery Disposition(IDeliveryState state);

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
      /// <returns>This incoming delivery instance</returns>
      /// <exception cref="InvalidOperationException">If already locally settled"</exception>
      IIncomingDelivery Disposition(IDeliveryState state, bool settled);

      /// <summary>
      /// Settles this delivery locally, transmitting a disposition frame to the remote
      /// if the remote has not already settled the delivery. Once locally settled the
      /// delivery will not accept any additional updates to the delivery state via one
      /// of the disposition methods.
      /// </summary>
      /// <returns>This incoming delivery instance</returns>
      IIncomingDelivery Settle();

      /// <summary>
      /// Returns the total number of transfer frames that have occurred for this
      /// delivery so far.
      /// </summary>
      uint TransferCount { get; }

      #region Incoming delivery event points

      /// <summary>
      /// Handler for incoming deliveries that is called for each incoming transfer frame
      /// that comprises either one complete delivery or a chunk of a split framed transfer.
      /// The handler should check that the delivery being read is partial or not and act
      /// accordingly, as partial deliveries expect additional updates as more frames
      /// comprising that delivery arrive or the remote aborts the transfer.
      /// </summary>
      /// <param name="handler">A delegate that will handle this event</param>
      /// <returns>This incoming delivery instance</returns>
      IIncomingDelivery DeliveryReadHandler(Action<IIncomingDelivery> handler);

      /// <summary>
      /// Handler for aborted deliveries that is called for each aborted in-progress delivery.
      /// <para/>
      /// This handler is an optional convenience handler that supplements the standard
      /// delivery read event handler in cases where the users wishes to break out the
      /// processing of inbound delivery data from abort processing. If this handler is not
      /// set the delivery will call the registered delivery read handler if one is set.
      /// </summary>
      /// <param name="handler">A delegate that will handle this event</param>
      /// <returns>This incoming delivery instance</returns>
      IIncomingDelivery DeliveryAbortedHandler(Action<IIncomingDelivery> handler);

      /// <summary>
      /// Handler for updates to the remote state of incoming deliveries that have previously
      /// been received.
      /// <para/>
      /// Remote state updates for an previously received delivery can happen when the remote
      /// settles a complete delivery or otherwise modifies the delivery outcome and the user needs
      /// to act on those changes such as a spontaneous update to the delivery state.
      /// </summary>
      /// <param name="handler">A delegate that will handle this event</param>
      /// <returns>This incoming delivery instance</returns>
      IIncomingDelivery DeliveryStateUpdatedHandler(Action<IIncomingDelivery> handler);

      #endregion
   }
}