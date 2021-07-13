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
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Engine.Exceptions;

namespace Apache.Qpid.Proton.Engine
{
   /// <summary>
   /// The engine pipeline contains a list of handlers that deal with incoming and
   /// outgoing AMQP frames such as logging and encoders and decoders.
   /// </summary>
   public interface ISession : IEndpoint<ISession>
   {
      /// <summary>
      /// Provides access to the connection that owns this session endpoint.
      /// </summary>
      IConnection Connection { get; }

      /// <summary>
      /// Access the session state for the local end of the session.
      /// </summary>
      SessionState State { get; }

      /// <summary>
      /// Access the session state for the remote end of the session.
      /// </summary>
      SessionState RemoteState { get; }

      /// <summary>
      /// Returns an enumerator of all the receivers currently tracked by this session.
      /// </summary>
      IEnumerator<IReceiver> Receivers { get; }

      /// <summary>
      /// Returns an enumerator of all the senders currently tracked by this session.
      /// </summary>
      IEnumerator<ISender> Senders { get; }

      /// <summary>
      /// Create a new sender link using the provided name.
      /// </summary>
      /// <param name="name">The link name to assign the new sender</param>
      /// <returns>The newly created sender instance</returns>
      /// <exception cref="InvalidOperationException">If the session is closed"</exception>
      ISender Sender(string name);

      /// <summary>
      /// Create a new receiver link using the provided name.
      /// </summary>
      /// <param name="name">The link name to assign the new sender</param>
      /// <returns>The newly created receiver instance</returns>
      /// <exception cref="InvalidOperationException">If the session is closed"</exception>
      IReceiver Receiver(string name);

      /// <summary>
      /// Create a new transaction controller link using the provided name.
      /// </summary>
      /// <param name="name">The link name to assign the new sender</param>
      /// <returns>The newly created transaction controller</returns>
      /// <exception cref="InvalidOperationException">If the session is closed"</exception>
      ITransactionController Coordinator(string name);

      /// <summary>
      /// Sets the maximum number of bytes this session can be sent from the remote.
      /// </summary>
      uint IncomingCapacity { get; set; }

      /// <summary>
      /// Gets the maximum number of bytes the remote session can be sent from the this session.
      /// </summary>
      uint RemoteIncomingCapacity { get; }

      /// <summary>
      /// Sets the maximum number of bytes this session can have written write before blocking
      /// additional sends until the written bytes are known to have been flushed to the I/O.
      /// This limit is intended to deal with issues of memory allocation when the I/O layer
      /// allows for asynchronous writes and finer grained control over the pending write
      /// buffers is needed.
      /// </summary>
      uint OutgoingCapacity { get; set; }

      /// <summary>
      /// Gets the maximum number of bytes the remote session can be have pending.
      /// </summary>
      uint RemoteOutgoingCapacity { get; }

      /// <summary>
      /// Set the handle max value for this Session which is the highest possible
      /// link handle that can be open at one time before new attach request are
      /// rejected.
      /// <para/>
      /// The handle max value can only be modified prior to a call to session open,
      /// once the session has been opened locally an error will be thrown if this
      /// method is called.
      /// </summary>
      uint HandleMax { get; set; }

      /// <summary>
      /// Gets the remote handle max to determine how many links could be attached
      /// before the remote will refuse incoming attach requests.
      /// </summary>
      uint RemoteHandleMax { get; }

      /// <summary>
      /// Sets a delegate for when an AMQP Attach frame is received from the remote peer
      /// for a sending link attach.
      /// <para/>
      /// Used to process remotely initiated sending link. Locally initiated links have their
      /// own handlers invoked instead. This method is Typically used by servers to listen for
      /// remote receiver creation. If an event handler for remote sender open is registered on
      /// this Session for a link scoped to it then this handler will be invoked instead of the
      /// variant in the Connection API.
      /// </summary>
      /// <param name="handler">The delegate that will handle this event</param>
      /// <returns>This session instance</returns>
      ISession ReceiverOpenHandler(Action<IReceiver> handler);

      /// <summary>
      /// Sets a delegate for when an AMQP Attach frame is received from the remote peer
      /// for a receiving link attach.
      /// <para/>
      /// Used to process remotely initiated receiving link. Locally initiated links have their
      /// own handlers invoked instead. This method is Typically used by servers to listen for
      /// remote sender creation. If an event handler for remote sender open is registered on
      /// this Session for a link scoped to it then this handler will be invoked instead of the
      /// variant in the Connection API.
      /// </summary>
      /// <param name="handler">The delegate that will handle this event</param>
      /// <returns>This session instance</returns>
      ISession SenderOpenHandler(Action<ISender> handler);

      /// <summary>
      /// Sets a delegate for when an AMQP Attach frame is received from the remote peer
      /// for a transaction manager link attach.
      /// <para/>
      /// Used to process remotely initiated transcation mangaers. Locally initiated links have
      /// their own handlers invoked instead. This method is Typically used by servers to listen
      /// for remote resource creation. If an event handler for remote sender open is registered on
      /// this Session for a link scoped to it then this handler will be invoked instead of the
      /// variant in the Connection API.
      /// </summary>
      /// <param name="handler">The delegate that will handle this event</param>
      /// <returns>This session instance</returns>
      ISession TransactionManagerOpenedHandler(Action<ITransactionManager> handler);

   }
}