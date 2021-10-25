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
using System.Threading.Tasks;

namespace Apache.Qpid.Proton.Client
{
   /// <summary>
   /// A single AMQP receiver instance.
   /// </summary>
   public interface IReceiver : IDisposable
   {
      /// <summary>
      /// Returns the parent client instance that hosts this receiver.
      /// </summary>
      IClient Client { get; }

      /// <summary>
      /// Returns the parent connection instance that hosts this receiver.
      /// </summary>
      IConnection Connection { get; }

      /// <summary>
      /// Returns the parent session instance that created this receiver.
      /// </summary>
      ISession Session { get; }

      /// <summary>
      /// When a receiver is created and returned to the client application it may not
      /// be remotely opened yet and if the client needs to wait for completion of the
      /// open before proceeding the open task can be fetched and waited upon.
      /// </summary>
      Task<IReceiver> OpenTask { get; }

      /// <summary>
      /// Initiates a close of the receiver and awaits a response from the remote that
      /// indicates completion of the close operation. If the response from the remote
      /// exceeds the configure close timeout the method returns after cleaning up the
      /// receiver resources.
      /// </summary>
      void Close();

      /// <summary>
      /// Initiates a close of the receiver and awaits a response from the remote that
      /// indicates completion of the close operation. If the response from the remote
      /// exceeds the configure close timeout the method returns after cleaning up the
      /// receiver resources.
      /// </summary>
      /// <param name="error">The error condition to convery to the remote</param>
      void Close(IErrorCondition error);

      /// <summary>
      /// Initiates a detach of the receiver and awaits a response from the remote that
      /// indicates completion of the detach operation. If the response from the remote
      /// exceeds the configure close timeout the method returns after cleaning up the
      /// receiver resources.
      /// </summary>
      void Detach();

      /// <summary>
      /// Initiates a detach of the receiver and awaits a response from the remote that
      /// indicates completion of the detach operation. If the response from the remote
      /// exceeds the configure close timeout the method returns after cleaning up the
      /// receiver resources.
      /// </summary>
      /// <param name="error">The error condition to convery to the remote</param>
      void Detach(IErrorCondition error);

      /// <summary>
      /// Initiates a close of the receiver and a Task that allows the caller to await
      /// or poll for the response from the remote that indicates completion of the close
      /// operation. If the response from the remote exceeds the configure close timeout
      /// the receiver will be cleaned up and the Task signalled indicating completion.
      /// </summary>
      Task<IReceiver> CloseAsync();

      /// <summary>
      /// Initiates a close of the receiver and a Task that allows the caller to await
      /// or poll for the response from the remote that indicates completion of the close
      /// operation. If the response from the remote exceeds the configure close timeout
      /// the receiver will be cleaned up and the Task signalled indicating completion.
      /// </summary>
      /// <param name="error">The error condition to convery to the remote</param>
      Task<IReceiver> CloseAsync(IErrorCondition error);

      /// <summary>
      /// Initiates a detach of the receiver and a Task that allows the caller to await
      /// or poll for the response from the remote that indicates completion of the detach
      /// operation. If the response from the remote exceeds the configure close timeout
      /// the receiver will be cleaned up and the Task signalled indicating completion.
      /// </summary>
      Task<ISender> DetachAsync();

      /// <summary>
      /// Initiates a detach of the receiver and a Task that allows the caller to await
      /// or poll for the response from the remote that indicates completion of the detach
      /// operation. If the response from the remote exceeds the configure close timeout
      /// the receiver will be cleaned up and the Task signalled indicating completion.
      /// </summary>
      /// <param name="error">The error condition to convery to the remote</param>
      Task<ISender> DetachAsync(IErrorCondition error);

      /// <summary>
      /// Returns the address that the receiver instance will send message objects to. The value
      /// returned from this method is controlled by the configuration that was used to create
      /// the receiver.
      /// <list type="bullet">
      /// <item>
      /// <description>
      /// If the receiver was created with the dynamic receiver methods then the method will return
      /// the dynamically created address once the remote has attached its end of the receiver link.
      /// Due to the need to await the remote peer to populate the dynamic address this method will
      /// block until the open of the receiver link has completed.
      /// </description>
      /// </item>
      /// <item>
      /// <description>
      /// If neither of the above is true then the address returned is the address passed to the
      /// original address value passed to one of the open receiver methods.
      /// </description>
      /// </item>
      /// </list>
      /// </summary>
      string Address { get; }

      /// <summary>
      /// Returns an immutable view of the remote Source object assigned to this receiver link.
      /// If the attach has not completed yet this method will block to await the attach response
      /// which carries the remote source.
      /// </summary>
      ISource Source { get; }

      /// <summary>
      /// Returns an immutable view of the remote Target object assigned to this receiver link.
      /// If the attach has not completed yet this method will block to await the attach response
      /// which carries the remote target.
      /// </summary>
      ITarget Target { get; }

      /// <summary>
      /// Returns the properties that the remote provided upon successfully opening the receiver.
      /// If the open has not completed yet this method will block to await the open response which
      /// carries the remote properties.  If the remote provides no properties this method will
      /// return null.
      /// </summary>
      IReadOnlyDictionary<string, object> Properties { get; }

      /// <summary>
      /// Returns the offered capabilities that the remote provided upon successfully opening the
      /// receiver. If the open has not completed yet this method will block to await the open
      /// response which carries the remote offered capabilities. If the remote provides no offered
      /// capabilities this method will return null.
      /// </summary>
      IReadOnlyCollection<string> OfferedCapabilities { get; }

      /// <summary>
      /// Returns the desired capabilities that the remote provided upon successfully opening the
      /// receiver. If the open has not completed yet this method will block to await the open
      /// response which carries the remote desired capabilities. If the remote provides no desired
      /// capabilities this method will return null.
      /// </summary>
      IReadOnlyCollection<string> DesiredCapabilities { get; }

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
      IReceiver AddCredit(int credit);

      /// <summary>
      /// Blocking receive method that waits forever for the remote to provide a delivery for consumption.
      /// </summary>
      /// <remarks>
      /// Receive calls will only grant credit on their own if a credit window is configured in the options
      /// which by default will have been configured.  If the client application has not configured a credit
      /// window then this method won't grant or extend the credit window but will wait for a delivery
      /// regardless. The application needs to arrage for credit to be granted in that case.
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
      /// regardless. The application needs to arrage for credit to be granted in that case.
      /// </remarks>
      /// <returns>The next available delivery or null if the time span elapses</returns>
      IDelivery Receive(TimeSpan timeout);

      /// <summary>
      /// Non-blocking receive method that either returns a delivery if one is immediately available
      /// or returns null if none is currently at hand.
      /// </summary>
      /// <returns>A delivery if one is immediately available or null if not</returns>
      IDelivery TryReceive();

      /// <summary>
      /// Requests the remote to drain previously granted credit for this receiver link.
      /// The remote will either send all available deliveries up to the currently granted
      /// link credit or will report it has none to send an link credit will be set to zero.
      /// The caller can wait on the returned task which will be signalled either after the
      /// remote reports drained or once the configured drain timeout is reached.
      /// </summary>
      /// <returns>A Task that will be completed when the remote reports drained.</returns>
      Task<IReceiver> Drain();

      /// <summary>
      /// A count of the currently queued deliveries which can be read immediately without
      /// blocking a call to receive.
      /// </summary>
      int QueuedDeliveries { get; }

   }
}