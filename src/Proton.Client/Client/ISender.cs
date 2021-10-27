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
   /// A single AMQP sender instance.
   /// </summary>
   public interface ISender : IDisposable
   {
      /// <summary>
      /// Returns the parent client instance that hosts this sender.
      /// </summary>
      IClient Client { get; }

      /// <summary>
      /// Returns the parent connection instance that hosts this sender.
      /// </summary>
      IConnection Connection { get; }

      /// <summary>
      /// Returns the parent session instance that created this sender.
      /// </summary>
      ISession Session { get; }

      /// <summary>
      /// When a sender is created and returned to the client application it may not
      /// be remotely opened yet and if the client needs to wait for completion of the
      /// open before proceeding the open task can be fetched and waited upon.
      /// </summary>
      Task<ISender> OpenTask { get; }

      /// <summary>
      /// Initiates a close of the sender and awaits a response from the remote that
      /// indicates completion of the close operation. If the response from the remote
      /// exceeds the configure close timeout the method returns after cleaning up the
      /// sender resources.
      /// </summary>
      /// <param name="error">Optional error condition to convery to the remote</param>
      void Close(IErrorCondition error = null);

      /// <summary>
      /// Initiates a detach of the sender and awaits a response from the remote that
      /// indicates completion of the detach operation. If the response from the remote
      /// exceeds the configure close timeout the method returns after cleaning up the
      /// sender resources.
      /// </summary>
      /// <param name="error">Optional error condition to convery to the remote</param>
      void Detach(IErrorCondition error = null);

      /// <summary>
      /// Initiates a close of the sender and a Task that allows the caller to await
      /// or poll for the response from the remote that indicates completion of the close
      /// operation. If the response from the remote exceeds the configure close timeout
      /// the sender will be cleaned up and the Task signalled indicating completion.
      /// </summary>
      /// <param name="error">Optional error condition to convery to the remote</param>
      Task<ISender> CloseAsync(IErrorCondition error = null);

      /// <summary>
      /// Initiates a detach of the sender and a Task that allows the caller to await
      /// or poll for the response from the remote that indicates completion of the detach
      /// operation. If the response from the remote exceeds the configure close timeout
      /// the sender will be cleaned up and the Task signalled indicating completion.
      /// </summary>
      /// <param name="error">Optional error condition to convery to the remote</param>
      Task<ISender> DetachAsync(IErrorCondition error = null);

      /// <summary>
      /// Returns the address that the sender instance will send message objects to. The value
      /// returned from this method is controlled by the configuration that was used to create
      /// the sender.
      /// <list type="bullet">
      /// <item>
      /// <description>
      /// If the Sender is configured as an anonymous sender then this method returns null.
      /// </description>
      /// </item>
      /// <item>
      /// <description>
      /// If the Sender was created with the dynamic sender methods then the method will return
      /// the dynamically created address once the remote has attached its end of the sender link.
      /// Due to the need to await the remote peer to populate the dynamic address this method will
      /// block until the open of the sender link has completed.
      /// </description>
      /// </item>
      /// <item>
      /// <description>
      /// If neither of the above is true then the address returned is the address passed to the
      /// original address value passed to one of the open sender methods.
      /// </description>
      /// </item>
      /// </list>
      /// </summary>
      string Address { get; }

      /// <summary>
      /// Returns an immutable view of the remote Source object assigned to this sender link.
      /// If the attach has not completed yet this method will block to await the attach response
      /// which carries the remote source.
      /// </summary>
      ISource Source { get; }

      /// <summary>
      /// Returns an immutable view of the remote Target object assigned to this sender link.
      /// If the attach has not completed yet this method will block to await the attach response
      /// which carries the remote target.
      /// </summary>
      ITarget Target { get; }

      /// <summary>
      /// Returns the properties that the remote provided upon successfully opening the sender.
      /// If the open has not completed yet this method will block to await the open response which
      /// carries the remote properties.  If the remote provides no properties this method will
      /// return null.
      /// </summary>
      IReadOnlyDictionary<string, object> Properties { get; }

      /// <summary>
      /// Returns the offered capabilities that the remote provided upon successfully opening the
      /// sender. If the open has not completed yet this method will block to await the open
      /// response which carries the remote offered capabilities. If the remote provides no offered
      /// capabilities this method will return null.
      /// </summary>
      IReadOnlyCollection<string> OfferedCapabilities { get; }

      /// <summary>
      /// Returns the desired capabilities that the remote provided upon successfully opening the
      /// sender. If the open has not completed yet this method will block to await the open
      /// response which carries the remote desired capabilities. If the remote provides no desired
      /// capabilities this method will return null.
      /// </summary>
      IReadOnlyCollection<string> DesiredCapabilities { get; }

      /// <summary>
      /// Send the given message immediately if there is credit available or blocks if the link
      /// has not yet been granted credit. If a send timeout has been configured then this method
      /// will throw a timed out error after that if the message cannot be sent.
      /// </summary>
      /// <typeparam name="T">The type that describes the message body</typeparam>
      /// <param name="message">The message object that will be sent</param>
      /// <param name="deliveryAnnotations">Optional delivery annotation to include with the message</param>
      /// <returns>A Tracker for the sent message</returns>
      ITracker Send<T>(IMessage<T> message, IDictionary<string, object> deliveryAnnotations = null);

      /// <summary>
      /// Send the given message if credit is available or returns null if no credit has been
      /// granted to the link at the time of the send attempt.
      /// </summary>
      /// <typeparam name="T">The type that describes the message body</typeparam>
      /// <param name="message">The message object that will be sent</param>
      /// <param name="deliveryAnnotations">Optional delivery annotation to include with the message</param>
      /// <returns>A Tracker for the sent message or null if no credit to send is available</returns>
      ITracker TrySend<T>(IMessage<T> message, IDictionary<string, object> deliveryAnnotations = null);

   }
}