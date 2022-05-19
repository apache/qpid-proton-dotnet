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
using System.Threading.Tasks;

namespace Apache.Qpid.Proton.Client
{
   /// <summary>
   /// A single AMQP receiver instance.
   /// </summary>
   public interface IReceiver : ILink<IReceiver>
   {
      /// <summary>
      /// Adds credit to the Receiver link for use when there receiver has not been configured with
      /// with a credit window.  When credit window is configured credit replenishment is automatic
      /// and calling this method will result in an exception indicating that the operation is invalid.
      ///
      /// If the Receiver is draining and this method is called an exception will be thrown to
      /// indicate that credit cannot be replenished until the remote has drained the existing link
      /// credit.
      /// </summary>
      /// <param name="credit">The amount of new credit to add to the existing credit if any</param>
      /// <returns>This receiver instance.</returns>
      IReceiver AddCredit(uint credit);

      /// <summary>
      /// Asynchronously Adds credit to the Receiver link for use when there receiver has not been
      /// configured with with a credit window.  When credit window is configured credit replenishment
      /// is automatic and calling this method will result in an exception indicating that the operation
      /// is invalid.
      ///
      /// If the Receiver is draining and this method is called an exception will be thrown to
      /// indicate that credit cannot be replenished until the remote has drained the existing link
      /// credit.
      /// </summary>
      /// <param name="credit">The amount of new credit to add to the existing credit if any</param>
      /// <returns>This receiver instance.</returns>
      Task<IReceiver> AddCreditAsync(uint credit);

      /// <summary>
      /// Blocking receive method that waits forever for the remote to provide a delivery for consumption.
      /// </summary>
      /// <remarks>
      /// Receive calls will only grant credit on their own if a credit window is configured in the options
      /// which by default will have been configured.  If the client application has not configured a credit
      /// window then this method won't grant or extend the credit window but will wait for a delivery
      /// regardless. The application needs to arrange for credit to be granted in that case.
      /// </remarks>
      /// <returns>The next available delivery</returns>
      IDelivery Receive();

      /// <summary>
      /// Blocking receive method that waits for the specified time period for the remote to provide a
      /// delivery for consumption before returning null if none was received.
      /// </summary>
      /// <remarks>
      /// Receive calls will only grant credit on their own if a credit window is configured in the options
      /// which by default will have been configured.  If the client application has not configured a credit
      /// window then this method won't grant or extend the credit window but will wait for a delivery
      /// regardless. The application needs to arrange for credit to be granted in that case.
      /// </remarks>
      /// <returns>The next available delivery or null if the time span elapses</returns>
      IDelivery Receive(TimeSpan timeout);

      /// <summary>
      /// A mostly Non-blocking receive method that either returns a delivery if one is immediately available
      /// or returns null if none is currently at hand. It is possible that this method will block for short
      /// time intervals while acquiring and releasing internal locks.
      /// </summary>
      /// <returns>A delivery if one is immediately available or null if not</returns>
      IDelivery TryReceive();

      /// <summary>
      /// Asynchronous receive method that waits forever for the remote to provide a delivery for consumption
      /// and when a delivery is available the returned Task will be completed.
      /// </summary>
      /// <remarks>
      /// Receive calls will only grant credit on their own if a credit window is configured in the options
      /// which by default will have been configured.  If the client application has not configured a credit
      /// window then this method won't grant or extend the credit window but will wait for a delivery
      /// regardless. The application needs to arrange for credit to be granted in that case.
      /// </remarks>
      /// <returns>The next available delivery</returns>
      Task<IDelivery> ReceiveAsync();

      /// <summary>
      /// Asynchronous receive method that returns a Task that will be completed after the specified time
      /// period if the remote to provides a delivery for consumption before completing with null if none was
      /// received.
      /// </summary>
      /// <remarks>
      /// Receive calls will only grant credit on their own if a credit window is configured in the options
      /// which by default will have been configured.  If the client application has not configured a credit
      /// window then this method won't grant or extend the credit window but will wait for a delivery
      /// regardless. The application needs to arrange for credit to be granted in that case.
      /// </remarks>
      /// <returns>The next available delivery or null if the time span elapses</returns>
      Task<IDelivery> ReceiveAsync(TimeSpan timeout);

      /// <summary>
      /// Asynchronous receive method that returns a Task which will be completed either with a
      /// currently available delivery or with null to indicate there are no queued deliveries.
      /// </summary>
      /// <returns>A Task that completes with a delivery if one is immediately available or null if not</returns>
      Task<IDelivery> TryReceiveAsync();

      /// <summary>
      /// Requests the remote to drain previously granted credit for this receiver link.
      /// The remote will either send all available deliveries up to the currently granted
      /// link credit or will report it has none to send an link credit will be set to zero.
      /// This method will block until the remote answers the drain request or the configured
      /// drain timeout expires.
      /// </summary>
      /// <returns>This receiver instance once the remote reports drain completed</returns>
      IReceiver Drain();

      /// <summary>
      /// Requests the remote to drain previously granted credit for this receiver link.
      /// The remote will either send all available deliveries up to the currently granted
      /// link credit or will report it has none to send an link credit will be set to zero.
      /// The caller can wait on the returned task which will be signalled either after the
      /// remote reports drained or once the configured drain timeout is reached.
      /// </summary>
      /// <returns>A Task that will be completed when the remote reports drained.</returns>
      Task<IReceiver> DrainAsync();

      /// <summary>
      /// A count of the currently queued deliveries which can be read immediately without
      /// blocking a call to receive.
      /// </summary>
      int QueuedDeliveries { get; }

   }
}