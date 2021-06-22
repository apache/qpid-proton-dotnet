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
   /// A single AMQP session instance.
   /// </summary>
   public interface ISession : IDisposable
   {
      /// <summary>
      /// Returns the parent client instance that hosts this session.
      /// </summary>
      IClient Client { get; }

      /// <summary>
      /// Returns the parent connection instance that created this session.
      /// </summary>
      IConnection Connection { get; }

      /// <summary>
      /// When a session is created and returned to the client application it may not
      /// be remotely opened yet and if the client needs to wait for completion of the
      /// open before proceeding the open task can be fetched and waited upon.
      /// </summary>
      Task<ISession> OpenTask { get; }

      /// <summary>
      /// Initiates a close of the session and awaits a response from the remote that
      /// indicates completion of the close operation. If the response from the remote
      /// exceeds the configure close timeout the method returns after cleaning up the
      /// session resources.
      /// </summary>
      void Close();

      /// <summary>
      /// Initiates a close of the session and awaits a response from the remote that
      /// indicates completion of the close operation. If the response from the remote
      /// exceeds the configure close timeout the method returns after cleaning up the
      /// session resources.
      /// </summary>
      /// <param name="error">The error condition to convery to the remote</param>
      void Close(IErrorCondition error);

      /// <summary>
      /// Initiates a close of the session and a Task that allows the caller to await
      /// or poll for the response from the remote that indicates completion of the close
      /// operation. If the response from the remote exceeds the configure close timeout
      /// the session will be cleaned up and the Task signalled indicating completion.
      /// </summary>
      Task<ISession> CloseAsync();

      /// <summary>
      /// Initiates a close of the session and a Task that allows the caller to await
      /// or poll for the response from the remote that indicates completion of the close
      /// operation. If the response from the remote exceeds the configure close timeout
      /// the session will be cleaned up and the Task signalled indicating completion.
      /// </summary>
      /// <param name="error">The error condition to convery to the remote</param>
      Task<ISession> CloseAsync(IErrorCondition error);

      /// <summary>
      /// Creates a receiver used to consume messages from the given node address.  The
      /// returned receiver will be configured using default options and will take its timeout
      /// configuration values from those specified in the parent session.
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
      /// configuration values from those specified in the parent session.
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
      /// its timeout configuration values from those specified in the parent session.
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
      /// Creates a sender used to send messages to the given node address.  The returned sender
      /// will be configured using default options and will take its timeout configuration values
      /// from those specified in the parent session.
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
      /// Creates a anonymous sender used to send messages to the "anonymous relay" on the
      /// remote. Each message sent must include a "to" address for the remote to route the
      /// message. The returned sender will be configured using default options and will take
      /// its timeout configuration values from those specified in the parent session.
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
      /// Returns the properties that the remote provided upon successfully opening the session.
      /// If the open has not completed yet this method will block to await the open response which
      /// carries the remote properties.  If the remote provides no properties this method will
      /// return null.
      /// </summary>
      IDictionary<string, object> Properties { get; }

      /// <summary>
      /// Returns the offered capabilities that the remote provided upon successfully opening the
      /// session. If the open has not completed yet this method will block to await the open
      /// response which carries the remote offered capabilities. If the remote provides no offered
      /// capabilities this method will return null.
      /// </summary>
      ICollection<string> OfferedCapabilities { get; }

      /// <summary>
      /// Returns the desired capabilities that the remote provided upon successfully opening the
      /// session. If the open has not completed yet this method will block to await the open
      /// response which carries the remote desired capabilities. If the remote provides no desired
      /// capabilities this method will return null.
      /// </summary>
      ICollection<string> DesiredCapabilities { get; }

      /// <summary>
      /// Opens a new transaction scoped to this {@link Session} if one is not already active.
      /// <para>
      /// A Session that has an active transaction will perform all sends and all delivery
      /// dispositions under that active transaction.  If the user wishes to send with the same
      /// session but outside of a transaction they user must commit the active transaction and
      /// not request that a new one be started. A session can only have one active transaction
      /// at a time and as such any call to begin while there is a currently active transaction
      /// will throw an ClientTransactionNotActiveException to indicate that the operation being
      /// requested is not valid at that time.
      /// </para>
      /// </summary>
      /// <remarks>
      /// This is a blocking method that will return successfully only after a new transaction
      /// has been started.
      /// </remarks>
      /// <returns>This Session instance</returns>
      ISession BeginTransaction();

      /// <summary>
      /// Commit the currently active transaction in this Session.
      /// <para>
      /// Commit the currently active transaction in this Session but does not start a new
      /// transaction automatically.  If there is no current transaction this method will throw
      /// an ClientTransactionNotActiveException to indicate this error.  If the active transaction
      /// has entered an in doubt state or was remotely rolled back this method will throw an error
      /// to indicate that the commit failed and that a new transaction need to be started by the
      /// user. When a transaction rolled back error occurs the user should assume that all work
      /// performed under that transaction has failed and will need to be attempted under a new
      /// transaction.
      /// </para>
      /// </summary>
      /// <remarks>
      /// This is a blocking method that will return successfully only after the transaction
      /// has been committed.
      /// </remarks>
      /// <returns>This Session instance</returns>
      ISession CommitTransaction();

      /// <summary>
      /// Roll back the currently active transaction in this Session.
      /// <para>
      /// Roll back the currently active transaction in this Session but does not automatically
      /// start a new transaction. If there is no current transaction this method will throw an
      /// ClientTransactionNotActiveException to indicate this error.  If the active transaction
      /// has entered an in doubt state or was remotely rolled back this method will throw an
      /// error to indicate that the roll back failed and that a new transaction need to be
      /// started by the user.
      /// </para>
      /// </summary>
      /// <remarks>
      /// This is a blocking method that will return successfully only after the transaction
      /// has been rolled back.
      /// </remarks>
      /// <returns>This Session instance</returns>
      ISession RollbackTransaction();

   }
}