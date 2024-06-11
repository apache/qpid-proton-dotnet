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

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apache.Qpid.Proton.Client
{
   /// <summary>
   /// A single AMQP stream sender instance which can be used to transmit large message
   /// payloads to the remote without needing to load the full message contents into
   /// memory.  The streaming sender will also provide flow control that attempts to
   /// provide additional safety values for out of memory situations.
   /// </summary>
   public interface IStreamSender : ILink<IStreamSender>
   {
      /// <summary>
      /// If no streaming send has been initiated and not yet completed then this method will
      /// send the given message immediately if there is credit available or blocks if the link
      /// has not yet been granted credit. If a send timeout has been configured then this method
      /// will throw a timed out error after that if the message cannot be sent.
      /// </summary>
      /// <typeparam name="T">The type that describes the message body</typeparam>
      /// <param name="message">The message object that will be sent</param>
      /// <param name="deliveryAnnotations">Optional delivery annotation to include with the message</param>
      /// <returns>A Tracker for the sent message</returns>
      IStreamTracker Send<T>(IMessage<T> message, IDictionary<string, object> deliveryAnnotations = null);

      /// <summary>
      /// If no streaming send has been initiated and not yet completed then this method will
      /// send the given message immediately if there is credit available or waits if the link
      /// has not yet been granted credit. If a send timeout has been configured then this method
      /// will fail the returned Task with a timed out error after that if the message cannot be sent.
      /// The returned Task will be completed once the message has been sent.
      /// </summary>
      /// <typeparam name="T">The type that describes the message body</typeparam>
      /// <param name="message">The message object that will be sent</param>
      /// <param name="deliveryAnnotations">Optional delivery annotation to include with the message</param>
      /// <returns>A Task that is completed with a Tracker once the send completes</returns>
      Task<IStreamTracker> SendAsync<T>(IMessage<T> message, IDictionary<string, object> deliveryAnnotations = null);

      /// <summary>
      /// If no streaming send has been initiated and not yet completed then this method will
      /// send the given message if credit is available or returns null if no credit has been
      /// granted to the link at the time of the send attempt.
      /// </summary>
      /// <typeparam name="T">The type that describes the message body</typeparam>
      /// <param name="message">The message object that will be sent</param>
      /// <param name="deliveryAnnotations">Optional delivery annotation to include with the message</param>
      /// <returns>A Tracker for the sent message or null if no credit to send is available</returns>
      IStreamTracker TrySend<T>(IMessage<T> message, IDictionary<string, object> deliveryAnnotations = null);

      /// <summary>
      /// If no streaming send has been initiated and not yet completed then this method will
      /// send the given message if credit is available or completes the returned Task with null if no credit
      /// has been granted to the link at the time of the send attempt.
      /// </summary>
      /// <typeparam name="T">The type that describes the message body</typeparam>
      /// <param name="message">The message object that will be sent</param>
      /// <param name="deliveryAnnotations">Optional delivery annotation to include with the message</param>
      /// <returns>A Task that provides a tracker if the send completes or null if no credit</returns>
      Task<IStreamTracker> TrySendAsync<T>(IMessage<T> message, IDictionary<string, object> deliveryAnnotations = null);

      /// <summary>
      /// Creates and returns a new stream capable message that can be used by the caller to perform
      /// streaming sends of large message payload data.  Only one streamed message can be active
      /// at a time so any successive calls to begin a new streaming message will throw an error
      /// to indicate that the previous instance has not yet been completed.
      /// </summary>
      /// <param name="deliveryAnnotations">The optional delivery annotations to transmit with the message</param>
      /// <returns>A new IStreamSenderMessage if no send is currently in progress</returns>
      IStreamSenderMessage BeginMessage(IDictionary<string, object> deliveryAnnotations = null);

      /// <summary>
      /// Creates and returns a new stream capable message that can be used by the caller to perform
      /// streaming sends of large message payload data.  Only one streamed message can be active
      /// at a time so any successive calls to begin a new streaming message will throw an error
      /// to indicate that the previous instance has not yet been completed.
      /// </summary>
      /// <param name="deliveryAnnotations">The optional delivery annotations to transmit with the message</param>
      /// <returns>A Task that returns new IStreamSenderMessage if no send is currently in progress</returns>
      Task<IStreamSenderMessage> BeginMessageAsync(IDictionary<string, object> deliveryAnnotations = null);

   }
}