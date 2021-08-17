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
using System.Collections.Generic;
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Engine
{
   /// <summary>
   /// Represents an AMQP Connection which is owned by a single engine instance
   /// </summary>
   public interface IConnection : IEndpoint<IConnection>
   {
      /// <summary>
      /// <para>
      /// If not already negotiated this method initiates the AMQP protocol negotiation phase
      /// of the connection process sending the AMQP header to the remote peer.  For a client
      /// application this could mean requesting the server to indicate if it supports the
      /// version of the protocol this client speaks. In rare cases a server could use this to
      /// preemptively send its AMQP header.
      /// </para>
      /// <para>
      /// Once a header is sent the remote should respond with the AMQP Header that indicates
      /// what protocol level it supports and if there is a mismatch the the engine will be
      /// failed with an error indicating the protocol support was not successfully negotiated.
      /// </para>
      /// </summary>
      /// <remarks>
      /// If the engine has a configured SASL layer then by starting the AMQP Header exchange
      /// this will implicitly first attempt the SASL authentication step of the connection
      /// process.
      /// </remarks>
      /// <returns>This connection instance</returns>
      /// <exception cref="EngineStateException">If the engine has failed or was shutdown</exception>
      IConnection Negotiate();

      /// <summary>
      /// <para>
      /// If not already negotiated this method initiates the AMQP protocol negotiation phase
      /// of the connection process sending the AMQP header to the remote peer.  For a client
      /// application this could mean requesting the server to indicate if it supports the
      /// version of the protocol this client speaks. In rare cases a server could use this to
      /// preemptively send its AMQP header.
      /// </para>
      /// <para>
      /// Once a header is sent the remote should respond with the AMQP Header that indicates
      /// what protocol level it supports and if there is a mismatch the the engine will be
      /// failed with an error indicating the protocol support was not successfully negotiated.
      /// </para>
      /// <para>
      /// The provided remote AMQP Header handler will be called once the remote sends its AMQP
      /// Header to the either preemptively or as a response to offered AMQP Header from this peer,
      /// even if that has already happened prior to this call.
      /// </para>
      /// </summary>
      /// <remarks>
      /// If the engine has a configured SASL layer then by starting the AMQP Header exchange
      /// this will implicitly first attempt the SASL authentication step of the connection
      /// process.
      /// </remarks>
      /// <returns>This connection instance</returns>
      /// <exception cref="EngineStateException">If the engine has failed or was shutdown</exception>
      IConnection Negotiate(in Action<AmqpHeader> remoteAMQPHeaderHandler);

      /// <summary>
      /// Performs a tick operation on the connection which checks that Connection Idle timeout
      /// processing is run. It is an error to call this method if the
      /// scheduled tick method has been invoked.
      /// </summary>
      /// <param name="current">The current system tick count</param>
      /// <returns>the absolute deadline in milliseconds to next call tick by/at, or 0 if there is none</returns>
      /// <exception cref="InvalidOperationException">If the engine has already been set to auto tick</exception>
      /// <exception cref="EngineStateException">If the engine has failed or was shutdown</exception>
      long Tick(long current);

      /// <summary>
      /// Convenience method which is the same as calling Engine auto tick idle checking API.
      /// </summary>
      /// <param name="scheduler">The single threaded scheduler where are engine work is queued</param>
      /// <returns>This connection instance</returns>
      /// <exception cref="EngineStateException">If the engine has failed or was shutdown</exception>
      IConnection TickAuto(in TaskScheduler scheduler);

      /// <summary>
      /// Provides access to the current connection operating state.
      /// </summary>
      ConnectionState ConnectionState { get; }

      /// <summary>
      /// Provides access to the current connection operating state on the remote end of the connection.
      /// </summary>
      ConnectionState RemoteConnectionState { get; }

      /// <summary>
      /// Provides access to the container Id value of this connection. The value can be
      /// modified until the connection is opened after which any modification results
      /// in an exception being thrown.
      /// </summary>
      string ContainerId { get; set; }

      /// <summary>
      /// Access the remote container Id that was returned in the remote open performative.
      /// </summary>
      string RemoteContainerId { get; }

      /// <summary>
      /// Access the name of the host (either fully qualified or relative) to which this
      /// connection is connecting to. This information may be used by the remote peer
      /// to determine the correct back-end service to connect the client to. This value
      /// will be sent in the Open performative.
      /// <para/>
      /// Note that it is illegal to set the host name to a numeric IP address or include
      /// a port number.
      /// <para/>
      /// The host name value can only be modified prior to a call to open the connection
      /// once the connection has been opened locally an error will be thrown if this method
      /// is called.
      /// </summary>
      string Hostname { get; set; }

      /// <summary>
      /// Access the remote host name that was returned in the remote open performative.
      /// </summary>
      string RemoteHostname { get; }

      /// <summary>
      /// Access the channel max value for this Connection.
      /// <para/>
      /// The channel max value can only be modified prior to a call to open the connection
      /// once the connection has been opened locally an error will be thrown if this method
      /// is called.
      /// </summary>
      ushort ChannelMax { get; set; }

      /// <summary>
      /// Access the remote channel max that was returned in the remote open performative.
      /// </summary>
      ushort RemoteChannelMax { get; }

      /// <summary>
      /// Access the maximum frame size allowed for this connection, which is the largest single
      /// frame that the remote can send to this connection before it will close the connection
      /// with an error condition indicating the violation.
      /// <para/>
      /// The legal range for this value is defined as (512 - 2^32-1) bytes.
      /// <para/>
      /// The max frame size value can only be modified prior to a call to open the connection
      /// once the connection has been opened locally an error will be thrown if this method
      /// is called.
      /// </summary>
      uint MaxFrameSize { get; set; }

      /// <summary>
      /// Access the remote frame size that was returned in the remote open performative.
      /// </summary>
      uint RemoteMaxFrameSize { get; }

      /// <summary>
      /// Access the idle timeout value for this Connection.
      /// <para/>
      /// The idle timeout value can only be modified prior to a call to open the connection
      /// once the connection has been opened locally an error will be thrown if this method
      /// is called.
      /// </summary>
      uint IdleTimeout { get; set; }

      /// <summary>
      /// Access the remote idle timeout that was returned in the remote open performative.
      /// </summary>
      uint RemoteIdleTimeout { get; }

      /// <summary>
      /// Creates a new Session linked to this Connection.
      /// </summary>
      /// <returns>A new session instance</returns>
      ISession Session();

      /// <summary>
      /// Access an enumerator that provides a view of all the currently tracked by
      /// this connection meaning each is either locally or remotely opened or both.
      /// </summary>
      ICollection<ISession> Sessions { get; }

      /// <summary>
      /// Sets an Action for when an AMQP Begin frame is received from the remote peer.
      /// <para/>
      /// Used to process remotely initiated Sessions. Locally initiated sessions have their own
      /// Action delegate invoked instead. This method is Typically used by servers to listen for
      /// remote Session creation.
      /// </summary>
      /// <param name="handler">The handler that will process the event</param>
      /// <returns>This connection instance</returns>
      IConnection SessionOpenedHandler(Action<ISession> handler);

      /// <summary>
      /// Sets an Action for when an AMQP Attach frame is received from the remote peer that
      /// represents the sending end of a link.
      /// <para/>
      /// Used to process remotely initiated sender. Locally initiated senders have their own
      /// Action delegate invoked instead. This method is Typically used by servers to listen for
      /// remote sender creation. If an event handler for remote sender open is registered on the
      /// session that the link is owned by then that handler will be invoked instead of this one.
      /// </summary>
      /// <param name="handler">The handler that will process the event</param>
      /// <returns>This connection instance</returns>
      IConnection SenderOpenedHandler(Action<ISender> handler);

      /// <summary>
      /// Sets an Action for when an AMQP Attach frame is received from the remote peer that
      /// represents the receiving end of a link.
      /// <para/>
      /// Used to process remotely initiated receivers. Locally initiated receivers have their own
      /// Action delegate invoked instead. This method is Typically used by servers to listen for
      /// remote receiver creation. If an event handler for remote receiver open is registered on
      /// the Session that the link is owned by then that handler will be invoked instead of this
      /// one.
      /// </summary>
      /// <param name="handler">The handler that will process the event</param>
      /// <returns>This connection instance</returns>
      IConnection ReceiverOpenedHandler(Action<IReceiver> handler);

      /// <summary>
      /// Sets an Action for when an AMQP Attach frame is received from the remote peer that
      /// represents the manager side of a coordinator link.
      /// <para/>
      /// Used to process remotely initiated manager. Locally initiated managers have their own
      /// Action delegate invoked instead. This method is Typically used by servers to listen for
      /// remote transaction manager creation. If an event handler for remote transaction manager
      /// open is registered on the Session that the link is owned by then that handler will be
      /// invoked instead of this one.
      /// </summary>
      /// <param name="handler">The handler that will process the event</param>
      /// <returns>This connection instance</returns>
      IConnection TransactionManagerOpenedHandler(Action<ITransactionManager> handler);

   }
}