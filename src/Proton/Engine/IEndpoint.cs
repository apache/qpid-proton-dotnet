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
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Engine
{
   /// <summary>
   /// Represents an AMQP Connection which is owned by a single engine instance
   /// </summary>
   public interface IEndpoint<T> where T : IEndpoint<T>
   {
      /// <summary>
      /// Provides access to the engine instance that owns the resources
      /// of this endpoint and its parent.
      /// </summary>
      IEngine Engine { get; }

      /// <summary>
      /// Access the attachments instance that is associated with this resource
      /// where the application can store data relevant to the endpoint for later
      /// use.
      /// </summary>
      IAttachments Attachments { get; }

      /// <summary>
      /// Allows the endpoint to have some user defined resource linked to it
      /// which can be used to store application state data or other associated
      /// object instances with this endpoint.
      /// </summary>
      object LinkedResource { get; set; }

      /// <summary>
      /// Provides access to the error condition that should be applied to the
      /// AMQP frame that closes or ends this endpoint when the close method is
      /// called be the user.  Setting this value after closing the endpoint has
      /// no effect.
      /// </summary>
      ErrorCondition ErrorCondition { get; set; }

      /// <summary>
      /// If the remote has closed this endpoint and provided an ErrorCondition
      /// as part of the closing AMQP performative then this method will return it.
      /// </summary>
      ErrorCondition RemoteCondition { get; }

      /// <summary>
      /// Returns true if the endpoint open was previously called and the close
      /// method has not yet been invoked. This only reflects the state on the
      /// local end and the user should also check the remote state.
      /// </summary>
      bool IsLocallyOpen { get; }

      /// <summary>
      /// Returns true if the endpoint having been previously locally opened is
      /// now closed due to a call the the close method. This does not reflect the
      /// state of the remote endpoint and that should be checked separately.
      /// </summary>
      bool IsLocallyClosed { get; }

      /// <summary>
      /// Returns true if this endpoint is currently remotely open meaning that the
      /// AMQP performative that completes the open phase of this endpoint's lifetime
      /// has arrived but the performative that closes it has not.
      /// </summary>
      bool IsRemotelyOpen { get; }

      /// <summary>
      /// Returns true if this endpoint is currently remotely closed meaning that the
      /// AMQP performative that completes the close phase of this endpoint's lifetime
      /// has arrived.
      /// </summary>
      bool IsRemotelyClosed { get; }

      /// <summary>
      /// Open the end point locally, sending the Open performative immediately if
      /// possible or holding it until SASL negotiations or the AMQP header exchange
      /// and other required performative exchanges has completed.
      /// <para/>
      /// The endpoint will signal any registered handler of the remote opening the
      /// endpoint once the remote performative that signals open completion arrives.
      /// </summary>
      /// <returns>This endpoint instance.</returns>
      /// <exception cref="EngineStateException">If the engine state is already failed or shutdown</exception>
      T Open();

      /// <summary>
      /// Close the end point locally and send the closing performative immediately if
      /// possible or holds it until the Connection / Engine state allows it.  If the
      /// engine encounters an error writing the performative or the engine is in a failed
      /// state from a previous error then this method will throw an exception.  If the
      /// engine has been shutdown then this method will close out the local end of the
      /// endpoint and clean up any local resources before returning normally.
      /// </summary>
      /// <returns>This endpoint instance.</returns>
      /// <exception cref="EngineStateException">If the engine state is already failed or shutdown</exception>
      T Close();

      /// <summary>
      /// Access the capabilities to be offered on to the remote when this endpoint is opened.
      /// The offered capabilities value can only be modified prior to a call to open, once
      /// the endpoint has been opened locally an error will be thrown if this method is called.
      /// </summary>
      Symbol[] OfferedCapabilities { get; set; }

      /// <summary>
      /// Access the capabilities that are desired on to the remote when this endpoint is opened.
      /// The desired capabilities value can only be modified prior to a call to open, once
      /// the endpoint has been opened locally an error will be thrown if this method is called.
      /// </summary>
      Symbol[] DesiredCapabilities { get; set; }

      /// <summary>
      /// Access the properties that are conveyed to the remote when this endpoint is opened.
      /// The properties value can only be modified prior to a call to open, once the endpoint
      /// has been opened locally an error will be thrown if this method is called.
      /// </summary>
      IDictionary<Symbol, object> Properties { get; set; }

      /// <summary>
      /// The capabilities offered by the remote when it opened its end of the endpoint.
      /// </summary>
      Symbol[] RemoteOfferedCapabilities { get; }

      /// <summary>
      /// The capabilities desired by the remote when it opened its end of the endpoint.
      /// </summary>
      Symbol[] RemoteDesiredCapabilities { get; }

      /// <summary>
      /// The properties sent by the remote when it opened its end of this endpoint.
      /// </summary>
      IDictionary<Symbol, object> RemoteProperties { get; }

      /// <summary>
      /// Sets a Action for when an this endpoint is opened locally via a call to Open.
      /// <para/>
      /// Typically used by clients for logging or other state update event processing. Clients
      /// should not perform any blocking calls within this context. It is an error for the
      /// handler to throw an exception and the outcome of doing so is undefined.
      /// <para/>
      /// Typically used by clients as servers will typically listen to some parent resource
      /// event handler to determine if the remote is initiating a resource open.
      /// </summary>
      /// <param name="localOpenHandler">The handler to invoke when the event occurs</param>
      /// <returns>This Endpoint instance.</returns>
      T LocalOpenHandler(Action<T> localOpenHandler);

      /// <summary>
      /// Sets a Action for when an this endpoint is closed locally via a call to Close.
      /// <para/>
      /// Typically used by clients for logging or other state update event processing. Clients
      /// should not perform any blocking calls within this context. It is an error for the
      /// handler to throw an exception and the outcome of doing so is undefined.
      /// </summary>
      /// <param name="localCloseHandler">The handler to invoke when the event occurs</param>
      /// <returns>This Endpoint instance.</returns>
      T LocalCloseHandler(Action<T> localCloseHandler);

      /// <summary>
      /// Sets a Action for when an AMQP Open frame is received from the remote peer.
      /// <para/>
      /// Used to process remotely initiated Connections. Locally initiated sessions have
      /// their own Action invoked instead. This method is typically used by servers to listen
      /// for the remote peer to open its endpoint, while a client would listen for the server
      /// to open its end of the endpoint once a local open has been performed.
      /// <para/>
      /// Typically used by clients as servers will typically listen to some parent resource
      /// event handler to determine if the remote is initiating a resource open.
      /// </summary>
      /// <param name="localOpenHandler">The handler to invoke when the event occurs</param>
      /// <returns>This Endpoint instance.</returns>
      T OpenHandler(Action<T> localOpenHandler);

      /// <summary>
      /// Sets a EventHandler for when an AMQP closing frame is received from the remote peer.
      /// </summary>
      /// <param name="localCloseHandler">The handler to invoke when the event occurs</param>
      /// <returns>This Endpoint instance.</returns>
      T CloseHandler(Action<T> localCloseHandler);

      /// <summary>
      /// Sets an Action that is invoked when the engine that supports this endpoint is
      /// shutdown which indicates a desire to terminate all engine operations. Any endpoint
      /// that has been both locally and remotely closed will not receive this event as it
      /// will no longer be tracked by the parent its parent endpoint.
      /// <para/>
      /// A typical use of this event would be from a locally closed endpoint that is awaiting
      /// response from the remote. If this event fires then there will never be a remote response
      /// to any pending operations and the client or server instance should react accordingly to
      /// clean up any related resources etc.
      /// </summary>
      /// <param name="shutdownHandler">The handler to invoke when the event occurs</param>
      /// <returns>This Endpoint instance.</returns>
      T EngineShutdownHandler(Action<IEngine> shutdownHandler);

   }
}