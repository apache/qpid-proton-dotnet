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
   /// A single AMQP Connection instance.
   /// </summary>
   public interface IConnection : IDisposable
   {
      /// <summary>
      /// Returns the parent client instance that created this connection.
      /// </summary>
      IClient Client { get; }

      /// <summary>
      /// When a connection is created and returned to the client application it may not
      /// be remotely opened yet and if the client needs to wait for completion of the
      /// open before proceeding the open task can be fetched and waited upon.
      /// </summary>
      Task<IConnection> OpenTask { get; }

      /// <summary>
      /// Initiates a close of the connection and awaits a response from the remote that
      /// indicates completion of the close operation. If the response from the remote
      /// exceeds the configure close timeout the method returns after cleaning up the
      /// connection resources.
      /// </summary>
      void Close();

      /// <summary>
      /// Initiates a close of the connection and awaits a response from the remote that
      /// indicates completion of the close operation. If the response from the remote
      /// exceeds the configure close timeout the method returns after cleaning up the
      /// connection resources.
      /// </summary>
      /// <param name="error">The error condition to convery to the remote</param>
      void Close(IErrorCondition error);

      /// <summary>
      /// Initiates a close of the connection and a Task that allows the caller to await
      /// or poll for the response from the remote that indicates completion of the close
      /// operation. If the response from the remote exceeds the configure close timeout
      /// the connection will be cleaned up and the Task signalled indicating completion.
      /// </summary>
      Task<IConnection> CloseAsync();

      /// <summary>
      /// Initiates a close of the connection and a Task that allows the caller to await
      /// or poll for the response from the remote that indicates completion of the close
      /// operation. If the response from the remote exceeds the configure close timeout
      /// the connection will be cleaned up and the Task signalled indicating completion.
      /// </summary>
      /// <param name="error">The error condition to convery to the remote</param>
      Task<IConnection> CloseAsync(IErrorCondition error);

      /// <summary>
      /// Creates a receiver used to consume messages from the given node address.  The
      /// returned receiver will be configured using default options and will take its timeout
      /// configuration values from those specified in the parent connection.
      ///
      /// The returned receiver may not have been opened on the remote when it is returned.  Some
      /// methods of the receiver can block until the remote fully opens the receiver, the user can
      /// wait for the remote to respond to the open request by obtaining the open task from the
      /// receiver and using it to await the completion of the receiver open.
      /// </summary>
      /// <param name="address">The address of the node the receiver attaches to</param>
      /// <returns>A new receiver instance</returns>
      IReceiver OpenReceiver(string address);

      /// <summary>
      /// Creates a receiver used to consume messages from the given node address.  The
      /// returned receiver will be configured using the provided receiver options.
      ///
      /// The returned receiver may not have been opened on the remote when it is returned.  Some
      /// methods of the receiver can block until the remote fully opens the receiver, the user can
      /// wait for the remote to respond to the open request by obtaining the open task from the
      /// receiver and using it to await the completion of the receiver open.
      /// </summary>
      /// <param name="address">The address of the node the receiver attaches to</param>
      /// <param name="options">The receiver options to use for configuration</param>
      /// <returns>A new receiver instance</returns>
      IReceiver OpenReceiver(string address, ReceiverOptions options);

      /// <summary>
      /// Creates a receiver used to consume messages from the given node address.  The
      /// returned receiver will be configured using default options and will take its timeout
      /// configuration values from those specified in the parent connection.
      ///
      /// The returned receiver may not have been opened on the remote when it is returned.  Some
      /// methods of the receiver can block until the remote fully opens the receiver, the user can
      /// wait for the remote to respond to the open request by obtaining the open task from the
      /// receiver and using it to await the completion of the receiver open.
      /// </summary>
      /// <param name="address">The address of the node the receiver attaches to</param>
      /// <param name="subscriptionName">The subscription name to use for the receiver</param>
      /// <returns>A new receiver instance</returns>
      IReceiver OpenDurableReceiver(string address, string subscriptionName);

      /// <summary>
      /// Creates a receiver used to consume messages from the given node address.  The
      /// returned receiver will be configured using the provided receiver options.
      ///
      /// The returned receiver may not have been opened on the remote when it is returned.  Some
      /// methods of the receiver can block until the remote fully opens the receiver, the user can
      /// wait for the remote to respond to the open request by obtaining the open task from the
      /// receiver and using it to await the completion of the receiver open.
      /// </summary>
      /// <param name="address">The address of the node the receiver attaches to</param>
      /// <param name="subscriptionName">The subscription name to use for the receiver</param>
      /// <param name="options">The receiver options to use for configuration</param>
      /// <returns>A new receiver instance</returns>
      IReceiver OpenDurableReceiver(string address, string subscriptionName, ReceiverOptions options);

      /// <summary>
      /// Creates a dynamic receiver used to consume messages from the dynamically generated node.
      /// on the remote. The returned receiver will be configured using default options and will take
      /// its timeout configuration values from those specified in the parent connection.
      ///
      /// The returned receiver may not have been opened on the remote when it is returned.  Some
      /// methods of the receiver can block until the remote fully opens the receiver, the user can
      /// wait for the remote to respond to the open request by obtaining the open task from the
      /// receiver and using it to await the completion of the receiver open.
      /// </summary>
      /// <returns>A new receiver instance</returns>
      IReceiver OpenDynamicReceiver();

      /// <summary>
      /// Creates a dynamic receiver used to consume messages from the dynamically generated node.
      /// on the remote. The returned receiver will be configured using the provided options.
      ///
      /// The returned receiver may not have been opened on the remote when it is returned.  Some
      /// methods of the receiver can block until the remote fully opens the receiver, the user can
      /// wait for the remote to respond to the open request by obtaining the open task from the
      /// receiver and using it to await the completion of the receiver open.
      /// </summary>
      /// <param name="options">The receiver options to use for configuration</param>
      /// <returns>A new receiver instance</returns>
      IReceiver OpenDynamicReceiver(ReceiverOptions options);

      /// <summary>
      /// Creates a dynamic receiver used to consume messages from the dynamically generated node.
      /// on the remote. The returned receiver will be configured using the provided options.
      ///
      /// The returned receiver may not have been opened on the remote when it is returned.  Some
      /// methods of the receiver can block until the remote fully opens the receiver, the user can
      /// wait for the remote to respond to the open request by obtaining the open task from the
      /// receiver and using it to await the completion of the receiver open.
      /// </summary>
      /// <param name="dynamicNodeProperties">The properties to assign to the node create</param>
      /// <param name="options">The receiver options to use for configuration</param>
      /// <returns>A new receiver instance</returns>
      IReceiver OpenDynamicReceiver(IDictionary<string, object> dynamicNodeProperties, ReceiverOptions options);

      /// <summary>
      /// Creates a stream receiver used to consume large messages from the given node address.
      /// The returned receiver will be configured using default options and will take its timeout
      /// configuration values from those specified in the parent connection.
      ///
      /// The returned receiver may not have been opened on the remote when it is returned.  Some
      /// methods of the receiver can block until the remote fully opens the receiver, the user can
      /// wait for the remote to respond to the open request by obtaining the open task from the
      /// receiver and using it to await the completion of the receiver open.
      /// </summary>
      /// <param name="address">The address of the node the receiver attaches to</param>
      /// <returns>A new receiver instance</returns>
      IStreamReceiver OpenStreamReceiver(string address);

      /// <summary>
      /// Creates a stream receiver used to consume large messages from the given node address.
      /// The returned receiver will be configured using the provided stream receiver options.
      ///
      /// The returned receiver may not have been opened on the remote when it is returned.  Some
      /// methods of the receiver can block until the remote fully opens the receiver, the user can
      /// wait for the remote to respond to the open request by obtaining the open task from the
      /// receiver and using it to await the completion of the receiver open.
      /// </summary>
      /// <param name="address">The address of the node the receiver attaches to</param>
      /// <param name="options">The receiver options to use for configuration</param>
      /// <returns>A new receiver instance</returns>
      IStreamReceiver OpenStreamReceiver(string address, StreamReceiverOptions options);

      /// <summary>
      /// Returns the default anonymous sender used by this connection for all send calls from the
      /// connection. If the sender has not been created yet this call will initiate its creation and
      /// open with the remote peer.
      /// </summary>
      /// <returns>The connection wide default anonymous sender instance.</returns>
      ISender DefaultSender();

      /// <summary>
      /// Creates a sender used to send messages to the given node address.  The returned sender
      /// will be configured using default options and will take its timeout configuration values
      /// from those specified in the parent connection.
      ///
      /// The returned sender may not have been opened on the remote when it is returned.  Some
      /// methods of the sender can block until the remote fully opens the sender, the user can
      /// wait for the remote to respond to the open request by obtaining the open task from the
      /// sender and using it to await the completion of the sender open.
      /// </summary>
      /// <param name="address">The address of the node the sender attaches to</param>
      /// <returns>A new sender instance.</returns>
      ISender OpenSender(string address);

      /// <summary>
      /// Creates a sender used to send messages to the given node address.  The returned sender
      /// will be configured using configuration options provided.
      ///
      /// The returned sender may not have been opened on the remote when it is returned.  Some
      /// methods of the sender can block until the remote fully opens the sender, the user can
      /// wait for the remote to respond to the open request by obtaining the open task from the
      /// sender and using it to await the completion of the sender open.
      /// </summary>
      /// <param name="address">The address of the node the sender attaches to</param>
      /// <param name="options">The sender options to use for configuration</param>
      /// <returns>A new sender instance.</returns>
      ISender OpenSender(string address, SenderOptions options);

      /// <summary>
      /// Creates a stream sender used to send large messages to the given node address. The
      /// returned sender will be configured using default options and will take its timeout
      /// configuration values from those specified in the parent connection.
      ///
      /// The returned sender may not have been opened on the remote when it is returned. Some
      /// methods of the sender can block until the remote fully opens the sender, the user can
      /// wait for the remote to respond to the open request by obtaining the open task from the
      /// sender and using it to await the completion of the sender open.
      /// </summary>
      /// <param name="address">The address of the node the sender attaches to</param>
      /// <returns>A new sender instance.</returns>
      IStreamSender OpenStreamSender(string address);

      /// <summary>
      /// Creates a stream sender used to send large messages to the given node address. The
      /// returned sender will be configured using configuration options provided.
      ///
      /// The returned sender may not have been opened on the remote when it is returned. Some
      /// methods of the sender can block until the remote fully opens the sender, the user can
      /// wait for the remote to respond to the open request by obtaining the open task from the
      /// sender and using it to await the completion of the sender open.
      /// </summary>
      /// <param name="address">The address of the node the sender attaches to</param>
      /// <param name="options">The stream sender options to use for configuration</param>
      /// <returns>A new sender instance.</returns>
      IStreamSender OpenStreamSender(string address, StreamSenderOptions options);

      /// <summary>
      /// Creates a anonymous sender used to send messages to the "anonymous relay" on the
      /// remote. Each message sent must include a "to" address for the remote to route the
      /// message. The returned sender will be configured using default options and will take
      /// its timeout configuration values from those specified in the parent connection.
      ///
      /// The returned sender may not have been opened on the remote when it is returned. Some
      /// methods of the sender can block until the remote fully opens the sender, the user can
      /// wait for the remote to respond to the open request by obtaining the open task from the
      /// sender and using it to await the completion of the sender open.
      /// </summary>
      /// <returns>A new sender instance.</returns>
      ISender OpenAnonymousSender();

      /// <summary>
      /// Creates a anonymous sender used to send messages to the "anonymous relay" on the
      /// remote. Each message sent must include a "to" address for the remote to route the
      /// message. The returned sender will be configured using the provided sender options.
      ///
      /// The returned sender may not have been opened on the remote when it is returned. Some
      /// methods of the sender can block until the remote fully opens the sender, the user can
      /// wait for the remote to respond to the open request by obtaining the open task from the
      /// sender and using it to await the completion of the sender open.
      /// </summary>
      /// <param name="options">The sender options to use for configuration</param>
      /// <returns>A new sender instance.</returns>
      ISender OpenAnonymousSender(SenderOptions options);

      /// <summary>
      /// Returns the default session instance that is used by this Connection to create the
      /// default anonymous connection sender as well as creating those resources created from
      /// the connection} such as sender and receiver instances not married to a specific session.
      /// </summary>
      /// <returns>The default session that is owned by this connection</returns>
      ISession DefaultSession();

      /// <summary>
      /// Creates a new session instance for use by the client application. The returned
      /// session will be configured using default options and will take its timeout
      /// configuration values from those specified in the parent connection.
      ///
      /// The returned session may not have been opened on the remote when it is returned. Some
      /// methods of the session can block until the remote fully opens the session, the user can
      /// wait for the remote to respond to the open request by obtaining the open task from the
      /// session and using it to await the completion of the session open.
      /// </summary>
      /// <returns>A new session instance.</returns>
      ISession OpenSession();

      /// <summary>
      /// Creates a new session instance for use by the client application. The returned
      /// session will be configured using the provided session options.
      ///
      /// The returned session may not have been opened on the remote when it is returned. Some
      /// methods of the session can block until the remote fully opens the session, the user can
      /// wait for the remote to respond to the open request by obtaining the open task from the
      /// session and using it to await the completion of the session open.
      /// </summary>
      /// <param name="options">The session options to use for configuration</param>
      /// <returns>A new session instance.</returns>
      ISession OpenSession(SessionOptions options);

      /// <summary>
      /// Sends the given message using the connection scoped default sender instance. The
      /// connection send uses the remote "anonymous relay" to route messages which requires
      /// that the sent message have a valid "To" address set and that the remote supports
      /// the anonymous relay.
      /// </summary>
      /// <typeparam name="T">The type of body the message carries</typeparam>
      /// <param name="message">The message to be sent</param>
      /// <returns>A tracker instance that can be used to track the send outcome</returns>
      ITracker Send<T>(IMessage<T> message);

      /// <summary>
      /// Returns the properties that the remote provided upon successfully opening the connection.
      /// If the open has not completed yet this method will block to await the open response which
      /// carries the remote properties.  If the remote provides no properties this method will
      /// return null.
      /// </summary>
      IDictionary<string, object> Properties { get; }

      /// <summary>
      /// Returns the offered capabilities that the remote provided upon successfully opening the
      /// connection. If the open has not completed yet this method will block to await the open
      /// response which carries the remote offered capabilities. If the remote provides no offered
      /// capabilities this method will return null.
      /// </summary>
      ICollection<string> OfferedCapabilities { get; }

      /// <summary>
      /// Returns the desired capabilities that the remote provided upon successfully opening the
      /// connection. If the open has not completed yet this method will block to await the open
      /// response which carries the remote desired capabilities. If the remote provides no desired
      /// capabilities this method will return null.
      /// </summary>
      ICollection<string> DesiredCapabilities { get; }

   }
}