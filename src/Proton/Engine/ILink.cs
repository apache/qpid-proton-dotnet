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
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Types.Transactions;
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Engine
{
   /// <summary>
   /// Base API for all AMQP Sender and Receiver links.
   /// </summary>
   /// <typeparam name="T">The type of the class that implements the link</typeparam>
   public interface ILink<T> : IEndpoint<T> where T : ILink<T>
   {
      /// <summary>
      /// Detaches the local end of the link if not already closed or detached.
      /// </summary>
      /// <returns>This link instance.</returns>
      /// <exception cref="EngineStateException">If the engine state is already failed or shutdown</exception>
      T Detach();

      /// <summary>
      /// Returns true if the endpoint having been previously locally opened is
      /// now detached due to a call the the detach method. This does not reflect the
      /// state of the remote endpoint and that should be checked separately.
      /// </summary>
      bool IsLocallDetached { get; }

      /// <summary>
      /// Returns true if the endpoint having been previously locally opened is
      /// now detached or closed due to a call to either the close method or the
      /// detach method. This does not reflect the state of the remote endpoint
      /// and that should be checked separately.
      /// </summary>
      bool IsLocallyClosedOrDetached { get; }

      /// <summary>
      /// Gets the current state of the local end of the link object.
      /// </summary>
      LinkState LinkState { get; }

      /// <summary>
      /// Reads the credit that is currently available on or assigned to
      /// this link object.
      /// </summary>
      uint Credit { get; }

      /// <summary>
      /// Indicates if the link is draining. For a sender link this indicates that
      /// the remote has requested that the sender transmit deliveries up to the
      /// currently available credit or indicate that it has no more to send.
      /// For a receiver this indicates that the Receiver has requested that the
      /// remote sender consume its outstanding credit.
      /// </summary>
      bool IsDraining { get; }

      /// <summary>
      /// Reads the role assigned to the local end of the link.
      /// </summary>
      Role Role { get; }

      /// <summary>
      /// Returns if this link is a sender link.
      /// </summary>
      bool IsSender { get; }

      /// <summary>
      /// Returns if this link is a receiver link.
      /// </summary>
      bool IsReceiver { get; }

      /// <summary>
      /// Provides access to the connection that owns this sender endpoint.
      /// </summary>
      IConnection Connection { get; }

      /// <summary>
      /// Provides access to the session that owns this sender endpoint.
      /// </summary>
      ISession Session { get; }

      /// <summary>
      /// Access the name that was given to the link on creation.
      /// </summary>
      string Name { get; }

      /// <summary>
      /// Access the sender settle mode.
      /// <para/>
      /// Should only be called during link set-up, i.e. before calling open.
      /// If this endpoint is the initiator of the link, this method can be used
      /// to set a value other than the default.
      /// <para/>
      /// If this endpoint is not the initiator, this method should be used to set
      /// a local value. According to the AMQP spec, the application may choose to
      /// accept the sender's suggestion (accessed by calling the remote sender
      /// settle mode API) or choose another value. The value has no effect on
      /// Proton, but may be useful to the application at a later point.
      /// <para/>
      /// In order to be AMQP compliant the application is responsible for honoring
      /// the settlement mode.
      /// </summary>
      SenderSettleMode SenderSettleMode { get; set; }

      /// <summary>
      /// Access the receiver settle mode.
      /// <para/>
      /// Should only be called during link set-up, i.e. before calling open. If this
      /// endpoint is the initiator of the link, this method can be used to set a value
      /// other than the default.
      /// </summary>
      ReceiverSettleMode ReceiverSettleMode { get; set; }

      /// <summary>
      /// Sets the Source value to assign to the local end of this link.
      /// <para/>
      /// Must be called during link setup, i.e. before calling the open method.
      /// </summary>
      Source Source { get; set; }

      /// <summary>
      /// Gets the local Target terminus value as a generic terminus instance
      /// which the user can then interrogate to determine if it is a Target
      /// or a Coordinator instance.  When set the value must be one of Target
      /// or Coordinator.
      /// </summary>
      ITerminus TargetTerminus { get; set; }

      /// <summary>
      /// Attempt to access the local target terminus value as a Coordinator
      /// value.  When reading if the target terminus is not a Coordinator the
      /// result is null.
      /// <para/>
      /// Must be called during link setup, i.e. before calling the open method.
      /// </summary>
      Coordinator Coordinator { get; }

      /// <summary>
      /// Attempt to access the local target terminus value as a Target value.
      /// When reading if the target terminus is not a Target the result is null.
      /// <para/>
      /// Must be called during link setup, i.e. before calling the open method.
      /// </summary>
      Target Target { get; set; }

      /// <summary>
      /// Sets the local link max message size, to be conveyed to the peer via the Attach
      /// frame when attaching the link to the session. Null or 0 means no limit.
      /// <para/>
      /// Must be called during link setup, i.e. before calling the open method.
      /// </summary>
      ulong MaxMessageSize { get; set; }

      /// <summary>
      /// Returns true if this link is currently remotely detached meaning the state
      /// returned from the remote state accessor is equal to detached. A link is
      /// remotely detached after an Detach performative has been received from the
      /// remote with the close flag equal to false.
      /// </summary>
      bool IsRemotelyDetached { get; }

      /// <summary>
      /// Returns true if this link is currently remotely detached or closed meaning
      /// the state returned from the remote state accessor is equal to detached or to
      /// closed. A link is remotely detached after an Detach performative has been
      /// received from the remote with the close flag equal to false otherwise is
      /// considered closed..
      /// </summary>
      bool IsRemotelyClosedOrDetached { get; }

      /// <summary>
      /// Gets the remote link sender settlement mode, as conveyed from the peer via the
      /// Attach frame when attaching the link to the session.
      /// </summary>
      SenderSettleMode RemoteSenderSettleMode { get; }

      /// <summary>
      /// Gets the remote link receiver settlement mode, as conveyed from the peer via the
      /// Attach frame when attaching the link to the session.
      /// </summary>
      ReceiverSettleMode RemoteReceiverSettleMode { get; }

      /// <summary>
      /// Gets the current remote link state.
      /// </summary>
      LinkState RemoteState { get; }

      #region Event point API for link specific state changes

      /// <summary>
      /// Sets a Action for when an this endpoint is closed locally via a call to Close.
      /// <para/>
      /// This is a convenience event that supplements the normal locally closed event
      /// point if set. If no local detached event handler is set the endpoint will route
      /// the detached event to the local closed event handler if set and allow it to process
      /// the event in one location.
      /// <para/>
      /// Typically used by clients for logging or other state update event processing. Clients
      /// should not perform any blocking calls within this context. It is an error for the
      /// handler to throw an exception and the outcome of doing so is undefined.
      /// </summary>
      /// <param name="localCloseHandler">The handler to invoke when the event occurs</param>
      /// <returns>This Endpoint instance.</returns>
      T LocalDetachHandler(Action<T> localDetachHandler);

      /// <summary>
      /// Sets a EventHandler for when an AMQP detach frame is received from the remote peer.
      /// <para/>
      /// This is a convenience event that supplements the normal link closed event point
      /// if set.  If no detached event handler is set the endpoint will route the detached
      /// event to the closed event handler if set and allow it to process the event in one
      /// location.
      /// </summary>
      /// <param name="detachHandler">The handler to invoke when the event occurs</param>
      /// <returns>This Endpoint instance.</returns>
      T DetachHandler(Action<T> detachHandler);

      /// <summary>
      /// Handler for link credit updates that occur after a remote flow frame arrives.
      /// </summary>
      /// <param name="handler">The handler to invoke when the event occurs</param>
      /// <returns>This Endpoint instance.</returns>
      T CreditStateUpdateHandler(Action<T> handler);

      /// <summary>
      /// Sets a Action delegate for when the parent Session or Connection of this link
      /// is locally closed.
      /// <para/
      /// Typically used by clients for logging or other state update event processing.
      /// Clients should not perform any blocking calls within this context.  It is an error
      /// for the handler to throw an exception and the outcome of doing so is undefined.
      /// </summary>
      /// <param name="handler">The handler to invoke when the event occurs</param>
      /// <returns>This Endpoint instance.</returns>
      T ParentEndpointClosedHandler(Action<T> handler);

      #endregion

   }
}