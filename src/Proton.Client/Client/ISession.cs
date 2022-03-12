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
      /// <param name="error">Optional error condition to convery to the remote</param>
      void Close(IErrorCondition error = null);

      /// <summary>
      /// Initiates a close of the session and a Task that allows the caller to await
      /// or poll for the response from the remote that indicates completion of the close
      /// operation. If the response from the remote exceeds the configure close timeout
      /// the session will be cleaned up and the Task signalled indicating completion.
      /// </summary>
      /// <param name="error">Optional error condition to convery to the remote</param>
      Task<ISession> CloseAsync(IErrorCondition error = null);

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
      /// <param name="options">Optional receiver options to use for configuration</param>
      /// <returns>A new receiver instance</returns>
      IReceiver OpenReceiver(string address, ReceiverOptions options = null);

      /// <summary>
      /// Creates a receiver used to consume messages from the given node address. The
      /// returned Task will allow the caller to wait for the creation of the receiver
      /// configured using the provided receiver options.
      ///
      /// The returned receiver may not have been opened on the remote when it is returned. Some
      /// methods of the receiver can block until the remote fully opens the receiver, the user can
      /// wait for the remote to respond to the open request by obtaining the open task from the
      /// receiver and using it to await the completion of the receiver open.
      /// </summary>
      /// <param name="address">The address of the node the receiver attaches to</param>
      /// <param name="options">Optional receiver options to use for configuration</param>
      /// <returns>A new Task<Receiver> instance that can be awaited</returns>
      Task<IReceiver> OpenReceiverAsync(string address, ReceiverOptions options = null);

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
      /// <param name="options">Optional receiver options to use for configuration</param>
      /// <returns>A new receiver instance</returns>
      IReceiver OpenDurableReceiver(string address, string subscriptionName, ReceiverOptions options = null);

      /// <summary>
      /// Creates a receiver used to consume messages from the given node address. The
      /// returned Task will allow the caller to wait for the creation of the receiver
      /// configured using the provided receiver options.
      ///
      /// The returned receiver may not have been opened on the remote when it is returned.  Some
      /// methods of the receiver can block until the remote fully opens the receiver, the user can
      /// wait for the remote to respond to the open request by obtaining the open task from the
      /// receiver and using it to await the completion of the receiver open.
      /// </summary>
      /// <param name="address">The address of the node the receiver attaches to</param>
      /// <param name="subscriptionName">The subscription name to use for the receiver</param>
      /// <param name="options">Optional receiver options to use for configuration</param>
      /// <returns>A new Task<Receiver> instance that can be awaited</returns>
      Task<IReceiver> OpenDurableReceiverAsync(string address, string subscriptionName, ReceiverOptions options = null);

      /// <summary>
      /// Creates a dynamic receiver used to consume messages from the dynamically generated node.
      /// on the remote. The returned receiver will be configured using the provided options.
      ///
      /// The returned receiver may not have been opened on the remote when it is returned.  Some
      /// methods of the receiver can block until the remote fully opens the receiver, the user can
      /// wait for the remote to respond to the open request by obtaining the open task from the
      /// receiver and using it to await the completion of the receiver open.
      /// </summary>
      /// <param name="options">Optional receiver options to use for configuration</param>
      /// <param name="dynamicNodeProperties">Optional properties to assign to the node create</param>
      /// <returns>A new receiver instance</returns>
      IReceiver OpenDynamicReceiver(ReceiverOptions options = null, IDictionary<string, object> dynamicNodeProperties = null);

      /// <summary>
      /// Creates a dynamic receiver used to consume messages from the dynamically generated node.
      /// on the remote. The returned Task will allow the caller to wait for the creation of the
      /// receiver configured using the provided receiver options.
      ///
      /// The returned receiver may not have been opened on the remote when it is returned.  Some
      /// methods of the receiver can block until the remote fully opens the receiver, the user can
      /// wait for the remote to respond to the open request by obtaining the open task from the
      /// receiver and using it to await the completion of the receiver open.
      /// </summary>
      /// <param name="options">Optional receiver options to use for configuration</param>
      /// <param name="dynamicNodeProperties">Optional properties to assign to the node create</param>
      /// <returns>A new Task<Receiver> instance that can be awaited</returns>
      Task<IReceiver> OpenDynamicReceiverAsync(ReceiverOptions options = null, IDictionary<string, object> dynamicNodeProperties = null);

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
      /// <param name="options">Optional sender options to use for configuration</param>
      /// <returns>A new sender instance.</returns>
      ISender OpenSender(string address, SenderOptions options = null);

      /// <summary>
      /// Creates a sender used to send messages to the given node address. The returned Task
      /// will allow the caller to wait for the creation of the receiver configured using the
      /// provided receiver options.
      ///
      /// The returned sender may not have been opened on the remote when it is returned.  Some
      /// methods of the sender can block until the remote fully opens the sender, the user can
      /// wait for the remote to respond to the open request by obtaining the open task from the
      /// sender and using it to await the completion of the sender open.
      /// </summary>
      /// <param name="address">The address of the node the sender attaches to</param>
      /// <param name="options">Optional sender options to use for configuration</param>
      /// <returns>A new Task<ISender> instance.</returns>
      Task<ISender> OpenSenderAsync(string address, SenderOptions options = null);

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
      /// <param name="options">Optional sender options to use for configuration</param>
      /// <returns>A new sender instance.</returns>
      ISender OpenAnonymousSender(SenderOptions options = null);

      /// <summary>
      /// Creates a anonymous sender used to send messages to the "anonymous relay" on the
      /// remote. Each message sent must include a "to" address for the remote to route the
      /// message. The returned Task will allow the caller to wait for the creation of the
      /// receiver configured using the provided receiver options.
      ///
      /// The returned sender may not have been opened on the remote when it is returned. Some
      /// methods of the sender can block until the remote fully opens the sender, the user can
      /// wait for the remote to respond to the open request by obtaining the open task from the
      /// sender and using it to await the completion of the sender open.
      /// </summary>
      /// <param name="options">Optional sender options to use for configuration</param>
      /// <returns>A new Task<ISender> instance.</returns>
      Task<ISender> OpenAnonymousSenderAsync(SenderOptions options = null);

      /// <summary>
      /// Returns the properties that the remote provided upon successfully opening the session.
      /// If the open has not completed yet this method will block to await the open response which
      /// carries the remote properties.  If the remote provides no properties this method will
      /// return null.
      /// </summary>
      IReadOnlyDictionary<string, object> Properties { get; }

      /// <summary>
      /// Returns the offered capabilities that the remote provided upon successfully opening the
      /// session. If the open has not completed yet this method will block to await the open
      /// response which carries the remote offered capabilities. If the remote provides no offered
      /// capabilities this method will return null.
      /// </summary>
      IReadOnlyCollection<string> OfferedCapabilities { get; }

      /// <summary>
      /// Returns the desired capabilities that the remote provided upon successfully opening the
      /// session. If the open has not completed yet this method will block to await the open
      /// response which carries the remote desired capabilities. If the remote provides no desired
      /// capabilities this method will return null.
      /// </summary>
      IReadOnlyCollection<string> DesiredCapabilities { get; }

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
      /// Opens a new transaction scoped to this {@link Session} if one is not already active.
      /// The transaction will not be considered active until the returned Task is completed.
      /// <para>
      /// A Session that has an active transaction will perform all sends and all delivery
      /// dispositions under that active transaction. If the user wishes to send with the same
      /// session but outside of a transaction they user must commit the active transaction and
      /// not request that a new one be started. A session can only have one active transaction
      /// at a time and as such any call to begin while there is a currently active transaction
      /// will throw an ClientTransactionNotActiveException to indicate that the operation being
      /// requested is not valid at that time.
      /// </para>
      /// </summary>
      /// <returns>A Task whose result is the Session instance now under a transaction</returns>
      Task<ISession> BeginTransactionAsync();

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
      /// Asynchronously commit the currently active transaction in this Session, the transaction
      /// cannot be considered retired until the Task has been compeleted.
      /// <para>
      /// Commit the currently active transaction in this Session but does not start a new
      /// transaction automatically. If there is no current transaction this method fails the returned
      /// Task with an ClientTransactionNotActiveException to indicate this error. If the active
      /// transaction has entered an in doubt state or was remotely rolled back this method fail the
      /// Task with an error to indicate that the commit failed and that a new transaction need to be
      /// started by the user. When a transaction rolled back error occurs the user should assume that
      /// all work performed under that transaction has failed and will need to be attempted under a
      /// new transaction.
      /// </para>
      /// </summary>
      /// <returns>A Task whose result is the Session no longer under a transaction</returns>
      Task<ISession> CommitTransactionAsync();

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

      /// <summary>
      /// Asynchronously roll back the currently active transaction in this Session.
      /// <para>
      /// Roll back the currently active transaction in this Session but does not automatically
      /// start a new transaction. If there is no current transaction this method fails the returned
      /// Task an ClientTransactionNotActiveException to indicate this error. If the active transaction
      /// has entered an in doubt state or was remotely rolled back this method will fail the returned
      /// Task with an error to indicate that the roll back failed and that a new transaction need to
      /// be started by the user.
      /// </para>
      /// </summary>
      /// <returns>A Task whose result is the Session no longer under a transaction</returns>
      Task<ISession> RollbackTransactionAsync();

   }
}